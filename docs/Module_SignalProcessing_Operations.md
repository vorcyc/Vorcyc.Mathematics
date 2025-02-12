当前位置 : [根目录](README.md)/[信号处理模块](Module_SignalProcessing.md)/[常用操作](Module_SignalProcessing_Operations.md)

# 信号处理模块 - Signal Processing Module
## 常用操作 - Operations

`Vorcyc.Mathematics.SignalProcessing.Operations` 命名空间包含多种常用的信号处理操作类，包括卷积操作类（如 `ComplexConvolver`、`Convolver`、`OlaBlockConvolver`、`OlsBlockConvolver`）、动态处理类（如 `DynamicsProcessor`）、包络跟随器类（如 `EnvelopeFollower`）、信号重建类（如 `GriffinLimReconstructor`）、谐波/打击乐分离器类（如 `HarmonicPercussiveSeparator`）、调制类（如 `Modulator`）、通用操作类（如 `Operation`）、重采样类（如 `Resampler`）、谱减法滤波类（如 `SpectralSubtractor`）和波形整形类（如 `WaveShaper`）。这些类提供了丰富的信号处理功能，适用于各种音频和信号处理需求。


> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Operations

---

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.ComplexConvolver 类](#vorcycmathematicssignalprocessingoperationsconvolutioncomplexconvolver-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.Convolver 类](#vorcycmathematicssignalprocessingoperationsconvolutionconvolver-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.OlaBlockConvolver 类](#vorcycmathematicssignalprocessingoperationsconvolutionolablockconvolver-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.OlsBlockConvolver 类](#vorcycmathematicssignalprocessingoperationsconvolutionolsblockconvolver-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.DynamicsMode 枚举](#vorcycmathematicssignalprocessingoperationsdynamicsmode-枚举)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.DynamicsProcessor 类](#vorcycmathematicssignalprocessingoperationsdynamicsprocessor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.EnvelopeFollower 类](#vorcycmathematicssignalprocessingoperationsenvelopefollower-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.GriffinLimReconstructor 类](#vorcycmathematicssignalprocessingoperationsgriffinlimreconstructor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.HarmonicPercussiveSeparator 类](#vorcycmathematicssignalprocessingoperationsharmonicpercussiveseparator-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.Modulator 类](#vorcycmathematicssignalprocessingoperationsmodulator-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.Operation 类](#vorcycmathematicssignalprocessingoperationsoperation-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.Resampler 类](#vorcycmathematicssignalprocessingoperationsresampler-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.SpectralSubtractor 类](#vorcycmathematicssignalprocessingoperationsspectralsubtractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Operations.WaveShaper 类](#vorcycmathematicssignalprocessingoperationswaveshaper-类)  

---


> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Operations.Convolution 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.ComplexConvolver 类

Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.ComplexConvolver 是一个用于实现复数值输入/输出的快速卷积（FFT）的类。

### 方法

#### 1. Convolve
- `public ComplexDiscreteSignal Convolve(ComplexDiscreteSignal signal, ComplexDiscreteSignal kernel, int fftSize = 0)`
  - 使用 FFT 对 `signal` 和 `kernel` 进行快速卷积。
  - 参数:
    - `signal`: 输入信号。
    - `kernel`: 卷积核。
    - `fftSize`: FFT 大小，默认为 0。
  - 返回值: 卷积结果的 `ComplexDiscreteSignal`。

#### 2. CrossCorrelate
- `public ComplexDiscreteSignal CrossCorrelate(ComplexDiscreteSignal signal, ComplexDiscreteSignal kernel, int fftSize = 0)`
  - 使用 FFT 对 `signal` 和 `kernel` 进行快速互相关。
  - 参数:
    - `signal`: 输入信号。
    - `kernel`: 卷积核。
    - `fftSize`: FFT 大小，默认为 0。
  - 返回值: 互相关结果的 `ComplexDiscreteSignal`。

#### 3. Deconvolve
- `public ComplexDiscreteSignal Deconvolve(ComplexDiscreteSignal signal, ComplexDiscreteSignal kernel, int fftSize = 0)`
  - 使用多项式除法和 FFT 对 `signal` 和 `kernel` 进行快速反卷积。
  - 参数:
    - `signal`: 输入信号。
    - `kernel`: 卷积核。
    - `fftSize`: FFT 大小，默认为 0。
  - 返回值: 反卷积结果的 `ComplexDiscreteSignal`。

### 代码示例
以下是一个使用 ComplexConvolver 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;

public class ComplexConvolverExample
{
    public static void Main()
    {
        // 创建 ComplexConvolver 实例
        var convolver = new ComplexConvolver();

        // 定义输入信号和卷积核
        var signal = new ComplexDiscreteSignal(44100, new float[] { 0.5f, 0.6f, 0.55f }, new float[] { 0.1f, 0.2f, 0.15f });
        var kernel = new ComplexDiscreteSignal(44100, new float[] { 0.3f, 0.4f }, new float[] { 0.05f, 0.1f });

        // 执行卷积
        var convolvedSignal = convolver.Convolve(signal, kernel);
        Console.WriteLine("Convolved Signal:");
        for (int i = 0; i < convolvedSignal.Length; i++)
        {
            Console.WriteLine($"Real: {convolvedSignal.Real[i]}, Imaginary: {convolvedSignal.Imag[i]}");
        }

        // 执行互相关
        var crossCorrelatedSignal = convolver.CrossCorrelate(signal, kernel);
        Console.WriteLine("Cross-Correlated Signal:");
        for (int i = 0; i < crossCorrelatedSignal.Length; i++)
        {
            Console.WriteLine($"Real: {crossCorrelatedSignal.Real[i]}, Imaginary: {crossCorrelatedSignal.Imag[i]}");
        }

        // 执行反卷积
        var deconvolvedSignal = convolver.Deconvolve(signal, kernel);
        Console.WriteLine("Deconvolved Signal:");
        for (int i = 0; i < deconvolvedSignal.Length; i++)
        {
            Console.WriteLine($"Real: {deconvolvedSignal.Real[i]}, Imaginary: {deconvolvedSignal.Imag[i]}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.Convolver 类

Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.Convolver 是一个用于实现快速卷积（FFT）的类。

### 方法

#### 1. Convolve
- `public DiscreteSignal Convolve(DiscreteSignal signal, DiscreteSignal kernel)`
  - 使用 FFT 对 `signal` 和 `kernel` 进行快速卷积。
  - 参数:
    - `signal`: 输入信号。
    - `kernel`: 卷积核。
  - 返回值: 卷积结果的 `DiscreteSignal`。

- `public void Convolve(float[] input, float[] kernel, float[] output)`
  - 使用 FFT 对 `input` 和 `kernel` 进行快速卷积，并将结果存储在 `output` 数组中。
  - 参数:
    - `input`: 输入信号数组。
    - `kernel`: 卷积核数组。
    - `output`: 输出结果数组。

#### 2. CrossCorrelate
- `public DiscreteSignal CrossCorrelate(DiscreteSignal signal1, DiscreteSignal signal2)`
  - 使用 FFT 对 `signal1` 和 `signal2` 进行快速互相关。
  - 参数:
    - `signal1`: 输入信号1。
    - `signal2`: 输入信号2。
  - 返回值: 互相关结果的 `DiscreteSignal`。

- `public void CrossCorrelate(float[] input1, float[] input2, float[] output)`
  - 使用 FFT 对 `input1` 和 `input2` 进行快速互相关，并将结果存储在 `output` 数组中。
  - 参数:
    - `input1`: 输入信号数组1。
    - `input2`: 输入信号数组2。
    - `output`: 输出结果数组。

### 代码示例
以下是一个使用 Convolver 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;

public class ConvolverExample
{
    public static void Main()
    {
        // 创建 Convolver 实例
        var convolver = new Convolver();

        // 定义输入信号和卷积核
        var signal = new DiscreteSignal(44100, new float[] { 0.5f, 0.6f, 0.55f });
        var kernel = new DiscreteSignal(44100, new float[] { 0.3f, 0.4f });

        // 执行卷积
        var convolvedSignal = convolver.Convolve(signal, kernel);
        Console.WriteLine("Convolved Signal:");
        for (int i = 0; i < convolvedSignal.Length; i++)
        {
            Console.WriteLine($"Sample: {convolvedSignal[i]}");
        }

        // 执行互相关
        var crossCorrelatedSignal = convolver.CrossCorrelate(signal, kernel);
        Console.WriteLine("Cross-Correlated Signal:");
        for (int i = 0; i < crossCorrelatedSignal.Length; i++)
        {
            Console.WriteLine($"Sample: {crossCorrelatedSignal[i]}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.OlaBlockConvolver 类

Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.OlaBlockConvolver 是一个用于实现基于重叠-相加算法的块卷积的信号处理器类。

### 属性

#### 1. HopSize
- `public int HopSize { get; }`
  - 获取跳跃长度：FFT 大小 - 核大小 + 1。

### 方法

#### 1. OlaBlockConvolver 构造函数
- `public OlaBlockConvolver(IEnumerable<float> kernel, int fftSize)`
  - 构造 OlaBlockConvolver 实例，指定卷积核和 FFT 大小。
  - 参数:
    - `kernel`: 卷积核。
    - `fftSize`: FFT 大小。

- `public OlaBlockConvolver(IEnumerable<double> kernel, int fftSize)`
  - 构造 OlaBlockConvolver 实例，指定卷积核和 FFT 大小。
  - 参数:
    - `kernel`: 卷积核。
    - `fftSize`: FFT 大小。

- `public static OlaBlockConvolver FromFilter(FirFilter filter, int fftSize)`
  - 使用 FIR 滤波器核和 FFT 大小构造 OlaBlockConvolver 实例。
  - 参数:
    - `filter`: FIR 滤波器。
    - `fftSize`: FFT 大小。
  - 返回值: OlaBlockConvolver 实例。

#### 2. ChangeKernel
- `public void ChangeKernel(float[] kernel)`
  - 在线更改卷积核系数。
  - 参数:
    - `kernel`: 新的卷积核。

#### 3. Process
- `public float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 处理整个信号并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 滤波方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 5. Reset
- `public void Reset()`
  - 重置重叠-相加卷积器。

### 代码示例
以下是一个使用 OlaBlockConvolver 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;

public class OlaBlockConvolverExample
{
    public static void Main()
    {
        // 定义卷积核和 FFT 大小
        float[] kernel = { 0.3f, 0.4f, 0.5f }; int fftSize = 8;

        // 创建 OlaBlockConvolver 实例
        var convolver = new OlaBlockConvolver(kernel, fftSize);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f };

        // 处理整个信号
        var outputSignal = convolver.ApplyTo(new DiscreteSignal(44100, inputSignal));
        Console.WriteLine("Filtered Signal:");
        foreach (var sample in outputSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 在线更改卷积核
        float[] newKernel = { 0.2f, 0.3f, 0.4f };
        convolver.ChangeKernel(newKernel);

        // 处理单个样本
        float processedSample = convolver.Process(0.85f);
        Console.WriteLine($"Processed Sample: {processedSample}");

        // 重置卷积器
        convolver.Reset();
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.OlsBlockConvolver 类

Vorcyc.Mathematics.SignalProcessing.Operations.Convolution.OlsBlockConvolver 是一个用于实现基于重叠-保存算法的块卷积的信号处理器类。

### 属性

#### 1. HopSize
- `public int HopSize { get; }`
  - 获取跳跃长度：FFT 大小 - 核大小 + 1。

### 方法

#### 1. OlsBlockConvolver 构造函数
- `public OlsBlockConvolver(IEnumerable<float> kernel, int fftSize)`
  - 构造 OlsBlockConvolver 实例，指定卷积核和 FFT 大小。
  - 参数:
    - `kernel`: 卷积核。
    - `fftSize`: FFT 大小。

- `public OlsBlockConvolver(IEnumerable<double> kernel, int fftSize)`
  - 构造 OlsBlockConvolver 实例，指定卷积核和 FFT 大小。
  - 参数:
    - `kernel`: 卷积核。
    - `fftSize`: FFT 大小。

- `public static OlsBlockConvolver FromFilter(FirFilter filter, int fftSize)`
  - 使用 FIR 滤波器核和 FFT 大小构造 OlsBlockConvolver 实例。
  - 参数:
    - `filter`: FIR 滤波器。
    - `fftSize`: FFT 大小。
  - 返回值: OlsBlockConvolver 实例。

#### 2. ChangeKernel
- `public void ChangeKernel(float[] kernel)`
  - 在线更改卷积核系数。
  - 参数:
    - `kernel`: 新的卷积核。

#### 3. Process
- `public float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 处理整个信号并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 滤波方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 5. Reset
- `public void Reset()`
  - 重置重叠-保存卷积器。

### 代码示例
以下是一个使用 OlsBlockConvolver 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;

public class OlsBlockConvolverExample
{
    public static void Main()
    {
        // 定义卷积核和 FFT 大小
        float[] kernel = { 0.3f, 0.4f, 0.5f }; int fftSize = 8;

        // 创建 OlsBlockConvolver 实例
        var convolver = new OlsBlockConvolver(kernel, fftSize);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f };

        // 处理整个信号
        var outputSignal = convolver.ApplyTo(new DiscreteSignal(44100, inputSignal));
        Console.WriteLine("Filtered Signal:");
        foreach (var sample in outputSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 在线更改卷积核
        float[] newKernel = { 0.2f, 0.3f, 0.4f };
        convolver.ChangeKernel(newKernel);

        // 处理单个样本
        float processedSample = convolver.Process(0.85f);
        Console.WriteLine($"Processed Sample: {processedSample}");

        // 重置卷积器
        convolver.Reset();
    }
}
```

---

> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Operations


## Vorcyc.Mathematics.SignalProcessing.Operations.DynamicsMode 枚举

Vorcyc.Mathematics.SignalProcessing.Operations.DynamicsMode 枚举定义了动态处理器的类型（模式）。

### 枚举成员

#### 1. Compressor
- `Compressor`
  - 较小的压缩比，例如 1:1, 2:1。

#### 2. Limiter
- `Limiter`
  - 较大的压缩比，例如 5:1, 10:1。

#### 3. Expander
- `Expander`
  - 较小的扩展比，例如 1:1, 2:1。

#### 4. NoiseGate
- `NoiseGate`
  - 非常高的压缩比，例如 5:1。




  ## Vorcyc.Mathematics.SignalProcessing.Operations.DynamicsProcessor 类

Vorcyc.Mathematics.SignalProcessing.Operations.DynamicsProcessor 是一个用于实现动态处理（如限制器、压缩器、扩展器或噪声门）的类。

### 属性

#### 1. Threshold
- `public float Threshold { get; set; }`
  - 获取或设置压缩/扩展阈值。

#### 2. Ratio
- `public float Ratio { get; set; }`
  - 获取或设置压缩/扩展比率。

#### 3. MakeupGain
- `public float MakeupGain { get; set; }`
  - 获取或设置补偿增益。

#### 4. Attack
- `public float Attack { get; set; }`
  - 获取或设置攻击时间。

#### 5. Release
- `public float Release { get; set; }`
  - 获取或设置释放时间。

### 方法

#### 1. DynamicsProcessor 构造函数
- `public DynamicsProcessor(DynamicsMode mode, int samplingRate, float threshold, float ratio, float makeupGain = 0, float attack = 0.01f, float release = 0.1f, float minAmplitudeDb = -120)`
  - 构造 DynamicsProcessor 实例，指定动态处理器模式、采样率、阈值、比率、补偿增益、攻击时间、释放时间和最小振幅阈值。
  - 参数:
    - `mode`: 动态处理器模式。
    - `samplingRate`: 采样率。
    - `threshold`: 压缩/扩展阈值。
    - `ratio`: 压缩/扩展比率。
    - `makeupGain`: 补偿增益，默认为 0。
    - `attack`: 攻击时间，默认为 0.01 秒。
    - `release`: 释放时间，默认为 0.1 秒。
    - `minAmplitudeDb`: 最小振幅阈值，默认为 -120 dB。

#### 2. Process
- `public float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public void Reset()`
  - 重置动态处理器。

#### 4. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 处理整个信号并返回新的信号（动态处理）。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 滤波方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

### 代码示例
以下是一个使用 DynamicsProcessor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations;

public class DynamicsProcessorExample
{
    public static void Main()
    {
        // 定义动态处理器模式
        DynamicsMode mode = DynamicsMode.Compressor;

        // 创建 DynamicsProcessor 实例
        var processor = new DynamicsProcessor(mode, 44100, -20, 2, 0, 0.01f, 0.1f);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f };

        // 处理整个信号
        var outputSignal = processor.ApplyTo(new DiscreteSignal(44100, inputSignal));
        Console.WriteLine("Processed Signal:");
        foreach (var sample in outputSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 处理单个样本
        float processedSample = processor.Process(0.85f);
        Console.WriteLine($"Processed Sample: {processedSample}");

        // 重置动态处理器
        processor.Reset();
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.Operations.EnvelopeFollower 类

Vorcyc.Mathematics.SignalProcessing.Operations.EnvelopeFollower 是一个用于实现包络跟随（包络检测）的类。

### 属性

#### 1. AttackTime
- `public float AttackTime { get; set; }`
  - 获取或设置攻击时间（以秒为单位）。

#### 2. ReleaseTime
- `public float ReleaseTime { get; set; }`
  - 获取或设置释放时间（以秒为单位）。

### 方法

#### 1. EnvelopeFollower 构造函数
- `public EnvelopeFollower(int samplingRate, float attackTime = 0.01f, float releaseTime = 0.05f)`
  - 构造 EnvelopeFollower 实例，指定采样率、攻击时间和释放时间。
  - 参数:
    - `samplingRate`: 采样率。
    - `attackTime`: 攻击时间，默认为 0.01 秒。
    - `releaseTime`: 释放时间，默认为 0.05 秒。

#### 2. Process
- `public float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public void Reset()`
  - 重置包络跟随器。

#### 4. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 处理整个信号并返回新的信号（包络）。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 滤波方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

### 代码示例
以下是一个使用 EnvelopeFollower 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations;

public class EnvelopeFollowerExample
{
    public static void Main()
    {
        // 创建 EnvelopeFollower 实例
        var envelopeFollower = new EnvelopeFollower(44100, 0.01f, 0.05f);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f };

        // 处理整个信号
        var outputSignal = envelopeFollower.ApplyTo(new DiscreteSignal(44100, inputSignal));
        Console.WriteLine("Envelope Signal:");
        foreach (var sample in outputSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 处理单个样本
        float processedSample = envelopeFollower.Process(0.85f);
        Console.WriteLine($"Processed Sample: {processedSample}");

        // 重置包络跟随器
        envelopeFollower.Reset();
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Operations.GriffinLimReconstructor 类

Vorcyc.Mathematics.SignalProcessing.Operations.GriffinLimReconstructor 是一个用于从功率（或幅度）谱图中使用 Griffin-Lim 迭代算法重建信号的类。

### 属性

#### 1. Gain
- `public float Gain { get; set; }`
  - 获取或设置幅度增益因子。

### 方法

#### 1. GriffinLimReconstructor 构造函数
- `public GriffinLimReconstructor(List<float[]> spectrogram, int windowSize = 1024, int hopSize = 256, WindowType window = WindowType.Hann, int power = 2)`
  - 构造 GriffinLimReconstructor 实例，指定谱图、窗口大小、跳跃大小、窗口类型和功率。
  - 参数:
    - `spectrogram`: 谱图（谱列表）。
    - `windowSize`: 窗口大小，默认为 1024。
    - `hopSize`: 跳跃大小，默认为 256。
    - `window`: 窗口类型，默认为 `WindowType.Hann`。
    - `power`: 功率（2 - 功率谱，否则 - 幅度谱），默认为 2。

- `public GriffinLimReconstructor(List<float[]> spectrogram, Stft stft, int power = 2)`
  - 构造 GriffinLimReconstructor 实例，指定谱图、STFT 变换器和功率。
  - 参数:
    - `spectrogram`: 谱图（谱列表）。
    - `stft`: STFT 变换器。
    - `power`: 功率（2 - 功率谱，否则 - 幅度谱），默认为 2。

#### 2. Iterate
- `public float[] Iterate(float[] signal = null)`
  - 执行一次重建迭代并返回当前步骤的重建信号。
  - 参数:
    - `signal`: 上一次迭代重建的信号，默认为 null。
  - 返回值: 当前步骤的重建信号。

#### 3. Reconstruct
- `public float[] Reconstruct(int iterations = 20)`
  - 迭代地从谱图中重建信号。
  - 参数:
    - `iterations`: Griffin-Lim 算法的迭代次数，默认为 20。
  - 返回值: 重建的信号。

### 代码示例
以下是一个使用 GriffinLimReconstructor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.SignalProcessing.Operations;

public class GriffinLimReconstructorExample
{
    public static void Main()
    {
        // 定义谱图
        var spectrogram = new List<float[]> { new float[] { 0.5f, 0.6f, 0.55f }, new float[] { 0.7f, 0.65f, 0.8f }, new float[] { 0.75f, 0.9f, 0.85f } };

        // 创建 GriffinLimReconstructor 实例
        var reconstructor = new GriffinLimReconstructor(spectrogram);

        // 执行重建
        var reconstructedSignal = reconstructor.Reconstruct();
        Console.WriteLine("Reconstructed Signal:");
        foreach (var sample in reconstructedSignal)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行一次迭代
        var iteratedSignal = reconstructor.Iterate(reconstructedSignal);
        Console.WriteLine("Iterated Signal:");
        foreach (var sample in iteratedSignal)
        {
            Console.WriteLine($"Sample: {sample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Operations.HarmonicPercussiveSeparator 类

Vorcyc.Mathematics.SignalProcessing.Operations.HarmonicPercussiveSeparator 是一个基于中值滤波的谐波/打击乐分离器类。

### 方法

#### 1. HarmonicPercussiveSeparator 构造函数
- `public HarmonicPercussiveSeparator(int fftSize = 2048, int hopSize = 512, int harmonicWinSize = 17, int percussiveWinSize = 17, HpsMasking masking = HpsMasking.WienerOrder2)`
  - 构造 HarmonicPercussiveSeparator 实例，指定 FFT 大小、跳跃长度、谐波中值滤波器大小、打击乐中值滤波器大小和掩蔽模式。
  - 参数:
    - `fftSize`: FFT 大小，默认为 2048。
    - `hopSize`: 跳跃长度（样本数），默认为 512。
    - `harmonicWinSize`: 沿时间轴的中值滤波器大小，默认为 17。
    - `percussiveWinSize`: 沿频率轴的中值滤波器大小，默认为 17。
    - `masking`: 掩蔽模式，默认为 `HpsMasking.WienerOrder2`。

#### 2. EvaluateSpectrograms
- `public (MagnitudePhaseList, MagnitudePhaseList) EvaluateSpectrograms(DiscreteSignal signal)`
  - 从给定的信号中评估谐波和打击乐的幅度-相位谱图。两个谱图对象共享相同的相位数组。
  - 参数:
    - `signal`: 输入信号。
  - 返回值: 包含谐波和打击乐谱图的元组。

#### 3. EvaluateSignals
- `public (DiscreteSignal, DiscreteSignal) EvaluateSignals(DiscreteSignal signal)`
  - 从给定的信号中提取谐波和打击乐信号。
  - 参数:
    - `signal`: 输入信号。
  - 返回值: 包含谐波和打击乐信号的元组。

### 代码示例
以下是一个使用 HarmonicPercussiveSeparator 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations;

public class HarmonicPercussiveSeparatorExample
{
    public static void Main()
    {
        // 创建 HarmonicPercussiveSeparator 实例
        var separator = new HarmonicPercussiveSeparator();

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f };
        var signal = new DiscreteSignal(44100, inputSignal);

        // 评估谱图
        var (harmonicSpectrogram, percussiveSpectrogram) = separator.EvaluateSpectrograms(signal);
        Console.WriteLine("Harmonic and Percussive Spectrograms evaluated.");

        // 提取谐波和打击乐信号
        var (harmonicSignal, percussiveSignal) = separator.EvaluateSignals(signal);
        Console.WriteLine("Harmonic and Percussive Signals extracted.");

        // 输出谐波信号
        Console.WriteLine("Harmonic Signal:");
        foreach (var sample in harmonicSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 输出打击乐信号
        Console.WriteLine("Percussive Signal:");
        foreach (var sample in percussiveSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Operations.Modulator 类

Vorcyc.Mathematics.SignalProcessing.Operations.Modulator 提供了多种调制方法：

### 方法

#### 1. Ring
- `public static DiscreteSignal Ring(DiscreteSignal carrier, DiscreteSignal modulator)`
  - 执行环形调制（RM）并返回 RM 信号。
  - 参数:
    - `carrier`: 载波信号。
    - `modulator`: 调制信号。
  - 返回值: 调制后的 `DiscreteSignal`。

#### 2. Amplitude
- `public static DiscreteSignal Amplitude(DiscreteSignal carrier, float modulatorFrequency = 20, float modulationIndex = 0.5f)`
  - 执行幅度调制（AM）并返回 AM 信号。
  - 参数:
    - `carrier`: 载波信号。
    - `modulatorFrequency`: 调制频率，默认为 20 Hz。
    - `modulationIndex`: 调制指数（深度），默认为 0.5。
  - 返回值: 调制后的 `DiscreteSignal`。

#### 3. Frequency
- `public static DiscreteSignal Frequency(DiscreteSignal baseband, float carrierAmplitude, float carrierFrequency, float deviation = 0.1f)`
  - 执行频率调制（FM）并返回 FM 信号。
  - 参数:
    - `baseband`: 基带信号。
    - `carrierAmplitude`: 载波幅度。
    - `carrierFrequency`: 载波频率。
    - `deviation`: 频率偏移，默认为 0.1 Hz。
  - 返回值: 调制后的 `DiscreteSignal`。

#### 4. FrequencySinusoidal
- `public static DiscreteSignal FrequencySinusoidal(float carrierFrequency, float carrierAmplitude, float modulatorFrequency, float modulationIndex, int length, int samplingRate = 1)`
  - 执行正弦频率调制（FM）并返回正弦 FM 信号。
  - 参数:
    - `carrierFrequency`: 载波频率。
    - `carrierAmplitude`: 载波幅度。
    - `modulatorFrequency`: 调制频率。
    - `modulationIndex`: 调制指数（深度）。
    - `length`: FM 信号长度。
    - `samplingRate`: 采样率，默认为 1。
  - 返回值: 调制后的 `DiscreteSignal`。

#### 5. FrequencyLinear
- `public static DiscreteSignal FrequencyLinear(float carrierFrequency, float carrierAmplitude, float modulationIndex, int length, int samplingRate = 1)`
  - 执行线性频率调制（FM）并返回 FM 信号。
  - 参数:
    - `carrierFrequency`: 载波频率。
    - `carrierAmplitude`: 载波幅度。
    - `modulationIndex`: 调制指数（深度）。
    - `length`: FM 信号长度。
    - `samplingRate`: 采样率，默认为 1。
  - 返回值: 调制后的 `DiscreteSignal`。

#### 6. Phase
- `public static DiscreteSignal Phase(DiscreteSignal baseband, float carrierAmplitude, float carrierFrequency, float deviation = 0.8f)`
  - 执行相位调制（PM）并返回 PM 信号。
  - 参数:
    - `baseband`: 基带信号。
    - `carrierAmplitude`: 载波幅度。
    - `carrierFrequency`: 载波频率。
    - `deviation`: 频率偏移，默认为 0.8。
  - 返回值: 调制后的 `DiscreteSignal`。

#### 7. DemodulateAmplitude
- `public static DiscreteSignal DemodulateAmplitude(DiscreteSignal signal)`
  - 基于 Hilbert 变换对信号进行简单的幅度解调。
  - 参数:
    - `signal`: 输入信号。
  - 返回值: 解调后的 `DiscreteSignal`。

#### 8. DemodulateFrequency
- `public static DiscreteSignal DemodulateFrequency(DiscreteSignal signal)`
  - 基于 Hilbert 变换对信号进行简单的频率解调。
  - 参数:
    - `signal`: 输入信号。
  - 返回值: 解调后的 `DiscreteSignal`。

### 代码示例
以下是一个使用 Modulator 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using System.Threading;
using Vorcyc.Mathematics.SignalProcessing.Operations;

public class ModulatorExample
{
    public static void Main()
    {
        // 定义载波信号和调制信号
        var carrier = new DiscreteSignal(44100, new float[] { 0.5f, 0.6f, 0.55f });
        var modulator = new DiscreteSignal(44100, new float[] { 0.3f, 0.4f, 0.35f });

        // 执行环形调制
        var ringModulatedSignal = Modulator.Ring(carrier, modulator);
        Console.WriteLine("Ring Modulated Signal:");
        foreach (var sample in ringModulatedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行幅度调制
        var amplitudeModulatedSignal = Modulator.Amplitude(carrier);
        Console.WriteLine("Amplitude Modulated Signal:");
        foreach (var sample in amplitudeModulatedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行频率调制
        var frequencyModulatedSignal = Modulator.Frequency(carrier, 1.0f, 1000.0f);
        Console.WriteLine("Frequency Modulated Signal:");
        foreach (var sample in frequencyModulatedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行相位调制
        var phaseModulatedSignal = Modulator.Phase(carrier, 1.0f, 1000.0f);
        Console.WriteLine("Phase Modulated Signal:");
        foreach (var sample in phaseModulatedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行幅度解调
        var demodulatedAmplitudeSignal = Modulator.DemodulateAmplitude(amplitudeModulatedSignal);
        Console.WriteLine("Demodulated Amplitude Signal:");
        foreach (var sample in demodulatedAmplitudeSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行频率解调
        var demodulatedFrequencySignal = Modulator.DemodulateFrequency(frequencyModulatedSignal);
        Console.WriteLine("Demodulated Frequency Signal:");
        foreach (var sample in demodulatedFrequencySignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Operations.Operation 类

Vorcyc.Mathematics.SignalProcessing.Operations.Operation 提供了多种 DSP/音频操作方法：

### 方法

#### 1. Convolve
- `public static DiscreteSignal Convolve(DiscreteSignal signal, DiscreteSignal kernel)`
  - 使用 FFT 对 `signal` 和 `kernel` 进行快速卷积。
  - 参数:
    - `signal`: 输入信号。
    - `kernel`: 卷积核。
  - 返回值: 卷积结果的 `DiscreteSignal`。

- `public static ComplexDiscreteSignal Convolve(ComplexDiscreteSignal signal, ComplexDiscreteSignal kernel)`
  - 使用 FFT 对 `signal` 和 `kernel` 进行快速卷积。
  - 参数:
    - `signal`: 输入信号。
    - `kernel`: 卷积核。
  - 返回值: 卷积结果的 `ComplexDiscreteSignal`。

- `public static float[] Convolve(float[] signal, float[] kernel)`
  - 使用 FFT 对 `signal` 和 `kernel` 进行快速卷积。
  - 参数:
    - `signal`: 输入信号数组。
    - `kernel`: 卷积核数组。
  - 返回值: 卷积结果的数组。

#### 2. CrossCorrelate
- `public static DiscreteSignal CrossCorrelate(DiscreteSignal signal1, DiscreteSignal signal2)`
  - 使用 FFT 对 `signal1` 和 `signal2` 进行快速互相关。
  - 参数:
    - `signal1`: 输入信号1。
    - `signal2`: 输入信号2。
  - 返回值: 互相关结果的 `DiscreteSignal`。

- `public static ComplexDiscreteSignal CrossCorrelate(ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)`
  - 使用 FFT 对 `signal1` 和 `signal2` 进行快速互相关。
  - 参数:
    - `signal1`: 输入信号1。
    - `signal2`: 输入信号2。
  - 返回值: 互相关结果的 `ComplexDiscreteSignal`。

#### 3. BlockConvolve
- `public static DiscreteSignal BlockConvolve(DiscreteSignal signal, DiscreteSignal kernel, int fftSize, FilteringMethod method = FilteringMethod.OverlapSave)`
  - 对 `signal` 和 `kernel` 进行块卷积（使用重叠-相加或重叠-保存算法）。
  - 参数:
    - `signal`: 输入信号。
    - `kernel`: 卷积核。
    - `fftSize`: FFT 大小。
    - `method`: 块卷积方法（OverlapAdd / OverlapSave），默认为 `FilteringMethod.OverlapSave`。
  - 返回值: 块卷积结果的 `DiscreteSignal`。

#### 4. Deconvolve
- `public static ComplexDiscreteSignal Deconvolve(ComplexDiscreteSignal signal, ComplexDiscreteSignal kernel)`
  - 对 `signal` 和 `kernel` 进行反卷积。
  - 参数:
    - `signal`: 输入信号。
    - `kernel`: 卷积核。
  - 返回值: 反卷积结果的 `ComplexDiscreteSignal`。

#### 5. Interpolate
- `public static DiscreteSignal Interpolate(DiscreteSignal signal, int factor, FirFilter? filter = null)`
  - 对 `signal` 进行插值并进行低通滤波。
  - 参数:
    - `signal`: 输入信号。
    - `factor`: 插值因子（例如 factor=2 表示从 8000 Hz -> 16000 Hz）。
    - `filter`: 低通抗混叠滤波器。
  - 返回值: 插值结果的 `DiscreteSignal`。

#### 6. Decimate
- `public static DiscreteSignal Decimate(DiscreteSignal signal, int factor, FirFilter? filter = null)`
  - 对 `signal` 进行抽取并进行低通滤波。
  - 参数:
    - `signal`: 输入信号。
    - `factor`: 抽取因子（例如 factor=2 表示从 16000 Hz -> 8000 Hz）。
    - `filter`: 低通抗混叠滤波器。
  - 返回值: 抽取结果的 `DiscreteSignal`。

#### 7. Resample
- `public static DiscreteSignal Resample(DiscreteSignal signal, int newSamplingRate, FirFilter? filter = null, int order = 15)`
  - 对 `signal` 进行带限重采样。
  - 参数:
    - `signal`: 输入信号。
    - `newSamplingRate`: 目标采样率。
    - `filter`: 低通抗混叠滤波器。
    - `order`: 滤波器阶数，默认为 15。
  - 返回值: 重采样结果的 `DiscreteSignal`。

- `public static DiscreteSignal ResampleUpDown(DiscreteSignal signal, int up, int down, FirFilter? filter = null)`
  - 对 `signal` 进行简单重采样（插值和抽取的组合）。
  - 参数:
    - `signal`: 输入信号。
    - `up`: 插值因子。
    - `down`: 抽取因子。
    - `filter`: 低通抗混叠滤波器。
  - 返回值: 重采样结果的 `DiscreteSignal`。

#### 8. TimeStretch
- `public static DiscreteSignal TimeStretch(DiscreteSignal signal, float stretch, int windowSize, int hopSize, TsmAlgorithm algorithm = TsmAlgorithm.PhaseVocoderPhaseLocking)`
  - 对 `signal` 进行时间拉伸，参数由用户设置。
  - 参数:
    - `signal`: 输入信号。
    - `stretch`: 拉伸因子（比率）。
    - `windowSize`: 窗口大小（对于声码器 - FFT 大小）。
    - `hopSize`: 跳跃长度。
    - `algorithm`: 时间拉伸算法，默认为 `TsmAlgorithm.PhaseVocoderPhaseLocking`。
  - 返回值: 时间拉伸结果的 `DiscreteSignal`。

- `public static DiscreteSignal TimeStretch(DiscreteSignal signal, double stretch, TsmAlgorithm algorithm = TsmAlgorithm.PhaseVocoderPhaseLocking)`
  - 对 `signal` 进行时间拉伸，参数自动推导。
  - 参数:
    - `signal`: 输入信号。
    - `stretch`: 拉伸因子（比率）。
    - `algorithm`: 时间拉伸算法，默认为 `TsmAlgorithm.PhaseVocoderPhaseLocking`。
  - 返回值: 时间拉伸结果的 `DiscreteSignal`。

#### 9. Envelope
- `public static DiscreteSignal Envelope(DiscreteSignal signal, float attackTime = 0.01f, float releaseTime = 0.05f)`
  - 提取 `signal` 的包络。
  - 参数:
    - `signal`: 输入信号。
    - `attackTime`: 攻击时间（以秒为单位），默认为 0.01 秒。
    - `releaseTime`: 释放时间（以秒为单位），默认为 0.05 秒。
  - 返回值: 包络结果的 `DiscreteSignal`。

#### 10. FullRectify
- `public static DiscreteSignal FullRectify(DiscreteSignal signal)`
  - 对 `signal` 进行全波整流。
  - 参数:
    - `signal`: 输入信号。
  - 返回值: 全波整流结果的 `DiscreteSignal`。

#### 11. HalfRectify
- `public static DiscreteSignal HalfRectify(DiscreteSignal signal)`
  - 对 `signal` 进行半波整流。
  - 参数:
    - `signal`: 输入信号。
  - 返回值: 半波整流结果的 `DiscreteSignal`。

#### 12. SpectralSubtract
- `public static DiscreteSignal SpectralSubtract(DiscreteSignal signal, DiscreteSignal noise, int fftSize = 1024, int hopSize = 256)`
  - 使用谱减法对 `signal` 进行去噪。将 `noise` 从 `signal` 中减去。
  - 参数:
    - `signal`: 输入信号。
    - `noise`: 噪声信号。
    - `fftSize`: FFT 大小，默认为 1024。
    - `hopSize`: 跳跃大小（样本数），默认为 256。
  - 返回值: 去噪结果的 `DiscreteSignal`。

#### 13. NormalizePeak
- `public static void NormalizePeak(float[] samples, float peakDb)`
  - 归一化峰值电平。
  - 参数:
    - `samples`: 样本数组。
    - `peakDb`: 峰值电平（以分贝为单位），例如 -1dB, -3dB 等。

- `public static DiscreteSignal NormalizePeak(DiscreteSignal signal, float peakDb)`
  - 归一化峰值电平。
  - 参数:
    - `signal`: 输入信号。
    - `peakDb`: 峰值电平（以分贝为单位），例如 -1dB, -3dB 等。
  - 返回值: 归一化结果的 `DiscreteSignal`。

#### 14. ChangePeak
- `public static void ChangePeak(float[] samples, float peakDb)`
  - 相对于输入 `samples` 改变峰值电平（就地）。
  - 参数:
    - `samples`: 样本数组。
    - `peakDb`: 峰值变化（以分贝为单位），例如 -6dB 表示峰值电平减半。

- `public static DiscreteSignal ChangePeak(DiscreteSignal signal, float peakDb)`
  - 相对于输入 `signal` 改变峰值电平。
  - 参数:
    - `signal`: 输入信号。
    - `peakDb`: 峰值变化（以分贝为单位），例如 -6dB 表示峰值电平减半。
  - 返回值: 改变峰值后的 `DiscreteSignal`。

#### 15. NormalizeRms
- `public static void NormalizeRms(float[] samples, float rmsDb)`
  - 归一化 RMS。
  - 参数:
    - `samples`: 样本数组。
    - `rmsDb`: RMS 电平（以分贝为单位），例如 -6dB, -18dB, -26dB 等。

- `public static DiscreteSignal NormalizeRms(DiscreteSignal signal, float rmsDb)`
  - 归一化 RMS。
  - 参数:
    - `signal`: 输入信号。
    - `rmsDb`: RMS 电平（以分贝为单位），例如 -6dB, -18dB, -26dB 等。
  - 返回值: 归一化结果的 `DiscreteSignal`。

#### 16. ChangeRms
- `public static void ChangeRms(float[] samples, float rmsDb)`
  - 相对于输入 `samples` 改变 RMS。
  - 参数:
    - `samples`: 样本数组。
    - `rmsDb`: RMS 变化（以分贝为单位），例如 -6dB 表示 RMS 减半。

- `public static DiscreteSignal ChangeRms(DiscreteSignal signal, float rmsDb)`
  - 相对于输入 `signal` 改变 RMS。
  - 参数:
    - `signal`: 输入信号。
    - `rmsDb`: RMS 变化（以分贝为单位），例如 -6dB 表示 RMS 减半。
  - 返回值: 改变 RMS 后的 `DiscreteSignal`。

#### 17. Welch
- `public static float[] Welch(DiscreteSignal signal, int windowSize = 1024, int hopSize = 256, WindowType window = WindowType.Hann, int fftSize = 0, int samplingRate = 0)`
  - 使用 Welch 方法计算周期图。如果 `samplingRate`=0，则评估功率谱，否则评估功率谱密度。
  - 参数:
    - `signal`: 输入信号。
    - `windowSize`: 窗口大小（样本数），默认为 1024。
    - `hopSize`: 跳跃大小（样本数），默认为 256。
    - `window`: 窗口函数，默认为 `WindowType.Hann`。
    - `fftSize`: FFT 大小，默认为 0。
    - `samplingRate`: 如果采样率=0，则评估功率谱，否则评估功率谱密度。
  - 返回值: 周期图数组。

#### 18. LombScargle
- `public static float[] LombScargle(float[] x, float[] y, float[] freqs, bool subtractMean = false, bool normalize = false)`
  - 计算 Lomb-Scargle 周期图。
  - 参数:
    - `x`: 样本时间。
    - `y`: 样本时间对应的信号值。
    - `freqs`: 输出周期图的角频率。
    - `subtractMean`: 在评估周期图之前从值中减去均值。
    - `normalize`: 通过数据围绕常数参考模型（零）的残差来归一化周期图。
  - 返回值: 周期图数组。

### 代码示例
以下是一个使用 Operation 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations;

public class OperationExample
{
    public static void Main()
    {
        // 定义输入信号和卷积核
        var signal = new DiscreteSignal(44100, new float[] { 0.5f, 0.6f, 0.55f });
        var kernel = new DiscreteSignal(44100, new float[] { 0.3f, 0.4f });

        // 执行卷积
        var convolvedSignal = Operation.Convolve(signal, kernel);
        Console.WriteLine("Convolved Signal:");
        foreach (var sample in convolvedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行互相关
        var crossCorrelatedSignal = Operation.CrossCorrelate(signal, kernel);
        Console.WriteLine("Cross-Correlated Signal:");
        foreach (var sample in crossCorrelatedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行时间拉伸
        var timeStretchedSignal = Operation.TimeStretch(signal, 1.5f, 1024, 256);
        Console.WriteLine("Time-Stretched Signal:");
        foreach (var sample in timeStretchedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 提取包络
        var envelopeSignal = Operation.Envelope(signal);
        Console.WriteLine("Envelope Signal:");
        foreach (var sample in envelopeSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行全波整流
        var fullRectifiedSignal = Operation.FullRectify(signal);
        Console.WriteLine("Full-Rectified Signal:");
        foreach (var sample in fullRectifiedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行谱减法去噪
        var noise = new DiscreteSignal(44100, new float[] { 0.1f, 0.1f, 0.1f });
        var denoisedSignal = Operation.SpectralSubtract(signal, noise);
        Console.WriteLine("Denoised Signal:");
        foreach (var sample in denoisedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 归一化峰值电平
        var normalizedPeakSignal = Operation.NormalizePeak(signal, -3);
        Console.WriteLine("Normalized Peak Signal:");
        foreach (var sample in normalizedPeakSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 归一化 RMS
        var normalizedRmsSignal = Operation.NormalizeRms(signal, -18);
        Console.WriteLine("Normalized RMS Signal:");
        foreach (var sample in normalizedRmsSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 计算 Welch 周期图
        var welchPeriodogram = Operation.Welch(signal);
        Console.WriteLine("Welch Periodogram:");
        foreach (var value in welchPeriodogram)
        {
            Console.WriteLine($"Value: {value}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Operations.Resampler 类

Vorcyc.Mathematics.SignalProcessing.Operations.Resampler 是一个用于信号重采样（采样率转换）的类。

### 属性

#### 1. MinResamplingFilterOrder
- `public int MinResamplingFilterOrder { get; set; }`
  - 获取或设置低通抗混叠 FIR 滤波器的阶数，如果未显式指定滤波器，则会自动创建。默认值为 101。

### 方法

#### 1. Interpolate
- `public DiscreteSignal Interpolate(DiscreteSignal signal, int factor, FirFilter? filter = null)`
  - 对 `signal` 进行插值并进行低通滤波。
  - 参数:
    - `signal`: 输入信号。
    - `factor`: 插值因子（例如 factor=2 表示从 8000 Hz -> 16000 Hz）。
    - `filter`: 低通抗混叠滤波器。
  - 返回值: 插值结果的 `DiscreteSignal`。

#### 2. Decimate
- `public DiscreteSignal Decimate(DiscreteSignal signal, int factor, FirFilter? filter = null)`
  - 对 `signal` 进行抽取并进行低通滤波。
  - 参数:
    - `signal`: 输入信号。
    - `factor`: 抽取因子（例如 factor=2 表示从 16000 Hz -> 8000 Hz）。
    - `filter`: 低通抗混叠滤波器。
  - 返回值: 抽取结果的 `DiscreteSignal`。

#### 3. Resample
- `public DiscreteSignal Resample(DiscreteSignal signal, int newSamplingRate, FirFilter? filter = null, int order = 15)`
  - 对 `signal` 进行带限重采样。
  - 参数:
    - `signal`: 输入信号。
    - `newSamplingRate`: 目标采样率。
    - `filter`: 低通抗混叠滤波器。
    - `order`: 滤波器阶数，默认为 15。
  - 返回值: 重采样结果的 `DiscreteSignal`。

#### 4. ResampleUpDown
- `public DiscreteSignal ResampleUpDown(DiscreteSignal signal, int up, int down, FirFilter? filter = null)`
  - 对 `signal` 进行简单重采样（插值和抽取的组合）。
  - 参数:
    - `signal`: 输入信号。
    - `up`: 插值因子。
    - `down`: 抽取因子。
    - `filter`: 低通抗混叠滤波器。
  - 返回值: 重采样结果的 `DiscreteSignal`。

### 代码示例
以下是一个使用 Resampler 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations;

public class ResamplerExample
{
    public static void Main()
    {
        // 定义输入信号
        var signal = new DiscreteSignal(44100, new float[] { 0.5f, 0.6f, 0.55f });

        // 创建 Resampler 实例
        var resampler = new Resampler();

        // 执行插值
        var interpolatedSignal = resampler.Interpolate(signal, 2);
        Console.WriteLine("Interpolated Signal:");
        foreach (var sample in interpolatedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行抽取
        var decimatedSignal = resampler.Decimate(signal, 2);
        Console.WriteLine("Decimated Signal:");
        foreach (var sample in decimatedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行带限重采样
        var resampledSignal = resampler.Resample(signal, 22050);
        Console.WriteLine("Resampled Signal:");
        foreach (var sample in resampledSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 执行简单重采样（插值和抽取的组合）
        var resampledUpDownSignal = resampler.ResampleUpDown(signal, 2, 3);
        Console.WriteLine("Resampled UpDown Signal:");
        foreach (var sample in resampledUpDownSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Operations.SpectralSubtractor 类

Vorcyc.Mathematics.SignalProcessing.Operations.SpectralSubtractor 是一个用于实现谱减法滤波的类。

### 属性

#### 1. Beta
- `public float Beta { get; set; }`
  - 获取或设置谱底（beta 系数）。默认值为 0.009f。

#### 2. AlphaMin
- `public float AlphaMin { get; set; }`
  - 获取或设置减法因子的最小阈值（alpha）。默认值为 2f。

#### 3. AlphaMax
- `public float AlphaMax { get; set; }`
  - 获取或设置减法因子的最大阈值（alpha）。默认值为 5f。

#### 4. SnrMin
- `public float SnrMin { get; set; }`
  - 获取或设置最小信噪比（以 dB 为单位）。默认值为 -5f。

#### 5. SnrMax
- `public float SnrMax { get; set; }`
  - 获取或设置最大信噪比（以 dB 为单位）。默认值为 20f。

### 方法

#### 1. SpectralSubtractor 构造函数
- `public SpectralSubtractor(float[] noise, int fftSize = 1024, int hopSize = 128)`
  - 构造 SpectralSubtractor 实例，指定噪声样本数组、FFT 大小和跳跃长度。
  - 参数:
    - `noise`: 噪声样本数组。
    - `fftSize`: FFT 大小，默认为 1024。
    - `hopSize`: 跳跃长度（样本数），默认为 128。

- `public SpectralSubtractor(DiscreteSignal noise, int fftSize = 1024, int hopSize = 128)`
  - 构造 SpectralSubtractor 实例，指定噪声信号、FFT 大小和跳跃长度。
  - 参数:
    - `noise`: 噪声信号。
    - `fftSize`: FFT 大小，默认为 1024。
    - `hopSize`: 跳跃长度（样本数），默认为 128。

#### 2. ProcessSpectrum
- `protected override void ProcessSpectrum(float[] re, float[] im, float[] filteredRe, float[] filteredIm)`
  - 在每个 STFT 步骤中处理一个谱。
  - 参数:
    - `re`: 输入谱的实部。
    - `im`: 输入谱的虚部。
    - `filteredRe`: 输出谱的实部。
    - `filteredIm`: 输出谱的虚部。

#### 3. EstimateNoise
- `public void EstimateNoise(float[] noise, int startPos = 0, int endPos = -1)`
  - 估计噪声的功率谱。
  - 参数:
    - `noise`: 噪声样本数组。
    - `startPos`: 数组中处理的第一个样本的索引。
    - `endPos`: 数组中处理的最后一个样本的索引。

- `public void EstimateNoise(DiscreteSignal noise, int startPos = 0, int endPos = -1)`
  - 估计噪声信号的功率谱。
  - 参数:
    - `noise`: 噪声信号。
    - `startPos`: 信号中处理的第一个样本的索引。
    - `endPos`: 信号中处理的最后一个样本的索引。

### 代码示例
以下是一个使用 SpectralSubtractor 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations;

public class SpectralSubtractorExample
{
    public static void Main()
    {
        // 定义噪声样本数组
        float[] noiseSamples = { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f };

        // 创建 SpectralSubtractor 实例
        var subtractor = new SpectralSubtractor(noiseSamples);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f };
        var signal = new DiscreteSignal(44100, inputSignal);

        // 执行谱减法去噪
        var denoisedSignal = subtractor.ApplyTo(signal);
        Console.WriteLine("Denoised Signal:");
        foreach (var sample in denoisedSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Operations.WaveShaper 类

Vorcyc.Mathematics.SignalProcessing.Operations.WaveShaper 是一个用于波形整形的类。

### 属性

无

### 方法

#### 1. WaveShaper 构造函数
- `public WaveShaper(Func<float, float> waveShapingFunction)`
  - 构造 WaveShaper 实例，指定波形整形函数。
  - 参数:
    - `waveShapingFunction`: 波形整形函数。

#### 2. Process
- `public float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public void Reset()`
  - 重置波形整形器。

#### 4. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 处理整个信号并返回新的波形整形信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 滤波方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

### 代码示例
以下是一个使用 WaveShaper 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Operations;

public class WaveShaperExample
{
    public static void Main()
    {
        // 定义波形整形函数
        Func<float, float> waveShapingFunction = x => x * x;

        // 创建 WaveShaper 实例
        var waveShaper = new WaveShaper(waveShapingFunction);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f };
        var signal = new DiscreteSignal(44100, inputSignal);

        // 处理整个信号
        var outputSignal = waveShaper.ApplyTo(signal);
        Console.WriteLine("Wave-Shaped Signal:");
        foreach (var sample in outputSignal.Samples)
        {
            Console.WriteLine($"Sample: {sample}");
        }

        // 处理单个样本
        float processedSample = waveShaper.Process(0.85f);
        Console.WriteLine($"Processed Sample: {processedSample}");

        // 重置波形整形器
        waveShaper.Reset();
    }
}
```

