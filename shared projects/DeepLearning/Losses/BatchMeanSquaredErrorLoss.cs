namespace Vorcyc.Mathematics.DeepLearning.Losses;

using System.Numerics;

/// <summary>
/// Mean squared error on NHWC batch tensors: 0.5 · mean((prediction - target)²).
/// </summary>
public sealed class BatchMeanSquaredErrorLoss<T> : IBatchLoss<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <inheritdoc/>
    public T Compute(BatchTensor<T> prediction, BatchTensor<T> target)
    {
        ValidateShapes(prediction, target);
        var predSpan = prediction.Values;
        var targetSpan = target.Values;
        T sum = T.Zero;

        for (var i = 0; i < predSpan.Length; i++)
        {
            var diff = predSpan[i] - targetSpan[i];
            sum += diff * diff;
        }

        return sum / T.CreateTruncating(2.0 * predSpan.Length);
    }

    /// <inheritdoc/>
    public BatchTensor<T> Backward(BatchTensor<T> prediction, BatchTensor<T> target)
    {
        ValidateShapes(prediction, target);
        var grad = new BatchTensor<T>(prediction.Batch, prediction.Height, prediction.Width, prediction.Channels);
        var gradSpan = grad.Values;
        var predSpan = prediction.Values;
        var targetSpan = target.Values;
        var scale = T.One / T.CreateTruncating(predSpan.Length);

        for (var i = 0; i < gradSpan.Length; i++)
        {
            gradSpan[i] = (predSpan[i] - targetSpan[i]) * scale;
        }

        return grad;
    }

    private static void ValidateShapes(BatchTensor<T> prediction, BatchTensor<T> target)
    {
        if (prediction.Batch != target.Batch
            || prediction.Height != target.Height
            || prediction.Width != target.Width
            || prediction.Channels != target.Channels)
        {
            throw new ArgumentException("Prediction and target batches must have the same shape.");
        }
    }
}
