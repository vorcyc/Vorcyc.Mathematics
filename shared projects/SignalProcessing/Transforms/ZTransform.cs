using System.Numerics;
using System.Runtime.InteropServices;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms;

/// <summary>
/// Discrete-time Z-transform helpers: DTFT on the unit circle, rational H(z), stability.
/// Discrete TF coefficients use negative powers of z:
/// <c>b0 + b1·z⁻¹ + …</c> over <c>a0 + a1·z⁻¹ + …</c> (same as <see cref="Filters.Base.TransferFunction"/>).
/// </summary>
public static class ZTransform
{
    /// <summary>
    /// Discrete-time Fourier transform (Z on the unit circle):
    /// X(e^{jω_k}) for ω_k = 2π k / N, k = 0..N−1.
    /// Exact for finite-length sequences (not an FFT approximation).
    /// Honors <paramref name="context"/> / <see cref="ComputingScope"/>:
    /// parallel over bins when the shared parallel gate fires; otherwise scalar or frequency-lane SIMD
    /// (explicit <see cref="ComputingContext.Simd"/> selects SIMD regardless of Auto size ladders).
    /// </summary>
    public static ComplexFp32[] Dtft(ReadOnlySpan<float> input, int numPoints, ComputingContext? context = null)
    {
        if (numPoints < 2)
            throw new ArgumentOutOfRangeException(nameof(numPoints));

        float[] data = input.ToArray();
        int nLen = data.Length;
        var result = new ComplexFp32[numPoints];
        long workPer = Math.Max(1, nLen);

        if (ComputingContextExecution.UseParallelIndexed(context, numPoints, workPer))
        {
            ComputingContextExecution.ForEach(
                context,
                0,
                numPoints,
                k => result[k] = DtftBinScalar(data, k, numPoints),
                workPerItem: workPer);
            return result;
        }

        int problemSize = TotalWork(numPoints, workPer);
        if (PreferScalar(context, problemSize))
        {
            for (int k = 0; k < numPoints; k++)
                result[k] = DtftBinScalar(data, k, numPoints);
            return result;
        }

        DtftSimd(data, result, numPoints);
        return result;
    }

    /// <summary>Alias for <see cref="Dtft"/>.</summary>
    public static ComplexFp32[] Transform(float[] input, int numPoints, ComputingContext? context = null)
        => Dtft(input, numPoints, context);

    /// <summary>
    /// Generic DTFT (float/double get SIMD when policy allows; other <typeparamref name="T"/> stay scalar).
    /// </summary>
    public static Complex<T>[] Transform<T>(T[] input, int numPoints, ComputingContext? context = null)
        where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (numPoints < 2)
            throw new ArgumentOutOfRangeException(nameof(numPoints));
        ArgumentNullException.ThrowIfNull(input);

        if (typeof(T) == typeof(float))
        {
            var fp = MemoryMarshal.Cast<T, float>(input);
            var r = Dtft(fp, numPoints, context);
            var mapped = new Complex<T>[numPoints];
            for (int k = 0; k < numPoints; k++)
                mapped[k] = new Complex<T>(T.CreateChecked(r[k].Real), T.CreateChecked(r[k].Imaginary));
            return mapped;
        }

        if (typeof(T) == typeof(double))
        {
            var dp = MemoryMarshal.Cast<T, double>(input).ToArray();
            var r = Dtft64Core(dp, numPoints, context);
            var mapped = new Complex<T>[numPoints];
            for (int k = 0; k < numPoints; k++)
                mapped[k] = new Complex<T>(T.CreateChecked(r[k].Re), T.CreateChecked(r[k].Im));
            return mapped;
        }

        var result = new Complex<T>[numPoints];
        var twoPi = Constants<T>.Two * Constants<T>.Pi;
        var nPts = T.CreateChecked(numPoints);
        int nLen = input.Length;

