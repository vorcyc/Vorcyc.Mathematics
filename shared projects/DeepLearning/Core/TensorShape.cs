namespace Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Describes a 3-D tensor layout used by the deep learning module.
/// </summary>
/// <param name="Width">Width (X).</param>
/// <param name="Height">Height (Y). Also used as batch size for the 1×N×F vector layout.</param>
/// <param name="Depth">Depth (channels / features).</param>
public readonly record struct TensorShape(int Width, int Height, int Depth)
{
    /// <summary>
    /// Gets the total number of elements.
    /// </summary>
    public int ElementCount => Width * Height * Depth;

    /// <summary>
    /// Gets the batch size for batched feature vectors laid out as 1×N×F.
    /// </summary>
    public int BatchSize => Height;

    /// <summary>
    /// Creates a single-sample vector shape 1×1×F.
    /// </summary>
    public static TensorShape Vector(int features) => new(1, 1, features);

    /// <summary>
    /// Creates a batched vector shape 1×N×F.
    /// </summary>
    public static TensorShape BatchedVector(int batchSize, int features) => new(1, batchSize, features);

    /// <summary>
    /// Creates a shape from an existing tensor.
    /// </summary>
    public static TensorShape From<T>(LinearAlgebra.Tensor<T> tensor)
        where T : System.Numerics.IBinaryFloatingPointIeee754<T>
        => new(tensor.Width, tensor.Height, tensor.Depth);
}
