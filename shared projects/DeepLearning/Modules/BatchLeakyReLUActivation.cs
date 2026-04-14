namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Leaky ReLU activation on NHWC tensors.
/// </summary>
public sealed class BatchLeakyReLUActivation<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private BatchTensor<T>? _cachedInputTensor;

    public BatchLeakyReLUActivation(double negativeSlope = 0.01, string? name = null)
        : base(name)
    {
        NegativeSlope = T.CreateTruncating(negativeSlope);
    }

    /// <summary>Gets the negative slope.</summary>
    public T NegativeSlope { get; }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape) => inputShape;

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var output = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        for (int i = 0; i < input.Values.Length; i++)
        {
            var x = input.Values[i];
            output.Values[i] = x > T.Zero ? x : x * NegativeSlope;
        }

        _cachedInputTensor = input;
        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        var input = _cachedInputTensor!;
        var gradInput = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        for (int i = 0; i < gradInput.Values.Length; i++)
        {
            gradInput.Values[i] = input.Values[i] > T.Zero
                ? gradOutput.Values[i]
                : gradOutput.Values[i] * NegativeSlope;
        }

        return gradInput;
    }
}
