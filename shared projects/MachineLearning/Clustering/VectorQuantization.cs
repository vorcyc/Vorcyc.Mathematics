using System.Numerics;
using System.Text.Json;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// 表示矢量量化算法的类。
/// </summary>
/// <typeparam name="T">矢量元素的类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 和 <see cref="IMinMaxValue{T}"/> 接口。</typeparam>
/// <remarks>
/// 矢量量化（Vector Quantization, VQ）是一种经典的信号处理技术，广泛应用于数据压缩、图像处理和模式识别等领域。
/// 其基本思想是将高维空间中的矢量映射到一个有限的矢量集合（码书）中，从而实现数据的压缩和特征提取。
/// 
/// 在训练过程中，矢量量化算法通过迭代优化码书中的矢量，使其能够更好地代表输入数据集中的矢量。
/// 每次迭代中，算法将输入数据集中的每个矢量分配到最近的码矢量，然后更新码矢量为其对应聚类的质心。
/// 
/// 该类提供了初始化码书、训练模型、查找最近码矢量和计算质心等功能。
/// </remarks>
public class VectorQuantization<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    /// <summary>
    /// 码书，存储矢量的列表。
    /// </summary>
    private List<Vorcyc.Mathematics.LinearAlgebra.Vector<T>> _codebook;

    /// <summary>
    /// 矢量的维度。
    /// </summary>
    private int _dimensions;

    /// <summary>
    /// 获取码书。
    /// </summary>
    public IReadOnlyList<Vorcyc.Mathematics.LinearAlgebra.Vector<T>> Codebook => _codebook;

    public MachineLearningTask Task => MachineLearningTask.Clustering;



    /// <summary>
    /// 初始化 <see cref="VectorQuantization{T}"/> 类的新实例。
    /// </summary>
    /// <param name="codebookSize">码书的大小。</param>
    /// <param name="dimensions">矢量的维度。</param>
    public VectorQuantization(int codebookSize, int dimensions)
    {
        this._dimensions = dimensions;
        this._codebook = InitializeCodebook(codebookSize, dimensions);
    }

    /// <summary>
    /// 初始化码书。
    /// </summary>
    /// <param name="codebookSize">码书的大小。</param>
    /// <param name="dimensions">矢量的维度。</param>
    /// <returns>初始化后的码书。</returns>
    private List<Vorcyc.Mathematics.LinearAlgebra.Vector<T>> InitializeCodebook(int codebookSize, int dimensions)
    {
        var codebook = new List<Vorcyc.Mathematics.LinearAlgebra.Vector<T>>();
        for (int i = 0; i < codebookSize; i++)
        {
            var elements = new T[dimensions];
            for (int j = 0; j < dimensions; j++)
            {
                // 使用随机数初始化矢量元素
                elements[j] = T.CreateChecked(Random.Shared.NextDouble());
            }
            codebook.Add(new Vorcyc.Mathematics.LinearAlgebra.Vector<T>(elements));
        }
        return codebook;
    }

    /// <summary>
    /// 训练矢量量化模型。
    /// </summary>
    /// <param name="data">训练数据，包含多个矢量。</param>
    /// <param name="maxIterations">最大迭代次数。</param>
    /// <returns>每次迭代的误差列表。</returns>
    public List<T> Train(IEnumerable<Vorcyc.Mathematics.LinearAlgebra.Vector<T>> data, int maxIterations = 100)
    {
        var errors = new List<T>();

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            // 初始化聚类
            var clusters = new List<List<Vorcyc.Mathematics.LinearAlgebra.Vector<T>>>(_codebook.Count);
            for (int i = 0; i < _codebook.Count; i++)
            {
                clusters.Add(new List<Vorcyc.Mathematics.LinearAlgebra.Vector<T>>());
            }

            // 将每个矢量分配到最近的码矢量
            foreach (var vector in data)
            {
                int nearestIndex = FindNearestCodeVector(vector);
                clusters[nearestIndex].Add(vector);
            }

            // 更新码矢量为聚类的质心
            for (int i = 0; i < _codebook.Count; i++)
            {
                if (clusters[i].Count > 0)
                {
                    _codebook[i] = CalculateCentroid(clusters[i]);
                }
            }

            // 计算当前迭代的误差
            T error = CalculateError(data);
            errors.Add(error);
        }

        return errors;
    }

    /// <summary>
    /// 查找与给定矢量最近的码矢量。
    /// </summary>
    /// <param name="vector">给定的矢量。</param>
    /// <returns>最近的码矢量的索引。</returns>
    private int FindNearestCodeVector(Vorcyc.Mathematics.LinearAlgebra.Vector<T> vector)
    {
        int nearestIndex = -1;
        T minDistance = T.MaxValue;

        // 计算每个码矢量与给定矢量的欧几里得距离
        for (int i = 0; i < _codebook.Count; i++)
        {
            T distance = Distances.Euclidean<T>.Distance(vector.Elements, _codebook[i].Elements);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    /// <summary>
    /// 计算给定矢量列表的质心。
    /// </summary>
    /// <param name="vectors">矢量列表。</param>
    /// <returns>质心矢量。</returns>
    private Vorcyc.Mathematics.LinearAlgebra.Vector<T> CalculateCentroid(List<Vorcyc.Mathematics.LinearAlgebra.Vector<T>> vectors)
    {
        var centroidElements = new T[_dimensions];
        foreach (var vector in vectors)
        {
            for (int i = 0; i < _dimensions; i++)
            {
                // 累加每个矢量的对应元素
                centroidElements[i] += vector.Elements[i];
            }
        }
        for (int i = 0; i < _dimensions; i++)
        {
            // 计算平均值
            centroidElements[i] /= T.CreateChecked(vectors.Count);
        }
        return new Vorcyc.Mathematics.LinearAlgebra.Vector<T>(centroidElements);
    }

    /// <summary>
    /// 计算当前码书与数据集之间的误差。
    /// </summary>
    /// <param name="data">数据集。</param>
    /// <returns>误差值。</returns>
    private T CalculateError(IEnumerable<Vorcyc.Mathematics.LinearAlgebra.Vector<T>> data)
    {
        T totalError = T.Zero;
        foreach (var vector in data)
        {
            int nearestIndex = FindNearestCodeVector(vector);
            T distance = Distances.Euclidean<T>.Distance(vector.Elements, _codebook[nearestIndex].Elements);
            totalError += distance;
        }
        return totalError;
    }

    /// <summary>
    /// 根据输入矢量找到最近的码矢量。
    /// </summary>
    /// <param name="vector">输入矢量。</param>
    /// <returns>最近的码矢量。</returns>
    public Vorcyc.Mathematics.LinearAlgebra.Vector<T> Predict(Vorcyc.Mathematics.LinearAlgebra.Vector<T> vector)
    {
        int nearestIndex = FindNearestCodeVector(vector);
        return _codebook[nearestIndex];
    }

    /// <summary>
    /// 将码书保存到文件中。
    /// </summary>
    /// <param name="filePath">文件路径。</param>
    public void SaveCodebook(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_codebook, options);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 从文件中加载码书。
    /// </summary>
    /// <param name="filePath">文件路径。</param>
    public void LoadCodebook(string filePath)
    {
        var json = File.ReadAllText(filePath);
        _codebook = JsonSerializer.Deserialize<List<Vorcyc.Mathematics.LinearAlgebra.Vector<T>>>(json);
    }
}
