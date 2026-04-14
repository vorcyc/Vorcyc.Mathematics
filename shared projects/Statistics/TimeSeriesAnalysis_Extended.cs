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

        return fitted;
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
            int seasonIndex = i % seasonLength;
            T prevLevel = i == 1 ? series[0] : level[i - 1];
            T prevTrend = i == 1 ? T.Zero : trend[i - 1];
            T prevSeasonal = seasonal[Math.Max(0, i - seasonLength)];

            level[i] = alpha * (series[i] - prevSeasonal) + (T.One - alpha) * (prevLevel + prevTrend);
            trend[i] = beta * (level[i] - prevLevel) + (T.One - beta) * prevTrend;
            seasonal[i] = gamma * (series[i] - level[i]) + (T.One - gamma) * prevSeasonal;
            fitted[i] = level[i] + trend[i] + seasonal[i];
        }

        var forecast = new T[forecastPeriod];
        for (int h = 1; h <= forecastPeriod; h++)
        {
            int seasonIndex = (n - seasonLength + h) % seasonLength;
            if (seasonIndex < 0) seasonIndex += seasonLength;
            T s = seasonal[n - seasonLength + seasonIndex];
            forecast[h - 1] = level[n - 1] + T.CreateChecked(h) * trend[n - 1] + s;
        }

        return (fitted, forecast);
    }

    /// <summary>
    /// Forecast using Holt linear trend extrapolation.
    /// </summary>
    public static T[] ForecastHolt<T>(this ReadOnlySpan<T> series, int forecastPeriod, T alpha, T beta)
        where T : IFloatingPointIeee754<T>
    {
        if (series.IsEmpty)
            throw new ArgumentException("Series cannot be empty.", nameof(series));

        var fitted = series.Holt(alpha, beta);
        T level = fitted[^1];
        T trend = fitted.Length > 1 ? fitted[^1] - fitted[^2] : T.Zero;

        var forecast = new T[forecastPeriod];
        for (int h = 1; h <= forecastPeriod; h++)
            forecast[h - 1] = level + T.CreateChecked(h) * trend;

        return forecast;
    }
}
