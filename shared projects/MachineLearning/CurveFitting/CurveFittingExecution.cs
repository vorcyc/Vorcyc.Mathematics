using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vorcyc.Mathematics.MachineLearning.CurveFitting;

/// <summary>
/// Curve-fitting dispatch aligned with <see cref="LinearAlgebra.VectorSpan"/> /
/// Statistics:
/// <list type="number">
/// <item><see cref="ComputingContextExecution.UseParallelIndexed"/> → Parallel (workers stay scalar)</item>
/// <item>else <see cref="CpuExecutionMode.Normal"/> (or non float/double) → scalar</item>
/// <item>else → SIMD (also: explicit Parallel below the shared threshold)</item>
/// </list>
/// Parallel policy is owned solely by <see cref="ComputingContextExecution"/>;
/// call <see cref="ComputingContextExecution.ForEach"/> with the real caller context
/// (same as Matrix / KMeans / PCA / Standardization).
/// </summary>
internal enum CurveFitDispatchKind
{
    Normal,
    Simd,
    Parallel
}

internal static class CurveFittingExecution
{
    /// <summary>
    /// Total work used for both Auto mode resolution and the parallel gate
    /// (<c>count * workPerItem</c>, clamped to <see cref="int.MaxValue"/>).
    /// </summary>
    public static int TotalWorkSize(int count, long workPerItem)
    {
        if (count <= 0 || workPerItem <= 0)
            return 0;
        long total = (long)count * workPerItem;
        return total > int.MaxValue ? int.MaxValue : (int)total;
    }

    /// <summary>
    /// Resolves Normal / Simd / Parallel (VectorSpan order: Parallel gate first).
    /// </summary>
    public static CurveFitDispatchKind ResolveDispatch<T>(
        ComputingContext? context, int count, long workPerItem = 1)
        where T : unmanaged
    {
        if (ComputingContextExecution.UseParallelIndexed(context, count, workPerItem))
            return CurveFitDispatchKind.Parallel;

        int problemSize = TotalWorkSize(count, workPerItem);
        var mode = ComputingContext.Resolve(context).ResolveCpuMode(problemSize);
        if (mode == CpuExecutionMode.Normal || !CanUseSimdHardware<T>())
            return CurveFitDispatchKind.Normal;

        return CurveFitDispatchKind.Simd;
    }

    public static bool CanUseSimdHardware<T>()
        where T : unmanaged
        => typeof(T) == typeof(float) || typeof(T) == typeof(double);

    /// <summary>热循环取消步长（与 VSS 重构扫点相同：每 1024 项）。</summary>
    public const int CancelCheckStride = 1024;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfCancelled(CancellationToken cancellationToken, int index = 0)
    {
        if ((index & (CancelCheckStride - 1)) == 0)
            cancellationToken.ThrowIfCancellationRequested();
    }

    // ── kernels (no parallel policy) ─────────────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SumRange<T>(ReadOnlySpan<T> values, int start, int end, bool useSimd)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int count = end - start + 1;
        if (count <= 0)
            return T.Zero;

        if (useSimd && CanUseSimdHardware<T>())
        {
            if (typeof(T) == typeof(float))
                return T.CreateTruncating(TensorPrimitives.Sum(MemoryMarshal.Cast<T, float>(values.Slice(start, count))));
            if (typeof(T) == typeof(double))
                return T.CreateTruncating(TensorPrimitives.Sum(MemoryMarshal.Cast<T, double>(values.Slice(start, count))));
        }

