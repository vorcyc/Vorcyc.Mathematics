当前位置 : [根目录](README.md)/[统计模块](Module_Statistics.md)/[基础统计函数](Module_Statistics_Basic.md)

# 统计模块 - Statistics Module
## 基础统计函数 - Basic Statistics Functions

`Vorcyc.Mathematics.Statistics.Basic` 是一个提供基本统计函数的类，包括均值、中位数、众数、方差、标准差和变异系数的计算方法。

> 以下方法均位于类 ：Vorcyc.Mathematics.Statistics.Basic


---

:ledger:目录  
- :bookmark: [Sum 方法](#1-sum-方法)  
- :bookmark: [Average 方法](#2-average-方法)  
- :bookmark: [Median 方法](#3-median-方法)  
- :bookmark: [Mode 方法](#4-mode-方法)  
- :bookmark: [Variance 方法](#5-variance-方法)  
- :bookmark: [StandardDeviation 方法](#6-standarddeviation-方法)  
- :bookmark: [CoefficientOfVariation 方法](#7-coefficientofvariation-方法)  
- :bookmark: [CalculateAllStatistics 方法](#8-calculateallstatistics-方法)  


---

## Vorcyc.Mathematics.Statistics.Basic 类

Vorcyc.Mathematics.Statistics.Basic 是一个提供基本统计函数的类，包括均值、中位数、众数、方差、标准差和变异系数的计算方法。

### 方法

#### 1. Sum 方法
- `public static T Sum<T>(this Span<T> values) where T : INumber<T>`
  - 计算一组值中元素的总和，使用 SIMD 进行优化。
  - 参数:
    - `values`: 要计算总和的值的一组值。
  - 返回值: 一组值中元素的总和。
  - 异常: 
    - `ArgumentException`: 当 `values` 为空时抛出。

- `public static T Sum<T>(this Span<T> values, Func<T, T> selector) where T : INumber<T>`
  - 计算一组值中元素的总和，使用指定的选择器函数进行选择。
  - 参数:
    - `values`: 要计算总和的值的一组值。
    - `selector`: 用于选择值的函数。
  - 返回值: 一组值中元素的总和。
  - 异常: 
    - `ArgumentException`: 当 `values` 为空时抛出。

#### 2. Average 方法
- `public static T Average<T>(this Span<T> values) where T : INumber<T>`
  - 计算一组值中元素的平均值。
  - 参数:
    - `values`: 要计算平均值的值的一组值。
  - 返回值: 一组值中元素的平均值。
  - 异常: 
    - `ArgumentException`: 当 `values` 为空时抛出。

#### 3. Median 方法
- `public static T Median<T>(this Span<T> values) where T : INumber<T>`
  - 计算一组值中元素的中位数。
  - 参数:
    - `values`: 要计算中位数的值的一组值。
  - 返回值: 一组值中元素的中位数。
  - 异常: 
    - `ArgumentException`: 当 `values` 为空时抛出。

#### 4. Mode 方法
- `public static T Mode<T>(this Span<T> values) where T : INumber<T>`
  - 计算一组值中元素的众数。
  - 参数:
    - `values`: 要计算众数的值的一组值。
  - 返回值: 一组值中出现频率最高的元素。
  - 异常: 
    - `ArgumentException`: 当 `values` 为空时抛出。

#### 5. Variance 方法
- `public static (T average, T variance) Variance<T>(this Span<T> values) where T : INumber<T>`
  - 计算一组值中元素的平均值和方差。
  - 参数:
    - `values`: 要计算平均值和方差的值的一组值。
  - 返回值: 包含一组值中元素的平均值和方差的元组。
  - 异常: 
    - `ArgumentException`: 当 `values` 为空时抛出。

#### 6. StandardDeviation 方法
- `public static T StandardDeviation<T>(this Span<T> values) where T : IFloatingPointIeee754<T>`
  - 计算一组值中元素的标准差。
  - 参数:
    - `values`: 要计算标准差的值的一组值。
  - 返回值: 一组值中元素的标准差。
  - 异常: 
    - `ArgumentException`: 当 `values` 为空时抛出。

#### 7. CoefficientOfVariation 方法
- `public static T CoefficientOfVariation<T>(this Span<T> values) where T : IFloatingPointIeee754<T>`
  - 计算一组值中元素的变异系数。
  - 参数:
    - `values`: 要计算变异系数的值的一组值。
  - 返回值: 一组值中元素的变异系数。
  - 异常: 
    - `ArgumentException`: 当 `values` 为空时抛出。

#### 8. CalculateAllStatistics 方法
- `public static (T Mean, T Median, T Mode, T Variance, T StandardDeviation, T CoefficientOfVariation) CalculateAllStatistics<T>(this Span<T> values) where T : IFloatingPointIeee754<T>`
  - 计算一组值的所有统计值，包括均值、中位数、众数、方差、标准差和变异系数。
  - 参数:
    - `values`: 要计算统计值的一组值。
  - 返回值: 包含均值、中位数、众数、方差、标准差和变异系数的元组。
  - 异常: 
    - `ArgumentException`: 当 `values` 为空时抛出。

### 代码示例
以下是一个使用 Basic 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.Statistics;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class BasicStatisticsExample
{
    public static void Main()
    {
        // 定义数据集
        Span<double> data = new double[] { 1.0, 2.0, 2.0, 3.0, 4.0, 5.0 };

        // 计算总和
        double sum = data.Sum();
        Console.WriteLine("Sum: " + sum);

        // 计算平均值
        double average = data.Average();
        Console.WriteLine("Average: " + average);

        // 计算中位数
        double median = data.Median();
        Console.WriteLine("Median: " + median);

        // 计算众数
        double mode = data.Mode();
        Console.WriteLine("Mode: " + mode);

        // 计算方差
        var (mean, variance) = data.Variance();
        Console.WriteLine("Variance: " + variance);

        // 计算标准差
        double standardDeviation = data.StandardDeviation();
        Console.WriteLine("Standard Deviation: " + standardDeviation);

        // 计算变异系数
        double coefficientOfVariation = data.CoefficientOfVariation();
        Console.WriteLine("Coefficient of Variation: " + coefficientOfVariation);

        // 计算所有统计值
        var (meanAll, medianAll, modeAll, varianceAll, standardDeviationAll, coefficientOfVariationAll) = data.CalculateAllStatistics();

        Console.WriteLine($"Mean: {meanAll}, Median: {medianAll}, Mode: {modeAll}, Variance: {varianceAll}, Standard Deviation: {standardDeviationAll}, Coefficient of Variation: {coefficientOfVariationAll}");
    }
}
```


   



