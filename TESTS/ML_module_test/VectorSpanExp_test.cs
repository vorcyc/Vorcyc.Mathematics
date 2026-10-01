using Vorcyc.Mathematics.LinearAlgebra;

namespace ML_module_test;

/// <summary>
/// Validates <see cref="VectorSpan.Exp{T}"/> (vectorized expf/exp replacement for the removed
/// <c>System.Numerics.Tensors.TensorPrimitives.Exp</c> dependency) against <see cref="MathF.Exp(float)"/> /
/// <see cref="Math.Exp(double)"/> across scalar values and varied array lengths (to exercise both the
/// SIMD main loop and the scalar remainder loop).
/// </summary>
public static class VectorSpanExp_test
{
    static int _failures;

    public static void Go()
    {
        _failures = 0;
        Console.WriteLine("Testing VectorSpan.Exp against MathF.Exp/Math.Exp...");

        CheckScalarGridFloat();
        CheckScalarGridDouble();
        CheckArrayLengthsFloat();
        CheckArrayLengthsDouble();
        CheckRandomRangeFloat();
        CheckRandomRangeDouble();

        if (_failures != 0)
            throw new InvalidOperationException($"VectorSpan.Exp: {_failures} assertion(s) failed.");

        Console.WriteLine("VectorSpan.Exp: PASS");
    }

    static readonly float[] s_gridF =
        [-80f, -20f, -10f, -5f, -1f, -0.5f, 0f, 0.5f, 1f, 5f, 10f, 20f, 80f];

    static readonly double[] s_gridD =
        [-700d, -20d, -10d, -5d, -1d, -0.5d, 0d, 0.5d, 1d, 5d, 10d, 20d, 700d];

    static void CheckScalarGridFloat()
    {
        var input = s_gridF;
        var result = new float[input.Length];
        VectorSpan.Exp<float>(input, result);

        for (int i = 0; i < input.Length; i++)
        {
            float expected = MathF.Exp(input[i]);
            float actual = result[i];
            float tol = MathF.Max(1e-4f, MathF.Abs(expected) * 1e-5f);
            Expect($"grid-f x={input[i]}", MathF.Abs(actual - expected) <= tol,
                $"expected={expected}, actual={actual}");
        }
    }

    static void CheckScalarGridDouble()
    {
        var input = s_gridD;
        var result = new double[input.Length];
        VectorSpan.Exp<double>(input, result);

        for (int i = 0; i < input.Length; i++)
        {
            double expected = Math.Exp(input[i]);
            double actual = result[i];
            double tol = Math.Max(1e-9, Math.Abs(expected) * 1e-10);
            Expect($"grid-d x={input[i]}", Math.Abs(actual - expected) <= tol,
                $"expected={expected}, actual={actual}");
        }
    }

    static void CheckArrayLengthsFloat()
    {
        foreach (int n in new[] { 0, 1, 2, 3, 7, 8, 9, 15, 16, 17, 33, 100 })
        {
            var input = new float[n];
            for (int i = 0; i < n; i++)
                input[i] = -3f + 6f * i / Math.Max(1, n - 1);

            var result = new float[n];
            VectorSpan.Exp<float>(input, result);

            bool ok = true;
            for (int i = 0; i < n; i++)
            {
                float expected = MathF.Exp(input[i]);
                float tol = MathF.Max(1e-4f, MathF.Abs(expected) * 1e-5f);
                if (MathF.Abs(result[i] - expected) > tol)
                    ok = false;
            }
            Expect($"len-f n={n}", ok);
        }
    }

    static void CheckArrayLengthsDouble()
    {
        foreach (int n in new[] { 0, 1, 2, 3, 7, 8, 9, 15, 16, 17, 33, 100 })
        {
            var input = new double[n];
            for (int i = 0; i < n; i++)
                input[i] = -3d + 6d * i / Math.Max(1, n - 1);

            var result = new double[n];
            VectorSpan.Exp<double>(input, result);

            bool ok = true;
            for (int i = 0; i < n; i++)
            {
                double expected = Math.Exp(input[i]);
                double tol = Math.Max(1e-9, Math.Abs(expected) * 1e-10);
                if (Math.Abs(result[i] - expected) > tol)
                    ok = false;
            }
            Expect($"len-d n={n}", ok);
        }
    }

    static void CheckRandomRangeFloat()
    {
        var rng = new Random(12345);
        const int n = 10_000;
        var input = new float[n];
        for (int i = 0; i < n; i++)
            input[i] = (float)(rng.NextDouble() * 40 - 20);

        var result = new float[n];
        VectorSpan.Exp<float>(input, result);

        float maxRelErr = 0f;
        for (int i = 0; i < n; i++)
        {
            float expected = MathF.Exp(input[i]);
            float relErr = MathF.Abs(result[i] - expected) / MathF.Max(1e-30f, MathF.Abs(expected));
            maxRelErr = MathF.Max(maxRelErr, relErr);
        }
        Expect("random-f max-rel-err", maxRelErr < 1e-5f, $"maxRelErr={maxRelErr}");
    }

    static void CheckRandomRangeDouble()
    {
        var rng = new Random(54321);
        const int n = 10_000;
        var input = new double[n];
        for (int i = 0; i < n; i++)
            input[i] = rng.NextDouble() * 40 - 20;

        var result = new double[n];
        VectorSpan.Exp<double>(input, result);

        double maxRelErr = 0d;
        for (int i = 0; i < n; i++)
        {
            double expected = Math.Exp(input[i]);
            double relErr = Math.Abs(result[i] - expected) / Math.Max(1e-300, Math.Abs(expected));
            maxRelErr = Math.Max(maxRelErr, relErr);
        }
        Expect("random-d max-rel-err", maxRelErr < 1e-10, $"maxRelErr={maxRelErr}");
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
