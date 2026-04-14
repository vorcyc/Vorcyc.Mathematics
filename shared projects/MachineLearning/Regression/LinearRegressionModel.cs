using System.Numerics;
using Vorcyc.Mathematics.MachineLearning.Internal;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// 多元仿射回归模型（截距 + 系数）的共享逻辑。
/// </summary>
internal static class LinearRegressionModel
{
    public static void ValidateTrainingData<T>(T[,]? x, T[]? y)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (x == null || y == null)
            throw new ArgumentException("输入不能为 null。");
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || y.Length == 0)
            throw new ArgumentException("训练数据不能为空。");
        if (rows != y.Length)
            throw new ArgumentException("样本数与标签数不匹配。");
    }

    public static void ApplyDesignSolution<T>(
        T[] solution,
        int featureCount,
        out T intercept,
        out T[] coefficients)
        where T : struct, IFloatingPointIeee754<T>
    {
        intercept = solution[0];
        coefficients = new T[featureCount];
        Array.Copy(solution, 1, coefficients, 0, featureCount);
    }

    public static T PredictAffine<T>(T intercept, ReadOnlySpan<T> coefficients, T[] x)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (x == null || x.Length != coefficients.Length)
            throw new ArgumentException("特征维度与模型不匹配。", nameof(x));

        return intercept + NumericKernels.Dot(coefficients, x);
    }

    public static void PredictAffineBatch<T>(
        T intercept,
        ReadOnlySpan<T> coefficients,
        T[,] x,
        Span<T> predictions)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (cols != coefficients.Length)
            throw new ArgumentException("特征维度与模型不匹配。");
        if (predictions.Length < rows)
            throw new ArgumentException("predictions 长度不足。", nameof(predictions));

        for (int i = 0; i < rows; i++)
            predictions[i] = intercept + NumericKernels.DotRow(x, i, coefficients);
    }
}
