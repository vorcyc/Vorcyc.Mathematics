using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// 表示用于二维平面点的层次聚类算法。
/// </summary>
/// <typeparam name="T">坐标的数值类型。</typeparam>
public class HierarchicalClustering<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly List<List<Point<T>>> _clusters;

    public MachineLearningTask Task => MachineLearningTask.Clustering;

    /// <summary>
    /// 使用指定的点初始化 <see cref="HierarchicalClustering{T}"/> 类的新实例。
    /// </summary>
    /// <param name="points">要聚类的点。</param>
    public HierarchicalClustering(Point<T>[] points)
    {
        _clusters = points.Select(p => new List<Point<T>> { p }).ToList();
    }

    /// <summary>
    /// 对点执行层次聚类，直到达到指定数量的聚类。
    /// </summary>
    /// <param name="k">所需的聚类数量。</param>
    /// <returns>聚类的列表，每个聚类是一个点的列表。</returns>
    public List<List<Point<T>>> Cluster(int k)
    {
        while (_clusters.Count > k)
        {
            (int cluster1, int cluster2, T minDistance) = FindClosestClusters();

            _clusters[cluster1].AddRange(_clusters[cluster2]);
            _clusters.RemoveAt(cluster2);
        }

        return _clusters;
    }

    /// <summary>
    /// 查找最接近的两个聚类。
    /// </summary>
    /// <returns>最接近的两个聚类的索引和它们之间的距离。</returns>
    private (int cluster1, int cluster2, T minDistance) FindClosestClusters()
    {
        T minDistance = T.PositiveInfinity;
        int cluster1 = 0, cluster2 = 0;

        for (int i = 0; i < _clusters.Count; i++)
        {
            for (int j = i + 1; j < _clusters.Count; j++)
            {
                T distance = AverageLinkage(_clusters[i], _clusters[j]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    cluster1 = i;
                    cluster2 = j;
                }
            }
        }

        return (cluster1, cluster2, minDistance);
    }

    /// <summary>
    /// 计算两个聚类之间的平均连接距离。
    /// </summary>
    /// <param name="cluster1">第一个聚类。</param>
    /// <param name="cluster2">第二个聚类。</param>
    /// <returns>两个聚类之间的平均连接距离。</returns>
    private T AverageLinkage(List<Point<T>> cluster1, List<Point<T>> cluster2)
    {
        T totalDistance = T.Zero;
        int count = 0;

        foreach (var point1 in cluster1)
        {
            foreach (var point2 in cluster2)
            {
                totalDistance += Distance(point1, point2);
                count++;
            }
        }

        return totalDistance / T.CreateChecked(count);
    }

    /// <summary>
    /// 计算两个点之间的欧几里得距离。
    /// </summary>
    /// <param name="a">第一个点。</param>
    /// <param name="b">第二个点。</param>
    /// <returns>两个点之间的欧几里得距离。</returns>
    private T Distance(Point<T> a, Point<T> b)
    {
        T dx = a.X - b.X;
        T dy = a.Y - b.Y;
        return T.Sqrt(dx * dx + dy * dy);
    }
}
