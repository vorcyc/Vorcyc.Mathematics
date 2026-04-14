namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Sigmoid activation: σ(x) = 1 / (1 + exp(-x)).
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class SigmoidActivation<T> : LayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Initializes the activation layer.
    /// </summary>
    public SigmoidActivation(string? name = null) : base(name) { }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override TensorShape GetOutputShape(TensorShape inputShape) => inputShape;

    /// <inheritdoc/>
    public override Tensor<T> Forward(Tensor<T> input, bool training = true)
    {
        var output = new Tensor<T>(input.Width, input.Height, input.Depth);
        var inSpan = input.Values;
        var outSpan = output.Values;
        for (int i = 0; i < inSpan.Length; i++)
        {
            outSpan[i] = T.One / (T.One + T.Exp(-inSpan[i]));
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override Tensor<T> Backward(Tensor<T> gradOutput)
    {
        EnsureCached();
        var output = CachedOutput!;
        var gradInput = new Tensor<T>(output.Width, output.Height, output.Depth);
        var gradSpan = gradInput.Values;
        var gradOutSpan = gradOutput.Values;
        var outSpan = output.Values;

        for (int i = 0; i < gradSpan.Length; i++)
        {
            var y = outSpan[i];
            gradSpan[i] = gradOutSpan[i] * y * (T.One - y);
        }

        return gradInput;
    }
}
