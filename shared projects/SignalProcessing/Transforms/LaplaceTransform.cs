using System.Numerics;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms;

/// <summary>
/// Continuous-time Laplace helpers for rational transfer functions H(s).
/// Coefficient convention: ascending powers of s —
/// <c>b[0] + b[1]·s + b[2]·s² + …</c> over <c>a[0] + a[1]·s + …</c>.
/// </summary>
public static class LaplaceTransform
{
    /// <summary>Evaluate H(s) = num(s) / den(s) at a complex s.</summary>
    public static ComplexFp32 Evaluate(
        ReadOnlySpan<float> numerator,
        ReadOnlySpan<float> denominator,
        ComplexFp32 s)
    {
        if (denominator.IsEmpty)
            throw new ArgumentException("Denominator must not be empty.", nameof(denominator));

        var num = EvalPoly(numerator, s);
        var den = EvalPoly(denominator, s);
        if (den == ComplexFp32.Zero)
            return ComplexFp32.Zero;
        return num / den;
    }

    /// <summary>
    /// Frequency response H(jω) for ω in rad/s.
    /// Honors <paramref name="context"/> / <see cref="ComputingScope"/>:
    /// parallel over ω when the shared parallel gate fires; otherwise scalar or ω-lane SIMD
    /// (explicit <see cref="ComputingContext.Simd"/> selects SIMD regardless of Auto size ladders).
    /// </summary>
    public static ComplexFp32[] FrequencyResponse(
        ReadOnlySpan<float> numerator,
        ReadOnlySpan<float> denominator,
        ReadOnlySpan<float> omegaRadPerSec,
        ComputingContext? context = null)
    {
        if (denominator.IsEmpty)
            throw new ArgumentException("Denominator must not be empty.", nameof(denominator));

        float[] num = numerator.ToArray();
        float[] den = denominator.ToArray();
        float[] omega = omegaRadPerSec.ToArray();
        var h = new ComplexFp32[omega.Length];
        long work = Math.Max(1, num.Length + den.Length);

        if (ComputingContextExecution.UseParallelIndexed(context, omega.Length, work))
        {
            ComputingContextExecution.ForEach(
                context,
                0,
                omega.Length,
                i =>
                {
                    var s = new ComplexFp32(0f, omega[i]);
                    h[i] = Evaluate(num, den, s);
                },
                workPerItem: work);
            return h;
        }

        int problemSize = TotalWork(omega.Length, work);
        if (PreferScalar(context, problemSize))
        {
            for (int i = 0; i < omega.Length; i++)
            {
                var s = new ComplexFp32(0f, omega[i]);
                h[i] = Evaluate(num, den, s);
            }
            return h;
        }

        FrequencyResponseSimd(num, den, omega, h);
        return h;
    }

    static int TotalWork(int count, long workPerItem)
    {
        long total = workPerItem * count;
        return total > int.MaxValue ? int.MaxValue : (int)total;
    }

    static bool PreferScalar(ComputingContext? context, int problemSize)
    {
        if (!Vector.IsHardwareAccelerated)
            return true;
        return ComputingContext.Resolve(context).ResolveCpuMode(problemSize) == CpuExecutionMode.Normal;
    }

