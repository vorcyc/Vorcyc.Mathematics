当前位置 : [根目录](README.md)/[统计模块](Module_Statistics.md)/[分布计算](Module_Statistics_Distribution.md)

# 统计模块 - Statistics Module
## 分布计算 - Distribution

`Vorcyc.Mathematics.Statistics.Distribution` 是一个提供各种概率分布计算方法的类，包括正态分布、泊松分布、指数分布、二项分布、多项分布、Gamma分布和Beta分布。

> 以下方法均位于类 ：Vorcyc.Mathematics.Statistics.Distribution


---

:ledger:目录  
- :bookmark: [NormalPDF 方法](#1-normalpdf-方法)  
- :bookmark: [NormalCDF 方法](#2-normalcdf-方法)  
- :bookmark: [PoissonPMF 方法](#3-poissonpmf-方法)  
- :bookmark: [PoissonCDF 方法](#4-poissoncdf-方法)  
- :bookmark: [ExponentialPDF 方法](#5-exponentialpdf-方法)  
- :bookmark: [ExponentialCDF 方法](#6-exponentialcdf-方法)  
- :bookmark: [BinomialPMF 方法](#7-binomialpmf-方法)  
- :bookmark: [BinomialCDF 方法](#8-binomialcdf-方法)  
- :bookmark: [MultinomialPMF 方法](#9-multinomialpmf-方法)  
- :bookmark: [GammaPDF 方法](#10-gammapdf-方法)  
- :bookmark: [GammaCDF 方法](#11-gammacdf-方法)  
- :bookmark: [BetaPDF 方法](#12-betapdf-方法)  
- :bookmark: [BetaCDF 方法](#13-betacdf-方法)  

---

## Vorcyc.Mathematics.Statistics.Distribution 类

Vorcyc.Mathematics.Statistics.Distribution 是一个提供各种概率分布计算方法的类，包括正态分布、泊松分布、指数分布、二项分布、多项分布、Gamma分布和Beta分布。

### 方法

#### 1. NormalPDF 方法
- `public static T NormalPDF<T>(T x, T mean, T stdDev) where T : IFloatingPointIeee754<T>`
  - 计算正态分布的概率密度函数。
  - 参数:
    - `x`: 变量值。
    - `mean`: 均值。
    - `stdDev`: 标准差。
  - 返回值: 正态分布的概率密度值。

#### 2. NormalCDF 方法
- `public static T NormalCDF<T>(T x, T mean, T stdDev) where T : IFloatingPointIeee754<T>`
  - 计算正态分布的累积分布函数。
  - 参数:
    - `x`: 变量值。
    - `mean`: 均值。
    - `stdDev`: 标准差。
  - 返回值: 正态分布的累积分布值。

#### 3. PoissonPMF 方法
- `public static T PoissonPMF<T>(int k, T lambda) where T : IFloatingPointIeee754<T>`
  - 计算泊松分布的概率质量函数。
  - 参数:
    - `k`: 事件发生的次数。
    - `lambda`: 单位时间内事件的平均发生率。
  - 返回值: 泊松分布的概率质量值。

#### 4. PoissonCDF 方法
- `public static T PoissonCDF<T>(int k, T lambda) where T : IFloatingPointIeee754<T>`
  - 计算泊松分布的累积分布函数。
  - 参数:
    - `k`: 事件发生的次数。
    - `lambda`: 单位时间内事件的平均发生率。
  - 返回值: 泊松分布的累积分布值。

#### 5. ExponentialPDF 方法
- `public static T ExponentialPDF<T>(T x, T lambda) where T : IFloatingPointIeee754<T>`
  - 计算指数分布的概率密度函数。
  - 参数:
    - `x`: 变量值。
    - `lambda`: 分布的参数。
  - 返回值: 指数分布的概率密度值。

#### 6. ExponentialCDF 方法
- `public static T ExponentialCDF<T>(T x, T lambda) where T : IFloatingPointIeee754<T>`
  - 计算指数分布的累积分布函数。
  - 参数:
    - `x`: 变量值。
    - `lambda`: 分布的参数。
  - 返回值: 指数分布的累积分布值。

#### 7. BinomialPMF 方法
- `public static T BinomialPMF<T>(int k, int n, T p) where T : IFloatingPointIeee754<T>`
  - 计算二项分布的概率质量函数。
  - 参数:
    - `k`: 成功的次数。
    - `n`: 试验的总次数。
    - `p`: 每次试验成功的概率。
  - 返回值: 二项分布的概率质量值。

#### 8. BinomialCDF 方法
- `public static T BinomialCDF<T>(int k, int n, T p) where T : IFloatingPointIeee754<T>`
  - 计算二项分布的累积分布函数。
  - 参数:
    - `k`: 成功的次数。
    - `n`: 试验的总次数。
    - `p`: 每次试验成功的概率。
  - 返回值: 二项分布的累积分布值。

#### 9. MultinomialPMF 方法
- `public static T MultinomialPMF<T>(int[] counts, T[] probabilities) where T : IFloatingPointIeee754<T>`
  - 计算多项分布的概率质量函数。
  - 参数:
    - `counts`: 每个类别的计数。
    - `probabilities`: 每个类别的概率。
  - 返回值: 多项分布的概率质量值。

#### 10. GammaPDF 方法
- `public static T GammaPDF<T>(T x, T shape, T scale) where T : IFloatingPointIeee754<T>`
  - 计算Gamma分布的概率密度函数。
  - 参数:
    - `x`: 变量值。
    - `shape`: 形状参数。
    - `scale`: 尺度参数。
  - 返回值: Gamma分布的概率密度值。

#### 11. GammaCDF 方法
- `public static T GammaCDF<T>(T x, T shape, T scale) where T : IFloatingPointIeee754<T>`
  - 计算Gamma分布的累积分布函数。
  - 参数:
    - `x`: 变量值。
    - `shape`: 形状参数。
    - `scale`: 尺度参数。
  - 返回值: Gamma分布的累积分布值。

#### 12. BetaPDF 方法
- `public static T BetaPDF<T>(T x, T alpha, T beta) where T : IFloatingPointIeee754<T>`
  - 计算Beta分布的概率密度函数。
  - 参数:
    - `x`: 变量值。
    - `alpha`: 形状参数α。
    - `beta`: 形状参数β。
  - 返回值: Beta分布的概率密度值。

#### 13. BetaCDF 方法
- `public static T BetaCDF<T>(T x, T alpha, T beta) where T : IFloatingPointIeee754<T>`
  - 计算Beta分布的累积分布函数。
  - 参数:
    - `x`: 变量值。
    - `alpha`: 形状参数α。
    - `beta`: 形状参数β。
  - 返回值: Beta分布的累积分布值。

### 代码示例
以下是一个使用 Distribution 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.Statistics;

public class DistributionExample
{
    public static void Main()
    { 
        // 正态分布示例
        double normalPDF = Distribution.NormalPDF(1.0, 0.0, 1.0); 
        double normalCDF = Distribution.NormalCDF(1.0, 0.0, 1.0); 
        Console.WriteLine($"Normal PDF: {normalPDF}, Normal CDF: {normalCDF}");

      // 泊松分布示例
        double poissonPMF = Distribution.PoissonPMF(3, 2.0);
        double poissonCDF = Distribution.PoissonCDF(3, 2.0);
        Console.WriteLine($"Poisson PMF: {poissonPMF}, Poisson CDF: {poissonCDF}");

        // 指数分布示例
        double exponentialPDF = Distribution.ExponentialPDF(1.0, 1.0);
        double exponentialCDF = Distribution.ExponentialCDF(1.0, 1.0);
        Console.WriteLine($"Exponential PDF: {exponentialPDF}, Exponential CDF: {exponentialCDF}");

        // 二项分布示例
        double binomialPMF = Distribution.BinomialPMF(2, 5, 0.5);
        double binomialCDF = Distribution.BinomialCDF(2, 5, 0.5);
        Console.WriteLine($"Binomial PMF: {binomialPMF}, Binomial CDF: {binomialCDF}");

        // 多项分布示例
        int[] counts = { 2, 3, 1 };
        double[] probabilities = { 0.2, 0.5, 0.3 };
        double multinomialPMF = Distribution.MultinomialPMF(counts, probabilities);
        Console.WriteLine($"Multinomial PMF: {multinomialPMF}");

        // Gamma分布示例
        double gammaPDF = Distribution.GammaPDF(2.0, 2.0, 2.0);
        double gammaCDF = Distribution.GammaCDF(2.0, 2.0, 2.0);
        Console.WriteLine($"Gamma PDF: {gammaPDF}, Gamma CDF: {gammaCDF}");

        // Beta分布示例
        double betaPDF = Distribution.BetaPDF(0.5, 2.0, 2.0);
        double betaCDF = Distribution.BetaCDF(0.5, 2.0, 2.0);
        Console.WriteLine($"Beta PDF: {betaPDF}, Beta CDF: {betaCDF}");
    }
}
```