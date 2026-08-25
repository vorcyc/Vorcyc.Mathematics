using Vorcyc.Mathematics;
using Vorcyc.Mathematics.MachineLearning.CurveFitting;

namespace ML_module_test;

/// <summary>
/// Classic CurveFitter kernels honor CancellationToken inside hot loops (not only after FitCore).
/// </summary>
public static class CurveFitting_Cancel_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing CurveFitter cancellation...");

        AssertPreCancelled("linear", (x, y, ct) =>
            CurveFitter<float>.Linear(x, y, ComputingContext.Simd, ct));
        AssertPreCancelled("polynomial", (x, y, ct) =>
            CurveFitter<float>.Polynomial(x, y, 3, ComputingContext.Simd, ct));
        AssertPreCancelled("exponential", (x, y, ct) =>
        {
            for (int i = 0; i < y.Length; i++)
                y[i] = MathF.Max(y[i], 0.01f);
            CurveFitter<float>.Exponential(x, y, ComputingContext.Simd, ct);
        });

        AssertMidFlight("sinusoidal", (x, y, ct) =>
            CurveFitter<float>.Sinusoidal(x, y, maxIterations: 5000, ComputingContext.Normal, ct),
            n: 80_000);
        AssertMidFlight("lowess", (x, y, ct) =>
            CurveFitter<float>.LocallyWeighted(x, y, 0.3f, ComputingContext.Normal, ct),
            n: 12_000);

        if (_failures != 0)
            throw new InvalidOperationException($"CurveFitter cancel: {_failures} assertion(s) failed.");

        Console.WriteLine("CurveFitter cancellation: PASS");
    }

    static (float[] x, float[] y) MakeData(int n)
    {
        var x = new float[n];
        var y = new float[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = i * 0.01f;
            y[i] = MathF.Sin(0.2f * x[i]) + 0.1f * x[i] + 1.5f;
        }
        return (x, y);
    }

    static void AssertPreCancelled(string name, Action<float[], float[], CancellationToken> fit)
    {
        var (x, y) = MakeData(8_192);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        bool cancelled = false;
        try
        {
            fit(x, y, cts.Token);
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }
        sw.Stop();

        if (!cancelled)
        {
            _failures++;
            Console.WriteLine($"  FAIL {name}: pre-cancelled token was ignored");
            return;
        }
        if (sw.Elapsed > TimeSpan.FromSeconds(1))
        {
            _failures++;
            Console.WriteLine($"  FAIL {name}: pre-cancel too slow ({sw.ElapsedMilliseconds} ms)");
            return;
        }
        Console.WriteLine($"  OK {name}: pre-cancel {sw.ElapsedMilliseconds} ms");
    }

    static void AssertMidFlight(string name, Action<float[], float[], CancellationToken> fit, int n)
    {
        var (x, y) = MakeData(n);
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(40));
        var sw = System.Diagnostics.Stopwatch.StartNew();
        bool cancelled = false;
        try
        {
            fit(x, y, cts.Token);
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }
        sw.Stop();

        if (!cancelled)
        {
            _failures++;
            Console.WriteLine($"  FAIL {name}: completed without cancel ({sw.ElapsedMilliseconds} ms)");
            return;
        }
        if (sw.Elapsed > TimeSpan.FromSeconds(3))
        {
            _failures++;
            Console.WriteLine($"  FAIL {name}: mid-flight cancel too slow ({sw.ElapsedMilliseconds} ms)");
            return;
        }
        Console.WriteLine($"  OK {name}: mid-flight cancel {sw.ElapsedMilliseconds} ms");
    }
}
