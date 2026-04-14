namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Tanh activation applied element-wise to a <see cref="BatchTensor{T}"/>.
/// </summary>
public sealed class BatchTanhActivation<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    public BatchTanhActivation(string? name = null) : base(name) { }

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
            output.Values[i] = T.Tanh(input.Values[i]);
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
        for (int i = 0; i < gradInput.Values.Length; i++)
        {
            var y = output.Values[i];
            gradInput.Values[i] = gradOutput.Values[i] * (T.One - y * y);
        }

        return gradInput;
    }
}
