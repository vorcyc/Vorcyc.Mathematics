namespace Vorcyc.Mathematics.DeepLearning.Losses;

using System.Numerics;

/// <summary>
/// Categorical cross-entropy on NHWC logits (stable log-softmax internally).
/// Supports one-hot targets and integer class indices.
/// </summary>
/// <remarks>
/// Do not place <see cref="Modules.BatchSoftmaxLayer{T}"/> before this loss during training.
/// </remarks>
public sealed class BatchCategoricalCrossEntropyLoss<T> : ISparseBatchLoss<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private BatchTensor<T>? _lastLogProbabilities;
    private int[]? _lastClassIndices;
    private bool _lastSparse;

    /// <inheritdoc/>
    public T Compute(BatchTensor<T> prediction, BatchTensor<T> target)
    {
        ValidateDenseShapes(prediction, target);
        _lastSparse = false;
        _lastClassIndices = null;
        _lastLogProbabilities = ComputeLogSoftmax(prediction);

        T loss = T.Zero;
        var logProb = _lastLogProbabilities.Values;
        var targetSpan = target.Values;
        for (int i = 0; i < logProb.Length; i++)
        {
            loss -= targetSpan[i] * logProb[i];
        }

        return loss / T.CreateTruncating(logProb.Length);
    }

    /// <summary>Computes loss from per-sample class indices (N×1×1 layout).</summary>
    public T ComputeFromClassIndices(BatchTensor<T> prediction, ReadOnlySpan<int> classIndices)
    {
        ValidateSparse(prediction, classIndices);
        _lastSparse = true;
        _lastClassIndices = classIndices.ToArray();
        _lastLogProbabilities = ComputeLogSoftmax(prediction);

        T loss = T.Zero;
        int spatial = prediction.Height * prediction.Width;
        for (int n = 0; n < prediction.Batch; n++)
        {
            int classIndex = classIndices[n];
            int baseIndex = ((n * prediction.Height) * prediction.Width) * prediction.Channels;
            if (spatial > 1)
            {
                for (int s = 0; s < spatial; s++)
                {
                    int h = s / prediction.Width;
                    int w = s % prediction.Width;
                    baseIndex = ((n * prediction.Height + h) * prediction.Width + w) * prediction.Channels;
                    loss -= _lastLogProbabilities.Values[baseIndex + classIndex];
                }
            }
            else
            {
                loss -= _lastLogProbabilities.Values[baseIndex + classIndex];
            }
        }

        return loss / T.CreateTruncating(prediction.Batch * Math.Max(1, spatial));
    }

    /// <inheritdoc/>
    public BatchTensor<T> Backward(BatchTensor<T> prediction, BatchTensor<T> target)
    {
        ValidateDenseShapes(prediction, target);
        _lastSparse = false;
        var logProb = _lastLogProbabilities ?? ComputeLogSoftmax(prediction);
        return DenseBackward(prediction, logProb, target.Values);
    }

    /// <summary>Backpropagates using integer class indices.</summary>
    public BatchTensor<T> BackwardFromClassIndices(BatchTensor<T> prediction, ReadOnlySpan<int> classIndices)
    {
        ValidateSparse(prediction, classIndices);
        var logProb = _lastLogProbabilities ?? ComputeLogSoftmax(prediction);
        var grad = new BatchTensor<T>(prediction.Batch, prediction.Height, prediction.Width, prediction.Channels);
        grad.Values.Clear();

        int spatial = prediction.Height * prediction.Width;
        var scale = T.One / T.CreateTruncating(prediction.Batch * Math.Max(1, spatial));
        for (int n = 0; n < prediction.Batch; n++)
        {
            int classIndex = classIndices[n];
            for (int s = 0; s < spatial; s++)
            {
                int h = s / prediction.Width;
                int w = s % prediction.Width;
                int baseIndex = ((n * prediction.Height + h) * prediction.Width + w) * prediction.Channels;
                for (int c = 0; c < prediction.Channels; c++)
                {
                    var prob = T.Exp(logProb.Values[baseIndex + c]);
                    var targetVal = c == classIndex ? T.One : T.Zero;
                    grad.Values[baseIndex + c] = (prob - targetVal) * scale;
                }
            }
        }

        return grad;
    }

    private BatchTensor<T> DenseBackward(BatchTensor<T> prediction, BatchTensor<T> logProb, ReadOnlySpan<T> targetSpan)
    {
        var grad = new BatchTensor<T>(prediction.Batch, prediction.Height, prediction.Width, prediction.Channels);
        var gradSpan = grad.Values;
        var logSpan = logProb.Values;
        var scale = T.One / T.CreateTruncating(gradSpan.Length);

        for (int i = 0; i < gradSpan.Length; i++)
        {
            gradSpan[i] = (T.Exp(logSpan[i]) - targetSpan[i]) * scale;
        }

        return grad;
    }

    private static BatchTensor<T> ComputeLogSoftmax(BatchTensor<T> logits)
    {
        var result = new BatchTensor<T>(logits.Batch, logits.Height, logits.Width, logits.Channels);
        int spatial = logits.Height * logits.Width;

        for (int n = 0; n < logits.Batch; n++)
        {
            for (int s = 0; s < spatial; s++)
            {
                int h = s / logits.Width;
                int w = s % logits.Width;
                int baseIndex = ((n * logits.Height + h) * logits.Width + w) * logits.Channels;

                T max = logits.Values[baseIndex];
                for (int c = 1; c < logits.Channels; c++)
                {
                    max = T.Max(max, logits.Values[baseIndex + c]);
                }

                T sum = T.Zero;
                for (int c = 0; c < logits.Channels; c++)
                {
                    sum += T.Exp(logits.Values[baseIndex + c] - max);
                }

                var logSum = max + T.Log(sum);
                for (int c = 0; c < logits.Channels; c++)
                {
                    result.Values[baseIndex + c] = logits.Values[baseIndex + c] - logSum;
                }
            }
        }

        return result;
    }

    private static void ValidateDenseShapes(BatchTensor<T> prediction, BatchTensor<T> target)
    {
        if (prediction.Batch != target.Batch
            || prediction.Height != target.Height
            || prediction.Width != target.Width
            || prediction.Channels != target.Channels)
        {
            throw new ArgumentException("Prediction and target batch tensors must have the same shape.");
        }
    }

    private static void ValidateSparse(BatchTensor<T> prediction, ReadOnlySpan<int> classIndices)
    {
        if (classIndices.Length != prediction.Batch)
        {
            throw new ArgumentException("Class index count must match batch size.", nameof(classIndices));
        }

        for (int i = 0; i < classIndices.Length; i++)
        {
            if (classIndices[i] < 0 || classIndices[i] >= prediction.Channels)
            {
                throw new ArgumentOutOfRangeException(nameof(classIndices), "Class index out of range.");
            }
        }
    }
}
