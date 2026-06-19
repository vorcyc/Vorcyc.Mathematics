using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// Represents the DBSCAN clustering algorithm for points on a 2D plane.
/// </summary>
/// <typeparam name="T">The numeric type of the coordinates.</typeparam>
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
    /// Execution policy honored by the neighbor search. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DBSCAN{T}"/> class using the specified points, neighborhood radius, and minimum number of points.
    /// </summary>
    /// <param name="points">The points to cluster.</param>
    /// <param name="eps">The neighborhood radius.</param>
    /// <param name="minPts">The minimum number of points required to form a cluster.</param>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    public DBSCAN(Point<T>[] points, T eps, int minPts, ComputingContext? context = null)
    {
        _points = points;
        _eps = eps;
        _minPts = minPts;
        _visited = new HashSet<Point<T>>();
        _noise = new HashSet<Point<T>>();
        _clusters = new List<List<Point<T>>>();
        Context = context;
    }

    public MachineLearningTask Task => MachineLearningTask.Clustering;

    /// <summary>
    /// Runs the DBSCAN clustering.
    /// </summary>
    /// <returns>The list of clusters; each cluster is a list of points.</returns>
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
    /// Expands the cluster.
    /// </summary>
    /// <param name="point">The current point.</param>
    /// <param name="neighbors">The neighbors of the current point.</param>
    /// <param name="cluster">The current cluster.</param>
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
    /// Gets the neighbors of a point.
    /// </summary>
    /// <param name="point">The current point.</param>
    /// <returns>The list of neighbors.</returns>
    private List<Point<T>> GetNeighbors(Point<T> point)
    {
        var neighbors = new List<Point<T>>();
        bool[] isNeighbor = new bool[_points.Length];

        ComputingContextExecution.ForEach(
            Context,
            0,
            _points.Length,
            i =>
            {
                isNeighbor[i] = Point<T>.Distance(point, _points[i]) <= _eps;
            });

        for (int i = 0; i < _points.Length; i++)
        {
            if (isNeighbor[i])
            {
                neighbors.Add(_points[i]);
            }
        }

        return neighbors;
    }
}
