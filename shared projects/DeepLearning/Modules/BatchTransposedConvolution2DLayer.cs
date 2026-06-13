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

        int inChannels = InputChannels, outChannels = OutputChannels;
        int kernelSize = KernelSize, stride = Stride, dilation = Dilation;
        int inHeight = input.Height, inWidth = input.Width;
        int outHeight = outShape.Height, outWidth = outShape.Width;

        // Parallel over samples: each sample writes only its own output region, so the
        // scatter-add is race-free across n (accumulation within one n stays serial).
        long workPerSample = (long)inChannels * inHeight * inWidth * outChannels * kernelSize * kernelSize;
        ComputingContextExecution.ForEach(null, 0, input.Batch, n =>
        {
            for (int ic = 0; ic < inChannels; ic++)
            {
                for (int ay = 0; ay < inHeight; ay++)
                {
                    for (int ax = 0; ax < inWidth; ax++)
                    {
                        var inVal = input[n, ay, ax, ic];
                        var y = ay * stride - pad;
                        var x = ax * stride - pad;

                        for (int d = 0; d < outChannels; d++)
                        {
                            var filter = _filters[d].Value.Values;
                            for (int fy = 0; fy < kernelSize; fy++)
                            {
                                var oy = y + fy * dilation + dilation - 1;
                                if (oy < 0 || oy >= outHeight)
                                {
                                    continue;
                                }

                                for (int fx = 0; fx < kernelSize; fx++)
                                {
                                    var ox = x + fx * dilation + dilation - 1;
                                    if (ox < 0 || ox >= outWidth)
                                    {
                                        continue;
                                    }

                                    var fi = ((kernelSize * fy) + fx) * inChannels + ic;
                                    output[n, oy, ox, d] += inVal * filter[fi];
                                }
                            }
                        }
                    }
                }
            }

            for (int d = 0; d < outChannels; d++)
            {
                var bias = _bias.Value[0, 0, d];
                for (int oy = 0; oy < outHeight; oy++)
                {
                    for (int ox = 0; ox < outWidth; ox++)
                    {
                        output[n, oy, ox, d] += bias;
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

        int inChannels = InputChannels, outChannels = OutputChannels;
        int kernelSize = KernelSize, stride = Stride, dilation = Dilation;
        int inHeight = input.Height, inWidth = input.Width;
        int outHeight = outShape.Height, outWidth = outShape.Width;
        int batch = input.Batch;

        // Two race-free parallel kernels (same approach as BatchConv2DMath.Backward):
        //   (1) gradInput — parallel over samples n (disjoint input regions).
        //   (2) weight/bias grads — parallel over output channels d (disjoint filter/bias rows).
        long gradInputWork = (long)inChannels * inHeight * inWidth * outChannels * kernelSize * kernelSize;
        ComputingContextExecution.ForEach(null, 0, batch, n =>
        {
            for (int d = 0; d < outChannels; d++)
            {
                var filter = _filters[d].Value.Values;
                for (int ic = 0; ic < inChannels; ic++)
                {
                    for (int ay = 0; ay < inHeight; ay++)
                    {
                        for (int ax = 0; ax < inWidth; ax++)
                        {
                            var y = ay * stride - pad;
                            var x = ax * stride - pad;
                            T gradInAcc = T.Zero;

                            for (int fy = 0; fy < kernelSize; fy++)
                            {
                                var oy = y + fy * dilation + dilation - 1;
                                if (oy < 0 || oy >= outHeight)
                                {
                                    continue;
                                }

                                for (int fx = 0; fx < kernelSize; fx++)
                                {
                                    var ox = x + fx * dilation + dilation - 1;
                                    if (ox < 0 || ox >= outWidth)
                                    {
                                        continue;
                                    }

                                    var fi = ((kernelSize * fy) + fx) * inChannels + ic;
                                    gradInAcc += gradOutput[n, oy, ox, d] * filter[fi];
                                }
                            }

                            gradInput[n, ay, ax, ic] += gradInAcc;
                        }
                    }
                }
            }
        }, gradInputWork);

        long weightWork = (long)batch * inChannels * inHeight * inWidth * kernelSize * kernelSize;
        ComputingContextExecution.ForEach(null, 0, outChannels, d =>
        {
            var filter = _filters[d].Value.Values;
            var filterGrad = _filters[d].Gradient.Values;
            T biasAcc = T.Zero;

            for (int n = 0; n < batch; n++)
            {
                for (int ic = 0; ic < inChannels; ic++)
                {
                    for (int ay = 0; ay < inHeight; ay++)
                    {
                        for (int ax = 0; ax < inWidth; ax++)
                        {
                            var inVal = input[n, ay, ax, ic];
                            var y = ay * stride - pad;
                            var x = ax * stride - pad;

                            for (int fy = 0; fy < kernelSize; fy++)
                            {
                                var oy = y + fy * dilation + dilation - 1;
                                if (oy < 0 || oy >= outHeight)
                                {
                                    continue;
                                }

                                for (int fx = 0; fx < kernelSize; fx++)
                                {
                                    var ox = x + fx * dilation + dilation - 1;
                                    if (ox < 0 || ox >= outWidth)
                                    {
                                        continue;
                                    }

                                    var fi = ((kernelSize * fy) + fx) * inChannels + ic;
                                    filterGrad[fi] += gradOutput[n, oy, ox, d] * inVal;
                                }
                            }
                        }
                    }
                }

                for (int oy = 0; oy < outHeight; oy++)
                {
                    for (int ox = 0; ox < outWidth; ox++)
                    {
                        biasAcc += gradOutput[n, oy, ox, d];
                    }
                }
            }

            _bias.Gradient[0, 0, d] += biasAcc;
        }, weightWork);

        return gradInput;
    }
}
