namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Describes a four-dimensional tensor shape. Axes are layout-neutral (<see cref="Dim0"/> … <see cref="Dim3"/>).
/// </summary>
public readonly record struct Tensor4DShape(int Dim0, int Dim1, int Dim2, int Dim3)
{
    /// <summary>Gets the total number of elements.</summary>
    public int ElementCount => Dim0 * Dim1 * Dim2 * Dim3;

    /// <summary>Creates a shape equivalent to N×H×W×C (NHWC) row-major storage.</summary>
    public static Tensor4DShape Nhwc(int dim0, int dim1, int dim2, int dim3)
        => new(dim0, dim1, dim2, dim3);
}

/// <summary>
/// A four-dimensional tensor with contiguous row-major storage over
/// <see cref="Tensor4DShape.Dim0"/> × <see cref="Tensor4DShape.Dim1"/> ×
/// <see cref="Tensor4DShape.Dim2"/> × <see cref="Tensor4DShape.Dim3"/>.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class Tensor4D<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Memory<T> _data;

    /// <summary>
    /// Creates an uninitialized 4-D tensor.
    /// </summary>
    public Tensor4D(int dim0, int dim1, int dim2, int dim3)
    {
        if (dim0 <= 0 || dim1 <= 0 || dim2 <= 0 || dim3 <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dim0), "All dimensions must be positive.");
        }

        Shape = new Tensor4DShape(dim0, dim1, dim2, dim3);
        _data = new T[Shape.ElementCount];
    }

    /// <summary>Gets the tensor shape.</summary>
    public Tensor4DShape Shape { get; }

    /// <summary>Gets the extent of axis 0.</summary>
    public int Dim0 => Shape.Dim0;

    /// <summary>Gets the extent of axis 1.</summary>
    public int Dim1 => Shape.Dim1;

    /// <summary>Gets the extent of axis 2.</summary>
    public int Dim2 => Shape.Dim2;

    /// <summary>Gets the extent of axis 3.</summary>
    public int Dim3 => Shape.Dim3;

    /// <summary>Gets the underlying contiguous values.</summary>
    public Span<T> Values => _data.Span;

    /// <summary>Gets the underlying memory for zero-copy kernel access.</summary>
    internal Memory<T> Buffer => _data;

    /// <summary>
    /// Gets or sets an element in row-major order.
    /// </summary>
    public T this[int d0, int d1, int d2, int d3]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Values[GetIndex(d0, d1, d2, d3)];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Values[GetIndex(d0, d1, d2, d3)] = value;
    }

    /// <summary>
    /// Computes the linear index for the given coordinates.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetIndex(int d0, int d1, int d2, int d3)
        => GetLinearIndex(Shape, d0, d1, d2, d3);

    /// <summary>
    /// Computes a linear index for the given shape and coordinates.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetLinearIndex(Tensor4DShape shape, int d0, int d1, int d2, int d3)
        => ((d0 * shape.Dim1 + d1) * shape.Dim2 + d2) * shape.Dim3 + d3;

    /// <summary>
    /// Fills all elements with <paramref name="value"/>.
    /// </summary>
    public void Fill(T value)
    {
        var span = Values;
        int simdLength = Vector<T>.Count;
        int i = 0;

        if (simdLength > 1)
        {
            var simdValue = new Vector<T>(value);
            for (; i <= span.Length - simdLength; i += simdLength)
            {
                simdValue.CopyTo(span.Slice(i, simdLength));
            }
        }

        for (; i < span.Length; i++)
        {
            span[i] = value;
        }
    }
}
