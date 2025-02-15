当前位置 : [根目录](README.md)/[统计模块](Module_Statistics.md)/[高级统计函数](Module_Statistics_Advanced.md)

# 统计模块 - Statistics Module
## 高级统计函数 - Advanced Statistics Functions

`Vorcyc.Mathematics.Statistics.Advanced` 是一个提供高级统计函数的类，包括百分位数、四分位数、偏度、峰度、置信区间、假设检验、方差分析、卡方检验和非参数检验的计算方法。

> 以下方法均位于类 ：Vorcyc.Mathematics.Statistics.Advanced


---

:ledger:目录  
- :bookmark: [Percentile 方法](#1-percentile-方法)  
- :bookmark: [Quartiles 方法](#2-quartiles-方法)  
- :bookmark: [Skewness 方法](#3-skewness-方法)  
- :bookmark: [Kurtosis 方法](#4-kurtosis-方法)  
- :bookmark: [ConfidenceInterval 方法](#5-confidenceinterval-方法)  
- :bookmark: [TTest 方法](#6-ttest-方法)  
- :bookmark: [Anova 方法](#7-anova-方法)  
- :bookmark: [ChiSquaredTest 方法](#8-chisquaredtest-方法)  
- :bookmark: [MannWhitneyUTest 方法](#9-mannwhitneyutest-方法)  


---

## Vorcyc.Mathematics.Statistics.Advanced 类

Vorcyc.Mathematics.Statistics.Advanced 是一个提供高级统计函数的类，包括百分位数、四分位数、偏度、峰度、置信区间、假设检验、方差分析、卡方检验和非参数检验的计算方法。

### 方法

#### 1. Percentile
- `public static T Percentile<T>(this Span<T> sequence, double percentile) where T : IFloatingPointIeee754<T>`
  - 计算数据集的指定百分位数。
  - 参数:
    - `sequence`: 数据集。
    - `percentile`: 百分位数（0到1之间）。
  - 返回值: 指定百分位数的值。

#### 2. Quartiles
- `public static (T Q1, T Q2, T Q3) Quartiles<T>(this Span<T> sequence) where T : IFloatingPointIeee754<T>`
  - 计算数据集的四分位数。
  - 参数:
    - `sequence`: 数据集。
  - 返回值: 包含第一、第二和第三四分位数的元组。

#### 3. Skewness
- `public static T Skewness<T>(this Span<T> sequence) where T : IFloatingPointIeee754<T>`
  - 计算数据集的偏度，衡量数据分布的对称性。
  - 参数:
    - `sequence`: 数据集。
  - 返回值: 数据集的偏度值。

#### 4. Kurtosis
- `public static T Kurtosis<T>(this Span<T> sequence) where T : IFloatingPointIeee754<T>`
  - 计算数据集的峰度，衡量数据分布的尖锐程度。
  - 参数:
    - `sequence`: 数据集。
  - 返回值: 数据集的峰度值。

#### 5. ConfidenceInterval
- `public static (T Lower, T Upper) ConfidenceInterval<T>(this Span<T> sequence, double confidenceLevel) where T : IFloatingPointIeee754<T>`
  - 计算均值或比例的置信区间。
  - 参数:
    - `sequence`: 数据集。
    - `confidenceLevel`: 置信水平（例如0.95表示95%的置信水平）。
  - 返回值: 包含置信区间下限和上限的元组。

#### 6. TTest
- `public static T TTest<T>(this Span<T> sample, T populationMean) where T : IFloatingPointIeee754<T>`
  - 实现各种假设检验，如z检验、t检验、卡方检验等。
  - 参数:
    - `sample`: 样本数据集。
    - `populationMean`: 总体均值。
  - 返回值: t检验的统计量。

#### 7. Anova
- `public static T Anova<T>(this IEnumerable<ArraySegment<T>> groups) where T : IFloatingPointIeee754<T>`
  - 实现单因素和多因素方差分析。
  - 参数:
    - `groups`: 数据组的集合。
  - 返回值: 方差分析的F值。

#### 8. ChiSquaredTest
- `public static T ChiSquaredTest<T>(this Span<T> observed, Span<T> expected) where T : IFloatingPointIeee754<T>`
  - 实现卡方独立性检验和拟合优度检验。
  - 参数:
    - `observed`: 观察值数据集。
    - `expected`: 期望值数据集。
  - 返回值: 卡方检验的统计量。

#### 9. MannWhitneyUTest
- `public static T MannWhitneyUTest<T>(this Span<T> sample1, Span<T> sample2) where T : IFloatingPointIeee754<T>`
  - 实现如曼-惠特尼U检验、克鲁斯卡尔-沃利斯检验等非参数检验。
  - 参数:
    - `sample1`: 样本1数据集。
    - `sample2`: 样本2数据集。
  - 返回值: 曼-惠特尼U检验的统计量。

### 代码示例
以下是一个使用 Advanced 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.Statistics;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class AdvancedStatisticsExample
{
    public static void Main()
    {
        // 定义数据集
        Span<double> data = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // 计算百分位数
        double percentile = data.Percentile(0.9);
        Console.WriteLine("90th Percentile: " + percentile);

        // 计算四分位数
        var (Q1, Q2, Q3) = data.Quartiles();
        Console.WriteLine($"Q1: {Q1}, Q2: {Q2}, Q3: {Q3}");

        // 计算偏度
        double skewness = data.Skewness();
        Console.WriteLine("Skewness: " + skewness);

        // 计算峰度
        double kurtosis = data.Kurtosis();
        Console.WriteLine("Kurtosis: " + kurtosis);

        // 计算置信区间
        var (lower, upper) = data.ConfidenceInterval(0.95);
        Console.WriteLine($"95% Confidence Interval: ({lower}, {upper})");

        // 进行t检验
        double tTest = data.TTest(3.0);
        Console.WriteLine("T-Test: " + tTest);

        // 进行方差分析
        var groups = new List<ArraySegment<double>>
    {
        new ArraySegment<double>(new double[] { 1.0, 2.0, 3.0 }),
        new ArraySegment<double>(new double[] { 4.0, 5.0, 6.0 }),
        new ArraySegment<double>(new double[] { 7.0, 8.0, 9.0 })
    };
        double anova = groups.Anova();
        Console.WriteLine("ANOVA: " + anova);

        // 进行卡方检验
        Span<double> observed = new double[] { 10, 20, 30 };
        Span<double> expected = new double[] { 15, 25, 35 };
        double chiSquared = observed.ChiSquaredTest(expected);
        Console.WriteLine("Chi-Squared Test: " + chiSquared);

        // 进行曼-惠特尼U检验
        Span<double> sample1 = new double[] { 1.0, 2.0, 3.0 };
        Span<double> sample2 = new double[] { 4.0, 5.0, 6.0 };
        double mannWhitneyU = sample1.MannWhitneyUTest(sample2);
        Console.WriteLine("Mann-Whitney U Test: " + mannWhitneyU);
    }
}
```


