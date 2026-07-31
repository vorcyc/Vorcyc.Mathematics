using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing.Transforms.ModeDecomposition;

/// <summary>
/// Empirical Mode Decomposition (Huang et al.): sift a real signal into intrinsic mode
/// functions (IMFs) plus a residual trend.
/// </summary>
/// <remarks>
/// Envelope interpolation uses natural cubic splines with mirrored extrema at the ends
/// (Rilling-style boundary treatment). Sifting stops when the relative change of the
/// proto-IMF falls below <see cref="EmdOptions.SiftingTolerance"/> or
/// <see cref="EmdOptions.MaxSiftIterations"/> is reached.
/// </remarks>
public static class EmpiricalModeDecomposition
{
    private const int MinLength = 4;

    /// <summary>Decompose a real signal into IMFs + residual.</summary>
    public static EmdResult<T> Decompose<T>(
        ReadOnlySpan<T> signal,
        EmdOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ModeDecompositionProgress>? progress = null)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            throw new NotSupportedException("Only float and double are supported.");

        options ??= new EmdOptions();

        int n = signal.Length;
        if (n < MinLength)
        {
            return new EmdResult<T>
            {
                IntrinsicModeFunctions = Array.Empty<T[]>(),
                Residual = signal.ToArray(),
                StopReason = EmdStopReason.InputRejected,
            };
        }

        int maxImf = Math.Clamp(options.MaxImf, 1, 256);
        int maxSift = Math.Clamp(options.MaxSiftIterations, 1, 10_000);
        double tol = options.SiftingTolerance;
        if (!(tol > 0) || double.IsNaN(tol) || double.IsInfinity(tol))
            tol = 0.2;
        int minExtrema = Math.Max(2, options.MinExtremaToContinue);
        var ctx = options.ComputingContext;

        var residual = signal.ToArray();
        var imfs = new List<T[]>(capacity: Math.Min(maxImf, 16));
        var work = new T[n];
        var prev = new T[n];
        var upper = new T[n];
        var lower = new T[n];
        var maxIdx = new List<int>(n / 4 + 4);
        var minIdx = new List<int>(n / 4 + 4);

        var stop = EmdStopReason.MaxImfReached;

        for (int mode = 0; mode < maxImf; mode++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ModeDecompositionProgress
            {
                Algorithm = "EMD",
                CurrentMode = mode,
                TotalModes = maxImf,
                Iteration = 0,
                Fraction = (double)mode / maxImf,
                Message = $"IMF {mode + 1}",
            });

            CountExtrema(residual, maxIdx, minIdx);
            if (maxIdx.Count + minIdx.Count < minExtrema)
            {
                stop = EmdStopReason.ResidualTooFewExtrema;
                break;
            }

            residual.AsSpan().CopyTo(work);

            for (int iter = 0; iter < maxSift; iter++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                work.AsSpan().CopyTo(prev);
                CountExtrema(work, maxIdx, minIdx);
                if (maxIdx.Count < 2 || minIdx.Count < 2)
                    break;

                if (!TryBuildEnvelope(work, maxIdx, upper) ||
                    !TryBuildEnvelope(work, minIdx, lower))
                    break;

                SubtractMean(work, upper, lower, ctx);

                if (iter > 0 && RelativeChange(prev, work) < tol)
                    break;
            }

            var imf = (T[])work.Clone();
            imfs.Add(imf);
            SubtractInPlace(residual, imf, ctx);

            CountExtrema(residual, maxIdx, minIdx);
            if (maxIdx.Count + minIdx.Count < minExtrema)
            {
                stop = EmdStopReason.ResidualTooFewExtrema;
                break;
            }

