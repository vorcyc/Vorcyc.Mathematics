namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Residual block: Conv→BN→ReLU→Conv→BN + skip connection → ReLU.
/// </summary>
public sealed class BatchResidualBlockLayer<T> : BatchLayerBase<T>, IBatchCompositeLayer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly BatchConvolution2DLayer<T> _conv1;
    private readonly BatchBatchNormLayer<T> _bn1;
    private readonly BatchReLUActivation<T> _relu1;
    private readonly BatchConvolution2DLayer<T> _conv2;
    private readonly BatchBatchNormLayer<T> _bn2;
    private readonly BatchReLUActivation<T> _reluOut;
    private readonly BatchConvolution2DLayer<T>? _projection;
    private BatchTensor<T>? _skipInput;

    public BatchResidualBlockLayer(int inputChannels, int outputChannels, int stride = 1, int kernelSize = 3, string? name = null)
        : base(name)
    {
        InputChannels = inputChannels;
        OutputChannels = outputChannels;
        Stride = stride;
        KernelSize = kernelSize;

        _conv1 = new BatchConvolution2DLayer<T>(inputChannels, outputChannels, kernelSize, stride, name: $"{name}.conv1");
        _bn1 = new BatchBatchNormLayer<T>(outputChannels, $"{name}.bn1");
        _relu1 = new BatchReLUActivation<T>($"{name}.relu1");
        _conv2 = new BatchConvolution2DLayer<T>(outputChannels, outputChannels, kernelSize, name: $"{name}.conv2");
        _bn2 = new BatchBatchNormLayer<T>(outputChannels, $"{name}.bn2");
        _reluOut = new BatchReLUActivation<T>($"{name}.relu_out");

        if (inputChannels != outputChannels || stride != 1)
        {
            _projection = new BatchConvolution2DLayer<T>(inputChannels, outputChannels, kernelSize: 1, stride, name: $"{name}.proj");
        }
    }

    public int InputChannels { get; }
    public int OutputChannels { get; }
    public int Stride { get; }
    public int KernelSize { get; }

    /// <summary>
    /// Child layers in execution order. Exposes the internal BatchNorm layers so the
    /// serializer can persist their running statistics (Conv/ReLU carry no extra state).
    /// </summary>
    public IReadOnlyList<IBatchLayer<T>> Children
    {
        get
        {
            var list = new List<IBatchLayer<T>>
            {
                _conv1, _bn1, _relu1, _conv2, _bn2, _reluOut,
            };
            if (_projection is not null)
            {
                list.Add(_projection);
            }

            return list;
        }
    }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters
    {
        get
        {
            var list = new List<Parameter<T>>();
            list.AddRange(_conv1.Parameters);
            list.AddRange(_bn1.Parameters);
            list.AddRange(_conv2.Parameters);
            list.AddRange(_bn2.Parameters);
            if (_projection is not null)
            {
                list.AddRange(_projection.Parameters);
            }

            return list;
        }
    }

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
        => _conv2.GetOutputShape(_bn1.GetOutputShape(_conv1.GetOutputShape(inputShape)));

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        _skipInput = input;
        var main = _relu1.Forward(_bn1.Forward(_conv1.Forward(input, training), training), training);
        var residual = _bn2.Forward(_conv2.Forward(main, training), training);
        var skip = _projection?.Forward(input, training) ?? input;

        var output = new BatchTensor<T>(residual.Batch, residual.Height, residual.Width, residual.Channels);
        for (int i = 0; i < output.Values.Length; i++)
        {
            output.Values[i] = residual.Values[i] + skip.Values[i];
        }

        output = _reluOut.Forward(output, training);
        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        var gradAfterRelu = _reluOut.Backward(gradOutput);
        var gradResidual = new BatchTensor<T>(gradAfterRelu.Batch, gradAfterRelu.Height, gradAfterRelu.Width, gradAfterRelu.Channels);
        var gradSkip = new BatchTensor<T>(gradAfterRelu.Batch, gradAfterRelu.Height, gradAfterRelu.Width, gradAfterRelu.Channels);
        for (int i = 0; i < gradAfterRelu.Values.Length; i++)
        {
            gradResidual.Values[i] = gradAfterRelu.Values[i];
            gradSkip.Values[i] = gradAfterRelu.Values[i];
        }

        var gradBn2 = _bn2.Backward(gradResidual);
        var gradConv2 = _conv2.Backward(gradBn2);
        var gradRelu1 = _relu1.Backward(gradConv2);
        var gradBn1 = _bn1.Backward(gradRelu1);
        var gradConv1 = _conv1.Backward(gradBn1);
        var gradProj = _projection?.Backward(gradSkip) ?? gradSkip;
        var gradInput = new BatchTensor<T>(_skipInput!.Batch, _skipInput.Height, _skipInput.Width, _skipInput.Channels);
        for (int i = 0; i < gradInput.Values.Length; i++)
        {
            gradInput.Values[i] = gradConv1.Values[i] + gradProj.Values[i];
        }

        return gradInput;
    }
}
