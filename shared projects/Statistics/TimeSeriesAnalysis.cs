//时间序列分析
//23.	时间序列分析 (Time Series Analysis): 实现时间序列的平滑、分解和预测。
//24.	自相关函数 (Autocorrelation Function, ACF): 计算时间序列的自相关函数。
//25.	偏自相关函数 (Partial Autocorrelation Function, PACF): 计算时间序列的偏自相关函数。
//26.	移动平均 (Moving Average): 计算时间序列的移动平均。
//27.	指数平滑 (Exponential Smoothing): 实现时间序列的指数平滑。  


namespace Vorcyc.Mathematics.Statistics;

using System.Numerics;


/// <summary>
/// 提供时间序列分析的各种方法，包括平滑、分解、预测、自相关、偏自相关、移动平均和指数平滑。
/// </summary>
public static class TimeSeriesAnalysis
{
    /// <summary>
    /// 实现时间序列的平滑。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="series">时间序列数据。</param>
    /// <param name="windowSize">平滑窗口的大小。</param>
    /// <returns>平滑后的时间序列。</returns>
    /// <remarks>
    /// 平滑 (Smoothing): 通过计算时间序列中每个点的局部平均值来减少噪声和波动，从而更清晰地显示数据的趋势。
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
    /// 实现时间序列的分解，返回趋势、季节性和残差。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="series">时间序列数据。</param>
    /// <param name="period">季节周期。</param>
    /// <returns>包含趋势、季节性和残差的元组。</returns>
    /// <remarks>
    /// 分解 (Decomposition): 将时间序列分解为三个部分：趋势 (Trend)、季节性 (Seasonal) 和残差 (Residual)。
    /// 趋势表示数据的长期变化，季节性表示数据的周期性波动，残差表示数据中无法解释的随机波动。
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
    /// 实现时间序列的预测，返回预测值。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="series">时间序列数据。</param>
    /// <param name="forecastPeriod">预测期数。</param>
    /// <returns>预测值数组。</returns>
    /// <remarks>
    /// 预测 (Forecasting): 基于时间序列的历史数据，使用统计模型预测未来的值。
    /// 这里使用简单的平均值作为预测值。
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
    /// 计算时间序列的自相关函数。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="series">时间序列数据。</param>
    /// <param name="lagMax">最大滞后期数。</param>
    /// <returns>自相关函数值数组。</returns>
    /// <remarks>
    /// 自相关函数 (Autocorrelation Function, ACF): 衡量时间序列在不同滞后期数下的相关性。
    /// 自相关函数用于识别时间序列中的周期性和趋势。
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
    /// 计算时间序列的偏自相关函数。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="series">时间序列数据。</param>
    /// <param name="lagMax">最大滞后期数。</param>
    /// <returns>偏自相关函数值数组。</returns>
    /// <remarks>
    /// 偏自相关函数 (Partial Autocorrelation Function, PACF): 衡量时间序列在不同滞后期数下的直接相关性。
    /// 偏自相关函数用于识别时间序列中的直接影响，而不是间接影响。
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
    /// 计算时间序列的移动平均。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="series">时间序列数据。</param>
    /// <param name="windowSize">移动平均窗口的大小。</param>
    /// <returns>移动平均值数组。</returns>
    /// <remarks>
    /// 移动平均 (Moving Average): 通过计算时间序列中每个点的局部平均值来平滑数据，减少噪声和波动。
    /// 移动平均用于识别时间序列中的趋势和周期性。
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
    /// 实现时间序列的指数平滑。
    /// </summary>
    /// <typeparam name="T">数据类型，必须实现<see cref="IFloatingPointIeee754{T}"/>接口。</typeparam>
    /// <param name="series">时间序列数据。</param>
    /// <param name="alpha">平滑系数。</param>
    /// <returns>指数平滑后的时间序列。</returns>
    /// <remarks>
    /// 指数平滑 (Exponential Smoothing): 通过对时间序列中的每个点赋予不同的权重来平滑数据，最近的点权重较大，较远的点权重较小。
    /// 指数平滑用于识别时间序列中的趋势和周期性。
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


internal static class TimeSeriesAnalysis2
{
    // 23. 时间序列分析 (Time Series Analysis): 实现时间序列的平滑、分解和预测。
    public static double[] Smooth(double[] series, int windowSize)
    {
        double[] smoothedSeries = new double[series.Length];
        for (int i = 0; i < series.Length; i++)
        {
            int start = Math.Max(0, i - windowSize / 2);
            int end = Math.Min(series.Length - 1, i + windowSize / 2);
            smoothedSeries[i] = series[start..(end + 1)].Average();
        }
        return smoothedSeries;
    }

