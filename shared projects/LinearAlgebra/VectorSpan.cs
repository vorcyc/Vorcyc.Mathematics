namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Span-based vector math helpers. Mathematical vectors are represented as
/// <see cref="ReadOnlySpan{T}"/> / <see cref="Span{T}"/> rather than a dedicated type.
/// </summary>
public static partial class VectorSpan
{
    /// <summary>
    /// Computes the dot product of two vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Dot<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (a.Length != b.Length)
            throw new ArgumentException("向量长度必须相同。", nameof(b));

        T sum = T.Zero;
        int vectorSize = Vector<T>.Count;
        int i = 0;

        if (vectorSize > 1)
        {
            var acc = Vector<T>.Zero;
            for (; i <= a.Length - vectorSize; i += vectorSize)
            {
                acc += new Vector<T>(a.Slice(i, vectorSize)) * new Vector<T>(b.Slice(i, vectorSize));
            }

            for (int j = 0; j < vectorSize; j++)
            {
                sum += acc[j];
            }
        }

        for (; i < a.Length; i++)
        {
            sum += a[i] * b[i];
        }

        return sum;
    }

    /// <summary>
    /// Computes the Euclidean norm of a vector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Norm<T>(ReadOnlySpan<T> vector)
        where T : struct, IFloatingPointIeee754<T>
        => T.Sqrt(Dot(vector, vector));

    /// <summary>
    /// Sums all elements.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(ReadOnlySpan<T> values)
        where T : struct, IFloatingPointIeee754<T>
    {
        T sum = T.Zero;
        int vectorSize = Vector<T>.Count;
        int i = 0;

        if (vectorSize > 1)
        {
            var acc = Vector<T>.Zero;
            for (; i <= values.Length - vectorSize; i += vectorSize)
            {
                acc += new Vector<T>(values.Slice(i, vectorSize));
            }

            for (int j = 0; j < vectorSize; j++)
                sum += acc[j];
        }

        for (; i < values.Length; i++)
            sum += values[i];

        return sum;
    }

    /// <summary>
    /// Computes <c>y += alpha * x</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Axpy<T>(T alpha, ReadOnlySpan<T> x, Span<T> y)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("向量长度必须相同。");

        int vectorSize = Vector<T>.Count;
        var vAlpha = new Vector<T>(alpha);
        int i = 0;

        for (; i <= x.Length - vectorSize; i += vectorSize)
        {
            var vx = new Vector<T>(x.Slice(i, vectorSize));
            var vy = new Vector<T>(y.Slice(i, vectorSize));
            (vy + vx * vAlpha).CopyTo(y.Slice(i, vectorSize));
        }

        for (; i < x.Length; i++)
            y[i] += alpha * x[i];
    }

    /// <summary>
    /// Writes <c>a + b</c> into <paramref name="result"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (a.Length != b.Length)
            throw new ArgumentException("向量长度必须相同。", nameof(b));
        if (result.Length != a.Length)
            throw new ArgumentException("结果向量长度必须与输入向量相同。", nameof(result));

        int vectorSize = Vector<T>.Count;
        int i = 0;

        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = new Vector<T>(a.Slice(i, vectorSize));
            var vb = new Vector<T>(b.Slice(i, vectorSize));
            (va + vb).CopyTo(result.Slice(i, vectorSize));
        }

        for (; i < a.Length; i++)
        {
            result[i] = a[i] + b[i];
        }
    }

    /// <summary>
    /// Writes <c>a - b</c> into <paramref name="result"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Subtract<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (a.Length != b.Length)
            throw new ArgumentException("向量长度必须相同。", nameof(b));
        if (result.Length != a.Length)
            throw new ArgumentException("结果向量长度必须与输入向量相同。", nameof(result));

        int vectorSize = Vector<T>.Count;
        int i = 0;

        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = new Vector<T>(a.Slice(i, vectorSize));
            var vb = new Vector<T>(b.Slice(i, vectorSize));
            (va - vb).CopyTo(result.Slice(i, vectorSize));
        }

        for (; i < a.Length; i++)
        {
            result[i] = a[i] - b[i];
        }
    }

    /// <summary>
    /// Writes <c>vector * scalar</c> into <paramref name="result"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Scale<T>(ReadOnlySpan<T> vector, T scalar, Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (result.Length != vector.Length)
            throw new ArgumentException("结果向量长度必须与输入向量相同。", nameof(result));

        int vectorSize = Vector<T>.Count;
        var vScalar = new Vector<T>(scalar);
        int i = 0;

        for (; i <= vector.Length - vectorSize; i += vectorSize)
        {
            var v = new Vector<T>(vector.Slice(i, vectorSize));
            (v * vScalar).CopyTo(result.Slice(i, vectorSize));
        }

        for (; i < vector.Length; i++)
        {
            result[i] = vector[i] * scalar;
        }
    }

    /// <summary>
    /// Writes the normalized vector into <paramref name="result"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Normalize<T>(ReadOnlySpan<T> vector, Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        T norm = Norm(vector);
        if (norm == T.Zero)
            throw new InvalidOperationException("零向量无法归一化。");

        Scale(vector, T.One / norm, result);
    }
}
