//时间序列分析
//23.	时间序列分析 (Time Series Analysis): 实现时间序列的平滑、分解和预测。
//24.	自相关函数 (Autocorrelation Function, ACF): 计算时间序列的自相关函数。
//25.	偏自相关函数 (Partial Autocorrelation Function, PACF): 计算时间序列的偏自相关函数。
//26.	移动平均 (Moving Average): 计算时间序列的移动平均。
//27.	指数平滑 (Exponential Smoothing): 实现时间序列的指数平滑。  


namespace Vorcyc.Mathematics.Statistics;

using System.Numerics;


/// <summary>
/// Provides various methods for time series analysis, including smoothing, decomposition, forecasting, autocorrelation, partial autocorrelation, moving average, and exponential smoothing.
/// </summary>
public static partial class TimeSeriesAnalysis
{
    /// <summary>
    /// Performs smoothing of a time series.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="series">The time series data.</param>
    /// <param name="windowSize">The size of the smoothing window.</param>
    /// <returns>The smoothed time series.</returns>
    /// <remarks>
    /// Smoothing: Reduces noise and fluctuations by computing the local average of each point in the time series, thereby revealing the trend of the data more clearly.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Smooth<T>(this Span<T> series, int windowSize) where T : IFloatingPointIeee754<T>
    {
        T[] smoothedSeries = new T[series.Length];
        for (int i = 0; i < series.Length; i++)
        {
            int start = Math.Max(0, i - windowSize / 2);
            int end = Math.Min(series.Length - 1, i + windowSize / 2);
            smoothedSeries[i] = series[start..(end + 1)].Average();
        }
        return smoothedSeries;
    }

    /// <summary>
    /// Performs decomposition of a time series, returning the trend, seasonal, and residual components.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="series">The time series data.</param>
    /// <param name="period">The seasonal period.</param>
    /// <returns>A tuple containing the trend, seasonal, and residual components.</returns>
    /// <remarks>
    /// Decomposition: Decomposes a time series into three components: trend, seasonal, and residual.
    /// The trend represents the long-term change in the data, the seasonal component represents the periodic fluctuations in the data, and the residual represents the random fluctuations in the data that cannot be explained.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T[] Trend, T[] Seasonal, T[] Residual) Decompose<T>(this Span<T> series, int period) where T : IFloatingPointIeee754<T>
    {
        int n = series.Length;
        T[] trend = new T[n];
        T[] seasonal = new T[n];
        T[] residual = new T[n];

        // 计算趋势
        for (int i = 0; i < n; i++)
        {
            int start = Math.Max(0, i - period / 2);
            int end = Math.Min(n - 1, i + period / 2);
            trend[i] = series[start..(end + 1)].Average();
        }

        // 计算季节性
        for (int i = 0; i < period; i++)
        {
            T[] seasonalValues = new T[(n + period - 1) / period];
            for (int j = i; j < n; j += period)
            {
                seasonalValues[j / period] = series[j] - trend[j];
            }
            T seasonalAverage = seasonalValues.AsSpan().Average();
            for (int j = i; j < n; j += period)
            {
                seasonal[j] = seasonalAverage;
            }
        }

        // 计算残差
        for (int i = 0; i < n; i++)
        {
            residual[i] = series[i] - trend[i] - seasonal[i];
        }

        return (trend, seasonal, residual);
    }

    /// <summary>
    /// Performs forecasting of a time series, returning the forecast values.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="series">The time series data.</param>
    /// <param name="forecastPeriod">The number of forecast periods.</param>
    /// <returns>An array of forecast values.</returns>
    /// <remarks>
    /// Forecasting: Predicts future values using a statistical model based on the historical data of the time series.
    /// Here, a simple average is used as the forecast value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Forecast<T>(this Span<T> series, int forecastPeriod) where T : IFloatingPointIeee754<T>
    {
        T[] forecast = new T[forecastPeriod];
        T mean = series.Average();
        for (int i = 0; i < forecastPeriod; i++)
        {
            forecast[i] = mean;
        }
        return forecast;
    }

