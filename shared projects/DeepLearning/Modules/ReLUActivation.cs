namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Rectified linear unit activation.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class ReLUActivation<T> : LayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Initializes the activation layer.
    /// </summary>
    public ReLUActivation(string? name = null) : base(name) { }

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
            outSpan[i] = inSpan[i] > T.Zero ? inSpan[i] : T.Zero;
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override Tensor<T> Backward(Tensor<T> gradOutput)
    {
        EnsureCached();
        var input = CachedInput!;
        var gradInput = new Tensor<T>(input.Width, input.Height, input.Depth);
        var gradSpan = gradInput.Values;
        var gradOutSpan = gradOutput.Values;
        var inSpan = input.Values;

        for (int i = 0; i < gradSpan.Length; i++)
        {
            gradSpan[i] = inSpan[i] > T.Zero ? gradOutSpan[i] : T.Zero;
        }

        return gradInput;
    }
}
