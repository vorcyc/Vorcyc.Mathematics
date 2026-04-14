using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;

namespace Vorcyc.Mathematics.DeepLearning.Integration.Frontends;

/// <summary>
/// Differentiable pre-emphasis on waveform batches (N×1×1×L).
/// </summary>
public sealed class BatchPreEmphasisLayer : BatchLayerBase<float>
{
    private readonly float _b0;
    private readonly float _b1;

    /// <summary>
    /// Creates a pre-emphasis layer with coefficients [1, -<paramref name="coefficient"/>].
    /// </summary>
    public BatchPreEmphasisLayer(float coefficient = 0.97f, string? name = null) : base(name)
    {
        _b0 = 1f;
        _b1 = -coefficient;
    }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<float>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
    {
        if (inputShape.Height != 1 || inputShape.Width != 1)
        {
            throw new ArgumentException("Expected waveform layout N×1×1×L.");
        }

        return inputShape;
    }

    /// <inheritdoc/>
    public override BatchTensor<float> Forward(BatchTensor<float> input, bool training = true)
    {
        FrontendTensorOps.RequireWaveformLayout(input);
        var output = new BatchTensor<float>(input.Batch, 1, 1, input.Channels);

        ComputingContextExecution.ForEach(null, 0, input.Batch, n =>
        {
            var length = input.Channels;
            output[n, 0, 0, 0] = _b0 * input[n, 0, 0, 0];
            for (var i = 1; i < length; i++)
            {
                output[n, 0, 0, i] = _b0 * input[n, 0, 0, i] + _b1 * input[n, 0, 0, i - 1];
            }
        }, input.Channels);

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<float> Backward(BatchTensor<float> gradOutput)
    {
        EnsureCached();
        var input = CachedInput!;
        var gradInput = new BatchTensor<float>(input.Batch, 1, 1, input.Channels);

        for (var n = 0; n < input.Batch; n++)
        {
            var length = input.Channels;
            gradInput[n, 0, 0, 0] = _b0 * gradOutput[n, 0, 0, 0];
            for (var i = 1; i < length; i++)
            {
                gradInput[n, 0, 0, i] += _b0 * gradOutput[n, 0, 0, i];
                gradInput[n, 0, 0, i - 1] += _b1 * gradOutput[n, 0, 0, i];
            }
        }

        return gradInput;
    }
}
