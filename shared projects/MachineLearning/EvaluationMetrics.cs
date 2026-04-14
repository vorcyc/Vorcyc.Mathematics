using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 常用机器学习评估指标。
/// </summary>
public static class EvaluationMetrics
{
    /// <summary>
    /// 计算均方误差 (MSE)。
    /// </summary>
    public static T MeanSquaredError<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> predicted)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("实际值与预测值长度必须相同。");
        if (actual.Length == 0)
            throw new ArgumentException("输入不能为空。");

        T sum = T.Zero;
        for (int i = 0; i < actual.Length; i++)
        {
            T diff = actual[i] - predicted[i];
            sum += diff * diff;
        }
        return sum / T.CreateChecked(actual.Length);
    }

    /// <summary>
    /// 计算均方根误差 (RMSE)。
    /// </summary>
    public static T RootMeanSquaredError<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> predicted)
        where T : struct, IFloatingPointIeee754<T>
        => T.Sqrt(MeanSquaredError(actual, predicted));

    /// <summary>
    /// 计算平均绝对误差 (MAE)。
    /// </summary>
    public static T MeanAbsoluteError<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> predicted)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("实际值与预测值长度必须相同。");
        if (actual.Length == 0)
            throw new ArgumentException("输入不能为空。");

        T sum = T.Zero;
        for (int i = 0; i < actual.Length; i++)
            sum += T.Abs(actual[i] - predicted[i]);
        return sum / T.CreateChecked(actual.Length);
    }

    /// <summary>
    /// 计算分类准确率。
    /// </summary>
    public static double Accuracy(ReadOnlySpan<string> actual, ReadOnlySpan<string> predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("实际标签与预测标签长度必须相同。");
        if (actual.Length == 0)
            throw new ArgumentException("输入不能为空。");

        int correct = 0;
        for (int i = 0; i < actual.Length; i++)
        {
            if (actual[i] == predicted[i])
                correct++;
        }
        return (double)correct / actual.Length;
    }

    /// <summary>
    /// 计算整数标签分类准确率。
    /// </summary>
    public static double Accuracy(ReadOnlySpan<int> actual, ReadOnlySpan<int> predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("实际标签与预测标签长度必须相同。");
        if (actual.Length == 0)
            throw new ArgumentException("输入不能为空。");

        int correct = 0;
        for (int i = 0; i < actual.Length; i++)
        {
            if (actual[i] == predicted[i])
                correct++;
        }
        return (double)correct / actual.Length;
    }
}
