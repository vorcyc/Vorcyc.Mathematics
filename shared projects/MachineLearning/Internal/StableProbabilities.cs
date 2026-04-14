using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Internal;

/// <summary>
/// 数值稳定的概率归一化。
/// </summary>
internal static class StableProbabilities
{
    /// <summary>
    /// 将 logits 原地转换为 softmax 概率（log-sum-exp 稳定化）。
    /// </summary>
    public static void Softmax<T>(Span<T> values)
        where T : struct, IFloatingPointIeee754<T>
    {
        T max = values[0];
        for (int i = 1; i < values.Length; i++)
            max = T.Max(max, values[i]);

        T sum = T.Zero;
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = T.Exp(values[i] - max);
            sum += values[i];
        }

        for (int i = 0; i < values.Length; i++)
            values[i] /= sum;
    }

    /// <summary>
    /// 将 logits 写入 probabilities（两者可不同缓冲区）。
    /// </summary>
    public static void Softmax<T>(ReadOnlySpan<T> logits, Span<T> probabilities)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (logits.Length != probabilities.Length)
            throw new ArgumentException("logits 与 probabilities 长度必须相同。");

        T max = logits[0];
        for (int i = 1; i < logits.Length; i++)
            max = T.Max(max, logits[i]);

        T sum = T.Zero;
        for (int i = 0; i < logits.Length; i++)
        {
            probabilities[i] = T.Exp(logits[i] - max);
            sum += probabilities[i];
        }

        for (int i = 0; i < probabilities.Length; i++)
            probabilities[i] /= sum;
    }
}
