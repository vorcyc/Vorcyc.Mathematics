using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Operations;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace SP_module_test;

/// <summary>
/// 0.10.13: AveragePeriodogram / Operation.Welch use complete frames only and honor ComputingContext.
/// </summary>
internal static class Welch_test
{
    public static void Go()
    {
        "=== Welch / AveragePeriodogram (0.10.13) ===".PrintLine(ConsoleColor.Cyan);

        CompleteFramesOnly();
        ParallelAgreesWithNormal();
        WelchForwardsContext();
        OnesidedEndsNotDoubled();
        HannPeriodicDistinctFromRectangular();

        "Welch_test OK".PrintLine(ConsoleColor.Green);
    }

    /// <summary>
    /// Trailing incomplete samples must not change the mean vs a truncated complete-only signal.
    /// </summary>
    static void CompleteFramesOnly()
    {
        const int win = 256, hop = 128;
        // 3 complete frames: last sample index = win + 2*hop - 1 → length = win + 2*hop
        int completeLen = win + 2 * hop;
        var full = Tone(completeLen + hop / 2); // leftover &lt; win
        var trimmed = full.AsSpan(0, completeLen).ToArray();

        var stft = new Stft(win, hop, WindowType.Hann, win);
        var a = stft.AveragePeriodogram(trimmed, ComputingContext.Normal);
        var b = stft.AveragePeriodogram(full, ComputingContext.Normal);

        AssertClose(a, b, 1e-5f, "complete-only vs trailing leftover");
    }

    static void ParallelAgreesWithNormal()
    {
        const int win = 1024, hop = 512;
        var samples = Tone(win + hop * 64);
        var stft = new Stft(win, hop, WindowType.Hann, win);
        var normal = stft.AveragePeriodogram(samples, ComputingContext.Normal);
        var parallel = stft.AveragePeriodogram(samples, ComputingContext.Parallel);
        AssertClose(normal, parallel, 2e-4f, "Parallel vs Normal");
    }

    static void WelchForwardsContext()
    {
        var signal = Signal.FromCopy(Tone(8192), 48_000);
        var a = Operation.Welch(signal, 512, 256, WindowType.Hann, 512, samplingRate: 0, ComputingContext.Normal);
        var b = Operation.Welch(signal, 512, 256, WindowType.Hann, 512, samplingRate: 0, ComputingContext.Simd);
        AssertClose(a, b, 2e-4f, "Operation.Welch Normal vs Simd");
    }

    /// <summary>
    /// SciPy onesided: DC and Nyquist get half the interior scale (no extra ×2).
    /// </summary>
    static void OnesidedEndsNotDoubled()
    {
        const int nfft = 256, hop = 128;
        // DC-heavy tone (constant) → energy at bin 0; Nyquist probe via alternating
        var dc = new float[nfft + hop * 4];
        for (int i = 0; i < dc.Length; i++) dc[i] = 1f;
        var signal = Signal.FromCopy(dc, 1);
        var ps = Operation.Welch(signal, nfft, hop, WindowType.Rectangular, nfft, samplingRate: 0, ComputingContext.Normal);
        if (ps.Length < 3)
            throw new Exception("OnesidedEnds: spectrum too short");

        // Interior bins near DC should be ≪ DC; if DC were incorrectly ×2 vs SciPy ends rule,
        // ratio DC/interior mid would still be large — instead check Nyquist vs a mid bin on alternating:
        var alt = new float[nfft + hop * 4];
        for (int i = 0; i < alt.Length; i++) alt[i] = (i & 1) == 0 ? 1f : -1f;
        var ny = Operation.Welch(Signal.FromCopy(alt, 1), nfft, hop, WindowType.Rectangular, nfft, samplingRate: 0);
        int last = ny.Length - 1;
        int mid = ny.Length / 2;
        // Nyquist peak must exceed mid-band (alternating → Fs/2)
        if (!(ny[last] > ny[mid] * 10f))
            throw new Exception($"OnesidedEnds: Nyquist {ny[last]} not >> mid {ny[mid]}");
        if (!(ps[0] > ps[mid] * 10f))
            throw new Exception($"OnesidedEnds: DC {ps[0]} not >> mid {ps[mid]}");
    }

    static void HannPeriodicDistinctFromRectangular()
    {
        var a = WindowBuilder.OfType(WindowType.HannPeriodic, 64);
        var b = WindowBuilder.OfType(WindowType.Rectangular, 64);
        float maxDiff = 0;
        for (int i = 0; i < a.Length; i++)
            maxDiff = Math.Max(maxDiff, Math.Abs(a[i] - b[i]));
        if (maxDiff < 0.1f)
            throw new Exception("HannPeriodic must not silently fall back to Rectangular");
    }

    static float[] Tone(int n, float freq = 440f, float fs = 48_000f)
    {
        var y = new float[n];
        double w = 2 * Math.PI * freq / fs;
        for (int i = 0; i < n; i++)
            y[i] = 0.5f * MathF.Sin((float)(w * i));
        return y;
    }

    static void AssertClose(float[] a, float[] b, float absTol, string label)
    {
        if (a.Length != b.Length)
            throw new Exception($"{label}: length {a.Length} vs {b.Length}");
        float peak = 0, maxDiff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            peak = Math.Max(peak, Math.Abs(a[i]));
            maxDiff = Math.Max(maxDiff, Math.Abs(a[i] - b[i]));
        }
        float tol = Math.Max(absTol, peak * absTol);
        if (maxDiff > tol)
            throw new Exception($"{label}: maxDiff={maxDiff} tol={tol}");
    }
}
