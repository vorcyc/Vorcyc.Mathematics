using System.Numerics;
using Vorcyc.Mathematics.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>
/// Gaussian GARCH model on a demeaned series.
/// The GARCH order counts lagged variances and the ARCH order counts lagged squared innovations.
/// The intercept uses variance targeting.
/// </summary>
public sealed class GarchModel<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly double[] _epsilon;
    private readonly double[] _variance;
    private readonly double _omega;
    private readonly double[] _alpha;
    private readonly double[] _beta;

    private GarchModel(double mean, double omega, double[] alpha, double[] beta, double[] epsilon, double[] variance)
    {
        _omega = omega;
        _alpha = alpha;
        _beta = beta;
        _epsilon = epsilon;
        _variance = variance;
        Mean = T.CreateChecked(mean);
        Omega = T.CreateChecked(omega);
        Alpha = SeriesConvert.FromDouble<T>(alpha);
        Beta = SeriesConvert.FromDouble<T>(beta);
        ConditionalVariance = SeriesConvert.FromDouble<T>(variance);
    }

    public T Mean { get; }
    public T Omega { get; }
    public T[] Alpha { get; }
    public T[] Beta { get; }
    public T[] ConditionalVariance { get; }

    public static GarchModel<T> Estimate(ReadOnlySpan<T> series, int garchOrder = 1, int archOrder = 1)
    {
        if (garchOrder < 0 || archOrder < 0 || garchOrder + archOrder == 0)
            throw new ArgumentOutOfRangeException(nameof(garchOrder));
        if (series.Length < 30)
            throw new ArgumentException("GARCH estimation needs at least 30 observations.");

        var raw = SeriesConvert.ToDouble(series);
        double mean = raw.Average();
        var epsilon = new double[raw.Length];
        double variance = 0;
        for (int i = 0; i < raw.Length; i++)
        {
            epsilon[i] = raw[i] - mean;
            variance += epsilon[i] * epsilon[i];
        }

        variance /= raw.Length;
        int dimension = archOrder + garchOrder;
        var start = new double[dimension];
        var step = new double[dimension];
        for (int i = 0; i < archOrder; i++)
        {
            start[i] = 0.05;
            step[i] = 0.05;
        }

        for (int i = 0; i < garchOrder; i++)
        {
            start[archOrder + i] = garchOrder == 1 ? 0.9 : 0.4 / garchOrder;
            step[archOrder + i] = 0.05;
        }

        double[] Best(double[] initial)
        {
            return NelderMead.Minimize(point => NegativeLogLikelihood(epsilon, variance, point, archOrder), initial, step, 180);
        }

        double[] parameters = Best(start);
        double bestNll = NegativeLogLikelihood(epsilon, variance, parameters, archOrder);
        var alternate = (double[])start.Clone();
        for (int i = 0; i < archOrder; i++)
            alternate[i] = 0.1;
        for (int i = 0; i < garchOrder; i++)
            alternate[archOrder + i] = 0.4;
        double[] second = Best(alternate);
        double secondNll = NegativeLogLikelihood(epsilon, variance, second, archOrder);
        if (secondNll < bestNll)
            parameters = second;

        var alpha = parameters[..archOrder];
        var beta = parameters[archOrder..];
        double persistence = alpha.Sum() + beta.Sum();
        double omega = variance * Math.Max(1e-8, 1 - persistence);
        var path = VariancePath(epsilon, variance, omega, alpha, beta);
        return new GarchModel<T>(mean, omega, alpha, beta, epsilon, path);
    }

    public T[] ForecastVariance(int horizon)
    {
        if (horizon < 0)
            throw new ArgumentOutOfRangeException(nameof(horizon));

        var squared = new List<double>(_epsilon.Length + horizon);
        for (int i = 0; i < _epsilon.Length; i++)
            squared.Add(_epsilon[i] * _epsilon[i]);
        var variance = new List<double>(_variance);
        var forecast = new double[horizon];
        for (int h = 0; h < horizon; h++)
        {
            double value = _omega;
            for (int i = 0; i < _alpha.Length; i++)
            {
                int index = squared.Count - 1 - i;
                if (index >= 0)
                    value += _alpha[i] * squared[index];
            }

            for (int i = 0; i < _beta.Length; i++)
            {
                int index = variance.Count - 1 - i;
                if (index >= 0)
                    value += _beta[i] * variance[index];
            }

            value = Math.Max(value, 1e-12);
            forecast[h] = value;
            squared.Add(value);
            variance.Add(value);
        }

        return SeriesConvert.FromDouble<T>(forecast);
    }

    static double NegativeLogLikelihood(double[] epsilon, double unconditional, double[] parameters, int archOrder)
    {
        var alpha = parameters[..archOrder];
        var beta = parameters[archOrder..];
        double persistence = 0;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i] < 0 || parameters[i] > 0.999)
                return 1e12;
            persistence += parameters[i];
        }

        if (persistence >= 0.999)
            return 1e12;

        double omega = unconditional * (1 - persistence);
        var variance = VariancePath(epsilon, unconditional, omega, alpha, beta);
        double nll = 0;
        int burn = Math.Min(10, epsilon.Length / 5);
        for (int t = burn; t < epsilon.Length; t++)
        {
            double sigma2 = Math.Max(variance[t], 1e-12);
            nll += Math.Log(sigma2) + epsilon[t] * epsilon[t] / sigma2;
        }

        return double.IsFinite(nll) ? nll : 1e12;
    }

    static double[] VariancePath(double[] epsilon, double unconditional, double omega, double[] alpha, double[] beta)
    {
        var variance = new double[epsilon.Length];
        variance[0] = unconditional;
        for (int t = 1; t < epsilon.Length; t++)
        {
            double value = omega;
            for (int i = 0; i < alpha.Length; i++)
            {
                int lag = t - 1 - i;
                if (lag >= 0)
                    value += alpha[i] * epsilon[lag] * epsilon[lag];
            }

            for (int i = 0; i < beta.Length; i++)
            {
                int lag = t - 1 - i;
                if (lag >= 0)
                    value += beta[i] * variance[lag];
            }

            variance[t] = Math.Max(value, 1e-12);
        }

        return variance;
    }
}
