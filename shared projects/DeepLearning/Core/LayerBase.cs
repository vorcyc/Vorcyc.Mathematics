namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Base class that caches forward-pass tensors for backpropagation.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public abstract class LayerBase<T> : ILayer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Initializes the layer with an optional name.
    /// </summary>
    protected LayerBase(string? name = null) => Name = name ?? GetType().Name;

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Gets the input tensor cached from the most recent forward pass.
    /// </summary>
    protected Tensor<T>? CachedInput { get; private set; }

    /// <summary>
    /// Gets the output tensor cached from the most recent forward pass.
    /// </summary>
    protected Tensor<T>? CachedOutput { get; private set; }

    /// <inheritdoc/>
    public abstract IReadOnlyList<Parameter<T>> Parameters { get; }

    /// <inheritdoc/>
    public abstract TensorShape GetOutputShape(TensorShape inputShape);

    /// <inheritdoc/>
    public abstract Tensor<T> Forward(Tensor<T> input, bool training = true);

    /// <inheritdoc/>
    public abstract Tensor<T> Backward(Tensor<T> gradOutput);

    /// <summary>
    /// Stores forward-pass tensors for use in <see cref="Backward"/>.
    /// </summary>
    protected void CacheForward(Tensor<T> input, Tensor<T> output)
    {
        CachedInput = input;
        CachedOutput = output;
    }

    /// <summary>
    /// Ensures that <see cref="Forward"/> has been called before <see cref="Backward"/>.
    /// </summary>
    protected void EnsureCached()
    {
        if (CachedInput is null || CachedOutput is null)
        {
            throw new InvalidOperationException($"Forward must be called on {Name} before Backward.");
        }
    }
}
