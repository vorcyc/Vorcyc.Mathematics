当前位置 : [根目录](README.md)/[统计模块](Module_Statistics.md)/[其他统计函数](Module_Statistics_Others.md)

# 统计模块 - Statistics Module
## 其他统计函数 - Others Statistics Functions

`Vorcyc.Mathematics.Statistics.Others` 是一个提供其他统计函数的类，包括协方差、相关系数和线性回归分析。

> 以下方法均位于类 ：Vorcyc.Mathematics.Statistics.Others

---

:ledger:目录  
- :bookmark: [Covariance 方法](#1-covariance-方法)  
- :bookmark: [CorrelationCoefficient 方法](#2-correlationcoefficient-方法)  
- :bookmark: [LinearRegression 方法](#3-linearregression-方法)  

---

## Vorcyc.Mathematics.Statistics.Others 类

Vorcyc.Mathematics.Statistics.Others 是一个提供其他统计函数的类，包括协方差、相关系数和线性回归分析。

### 方法

#### 1. Covariance
- `public static T Covariance<T>(this Span<T> x, Span<T> y) where T : INumber<T>`
  - 计算两组数据的协方差。
  - 参数:
    - `x`: 第一组数据。
    - `y`: 第二组数据。
  - 返回值: 两组数据的协方差。

#### 2. CorrelationCoefficient
- `public static T CorrelationCoefficient<T>(this Span<T> x, Span<T> y) where T : IFloatingPointIeee754<T>`
  - 计算两组数据的相关系数。
  - 参数:
    - `x`: 第一组数据。
    - `y`: 第二组数据。
  - 返回值: 两组数据的相关系数。

#### 3. LinearRegression
- `public static (T Slope, T Intercept) LinearRegression<T>(this Span<T> x, Span<T> y) where T : IFloatingPointIeee754<T>`
  - 实现简单的线性回归分析。
  - 参数:
    - `x`: 自变量数据。
    - `y`: 因变量数据。
  - 返回值: 包含回归系数和截距的元组。

### 代码示例
以下是一个使用 Others 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.Statistics;

public class OthersExample
{
    public static void Main()
    { 
        // 定义两组数据
        Span<double> x = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 }; 
        Span<double> y = new double[] { 2.0, 4.0, 6.0, 8.0, 10.0 };

      // 计算协方差
        double covariance = x.Covariance(y);
        Console.WriteLine("Covariance: " + covariance);

        // 计算相关系数
        double correlationCoefficient = x.CorrelationCoefficient(y);
        Console.WriteLine("Correlation Coefficient: " + correlationCoefficient);

        // 进行线性回归分析
        var (slope, intercept) = x.LinearRegression(y);
        Console.WriteLine($"Linear Regression: Slope = {slope}, Intercept = {intercept}");
    }
}
```



