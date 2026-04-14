namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Softmax activation over the depth (feature) dimension.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class SoftmaxLayer<T> : LayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Initializes the softmax layer.
    /// </summary>
    public SoftmaxLayer(string? name = null) : base(name) { }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override TensorShape GetOutputShape(TensorShape inputShape) => inputShape;

    /// <inheritdoc/>
    public override Tensor<T> Forward(Tensor<T> input, bool training = true)
    {
        var output = new Tensor<T>(input.Width, input.Height, input.Depth);
        ApplySoftmax(input, output);
        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override Tensor<T> Backward(Tensor<T> gradOutput)
    {
        EnsureCached();
        var output = CachedOutput!;
        var gradInput = new Tensor<T>(output.Width, output.Height, output.Depth);

        // Jacobian of softmax: gradInput_i = sum_j gradOutput_j * y_i * (δ_ij - y_j)
        for (int z = 0; z < output.Depth; z++)
        {
            for (int y = 0; y < output.Height; y++)
            {
                for (int x = 0; x < output.Width; x++)
                {
                    T sum = T.Zero;
                    var yi = output[x, y, z];
                    for (int j = 0; j < output.Depth; j++)
                    {
                        var yj = output[x, y, j];
                        var delta = z == j ? T.One : T.Zero;
                        sum += gradOutput[x, y, j] * yi * (delta - yj);
                    }

                    gradInput[x, y, z] = sum;
                }
            }
        }

        return gradInput;
    }

    private static void ApplySoftmax(Tensor<T> input, Tensor<T> output)
    {
        for (int z = 0; z < input.Depth; z++)
        {
            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    T max = input[x, y, 0];
                    for (int d = 1; d < input.Depth; d++)
                    {
                        var v = input[x, y, d];
                        if (v > max)
                        {
                            max = v;
                        }
                    }

                    T sum = T.Zero;
                    for (int d = 0; d < input.Depth; d++)
                    {
                        var exp = T.Exp(input[x, y, d] - max);
                        output[x, y, d] = exp;
                        sum += exp;
                    }

                    for (int d = 0; d < input.Depth; d++)
                    {
                        output[x, y, d] /= sum;
                    }
                }
            }
        }
    }
}