    /// <summary>
    /// Computes the autocorrelation function of a time series.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="series">The time series data.</param>
    /// <param name="lagMax">The maximum lag.</param>
    /// <returns>An array of autocorrelation function values.</returns>
    /// <remarks>
    /// Autocorrelation Function (ACF): Measures the correlation of a time series at different lags.
    /// The autocorrelation function is used to identify the periodicity and trend in a time series.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Autocorrelation<T>(this Span<T> series, int lagMax) where T : IFloatingPointIeee754<T>
    {
        int n = series.Length;
        T mean = series.Average();
        T[] acf = new T[lagMax + 1];
        T variance = series.Sum(x => T.Pow(x - mean, T.CreateChecked(2))) / T.CreateChecked(n);

        for (int lag = 0; lag <= lagMax; lag++)
        {
            T covariance = T.Zero;
            for (int i = 0; i < n - lag; i++)
            {
                covariance += (series[i] - mean) * (series[i + lag] - mean);
            }
            acf[lag] = covariance / (T.CreateChecked(n) * variance);
        }

        return acf;
    }

    /// <summary>
    /// Computes the partial autocorrelation function of a time series.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="series">The time series data.</param>
    /// <param name="lagMax">The maximum lag.</param>
    /// <returns>An array of partial autocorrelation function values.</returns>
    /// <remarks>
    /// Partial Autocorrelation Function (PACF): Measures the direct correlation of a time series at different lags.
    /// The partial autocorrelation function is used to identify the direct, rather than indirect, influences in a time series.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] PartialAutocorrelation<T>(this Span<T> series, int lagMax) where T : IFloatingPointIeee754<T>
    {
        int n = series.Length;
        T[] pacf = new T[lagMax + 1];
        T[] acf = series.Autocorrelation(lagMax);

        pacf[0] = T.One;
        for (int lag = 1; lag <= lagMax; lag++)
        {
            T[] phi = new T[lag + 1];
            phi[lag] = acf[lag];
            for (int k = 1; k < lag; k++)
            {
                phi[lag] -= phi[k] * acf[lag - k];
            }
            pacf[lag] = phi[lag];
        }

        return pacf;
    }

    /// <summary>
    /// Computes the moving average of a time series.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="series">The time series data.</param>
    /// <param name="windowSize">The size of the moving average window.</param>
    /// <returns>An array of moving average values.</returns>
    /// <remarks>
    /// Moving Average: Smooths the data by computing the local average of each point in the time series, reducing noise and fluctuations.
    /// The moving average is used to identify trends and periodicity in a time series.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] MovingAverage<T>(this Span<T> series, int windowSize) where T : IFloatingPointIeee754<T>
    {
        T[] movingAverage = new T[series.Length];
        for (int i = 0; i < series.Length; i++)
        {
            int start = Math.Max(0, i - windowSize + 1);
            int end = i;
            movingAverage[i] = series[start..(end + 1)].Average();
        }
        return movingAverage;
    }

    /// <summary>
    /// Performs exponential smoothing of a time series.
    /// </summary>
    /// <typeparam name="T">The data type, which must implement the <see cref="IFloatingPointIeee754{T}"/> interface.</typeparam>
    /// <param name="series">The time series data.</param>
    /// <param name="alpha">The smoothing factor.</param>
    /// <returns>The exponentially smoothed time series.</returns>
    /// <remarks>
    /// Exponential Smoothing: Smooths the data by assigning different weights to each point in the time series; more recent points receive larger weights, and more distant points receive smaller weights.
    /// Exponential smoothing is used to identify trends and periodicity in a time series.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ExponentialSmoothing<T>(this Span<T> series, T alpha) where T : IFloatingPointIeee754<T>
    {
        T[] smoothedSeries = new T[series.Length];
        smoothedSeries[0] = series[0];
        for (int i = 1; i < series.Length; i++)
        {
            smoothedSeries[i] = alpha * series[i] + (T.One - alpha) * smoothedSeries[i - 1];
        }
        return smoothedSeries;
    }
}

