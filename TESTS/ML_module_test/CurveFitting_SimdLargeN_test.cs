using System.Diagnostics;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.MachineLearning.CurveFitting;

namespace ML_module_test;

/// <summary>
/// 曲线拟合 SIMD 大 N 回归：
/// 1) 循环内 <c>stackalloc</c> 曾在 ~5e5 触发 <see cref="StackOverflowException"/>；
/// 2) 多项式 x→[-1,1] 后，5e7 float 系数应贴近真值。
/// 强制 <see cref="ComputingContext.Simd"/>。
/// </summary>
public static class CurveFitting_SimdLargeN_test
{
    public const int LargeN = 50_000_000;
    public const int AccuracyN = 1_000_000;

    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine($"Testing CurveFitter SIMD large-N (N={LargeN:N0}; mid accuracy N={AccuracyN:N0})...");

        AssertSimdSurvivesAndPolyExpAccuracy(LargeN);
        AssertSimdAccuracy(AccuracyN);

        if (_failures != 0)
            throw new InvalidOperationException(
                $"CurveFitter SIMD large-N: {_failures} assertion(s) failed.");

        Console.WriteLine("CurveFitter SIMD large-N: PASS");
    }

    /// <summary>大 N：不崩 + 有限；多项式/指数额外卡系数（归一化后应准）。</summary>
    static void AssertSimdSurvivesAndPolyExpAccuracy(int n)
    {
        {
            var xs = new float[n];
            var ys = new float[n];
            float denom = n - 1;
            for (int i = 0; i < n; i++)
            {
                float x = i / denom;
                xs[i] = x;
                ys[i] = 1f + 2f * x + 3f * x * x;
            }
            var sw = Stopwatch.StartNew();
            FitResult<float> r;
            try
            {
                r = CurveFitter<float>.Polynomial(xs, ys, degree: 2, ComputingContext.Simd);
            }
            catch (Exception ex)
            {
                Fail("poly-large", $"threw {ex.GetType().Name}: {ex.Message}");
                goto Exp;
            }
            sw.Stop();
            Expect("poly-large finite",
                r.Parameters.All(float.IsFinite) && float.IsFinite(r.MeanSquaredError));
            Expect("poly-large a0", MathF.Abs(r.Parameters[0] - 1f) < 2e-2f, $"a0={r.Parameters[0]}");
            Expect("poly-large a1", MathF.Abs(r.Parameters[1] - 2f) < 2e-2f, $"a1={r.Parameters[1]}");
            Expect("poly-large a2", MathF.Abs(r.Parameters[2] - 3f) < 2e-2f, $"a2={r.Parameters[2]}");
            Expect("poly-large mse", r.MeanSquaredError < 1e-6f, $"mse={r.MeanSquaredError}");
            Expect("poly-large pred",
                MathF.Abs(r.Predict(0.5f) - (1f + 1f + 0.75f)) < 2e-2f,
                $"pred={r.Predict(0.5f)}");
            Console.WriteLine($"  info  poly-large elapsed={sw.Elapsed.TotalSeconds:F2}s mse={r.MeanSquaredError:E3}");
        }

    Exp:
        {
            var xs = new float[n];
            var ys = new float[n];
            float denom = n - 1;
            for (int i = 0; i < n; i++)
            {
                float x = i / denom;
                xs[i] = x;
                ys[i] = 2f * MathF.Exp(0.5f * x);
            }
            var sw = Stopwatch.StartNew();
            FitResult<float> r;
            try
            {
                r = CurveFitter<float>.Exponential(xs, ys, ComputingContext.Simd);
            }
            catch (Exception ex)
            {
                Fail("exp-large", $"threw {ex.GetType().Name}: {ex.Message}");
                goto Log;
            }
            sw.Stop();
            Expect("exp-large finite",
                r.Parameters.All(float.IsFinite) && float.IsFinite(r.MeanSquaredError));
            Expect("exp-large a", MathF.Abs(r.Parameters[0] - 2f) < 3e-2f, $"a={r.Parameters[0]}");
            Expect("exp-large b", MathF.Abs(r.Parameters[1] - 0.5f) < 3e-2f, $"b={r.Parameters[1]}");
            Console.WriteLine($"  info  exp-large elapsed={sw.Elapsed.TotalSeconds:F2}s mse={r.MeanSquaredError:E3}");
        }

    Log:
        Survive("log", n, (xs, ys) =>
        {
            for (int i = 0; i < n; i++)
            {
                float x = (i + 1f) / n;
                xs[i] = x;
                ys[i] = 1.5f + 0.8f * MathF.Log(x);
            }
            return CurveFitter<float>.Logarithmic(xs, ys, ComputingContext.Simd);
        });

        Survive("pow", n, (xs, ys) =>
        {
            for (int i = 0; i < n; i++)
            {
                float x = (i + 1f) / n;
                xs[i] = x;
                ys[i] = 1.2f * MathF.Pow(x, 0.4f);
            }
            return CurveFitter<float>.Power(xs, ys, ComputingContext.Simd);
        });
    }

    static void Survive(string name, int n, Func<float[], float[], FitResult<float>> fit)
    {
        var xs = new float[n];
        var ys = new float[n];
        var sw = Stopwatch.StartNew();
        FitResult<float> r;
        try
        {
            r = fit(xs, ys);
        }
        catch (Exception ex)
        {
            Fail($"{name}-survive", $"threw {ex.GetType().Name}: {ex.Message}");
            return;
        }
        sw.Stop();

        Expect($"{name}-survive finite",
            r.Parameters.Length > 0
            && r.Parameters.All(float.IsFinite)
            && float.IsFinite(r.MeanSquaredError),
            $"params=[{string.Join(", ", r.Parameters)}] mse={r.MeanSquaredError}");
        Console.WriteLine(
            $"  info  {name}-survive elapsed={sw.Elapsed.TotalSeconds:F2}s mse={r.MeanSquaredError:E3}");
    }

    /// <summary>中等 N：Simd 系数贴近真值（x 已归一，float 仍可用）。</summary>
    static void AssertSimdAccuracy(int n)
    {
        {
            var xs = new float[n];
            var ys = new float[n];
            float denom = n - 1;
            for (int i = 0; i < n; i++)
            {
                float x = i / denom;
                xs[i] = x;
                ys[i] = 1f + 2f * x + 3f * x * x;
            }
            var r = CurveFitter<float>.Polynomial(xs, ys, 2, ComputingContext.Simd);
            Expect("poly-acc finite", r.Parameters.All(float.IsFinite));
            Expect("poly-acc a0", MathF.Abs(r.Parameters[0] - 1f) < 5e-3f, $"a0={r.Parameters[0]}");
            Expect("poly-acc a1", MathF.Abs(r.Parameters[1] - 2f) < 5e-3f, $"a1={r.Parameters[1]}");
            Expect("poly-acc a2", MathF.Abs(r.Parameters[2] - 3f) < 5e-3f, $"a2={r.Parameters[2]}");
            Expect("poly-acc mse", r.MeanSquaredError < 1e-8f, $"mse={r.MeanSquaredError}");
        }

        {
            var xs = new float[n];
            var ys = new float[n];
            float denom = n - 1;
            for (int i = 0; i < n; i++)
            {
                float x = i / denom;
                xs[i] = x;
                ys[i] = 2f * MathF.Exp(0.5f * x);
            }
            var r = CurveFitter<float>.Exponential(xs, ys, ComputingContext.Simd);
            Expect("exp-acc a", MathF.Abs(r.Parameters[0] - 2f) < 1e-2f, $"a={r.Parameters[0]}");
            Expect("exp-acc b", MathF.Abs(r.Parameters[1] - 0.5f) < 1e-2f, $"b={r.Parameters[1]}");
        }

        {
            var xs = new float[n];
            var ys = new float[n];
            for (int i = 0; i < n; i++)
            {
                float x = (i + 1f) / n;
                xs[i] = x;
                ys[i] = 1.5f + 0.8f * MathF.Log(x);
            }
            var r = CurveFitter<float>.Logarithmic(xs, ys, ComputingContext.Simd);
            Expect("log-acc a", MathF.Abs(r.Parameters[0] - 1.5f) < 1e-2f, $"a={r.Parameters[0]}");
            Expect("log-acc b", MathF.Abs(r.Parameters[1] - 0.8f) < 1e-2f, $"b={r.Parameters[1]}");
        }

        {
            var xs = new float[n];
            var ys = new float[n];
            for (int i = 0; i < n; i++)
            {
                float x = (i + 1f) / n;
                xs[i] = x;
                ys[i] = 1.2f * MathF.Pow(x, 0.4f);
            }
            var r = CurveFitter<float>.Power(xs, ys, ComputingContext.Simd);
            Expect("pow-acc a", MathF.Abs(r.Parameters[0] - 1.2f) < 1e-2f, $"a={r.Parameters[0]}");
            Expect("pow-acc b", MathF.Abs(r.Parameters[1] - 0.4f) < 1e-2f, $"b={r.Parameters[1]}");
        }
    }

    static void Expect(string name, bool ok, string detail = "")
    {
        if (ok)
        {
            Console.WriteLine($"  PASS  {name}");
            return;
        }
        Fail(name, detail);
    }

    static void Fail(string name, string detail)
    {
        _failures++;
        Console.WriteLine($"  FAIL  {name}{(detail.Length > 0 ? " | " + detail : "")}");
    }
}
