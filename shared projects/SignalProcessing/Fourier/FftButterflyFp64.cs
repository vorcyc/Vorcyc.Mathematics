using System.Buffers;
using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

/// <summary>
/// <para>
/// Shared SoA (structure-of-arrays) radix-2 butterfly kernel used by <see cref="Fft64"/> and
/// <see cref="RealFft64"/>. Operates in-place on separate real/imaginary <see cref="double"/> spans.
/// </para>
/// <para>
/// Three execution paths are provided and selected via <see cref="CpuExecutionMode"/>:
/// scalar (Normal), <see cref="Vector{T}"/> SIMD, and parallel. All paths produce identical
/// results (up to floating-point rounding) to the original scalar decimation-in-frequency FFT.
/// This is the <see cref="double"/> counterpart of <see cref="FftButterflyFp32"/>.
/// </para>
/// </summary>
internal static class FftButterflyFp64
{
    /// <summary>
    /// Minimum transform length before the parallel path is used, even when explicitly requested.
    /// Below this the threading overhead dominates. Mirrors <see cref="FftButterflyFp32"/>.
    /// </summary>
    private const int ParallelMinSize = 16384;

    /// <summary>
    /// Minimum transform length before the SIMD path is used, even when explicitly requested.
    /// <see cref="Vector{Double}"/> holds half as many lanes as the float variant, so the crossover
    /// sits a little higher; measured, staying scalar below ~8K avoids a per-stage twiddle-setup loss.
    /// </summary>
    private const int SimdMinSize = 8192;

    /// <summary>
    /// Returns true when the given <paramref name="mode"/> and <paramref name="size"/> would take an
    /// accelerated (SIMD or parallel) path. When false, callers should use their own scalar routine —
    /// this kernel's scalar path carries extra twiddle-buffer overhead and is slower than a
    /// hand-written inline scalar FFT.
    /// </summary>
    public static bool WillAccelerate(CpuExecutionMode mode, int size)
    {
        if (!Vector.IsHardwareAccelerated || size < 2)
        {
            return false;
        }

        return mode switch
        {
            CpuExecutionMode.Parallel => size >= ParallelMinSize || size >= SimdMinSize,
            CpuExecutionMode.Simd => size >= SimdMinSize,
            _ => false,
        };
    }

    /// <summary>
    /// Performs the in-place radix-2 butterfly stages followed by the bit-reversal permutation.
    /// </summary>
    /// <param name="re">Real parts (length >= <paramref name="size"/>).</param>
    /// <param name="im">Imaginary parts (length >= <paramref name="size"/>).</param>
    /// <param name="size">Transform length, a power of two.</param>
    /// <param name="inverse">When true, uses the inverse twiddle sign.</param>
    /// <param name="mode">Resolved CPU execution mode.</param>
    /// <param name="context">Optional policy used for the parallel worker count.</param>
    public static void Transform(
        double[] re,
        double[] im,
        int size,
        bool inverse,
        CpuExecutionMode mode,
        ComputingContext? context = null)
    {
        if (size < 2)
        {
            return;
        }

        bool simdOk = Vector.IsHardwareAccelerated && size >= SimdMinSize;

        switch (mode)
        {
            case CpuExecutionMode.Parallel when size >= ParallelMinSize:
                Stages(re, im, size, inverse, useSimd: Vector.IsHardwareAccelerated, useParallel: true, context);
                break;
            case CpuExecutionMode.Simd when simdOk:
            case CpuExecutionMode.Parallel when simdOk:
                // Explicit Simd, or Parallel below the parallel-worthwhile size: use SIMD.
                Stages(re, im, size, inverse, useSimd: true, useParallel: false, context);
                break;
            default:
                // Small sizes (or no HW acceleration): scalar is fastest.
                Stages(re, im, size, inverse, useSimd: false, useParallel: false, context);
                break;
        }

        BitReverse(re, im, size);
    }

