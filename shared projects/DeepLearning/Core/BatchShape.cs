namespace Vorcyc.Mathematics.DeepLearning;

/// <summary>
/// Describes a 4-D batch tensor layout in N×H×W×C (NHWC) order.
/// </summary>
public readonly record struct BatchShape(int Batch, int Height, int Width, int Channels)
{
    /// <summary>Gets the total number of elements.</summary>
    public int ElementCount => Batch * Height * Width * Channels;

    /// <summary>Creates a batched vector shape N×1×1×F.</summary>
    public static BatchShape Vector(int batch, int features) => new(batch, 1, 1, features);

    /// <summary>Creates an image batch shape N×H×W×C.</summary>
    public static BatchShape Image(int batch, int height, int width, int channels)
        => new(batch, height, width, channels);
}
