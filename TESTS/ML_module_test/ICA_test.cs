using Vorcyc.Mathematics.MachineLearning.Decomposition;

namespace ML_module_test;

internal class ICA_test
{
    public static void Go()
    {
        Console.WriteLine("Testing FastICA blind source separation...");

        int n = 2000;
        var s1 = new double[n];
        var s2 = new double[n];
        for (int t = 0; t < n; t++)
        {
            s1[t] = Math.Sin(t * 0.02);             // sine wave
            s2[t] = Math.Sign(Math.Sin(t * 0.047)); // square wave
        }

        // Known mixing matrix: each observed signal is a linear mix of the two sources.
        double a11 = 0.60, a12 = 0.40, a21 = 0.35, a22 = 0.75;
        var data = new double[n, 2];
        for (int t = 0; t < n; t++)
        {
            data[t, 0] = a11 * s1[t] + a12 * s2[t];
            data[t, 1] = a21 * s1[t] + a22 * s2[t];
        }

        var nonlinearities = new[] { ICANonlinearity.LogCosh, ICANonlinearity.Exp, ICANonlinearity.Cube };
        var algorithms = new[] { ICAAlgorithm.Symmetric, ICAAlgorithm.Deflation };

        bool allPass = true;
        foreach (var algorithm in algorithms)
        {
            foreach (var nonlinearity in nonlinearities)
            {
                bool pass = RunCase(data, s1, s2, nonlinearity, algorithm);
                allPass &= pass;
            }
        }

        Console.WriteLine();
        Console.WriteLine(allPass
            ? "OVERALL RESULT: PASS (all nonlinearity x algorithm combinations recovered the sources)"
            : "OVERALL RESULT: FAIL");
    }

    private static bool RunCase(double[,] data, double[] s1, double[] s2, ICANonlinearity nonlinearity, ICAAlgorithm algorithm)
    {
        var ica = new ICA<double>(data, nonlinearity: nonlinearity, algorithm: algorithm, randomSeed: 7);
        var recovered = ica.Transform(); // [n, 2]

        var comp0 = GetColumn(recovered, 0);
        var comp1 = GetColumn(recovered, 1);

        double c0s1 = Math.Abs(Correlation(comp0, s1));
        double c0s2 = Math.Abs(Correlation(comp0, s2));
        double c1s1 = Math.Abs(Correlation(comp1, s1));
        double c1s2 = Math.Abs(Correlation(comp1, s2));

        double matchS1 = Math.Max(c0s1, c1s1);
        double matchS2 = Math.Max(c0s2, c1s2);
        bool pass = matchS1 > 0.95 && matchS2 > 0.95;

        Console.WriteLine(
            $"[{algorithm,-10} | {nonlinearity,-7}] Converged={ica.HasConverged,-5} Iter={ica.Iterations,-3} " +
            $"|corr| s1={matchS1:F3} s2={matchS2:F3} -> {(pass ? "PASS" : "FAIL")}");

        return pass;
    }

    private static double[] GetColumn(double[,] m, int col)
    {
        int rows = m.GetLength(0);
        var v = new double[rows];
        for (int i = 0; i < rows; i++)
            v[i] = m[i, col];
        return v;
    }

    private static double Correlation(double[] a, double[] b)
    {
        int n = a.Length;
        double ma = 0, mb = 0;
        for (int i = 0; i < n; i++) { ma += a[i]; mb += b[i]; }
        ma /= n; mb /= n;

        double cov = 0, va = 0, vb = 0;
        for (int i = 0; i < n; i++)
        {
            double da = a[i] - ma, db = b[i] - mb;
            cov += da * db; va += da * da; vb += db * db;
        }
        double denom = Math.Sqrt(va * vb);
        return denom == 0 ? 0 : cov / denom;
    }
}
