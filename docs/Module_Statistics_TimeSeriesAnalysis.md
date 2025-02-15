当前位置 : [根目录](README.md)/[统计模块](Module_Statistics.md)/[时间序列分析](Module_Statistics_TimeSeriesAnalysis.md)

# 统计模块 - Statistics Module

`Vorcyc.Mathematics.Statistics.TimeSeriesAnalysis` 是一个提供时间序列分析方法的类，包括平滑、分解、预测、自相关、偏自相关、移动平均和指数平滑。

> 以下方法均位于类 ：Vorcyc.Mathematics.Statistics.TimeSeriesAnalysis

---

:ledger:目录  
- :bookmark: [Smooth 方法](#1-smooth-方法)  
- :bookmark: [Decompose 方法](#2-decompose-方法)  
- :bookmark: [Forecast 方法](#3-forecast-方法)  
- :bookmark: [Autocorrelation 方法](#4-autocorrelation-方法)  
- :bookmark: [PartialAutocorrelation 方法](#5-partialautocorrelation-方法)  
- :bookmark: [MovingAverage 方法](#6-movingaverage-方法)  
- :bookmark: [ExponentialSmoothing 方法](#7-exponentialsmoothing-方法)  

---

## Vorcyc.Mathematics.Statistics.TimeSeriesAnalysis 类

Vorcyc.Mathematics.Statistics.TimeSeriesAnalysis 是一个提供时间序列分析方法的类，包括平滑、分解、预测、自相关、偏自相关、移动平均和指数平滑。

### 方法

#### 1. Smooth
- `public static T[] Smooth<T>(this Span<T> series, int windowSize) where T : IFloatingPointIeee754<T>`
  - 实现时间序列的平滑。
  - 参数:
    - `series`: 时间序列数据。
    - `windowSize`: 平滑窗口的大小。
  - 返回值: 平滑后的时间序列。

#### 2. Decompose
- `public static (T[] Trend, T[] Seasonal, T[] Residual) Decompose<T>(this Span<T> series, int period) where T : IFloatingPointIeee754<T>`
  - 实现时间序列的分解，返回趋势、季节性和残差。
  - 参数:
    - `series`: 时间序列数据。
    - `period`: 季节周期。
  - 返回值: 包含趋势、季节性和残差的元组。

#### 3. Forecast
- `public static T[] Forecast<T>(this Span<T> series, int forecastPeriod) where T : IFloatingPointIeee754<T>`
  - 实现时间序列的预测，返回预测值。
  - 参数:
    - `series`: 时间序列数据。
    - `forecastPeriod`: 预测期数。
  - 返回值: 预测值数组。

#### 4. Autocorrelation
- `public static T[] Autocorrelation<T>(this Span<T> series, int lagMax) where T : IFloatingPointIeee754<T>`
  - 计算时间序列的自相关函数。
  - 参数:
    - `series`: 时间序列数据。
    - `lagMax`: 最大滞后期数。
  - 返回值: 自相关函数值数组。

#### 5. PartialAutocorrelation
- `public static T[] PartialAutocorrelation<T>(this Span<T> series, int lagMax) where T : IFloatingPointIeee754<T>`
  - 计算时间序列的偏自相关函数。
  - 参数:
    - `series`: 时间序列数据。
    - `lagMax`: 最大滞后期数。
  - 返回值: 偏自相关函数值数组。

#### 6. MovingAverage
- `public static T[] MovingAverage<T>(this Span<T> series, int windowSize) where T : IFloatingPointIeee754<T>`
  - 计算时间序列的移动平均。
  - 参数:
    - `series`: 时间序列数据。
    - `windowSize`: 移动平均窗口的大小。
  - 返回值: 移动平均值数组。

#### 7. ExponentialSmoothing
- `public static T[] ExponentialSmoothing<T>(this Span<T> series, T alpha) where T : IFloatingPointIeee754<T>`
  - 实现时间序列的指数平滑。
  - 参数:
    - `series`: 时间序列数据。
    - `alpha`: 平滑系数。
  - 返回值: 指数平滑后的时间序列。

### 代码示例
以下是一个使用 TimeSeriesAnalysis 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.Statistics;

public class TimeSeriesAnalysisExample
{
    public static void Main()
    {
        // 定义时间序列数据
        Span<double> series = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };

        // 平滑
        double[] smoothedSeries = series.Smooth(3);
        Console.WriteLine("Smoothed Series: " + string.Join(", ", smoothedSeries));

        // 分解
        var (trend, seasonal, residual) = series.Decompose(3);
        Console.WriteLine("Trend: " + string.Join(", ", trend));
        Console.WriteLine("Seasonal: " + string.Join(", ", seasonal));
        Console.WriteLine("Residual: " + string.Join(", ", residual));

        // 预测
        double[] forecast = series.Forecast(5);
        Console.WriteLine("Forecast: " + string.Join(", ", forecast));

        // 自相关函数
        double[] acf = series.Autocorrelation(5);
        Console.WriteLine("Autocorrelation: " + string.Join(", ", acf));

        // 偏自相关函数
        double[] pacf = series.PartialAutocorrelation(5);
        Console.WriteLine("Partial Autocorrelation: " + string.Join(", ", pacf));

        // 移动平均
        double[] movingAverage = series.MovingAverage(3);
        Console.WriteLine("Moving Average: " + string.Join(", ", movingAverage));

        // 指数平滑
        double[] exponentialSmoothing = series.ExponentialSmoothing(0.5);
        Console.WriteLine("Exponential Smoothing: " + string.Join(", ", exponentialSmoothing));
    }
}
```