    /// <summary>
    /// Runs all decimation-in-frequency stages. Each stage rewrites disjoint index ranges, so the
    /// block loop is embarrassingly parallel and the inner twiddle loop is contiguous (SoA), which
    /// vectorizes without gather/scatter.
    /// </summary>
    private static void Stages(
        double[] re,
        double[] im,
        int size,
        bool inverse,
        bool useSimd,
        bool useParallel,
        ComputingContext? context)
    {
        // Per-stage twiddle scratch (contiguous, regenerated each stage via recurrence).
        int half = size >> 1;
        double[] wReBuf = ArrayPool<double>.Shared.Rent(half);
        double[] wImBuf = ArrayPool<double>.Shared.Rent(half);

        try
        {
            for (int L = size; L >= 2; L >>= 1)
            {
                int l = L >> 1;

                BuildStageTwiddles(wReBuf, wImBuf, l, inverse);

                if (useParallel && (size / L) > 1)
                {
                    int blockCount = size / L;
                    int stageL = L;
                    int stageHalf = l;
                    bool stageSimd = useSimd;
                    ComputingContextExecution.ForEach(
                        context,
                        0,
                        blockCount,
                        b =>
                        {
                            int baseIndex = b * stageL;
                            Butterfly(re, im, wReBuf, wImBuf, baseIndex, stageHalf, stageSimd);
                        },
                        workPerItem: l);
                }
                else
                {
                    for (int baseIndex = 0; baseIndex < size; baseIndex += L)
                    {
                        Butterfly(re, im, wReBuf, wImBuf, baseIndex, l, useSimd);
                    }
                }
            }
        }
        finally
        {
            ArrayPool<double>.Shared.Return(wReBuf);
            ArrayPool<double>.Shared.Return(wImBuf);
        }
    }

    /// <summary>
    /// Fills <paramref name="wRe"/>/<paramref name="wIm"/> with the twiddle factors for a stage whose
    /// half-length is <paramref name="l"/>, using the same complex-rotation recurrence as the original
    /// scalar FFT so results stay bit-compatible.
    /// </summary>
    private static void BuildStageTwiddles(double[] wRe, double[] wIm, int l, bool inverse)
    {
        double phi = Math.PI / l;
        double c = Math.Cos(phi);
        double s = inverse ? Math.Sin(phi) : -Math.Sin(phi);

        double u1 = 1.0;
        double u2 = 0.0;
        for (int j = 0; j < l; j++)
        {
            wRe[j] = u1;
            wIm[j] = u2;
            double u3 = u1 * c - u2 * s;
            u2 = u2 * c + u1 * s;
            u1 = u3;
        }
    }

    /// <summary>
    /// Applies the butterfly to one block [<paramref name="baseIndex"/>, baseIndex + 2*l).
    /// The i-run and p-run are each contiguous, enabling straight SIMD loads/stores.
    /// </summary>
    private static void Butterfly(
        Span<double> re,
        Span<double> im,
        double[] wRe,
        double[] wIm,
        int baseIndex,
        int l,
        bool useSimd)
    {
        int j = 0;

        if (useSimd && l >= Vector<double>.Count)
        {
            int w = Vector<double>.Count;
            int simdEnd = l - (l % w);
            for (; j < simdEnd; j += w)
            {
                int i = baseIndex + j;
                int p = i + l;

                var vReI = new Vector<double>(re.Slice(i, w));
                var vImI = new Vector<double>(im.Slice(i, w));
                var vReP = new Vector<double>(re.Slice(p, w));
                var vImP = new Vector<double>(im.Slice(p, w));
                var vU1 = new Vector<double>(wRe.AsSpan(j, w));
                var vU2 = new Vector<double>(wIm.AsSpan(j, w));

                var t1 = vReI + vReP;
                var t2 = vImI + vImP;
                var t3 = vReI - vReP;
                var t4 = vImI - vImP;

                (t3 * vU1 - t4 * vU2).CopyTo(re.Slice(p, w));
                (t4 * vU1 + t3 * vU2).CopyTo(im.Slice(p, w));
                t1.CopyTo(re.Slice(i, w));
                t2.CopyTo(im.Slice(i, w));
            }
        }

        for (; j < l; j++)
        {
            int i = baseIndex + j;
            int p = i + l;

            double u1 = wRe[j];
            double u2 = wIm[j];
            double t1 = re[i] + re[p];
            double t2 = im[i] + im[p];
            double t3 = re[i] - re[p];
            double t4 = im[i] - im[p];
            re[p] = t3 * u1 - t4 * u2;
            im[p] = t4 * u1 + t3 * u2;
            re[i] = t1;
            im[i] = t2;
        }
    }

    /// <summary>
    /// In-place bit-reversal permutation (identical to the original scalar FFT's reorder step).
    /// </summary>
    private static void BitReverse(Span<double> re, Span<double> im, int size)
    {
        int M = size >> 1;
        int S = size - 1;
        for (int i = 0, j = 0; i < S; i++)
        {
            if (i > j)
            {
                (re[j], re[i]) = (re[i], re[j]);
                (im[j], im[i]) = (im[i], im[j]);
            }
            int k = M;
            while (j >= k)
            {
                j -= k;
                k >>= 1;
            }
            j += k;
        }
    }
}
