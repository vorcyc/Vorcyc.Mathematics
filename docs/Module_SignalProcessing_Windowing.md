当前位置 : [根目录](README.md)/[信号处理模块](Module_SignalProcessing.md)/[窗函数](Module_SignalProcessing_Windowing.md)

# 信号处理模块 - Signal Processing Module
## 窗函数 - Windowing

> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Windowing

---

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Windowing.WindowApplier 类](#vorcycmathematicssignalprocessingwindowingwindowapplier-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Windowing.WindowBuilder 类](#vorcycmathematicssignalprocessingwindowingwindowbuilder-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Windowing.WindowBuilderExtensions 类](#vorcycmathematicssignalprocessingwindowingwindowbuilderextensions-类)  

---


> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Windowing 命名空间。


  
  ## Vorcyc.Mathematics.SignalProcessing.Windowing.WindowApplier 类

`Vorcyc.Mathematics.SignalProcessing.Windowing.WindowApplier` 提供了多种窗函数的计算方法。

### 方法

#### 1. Rectangular
- `public static void Rectangular(Span<ComplexFp32> values)`
  - 计算矩形窗函数。
  - 参数:
    - `values`: 输入数据。

#### 2. Triangular
- `public static void Triangular(Span<ComplexFp32> values)`
  - 计算三角窗函数。
  - 参数:
    - `values`: 输入数据。

#### 3. Hamming
- `public static void Hamming(Span<ComplexFp32> values)`
  - 计算汉明窗函数。
  - 参数:
    - `values`: 输入数据。

#### 4. Blackman
- `public static void Blackman(Span<ComplexFp32> values)`
  - 计算布莱克曼窗函数。
  - 参数:
    - `values`: 输入数据。

#### 5. Hann
- `public static void Hann(Span<ComplexFp32> values)`
  - 计算汉宁窗函数。
  - 参数:
    - `values`: 输入数据。

#### 6. Gaussian
- `public static void Gaussian(Span<ComplexFp32> values)`
  - 计算高斯窗函数。
  - 参数:
    - `values`: 输入数据。

#### 7. Kaiser
- `public static void Kaiser(Span<ComplexFp32> values, float alpha = 12f)`
  - 计算凯撒窗函数。
  - 参数:
    - `values`: 输入数据。
    - `alpha`: Alpha 参数。

#### 8. Kbd
- `public static void Kbd(Span<ComplexFp32> values, float alpha = 4f)`
  - 计算凯撒-贝塞尔派生窗函数。
  - 参数:
    - `values`: 输入数据。
    - `alpha`: Alpha 参数。

#### 9. Bartlett_Hann
- `public static void Bartlett_Hann(Span<ComplexFp32> values)`
  - 计算巴特利特-汉宁窗函数。
  - 参数:
    - `values`: 输入数据。

#### 10. Lanczos
- `public static void Lanczos(Span<ComplexFp32> values)`
  - 计算兰索斯窗函数。
  - 参数:
    - `values`: 输入数据。

#### 11. PowerOfSine
- `public static void PowerOfSine(Span<ComplexFp32> values, float alpha = 1.5f)`
  - 计算幂正弦窗函数。
  - 参数:
    - `values`: 输入数据。
    - `alpha`: Alpha 参数。

#### 12. Flattop
- `public static void Flattop(Span<ComplexFp32> values)`
  - 计算平顶窗函数。
  - 参数:
    - `values`: 输入数据。

#### 13. Liftering
- `public static void Liftering(Span<ComplexFp32> values, int l = 22)`
  - 计算升降窗函数。
  - 参数:
    - `values`: 输入数据。
    - `l`: L 参数。

#### 14. Blackman_Harris
- `public static void Blackman_Harris(Span<ComplexFp32> values)`
  - 计算布莱克曼-哈里斯窗函数。
  - 参数:
    - `values`: 输入数据。

#### 15. Apply
- `public static void Apply(Span<ComplexFp32> values, WindowType windowType)`
  - 应用指定的窗函数。
  - 参数:
    - `values`: 输入数据。
    - `windowType`: 窗函数类型。

### 代码示例
以下是一个使用 WindowApplier 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

public class WindowApplierExample
{
    public static void Main()
    {
        // 定义输入数据
        var values = new ComplexFp32[] { new ComplexFp32(1.0f, 0.0f), new ComplexFp32(2.0f, 0.0f), new ComplexFp32(3.0f, 0.0f), new ComplexFp32(4.0f, 0.0f), new ComplexFp32(5.0f, 0.0f), new ComplexFp32(6.0f, 0.0f), new ComplexFp32(7.0f, 0.0f), new ComplexFp32(8.0f, 0.0f) };

        // 应用三角窗函数
        WindowApplier.Triangular(values);
        Console.WriteLine("Triangular Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用汉明窗函数
        WindowApplier.Hamming(values);
        Console.WriteLine("Hamming Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用布莱克曼窗函数
        WindowApplier.Blackman(values);
        Console.WriteLine("Blackman Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用汉宁窗函数
        WindowApplier.Hann(values);
        Console.WriteLine("Hann Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用高斯窗函数
        WindowApplier.Gaussian(values);
        Console.WriteLine("Gaussian Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用凯撒窗函数
        WindowApplier.Kaiser(values);
        Console.WriteLine("Kaiser Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用凯撒-贝塞尔派生窗函数
        WindowApplier.Kbd(values);
        Console.WriteLine("Kbd Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用巴特利特-汉宁窗函数
        WindowApplier.Bartlett_Hann(values);
        Console.WriteLine("Bartlett-Hann Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用兰索斯窗函数
        WindowApplier.Lanczos(values);
        Console.WriteLine("Lanczos Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用幂正弦窗函数
        WindowApplier.PowerOfSine(values);
        Console.WriteLine("PowerOfSine Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用平顶窗函数
        WindowApplier.Flattop(values);
        Console.WriteLine("Flattop Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用升降窗函数
        WindowApplier.Liftering(values);
        Console.WriteLine("Liftering Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        // 应用布莱克曼-哈里斯窗函数
        WindowApplier.Blackman_Harris(values);
        Console.WriteLine("Blackman-Harris Window:");

        foreach (var value in values)
        {
            Console.WriteLine(value);
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Windowing.WindowBuilder 类

`Vorcyc.Mathematics.SignalProcessing.Windowing.WindowBuilder` 提供了生成各种类型窗函数系数的方法。

### 方法

#### 1. OfType
- `public static float[] OfType(WindowType type, int length, params object[] parameters)`
  - 生成给定类型和长度的窗函数系数。
  - 参数:
    - `type`: 窗函数类型。
    - `length`: 窗函数长度。
    - `parameters`: 其他可选参数。
  - 返回值: 窗函数系数数组。

#### 2. Rectangular
- `public static float[] Rectangular(int length)`
  - 生成给定长度的矩形窗函数。
  - 参数:
    - `length`: 窗函数长度。
  - 返回值: 窗函数系数数组。

#### 3. Triangular
- `public static float[] Triangular(int length)`
  - 生成给定长度的三角窗函数。
  - 参数:
    - `length`: 窗函数长度。
  - 返回值: 窗函数系数数组。

#### 4. Hamming
- `public static float[] Hamming(int length)`
  - 生成给定长度的汉明窗函数。
  - 参数:
    - `length`: 窗函数长度。
  - 返回值: 窗函数系数数组。

#### 5. Blackman
- `public static float[] Blackman(int length)`
  - 生成给定长度的布莱克曼窗函数。
  - 参数:
    - `length`: 窗函数长度。
  - 返回值: 窗函数系数数组。

#### 6. Hann
- `public static float[] Hann(int length)`
  - 生成给定长度的汉宁窗函数。
  - 参数:
    - `length`: 窗函数长度。
  - 返回值: 窗函数系数数组。

#### 7. Gaussian
- `public static float[] Gaussian(int length)`
  - 生成给定长度的高斯窗函数。
  - 参数:
    - `length`: 窗函数长度。
  - 返回值: 窗函数系数数组。

#### 8. Kaiser
- `public static float[] Kaiser(int length, double alpha = 12.0)`
  - 生成给定长度和 alpha 参数的凯撒窗函数。
  - 参数:
    - `length`: 窗函数长度。
    - `alpha`: Alpha 参数。
  - 返回值: 窗函数系数数组。

#### 9. Kbd
- `public static float[] Kbd(int length, double alpha = 4.0)`
  - 生成给定长度和 alpha 参数的凯撒-贝塞尔派生窗函数。
  - 参数:
    - `length`: 窗函数长度。
    - `alpha`: Alpha 参数。
  - 返回值: 窗函数系数数组。

#### 10. BartlettHann
- `public static float[] BartlettHann(int length)`
  - 生成给定长度的巴特利特-汉宁窗函数。
  - 参数:
    - `length`: 窗函数长度。
  - 返回值: 窗函数系数数组。

#### 11. Lanczos
- `public static float[] Lanczos(int length)`
  - 生成给定长度的兰索斯窗函数。
  - 参数:
    - `length`: 窗函数长度。
  - 返回值: 窗函数系数数组。

#### 12. PowerOfSine
- `public static float[] PowerOfSine(int length, double alpha = 1.5)`
  - 生成给定长度和 alpha 参数的幂正弦窗函数。
  - 参数:
    - `length`: 窗函数长度。
    - `alpha`: Alpha 参数。
  - 返回值: 窗函数系数数组。

#### 13. Flattop
- `public static float[] Flattop(int length)`
  - 生成给定长度的平顶窗函数。
  - 参数:
    - `length`: 窗函数长度。
  - 返回值: 窗函数系数数组。

#### 14. Liftering
- `public static float[] Liftering(int length, int l = 22)`
  - 生成给定长度和 l 参数的升降窗函数。
  - 参数:
    - `length`: 窗函数长度。
    - `l`: L 参数。
  - 返回值: 窗函数系数数组。

#### 15. Blackman_Harris
- `public static float[] Blackman_Harris(int length)`
  - 生成给定长度的布莱克曼-哈里斯窗函数。
  - 参数:
    - `length`: 窗函数长度。
  - 返回值: 窗函数系数数组。

### 代码示例
以下是一个使用 WindowBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

public class WindowBuilderExample
{
    public static void Main()
    {
        // 定义窗函数长度
        int length = 8;

        // 生成三角窗函数系数
        var triangular = WindowBuilder.Triangular(length);
        Console.WriteLine("Triangular Window:");

        foreach (var value in triangular)
        {
            Console.WriteLine(value);
        }

        // 生成汉明窗函数系数
        var hamming = WindowBuilder.Hamming(length);
        Console.WriteLine("Hamming Window:");

        foreach (var value in hamming)
        {
            Console.WriteLine(value);
        }

        // 生成布莱克曼窗函数系数
        var blackman = WindowBuilder.Blackman(length);
        Console.WriteLine("Blackman Window:");

        foreach (var value in blackman)
        {
            Console.WriteLine(value);
        }

        // 生成汉宁窗函数系数
        var hann = WindowBuilder.Hann(length);
        Console.WriteLine("Hann Window:");

        foreach (var value in hann)
        {
            Console.WriteLine(value);
        }

        // 生成高斯窗函数系数
        var gaussian = WindowBuilder.Gaussian(length);
        Console.WriteLine("Gaussian Window:");

        foreach (var value in gaussian)
        {
            Console.WriteLine(value);
        }

        // 生成凯撒窗函数系数
        var kaiser = WindowBuilder.Kaiser(length);
        Console.WriteLine("Kaiser Window:");

        foreach (var value in kaiser)
        {
            Console.WriteLine(value);
        }

        // 生成凯撒-贝塞尔派生窗函数系数
        var kbd = WindowBuilder.Kbd(length);
        Console.WriteLine("Kbd Window:");

        foreach (var value in kbd)
        {
            Console.WriteLine(value);
        }

        // 生成巴特利特-汉宁窗函数系数
        var bartlettHann = WindowBuilder.BartlettHann(length);
        Console.WriteLine("Bartlett-Hann Window:");

        foreach (var value in bartlettHann)
        {
            Console.WriteLine(value);
        }

        // 生成兰索斯窗函数系数
        var lanczos = WindowBuilder.Lanczos(length);
        Console.WriteLine("Lanczos Window:");

        foreach (var value in lanczos)
        {
            Console.WriteLine(value);
        }

        // 生成幂正弦窗函数系数
        var powerOfSine = WindowBuilder.PowerOfSine(length);
        Console.WriteLine("PowerOfSine Window:");

        foreach (var value in powerOfSine)
        {
            Console.WriteLine(value);
        }

        // 生成平顶窗函数系数
        var flattop = WindowBuilder.Flattop(length);
        Console.WriteLine("Flattop Window:");

        foreach (var value in flattop)
        {
            Console.WriteLine(value);
        }

        // 生成升降窗函数系数
        var liftering = WindowBuilder.Liftering(length);
        Console.WriteLine("Liftering Window:");

        foreach (var value in liftering)
        {
            Console.WriteLine(value);
        }

        // 生成布莱克曼-哈里斯窗函数系数
        var blackmanHarris = WindowBuilder.Blackman_Harris(length);
        Console.WriteLine("Blackman-Harris Window:");

        foreach (var value in blackmanHarris)
        {
            Console.WriteLine(value);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Windowing.WindowBuilderExtensions 类

`Vorcyc.Mathematics.SignalProcessing.Windowing.WindowBuilderExtensions` 提供了应用窗函数到信号和样本数组的扩展方法。

### 方法

#### 1. ApplyWindow (float[])
- `public static void ApplyWindow(this float[] samples, float[] windowSamples)`
  - 将窗函数应用到样本数组。
  - 参数:
    - `samples`: 样本数组。
    - `windowSamples`: 窗函数系数。

#### 2. ApplyWindow (double[])
- `public static void ApplyWindow(this double[] samples, double[] windowSamples)`
  - 将窗函数应用到样本数组。
  - 参数:
    - `samples`: 样本数组。
    - `windowSamples`: 窗函数系数。

#### 3. ApplyWindow (DiscreteSignal)
- `public static void ApplyWindow(this DiscreteSignal signal, float[] windowSamples)`
  - 将窗函数应用到信号。
  - 参数:
    - `signal`: 信号。
    - `windowSamples`: 窗函数系数。

#### 4. ApplyWindow (float[], WindowType)
- `public static void ApplyWindow(this float[] samples, WindowType window, params object[] parameters)`
  - 将指定类型的窗函数应用到样本数组。
  - 参数:
    - `samples`: 样本数组。
    - `window`: 窗函数类型。
    - `parameters`: 窗函数参数。

#### 5. ApplyWindow (double[], WindowType)
- `public static void ApplyWindow(this double[] samples, WindowType window, params object[] parameters)`
  - 将指定类型的窗函数应用到样本数组。
  - 参数:
    - `samples`: 样本数组。
    - `window`: 窗函数类型。
    - `parameters`: 窗函数参数。

#### 6. ApplyWindow (DiscreteSignal, WindowType)
- `public static void ApplyWindow(this DiscreteSignal signal, WindowType window, params object[] parameters)`
  - 将指定类型的窗函数应用到信号。
  - 参数:
    - `signal`: 信号。
    - `window`: 窗函数类型。
    - `parameters`: 窗函数参数。

### 代码示例
以下是一个使用 WindowBuilderExtensions 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Windowing;
public class WindowBuilderExtensionsExample
{
    public static void Main()
    {
        // 定义样本数组
        var samples = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };

        // 定义窗函数系数
        var windowSamples = WindowBuilder.Hamming(samples.Length);

        // 将窗函数应用到样本数组
        samples.ApplyWindow(windowSamples);
        Console.WriteLine("Samples after applying Hamming window:");

        foreach (var sample in samples)
        {
            Console.WriteLine(sample);
        }

        // 使用指定类型的窗函数应用到样本数组
        samples.ApplyWindow(WindowType.Blackman);
        Console.WriteLine("Samples after applying Blackman window:");

        foreach (var sample in samples)
        {
            Console.WriteLine(sample);
        }

        // 定义信号
        var signal = new DiscreteSignal(samples.Length, samples);

        // 将窗函数应用到信号
        signal.ApplyWindow(windowSamples);
        Console.WriteLine("Signal after applying Hamming window:");

        foreach (var sample in signal.Samples.Values)
        {
            Console.WriteLine(sample);
        }

        // 使用指定类型的窗函数应用到信号
        signal.ApplyWindow(WindowType.Blackman);
        Console.WriteLine("Signal after applying Blackman window:");

        foreach (var sample in signal.Samples.Values)
        {
            Console.WriteLine(sample);
        }
    }
}
```

