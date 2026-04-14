namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;

/// <summary>
/// Base class for batch layers that cache forward tensors.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public abstract class BatchLayerBase<T> : IBatchLayer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    protected BatchLayerBase(string? name = null) => Name = name ?? GetType().Name;

    /// <inheritdoc/>
    public string Name { get; }

    protected BatchTensor<T>? CachedInput { get; private set; }

    protected BatchTensor<T>? CachedOutput { get; private set; }

    /// <inheritdoc/>
    public abstract IReadOnlyList<Parameter<T>> Parameters { get; }

    /// <inheritdoc/>
    public abstract BatchShape GetOutputShape(BatchShape inputShape);

    /// <inheritdoc/>
    public abstract BatchTensor<T> Forward(BatchTensor<T> input, bool training = true);

    /// <inheritdoc/>
    public abstract BatchTensor<T> Backward(BatchTensor<T> gradOutput);

    protected void CacheForward(BatchTensor<T> input, BatchTensor<T> output)
    {
        CachedInput = input;
        CachedOutput = output;
    }

    protected void EnsureCached()
    {
        if (CachedInput is null || CachedOutput is null)
        {
            throw new InvalidOperationException($"Forward must be called on {Name} before Backward.");
        }
    }
}
