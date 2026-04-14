namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// ReLU activation applied to an entire <see cref="BatchTensor{T}"/>.
/// </summary>
public sealed class BatchReLUActivation<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private BatchTensor<T>? _cachedInputTensor;

    public BatchReLUActivation(string? name = null) : base(name) { }

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
        int vectorSize = Vector<T>.Count;
        int i = 0;
        var zero = Vector<T>.Zero;
        for (; i <= inSpan.Length - vectorSize; i += vectorSize)
        {
            var vec = new Vector<T>(inSpan.Slice(i, vectorSize));
            Vector.Max(vec, zero).CopyTo(outSpan.Slice(i));
        }

        for (; i < inSpan.Length; i++)
        {
            outSpan[i] = inSpan[i] > T.Zero ? inSpan[i] : T.Zero;
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
        var inSpan = input.Values;
        var gradOutSpan = gradOutput.Values;
        var gradInSpan = gradInput.Values;
        for (int i = 0; i < gradInSpan.Length; i++)
        {
            gradInSpan[i] = inSpan[i] > T.Zero ? gradOutSpan[i] : T.Zero;
        }

        return gradInput;
    }
}
