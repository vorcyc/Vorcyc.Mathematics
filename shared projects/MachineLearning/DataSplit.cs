using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 训练集与测试集划分工具。
/// </summary>
public static class DataSplit
{
    /// <summary>
    /// 生成 Fisher-Yates 洗牌后的索引。
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
    /// 按比例划分索引。
    /// </summary>
    public static (int[] trainIndices, int[] testIndices) SplitIndices(int count, double testRatio, int? seed = null)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (testRatio <= 0 || testRatio >= 1)
            throw new ArgumentOutOfRangeException(nameof(testRatio), "测试集比例必须在 (0, 1) 内。");

        var shuffled = CreateShuffledIndices(count, seed);
        int testCount = Math.Max(1, (int)Math.Round(count * testRatio));
        if (testCount >= count)
            testCount = count - 1;

        var test = shuffled.AsSpan(0, testCount).ToArray();
        var train = shuffled.AsSpan(testCount).ToArray();
        return (train, test);
    }

    /// <summary>
    /// 划分一维数组。
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
    /// 划分特征矩阵与整数分类标签。
    /// </summary>
    public static (T[,] xTrain, int[] yTrain, T[,] xTest, int[] yTest) TrainTestSplit<T>(
        T[,] x, int[] y, double testRatio, int? seed = null)
        where T : struct
    {
        if (x == null || y == null)
            throw new ArgumentException("输入不能为 null。");
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || y.Length == 0)
            throw new ArgumentException("训练数据不能为空。");
        if (rows != y.Length)
            throw new ArgumentException("样本数与标签数不匹配。");

        var (trainIdx, testIdx) = SplitIndices(rows, testRatio, seed);
        return (
            Array2DHelpers.CopyRows(x, trainIdx),
            Array2DHelpers.CopyIntLabels(y, trainIdx),
            Array2DHelpers.CopyRows(x, testIdx),
            Array2DHelpers.CopyIntLabels(y, testIdx));
    }

    /// <summary>
    /// 划分特征矩阵与标签向量。
    /// </summary>
    public static (T[,] xTrain, T[] yTrain, T[,] xTest, T[] yTest) TrainTestSplit<T>(
        T[,] x, T[] y, double testRatio, int? seed = null)
        where T : struct
    {
        if (x == null || y == null)
            throw new ArgumentException("输入不能为 null。");
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || y.Length == 0)
            throw new ArgumentException("训练数据不能为空。");
        if (rows != y.Length)
            throw new ArgumentException("样本数与标签数不匹配。");

        var (trainIdx, testIdx) = SplitIndices(rows, testRatio, seed);
        return (
            Array2DHelpers.CopyRows(x, trainIdx),
            Array2DHelpers.CopyLabels(y, trainIdx),
            Array2DHelpers.CopyRows(x, testIdx),
            Array2DHelpers.CopyLabels(y, testIdx));
    }
}