        ComputingContextExecution.ForEach(
            context,
            0,
            numPoints,
            k =>
            {
                T angle = -twoPi * T.CreateChecked(k) / nPts;
                var zk = new Complex<T>(T.Cos(angle), T.Sin(angle));
                var sum = Complex<T>.Zero;
                for (int n = 0; n < nLen; n++)
                    sum += input[n] * Complex<T>.Pow(zk, -n);
                result[k] = sum;
            },
            workPerItem: Math.Max(1, nLen));

        return result;
    }

    /// <summary>Evaluate H(z) for a rational discrete TF at an arbitrary complex z.</summary>
    public static ComplexFp32 Evaluate(
        ReadOnlySpan<float> b,
        ReadOnlySpan<float> a,
        ComplexFp32 z)
    {
        if (a.IsEmpty)
            throw new ArgumentException("Denominator must not be empty.", nameof(a));
        if (z == ComplexFp32.Zero)
            throw new ArgumentException("z must be non-zero for negative powers.", nameof(z));

        ComplexFp32 zInv = ComplexFp32.One / z;
        ComplexFp32 num = EvalNegPowPolyAt(b, zInv);
        ComplexFp32 den = EvalNegPowPolyAt(a, zInv);
        if (den == ComplexFp32.Zero)
            return ComplexFp32.Zero;
        return num / den;
    }

    /// <summary>Evaluate H(e^{jω}) for a rational discrete TF (ω in radians).</summary>
    public static ComplexFp32 EvaluateFrequencyResponse(float[] b, float[] a, float omega)
        => EvaluateFrequencyResponse((ReadOnlySpan<float>)b, a, omega);

    /// <summary>Evaluate H(e^{jω}) for a rational discrete TF (ω in radians).</summary>
    public static ComplexFp32 EvaluateFrequencyResponse(
        ReadOnlySpan<float> b,
        ReadOnlySpan<float> a,
        float omega)
    {
        if (a.IsEmpty)
            throw new ArgumentException("Denominator must not be empty.", nameof(a));

        float c = MathF.Cos(-omega);
        float s = MathF.Sin(-omega);
        ComplexFp32 num = EvalNegPowPoly(b, c, s);
        ComplexFp32 den = EvalNegPowPoly(a, c, s);
        if (den == ComplexFp32.Zero)
            return ComplexFp32.Zero;
        return num / den;
    }

    /// <summary>
    /// Evaluate H(e^{jω}) on a grid of ω (radians). Same ComputingContext dispatch as <see cref="Dtft"/>.
    /// </summary>
    public static ComplexFp32[] EvaluateFrequencyResponse(
        ReadOnlySpan<float> b,
        ReadOnlySpan<float> a,
        ReadOnlySpan<float> omegaRadians,
        ComputingContext? context = null)
    {
        if (a.IsEmpty)
            throw new ArgumentException("Denominator must not be empty.", nameof(a));

        float[] bb = b.ToArray();
        float[] aa = a.ToArray();
        float[] omega = omegaRadians.ToArray();
        var h = new ComplexFp32[omega.Length];
        long work = Math.Max(1, bb.Length + aa.Length);

        if (ComputingContextExecution.UseParallelIndexed(context, omega.Length, work))
        {
            ComputingContextExecution.ForEach(
                context,
                0,
                omega.Length,
                i => h[i] = EvaluateFrequencyResponse(bb, aa, omega[i]),
                workPerItem: work);
            return h;
        }

        int problemSize = TotalWork(omega.Length, work);
        if (PreferScalar(context, problemSize))
        {
            for (int i = 0; i < omega.Length; i++)
                h[i] = EvaluateFrequencyResponse(bb, aa, omega[i]);
            return h;
        }

        EvaluateFrequencyResponseSimd(bb, aa, omega, h);
        return h;
    }

    /// <summary>Discrete LTI is BIBO-stable when every pole satisfies |p| &lt; 1.</summary>
    public static bool IsStable(ReadOnlySpan<ComplexFp32> poles, float eps = 1e-7f)
    {
        for (int i = 0; i < poles.Length; i++)
        {
            float re = poles[i].Real;
            float im = poles[i].Imaginary;
            if (!float.IsFinite(re) || !float.IsFinite(im))
                return false;
            float r2 = re * re + im * im;
            if (r2 >= (1f - eps) * (1f - eps))
                return false;
        }
        return true;
    }

    /// <summary>Max pole radius max|p| (∞ if non-finite).</summary>
    public static float MaxPoleRadius(ReadOnlySpan<ComplexFp32> poles)
    {
        float max = 0f;
        for (int i = 0; i < poles.Length; i++)
        {
            float re = poles[i].Real;
            float im = poles[i].Imaginary;
            if (!float.IsFinite(re) || !float.IsFinite(im))
                return float.PositiveInfinity;
            float r = MathF.Sqrt(re * re + im * im);
            if (r > max) max = r;
        }
        return max;
    }

    /// <summary>
    /// Obsolete incorrect helper kept for binary compatibility — do not use.
    /// Prefer <see cref="Filters.Base.TransferFunction.Poles"/> / <see cref="Filters.Base.TransferFunction.Zeros"/>.
    /// </summary>
    [Obsolete("Incorrect pole/zero extraction from DTFT samples. Use TransferFunction.Poles/Zeros.")]
    public static (ComplexFp32[] poles, ComplexFp32[] zeros) GetPolesAndZeros(ComplexFp32[] zTransform)
    {
        int degree = Math.Max(0, zTransform.Length - 1);
        var poles = new ComplexFp32[degree];
        var zeros = new ComplexFp32[degree];
        for (int i = 0; i < degree; i++)
        {
            poles[i] = zTransform[i] != ComplexFp32.Zero ? 1 / zTransform[i] : ComplexFp32.Zero;
            zeros[i] = zTransform[i] == ComplexFp32.Zero ? ComplexFp32.One : ComplexFp32.Zero;
        }
        return (poles, zeros);
    }

    // ── dispatch helpers ─────────────────────────────────────────────────────

    static int TotalWork(int count, long workPerItem)
    {
        long total = workPerItem * count;
        return total > int.MaxValue ? int.MaxValue : (int)total;
    }

    /// <summary>
    /// Scalar when resolved mode is Normal, or SIMD hardware unavailable.
    /// Explicit <see cref="ComputingContext.Simd"/> / <see cref="ComputingContext.Parallel"/>
    /// (when parallel gate did not fire) select SIMD without Auto size ladders.
    /// </summary>
    static bool PreferScalar(ComputingContext? context, int problemSize)
    {
        if (!Vector.IsHardwareAccelerated)
            return true;
        return ComputingContext.Resolve(context).ResolveCpuMode(problemSize) == CpuExecutionMode.Normal;
    }

    // ── DTFT kernels ─────────────────────────────────────────────────────────

    static ComplexFp32 DtftBinScalar(float[] data, int k, int numPoints)
    {
        float angle = -2f * ConstantsFp32.PI * k / numPoints;
        float c = MathF.Cos(angle);
        float s = MathF.Sin(angle);
        float wr = 1f, wi = 0f;
        float sumR = 0f, sumI = 0f;
        for (int n = 0; n < data.Length; n++)
        {
            float x = data[n];
            sumR += x * wr;
            sumI += x * wi;
            float nr = wr * c - wi * s;
            float ni = wr * s + wi * c;
            wr = nr;
            wi = ni;
        }
        return new ComplexFp32(sumR, sumI);
    }

    static void DtftSimd(float[] data, ComplexFp32[] result, int numPoints)
    {
        int nLen = data.Length;
        int w = Vector<float>.Count;
        Span<float> cBuf = stackalloc float[w];
        Span<float> sBuf = stackalloc float[w];
        Span<float> outR = stackalloc float[w];
        Span<float> outI = stackalloc float[w];

        int k = 0;
        for (; k <= numPoints - w; k += w)
        {
            for (int lane = 0; lane < w; lane++)
            {
                float angle = -2f * ConstantsFp32.PI * (k + lane) / numPoints;
                cBuf[lane] = MathF.Cos(angle);
                sBuf[lane] = MathF.Sin(angle);
            }

            var c = new Vector<float>(cBuf);
            var s = new Vector<float>(sBuf);
            var wr = Vector<float>.One;
            var wi = Vector<float>.Zero;
            var sumR = Vector<float>.Zero;
            var sumI = Vector<float>.Zero;

            for (int n = 0; n < nLen; n++)
            {
                var x = new Vector<float>(data[n]);
                sumR += x * wr;
                sumI += x * wi;
                var nr = wr * c - wi * s;
                var ni = wr * s + wi * c;
                wr = nr;
                wi = ni;
            }

            sumR.CopyTo(outR);
            sumI.CopyTo(outI);
            for (int lane = 0; lane < w; lane++)
                result[k + lane] = new ComplexFp32(outR[lane], outI[lane]);
        }

        for (; k < numPoints; k++)
            result[k] = DtftBinScalar(data, k, numPoints);
    }

    static (double Re, double Im)[] Dtft64Core(double[] data, int numPoints, ComputingContext? context)
    {
        int nLen = data.Length;
        var result = new (double Re, double Im)[numPoints];
        long workPer = Math.Max(1, nLen);

        if (ComputingContextExecution.UseParallelIndexed(context, numPoints, workPer))
        {
            ComputingContextExecution.ForEach(
                context,
                0,
                numPoints,
                k => result[k] = Dtft64BinScalar(data, k, numPoints),
                workPerItem: workPer);
            return result;
        }

        int problemSize = TotalWork(numPoints, workPer);
        if (PreferScalar(context, problemSize) || !Vector.IsHardwareAccelerated)
        {
            for (int k = 0; k < numPoints; k++)
                result[k] = Dtft64BinScalar(data, k, numPoints);
            return result;
        }

        Dtft64Simd(data, result, numPoints);
        return result;
    }

    static (double Re, double Im) Dtft64BinScalar(double[] data, int k, int numPoints)
    {
        double angle = -2d * Math.PI * k / numPoints;
        double c = Math.Cos(angle);
        double s = Math.Sin(angle);
        double wr = 1d, wi = 0d;
        double sumR = 0d, sumI = 0d;
        for (int n = 0; n < data.Length; n++)
        {
            double x = data[n];
            sumR += x * wr;
            sumI += x * wi;
            double nr = wr * c - wi * s;
            double ni = wr * s + wi * c;
            wr = nr;
            wi = ni;
        }
        return (sumR, sumI);
    }

    static void Dtft64Simd(double[] data, (double Re, double Im)[] result, int numPoints)
    {
        int nLen = data.Length;
        int w = Vector<double>.Count;
        Span<double> cBuf = stackalloc double[w];
        Span<double> sBuf = stackalloc double[w];
        Span<double> outR = stackalloc double[w];
        Span<double> outI = stackalloc double[w];

        int k = 0;
        for (; k <= numPoints - w; k += w)
        {
            for (int lane = 0; lane < w; lane++)
            {
                double angle = -2d * Math.PI * (k + lane) / numPoints;
                cBuf[lane] = Math.Cos(angle);
                sBuf[lane] = Math.Sin(angle);
            }

            var c = new Vector<double>(cBuf);
            var s = new Vector<double>(sBuf);
            var wr = Vector<double>.One;
            var wi = Vector<double>.Zero;
            var sumR = Vector<double>.Zero;
            var sumI = Vector<double>.Zero;

            for (int n = 0; n < nLen; n++)
            {
                var x = new Vector<double>(data[n]);
                sumR += x * wr;
                sumI += x * wi;
                var nr = wr * c - wi * s;
                var ni = wr * s + wi * c;
                wr = nr;
                wi = ni;
            }

            sumR.CopyTo(outR);
            sumI.CopyTo(outI);
            for (int lane = 0; lane < w; lane++)
                result[k + lane] = (outR[lane], outI[lane]);
        }

        for (; k < numPoints; k++)
            result[k] = Dtft64BinScalar(data, k, numPoints);
    }

    static ComplexFp32 EvalNegPowPolyAt(ReadOnlySpan<float> c, ComplexFp32 zInv)
    {
        var acc = ComplexFp32.Zero;
        var w = ComplexFp32.One;
        for (int i = 0; i < c.Length; i++)
        {
            acc += new ComplexFp32(c[i], 0f) * w;
            w *= zInv;
        }
        return acc;
    }

    static ComplexFp32 EvalNegPowPoly(ReadOnlySpan<float> c, float cosNegW, float sinNegW)
    {
        float wr = 1f, wi = 0f;
        float sumR = 0f, sumI = 0f;
        for (int i = 0; i < c.Length; i++)
        {
            sumR += c[i] * wr;
            sumI += c[i] * wi;
            float nr = wr * cosNegW - wi * sinNegW;
            float ni = wr * sinNegW + wi * cosNegW;
            wr = nr;
            wi = ni;
        }
        return new ComplexFp32(sumR, sumI);
    }

    static void EvaluateFrequencyResponseSimd(float[] b, float[] a, float[] omega, ComplexFp32[] h)
    {
        int m = omega.Length;
        int w = Vector<float>.Count;
        Span<float> outR = stackalloc float[w];
        Span<float> outI = stackalloc float[w];

        int i = 0;
        for (; i <= m - w; i += w)
        {
            var omegaV = new Vector<float>(omega.AsSpan(i, w));
            // z^{-1} = cos(-ω) + j sin(-ω)
            Span<float> cBuf = stackalloc float[w];
            Span<float> sBuf = stackalloc float[w];
            omegaV.CopyTo(cBuf); // temp hold ω
            for (int lane = 0; lane < w; lane++)
            {
                float om = cBuf[lane];
                cBuf[lane] = MathF.Cos(-om);
                sBuf[lane] = MathF.Sin(-om);
            }
            var c = new Vector<float>(cBuf);
            var s = new Vector<float>(sBuf);

            EvalNegPowPolySimd(b, c, s, out var numR, out var numI);
            EvalNegPowPolySimd(a, c, s, out var denR, out var denI);

            var den2 = denR * denR + denI * denI;
            var zero = Vector<float>.Zero;
            var re = (numR * denR + numI * denI) / den2;
            var im = (numI * denR - numR * denI) / den2;
            var mask = Vector.Equals(den2, zero);
            re = Vector.ConditionalSelect(mask, zero, re);
            im = Vector.ConditionalSelect(mask, zero, im);

            re.CopyTo(outR);
            im.CopyTo(outI);
            for (int lane = 0; lane < w; lane++)
                h[i + lane] = new ComplexFp32(outR[lane], outI[lane]);
        }

        for (; i < m; i++)
            h[i] = EvaluateFrequencyResponse(b, a, omega[i]);
    }

    static void EvalNegPowPolySimd(
        ReadOnlySpan<float> coef,
        Vector<float> cosNegW,
        Vector<float> sinNegW,
        out Vector<float> sumR,
        out Vector<float> sumI)
    {
        var wr = Vector<float>.One;
        var wi = Vector<float>.Zero;
        sumR = Vector<float>.Zero;
        sumI = Vector<float>.Zero;
        for (int i = 0; i < coef.Length; i++)
        {
            var ci = new Vector<float>(coef[i]);
            sumR += ci * wr;
            sumI += ci * wi;
            var nr = wr * cosNegW - wi * sinNegW;
            var ni = wr * sinNegW + wi * cosNegW;
            wr = nr;
            wi = ni;
        }
    }
}
