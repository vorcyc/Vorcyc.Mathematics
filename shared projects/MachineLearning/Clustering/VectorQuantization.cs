namespace Vorcyc.Mathematics.MachineLearning.Clustering;

using System.Text.Json;
using System.Numerics;

/// <summary>
/// Represents the vector quantization algorithm.
/// </summary>
/// <typeparam name="T">The element type of the vectors, which must implement the <see cref="IFloatingPointIeee754{T}"/> and <see cref="IMinMaxValue{T}"/> interfaces.</typeparam>
/// <remarks>
/// Vector Quantization (VQ) is a classic signal-processing technique widely used in data compression, image processing, and pattern recognition.
/// Its basic idea is to map vectors in a high-dimensional space to a finite set of vectors (the codebook), thereby achieving data compression and feature extraction.
/// 
/// During training, the algorithm iteratively optimizes the vectors in the codebook so that they better represent the input data set. Each iteration assigns the input vectors to the nearest code vector and updates the code vectors to the centroids of their corresponding clusters.
/// 
/// This class uses <see cref="T"/> arrays to represent vectors and performs efficient operations via <see cref="Span{T}"/> or <see cref="ReadOnlySpan{T}"/> when necessary.
/// </remarks>
public class VectorQuantization<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    private readonly List<T[]> _codebook; // The codebook, storing the list of vector arrays
    private readonly int _dimensions;     // The dimension of the vectors

    /// <summary>
    /// Gets the codebook.
    /// </summary>
    public IReadOnlyList<T[]> Codebook => _codebook;

    /// <summary>
    /// Gets the machine learning task type.
    /// </summary>
    public MachineLearningTask Task => MachineLearningTask.Clustering;

    /// <summary>
    /// Execution policy honored by the training loops. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorQuantization{T}"/> class.
    /// </summary>
    /// <param name="codebookSize">The size of the codebook, which must be a positive integer.</param>
    /// <param name="dimensions">The dimension of the vectors, which must be a positive integer.</param>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="codebookSize"/> or <paramref name="dimensions"/> is less than or equal to 0.</exception>
    public VectorQuantization(int codebookSize, int dimensions, ComputingContext? context = null)
    {
        if (codebookSize <= 0)
            throw new ArgumentException("The codebook size must be a positive integer.", nameof(codebookSize));
        if (dimensions <= 0)
            throw new ArgumentException("The vector dimension must be a positive integer.", nameof(dimensions));

        _dimensions = dimensions;
        _codebook = InitializeCodebook(codebookSize, dimensions);
        Context = context;
    }

    /// <summary>
    /// Initializes the codebook.
    /// </summary>
    /// <param name="codebookSize">The size of the codebook.</param>
    /// <param name="dimensions">The dimension of the vectors.</param>
    /// <returns>The initialized codebook.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<T[]> InitializeCodebook(int codebookSize, int dimensions)
    {
        var codebook = new List<T[]>(codebookSize);
        for (int i = 0; i < codebookSize; i++)
        {
            var elements = new T[dimensions];
            for (int j = 0; j < dimensions; j++)
            {
                elements[j] = T.CreateChecked(Random.Shared.NextDouble());
            }
            codebook.Add(elements);
        }
        return codebook;
    }

    /// <summary>
    /// Trains the vector quantization model.
    /// </summary>
    /// <param name="data">The training data, containing multiple vectors.</param>
    /// <param name="maxIterations">The maximum number of iterations; the default is 100.</param>
    /// <returns>The list of errors for each iteration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="data"/> is empty or the vector dimension does not match.</exception>
    public List<T> Train(IEnumerable<T[]> data, int maxIterations = 100)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "The training data cannot be null.");
        var dataList = data.ToList();
        if (dataList.Count == 0)
            throw new ArgumentException("The training data cannot be empty.", nameof(data));
        if (dataList[0].Length != _dimensions)
            throw new ArgumentException("The dimension of the input vectors does not match the codebook dimension.", nameof(data));

        var errors = new List<T>(maxIterations);
        var clusters = new List<List<T[]>>(_codebook.Count);

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            // Reset clusters
            clusters.Clear();
            for (int i = 0; i < _codebook.Count; i++)
                clusters.Add(new List<T[]>());

            // Assign vectors to the nearest code vector
            int[] nearestIndices = new int[dataList.Count];
            ComputingContextExecution.ForEach(
                Context,
                0,
                dataList.Count,
                idx =>
                {
                    nearestIndices[idx] = FindNearestCodeVector(dataList[idx]);
                },
                workPerItem: (long)_codebook.Count * _dimensions);

            for (int idx = 0; idx < dataList.Count; idx++)
            {
                clusters[nearestIndices[idx]].Add(dataList[idx]);
            }

            // Update the code vectors
            bool anyChange = false;
            for (int i = 0; i < _codebook.Count; i++)
            {
                if (clusters[i].Count > 0)
                {
                    var newCentroid = CalculateCentroid(clusters[i]);
                    if (!newCentroid.SequenceEqual(_codebook[i]))
                    {
                        _codebook[i] = newCentroid;
                        anyChange = true;
                    }
                }
            }

            // Compute the error and check for convergence
            T error = CalculateError(dataList);
            errors.Add(error);

            if (!anyChange && iteration > 0)
                break;
        }

        return errors;
    }

    /// <summary>
    /// Finds the code vector nearest to the given vector.
    /// </summary>
    /// <param name="vector">The given vector.</param>
    /// <returns>The index of the nearest code vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindNearestCodeVector(ReadOnlySpan<T> vector)
    {
        int nearestIndex = 0;
        T minDistance = T.MaxValue;

        for (int i = 0; i < _codebook.Count; i++)
        {
            T distance = CalculateEuclideanDistance(vector, _codebook[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    /// <summary>
    /// Computes the Euclidean distance between two vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The Euclidean distance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T CalculateEuclideanDistance(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
    {
        T distance = T.Zero;
        for (int i = 0; i < a.Length; i++)
        {
            T diff = a[i] - b[i];
            distance += diff * diff;
        }
        return T.Sqrt(distance);
    }

    /// <summary>
    /// Computes the centroid of the given list of vectors.
    /// </summary>
    /// <param name="vectors">The list of vectors.</param>
    /// <returns>The element array of the centroid vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T[] CalculateCentroid(List<T[]> vectors)
    {
        var centroidElements = new T[_dimensions];
        int count = vectors.Count;
        T countInverse = T.One / T.CreateChecked(count);

        foreach (var vector in vectors)
        {
            for (int i = 0; i < _dimensions; i++)
            {
                centroidElements[i] += vector[i];
            }
        }

        for (int i = 0; i < _dimensions; i++)
        {
            centroidElements[i] *= countInverse;
        }

        return centroidElements;
    }

    /// <summary>
    /// Computes the error between the current codebook and the data set.
    /// </summary>
    /// <param name="data">The data set.</param>
    /// <returns>The error value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T CalculateError(IReadOnlyList<T[]> data)
    {
        T totalError = T.Zero;
        foreach (var vector in data)
        {
            int nearestIndex = FindNearestCodeVector(vector);
            totalError += CalculateEuclideanDistance(vector, _codebook[nearestIndex]);
        }
        return totalError;
    }

    /// <summary>
    /// Finds the nearest code vector for the input vector.
    /// </summary>
    /// <param name="vector">The input vector.</param>
    /// <returns>The nearest code vector.</returns>
    /// <exception cref="ArgumentException">Thrown when the dimension of <paramref name="vector"/> does not match the codebook.</exception>
    public T[] Predict(T[] vector)
    {
        if (vector == null || vector.Length != _dimensions)
            throw new ArgumentException("The dimension of the input vector does not match the codebook dimension.", nameof(vector));
        int nearestIndex = FindNearestCodeVector(vector);
        return _codebook[nearestIndex];
    }

    /// <summary>
    /// Saves the codebook to a file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    public void SaveCodebook(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath), "The file path cannot be empty.");

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(_codebook, options);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads the codebook from a file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public void LoadCodebook(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath), "The file path cannot be empty.");
        if (!File.Exists(filePath))
            throw new FileNotFoundException("The specified file does not exist.", filePath);

        string json = File.ReadAllText(filePath);
        var loadedCodebook = JsonSerializer.Deserialize<List<T[]>>(json);
        if (loadedCodebook == null || loadedCodebook.Count == 0)
            throw new InvalidOperationException("The loaded codebook is empty or invalid.");
        if (loadedCodebook[0].Length != _dimensions)
            throw new InvalidOperationException("The dimension of the loaded codebook does not match the current instance.");

        _codebook.Clear();
        _codebook.AddRange(loadedCodebook);
    }
}