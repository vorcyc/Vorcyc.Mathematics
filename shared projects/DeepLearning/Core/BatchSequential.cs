namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;

/// <summary>
/// A model stack for 4-D batch tensors.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class BatchSequential<T> : IBatchLayer<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly IBatchLayer<T>[] _layers;

    public BatchSequential(params IBatchLayer<T>[] layers)
    {
        ArgumentNullException.ThrowIfNull(layers);
        if (layers.Length == 0)
        {
            throw new ArgumentException("At least one layer is required.", nameof(layers));
        }

        _layers = layers;
    }

    /// <inheritdoc/>
    public string Name => "BatchSequential";

    /// <summary>Gets the ordered layers.</summary>
    public IReadOnlyList<IBatchLayer<T>> Layers => _layers;

    /// <inheritdoc/>
    public IReadOnlyList<Parameter<T>> Parameters
        => _layers.SelectMany(layer => layer.Parameters).ToArray();

    /// <inheritdoc/>
    public BatchShape GetOutputShape(BatchShape inputShape)
    {
        var shape = inputShape;
        foreach (var layer in _layers)
        {
            shape = layer.GetOutputShape(shape);
        }

        return shape;
    }

    /// <inheritdoc/>
    public BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var current = input;
        foreach (var layer in _layers)
        {
            current = layer.Forward(current, training);
        }

        return current;
    }

    /// <inheritdoc/>
    public BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        var current = gradOutput;
        for (int i = _layers.Length - 1; i >= 0; i--)
        {
            current = _layers[i].Backward(current);
        }

        return current;
    }

    /// <summary>Resets all parameter gradients.</summary>
    public void ZeroGradients()
    {
        foreach (var parameter in Parameters)
        {
            parameter.ZeroGradient();
        }
    }
}
