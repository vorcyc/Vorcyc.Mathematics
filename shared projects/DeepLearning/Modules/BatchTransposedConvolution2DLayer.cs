namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Transposed 2-D convolution (deconvolution) on NHWC batch tensors.
/// </summary>
public sealed class BatchTransposedConvolution2DLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Parameter<T>[] _filters;
    private readonly Parameter<T> _bias;

    public BatchTransposedConvolution2DLayer(
        int inputChannels,
        int outputChannels,
        int kernelSize,
        int stride = 1,
        int dilation = 1,
        string? name = null)
        : base(name)
    {
        if (inputChannels <= 0 || outputChannels <= 0 || kernelSize <= 0 || stride <= 0 || dilation <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(inputChannels), "All dimensions must be positive.");
        }

        InputChannels = inputChannels;
        OutputChannels = outputChannels;
        KernelSize = kernelSize;
        Stride = stride;
        Dilation = dilation;

        _filters = new Parameter<T>[outputChannels];
        for (int i = 0; i < outputChannels; i++)
        {
            var filter = new Parameter<T>(new Tensor<T>(kernelSize, kernelSize, inputChannels), $"{name}.filter.{i}");
            TensorUtilities.FillUniformRandom(filter.Value, T.CreateTruncating(0.1));
            _filters[i] = filter;
        }

        _bias = new Parameter<T>(new Tensor<T>(1, 1, outputChannels), $"{name}.bias");
        _bias.Value.Fill(T.Zero);
    }

    public int InputChannels { get; }
    public int OutputChannels { get; }
    public int KernelSize { get; }
    public int Stride { get; }
    public int Dilation { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters
    {
        get
        {
            var list = new List<Parameter<T>>(_filters.Length + 1);
            list.AddRange(_filters);
            list.Add(_bias);
            return list;
        }
    }

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
    {
        if (inputShape.Channels != InputChannels)
        {
            throw new ArgumentException($"Expected {InputChannels} input channels.");
        }

        int outH = (inputShape.Height - 1) * Stride + Dilation * (KernelSize - 1) + 1;
        int outW = (inputShape.Width - 1) * Stride + Dilation * (KernelSize - 1) + 1;
        return new BatchShape(inputShape.Batch, outH, outW, OutputChannels);
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var outShape = GetOutputShape(input.Shape);
        var output = new BatchTensor<T>(outShape.Batch, outShape.Height, outShape.Width, outShape.Channels);
        output.Values.Clear();
        var pad = (KernelSize * Dilation + Dilation - 1) / 2;

        for (int n = 0; n < input.Batch; n++)
        {
            for (int ic = 0; ic < InputChannels; ic++)
            {
                for (int ay = 0; ay < input.Height; ay++)
                {
                    for (int ax = 0; ax < input.Width; ax++)
                    {
                        var inVal = input[n, ay, ax, ic];
                        var y = ay * Stride - pad;
                        var x = ax * Stride - pad;

                        for (int d = 0; d < OutputChannels; d++)
                        {
                            var filter = _filters[d].Value.Values;
                            for (int fy = 0; fy < KernelSize; fy++)
                            {
                                var oy = y + fy * Dilation + Dilation - 1;
                                if (oy < 0 || oy >= outShape.Height)
                                {
                                    continue;
                                }

                                for (int fx = 0; fx < KernelSize; fx++)
                                {
                                    var ox = x + fx * Dilation + Dilation - 1;
                                    if (ox < 0 || ox >= outShape.Width)
                                    {
                                        continue;
                                    }

                                    var fi = ((KernelSize * fy) + fx) * InputChannels + ic;
                                    output[n, oy, ox, d] += inVal * filter[fi];
                                }
                            }
                        }
                    }
                }
            }

            for (int d = 0; d < OutputChannels; d++)
            {
                var bias = _bias.Value[0, 0, d];
                for (int oy = 0; oy < outShape.Height; oy++)
                {
                    for (int ox = 0; ox < outShape.Width; ox++)
                    {
                        output[n, oy, ox, d] += bias;
                    }
                }
            }
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
        gradInput.Values.Clear();
        var pad = (KernelSize * Dilation + Dilation - 1) / 2;

        for (int n = 0; n < input.Batch; n++)
        {
            for (int d = 0; d < OutputChannels; d++)
            {
                var filter = _filters[d].Value.Values;
                var filterGrad = _filters[d].Gradient.Values;
                for (int ic = 0; ic < InputChannels; ic++)
                {
                    for (int ay = 0; ay < input.Height; ay++)
                    {
                        for (int ax = 0; ax < input.Width; ax++)
                        {
                            var inVal = input[n, ay, ax, ic];
                            var y = ay * Stride - pad;
                            var x = ax * Stride - pad;
                            T gradInAcc = T.Zero;

                            for (int fy = 0; fy < KernelSize; fy++)
                            {
                                var oy = y + fy * Dilation + Dilation - 1;
                                if (oy < 0 || oy >= outShape.Height)
                                {
                                    continue;
                                }

                                for (int fx = 0; fx < KernelSize; fx++)
                                {
                                    var ox = x + fx * Dilation + Dilation - 1;
                                    if (ox < 0 || ox >= outShape.Width)
                                    {
                                        continue;
                                    }

                                    var fi = ((KernelSize * fy) + fx) * InputChannels + ic;
                                    var gradOut = gradOutput[n, oy, ox, d];
                                    filterGrad[fi] += gradOut * inVal;
                                    gradInAcc += gradOut * filter[fi];
                                }
                            }

                            gradInput[n, ay, ax, ic] += gradInAcc;
                        }
                    }
                }

                for (int oy = 0; oy < outShape.Height; oy++)
                {
                    for (int ox = 0; ox < outShape.Width; ox++)
                    {
                        _bias.Gradient[0, 0, d] += gradOutput[n, oy, ox, d];
                    }
                }
            }
        }

        return gradInput;
    }
}
