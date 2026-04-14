namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Concatenates a secondary NHWC tensor along the channel axis.
/// Set <see cref="Secondary"/> immediately before <see cref="Forward"/>.
/// </summary>
public sealed class BatchConcatenateLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private BatchTensor<T>? _secondary;

    public BatchConcatenateLayer(string? name = null) : base(name) { }

    /// <summary>Gets or sets the secondary tensor to concatenate with the primary input.</summary>
    public BatchTensor<T>? Secondary
    {
        get => _secondary;
        set => _secondary = value;
    }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
    {
        if (_secondary is null)
        {
            throw new InvalidOperationException("Secondary tensor must be assigned before querying output shape.");
        }

        if (inputShape.Batch != _secondary.Batch
            || inputShape.Height != _secondary.Height
            || inputShape.Width != _secondary.Width)
        {
            throw new ArgumentException("Primary and secondary tensors must share N, H, and W.");
        }

        return new BatchShape(inputShape.Batch, inputShape.Height, inputShape.Width, inputShape.Channels + _secondary.Channels);
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        if (_secondary is null)
        {
            throw new InvalidOperationException("Secondary tensor must be assigned before Forward.");
        }

        var output = BatchTensorUtilities.ConcatChannels(input, _secondary);
        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        if (_secondary is null)
        {
            throw new InvalidOperationException("Secondary tensor required for backward.");
        }

        var input = CachedInput!;
        int leftChannels = input.Channels;
        int rightChannels = _secondary.Channels;
        int plane = input.Height * input.Width;
        var gradPrimary = new BatchTensor<T>(input.Batch, input.Height, input.Width, leftChannels);
        var gradSecondary = new BatchTensor<T>(_secondary.Batch, _secondary.Height, _secondary.Width, rightChannels);

        for (int n = 0; n < input.Batch; n++)
        {
            int outOffset = n * plane * (leftChannels + rightChannels);
            gradOutput.Values.Slice(outOffset, plane * leftChannels)
                .CopyTo(gradPrimary.Values.Slice(n * plane * leftChannels, plane * leftChannels));
            gradOutput.Values.Slice(outOffset + plane * leftChannels, plane * rightChannels)
                .CopyTo(gradSecondary.Values.Slice(n * plane * rightChannels, plane * rightChannels));
        }

        _secondaryGrad = gradSecondary;
        return gradPrimary;
    }

    private BatchTensor<T>? _secondaryGrad;

    /// <summary>Gets gradients for the secondary branch after <see cref="Backward"/>.</summary>
    public BatchTensor<T>? SecondaryGradient => _secondaryGrad;
}
