namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// 2×2 max pooling with stride 2 (matches the legacy <see cref="Layers.Layers.MaxPool2D"/> behavior).
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class MaxPool2DLayer<T> : LayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
{
    private int[]? _argmaxIndices;

    /// <summary>
    /// Initializes the pooling layer.
    /// </summary>
    public MaxPool2DLayer(string? name = null) : base(name) { }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override TensorShape GetOutputShape(TensorShape inputShape)
        => new(inputShape.Width / 2, inputShape.Height / 2, inputShape.Depth);

    /// <inheritdoc/>
    public override Tensor<T> Forward(Tensor<T> input, bool training = true)
    {
        var outputWidth = input.Width / 2;
        var outputHeight = input.Height / 2;
        var output = new Tensor<T>(outputWidth, outputHeight, input.Depth);
        _argmaxIndices = new int[outputWidth * outputHeight * input.Depth];

        for (int d = 0; d < input.Depth; d++)
        {
            for (int ay = 0; ay < outputHeight; ay++)
            {
                var y = 2 * ay;
                for (int ax = 0; ax < outputWidth; ax++)
                {
                    var x = 2 * ax;
                    T max = T.MinValue;
                    int bestIndex = 0;

                    for (int fy = 0; fy < 2; fy++)
                    {
                        for (int fx = 0; fx < 2; fx++)
                        {
                            var oy = y + fy;
                            var ox = x + fx;
                            if (oy < input.Height && ox < input.Width)
                            {
                                var flatIndex = ((input.Width * oy) + ox) * input.Depth + d;
                                var v = input.Values[flatIndex];
                                if (v > max)
                                {
                                    max = v;
                                    bestIndex = flatIndex;
                                }
                            }
                        }
                    }

                    output[ax, ay, d] = max;
                    _argmaxIndices[((outputWidth * ay) + ax) * input.Depth + d] = bestIndex;
                }
            }
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override Tensor<T> Backward(Tensor<T> gradOutput)
    {
        EnsureCached();
        if (_argmaxIndices is null)
        {
            throw new InvalidOperationException("Forward must be called before Backward.");
        }

        var input = CachedInput!;
        var gradInput = new Tensor<T>(input.Width, input.Height, input.Depth);
        gradInput.Fill(T.Zero);
        var gradInSpan = gradInput.Values;

        for (int i = 0; i < _argmaxIndices.Length; i++)
        {
            gradInSpan[_argmaxIndices[i]] += gradOutput.Values[i];
        }

        return gradInput;
    }
}
