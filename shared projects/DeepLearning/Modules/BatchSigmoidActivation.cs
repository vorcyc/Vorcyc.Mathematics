namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Sigmoid activation applied element-wise to a <see cref="BatchTensor{T}"/>.
/// </summary>
public sealed class BatchSigmoidActivation<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    public BatchSigmoidActivation(string? name = null) : base(name) { }

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape) => inputShape;

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var output = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
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
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        var output = CachedOutput!;
        var gradInput = new BatchTensor<T>(output.Batch, output.Height, output.Width, output.Channels);
        var outSpan = output.Values;
        var gradOutSpan = gradOutput.Values;
        var gradInSpan = gradInput.Values;

        for (int i = 0; i < gradInSpan.Length; i++)
        {
            var y = outSpan[i];
            gradInSpan[i] = gradOutSpan[i] * y * (T.One - y);
        }

        return gradInput;
    }
}
