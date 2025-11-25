namespace Vorcyc.Mathematics.MachineLearning.Clustering;

using System.Text.Json;
using System.Numerics;

/// <summary>
/// 表示矢量量化算法的类。
/// </summary>
/// <typeparam name="T">矢量元素的类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 和 <see cref="IMinMaxValue{T}"/> 接口。</typeparam>
/// <remarks>
/// 矢量量化（Vector Quantization, VQ）是一种经典的信号处理技术，广泛应用于数据压缩、图像处理和模式识别等领域。
/// 其基本思想是将高维空间中的矢量映射到一个有限的矢量集合（码书）中，从而实现数据的压缩和特征提取。
/// 
/// 在训练过程中，算法通过迭代优化码书中的矢量，使其更好地代表输入数据集。每次迭代将输入矢量分配到最近的码矢量，并更新码矢量为其对应聚类的质心。
/// 
/// 该类使用 <see cref="T"/> 数组表示矢量，并在必要时通过 <see cref="Span{T}"/> 或 <see cref="ReadOnlySpan{T}"/> 进行高效操作。
/// </remarks>
public class VectorQuantization<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    private readonly List<T[]> _codebook; // 码书，存储矢量的数组列表
    private readonly int _dimensions;     // 矢量的维度

    /// <summary>
    /// 获取码书。
    /// </summary>
    public IReadOnlyList<T[]> Codebook => _codebook;

    /// <summary>
    /// 获取机器学习任务类型。
    /// </summary>
    public MachineLearningTask Task => MachineLearningTask.Clustering;

    /// <summary>
    /// 初始化 <see cref="VectorQuantization{T}"/> 类的新实例。
    /// </summary>
    /// <param name="codebookSize">码书的大小，必须为正整数。</param>
    /// <param name="dimensions">矢量的维度，必须为正整数。</param>
    /// <exception cref="ArgumentException">当 <paramref name="codebookSize"/> 或 <paramref name="dimensions"/> 小于等于 0 时抛出。</exception>
    public VectorQuantization(int codebookSize, int dimensions)
    {
        if (codebookSize <= 0)
            throw new ArgumentException("码书大小必须为正整数。", nameof(codebookSize));
        if (dimensions <= 0)
            throw new ArgumentException("矢量维度必须为正整数。", nameof(dimensions));

        _dimensions = dimensions;
        _codebook = InitializeCodebook(codebookSize, dimensions);
    }

    /// <summary>
    /// 初始化码书。
    /// </summary>
    /// <param name="codebookSize">码书的大小。</param>
    /// <param name="dimensions">矢量的维度。</param>
    /// <returns>初始化后的码书。</returns>
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
    /// 训练矢量量化模型。
    /// </summary>
    /// <param name="data">训练数据，包含多个矢量。</param>
    /// <param name="maxIterations">最大迭代次数，默认值为 100。</param>
    /// <returns>每次迭代的误差列表。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="data"/> 为 null 时抛出。</exception>
    /// <exception cref="ArgumentException">当 <paramref name="data"/> 为空或矢量维度不匹配时抛出。</exception>
    public List<T> Train(IEnumerable<T[]> data, int maxIterations = 100)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "训练数据不能为 null。");
        var dataList = data.ToList();
        if (dataList.Count == 0)
            throw new ArgumentException("训练数据不能为空。", nameof(data));
        if (dataList[0].Length != _dimensions)
            throw new ArgumentException("输入矢量的维度与码书维度不匹配。", nameof(data));

        var errors = new List<T>(maxIterations);
        var clusters = new List<List<T[]>>(_codebook.Count);

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            // 重置聚类
            clusters.Clear();
            for (int i = 0; i < _codebook.Count; i++)
                clusters.Add(new List<T[]>());

            // 分配矢量到最近的码矢量
            foreach (var vector in dataList)
            {
                int nearestIndex = FindNearestCodeVector(vector);
                clusters[nearestIndex].Add(vector);
            }

            // 更新码矢量
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

            // 计算误差并检查收敛
            T error = CalculateError(dataList);
            errors.Add(error);

            if (!anyChange && iteration > 0)
                break;
        }

        return errors;
    }

    /// <summary>
    /// 查找与给定矢量最近的码矢量。
    /// </summary>
    /// <param name="vector">给定的矢量。</param>
    /// <returns>最近的码矢量的索引。</returns>
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
    /// 计算两个矢量的欧几里得距离。
    /// </summary>
    /// <param name="a">第一个矢量。</param>
    /// <param name="b">第二个矢量。</param>
    /// <returns>欧几里得距离。</returns>
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
    /// 计算给定矢量列表的质心。
    /// </summary>
    /// <param name="vectors">矢量列表。</param>
    /// <returns>质心矢量的元素数组。</returns>
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
    /// 计算当前码书与数据集之间的误差。
    /// </summary>
    /// <param name="data">数据集。</param>
    /// <returns>误差值。</returns>
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
    /// 根据输入矢量找到最近的码矢量。
    /// </summary>
    /// <param name="vector">输入矢量。</param>
    /// <returns>最近的码矢量。</returns>
    /// <exception cref="ArgumentException">当 <paramref name="vector"/> 的维度与码书不匹配时抛出。</exception>
    public T[] Predict(T[] vector)
    {
        if (vector == null || vector.Length != _dimensions)
            throw new ArgumentException("输入矢量的维度与码书维度不匹配。", nameof(vector));
        int nearestIndex = FindNearestCodeVector(vector);
        return _codebook[nearestIndex];
    }

    /// <summary>
    /// 将码书保存到文件中。
    /// </summary>
    /// <param name="filePath">文件路径。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="filePath"/> 为 null 或空时抛出。</exception>
    public void SaveCodebook(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath), "文件路径不能为空。");

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(_codebook, options);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 从文件中加载码书。
    /// </summary>
    /// <param name="filePath">文件路径。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="filePath"/> 为 null 或空时抛出。</exception>
    /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
    public void LoadCodebook(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath), "文件路径不能为空。");
        if (!File.Exists(filePath))
            throw new FileNotFoundException("指定的文件不存在。", filePath);

        string json = File.ReadAllText(filePath);
        var loadedCodebook = JsonSerializer.Deserialize<List<T[]>>(json);
        if (loadedCodebook == null || loadedCodebook.Count == 0)
            throw new InvalidOperationException("加载的码书为空或无效。");
        if (loadedCodebook[0].Length != _dimensions)
            throw new InvalidOperationException("加载的码书维度与当前实例不匹配。");

        _codebook.Clear();
        _codebook.AddRange(loadedCodebook);
    }
}