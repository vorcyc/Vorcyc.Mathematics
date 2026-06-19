using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Clustering;

/// <summary>
/// Represents a hierarchical clustering algorithm for points in a two-dimensional plane.
/// </summary>
/// <typeparam name="T">The numeric type of the coordinates.</typeparam>
public class HierarchicalClustering<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly List<List<Point<T>>> _clusters;

    public MachineLearningTask Task => MachineLearningTask.Clustering;

    /// <summary>
    /// Execution policy honored by the pairwise distance search. When null, the ambient
    /// <see cref="ComputingScope"/> and then <see cref="ComputingContext.Default"/> are used.
    /// </summary>
    public ComputingContext? Context { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HierarchicalClustering{T}"/> class with the specified points.
    /// </summary>
    /// <param name="points">The points to cluster.</param>
    /// <param name="context">Optional execution policy; when null the ambient scope or default context is used.</param>
    public HierarchicalClustering(Point<T>[] points, ComputingContext? context = null)
    {
        _clusters = points.Select(p => new List<Point<T>> { p }).ToList();
        Context = context;
    }

    /// <summary>
    /// Performs hierarchical clustering on the points until the specified number of clusters is reached.
    /// </summary>
    /// <param name="k">The desired number of clusters.</param>
    /// <returns>A list of clusters, where each cluster is a list of points.</returns>
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
    /// Finds the two closest clusters.
    /// </summary>
    /// <returns>The indices of the two closest clusters and the distance between them.</returns>
    private (int cluster1, int cluster2, T minDistance) FindClosestClusters()
    {
        int count = _clusters.Count;
        var rowBestDistance = new T[count];
        var rowBestJ = new int[count];

        // Each row i independently finds its closest partner j > i (ascending j wins ties).
        ComputingContextExecution.ForEach(
            Context,
            0,
            count,
            i =>
            {
                T localMin = T.PositiveInfinity;
                int localJ = 0;
                for (int j = i + 1; j < count; j++)
                {
                    T distance = AverageLinkage(_clusters[i], _clusters[j]);
                    if (distance < localMin)
                    {
                        localMin = distance;
                        localJ = j;
                    }
                }
                rowBestDistance[i] = localMin;
                rowBestJ[i] = localJ;
            },
            workPerItem: count);

        // Serial reduction in ascending i order preserves the original tie-breaking.
        T minDistance = T.PositiveInfinity;
        int cluster1 = 0, cluster2 = 0;
        for (int i = 0; i < count; i++)
        {
            if (rowBestDistance[i] < minDistance)
            {
                minDistance = rowBestDistance[i];
                cluster1 = i;
                cluster2 = rowBestJ[i];
            }
        }

        return (cluster1, cluster2, minDistance);
    }

    /// <summary>
    /// Computes the average linkage distance between two clusters.
    /// </summary>
    /// <param name="cluster1">The first cluster.</param>
    /// <param name="cluster2">The second cluster.</param>
    /// <returns>The average linkage distance between the two clusters.</returns>
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
    /// Computes the Euclidean distance between two points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The Euclidean distance between the two points.</returns>
    private T Distance(Point<T> a, Point<T> b)
    {
        T dx = a.X - b.X;
        T dy = a.Y - b.Y;
        return T.Sqrt(dx * dx + dy * dy);
    }
}
