
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


namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// 提供 K 均值聚类算法的静态类。
public static class KMeansClusterer
{


    /// <summary>
    /// K均值聚类算法。
    /// </summary>
    /// <param name="data">二维数组，表示要聚类的数据。</param>
    /// <param name="numClusters">要创建的聚类数量。</param>
    /// <param name="maxIterations">最大迭代次数。</param>
    /// <param name="tolerance">收敛容忍度。</param>
    /// <returns>每个数据点的聚类分配。</returns>
    public static int[] KMeans(float[][] data, int numClusters, int maxIterations, float tolerance)
    {
        int numData = data.Length; // 数据的数量
        int numFeatures = data[0].Length; // 数据的特征维度
        int[] clustering = InitClustering(numData, numClusters); // 初始化聚类
        float[][] centroids = Allocate(numClusters, numFeatures); // 分配聚类中心的空间
        int iteration = 0; // 迭代次数
        bool changed = true; // 聚类是否发生变化
        bool success = true; // 聚类是否成功

        while (changed && success && iteration < maxIterations)
        {
            ++iteration; // 增加迭代次数
            success = UpdateCentroids(data, clustering, centroids); // 更新聚类中心
            changed = UpdateClustering(data, clustering, centroids); // 更新聚类
        }

        return clustering; // 返回聚类结果
    }

    /// <summary>
    /// 初始化聚类。
    /// </summary>
    /// <param name="numData">数据的数量。</param>
    /// <param name="numClusters">聚类的数量。</param>
    static int[] InitClustering(int numData, int numClusters)
    {
        int[] clustering = new int[numData];
        for (int i = 0; i < numClusters; ++i) // 将前numClusters个数据分别分配到不同的聚类
            clustering[i] = i;
        for (int i = numClusters; i < numData; ++i) // 将剩余的数据随机分配到任意一个聚类
            clustering[i] = Random.Shared.Next(0, numClusters);
        return clustering;
    }

    /// <summary>
    /// 分配二维数组的空间。
    /// </summary>
    /// <param name="numRows">行数。</param>
    /// <param name="numCols">列数。</param>
    static float[][] Allocate(int numRows, int numCols)
    {
        float[][] result = new float[numRows][];
        for (int i = 0; i < numRows; ++i)
            result[i] = new float[numCols];
        return result;
    }

    /// <summary>
    /// 更新质心（聚类中心）。
    /// </summary>
    /// <param name="data">要聚类的数据。</param>
    /// <param name="clustering">当前的聚类分配。</param>
    /// <param name="centroids">质心（聚类中心）。</param>
    static bool UpdateCentroids(float[][] data, int[] clustering, float[][] centroids)
    {
        int numClusters = centroids.Length;
        int[] clusterCounts = new int[numClusters]; // 每个聚类中的数据数量
        for (int i = 0; i < data.Length; ++i)
        {
            int cluster = clustering[i];
            ++clusterCounts[cluster];
        }

        for (int k = 0; k < numClusters; ++k)
            if (clusterCounts[k] == 0) // 如果某个聚类中没有数据，说明聚类失败
                return false;

        for (int k = 0; k < centroids.Length; ++k) // 将聚类中心清零
            for (int j = 0; j < centroids[k].Length; ++j)
                centroids[k][j] = 0.0f;

        for (int i = 0; i < data.Length; ++i) // 累加每个聚类中的数据
        {
            int cluster = clustering[i];
            for (int j = 0; j < data[i].Length; ++j)
                centroids[cluster][j] += data[i][j];
        }

        for (int k = 0; k < centroids.Length; ++k) // 计算每个聚类中的数据的平均值，作为新的聚类中心
            for (int j = 0; j < centroids[k].Length; ++j)
                centroids[k][j] /= clusterCounts[k];

        return true;
    }

    /// <summary>
    /// 更新聚类分配。
    /// </summary>
    /// <param name="data">要聚类的数据。</param>
    /// <param name="clustering">当前的聚类分配。</param>
    /// <param name="centroids">质心（聚类中心）。</param>
    static bool UpdateClustering(float[][] data, int[] clustering, float[][] centroids)
    {
        int numClusters = centroids.Length;
        bool isChanged = false;

        int[] newClustering = new int[clustering.Length]; // 保存新的聚类结果
        Array.Copy(clustering, newClustering, clustering.Length);

        float[] distances = new float[numClusters]; // 保存数据到每个聚类中心的距离

        for (int i = 0; i < data.Length; ++i) // 遍历每个数据
        {
            for (int k = 0; k < numClusters; ++k) // 计算到每个聚类中心的距离
                distances[k] = System.Numerics.Tensors.TensorPrimitives.Distance(data[i], centroids[k]);

            int newCluster = MinIndex(distances); // 找到最近的聚类中心
            if (newCluster != newClustering[i]) // 如果数据的聚类发生变化
            {
                isChanged = true; // 标记聚类发生变化
                newClustering[i] = newCluster; // 更新数据的聚类
            }
        }

        if (!isChanged) // 如果聚类没有变化，直接返回
            return false;

        int[] clusterCounts = new int[numClusters]; // 每个聚类中的数据数量
        for (int i = 0; i < data.Length; ++i)
        {
            int cluster = newClustering[i];
            ++clusterCounts[cluster];
        }

        for (int k = 0; k < numClusters; ++k)
            if (clusterCounts[k] == 0) // 如果某个聚类中没有数据，说明聚类失败
                return false;

        Array.Copy(newClustering, clustering, newClustering.Length); // 将新的聚类结果复制到原来的聚类
        return true;
    }

    //// 计算两个向量的欧几里得距离
    //static double Distance(double[] vector1, double[] vector2)
    //{
    //    double sum = 0.0;
    //    for (int i = 0; i < vector1.Length; ++i)
    //        sum += (vector1[i] - vector2[i]) * (vector1[i] - vector2[i]);
    //    return Math.Sqrt(sum);
    //}

    /// <summary>
    /// 找到数组中最小值的索引。
    /// </summary>
    /// <param name="distances">表示距离的数组。</param>
    static int MinIndex(float[] distances)
    {
        int index = 0;
        float min = distances[0];
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

