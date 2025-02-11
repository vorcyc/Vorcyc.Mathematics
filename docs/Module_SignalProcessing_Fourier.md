当前位置 : [根目录](README.md)/[信号处理模块](Module_SignalProcessing.md)/[傅立叶变换](Module_SignalProcessing_Fourier.md)

# 信号处理模块 - Signal Processing Module
## 一维滤波器 - Filters

> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Fourier

---

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Fourier.CudaFFT 类](#vorcycmathematicssignalprocessingfouriercudafft-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Fourier.FastFourierTransform 类](#vorcycmathematicssignalprocessingfourierfastfouriertransform-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Fourier.RealOnlyFFT_Fp32 类](#vorcycmathematicssignalprocessingfourierrealonlyfft_fp32-类)  

---


## Vorcyc.Mathematics.SignalProcessing.Fourier.CudaFFT 类

Vorcyc.Mathematics.SignalProcessing.Fourier.CudaFFT 是一个用于实现基于 CUDA 的 FFT 操作的类。

### 属性

无公开属性。

### 方法

#### 1. CudaFFT 构造函数
- `public CudaFFT()`
  - 构造 CudaFFT 实例，使用默认的 CUDA 设备。
  - 异常:
    - `PlatformNotSupportedException`: 如果未检测到 CUDA 设备。

#### 2. Forward
- `public void Forward(float[] input, out Span<ComplexFp32> output)`
  - 执行前向 FFT 变换。
  - 参数:
    - `input`: 输入信号数组。
    - `output`: 输出复数信号的 Span。

#### 3. Inverse
- `public void Inverse(Span<ComplexFp32> input, out Span<ComplexFp32> output)`
  - 执行逆向 FFT 变换。
  - 参数:
    - `input`: 输入复数信号的 Span。
    - `output`: 输出复数信号的 Span。

#### 4. Dispose
- `public void Dispose()`
  - 释放 CudaFFT 实例的资源。

### 代码示例
以下是一个使用 CudaFFT 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

public class CudaFFTExample
{
    public static void Main()
    {
        // 创建 CudaFFT 实例
        using var cudaFFT = new CudaFFT();

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 执行前向 FFT 变换
        cudaFFT.Forward(inputSignal, out var forwardOutput);
        Console.WriteLine("Forward FFT Output:");
        foreach (var sample in forwardOutput)
        {
            Console.WriteLine($"Real: {sample.Real}, Imaginary: {sample.Imaginary}");
        }

        // 执行逆向 FFT 变换
        cudaFFT.Inverse(forwardOutput, out var inverseOutput);
        Console.WriteLine("Inverse FFT Output:");
        foreach (var sample in inverseOutput)
        {
            Console.WriteLine($"Real: {sample.Real}, Imaginary: {sample.Imaginary}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Fourier.FastFourierTransform 类

Vorcyc.Mathematics.SignalProcessing.Fourier.FastFourierTransform 是一个用于实现快速傅里叶变换（FFT）的静态类。

### 属性

无公开属性。

### 方法

#### 1. Forward
- `public static bool Forward(float* input, ComplexFp32* output, int N)`
  - 执行前向 FFT 变换，将实数输入转换为复数输出。
  - 参数:
    - `input`: 输入数据，实数序列（时域）。
    - `output`: 变换结果，复数序列。
    - `N`: FFT 长度。
  - 返回值: 是否成功。

- `public static bool Forward(float[] input, int offset, out ComplexFp32[] output, int N)`
  - 执行前向 FFT 变换，将实数输入转换为复数输出。
  - 参数:
    - `input`: 输入数据，实数序列（时域）。
    - `offset`: 输入数据的偏移量。
    - `output`: 变换结果，复数序列。
    - `N`: FFT 长度。
  - 返回值: 是否成功。

- `public static bool Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output)`
  - 执行前向 FFT 变换，将实数输入转换为复数输出。
  - 参数:
    - `input`: 输入数据，实数序列（时域）。
    - `output`: 变换结果，复数序列。
  - 返回值: 是否成功。

- `public static bool Forward(ComplexFp32* input, ComplexFp32* output, int N)`
  - 执行前向 FFT 变换，将复数输入转换为复数输出。
  - 参数:
    - `input`: 输入数据，复数序列（时域）。
    - `output`: 变换结果，复数序列。
    - `N`: FFT 长度。
  - 返回值: 是否成功。

- `public static bool Forward(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output)`
  - 执行前向 FFT 变换，将复数输入转换为复数输出。
  - 参数:
    - `input`: 输入数据，复数序列（时域）。
    - `output`: 变换结果，复数序列。
  - 返回值: 是否成功。

- `public static bool Forward(ComplexFp32[] input, int offset, ComplexFp32[] output, int N)`
  - 执行前向 FFT 变换，将复数输入转换为复数输出。
  - 参数:
    - `input`: 输入数据，复数序列（时域）。
    - `offset`: 输入数据的偏移量。
    - `output`: 变换结果，复数序列。
    - `N`: FFT 长度。
  - 返回值: 是否成功。

- `public static bool Forward(ComplexFp32* data, int N)`
  - 执行前向 FFT 变换，原地版本。
  - 参数:
    - `data`: 输入和输出数据，复数序列。
    - `N`: FFT 长度。
  - 返回值: 是否成功。

- `public static bool Forward(ComplexFp32[] data, int offset, int N)`
  - 执行前向 FFT 变换，原地版本。
  - 参数:
    - `data`: 输入和输出数据，复数序列。
    - `offset`: 数据的偏移量。
    - `N`: FFT 长度。
  - 返回值: 是否成功。

- `public static bool Forward(Span<ComplexFp32> data)`
  - 执行前向 FFT 变换，原地版本。
  - 参数:
    - `data`: 输入和输出数据，复数序列。
  - 返回值: 是否成功。

#### 2. Inverse
- `public static bool Inverse(ComplexFp32* input, ComplexFp32* output, int N, bool scale = true)`
  - 执行逆向 FFT 变换，将复数输入转换为复数输出。
  - 参数:
    - `input`: 输入数据，复数序列（频域）。
    - `output`: 变换结果，复数序列（时域）。
    - `N`: 输入长度。
    - `scale`: 是否缩放，默认为 true。
  - 返回值: 是否成功。

- `public static bool Inverse(ComplexFp32[] input, int inOffset, out ComplexFp32[] output, int outOffset, int N, bool scale = true)`
  - 执行逆向 FFT 变换，将复数输入转换为复数输出。
  - 参数:
    - `input`: 输入数据，复数序列（频域）。
    - `inOffset`: 输入数据的偏移量。
    - `output`: 变换结果，复数序列（时域）。
    - `outOffset`: 输出数据的偏移量。
    - `N`: 输入长度。
    - `scale`: 是否缩放，默认为 true。
  - 返回值: 是否成功。

- `public static bool Inverse(ReadOnlySpan<ComplexFp32> input, Span<ComplexFp32> output, bool scale = true)`
  - 执行逆向 FFT 变换，将复数输入转换为复数输出。
  - 参数:
    - `input`: 输入数据，复数序列（频域）。
    - `output`: 变换结果，复数序列（时域）。
    - `scale`: 是否缩放，默认为 true。
  - 返回值: 是否成功。

- `public static bool Inverse(ComplexFp32* data, int N, bool scale = true)`
  - 执行逆向 FFT 变换，原地版本。
  - 参数:
    - `data`: 输入和输出数据，复数序列（频域）。
    - `N`: 输入长度。
    - `scale`: 是否缩放，默认为 true。
  - 返回值: 是否成功。

- `public static bool Inverse(ComplexFp32[] data, int offset, int N, bool scale = true)`
  - 执行逆向 FFT 变换，原地版本。
  - 参数:
    - `data`: 输入和输出数据，复数序列（频域）。
    - `offset`: 数据的偏移量。
    - `N`: 输入长度。
    - `scale`: 是否缩放，默认为 true。
  - 返回值: 是否成功。

- `public static bool Inverse(Span<ComplexFp32> data, bool scale = true)`
  - 执行逆向 FFT 变换，原地版本。
  - 参数:
    - `data`: 输入和输出数据，复数序列（频域）。
    - `scale`: 是否缩放，默认为 true。
  - 返回值: 是否成功。

### 代码示例
以下是一个使用 FastFourierTransform 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

public class FastFourierTransformExample
{
    public static void Main()
    {
        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 执行前向 FFT 变换
        FastFourierTransform.Forward(inputSignal, 0, out var forwardOutput, inputSignal.Length);
        Console.WriteLine("Forward FFT Output:");
        foreach (var sample in forwardOutput)
        {
            Console.WriteLine($"Real: {sample.Real}, Imaginary: {sample.Imaginary}");
        }

        // 执行逆向 FFT 变换
        FastFourierTransform.Inverse(forwardOutput, 0, out var inverseOutput, 0, forwardOutput.Length);
        Console.WriteLine("Inverse FFT Output:");
        foreach (var sample in inverseOutput)
        {
            Console.WriteLine($"Real: {sample.Real}, Imaginary: {sample.Imaginary}");
        }
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.Fourier.RealOnlyFFT_Fp32 类

Vorcyc.Mathematics.SignalProcessing.Fourier.RealOnlyFFT_Fp32 是一个用于实现仅实数输入和输出的快速傅里叶变换（FFT）的类。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取 FFT 大小。

### 方法

#### 1. RealOnlyFFT_Fp32 构造函数
- `public RealOnlyFFT_Fp32(int size)`
  - 构造 RealOnlyFFT_Fp32 实例，指定 FFT 大小。
  - 参数:
    - `size`: FFT 大小，必须是 2 的幂。

#### 2. Forward
- `public void Forward(ReadOnlySpan<float> input, Span<ComplexFp32> output)`
  - 执行前向 FFT 变换，将实数输入转换为复数输出。
  - 参数:
    - `input`: 实数输入数据。
    - `output`: 复数输出数据。

- `public ComplexFp32[] Forward(ReadOnlySpan<float> input)`
  - 执行前向 FFT 变换，将实数输入转换为复数输出。
  - 参数:
    - `input`: 实数输入数据。
  - 返回值: 复数输出数据的数组。

#### 3. Inverse
- `public void Inverse(ReadOnlySpan<ComplexFp32> input, Span<float> output)`
  - 执行逆向 FFT 变换，将复数输入转换为实数输出。
  - 参数:
    - `input`: 复数输入数据。
    - `output`: 实数输出数据。

- `public float[] Inverse(ReadOnlySpan<ComplexFp32> input)`
  - 执行逆向 FFT 变换，将复数输入转换为实数输出。
  - 参数:
    - `input`: 复数输入数据。
  - 返回值: 实数输出数据的数组。

#### 4. InverseNorm
- `public void InverseNorm(ReadOnlySpan<ComplexFp32> input, Span<float> output)`
  - 执行归一化的逆向 FFT 变换，将复数输入转换为实数输出。
  - 参数:
    - `input`: 复数输入数据。
    - `output`: 实数输出数据。

- `public float[] InverseNorm(ReadOnlySpan<ComplexFp32> input)`
  - 执行归一化的逆向 FFT 变换，将复数输入转换为实数输出。
  - 参数:
    - `input`: 复数输入数据。
  - 返回值: 实数输出数据的数组。

#### 5. MagnitudeSpectrum
- `public void MagnitudeSpectrum(ReadOnlySpan<float> samples, Span<float> spectrum, bool normalize = false)`
  - 计算幅度谱。
  - 参数:
    - `samples`: 输入样本数组。
    - `spectrum`: 幅度谱数组。
    - `normalize`: 是否归一化，默认为 false。

- `public float[] MagnitudeSpectrum(ReadOnlySpan<float> samples, bool normalize = false)`
  - 计算幅度谱。
  - 参数:
    - `samples`: 输入样本数组。
    - `normalize`: 是否归一化，默认为 false。
  - 返回值: 幅度谱数组。

#### 6. PowerSpectrum
- `public void PowerSpectrum(ReadOnlySpan<float> samples, Span<float> spectrum, bool normalize = true)`
  - 计算功率谱。
  - 参数:
    - `samples`: 输入样本数组。
    - `spectrum`: 功率谱数组。
    - `normalize`: 是否归一化，默认为 true。

- `public float[] PowerSpectrum(ReadOnlySpan<float> samples, bool normalize = true)`
  - 计算功率谱。
  - 参数:
    - `samples`: 输入样本数组。
    - `normalize`: 是否归一化，默认为 true。
  - 返回值: 功率谱数组。

#### 7. Shift
- `public static void Shift(float[] samples)`
  - 对 FFT 结果进行移位操作。
  - 参数:
    - `samples`: 输入样本数组。
  - 异常:
    - `ArgumentException`: 如果数组长度为奇数。

### 代码示例
以下是一个使用 RealOnlyFFT_Fp32 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Fourier;

public class RealOnlyFFT_Fp32Example
{
    public static void Main()
    {
        // 定义 FFT 大小
        int fftSize = 8;

        // 创建 RealOnlyFFT_Fp32 实例
        var fft = new RealOnlyFFT_Fp32(fftSize);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f };

        // 执行前向 FFT 变换
        var forwardOutput = fft.Forward(inputSignal);
        Console.WriteLine("Forward FFT Output:");
        foreach (var sample in forwardOutput)
        {
            Console.WriteLine($"Real: {sample.Real}, Imaginary: {sample.Imaginary}");
        }

        // 执行逆向 FFT 变换
        var inverseOutput = fft.Inverse(forwardOutput);
        Console.WriteLine("Inverse FFT Output:");
        foreach (var sample in inverseOutput)
        {
            Console.WriteLine($"Value: {sample}");
        }

        // 计算幅度谱
        var magnitudeSpectrum = fft.MagnitudeSpectrum(inputSignal);
        Console.WriteLine("Magnitude Spectrum:");
        foreach (var value in magnitudeSpectrum)
        {
            Console.WriteLine($"Value: {value}");
        }

        // 计算功率谱
        var powerSpectrum = fft.PowerSpectrum(inputSignal);
        Console.WriteLine("Power Spectrum:");
        foreach (var value in powerSpectrum)
        {
            Console.WriteLine($"Value: {value}");
        }
    }
}
```