            if (mode == maxImf - 1)
                stop = EmdStopReason.MaxImfReached;
        }

        return new EmdResult<T>
        {
            IntrinsicModeFunctions = imfs,
            Residual = residual,
            StopReason = stop,
        };
    }

    private static void CountExtrema<T>(Span<T> x, List<int> maxima, List<int> minima)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        maxima.Clear();
        minima.Clear();
        int n = x.Length;
        if (n < 3) return;

        for (int i = 1; i < n - 1; i++)
        {
            T left = x[i - 1], mid = x[i], right = x[i + 1];
            if (mid > left && mid > right)
                maxima.Add(i);
            else if (mid < left && mid < right)
                minima.Add(i);
        }
    }

    /// <summary>
    /// Natural cubic spline through mirrored extrema → evaluate on every sample index.
    /// </summary>
    private static bool TryBuildEnvelope<T>(
        Span<T> signal,
        List<int> extrema,
        Span<T> envelope)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = signal.Length;
        if (extrema.Count < 2)
            return false;

        int e0 = extrema[0];
        int e1 = extrema[^1];
        int leftIdx = -e0;
        int rightIdx = 2 * (n - 1) - e1;

        int m = extrema.Count + 2;
        var xs = new double[m];
        var ys = new double[m];

        xs[0] = leftIdx;
        ys[0] = Convert.ToDouble(signal[e0]);
        for (int k = 0; k < extrema.Count; k++)
        {
            xs[k + 1] = extrema[k];
            ys[k + 1] = Convert.ToDouble(signal[extrema[k]]);
        }
        xs[^1] = rightIdx;
        ys[^1] = Convert.ToDouble(signal[e1]);

        for (int k = 1; k < m; k++)
        {
            if (!(xs[k] > xs[k - 1]))
                xs[k] = xs[k - 1] + 1e-6;
        }

        if (!NaturalCubicSpline.TryCreate(xs, ys, out var spline))
        {
            double mean = 0;
            for (int k = 0; k < ys.Length; k++) mean += ys[k];
            mean /= ys.Length;
            envelope.Fill(T.CreateChecked(mean));
            return true;
        }

        for (int i = 0; i < n; i++)
            envelope[i] = T.CreateChecked(spline.Evaluate(i));
        return true;
    }

    private static void SubtractMean<T>(
        T[] h, T[] upper, T[] lower, ComputingContext? ctx)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = h.Length;
        T half = T.CreateChecked(0.5);
        ComputingContextExecution.ForEach(ctx, 0, n, i =>
        {
            T mean = (upper[i] + lower[i]) * half;
            h[i] -= mean;
        }, workPerItem: 4);
    }

    private static void SubtractInPlace<T>(T[] residual, T[] imf, ComputingContext? ctx)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        int n = residual.Length;
        ComputingContextExecution.ForEach(ctx, 0, n, i =>
        {
            residual[i] -= imf[i];
        }, workPerItem: 2);
    }

    private static double RelativeChange<T>(T[] prev, T[] curr)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        double num = 0, den = 0;
        for (int i = 0; i < prev.Length; i++)
        {
            double p = Convert.ToDouble(prev[i]);
            double c = Convert.ToDouble(curr[i]);
            double d = p - c;
            num += d * d;
            den += p * p;
        }
        if (den < 1e-30) return 0;
        return num / den;
    }

    private readonly struct NaturalCubicSpline
    {
        private readonly double[] _x;
        private readonly double[] _a;
        private readonly double[] _b;
        private readonly double[] _c;
        private readonly double[] _d;
        private readonly int _seg;

        private NaturalCubicSpline(double[] x, double[] a, double[] b, double[] c, double[] d, int seg)
        {
            _x = x;
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _seg = seg;
        }

        public static bool TryCreate(double[] x, double[] y, out NaturalCubicSpline spline)
        {
            spline = default;
            int n = x.Length - 1;
            if (n < 1 || x.Length != y.Length) return false;

            var h = new double[n];
            for (int i = 0; i < n; i++)
            {
                h[i] = x[i + 1] - x[i];
                if (!(h[i] > 0)) return false;
            }

            var alpha = new double[n];
            for (int i = 1; i < n; i++)
                alpha[i] = 3.0 * ((y[i + 1] - y[i]) / h[i] - (y[i] - y[i - 1]) / h[i - 1]);

            var l = new double[n + 1];
            var mu = new double[n];
            var z = new double[n + 1];
            var c = new double[n + 1];
            var b = new double[n];
            var d = new double[n];

            l[0] = 1;
            for (int i = 1; i < n; i++)
            {
                l[i] = 2.0 * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
                if (Math.Abs(l[i]) < 1e-30) return false;
                mu[i] = h[i] / l[i];
                z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
            }

            l[n] = 1;
            for (int j = n - 1; j >= 0; j--)
            {
                c[j] = z[j] - mu[j] * c[j + 1];
                b[j] = (y[j + 1] - y[j]) / h[j] - h[j] * (c[j + 1] + 2.0 * c[j]) / 3.0;
                d[j] = (c[j + 1] - c[j]) / (3.0 * h[j]);
            }

            spline = new NaturalCubicSpline(x, y, b, c, d, n);
            return true;
        }

        public double Evaluate(double xi)
        {
            if (xi <= _x[0]) return _a[0];
            if (xi >= _x[_seg]) return _a[_seg];

            int i = 0;
            while (i < _seg && xi > _x[i + 1]) i++;
            if (i == _seg) i--;
            double dx = xi - _x[i];
            return _a[i] + _b[i] * dx + _c[i] * dx * dx + _d[i] * dx * dx * dx;
        }
    }
}
