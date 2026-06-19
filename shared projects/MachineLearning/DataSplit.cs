using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// Train/test split utilities.
/// </summary>
public static class DataSplit
{
    /// <summary>
    /// Generates Fisher-Yates shuffled indices.
    /// </summary>
    public static int[] CreateShuffledIndices(int count, int? seed = null)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var indices = Enumerable.Range(0, count).ToArray();
        var random = seed.HasValue ? new Random(seed.Value) : Random.Shared;
        for (int i = indices.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }
        return indices;
    }

    /// <summary>
    /// Splits indices by ratio.
    /// </summary>
    public static (int[] trainIndices, int[] testIndices) SplitIndices(int count, double testRatio, int? seed = null)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (testRatio <= 0 || testRatio >= 1)
            throw new ArgumentOutOfRangeException(nameof(testRatio), "The test-set ratio must be within (0, 1).");

        var shuffled = CreateShuffledIndices(count, seed);
        int testCount = Math.Max(1, (int)Math.Round(count * testRatio));
        if (testCount >= count)
            testCount = count - 1;

        var test = shuffled.AsSpan(0, testCount).ToArray();
        var train = shuffled.AsSpan(testCount).ToArray();
        return (train, test);
    }

    /// <summary>
    /// Splits a one-dimensional array.
    /// </summary>
    public static (T[] train, T[] test) Split<T>(ReadOnlySpan<T> data, double testRatio, int? seed = null)
    {
        var (trainIdx, testIdx) = SplitIndices(data.Length, testRatio, seed);
        var train = new T[trainIdx.Length];
        var test = new T[testIdx.Length];
        for (int i = 0; i < trainIdx.Length; i++)
            train[i] = data[trainIdx[i]];
        for (int i = 0; i < testIdx.Length; i++)
            test[i] = data[testIdx[i]];
        return (train, test);
    }

    /// <summary>
    /// Splits a feature matrix and integer classification labels.
    /// </summary>
    public static (T[,] xTrain, int[] yTrain, T[,] xTest, int[] yTest) TrainTestSplit<T>(
        T[,] x, int[] y, double testRatio, int? seed = null)
        where T : struct
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || y.Length == 0)
            throw new ArgumentException("Training data cannot be empty.");
        if (rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");

        var (trainIdx, testIdx) = SplitIndices(rows, testRatio, seed);
        return (
            Array2DHelpers.CopyRows(x, trainIdx),
            Array2DHelpers.CopyIntLabels(y, trainIdx),
            Array2DHelpers.CopyRows(x, testIdx),
            Array2DHelpers.CopyIntLabels(y, testIdx));
    }

    /// <summary>
    /// Splits a feature matrix and a label vector.
    /// </summary>
    public static (T[,] xTrain, T[] yTrain, T[,] xTest, T[] yTest) TrainTestSplit<T>(
        T[,] x, T[] y, double testRatio, int? seed = null)
        where T : struct
    {
        if (x == null || y == null)
            throw new ArgumentException("Input cannot be null.");
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || y.Length == 0)
            throw new ArgumentException("Training data cannot be empty.");
        if (rows != y.Length)
            throw new ArgumentException("The number of samples does not match the number of labels.");

        var (trainIdx, testIdx) = SplitIndices(rows, testRatio, seed);
        return (
            Array2DHelpers.CopyRows(x, trainIdx),
            Array2DHelpers.CopyLabels(y, trainIdx),
            Array2DHelpers.CopyRows(x, testIdx),
            Array2DHelpers.CopyLabels(y, testIdx));
    }
}
