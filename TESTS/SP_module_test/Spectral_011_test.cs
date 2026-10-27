using Vorcyc.Mathematics.SignalProcessing.Operations;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace SP_module_test;

internal static class Spectral_011_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing 0.11.0 spectral estimates...");
        AutoSpectrumMatchesWelch();
        CoherenceAndTransfer();
        ParametricPeak();
        MultitaperPeak();
        if (_failures != 0)
            throw new InvalidOperationException($"Spectral 0.11.0: {_failures} assertion(s) failed.");
        Console.WriteLine("Spectral_011_test: PASS");
    }

    static void AutoSpectrumMatchesWelch()
    {
        var rng = new Random(1);
        var samples = new float[512];
        for (int i = 0; i < samples.Length; i++)
            samples[i] = (float)(rng.NextDouble() * 2 - 1);
        var signal = Signal.FromCopy(samples, 1f);
        float[] welch = Operation.Welch(signal, 128, 64, WindowType.Hann, 128, samplingRate: 0);
        var cross = Operation.Cpsd(signal, signal, 128, 64, WindowType.Hann, 128, samplingRate: 0);
        Expect(welch.Length == cross.Length, "cpsd length");
        for (int i = 0; i < welch.Length; i++)
        {
            Expect(Math.Abs(welch[i] - cross[i].Real) <= 1e-3f * (1f + Math.Abs(welch[i])), $"cpsd bin {i}: {cross[i].Real} vs {welch[i]}");
            Expect(Math.Abs(cross[i].Imaginary) <= 1e-3f * (1f + Math.Abs(welch[i])), $"cpsd imag bin {i}");
        }
    }

    static void CoherenceAndTransfer()
    {
        var rng = new Random(2);
        var x = new float[512];
        var y = new float[512];
        for (int i = 0; i < x.Length; i++)
        {
            x[i] = (float)(rng.NextDouble() * 2 - 1);
            y[i] = 0.5f * x[i];
        }

        var input = Signal.FromCopy(x, 1f);
        var output = Signal.FromCopy(y, 1f);
        float[] coherence = Operation.Mscohere(input, output, 128, 64, WindowType.Hann, 128);
        var response = Operation.TfEstimate(input, output, 128, 64, WindowType.Hann, 128);
        for (int i = 0; i < coherence.Length; i++)
        {
            Expect(Math.Abs(coherence[i] - 1f) < 1e-4f, $"coherence {coherence[i]}");
            Expect(Math.Abs(response[i].Real - 0.5f) < 1e-3f, $"tf real {response[i].Real}");
            Expect(Math.Abs(response[i].Imaginary) < 1e-3f, $"tf imag {response[i].Imaginary}");
        }
    }

    static void ParametricPeak()
    {
        const double phi1 = 1.2;
        const double phi2 = -0.7;
        const int fftSize = 256;
        var rng = new Random(3);
        var generated = new double[3500];
        for (int t = 2; t < generated.Length; t++)
            generated[t] = phi1 * generated[t - 1] + phi2 * generated[t - 2] + Gaussian(rng);
        var samples = new float[3000];
        for (int i = 0; i < samples.Length; i++)
            samples[i] = (float)generated[500 + i];
        var signal = Signal.FromCopy(samples, 1f);
        int expected = TrueAr2Peak(phi1, phi2, fftSize);
        ExpectNear(Operation.Pyulear(signal, 2, fftSize), expected, "pyulear");
        ExpectNear(Operation.Pburg(signal, 2, fftSize), expected, "pburg");
        ExpectNear(Operation.Pcov(signal, 2, fftSize), expected, "pcov");
    }

    static void MultitaperPeak()
    {
        var samples = new float[256];
        for (int i = 0; i < samples.Length; i++)
            samples[i] = MathF.Sin(2f * MathF.PI * 8f * i / 256f);
        float[] spectrum = Operation.Pmtm(Signal.FromCopy(samples, 1f), timeBandwidth: 3.5, taperCount: 5, windowSize: 256, fftSize: 256);
        int peak = ArgMax(spectrum);
        Expect(Math.Abs(peak - 8) <= 1, $"pmtm peak {peak}");
        float median = Median(spectrum);
        Expect(spectrum[peak] > 20f * Math.Max(median, 1e-8f), "pmtm peak stands out");
    }

    static void ExpectNear(float[] spectrum, int expected, string name)
    {
        int peak = ArgMax(spectrum);
        Expect(Math.Abs(peak - expected) <= 3, $"{name} peak {peak}, expected {expected}");
    }

    static int TrueAr2Peak(double phi1, double phi2, int fftSize)
    {
        int bins = fftSize / 2 + 1;
        int best = 0;
        double bestValue = double.NegativeInfinity;
        for (int bin = 0; bin < bins; bin++)
        {
            double omega = 2 * Math.PI * bin / fftSize;
            double real = 1 - phi1 * Math.Cos(omega) - phi2 * Math.Cos(2 * omega);
            double imag = phi1 * Math.Sin(omega) + phi2 * Math.Sin(2 * omega);
            double power = 1.0 / (real * real + imag * imag);
            if (power > bestValue)
            {
                bestValue = power;
                best = bin;
            }
        }

        return best;
    }

    static int ArgMax(float[] values)
    {
        int best = 0;
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > values[best])
                best = i;
        }

        return best;
    }

    static float Median(float[] values)
    {
        var copy = (float[])values.Clone();
        Array.Sort(copy);
        return copy[copy.Length / 2];
    }

    static double Gaussian(Random random)
    {
        double u1 = 1 - random.NextDouble();
        double u2 = 1 - random.NextDouble();
        return Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(2 * Math.PI * u2);
    }

    static void Expect(bool condition, string message)
    {
        if (condition)
            return;
        _failures++;
        Console.WriteLine("FAIL: " + message);
    }
}
