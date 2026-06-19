
/* usage : */
//todo Using random features:
//Random random = new Random();
//int numData = 10000; // Number of data points
//int numFeatures = 512; // Feature dimension of the data
//var data = new float[numData][]; // Data matrix
//for (int i = 0; i < numData; i++)
//{
//    data[i] = new float[numFeatures];
//    for (int j = 0; j < numFeatures; j++)
//    {
//        data[i][j] = random.NextSingle() * 10; // Generate a random number between 0 and 10
//    }
//}

//todo Using features:
//static float[][] LoadData()
//{
//    var file = @"C:\Users\cyclo\Desktop\all.txt";

//    (string fn, float[] feature) ParseLine(string line)
//    {
//        var t = line.Split('\t');
//        var label = t[0];
//        var f = ToSingles(t[1].Split(','));
//        return (label, f);
//    }

//    static float[] ToSingles(string[] strings)
//    {
//        var result = new float[strings.Length];
//        for (int i = 0; i < strings.Length; i++)
//        {
//            result[i] = float.Parse(strings[i]);
//        }
//        return result;
//    }

//    var lines = System.IO.File.ReadAllLines(file);
//    var result = new float[lines.Length][];

//    for (int i = 0; i < lines.Length; i++)
//    {
//        result[i] = ParseLine(lines[i]).feature;
//    }

//    return result;
//}


//var data = LoadData();


//! CODE :
//// Set the clustering parameters
//using static System.Runtime.InteropServices.JavaScript.JSType;
//using Vorcyc.Offlet.MachineLearning.Clustering;

//int numClusters = 2; // Number of clusters
//int maxIterations = 100; // Maximum number of iterations
//float tolerance = 0.01f; // Convergence tolerance

//// Invoke the K-means clustering algorithm
//int[] clustering = KMeansClusterer.KMeans(data, numClusters, maxIterations, tolerance);

//// Display the clustering result
//Console.WriteLine("The clustering result of the data is as follows:");
//for (int i = 0; i < 5; i++)
//{
//    Console.WriteLine($"Data {i}: ({data[i][0]:F2}, {data[i][1]:F2}) belongs to cluster {clustering[i]}");
//}


