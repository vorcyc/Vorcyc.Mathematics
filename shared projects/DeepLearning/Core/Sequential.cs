namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// A model composed of an ordered stack of layers.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class Sequential<T> : ILayer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly ILayer<T>[] _layers;

    /// <summary>
    /// Initializes a sequential model from the given layers.
    /// </summary>
    public Sequential(params ILayer<T>[] layers)
    {
        ArgumentNullException.ThrowIfNull(layers);
        if (layers.Length == 0)
        {
            throw new ArgumentException("At least one layer is required.", nameof(layers));
        }

        _layers = layers;
    }

    /// <inheritdoc/>
    public string Name => "Sequential";

    /// <summary>
    /// Gets the ordered layers.
    /// </summary>
    public IReadOnlyList<ILayer<T>> Layers => _layers;

    /// <inheritdoc/>
    public IReadOnlyList<Parameter<T>> Parameters
        => _layers.SelectMany(layer => layer.Parameters).ToArray();

    /// <inheritdoc/>
    public TensorShape GetOutputShape(TensorShape inputShape)
    {
        var shape = inputShape;
        foreach (var layer in _layers)
        {
            shape = layer.GetOutputShape(shape);
        }

        return shape;
    }

    /// <inheritdoc/>
    public Tensor<T> Forward(Tensor<T> input, bool training = true)
    {
        var current = input;
        foreach (var layer in _layers)
        {
            current = layer.Forward(current, training);
        }

        return current;
    }

    /// <inheritdoc/>
    public Tensor<T> Backward(Tensor<T> gradOutput)
    {
        var current = gradOutput;
        for (int i = _layers.Length - 1; i >= 0; i--)
        {
            current = _layers[i].Backward(current);
        }

        return current;
    }

    /// <summary>
    /// Resets gradients for all trainable parameters.
    /// </summary>
    public void ZeroGradients()
    {
        foreach (var parameter in Parameters)
        {
            parameter.ZeroGradient();
        }
    }
}