    public static (double[] Trend, double[] Seasonal, double[] Residual) Decompose(double[] series, int period)
    {
        int n = series.Length;
        double[] trend = new double[n];
        double[] seasonal = new double[n];
        double[] residual = new double[n];

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
            double[] seasonalValues = new double[(n + period - 1) / period];
            for (int j = i; j < n; j += period)
            {
                seasonalValues[j / period] = series[j] - trend[j];
            }
            double seasonalAverage = seasonalValues.Average();
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

    public static double[] Forecast(double[] series, int forecastPeriod)
    {
        double[] forecast = new double[forecastPeriod];
        double mean = series.Average();
        for (int i = 0; i < forecastPeriod; i++)
        {
            forecast[i] = mean;
        }
        return forecast;
    }

    // 24. 自相关函数 (Autocorrelation Function, ACF): 计算时间序列的自相关函数。
    public static double[] Autocorrelation(double[] series, int lagMax)
    {
        int n = series.Length;
        double mean = series.Average();
        double[] acf = new double[lagMax + 1];
        double variance = series.Sum(x => Math.Pow(x - mean, 2)) / n;

        for (int lag = 0; lag <= lagMax; lag++)
        {
            double covariance = 0;
            for (int i = 0; i < n - lag; i++)
            {
                covariance += (series[i] - mean) * (series[i + lag] - mean);
            }
            acf[lag] = covariance / (n * variance);
        }

        return acf;
    }

    // 25. 偏自相关函数 (Partial Autocorrelation Function, PACF): 计算时间序列的偏自相关函数。
    public static double[] PartialAutocorrelation(double[] series, int lagMax)
    {
        int n = series.Length;
        double[] pacf = new double[lagMax + 1];
        double[] acf = Autocorrelation(series, lagMax);

        pacf[0] = 1.0;
        for (int lag = 1; lag <= lagMax; lag++)
        {
            double[] phi = new double[lag + 1];
            phi[lag] = acf[lag];
            for (int k = 1; k < lag; k++)
            {
                phi[lag] -= phi[k] * acf[lag - k];
            }
            pacf[lag] = phi[lag];
        }

        return pacf;
    }

    // 26. 移动平均 (Moving Average): 计算时间序列的移动平均。
    public static double[] MovingAverage(double[] series, int windowSize)
    {
        double[] movingAverage = new double[series.Length];
        for (int i = 0; i < series.Length; i++)
        {
            int start = Math.Max(0, i - windowSize + 1);
            int end = i;
            movingAverage[i] = series[start..(end + 1)].Average();
        }
        return movingAverage;
    }

    // 27. 指数平滑 (Exponential Smoothing): 实现时间序列的指数平滑。
    public static double[] ExponentialSmoothing(double[] series, double alpha)
    {
        double[] smoothedSeries = new double[series.Length];
        smoothedSeries[0] = series[0];
        for (int i = 1; i < series.Length; i++)
        {
            smoothedSeries[i] = alpha * series[i] + (1 - alpha) * smoothedSeries[i - 1];
        }
        return smoothedSeries;
    }
}

