namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Depthwise 2-D convolution: each input channel is filtered independently.
/// </summary>
public sealed class BatchDepthwiseConvolution2DLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Parameter<T>[] _filters;
    private readonly Parameter<T> _bias;

    public BatchDepthwiseConvolution2DLayer(int channels, int kernelSize, int stride = 1, int dilation = 1, string? name = null)
        : base(name)
    {
        if (channels <= 0 || kernelSize <= 0 || stride <= 0 || dilation <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channels), "All dimensions must be positive.");
        }

        Channels = channels;
        KernelSize = kernelSize;
        Stride = stride;
        Dilation = dilation;

        _filters = new Parameter<T>[channels];
        for (int i = 0; i < channels; i++)
        {
            var filter = new Parameter<T>(new Tensor<T>(kernelSize, kernelSize, 1), $"{name}.filter.{i}");
            TensorUtilities.FillUniformRandom(filter.Value, T.CreateTruncating(0.1));
            _filters[i] = filter;
        }

        _bias = new Parameter<T>(new Tensor<T>(1, 1, channels), $"{name}.bias");
        _bias.Value.Fill(T.Zero);
    }

    public int Channels { get; }
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
        if (inputShape.Channels != Channels)
        {
            throw new ArgumentException($"Expected {Channels} channels, got {inputShape.Channels}.");
        }

        return new BatchShape(
            inputShape.Batch,
            inputShape.Height / Stride,
            inputShape.Width / Stride,
            Channels);
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var outShape = GetOutputShape(input.Shape);
        var output = new BatchTensor<T>(outShape.Batch, outShape.Height, outShape.Width, outShape.Channels);
        var pad = (KernelSize * Dilation + Dilation - 1) / 2;

        long workPerSample = (long)outShape.Height * outShape.Width * Channels * KernelSize * KernelSize;
        ComputingContextExecution.ForEach(null, 0, input.Batch, n =>
        {
            for (int c = 0; c < Channels; c++)
            {
                var filter = _filters[c].Value.Values;
                var bias = _bias.Value[0, 0, c];
                for (int ay = 0; ay < outShape.Height; ay++)
                {
                    var y = ay * Stride - pad;
                    for (int ax = 0; ax < outShape.Width; ax++)
                    {
                        var x = ax * Stride - pad;
                        T acc = T.Zero;
                        for (int fy = 0; fy < KernelSize; fy++)
                        {
                            var oy = y + fy * Dilation + Dilation - 1;
                            if (oy < 0 || oy >= input.Height)
                            {
                                continue;
                            }

                            for (int fx = 0; fx < KernelSize; fx++)
                            {
                                var ox = x + fx * Dilation + Dilation - 1;
                                if (ox < 0 || ox >= input.Width)
                                {
                                    continue;
                                }

                                var fi = (fy * KernelSize) + fx;
                                acc += filter[fi] * input[n, oy, ox, c];
                            }
                        }

                        output[n, ay, ax, c] = acc + bias;
                    }
                }
            }
        }, workPerSample);

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
            for (int c = 0; c < Channels; c++)
            {
                var filter = _filters[c].Value.Values;
                var filterGrad = _filters[c].Gradient.Values;
                for (int ay = 0; ay < outShape.Height; ay++)
                {
                    var y = ay * Stride - pad;
                    for (int ax = 0; ax < outShape.Width; ax++)
                    {
                        var x = ax * Stride - pad;
                        var gradOut = gradOutput[n, ay, ax, c];
                        _bias.Gradient[0, 0, c] += gradOut;

                        for (int fy = 0; fy < KernelSize; fy++)
                        {
                            var oy = y + fy * Dilation + Dilation - 1;
                            if (oy < 0 || oy >= input.Height)
                            {
                                continue;
                            }

                            for (int fx = 0; fx < KernelSize; fx++)
                            {
                                var ox = x + fx * Dilation + Dilation - 1;
                                if (ox < 0 || ox >= input.Width)
                                {
                                    continue;
                                }

                                var fi = (fy * KernelSize) + fx;
                                var inVal = input[n, oy, ox, c];
                                filterGrad[fi] += gradOut * inVal;
                                gradInput[n, oy, ox, c] += gradOut * filter[fi];
                            }
                        }
                    }
                }
            }
        }

        return gradInput;
    }
}
