using System.Numerics;

namespace Vorcyc.Mathematics.Statistics;

/// <summary>Whether a Holt–Winters seasonal term is added to or multiplies the level.</summary>
public enum HoltWintersSeasonality
{
    Additive,
    Multiplicative
}

public static partial class TimeSeriesAnalysis
{
    /// <summary>
    /// Multiplicative Holt–Winters. The series should stay positive.
    /// <paramref name="fitted"/> values are the filtered level-trend-season product after each update.
    /// The forecast is <c>(L + h T) S</c> using the last season of multiplicative factors.
    /// </summary>
    public static (T[] Fitted, T[] Forecast) HoltWintersMultiplicative<T>(
        this ReadOnlySpan<T> series,
        int seasonLength,
        int forecastPeriod,
        T alpha,
        T beta,
        T gamma)
        where T : IFloatingPointIeee754<T>
    {
        var (fitted, forecast, _) = series.HoltWintersMultiplicativeWithState(seasonLength, forecastPeriod, alpha, beta, gamma);
        return (fitted, forecast);
    }

    /// <summary>
    /// Multiplicative Holt–Winters recurrence plus the terminal level, trend, and one season of factors.
    /// </summary>
    public static (T[] Fitted, T[] Forecast, HoltWintersState<T> State) HoltWintersMultiplicativeWithState<T>(
        this ReadOnlySpan<T> series,
        int seasonLength,
        int forecastPeriod,
        T alpha,
        T beta,
        T gamma)
        where T : IFloatingPointIeee754<T>
    {
        if (series.Length < seasonLength * 2 || seasonLength <= 1)
            throw new ArgumentException("A multiplicative Holt–Winters fit needs at least two seasons.");
        if (forecastPeriod < 0)
            throw new ArgumentOutOfRangeException(nameof(forecastPeriod));

        double a = ClampUnit(double.CreateChecked(alpha));
        double b = ClampUnit(double.CreateChecked(beta));
        double g = ClampUnit(double.CreateChecked(gamma));
        var values = SeriesConvert.ToDouble(series);
        var factors = InitialSeasonalFactors(values, seasonLength);
        int seasons = values.Length / seasonLength;
        double first = SeasonAverage(values, 0, seasonLength);
        double last = SeasonAverage(values, seasons - 1, seasonLength);
        double level = first;
        double trend = (last - first) / ((seasons - 1) * seasonLength);
        var seasonal = (double[])factors.Clone();
        var fitted = new double[values.Length];

        for (int t = 0; t < values.Length; t++)
        {
            int slot = t % seasonLength;
            double previousSeason = seasonal[slot];
            if (Math.Abs(previousSeason) < 1e-8)
                previousSeason = Math.CopySign(1e-8, previousSeason == 0 ? 1 : previousSeason);

            double previousLevel = level;
            level = a * (values[t] / previousSeason) + (1 - a) * (level + trend);
            trend = b * (level - previousLevel) + (1 - b) * trend;
            if (Math.Abs(level) < 1e-8)
                level = Math.CopySign(1e-8, level == 0 ? 1 : level);
            seasonal[slot] = g * (values[t] / level) + (1 - g) * previousSeason;
            fitted[t] = (level + trend) * seasonal[slot];
        }

        var forecast = new double[forecastPeriod];
        for (int h = 1; h <= forecastPeriod; h++)
        {
            int slot = (values.Length + h - 1) % seasonLength;
            forecast[h - 1] = (level + h * trend) * seasonal[slot];
        }

        var ordered = new double[seasonLength];
        for (int i = 0; i < seasonLength; i++)
            ordered[i] = seasonal[(values.Length + i) % seasonLength];

        return (SeriesConvert.FromDouble<T>(fitted), SeriesConvert.FromDouble<T>(forecast), new HoltWintersState<T>(
            T.CreateChecked(level),
            T.CreateChecked(trend),
            SeriesConvert.FromDouble<T>(ordered)));
    }

    static double[] InitialSeasonalFactors(double[] values, int seasonLength)
    {
        int seasons = values.Length / seasonLength;
        var factors = new double[seasonLength];
        var counts = new int[seasonLength];
        for (int season = 0; season < seasons; season++)
        {
            double average = SeasonAverage(values, season, seasonLength);
            if (Math.Abs(average) < 1e-8)
                throw new ArgumentException("A seasonal block average is too close to zero for multiplicative Holt–Winters.");

            for (int slot = 0; slot < seasonLength; slot++)
            {
                factors[slot] += values[season * seasonLength + slot] / average;
                counts[slot]++;
            }
        }

        for (int slot = 0; slot < seasonLength; slot++)
            factors[slot] /= counts[slot];
        NormalizeMean(factors);
        return factors;
    }

