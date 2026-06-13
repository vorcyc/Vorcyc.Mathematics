namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Batched 2-D convolution on NHWC tensors with cross-sample parallelism and SIMD channel MAC.
/// </summary>
public sealed class BatchConvolution2DLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Convolution2DLayer<T> _inner;
    private bool _usedIm2Col;

    public BatchConvolution2DLayer(int inputChannels, int outputChannels, int kernelSize, int stride = 1, int dilation = 1, string? name = null)
        : base(name)
    {
        _inner = new Convolution2DLayer<T>(inputChannels, outputChannels, kernelSize, stride, dilation, name);
    }

    /// <summary>Gets the underlying single-sample convolution layer (shared weights).</summary>
    public Convolution2DLayer<T> InnerLayer => _inner;

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => _inner.Parameters;

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
    {
        var sampleShape = new TensorShape(inputShape.Width, inputShape.Height, inputShape.Channels);
        var outShape = _inner.GetOutputShape(sampleShape);
        return new BatchShape(inputShape.Batch, outShape.Height, outShape.Width, outShape.Depth);
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var outShape = GetOutputShape(input.Shape);
        var output = new BatchTensor<T>(outShape.Batch, outShape.Height, outShape.Width, outShape.Channels);
        var filters = _inner.FilterParameters;
        var bias = _inner.BiasParameter.Value.Values;
        _usedIm2Col = _inner.KernelSize >= BatchConv2DIm2Col.KernelThreshold;

        if (_usedIm2Col)
        {
            BatchConv2DIm2Col.Forward(
                input.Buffer,
                input.Batch,
                input.Height,
                input.Width,
                input.Channels,
                filters,
                bias,
                _inner.KernelSize,
                _inner.Stride,
                _inner.Dilation,
                output.Buffer,
                outShape.Height,
                outShape.Width,
                outShape.Channels);
        }
        else
        {
            BatchConv2DMath.Forward(
                input.Buffer,
                input.Batch,
                input.Height,
                input.Width,
                input.Channels,
                filters,
                bias,
                _inner.KernelSize,
                _inner.Stride,
                _inner.Dilation,
                output.Buffer,
                outShape.Height,
                outShape.Width,
                outShape.Channels);
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        var input = CachedInput!;
        var outShape = CachedOutput!.Shape;
        var gradInput = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);

        if (_usedIm2Col)
        {
            BatchConv2DIm2Col.Backward(
                input.Buffer,
                gradOutput.Buffer,
                input.Batch,
                input.Height,
                input.Width,
                input.Channels,
                _inner.FilterParameters,
                _inner.BiasParameter,
                _inner.KernelSize,
                _inner.Stride,
                _inner.Dilation,
                gradInput.Buffer,
                outShape.Height,
                outShape.Width,
                outShape.Channels);
        }
        else
        {
            BatchConv2DMath.Backward(
                input.Buffer,
                gradOutput.Buffer,
                input.Batch,
                input.Height,
                input.Width,
                input.Channels,
                _inner.FilterParameterArray,
                _inner.BiasParameter,
                _inner.KernelSize,
                _inner.Stride,
                _inner.Dilation,
                gradInput.Buffer,
                outShape.Height,
                outShape.Width,
                outShape.Channels);
        }

        return gradInput;
    }
}
