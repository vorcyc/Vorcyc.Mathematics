namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// A trainable 2-D convolution layer compatible with the legacy <see cref="Layers.Layers.Conv2D"/> layout.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class Convolution2DLayer<T> : LayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Parameter<T>[] _filters;
    private readonly Parameter<T> _bias;

    /// <summary>
    /// Initializes a convolution layer with square kernels.
    /// </summary>
    public Convolution2DLayer(
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
            throw new ArgumentOutOfRangeException(nameof(inputChannels), "All convolution dimensions must be positive.");
        }

        InputChannels = inputChannels;
        OutputChannels = outputChannels;
        KernelSize = kernelSize;
        Stride = stride;
        Dilation = dilation;

        _filters = new Parameter<T>[outputChannels];
        for (int i = 0; i < outputChannels; i++)
        {
            var filter = new Parameter<T>(new Tensor<T>(kernelSize, kernelSize, inputChannels));
            TensorUtilities.FillUniformRandom(filter.Value, T.CreateTruncating(0.1));
            _filters[i] = filter;
        }

        _bias = new Parameter<T>(new Tensor<T>(1, 1, outputChannels));
        _bias.Value.Fill(T.Zero);
    }

    /// <summary>Gets the number of input channels.</summary>
    public int InputChannels { get; }

    /// <summary>Gets the number of output channels (filters).</summary>
    public int OutputChannels { get; }

    /// <summary>Gets the square kernel size.</summary>
    public int KernelSize { get; }

    /// <summary>Gets the stride.</summary>
    public int Stride { get; }

    /// <summary>Gets the dilation.</summary>
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
    public override TensorShape GetOutputShape(TensorShape inputShape)
    {
        ValidateInputShape(inputShape);
        return new TensorShape(inputShape.Width / Stride, inputShape.Height / Stride, OutputChannels);
    }

    /// <inheritdoc/>
    public override Tensor<T> Forward(Tensor<T> input, bool training = true)
    {
        ValidateInputShape(TensorShape.From(input));
        var filters = Array.ConvertAll(_filters, p => p.Value);
        var output = Layers.Layers.Conv2D(input, filters, _bias.Value, Stride, Dilation);
        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override Tensor<T> Backward(Tensor<T> gradOutput)
    {
        EnsureCached();
        var input = CachedInput!;
        var gradInput = new Tensor<T>(input.Width, input.Height, input.Depth);
        gradInput.Fill(T.Zero);

        var pad = (KernelSize * Dilation + Dilation - 1) / 2;
        var gradInSpan = gradInput.Values;

        for (int d = 0; d < OutputChannels; d++)
        {
            var filter = _filters[d].Value;
            var filterGrad = _filters[d].Gradient;

            for (int ay = 0; ay < gradOutput.Height; ay++)
            {
                var y = ay * Stride - pad;
                for (int ax = 0; ax < gradOutput.Width; ax++)
                {
                    var x = ax * Stride - pad;
                    var gradOut = gradOutput[ax, ay, d];
                    _bias.Gradient[0, 0, d] += gradOut;

                    for (int fy = 0; fy < KernelSize; fy++)
                    {
                        var oy = y + fy * Dilation + Dilation - 1;
                        for (int fx = 0; fx < KernelSize; fx++)
                        {
                            var ox = x + fx * Dilation + Dilation - 1;
                            if (oy < 0 || oy >= input.Height || ox < 0 || ox >= input.Width)
                            {
                                continue;
                            }

                            var fi = ((filter.Width * fy) + fx) * filter.Depth;
                            var ti = ((input.Width * oy) + ox) * input.Depth;
                            for (int fd = 0; fd < InputChannels; fd++)
                            {
                                var w = filter.Values[fi + fd];
                                var inVal = input.Values[ti + fd];
                                filterGrad.Values[fi + fd] += gradOut * inVal;
                                gradInSpan[ti + fd] += gradOut * w;
                            }
                        }
                    }
                }
            }
        }

        return gradInput;
    }

    private void ValidateInputShape(TensorShape inputShape)
    {
        if (inputShape.Depth != InputChannels)
        {
            throw new ArgumentException(
                $"Expected {InputChannels} input channels, got {inputShape.Depth}.",
                nameof(inputShape));
        }
    }

    internal ReadOnlySpan<Parameter<T>> FilterParameters => _filters;

    /// <summary>Filter parameters as the backing array, to avoid per-call copies in hot paths (e.g. parallel backward closures).</summary>
    internal Parameter<T>[] FilterParameterArray => _filters;

    internal Parameter<T> BiasParameter => _bias;
}
