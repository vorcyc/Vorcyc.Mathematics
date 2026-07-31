using Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

namespace SP_module_test;

public static class HHT_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing HilbertHuangTransform...");

        AssertAnalyzeTone();
        AssertAnalyzeModesOnly();

        if (_failures != 0)
            throw new InvalidOperationException($"HHT: {_failures} assertion(s) failed.");

        Console.WriteLine("HilbertHuangTransform: PASS");
    }

    static void AssertAnalyzeTone()
    {
        int n = 512;
        float fs = 512f;
        float f0 = 8f;
        var x = new float[n];
        for (int i = 0; i < n; i++)
            x[i] = MathF.Sin(2f * MathF.PI * f0 * i / fs);

        var r = HilbertHuangTransform.Analyze<float>(x, fs, new HhtOptions
        {
            EmdOptions = new EmdOptions { MaxImf = 4, MaxSiftIterations = 40 },
        });

        Expect("modes>=1", r.ModeCount >= 1);
        Expect("amp len", r.InstantaneousAmplitudes[0].Length == n);
        Expect("freq len", r.InstantaneousFrequenciesHz[0].Length == n);
        Expect("finite amp", r.InstantaneousAmplitudes.All(a => a.All(float.IsFinite)));
        Expect("finite freq", r.InstantaneousFrequenciesHz.All(f => f.All(float.IsFinite)));

        // Median instantaneous frequency of first IMF near f0
        var freqs = r.InstantaneousFrequenciesHz[0];
        var mid = freqs.Skip(n / 8).Take(n / 2).OrderBy(v => v).ToArray();
        float med = mid[mid.Length / 2];
        Expect("inst f ~8Hz", Math.Abs(med - f0) < 2.5f, $"med={med}");
    }

    static void AssertAnalyzeModesOnly()
    {
        int n = 256;
        float fs = 256f;
        var mode = new double[n];
        for (int i = 0; i < n; i++)
            mode[i] = Math.Cos(2 * Math.PI * 10 * i / fs);

        var r = ModeDecomposer.Instantaneous<double>([mode], fs);
        Expect("one mode", r.ModeCount == 1);

        var mid = r.InstantaneousFrequenciesHz[0].Skip(20).Take(100).OrderBy(v => v).ToArray();
        double med = mid[mid.Length / 2];
        Expect("f~10", Math.Abs(med - 10) < 1.5, $"med={med}");

        var ampMid = r.InstantaneousAmplitudes[0].Skip(20).Take(100).Average();
        Expect("amp~1", Math.Abs(ampMid - 1) < 0.15, $"amp={ampMid}");
    }

    static void Expect(string name, bool ok, string? detail = null)
    {
        if (ok) return;
        _failures++;
        Console.WriteLine($"  FAIL: {name}" + (detail is null ? "" : $" ({detail})"));
    }
}