        T sum = T.Zero;
        for (int j = start; j <= end; j++)
            sum += values[j];
        return sum;
    }

    public static T MeanSquaredError<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, bool useSimd)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Length mismatch.");
        int n = a.Length;
        if (n == 0)
            return T.Zero;

        if (useSimd && CanUseSimdHardware<T>())
        {
            if (typeof(T) == typeof(float))
            {
                var af = MemoryMarshal.Cast<T, float>(a);
                var bf = MemoryMarshal.Cast<T, float>(b);
                Span<float> diff = n <= 256 ? stackalloc float[n] : new float[n];
                TensorPrimitives.Subtract(af, bf, diff);
                return T.CreateTruncating(TensorPrimitives.SumOfSquares(diff) / n);
            }
            if (typeof(T) == typeof(double))
            {
                var ad = MemoryMarshal.Cast<T, double>(a);
                var bd = MemoryMarshal.Cast<T, double>(b);
                Span<double> diff = n <= 256 ? stackalloc double[n] : new double[n];
                TensorPrimitives.Subtract(ad, bd, diff);
                return T.CreateTruncating(TensorPrimitives.SumOfSquares(diff) / n);
            }
        }

        T mse = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T d = a[i] - b[i];
            mse += d * d;
        }
        return mse / T.CreateChecked(n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Dot<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, bool useSimd)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (useSimd && CanUseSimdHardware<T>())
            return Internal.NumericKernels.Dot(a, b);

        T sum = T.Zero;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    public static void FillRbfKernelRow<T>(
        ReadOnlySpan<T> x, T xRef, T lengthScale, T signalVariance, Span<T> destination, bool useSimd)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = x.Length;
        if (destination.Length < n)
            throw new ArgumentException("Destination too short.");

        if (useSimd && typeof(T) == typeof(float))
        {
            FillRbfKernelRowFloat(
                MemoryMarshal.Cast<T, float>(x),
                float.CreateTruncating(xRef),
                float.CreateTruncating(lengthScale),
                float.CreateTruncating(signalVariance),
                MemoryMarshal.Cast<T, float>(destination));
            return;
        }
        if (useSimd && typeof(T) == typeof(double))
        {
            FillRbfKernelRowDouble(
                MemoryMarshal.Cast<T, double>(x),
                double.CreateTruncating(xRef),
                double.CreateTruncating(lengthScale),
                double.CreateTruncating(signalVariance),
                MemoryMarshal.Cast<T, double>(destination));
            return;
        }

        T inv = T.One / (T.CreateChecked(2) * lengthScale * lengthScale);
        for (int j = 0; j < n; j++)
        {
            T d = x[j] - xRef;
            destination[j] = signalVariance * T.Exp(-(d * d) * inv);
        }
    }

    private static void FillRbfKernelRowFloat(
        ReadOnlySpan<float> x, float xRef, float lengthScale, float signalVariance, Span<float> destination)
    {
        int n = x.Length;
        int vs = Vector<float>.Count;
        var xRefV = new Vector<float>(xRef);
        float inv = 1f / (2f * lengthScale * lengthScale);
        var invV = new Vector<float>(inv);
        int j = 0;
        for (; j <= n - vs; j += vs)
        {
            var d = new Vector<float>(x.Slice(j, vs)) - xRefV;
            (d * d * invV).CopyTo(destination.Slice(j, vs));
        }
        for (; j < n; j++)
        {
            float d = x[j] - xRef;
            destination[j] = d * d * inv;
        }
        TensorPrimitives.Negate(destination, destination);
        TensorPrimitives.Exp(destination, destination);
        TensorPrimitives.Multiply(destination, signalVariance, destination);
    }

    private static void FillRbfKernelRowDouble(
        ReadOnlySpan<double> x, double xRef, double lengthScale, double signalVariance, Span<double> destination)
    {
        int n = x.Length;
        int vs = Vector<double>.Count;
        var xRefV = new Vector<double>(xRef);
        double inv = 1.0 / (2.0 * lengthScale * lengthScale);
        var invV = new Vector<double>(inv);
        int j = 0;
        for (; j <= n - vs; j += vs)
        {
            var d = new Vector<double>(x.Slice(j, vs)) - xRefV;
            (d * d * invV).CopyTo(destination.Slice(j, vs));
        }
        for (; j < n; j++)
        {
            double d = x[j] - xRef;
            destination[j] = d * d * inv;
        }
        TensorPrimitives.Negate(destination, destination);
        TensorPrimitives.Exp(destination, destination);
        TensorPrimitives.Multiply(destination, signalVariance, destination);
    }

    public static void AccumWeightedLinearSums<T>(
        ReadOnlySpan<T> xData, ReadOnlySpan<T> yData, T xQuery, T bandwidth,
        out T wSum, out T wxSum, out T wySum, out T wxxSum, out T wxySum, bool useSimd,
        CancellationToken cancellationToken = default)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = xData.Length;
        wSum = wxSum = wySum = wxxSum = wxySum = T.Zero;

        if (useSimd && CanUseSimdHardware<T>() && n >= Vector<T>.Count * 2)
        {
            Span<T> w = n <= 256 ? stackalloc T[n] : new T[n];
            for (int i = 0; i < n; i++)
            {
                ThrowIfCancelled(cancellationToken, i);
                T u = T.Abs((xQuery - xData[i]) / bandwidth);
                if (u >= T.One)
                    w[i] = T.Zero;
                else
                {
                    T t = T.One - u * u * u;
                    w[i] = t * t * t;
                }
            }

            int vs = Vector<T>.Count;
            var vWSum = Vector<T>.Zero;
            var vWx = Vector<T>.Zero;
            var vWy = Vector<T>.Zero;
            var vWxx = Vector<T>.Zero;
            var vWxy = Vector<T>.Zero;
            int i0 = 0;
            for (; i0 <= n - vs; i0 += vs)
            {
                var wv = new Vector<T>(w.Slice(i0, vs));
                var xv = new Vector<T>(xData.Slice(i0, vs));
                var yv = new Vector<T>(yData.Slice(i0, vs));
                vWSum += wv;
                vWx += wv * xv;
                vWy += wv * yv;
                vWxx += wv * xv * xv;
                vWxy += wv * xv * yv;
            }
            wSum = Vector.Sum(vWSum);
            wxSum = Vector.Sum(vWx);
            wySum = Vector.Sum(vWy);
            wxxSum = Vector.Sum(vWxx);
            wxySum = Vector.Sum(vWxy);
            for (; i0 < n; i0++)
            {
                T wi = w[i0];
                T xi = xData[i0];
                T yi = yData[i0];
                wSum += wi;
                wxSum += wi * xi;
                wySum += wi * yi;
                wxxSum += wi * xi * xi;
                wxySum += wi * xi * yi;
            }
            return;
        }

        for (int i = 0; i < n; i++)
        {
            ThrowIfCancelled(cancellationToken, i);
            T u = T.Abs((xQuery - xData[i]) / bandwidth);
            T wi;
            if (u >= T.One)
                wi = T.Zero;
            else
            {
                T t = T.One - u * u * u;
                wi = t * t * t;
            }
            T xi = xData[i];
            T yi = yData[i];
            wSum += wi;
            wxSum += wi * xi;
            wySum += wi * yi;
            wxxSum += wi * xi * xi;
            wxySum += wi * xi * yi;
        }
    }
}
