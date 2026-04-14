namespace Vorcyc.Mathematics.DeepLearning.Losses;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Categorical cross-entropy loss applied to raw logits.
/// Internally applies numerically stable log-softmax.
/// </summary>
/// <remarks>
/// <paramref name="target"/> must be a one-hot vector (or probability distribution summing to 1).
/// Do not place a <see cref="Modules.SoftmaxLayer{T}"/> before this loss during training.
/// </remarks>
/// <typeparam name="T">Element type.</typeparam>
public sealed class CategoricalCrossEntropyLoss<T> : ILoss<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private Tensor<T>? _lastLogProbabilities;

    /// <inheritdoc/>
    public T Compute(Tensor<T> prediction, Tensor<T> target)
    {
        ValidateShapes(prediction, target);
        _lastLogProbabilities = ComputeLogSoftmax(prediction);

        T loss = T.Zero;
        var logProb = _lastLogProbabilities.Values;
        var targetSpan = target.Values;
        for (int i = 0; i < logProb.Length; i++)
        {
            loss -= targetSpan[i] * logProb[i];
        }

        return loss;
    }

    /// <inheritdoc/>
    public Tensor<T> Backward(Tensor<T> prediction, Tensor<T> target)
    {
        ValidateShapes(prediction, target);
        var logProb = _lastLogProbabilities ?? ComputeLogSoftmax(prediction);
        var grad = new Tensor<T>(prediction.Width, prediction.Height, prediction.Depth);
        var gradSpan = grad.Values;
        var logSpan = logProb.Values;
        var targetSpan = target.Values;

        for (int i = 0; i < gradSpan.Length; i++)
        {
            gradSpan[i] = T.Exp(logSpan[i]) - targetSpan[i];
        }

        return grad;
    }

    private static Tensor<T> ComputeLogSoftmax(Tensor<T> logits)
    {
        var result = new Tensor<T>(logits.Width, logits.Height, logits.Depth);
        for (int z = 0; z < logits.Depth; z++)
        {
            for (int y = 0; y < logits.Height; y++)
            {
                for (int x = 0; x < logits.Width; x++)
                {
                    T max = logits[x, y, 0];
                    for (int d = 1; d < logits.Depth; d++)
                    {
                        var v = logits[x, y, d];
                        if (v > max)
                        {
                            max = v;
                        }
                    }

                    T sum = T.Zero;
                    for (int d = 0; d < logits.Depth; d++)
                    {
                        sum += T.Exp(logits[x, y, d] - max);
                    }

                    var logSum = max + T.Log(sum);
                    for (int d = 0; d < logits.Depth; d++)
                    {
                        result[x, y, d] = logits[x, y, d] - logSum;
                    }
                }
            }
        }

        return result;
    }

    private static void ValidateShapes(Tensor<T> prediction, Tensor<T> target)
    {
        if (prediction.Width != target.Width
            || prediction.Height != target.Height
            || prediction.Depth != target.Depth)
        {
            throw new ArgumentException("Prediction and target tensors must have the same shape.");
        }
    }
}
