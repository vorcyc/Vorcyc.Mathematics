using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// 表示用于二维平面点的DBSCAN聚类算法。
/// </summary>
/// <typeparam name="T">坐标的数值类型。</typeparam>
public class DBSCAN<T> :IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly Point<T>[] _points;
    private readonly T _eps;
    private readonly int _minPts;
    private readonly HashSet<Point<T>> _visited;
    private readonly HashSet<Point<T>> _noise;
    private readonly List<List<Point<T>>> _clusters;

    /// <summary>
    /// 使用指定的点、邻域半径和最小点数初始化 <see cref="DBSCAN{T}"/> 类的新实例。
    /// </summary>
    /// <param name="points">要聚类的点。</param>
    /// <param name="eps">邻域半径。</param>
    /// <param name="minPts">形成一个聚类的最小点数。</param>
    public DBSCAN(Point<T>[] points, T eps, int minPts)
    {
        _points = points;
        _eps = eps;
        _minPts = minPts;
        _visited = new HashSet<Point<T>>();
        _noise = new HashSet<Point<T>>();
        _clusters = new List<List<Point<T>>>();
    }

    public MachineLearningTask Task => MachineLearningTask.Clustering;

    /// <summary>
    /// 执行DBSCAN聚类。
    /// </summary>
    /// <returns>聚类的列表，每个聚类是一个点的列表。</returns>
    public List<List<Point<T>>> Cluster()
    {
        foreach (var point in _points)
        {
            if (_visited.Contains(point))
                continue;

            _visited.Add(point);
            var neighbors = GetNeighbors(point);

            if (neighbors.Count < _minPts)
            {
                _noise.Add(point);
            }
            else
            {
                var cluster = new List<Point<T>>();
                _clusters.Add(cluster);
                ExpandCluster(point, neighbors, cluster);
            }
        }

        return _clusters;
    }

    /// <summary>
    /// 扩展聚类。
    /// </summary>
    /// <param name="point">当前点。</param>
    /// <param name="neighbors">当前点的邻居。</param>
    /// <param name="cluster">当前聚类。</param>
    private void ExpandCluster(Point<T> point, List<Point<T>> neighbors, List<Point<T>> cluster)
    {
        cluster.Add(point);

        for (int i = 0; i < neighbors.Count; i++)
        {
            var neighbor = neighbors[i];

            if (!_visited.Contains(neighbor))
            {
                _visited.Add(neighbor);
                var neighborNeighbors = GetNeighbors(neighbor);

                if (neighborNeighbors.Count >= _minPts)
                {
                    neighbors.AddRange(neighborNeighbors);
                }
            }

            if (!_noise.Contains(neighbor))
            {
                cluster.Add(neighbor);
            }
        }
    }

    /// <summary>
    /// 获取点的邻居。
    /// </summary>
    /// <param name="point">当前点。</param>
    /// <returns>邻居的列表。</returns>
    private List<Point<T>> GetNeighbors(Point<T> point)
    {
        var neighbors = new List<Point<T>>();

        foreach (var p in _points)
        {
            if (Point<T>.Distance(point, p) <= _eps)
            {
                neighbors.Add(p);
            }
        }

        return neighbors;
    }
}
