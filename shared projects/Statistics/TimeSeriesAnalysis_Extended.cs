using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Statistics;

public static partial class TimeSeriesAnalysis
{
    /// <summary>
    /// Rolling mean with fixed window size.
    /// </summary>
    public static T[] RollingMean<T>(this ReadOnlySpan<T> series, int windowSize)
        where T : IFloatingPointIeee754<T>
        => series.ToArray().AsSpan().MovingAverage(windowSize);

    /// <summary>
    /// Rolling sample variance.
    /// </summary>
    public static T[] RollingVariance<T>(this ReadOnlySpan<T> series, int windowSize)
        where T : IFloatingPointIeee754<T>
    {
        if (windowSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(windowSize));

        var result = new T[series.Length];
        for (int i = 0; i < series.Length; i++)
        {
            int start = Math.Max(0, i - windowSize + 1);
            var window = series[start..(i + 1)].ToArray().AsSpan();
            result[i] = window.Variance().variance;
        }

        return result;
    }

    /// <summary>
    /// Rolling sample standard deviation.
    /// </summary>
    public static T[] RollingStandardDeviation<T>(this ReadOnlySpan<T> series, int windowSize)
        where T : IFloatingPointIeee754<T>
    {
        var variances = series.RollingVariance(windowSize);
        for (int i = 0; i < variances.Length; i++)
            variances[i] = T.Sqrt(variances[i]);
        return variances;
    }

    /// <summary>
    /// Holt's linear trend method (double exponential smoothing).
    /// </summary>
    public static T[] Holt<T>(this ReadOnlySpan<T> series, T alpha, T beta)
        where T : IFloatingPointIeee754<T>
        => series.HoltWithState(alpha, beta).Fitted;

    /// <summary>
    /// Holt recurrence plus the terminal level and trend (not the last two fitted values).
    /// </summary>
    public static (T[] Fitted, HoltState<T> State) HoltWithState<T>(this ReadOnlySpan<T> series, T alpha, T beta)
        where T : IFloatingPointIeee754<T>
    {
        if (series.IsEmpty)
            throw new ArgumentException("Series cannot be empty.", nameof(series));

        var fitted = new T[series.Length];
        T level = series[0];
        T trend = series.Length > 1 ? series[1] - series[0] : T.Zero;
        fitted[0] = level;

        for (int i = 1; i < series.Length; i++)
        {
            T value = series[i];
            T prevLevel = level;
            level = alpha * value + (T.One - alpha) * (level + trend);
            trend = beta * (level - prevLevel) + (T.One - beta) * trend;
            fitted[i] = level + trend;
        }

        return (fitted, new HoltState<T>(level, trend));
    }

    /// <summary>
    /// Holt-Winters additive seasonal forecasting.
    /// </summary>
    public static (T[] Fitted, T[] Forecast) HoltWinters<T>(
        this ReadOnlySpan<T> series,
        int seasonLength,
        int forecastPeriod,
        T alpha,
        T beta,
        T gamma)
        where T : IFloatingPointIeee754<T>
    {
        var (fitted, forecast, _) = series.HoltWintersWithState(seasonLength, forecastPeriod, alpha, beta, gamma);
        return (fitted, forecast);
    }

    /// <summary>
    /// Holt-Winters additive recurrence plus the last level, trend, and one season of seasonal state.
    /// Forecast uses that terminal seasonal vector: <c>L + h·T + S[(h-1) mod m]</c>.
    /// </summary>
    public static (T[] Fitted, T[] Forecast, HoltWintersState<T> State) HoltWintersWithState<T>(
        this ReadOnlySpan<T> series,
        int seasonLength,
        int forecastPeriod,
        T alpha,
        T beta,
        T gamma)
        where T : IFloatingPointIeee754<T>
    {
        if (series.IsEmpty || seasonLength <= 1)
            throw new ArgumentException("Series and season length must be valid.");

        int n = series.Length;
        var level = new T[n];
        var trend = new T[n];
        var seasonal = new T[n];
        var fitted = new T[n];

        level[0] = series[0];
        trend[0] = T.Zero;
        for (int i = 0; i < seasonLength && i < n; i++)
            seasonal[i] = T.Zero;

        for (int i = 1; i < n; i++)
        {
            T prevLevel = i == 1 ? series[0] : level[i - 1];
            T prevTrend = i == 1 ? T.Zero : trend[i - 1];
            T prevSeasonal = seasonal[Math.Max(0, i - seasonLength)];

            level[i] = alpha * (series[i] - prevSeasonal) + (T.One - alpha) * (prevLevel + prevTrend);
            trend[i] = beta * (level[i] - prevLevel) + (T.One - beta) * prevTrend;
            seasonal[i] = gamma * (series[i] - level[i]) + (T.One - gamma) * prevSeasonal;
            fitted[i] = level[i] + trend[i] + seasonal[i];
        }

        fitted[0] = level[0] + trend[0] + seasonal[0];

        var seasonState = new T[seasonLength];
        if (n >= seasonLength)
        {
            for (int i = 0; i < seasonLength; i++)
                seasonState[i] = seasonal[n - seasonLength + i];
        }
        else
        {
            seasonal.AsSpan(0, n).CopyTo(seasonState);
        }

        var forecast = new T[forecastPeriod];
        T lastLevel = level[n - 1];
        T lastTrend = trend[n - 1];
        for (int h = 1; h <= forecastPeriod; h++)
        {
            int seasonIndex = (h - 1) % seasonLength;
            forecast[h - 1] = lastLevel + T.CreateChecked(h) * lastTrend + seasonState[seasonIndex];
        }

        return (fitted, forecast, new HoltWintersState<T>(lastLevel, lastTrend, seasonState));
    }

    /// <summary>
    /// Forecast using Holt linear trend extrapolation from the recurrence's last level and trend.
    /// </summary>
    public static T[] ForecastHolt<T>(this ReadOnlySpan<T> series, int forecastPeriod, T alpha, T beta)
        where T : IFloatingPointIeee754<T>
    {
        if (forecastPeriod < 0)
            throw new ArgumentOutOfRangeException(nameof(forecastPeriod));

        var (_, state) = series.HoltWithState(alpha, beta);
        var forecast = new T[forecastPeriod];
        for (int h = 1; h <= forecastPeriod; h++)
            forecast[h - 1] = state.LastLevel + T.CreateChecked(h) * state.LastTrend;

        return forecast;
    }
}

/// <summary>Terminal Holt level and trend after the last in-sample update.</summary>
public readonly record struct HoltState<T>(T LastLevel, T LastTrend)
    where T : IFloatingPointIeee754<T>;

/// <summary>Terminal Holt-Winters level, trend, and last-season seasonal vector.</summary>
public readonly record struct HoltWintersState<T>(T LastLevel, T LastTrend, T[] LastSeasonal)
    where T : IFloatingPointIeee754<T>;
