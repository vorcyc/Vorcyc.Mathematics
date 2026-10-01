namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
            throw new ArgumentException("Vector lengths must be the same.", nameof(b));

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
            throw new ArgumentException("Vector lengths must be the same.");

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
            throw new ArgumentException("Vector lengths must be the same.", nameof(b));
        if (result.Length != a.Length)
            throw new ArgumentException("Result vector length must match the input vector length.", nameof(result));

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
            throw new ArgumentException("Vector lengths must be the same.", nameof(b));
        if (result.Length != a.Length)
            throw new ArgumentException("Result vector length must match the input vector length.", nameof(result));

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
            throw new ArgumentException("Result vector length must match the input vector length.", nameof(result));

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
            throw new InvalidOperationException("Cannot normalize a zero vector.");

        Scale(vector, T.One / norm, result);
    }

    /// <summary>
    /// Writes the elementwise exponential <c>exp(x)</c> into <paramref name="result"/>.
    /// </summary>
    /// <remarks>
    /// <see langword="float"/>/<see langword="double"/> use a vectorized minimax-polynomial approximation
    /// (Cephes-derived, range-reduced via <c>exp(x) = 2^n * exp(r)</c>); other <typeparamref name="T"/>
    /// fall back to a scalar <c>T.Exp(x)</c> loop.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Exp<T>(ReadOnlySpan<T> vector, Span<T> result)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (result.Length != vector.Length)
            throw new ArgumentException("Result vector length must match the input vector length.", nameof(result));

        if (typeof(T) == typeof(float))
        {
            ExpCoreFloat(MemoryMarshal.Cast<T, float>(vector), MemoryMarshal.Cast<T, float>(result));
            return;
        }
        if (typeof(T) == typeof(double))
        {
            ExpCoreDouble(MemoryMarshal.Cast<T, double>(vector), MemoryMarshal.Cast<T, double>(result));
            return;
        }

        for (int i = 0; i < vector.Length; i++)
            result[i] = T.Exp(vector[i]);
    }

    // Cephes-derived expf constants (single precision); range reduction exp(x) = 2^n * exp(r).
    private static readonly Vector<float> s_expHiF = new(88.3762626647950f);
    private static readonly Vector<float> s_expLoF = new(-88.3762626647950f);
    private static readonly Vector<float> s_logTwoEF = new(1.44269504088896341f);
    private static readonly Vector<float> s_expC1F = new(0.693359375f);
    private static readonly Vector<float> s_expC2F = new(-2.12194440e-4f);
    private static readonly Vector<float> s_expP0F = new(1.9875691500e-4f);
    private static readonly Vector<float> s_expP1F = new(1.3981999507e-3f);
    private static readonly Vector<float> s_expP2F = new(8.3334519073e-3f);
    private static readonly Vector<float> s_expP3F = new(4.1665795894e-2f);
    private static readonly Vector<float> s_expP4F = new(1.6666665459e-1f);
    private static readonly Vector<float> s_expP5F = new(5.0000001201e-1f);
    private static readonly Vector<float> s_halfF = new(0.5f);
    private static readonly Vector<int> s_expBiasI = new(0x7f);

    private static void ExpCoreFloat(ReadOnlySpan<float> source, Span<float> destination)
    {
        int vectorSize = Vector<float>.Count;
        int i = 0;

        for (; i <= source.Length - vectorSize; i += vectorSize)
        {
            var x = Vector.Min(Vector.Max(new Vector<float>(source.Slice(i, vectorSize)), s_expLoF), s_expHiF);

            var fx = Vector.Floor(x * s_logTwoEF + s_halfF);
            var xr = x - fx * s_expC1F - fx * s_expC2F;
            var zr = xr * xr;

            var y = s_expP0F;
            y = y * xr + s_expP1F;
            y = y * xr + s_expP2F;
            y = y * xr + s_expP3F;
            y = y * xr + s_expP4F;
            y = y * xr + s_expP5F;
            y = y * zr + xr + Vector<float>.One;

            var pow2n = Vector.AsVectorSingle(Vector.ShiftLeft(Vector.ConvertToInt32(fx) + s_expBiasI, 23));

            (y * pow2n).CopyTo(destination.Slice(i, vectorSize));
        }

        for (; i < source.Length; i++)
            destination[i] = MathF.Exp(source[i]);
    }

    // Cephes-derived exp constants (double precision); rational (Padé) approximation of exp(r) on the reduced range.
    private static readonly Vector<double> s_expHiD = new(708.3964185322641);
    private static readonly Vector<double> s_expLoD = new(-708.3964185322641);
    private static readonly Vector<double> s_logTwoED = new(1.4426950408889634073599);
    private static readonly Vector<double> s_expC1D = new(6.93145751953125E-1);
    private static readonly Vector<double> s_expC2D = new(1.42860682030941723212E-6);
    private static readonly Vector<double> s_expP0D = new(1.26177193074810590878E-4);
    private static readonly Vector<double> s_expP1D = new(3.02994407707441961300E-2);
    private static readonly Vector<double> s_expP2D = new(9.99999999999999999910E-1);
    private static readonly Vector<double> s_expQ0D = new(3.00198505138664455042E-6);
    private static readonly Vector<double> s_expQ1D = new(2.52448340349684104192E-3);
    private static readonly Vector<double> s_expQ2D = new(2.27265548208155028766E-1);
    private static readonly Vector<double> s_expQ3D = new(2.00000000000000000009);
    private static readonly Vector<double> s_halfD = new(0.5);
    private static readonly Vector<double> s_twoD = new(2.0);
    private static readonly Vector<long> s_expBiasL = new(1023L);

    private static void ExpCoreDouble(ReadOnlySpan<double> source, Span<double> destination)
    {
        int vectorSize = Vector<double>.Count;
        int i = 0;

        for (; i <= source.Length - vectorSize; i += vectorSize)
        {
            var x = Vector.Min(Vector.Max(new Vector<double>(source.Slice(i, vectorSize)), s_expLoD), s_expHiD);

            var fx = Vector.Floor(x * s_logTwoED + s_halfD);
            var xr = x - fx * s_expC1D - fx * s_expC2D;
            var x2 = xr * xr;

            var px = s_expP0D;
            px = px * x2 + s_expP1D;
            px = px * x2 + s_expP2D;
            px = px * xr;

            var qx = s_expQ0D;
            qx = qx * x2 + s_expQ1D;
            qx = qx * x2 + s_expQ2D;
            qx = qx * x2 + s_expQ3D;

            var y = px / (qx - px);
            y = Vector<double>.One + s_twoD * y;

            var pow2n = Vector.AsVectorDouble(Vector.ShiftLeft(Vector.ConvertToInt64(fx) + s_expBiasL, 52));

            (y * pow2n).CopyTo(destination.Slice(i, vectorSize));
        }

        for (; i < source.Length; i++)
            destination[i] = Math.Exp(source[i]);
    }
}