    static void NormalizeMean(double[] factors)
    {
        double mean = 0;
        for (int i = 0; i < factors.Length; i++)
            mean += factors[i];
        mean /= factors.Length;
        if (Math.Abs(mean) < 1e-8)
            return;
        for (int i = 0; i < factors.Length; i++)
            factors[i] /= mean;
    }

    static double SeasonAverage(double[] values, int season, int seasonLength)
    {
        double sum = 0;
        int start = season * seasonLength;
        for (int i = 0; i < seasonLength; i++)
            sum += values[start + i];
        return sum / seasonLength;
    }

    static double ClampUnit(double value)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Smoothing weights must be finite.");
        return Math.Clamp(value, 0, 1);
    }
}

/// <summary>
/// Holt linear model with smoothing weights chosen by one-step squared error, then forecast from the terminal state.
/// </summary>
public sealed class HoltModel<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private HoltModel(T alpha, T beta, T[] fitted, HoltState<T> state)
    {
        Alpha = alpha;
        Beta = beta;
        Fitted = fitted;
        State = state;
    }

    public T Alpha { get; }
    public T Beta { get; }
    public T[] Fitted { get; }
    public HoltState<T> State { get; }

    public static HoltModel<T> Estimate(ReadOnlySpan<T> series)
    {
        if (series.IsEmpty)
            throw new ArgumentException("Series cannot be empty.", nameof(series));

        var values = SeriesConvert.ToDouble(series);
        double bestAlpha = 0.2;
        double bestBeta = 0.1;
        double best = double.PositiveInfinity;
        for (int ia = 1; ia <= 19; ia++)
        {
            for (int ib = 1; ib <= 19; ib++)
            {
                double alpha = ia * 0.05;
                double beta = ib * 0.05;
                double sse = OneStepSse(values, alpha, beta);
                if (sse < best)
                {
                    best = sse;
                    bestAlpha = alpha;
                    bestBeta = beta;
                }
            }
        }

        double refinedAlpha = bestAlpha;
        double refinedBeta = bestBeta;
        for (int ia = -4; ia <= 4; ia++)
        {
            for (int ib = -4; ib <= 4; ib++)
            {
                double alpha = Math.Clamp(bestAlpha + ia * 0.01, 0.01, 0.99);
                double beta = Math.Clamp(bestBeta + ib * 0.01, 0.01, 0.99);
                double sse = OneStepSse(values, alpha, beta);
                if (sse < best)
                {
                    best = sse;
                    refinedAlpha = alpha;
                    refinedBeta = beta;
                }
            }
        }

        T alphaT = T.CreateChecked(refinedAlpha);
        T betaT = T.CreateChecked(refinedBeta);
        var (fitted, state) = series.HoltWithState(alphaT, betaT);
        return new HoltModel<T>(alphaT, betaT, fitted, state);
    }

    public T[] Forecast(int horizon)
    {
        if (horizon < 0)
            throw new ArgumentOutOfRangeException(nameof(horizon));

        var forecast = new T[horizon];
        for (int h = 1; h <= horizon; h++)
            forecast[h - 1] = State.LastLevel + T.CreateChecked(h) * State.LastTrend;
        return forecast;
    }

    /// <summary>One-step squared error of the same recurrence used by <see cref="TimeSeriesAnalysis.HoltWithState{T}"/>.</summary>
    public static double OneStepSse(ReadOnlySpan<T> series, T alpha, T beta)
        => OneStepSse(SeriesConvert.ToDouble(series), double.CreateChecked(alpha), double.CreateChecked(beta));

    static double OneStepSse(double[] values, double alpha, double beta)
    {
        double level = values[0];
        double trend = values.Length > 1 ? values[1] - values[0] : 0;
        double sse = 0;
        for (int i = 1; i < values.Length; i++)
        {
            double forecast = level + trend;
            double error = values[i] - forecast;
            sse += error * error;
            double previous = level;
            level = alpha * values[i] + (1 - alpha) * (level + trend);
            trend = beta * (level - previous) + (1 - beta) * trend;
        }

        return sse;
    }
}