    static void FrequencyResponseSimd(float[] num, float[] den, float[] omega, ComplexFp32[] h)
    {
        int m = omega.Length;
        int w = Vector<float>.Count;
        Span<float> outR = stackalloc float[w];
        Span<float> outI = stackalloc float[w];

        int i = 0;
        for (; i <= m - w; i += w)
        {
            var omegaV = new Vector<float>(omega.AsSpan(i, w));
            EvalPolySimd(num, omegaV, out var numR, out var numI);
            EvalPolySimd(den, omegaV, out var denR, out var denI);

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
            h[i] = Evaluate(num, den, new ComplexFp32(0f, omega[i]));
    }

    /// <summary>Horner for H at s = jω (vectorized over ω lanes).</summary>
    static void EvalPolySimd(
        ReadOnlySpan<float> c,
        Vector<float> omega,
        out Vector<float> accR,
        out Vector<float> accI)
    {
        if (c.Length == 0)
        {
            accR = Vector<float>.Zero;
            accI = Vector<float>.Zero;
            return;
        }

        accR = new Vector<float>(c[^1]);
        accI = Vector<float>.Zero;
        for (int p = c.Length - 2; p >= 0; p--)
        {
            // acc *= jω  →  (-accI*ω, accR*ω) then + c[p]
            var newR = -accI * omega;
            var newI = accR * omega;
            accR = newR + new Vector<float>(c[p]);
            accI = newI;
        }
    }

    /// <summary>Analog LTI is BIBO-stable when every finite pole has strictly negative real part.</summary>
    public static bool IsStable(ReadOnlySpan<ComplexFp32> poles, float eps = 1e-7f)
    {
        for (int i = 0; i < poles.Length; i++)
        {
            if (!float.IsFinite(poles[i].Real) || !float.IsFinite(poles[i].Imaginary))
                return false;
            if (poles[i].Real >= -eps)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Bilinear map s → z = (1+s)/(1−s) in place (same convention as <see cref="VMath.BilinearTransform(float[], float[])"/>).
    /// Typically applied after frequency pre-warping / scaling of analog poles and zeros.
    /// </summary>
    public static void BilinearMapInPlace(Span<float> re, Span<float> im)
    {
        if (re.Length != im.Length)
            throw new ArgumentException("Real/imag lengths must match.");

        // Reuse core kernel via arrays when possible; for spans copy small buffers.
        var reA = re.ToArray();
        var imA = im.ToArray();
        VMath.BilinearTransform(reA, imA);
        reA.CopyTo(re);
        imA.CopyTo(im);
    }

    /// <summary>
    /// Map analog poles/zeros through bilinear transform with optional cutoff pre-warp.
    /// <paramref name="normalizedCutoff"/> is digital normalized frequency in (0, 0.5);
    /// when null, uses unwarped s (caller already scaled).
    /// When <paramref name="analogZeros"/> is empty, places digital zeros at <c>z = −1</c>
    /// (same relative-degree convention as <c>DesignFilter.IirLpTf</c>).
    /// </summary>
    public static (ComplexFp32[] Zeros, ComplexFp32[] Poles) BilinearMap(
        ReadOnlySpan<ComplexFp32> analogZeros,
        ReadOnlySpan<ComplexFp32> analogPoles,
        float? normalizedCutoff = null)
    {
        float scale = 1f;
        if (normalizedCutoff is float fc)
        {
            if (fc <= 0f || fc >= 0.5f)
                throw new ArgumentOutOfRangeException(nameof(normalizedCutoff), "Expected in (0, 0.5).");
            scale = MathF.Tan(ConstantsFp32.PI * fc);
        }

        MapScaled(analogPoles, scale, out var pRe, out var pIm);
        VMath.BilinearTransform(pRe, pIm);

        float[] zRe, zIm;
        if (analogZeros.Length == 0)
        {
            // Match DesignFilter convention: place zeros at z = −1 for relative degree.
            int n = analogPoles.Length;
            zRe = Enumerable.Repeat(-1f, n).ToArray();
            zIm = new float[n];
        }
        else
        {
            MapScaled(analogZeros, scale, out zRe, out zIm);
            VMath.BilinearTransform(zRe, zIm);
        }

        return (ToComplex(zRe, zIm), ToComplex(pRe, pIm));
    }

    /// <summary>
    /// Inverse bilinear map z → s = (z−1)/(z+1) in place (undoes <see cref="BilinearMapInPlace"/>).
    /// Points with z ≈ −1 map to infinity (non-finite); callers should drop or treat as analog zeros at ∞.
    /// </summary>
    public static void InverseBilinearMapInPlace(Span<float> re, Span<float> im)
    {
        if (re.Length != im.Length)
            throw new ArgumentException("Real/imag lengths must match.");

        for (int k = 0; k < re.Length; k++)
        {
            // s = (z-1)/(z+1)
            float zr = re[k], zi = im[k];
            float denR = zr + 1f;
            float denI = zi;
            float den2 = denR * denR + denI * denI;
            if (den2 < 1e-20f)
            {
                re[k] = float.PositiveInfinity;
                im[k] = float.NaN;
                continue;
            }
            float numR = zr - 1f;
            float numI = zi;
            re[k] = (numR * denR + numI * denI) / den2;
            im[k] = (numI * denR - numR * denI) / den2;
        }
    }

    /// <summary>
    /// Inverse bilinear on poles/zeros. Entries that were at z=−1 become non-finite and are omitted
    /// from the returned arrays (analog zeros at infinity).
    /// Optional <paramref name="normalizedCutoff"/> undoes pre-warp scaling (divide by tan(π f_c)).
    /// </summary>
    public static (ComplexFp32[] Zeros, ComplexFp32[] Poles) InverseBilinearMap(
        ReadOnlySpan<ComplexFp32> digitalZeros,
        ReadOnlySpan<ComplexFp32> digitalPoles,
        float? normalizedCutoff = null)
    {
        float scale = 1f;
        if (normalizedCutoff is float fc)
        {
            if (fc <= 0f || fc >= 0.5f)
                throw new ArgumentOutOfRangeException(nameof(normalizedCutoff), "Expected in (0, 0.5).");
            scale = MathF.Tan(ConstantsFp32.PI * fc);
        }

        var poles = InverseAndUnscale(digitalPoles, scale);
        var zeros = InverseAndUnscale(digitalZeros, scale);
        return (zeros, poles);
    }

    static ComplexFp32[] InverseAndUnscale(ReadOnlySpan<ComplexFp32> src, float scale)
    {
        var re = new float[src.Length];
        var im = new float[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            re[i] = src[i].Real;
            im[i] = src[i].Imaginary;
        }
        InverseBilinearMapInPlace(re, im);
        var list = new List<ComplexFp32>(src.Length);
        for (int i = 0; i < re.Length; i++)
        {
            if (!float.IsFinite(re[i]) || !float.IsFinite(im[i]))
                continue;
            list.Add(new ComplexFp32(re[i] / scale, im[i] / scale));
        }
        return list.ToArray();
    }

    static void MapScaled(ReadOnlySpan<ComplexFp32> src, float scale, out float[] re, out float[] im)
    {
        re = new float[src.Length];
        im = new float[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            var v = scale * src[i];
            re[i] = v.Real;
            im[i] = v.Imaginary;
        }
    }

    static ComplexFp32[] ToComplex(float[] re, float[] im)
    {
        var c = new ComplexFp32[re.Length];
        for (int i = 0; i < re.Length; i++)
            c[i] = new ComplexFp32(re[i], im[i]);
        return c;
    }

    static ComplexFp32 EvalPoly(ReadOnlySpan<float> c, ComplexFp32 s)
    {
        // Horner: c0 + s*(c1 + s*(c2 + …))
        if (c.Length == 0)
            return ComplexFp32.Zero;
        var acc = new ComplexFp32(c[^1], 0f);
        for (int i = c.Length - 2; i >= 0; i--)
            acc = acc * s + new ComplexFp32(c[i], 0f);
        return acc;
    }
}
