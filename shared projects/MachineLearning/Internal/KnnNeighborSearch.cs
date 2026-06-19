using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Internal;

/// <summary>
/// K-nearest-neighbor search: squared Euclidean distance + a fixed-size k buffer (avoids full sorting and LINQ).
/// </summary>
internal static class KnnNeighborSearch
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SquaredDistanceToRow<T>(T[,] matrix, int row, ReadOnlySpan<T> sample)
        where T : struct, IFloatingPointIeee754<T>
    {
        return NumericKernels.SquaredDistanceToRow(matrix, row, sample);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SquaredDistance<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
        where T : struct, IFloatingPointIeee754<T>
    {
        return NumericKernels.SquaredDistance(a, b);
    }

    /// <summary>
    /// Finds the integer label of the k nearest neighbors on a matrix row (majority vote).
    /// </summary>
    public static int MajorityLabelFromRows<T>(
        T[,] features,
        int[] labels,
        ReadOnlySpan<T> sample,
        int k)
        where T : struct, IFloatingPointIeee754<T>
    {
        SelectTopKFromRows(features, labels, sample, k, out ReadOnlySpan<int> topLabels);
        return ClassificationMath.MajorityVote(topLabels);
    }

    /// <summary>
    /// Majority vote of k nearest neighbors between training-matrix rows and query-matrix rows (batch inference path).
    /// </summary>
    public static int MajorityLabelFromQueryRow<T>(
        T[,] trainFeatures,
        int[] labels,
        T[,] query,
        int queryRow,
        int k)
        where T : struct, IFloatingPointIeee754<T>
    {
        SelectTopKFromQueryRow(trainFeatures, labels, query, queryRow, k, out ReadOnlySpan<int> topLabels);
        return ClassificationMath.MajorityVote(topLabels);
    }

    /// <summary>
    /// Finds the k nearest-neighbor labels over the stored feature vectors.
    /// </summary>
    public static int MajorityLabelFromVectors<T>(
        ReadOnlySpan<T[]> storedFeatures,
        ReadOnlySpan<int> storedLabels,
        ReadOnlySpan<T> sample,
        int k)
        where T : struct, IFloatingPointIeee754<T>
    {
        SelectTopKFromVectors(storedFeatures, storedLabels, sample, k, out ReadOnlySpan<int> topLabels);
        return ClassificationMath.MajorityVote(topLabels);
    }

    /// <summary>
    /// Finds the k nearest neighbors on a matrix row and regresses (average or distance-weighted).
    /// </summary>
    public static T MeanTargetFromRows<T>(
        T[,] features,
        T[] targets,
        ReadOnlySpan<T> sample,
        int k,
        bool distanceWeighted)
        where T : struct, IFloatingPointIeee754<T>
    {
        SelectTopKTargetsFromRows(features, targets, sample, k,
            out ReadOnlySpan<T> topDistances, out ReadOnlySpan<T> topTargets);
        return AggregateTargets(topDistances, topTargets, distanceWeighted);
    }

    /// <summary>
    /// k nearest-neighbor regression between training-matrix rows and query-matrix rows.
    /// </summary>
    public static T MeanTargetFromQueryRow<T>(
        T[,] trainFeatures,
        T[] targets,
        T[,] query,
        int queryRow,
        int k,
        bool distanceWeighted)
        where T : struct, IFloatingPointIeee754<T>
    {
        SelectTopKTargetsFromQueryRow(trainFeatures, targets, query, queryRow, k,
            out ReadOnlySpan<T> topDistances, out ReadOnlySpan<T> topTargets);
        return AggregateTargets(topDistances, topTargets, distanceWeighted);
    }

    /// <summary>
    /// k nearest-neighbor regression (unweighted average).
    /// </summary>
    public static T MeanTargetFromVectors<T>(
        ReadOnlySpan<T[]> storedFeatures,
        ReadOnlySpan<T> storedTargets,
        ReadOnlySpan<T> sample,
        int k,
        bool distanceWeighted)
        where T : struct, IFloatingPointIeee754<T>
    {
        SelectTopKTargets(storedFeatures, storedTargets, sample, k,
            out ReadOnlySpan<T> topDistances, out ReadOnlySpan<T> topTargets);
        return AggregateTargets(topDistances, topTargets, distanceWeighted);
    }

    private static T AggregateTargets<T>(
        ReadOnlySpan<T> topDistances,
        ReadOnlySpan<T> topTargets,
        bool distanceWeighted)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (!distanceWeighted)
        {
            T sum = T.Zero;
            for (int i = 0; i < topTargets.Length; i++)
                sum += topTargets[i];
            return sum / T.CreateChecked(topTargets.Length);
        }

        T weightedSum = T.Zero;
        T weightTotal = T.Zero;
        for (int i = 0; i < topTargets.Length; i++)
        {
            T dist = T.Sqrt(topDistances[i]);
            T weight = dist > T.Zero ? T.One / dist : T.One;
            weightedSum += weight * topTargets[i];
            weightTotal += weight;
        }
        return weightedSum / weightTotal;
    }

    private static void SelectTopKFromRows<T>(
        T[,] features,
        int[] labels,
        ReadOnlySpan<T> sample,
        int k,
        out ReadOnlySpan<int> topLabels)
        where T : struct, IFloatingPointIeee754<T>
    {
        int n = labels.Length;
        var distances = new T[k];
        var labelBuf = new int[k];
        InitializeTopK(distances, labelBuf, k);

        for (int i = 0; i < n; i++)
        {
            T dist2 = SquaredDistanceToRow(features, i, sample);
            TryInsert(distances, labelBuf, k, dist2, labels[i]);
        }

        topLabels = labelBuf;
    }

    private static void SelectTopKFromQueryRow<T>(
        T[,] trainFeatures,
        int[] labels,
        T[,] query,
        int queryRow,
        int k,
        out ReadOnlySpan<int> topLabels)
        where T : struct, IFloatingPointIeee754<T>
    {
        int n = labels.Length;
        var distances = new T[k];
        var labelBuf = new int[k];
        InitializeTopK(distances, labelBuf, k);

        for (int i = 0; i < n; i++)
        {
            T dist2 = NumericKernels.SquaredDistanceBetweenRows(trainFeatures, i, query, queryRow);
            TryInsert(distances, labelBuf, k, dist2, labels[i]);
        }

        topLabels = labelBuf;
    }

    private static void SelectTopKFromVectors<T>(
        ReadOnlySpan<T[]> storedFeatures,
        ReadOnlySpan<int> storedLabels,
        ReadOnlySpan<T> sample,
        int k,
        out ReadOnlySpan<int> topLabels)
        where T : struct, IFloatingPointIeee754<T>
    {
        int n = storedLabels.Length;
        var distances = new T[k];
        var labelBuf = new int[k];
        InitializeTopK(distances, labelBuf, k);

        for (int i = 0; i < n; i++)
        {
            T dist2 = SquaredDistance(storedFeatures[i], sample);
            TryInsert(distances, labelBuf, k, dist2, storedLabels[i]);
        }

        topLabels = labelBuf;
    }

    private static void SelectTopKTargetsFromRows<T>(
        T[,] features,
        T[] targets,
        ReadOnlySpan<T> sample,
        int k,
        out ReadOnlySpan<T> topDistances,
        out ReadOnlySpan<T> topTargets)
        where T : struct, IFloatingPointIeee754<T>
    {
        int n = targets.Length;
        var distances = new T[k];
        var targetBuf = new T[k];
        InitializeTopK(distances, targetBuf, k);

        for (int i = 0; i < n; i++)
        {
            T dist2 = SquaredDistanceToRow(features, i, sample);
            TryInsert(distances, targetBuf, k, dist2, targets[i]);
        }

        topDistances = distances;
        topTargets = targetBuf;
    }

    private static void SelectTopKTargetsFromQueryRow<T>(
        T[,] trainFeatures,
        T[] targets,
        T[,] query,
        int queryRow,
        int k,
        out ReadOnlySpan<T> topDistances,
        out ReadOnlySpan<T> topTargets)
        where T : struct, IFloatingPointIeee754<T>
    {
        int n = targets.Length;
        var distances = new T[k];
        var targetBuf = new T[k];
        InitializeTopK(distances, targetBuf, k);

        for (int i = 0; i < n; i++)
        {
            T dist2 = NumericKernels.SquaredDistanceBetweenRows(trainFeatures, i, query, queryRow);
            TryInsert(distances, targetBuf, k, dist2, targets[i]);
        }

        topDistances = distances;
        topTargets = targetBuf;
    }

    private static void SelectTopKTargets<T>(
        ReadOnlySpan<T[]> storedFeatures,
        ReadOnlySpan<T> storedTargets,
        ReadOnlySpan<T> sample,
        int k,
        out ReadOnlySpan<T> topDistances,
        out ReadOnlySpan<T> topTargets)
        where T : struct, IFloatingPointIeee754<T>
    {
        int n = storedTargets.Length;
        var distances = new T[k];
        var targets = new T[k];
        InitializeTopK(distances, targets, k);

        for (int i = 0; i < n; i++)
        {
            T dist2 = SquaredDistance(storedFeatures[i], sample);
            TryInsert(distances, targets, k, dist2, storedTargets[i]);
        }

        topDistances = distances;
        topTargets = targets;
    }

    private static void InitializeTopK<T>(Span<T> distances, Span<int> labels, int k)
        where T : struct, IFloatingPointIeee754<T>
    {
        T sentinel = T.CreateChecked(double.MaxValue);
        for (int i = 0; i < k; i++)
        {
            distances[i] = sentinel;
            labels[i] = 0;
        }
    }

    private static void InitializeTopK<T>(Span<T> distances, Span<T> targets, int k)
        where T : struct, IFloatingPointIeee754<T>
    {
        T sentinel = T.CreateChecked(double.MaxValue);
        for (int i = 0; i < k; i++)
        {
            distances[i] = sentinel;
            targets[i] = T.Zero;
        }
    }

    /// <summary>
    /// Inserts a candidate into the worst position among the current k largest distances (keeping the k smallest distances).
    /// </summary>
    private static void TryInsert<T>(Span<T> distances, Span<int> labels, int k, T dist2, int label)
        where T : struct, IFloatingPointIeee754<T>
    {
        int worst = 0;
        for (int i = 1; i < k; i++)
        {
            if (distances[i] > distances[worst])
                worst = i;
        }

        if (dist2 >= distances[worst])
            return;

        distances[worst] = dist2;
        labels[worst] = label;
    }

    private static void TryInsert<T>(Span<T> distances, Span<T> targets, int k, T dist2, T target)
        where T : struct, IFloatingPointIeee754<T>
    {
        int worst = 0;
        for (int i = 1; i < k; i++)
        {
            if (distances[i] > distances[worst])
                worst = i;
        }

        if (dist2 >= distances[worst])
            return;

        distances[worst] = dist2;
        targets[worst] = target;
    }
}
