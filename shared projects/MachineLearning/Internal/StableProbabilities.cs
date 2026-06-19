using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Internal;

/// <summary>
/// Numerically stable probability normalization.
/// </summary>
internal static class StableProbabilities
{
    /// <summary>
    /// Converts logits to softmax probabilities in place (log-sum-exp stabilization).
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
    /// Writes the softmax of logits into probabilities (the two may be different buffers).
    /// </summary>
    public static void Softmax<T>(ReadOnlySpan<T> logits, Span<T> probabilities)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (logits.Length != probabilities.Length)
            throw new ArgumentException("logits and probabilities must have the same length.");

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
