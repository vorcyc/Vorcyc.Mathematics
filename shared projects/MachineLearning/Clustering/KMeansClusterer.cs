
/* usage : */
//todo 用随机特征 :
//Random random = new Random();
//int numData = 10000; // 数据的数量
//int numFeatures = 512; // 数据的特征维度
//var data = new float[numData][]; // 数据矩阵
//for (int i = 0; i < numData; i++)
//{
//    data[i] = new float[numFeatures];
//    for (int j = 0; j < numFeatures; j++)
//    {
//        data[i][j] = random.NextSingle() * 10; // 生成0到10之间的随机数
//    }
//}

//todo 用特征 :
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
//// 设置聚类的参数
//using static System.Runtime.InteropServices.JavaScript.JSType;
//using Vorcyc.Offlet.MachineLearning.Clustering;

//int numClusters = 2; // 聚类的数量
//int maxIterations = 100; // 最大迭代次数
//float tolerance = 0.01f; // 收敛的容差

//// 调用K均值聚类算法
//int[] clustering = KMeansClusterer.KMeans(data, numClusters, maxIterations, tolerance);

//// 显示聚类的结果
//Console.WriteLine("数据的聚类结果如下：");
//for (int i = 0; i < 5; i++)
//{
//    Console.WriteLine($"数据 {i}: ({data[i][0]:F2}, {data[i][1]:F2}) 属于聚类 {clustering[i]}");
//}


using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// 提供 K 均值聚类算法的类。
/// </summary>
/// <typeparam name="T">坐标的数值类型。</typeparam>
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
    /// 使用指定的数据、聚类数量、最大迭代次数和收敛容忍度初始化 <see cref="KMeansClusterer{T}"/> 类的新实例。
    /// </summary>
    /// <param name="data">要聚类的数据。</param>
    /// <param name="numClusters">要创建的聚类数量。</param>
    /// <param name="maxIterations">最大迭代次数。</param>
    /// <param name="tolerance">收敛容忍度。</param>
    public KMeansClusterer(T[][] data, int numClusters, int maxIterations, T tolerance)
    {
        _data = data;
        _numClusters = numClusters;
        _maxIterations = maxIterations;
        _tolerance = tolerance;
        _clustering = InitClustering(data.Length, numClusters);
        _centroids = Allocate(numClusters, data[0].Length);
    }

    /// <summary>
    /// 执行 K 均值聚类算法。
    /// </summary>
    /// <returns>每个数据点的聚类分配。</returns>
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
    /// 初始化聚类。
    /// </summary>
    /// <param name="numData">数据的数量。</param>
    /// <param name="numClusters">聚类的数量。</param>
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
    /// 分配二维数组的空间。
    /// </summary>
    /// <param name="numRows">行数。</param>
    /// <param name="numCols">列数。</param>
    private T[][] Allocate(int numRows, int numCols)
    {
        T[][] result = new T[numRows][];
        for (int i = 0; i < numRows; ++i)
            result[i] = new T[numCols];
        return result;
    }

    /// <summary>
    /// 更新质心（聚类中心）。
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
    /// 更新聚类分配。
    /// </summary>
    private bool UpdateClustering()
    {
        int numClusters = _centroids.Length;
        bool isChanged = false;

        int[] newClustering = new int[_clustering.Length];
        Array.Copy(_clustering, newClustering, _clustering.Length);

        T[] distances = new T[numClusters];

        for (int i = 0; i < _data.Length; ++i)
        {
            for (int k = 0; k < numClusters; ++k)
                distances[k] = Distance(_data[i], _centroids[k]);

            int newCluster = MinIndex(distances);
            if (newCluster != newClustering[i])
            {
                isChanged = true;
                newClustering[i] = newCluster;
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
    /// 计算两个向量之间的欧几里得距离。
    /// </summary>
    /// <param name="vector1">第一个向量。</param>
    /// <param name="vector2">第二个向量。</param>
    /// <returns>两个向量之间的欧几里得距离。</returns>
    private T Distance(T[] vector1, T[] vector2)
    {
        T sum = T.Zero;
        for (int i = 0; i < vector1.Length; ++i)
            sum += (vector1[i] - vector2[i]) * (vector1[i] - vector2[i]);
        return T.Sqrt(sum);
    }

    /// <summary>
    /// 找到数组中最小值的索引。
    /// </summary>
    /// <param name="distances">表示距离的数组。</param>
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
