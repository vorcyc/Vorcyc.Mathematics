namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Batched 2×2 max pooling with stride 2 on NHWC tensors.
/// </summary>
public sealed class BatchMaxPool2DLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
{
    private int[]? _argmaxIndices;

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
        => new(inputShape.Batch, inputShape.Height / 2, inputShape.Width / 2, inputShape.Channels);

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var outShape = GetOutputShape(input.Shape);
        var output = new BatchTensor<T>(outShape.Batch, outShape.Height, outShape.Width, outShape.Channels);
        int argmaxLength = outShape.Batch * outShape.Height * outShape.Width * outShape.Channels;
        _argmaxIndices = new int[argmaxLength];

        BatchMaxPool2DMath.Forward(
            input.Buffer,
            input.Batch,
            input.Height,
            input.Width,
            input.Channels,
            output.Buffer,
            outShape.Height,
            outShape.Width,
            _argmaxIndices);

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        if (_argmaxIndices is null)
        {
            throw new InvalidOperationException("Forward must be called before Backward.");
        }

        var input = CachedInput!;
        var gradInput = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);

        BatchMaxPool2DMath.Backward(
            gradOutput.Buffer,
            gradOutput.Batch,
            gradOutput.Height,
            gradOutput.Width,
            gradOutput.Channels,
            _argmaxIndices,
            gradInput.Buffer);

        return gradInput;
    }
}