/// <summary>
/// Holt–Winters model with smoothing weights chosen by one-step squared error.
/// </summary>
public sealed class HoltWintersModel<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly ReadOnlyMemory<T> _series;

    private HoltWintersModel(ReadOnlyMemory<T> series, int seasonLength, HoltWintersSeasonality seasonality, T alpha, T beta, T gamma, T[] fitted, HoltWintersState<T> state)
    {
        _series = series;
        SeasonLength = seasonLength;
        Seasonality = seasonality;
        Alpha = alpha;
        Beta = beta;
        Gamma = gamma;
        Fitted = fitted;
        State = state;
    }

    public int SeasonLength { get; }
    public HoltWintersSeasonality Seasonality { get; }
    public T Alpha { get; }
    public T Beta { get; }
    public T Gamma { get; }
    public T[] Fitted { get; }
    public HoltWintersState<T> State { get; }

    public static HoltWintersModel<T> Estimate(ReadOnlySpan<T> series, int seasonLength, HoltWintersSeasonality seasonality = HoltWintersSeasonality.Additive)
    {
        if (seasonLength <= 1 || series.Length < seasonLength * 2)
            throw new ArgumentException("Holt–Winters estimation needs at least two seasons.");

        var values = SeriesConvert.ToDouble(series);
        double bestA = 0.2, bestB = 0.1, bestG = 0.2, best = double.PositiveInfinity;
        for (int ia = 1; ia <= 9; ia++)
        {
            for (int ib = 1; ib <= 9; ib++)
            {
                for (int ig = 1; ig <= 9; ig++)
                {
                    double alpha = ia * 0.1;
                    double beta = ib * 0.1;
                    double gamma = ig * 0.1;
                    double sse = seasonality == HoltWintersSeasonality.Multiplicative
                        ? MultiplicativeSse(values, seasonLength, alpha, beta, gamma)
                        : AdditiveSse(values, seasonLength, alpha, beta, gamma);
                    if (sse < best)
                    {
                        best = sse;
                        bestA = alpha;
                        bestB = beta;
                        bestG = gamma;
                    }
                }
            }
        }

        T alphaT = T.CreateChecked(bestA);
        T betaT = T.CreateChecked(bestB);
        T gammaT = T.CreateChecked(bestG);
        var owned = series.ToArray();
        if (seasonality == HoltWintersSeasonality.Multiplicative)
        {
            var (fitted, _, state) = owned.AsSpan().HoltWintersMultiplicativeWithState(seasonLength, 0, alphaT, betaT, gammaT);
            return new HoltWintersModel<T>(owned, seasonLength, seasonality, alphaT, betaT, gammaT, fitted, state);
        }

        var additive = owned.AsSpan().HoltWintersWithState(seasonLength, 0, alphaT, betaT, gammaT);
        return new HoltWintersModel<T>(owned, seasonLength, seasonality, alphaT, betaT, gammaT, additive.Fitted, additive.State);
    }

    public T[] Forecast(int horizon)
    {
        if (Seasonality == HoltWintersSeasonality.Multiplicative)
            return _series.Span.HoltWintersMultiplicative(SeasonLength, horizon, Alpha, Beta, Gamma).Forecast;
        return _series.Span.HoltWinters(SeasonLength, horizon, Alpha, Beta, Gamma).Forecast;
    }

    static double AdditiveSse(double[] values, int seasonLength, double alpha, double beta, double gamma)
    {
        int n = values.Length;
        double level = values[0];
        double trend = 0;
        var seasonal = new double[n];
        double sse = 0;
        for (int i = 1; i < n; i++)
        {
            double previousSeason = seasonal[Math.Max(0, i - seasonLength)];
            double forecast = level + trend + previousSeason;
            double error = values[i] - forecast;
            sse += error * error;
            double previousLevel = level;
            level = alpha * (values[i] - previousSeason) + (1 - alpha) * (level + trend);
            trend = beta * (level - previousLevel) + (1 - beta) * trend;
            seasonal[i] = gamma * (values[i] - level) + (1 - gamma) * previousSeason;
        }

        return sse;
    }

    static double MultiplicativeSse(double[] values, int seasonLength, double alpha, double beta, double gamma)
    {
        try
        {
            var series = SeriesConvert.FromDouble<T>(values);
            var (fitted, _) = series.AsSpan().HoltWintersMultiplicative(seasonLength, 0, T.CreateChecked(alpha), T.CreateChecked(beta), T.CreateChecked(gamma));
            double sse = 0;
            for (int i = seasonLength; i < values.Length; i++)
            {
                double error = values[i] - double.CreateChecked(fitted[i]);
                sse += error * error;
            }

            return sse;
        }
        catch (ArgumentException)
        {
            return double.PositiveInfinity;
        }
    }
}

internal static class SeriesConvert
{
    public static double[] ToDouble<T>(ReadOnlySpan<T> series)
        where T : IFloatingPointIeee754<T>
    {
        var values = new double[series.Length];
        for (int i = 0; i < series.Length; i++)
            values[i] = double.CreateChecked(series[i]);
        return values;
    }

    public static T[] FromDouble<T>(ReadOnlySpan<double> values)
        where T : IFloatingPointIeee754<T>
    {
        var result = new T[values.Length];
        for (int i = 0; i < values.Length; i++)
            result[i] = T.CreateChecked(values[i]);
        return result;
    }
}
