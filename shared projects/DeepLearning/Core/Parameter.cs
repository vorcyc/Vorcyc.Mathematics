namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// A trainable tensor parameter with an accumulated gradient buffer.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class Parameter<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Initializes a new parameter from an existing tensor value.
    /// </summary>
    /// <param name="value">Parameter value tensor.</param>
    /// <param name="name">Optional name used for serialization.</param>
    public Parameter(Tensor<T> value, string? name = null)
    {
        Value = value;
        Name = name;
        Gradient = new Tensor<T>(value.Width, value.Height, value.Depth);
        Gradient.Fill(T.Zero);
    }

    /// <summary>
    /// Gets an optional parameter name for serialization.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the parameter value.
    /// </summary>
    public Tensor<T> Value { get; }

    /// <summary>
    /// Gets the gradient buffer accumulated during backpropagation.
    /// </summary>
    public Tensor<T> Gradient { get; }

    /// <summary>
    /// Resets the gradient buffer to zero.
    /// </summary>
    public void ZeroGradient() => Gradient.Fill(T.Zero);
}