using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// Provides the K-means clustering algorithm.
/// </summary>
/// <typeparam name="T">The numeric type of the coordinates.</typeparam>
public class KMeansClusterer<T> : IMachineLearning 
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly T[][] _data;
    private readonly int _numClusters;
    private readonly int _maxIterations;
    private readonly T _tolerance;
    private int[] _clustering;
    private T[][] _centroids;

    public MachineLearningTask Task => MachineLearningTask.Clustering;

    /// <summary>
    /// Execution policy honored by the clustering loops. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KMeansClusterer{T}"/> class using the specified data, number of clusters, maximum iterations, and convergence tolerance.
    /// </summary>
    /// <param name="data">The data to cluster.</param>
    /// <param name="numClusters">The number of clusters to create.</param>
    /// <param name="maxIterations">The maximum number of iterations.</param>
    /// <param name="tolerance">The convergence tolerance.</param>
    /// <param name="context">Execution policy context; when null the ambient scope or default context is used.</param>
    public KMeansClusterer(T[][] data, int numClusters, int maxIterations, T tolerance, ComputingContext? context = null)
    {
        _data = data;
        _numClusters = numClusters;
        _maxIterations = maxIterations;
        _tolerance = tolerance;
        _clustering = InitClustering(data.Length, numClusters);
        _centroids = Allocate(numClusters, data[0].Length);
        Context = context;
    }

    /// <summary>
    /// Runs the K-means clustering algorithm.
    /// </summary>
    /// <returns>The cluster assignment of each data point.</returns>
    public int[] Cluster()
    {
        int iteration = 0;
        bool changed = true;
        bool success = true;

        while (changed && success && iteration < _maxIterations)
        {
            ++iteration;
            success = UpdateCentroids();
            changed = UpdateClustering();
        }

        return _clustering;
    }

    /// <summary>
    /// Initializes the clustering.
    /// </summary>
    /// <param name="numData">The number of data points.</param>
    /// <param name="numClusters">The number of clusters.</param>
    private int[] InitClustering(int numData, int numClusters)
    {
        int[] clustering = new int[numData];
        for (int i = 0; i < numClusters; ++i)
            clustering[i] = i;
        for (int i = numClusters; i < numData; ++i)
            clustering[i] = Random.Shared.Next(0, numClusters);
        return clustering;
    }

    /// <summary>
    /// Allocates the space for a jagged two-dimensional array.
    /// </summary>
    /// <param name="numRows">The number of rows.</param>
    /// <param name="numCols">The number of columns.</param>
    private T[][] Allocate(int numRows, int numCols)
    {
        T[][] result = new T[numRows][];
        for (int i = 0; i < numRows; ++i)
            result[i] = new T[numCols];
        return result;
    }

    /// <summary>
    /// Updates the centroids (cluster centers).
    /// </summary>
    private bool UpdateCentroids()
    {
        int numClusters = _centroids.Length;
        int[] clusterCounts = new int[numClusters];
        for (int i = 0; i < _data.Length; ++i)
        {
            int cluster = _clustering[i];
            ++clusterCounts[cluster];
        }

        for (int k = 0; k < numClusters; ++k)
            if (clusterCounts[k] == 0)
                return false;

        for (int k = 0; k < _centroids.Length; ++k)
            for (int j = 0; j < _centroids[k].Length; ++j)
                _centroids[k][j] = T.Zero;

        for (int i = 0; i < _data.Length; ++i)
        {
            int cluster = _clustering[i];
            for (int j = 0; j < _data[i].Length; ++j)
                _centroids[cluster][j] += _data[i][j];
        }

        for (int k = 0; k < _centroids.Length; ++k)
            for (int j = 0; j < _centroids[k].Length; ++j)
                _centroids[k][j] /= T.CreateChecked(clusterCounts[k]);

        return true;
    }

    /// <summary>
    /// Updates the cluster assignment.
    /// </summary>
    private bool UpdateClustering()
    {
        int numClusters = _centroids.Length;

        int[] newClustering = new int[_clustering.Length];
        Array.Copy(_clustering, newClustering, _clustering.Length);

        ComputingContextExecution.ForEach(
            Context,
            0,
            _data.Length,
            i =>
            {
                T[] distances = new T[numClusters];
                for (int k = 0; k < numClusters; ++k)
                    distances[k] = Distance(_data[i], _centroids[k]);

                newClustering[i] = MinIndex(distances);
            },
            workPerItem: numClusters);

        bool isChanged = false;
        for (int i = 0; i < newClustering.Length; ++i)
        {
            if (newClustering[i] != _clustering[i])
            {
                isChanged = true;
                break;
            }
        }

        if (!isChanged)
            return false;

        int[] clusterCounts = new int[numClusters];
        for (int i = 0; i < _data.Length; ++i)
        {
            int cluster = newClustering[i];
            ++clusterCounts[cluster];
        }

        for (int k = 0; k < numClusters; ++k)
            if (clusterCounts[k] == 0)
                return false;

        Array.Copy(newClustering, _clustering, newClustering.Length);
        return true;
    }

    /// <summary>
    /// Computes the Euclidean distance between two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The Euclidean distance between the two vectors.</returns>
    private T Distance(T[] vector1, T[] vector2)
    {
        T sum = T.Zero;
        for (int i = 0; i < vector1.Length; ++i)
            sum += (vector1[i] - vector2[i]) * (vector1[i] - vector2[i]);
        return T.Sqrt(sum);
    }

    /// <summary>
    /// Finds the index of the minimum value in the array.
    /// </summary>
    /// <param name="distances">The array representing distances.</param>
    private int MinIndex(T[] distances)
    {
        int index = 0;
        T min = distances[0];
        for (int i = 0; i < distances.Length; ++i)
        {
            if (distances[i] < min)
            {
                min = distances[i];
                index = i;
            }
        }
        return index;
    }
}
