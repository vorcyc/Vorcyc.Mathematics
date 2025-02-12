当前位置 : [根目录](README.md)/[信号处理模块](Module_SignalProcessing.md)/[信号定义和相关操作](Module_SignalProcessing_Signals.md)

# 信号处理模块 - Signal Processing Module
## 信号定义和相关操作 - Signals

> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Signals

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

---


> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Signals.Builders 命名空间。



## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.AdsrBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.AdsrBuilder 是一个用于构建 ADSR 包络的类，继承了 SignalBuilder 类。

### 枚举

#### 1. AdsrState
- `public enum AdsrState`
  - 表示 ADSR 包络的状态。
  - 枚举成员:
    - `Attack`: 攻击阶段。
    - `Decay`: 衰减阶段。
    - `Sustain`: 持续阶段。
    - `Release`: 释放阶段。

### 属性

#### 1. State
- `public AdsrState State { get; private set; }`
  - 获取当前的 ADSR 状态（攻击、衰减、持续、释放）。

### 方法

#### 1. AdsrBuilder 构造函数
- `public AdsrBuilder(int attack, int decay, int sustain, int release)`
  - 使用 ADSR 参数（以样本数表示）构造 AdsrBuilder 实例。
  - 参数:
    - `attack`: 攻击阶段的样本数。
    - `decay`: 衰减阶段的样本数。
    - `sustain`: 持续阶段的样本数。
    - `release`: 释放阶段的样本数。

- `public AdsrBuilder(float attack, float decay, float sustain, float release)`
  - 使用 ADSR 参数（以秒为单位）构造 AdsrBuilder 实例。
  - 参数:
    - `attack`: 攻击阶段的持续时间（秒）。
    - `decay`: 衰减阶段的持续时间（秒）。
    - `sustain`: 持续阶段的持续时间（秒）。
    - `release`: 释放阶段的持续时间（秒）。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. SampledAt
- `public override SignalBuilder SampledAt(int samplingRate)`
  - 设置信号的采样率。
  - 参数:
    - `samplingRate`: 采样率。
  - 返回值: 设置采样率后的 SignalBuilder 实例。

### 代码示例
以下是一个使用 AdsrBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class AdsrBuilderExample
{
    public static void Main()
    {
        // 使用样本数构造 AdsrBuilder 实例
        var adsrBuilder = new AdsrBuilder(100, 200, 300, 400);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(adsrBuilder.NextSample());
        }

        // 重置样本生成器
        adsrBuilder.Reset();

        // 使用秒数构造 AdsrBuilder 实例
        var adsrBuilderSeconds = new AdsrBuilder(0.1f, 0.2f, 0.3f, 0.4f);

        // 设置采样率
        adsrBuilderSeconds.SampledAt(44100);

        // 生成样本并输出
        Console.WriteLine("Generated Samples (Seconds):");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(adsrBuilderSeconds.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.AwgnBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.AwgnBuilder 是一个用于生成加性白高斯噪声（AWGN）的类，继承了 SignalBuilder 类。使用 Box-Muller 变换生成加性白高斯噪声。

### 属性

无

### 方法

#### 1. AwgnBuilder 构造函数
- `public AwgnBuilder()`
  - 构造 AwgnBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"mean", "mu"`: 均值，默认值为 0.0。
- `"sigma", "stddev"`: 标准差，默认值为 1.0。

### 代码示例
以下是一个使用 AwgnBuilder 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class AwgnBuilderExample
{
    public static void Main()
    {
        // 创建 AwgnBuilder 实例
        var awgnBuilder = new AwgnBuilder();

        // 设置参数
        awgnBuilder.SetParameter("mean", 0.0f);
        awgnBuilder.SetParameter("sigma", 1.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(awgnBuilder.NextSample());
        }

        // 重置样本生成器
        awgnBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(awgnBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.ChirpBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.ChirpBuilder 是一个用于生成啁啾信号的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. ChirpBuilder 构造函数
- `public ChirpBuilder()`
  - 构造 ChirpBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。
- `"start", "f0", "freq0"`: 起始频率，默认值为 100.0 Hz。
- `"end", "f1", "freq1"`: 结束频率，默认值为 1000.0 Hz。

### 代码示例
以下是一个使用 ChirpBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;
public class ChirpBuilderExample
{
    public static void Main()
    {
        // 创建 ChirpBuilder 实例
        var chirpBuilder = new ChirpBuilder();

        // 设置参数
        chirpBuilder.SetParameter("low", -1.0f);
        chirpBuilder.SetParameter("high", 1.0f);
        chirpBuilder.SetParameter("start", 100.0f);
        chirpBuilder.SetParameter("end", 1000.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(chirpBuilder.NextSample());
        }

        // 重置样本生成器
        chirpBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(chirpBuilder.NextSample());
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.CosineBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.CosineBuilder 是一个用于生成余弦信号的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. CosineBuilder 构造函数
- `public CosineBuilder()`
  - 构造 CosineBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。
- `"frequency", "freq"`: 频率，默认值为 100.0 Hz。
- `"phase", "phi"`: 初始相位，默认值为 0.0。

### 代码示例
以下是一个使用 CosineBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class CosineBuilderExample
{
    public static void Main()
    {
        // 创建 CosineBuilder 实例
        var cosineBuilder = new CosineBuilder();

        // 设置参数
        cosineBuilder.SetParameter("low", -1.0f);
        cosineBuilder.SetParameter("high", 1.0f);
        cosineBuilder.SetParameter("frequency", 100.0f);
        cosineBuilder.SetParameter("phase", 0.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(cosineBuilder.NextSample());
        }

        // 重置样本生成器
        cosineBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(cosineBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.FadeInOutBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.FadeInOutBuilder 是一个用于对信号生成器进行淡入淡出效果装饰的类，继承了 SignalBuilder 类。

### 属性

#### 1. FadeStarted
- `public bool FadeStarted { get; protected set; }`
  - 获取信号是否开始淡出。

#### 2. FadeFinished
- `public bool FadeFinished => _fadeOutIndex <= 0`
  - 获取信号是否完成淡出。

### 方法

#### 1. FadeInOutBuilder 构造函数
- `public FadeInOutBuilder(SignalBuilder builder)`
  - 构造 FadeInOutBuilder 实例。
  - 参数:
    - `builder`: 底层信号生成器。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. In
- `public FadeInOutBuilder In(double seconds)`
  - 设置淡入部分的持续时间（秒）。
  - 参数:
    - `seconds`: 持续时间（秒）。
  - 返回值: 当前的 FadeInOutBuilder 实例。

#### 5. Out
- `public FadeInOutBuilder Out(double seconds)`
  - 设置淡出部分的持续时间（秒）。
  - 参数:
    - `seconds`: 持续时间（秒）。
  - 返回值: 当前的 FadeInOutBuilder 实例。

#### 6. FadeOut
- `public void FadeOut()`
  - 开始淡出。

### 代码示例
以下是一个使用 FadeInOutBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class FadeInOutBuilderExample
{
    public static void Main()
    {
        // 创建一个 SineBuilder 实例
        var sineBuilder = new SineBuilder().OfFrequency(440).SampledAt(44100).OfLength(44100);

        // 创建一个 FadeInOutBuilder 实例，装饰 SineBuilder
        var fadeSineBuilder = new FadeInOutBuilder(sineBuilder).In(0.05).Out(0.2);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(fadeSineBuilder.NextSample());
        }

        // 重置样本生成器
        fadeSineBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(fadeSineBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.KarplusStrongBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.KarplusStrongBuilder 是一个使用 Karplus-Strong 算法生成信号的类，继承了 WaveTableBuilder 类。

### 属性

无

### 方法

#### 1. KarplusStrongBuilder 构造函数
- `public KarplusStrongBuilder()`
  - 构造 KarplusStrongBuilder 实例。

- `public KarplusStrongBuilder(float[] samples)`
  - 使用样本数组构造 KarplusStrongBuilder 实例。
  - 参数:
    - `samples`: 样本数组。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. SampledAt
- `public override SignalBuilder SampledAt(int samplingRate)`
  - 设置信号的采样率。
  - 参数:
    - `samplingRate`: 采样率。
  - 返回值: 设置采样率后的 SignalBuilder 实例。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"frequency", "freq", "f"`: 频率，默认值为 100.0 Hz。
- `"stretch", "s"`: 拉伸因子，默认值为 1.0。
- `"feedback", "a"`: 反馈系数，默认值为 1.0。

### 代码示例
以下是一个使用 KarplusStrongBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class KarplusStrongBuilderExample
{
    public static void Main()
    {
        // 创建 KarplusStrongBuilder 实例
        var karplusStrongBuilder = new KarplusStrongBuilder();

        // 设置参数
        karplusStrongBuilder.SetParameter("frequency", 440.0);
        karplusStrongBuilder.SetParameter("stretch", 1.0);
        karplusStrongBuilder.SetParameter("feedback", 0.98);

        // 设置信号的采样率
        karplusStrongBuilder.SampledAt(44100);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(karplusStrongBuilder.NextSample());
        }

        // 重置样本生成器
        karplusStrongBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(karplusStrongBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.KarplusStrongDrumBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.KarplusStrongDrumBuilder 是一个使用 Karplus-Strong 算法的 "Drum" 变体生成信号的类，继承了 KarplusStrongBuilder 类。

### 属性

无

### 方法

#### 1. KarplusStrongDrumBuilder 构造函数
- `public KarplusStrongDrumBuilder()`
  - 构造 KarplusStrongDrumBuilder 实例。

- `public KarplusStrongDrumBuilder(float[] samples)`
  - 使用样本数组构造 KarplusStrongDrumBuilder 实例。
  - 参数:
    - `samples`: 样本数组。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"frequency", "freq", "f"`: 频率，默认值为 100.0 Hz。
- `"stretch", "s"`: 拉伸因子，默认值为 1.0。
- `"feedback", "a"`: 反馈系数，默认值为 1.0。
- `"probability", "prob"`: 概率，默认值为 0.5。

### 代码示例
以下是一个使用 KarplusStrongDrumBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class KarplusStrongDrumBuilderExample
{
    public static void Main()
    {
        // 创建 KarplusStrongDrumBuilder 实例
        var karplusStrongDrumBuilder = new KarplusStrongDrumBuilder();

        // 设置参数
        karplusStrongDrumBuilder.SetParameter("frequency", 440.0);
        karplusStrongDrumBuilder.SetParameter("stretch", 1.0);
        karplusStrongDrumBuilder.SetParameter("feedback", 0.98);
        karplusStrongDrumBuilder.SetParameter("probability", 0.5);

        // 设置信号的采样率
        karplusStrongDrumBuilder.SampledAt(44100);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(karplusStrongDrumBuilder.NextSample());
        }

        // 重置样本生成器
        karplusStrongDrumBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(karplusStrongDrumBuilder.NextSample());
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.PadSynthBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.PadSynthBuilder 是一个使用 PadSynth 算法生成信号的类，继承了 WaveTableBuilder 类。

### 属性

无

### 方法

#### 1. PadSynthBuilder 构造函数
- `public PadSynthBuilder()`
  - 构造 PadSynthBuilder 实例。

#### 2. SetAmplitudes
- `public PadSynthBuilder SetAmplitudes(float[] amplitudes)`
  - 设置谐波的振幅。
  - 参数:
    - `amplitudes`: 振幅数组。
  - 返回值: 当前的 PadSynthBuilder 实例。

#### 3. SampledAt
- `public override SignalBuilder SampledAt(int samplingRate)`
  - 设置信号的采样率。
  - 参数:
    - `samplingRate`: 采样率。
  - 返回值: 设置采样率后的 SignalBuilder 实例。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"frequency", "freq", "f"`: 频率，默认值为 440.0 Hz。
- `"fftsize", "size"`: FFT 大小，默认值为 2048。
- `"bandwidth", "bw"`: 带宽，默认值为 40。
- `"bwscale", "scale"`: 带宽比例，默认值为 1.25。

### 代码示例
以下是一个使用 PadSynthBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class PadSynthBuilderExample
{
    public static void Main()
    {
        // 创建 PadSynthBuilder 实例
        var padSynthBuilder = new PadSynthBuilder();

        // 设置参数
        padSynthBuilder.SetParameter("frequency", 440.0);
        padSynthBuilder.SetParameter("fftsize", 2048);
        padSynthBuilder.SetParameter("bandwidth", 40.0);
        padSynthBuilder.SetParameter("bwscale", 1.25);

        // 设置谐波的振幅
        padSynthBuilder.SetAmplitudes(new float[] { 1.0f, 0.5f, 0.25f, 0.125f });

        // 设置信号的采样率
        padSynthBuilder.SampledAt(44100);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(padSynthBuilder.NextSample());
        }

        // 重置样本生成器
        padSynthBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine(padSynthBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.PerlinNoiseBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.PerlinNoiseBuilder 是一个用于生成 1D Perlin 噪声的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. PerlinNoiseBuilder 构造函数
- `public PerlinNoiseBuilder()`
  - 构造 PerlinNoiseBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。
- `"scale", "octave"`: 缩放比例，默认值为 0.02。

### 代码示例
以下是一个使用 PerlinNoiseBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class PerlinNoiseBuilderExample
{
    public static void Main()
    {
        // 创建 PerlinNoiseBuilder 实例
        var perlinNoiseBuilder = new PerlinNoiseBuilder();

        // 设置参数
        perlinNoiseBuilder.SetParameter("low", -1.0f);
        perlinNoiseBuilder.SetParameter("high", 1.0f);
        perlinNoiseBuilder.SetParameter("scale", 0.02f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(perlinNoiseBuilder.NextSample());
        }

        // 重置样本生成器
        perlinNoiseBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(perlinNoiseBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.PinkNoiseBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.PinkNoiseBuilder 是一个用于生成粉红噪声的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. PinkNoiseBuilder 构造函数
- `public PinkNoiseBuilder()`
  - 构造 PinkNoiseBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。

### 代码示例
以下是一个使用 PinkNoiseBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class PinkNoiseBuilderExample
{
    public static void Main()
    {
        // 创建 PinkNoiseBuilder 实例
        var pinkNoiseBuilder = new PinkNoiseBuilder();

        // 设置参数
        pinkNoiseBuilder.SetParameter("low", -1.0f);
        pinkNoiseBuilder.SetParameter("high", 1.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(pinkNoiseBuilder.NextSample());
        }

        // 重置样本生成器
        pinkNoiseBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(pinkNoiseBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.PulseWaveBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.PulseWaveBuilder 是一个用于生成周期性脉冲波的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. PulseWaveBuilder 构造函数
- `public PulseWaveBuilder()`
  - 构造 PulseWaveBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。
- `"pulse", "width"`: 脉冲持续时间，默认值为 0.05 秒。
- `"period", "t"`: 脉冲波周期，默认值为 0.1 秒。

### 代码示例
以下是一个使用 PulseWaveBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class PulseWaveBuilderExample
{
    public static void Main()
    {
        // 创建 PulseWaveBuilder 实例
        var pulseWaveBuilder = new PulseWaveBuilder();

        // 设置参数
        pulseWaveBuilder.SetParameter("low", -1.0f);
        pulseWaveBuilder.SetParameter("high", 1.0f);
        pulseWaveBuilder.SetParameter("pulse", 0.05f);
        pulseWaveBuilder.SetParameter("period", 0.1f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(pulseWaveBuilder.NextSample());
        }

        // 重置样本生成器
        pulseWaveBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(pulseWaveBuilder.NextSample());
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.RampBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.RampBuilder 是一个用于生成直线信号的类，继承了 SignalBuilder 类。信号的形式为 y[n] = slope * n + intercept。

### 属性

无

### 方法

#### 1. RampBuilder 构造函数
- `public RampBuilder()`
  - 构造 RampBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"slope", "k"`: 斜率，默认值为 0.0。
- `"intercept", "b"`: 截距，默认值为 0.0。

### 代码示例
以下是一个使用 RampBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class RampBuilderExample
{
    public static void Main()
    {
        // 创建 RampBuilder 实例
        var rampBuilder = new RampBuilder();

        // 设置参数
        rampBuilder.SetParameter("slope", 0.1f);
        rampBuilder.SetParameter("intercept", 1.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(rampBuilder.NextSample());
        }

        // 重置样本生成器
        rampBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(rampBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.RedNoiseBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.RedNoiseBuilder 是一个用于生成红噪声（布朗噪声）的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. RedNoiseBuilder 构造函数
- `public RedNoiseBuilder()`
  - 构造 RedNoiseBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。

### 代码示例
以下是一个使用 RedNoiseBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class RedNoiseBuilderExample
{
    public static void Main()
    {
        // 创建 RedNoiseBuilder 实例
        var redNoiseBuilder = new RedNoiseBuilder();

        // 设置参数
        redNoiseBuilder.SetParameter("low", -1.0f);
        redNoiseBuilder.SetParameter("high", 1.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(redNoiseBuilder.NextSample());
        }

        // 重置样本生成器
        redNoiseBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(redNoiseBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.SawtoothBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.SawtoothBuilder 是一个用于生成锯齿波信号的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. SawtoothBuilder 构造函数
- `public SawtoothBuilder()`
  - 构造 SawtoothBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. SampledAt
- `public override SignalBuilder SampledAt(int samplingRate)`
  - 设置信号的采样率。
  - 参数:
    - `samplingRate`: 采样率。
  - 返回值: 设置采样率后的 SignalBuilder 实例。

#### 5. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。
- `"frequency", "freq"`: 频率，默认值为 100.0 Hz。

### 代码示例
以下是一个使用 SawtoothBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class SawtoothBuilderExample
{
    public static void Main()
    {
        // 创建 SawtoothBuilder 实例
        var sawtoothBuilder = new SawtoothBuilder();

        // 设置参数
        sawtoothBuilder.SetParameter("low", -1.0f);
        sawtoothBuilder.SetParameter("high", 1.0f);
        sawtoothBuilder.SetParameter("frequency", 100.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(sawtoothBuilder.NextSample());
        }

        // 重置样本生成器
        sawtoothBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(sawtoothBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.SincBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.SincBuilder 是一个用于生成 Sinc(x) 信号的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. SincBuilder 构造函数
- `public SincBuilder()`
  - 构造 SincBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。
- `"frequency", "freq"`: 频率，默认值为 100.0 Hz。

### 代码示例
以下是一个使用 SincBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class SincBuilderExample
{
    public static void Main()
    {
        // 创建 SincBuilder 实例
        var sincBuilder = new SincBuilder();

        // 设置参数
        sincBuilder.SetParameter("low", -1.0f);
        sincBuilder.SetParameter("high", 1.0f);
        sincBuilder.SetParameter("frequency", 100.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(sincBuilder.NextSample());
        }

        // 重置样本生成器
        sincBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(sincBuilder.NextSample());
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.SineBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.SineBuilder 是一个用于生成正弦波信号的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. SineBuilder 构造函数
- `public SineBuilder()`
  - 构造 SineBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。
- `"frequency", "freq"`: 频率，默认值为 100.0 Hz。
- `"phase", "phi"`: 初始相位，默认值为 0.0。

### 代码示例
以下是一个使用 SineBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class SineBuilderExample
{
    public static void Main()
    {
        // 创建 SineBuilder 实例
        var sineBuilder = new SineBuilder();

        // 设置参数
        sineBuilder.SetParameter("low", -1.0f);
        sineBuilder.SetParameter("high", 1.0f);
        sineBuilder.SetParameter("frequency", 100.0f);
        sineBuilder.SetParameter("phase", 0.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(sineBuilder.NextSample());
        }

        // 重置样本生成器
        sineBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(sineBuilder.NextSample());
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.SquareWaveBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.SquareWaveBuilder 是一个用于生成方波信号的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. SquareWaveBuilder 构造函数
- `public SquareWaveBuilder()`
  - 构造 SquareWaveBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. SampledAt
- `public override SignalBuilder SampledAt(int samplingRate)`
  - 设置信号的采样率。
  - 参数:
    - `samplingRate`: 采样率。
  - 返回值: 设置采样率后的 SignalBuilder 实例。

#### 5. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。
- `"frequency", "freq"`: 频率，默认值为 100.0 Hz。

### 代码示例
以下是一个使用 SquareWaveBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class SquareWaveBuilderExample
{
    public static void Main()
    {
        // 创建 SquareWaveBuilder 实例
        var squareWaveBuilder = new SquareWaveBuilder();

        // 设置参数
        squareWaveBuilder.SetParameter("low", -1.0f);
        squareWaveBuilder.SetParameter("high", 1.0f);
        squareWaveBuilder.SetParameter("frequency", 100.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(squareWaveBuilder.NextSample());
        }

        // 重置样本生成器
        squareWaveBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(squareWaveBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.TriangleWaveBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.TriangleWaveBuilder 是一个用于生成三角波信号的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. TriangleWaveBuilder 构造函数
- `public TriangleWaveBuilder()`
  - 构造 TriangleWaveBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. SampledAt
- `public override SignalBuilder SampledAt(int samplingRate)`
  - 设置信号的采样率。
  - 参数:
    - `samplingRate`: 采样率。
  - 返回值: 设置采样率后的 SignalBuilder 实例。

#### 5. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。
- `"frequency", "freq"`: 频率，默认值为 100.0 Hz。

### 代码示例
以下是一个使用 TriangleWaveBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class TriangleWaveBuilderExample
{
    public static void Main()
    {
        // 创建 TriangleWaveBuilder 实例
        var triangleWaveBuilder = new TriangleWaveBuilder();

        // 设置参数
        triangleWaveBuilder.SetParameter("low", -1.0f);
        triangleWaveBuilder.SetParameter("high", 1.0f);
        triangleWaveBuilder.SetParameter("frequency", 100.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(triangleWaveBuilder.NextSample());
        }

        // 重置样本生成器
        triangleWaveBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(triangleWaveBuilder.NextSample());
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.WaveTableBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.WaveTableBuilder 是一个用于生成基于波表的信号的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. WaveTableBuilder 构造函数
- `public WaveTableBuilder(float[] samples)`
  - 使用样本数组构造 WaveTableBuilder 实例。
  - 参数:
    - `samples`: 波表样本数组。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"stride", "step", "delta"`: 步长，默认值为 1。

### 代码示例
以下是一个使用 WaveTableBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class WaveTableBuilderExample
{
    public static void Main()
    {
        // 定义波表样本数组
        float[] waveTableSamples = { 0.0f, 0.5f, 1.0f, 0.5f, 0.0f, -0.5f, -1.0f, -0.5f };

        // 创建 WaveTableBuilder 实例
        var waveTableBuilder = new WaveTableBuilder(waveTableSamples);

        // 设置参数
        waveTableBuilder.SetParameter("stride", 1.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(waveTableBuilder.NextSample());
        }

        // 重置样本生成器
        waveTableBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(waveTableBuilder.NextSample());
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Signals.Builders.WhiteNoiseBuilder 类

Vorcyc.Mathematics.SignalProcessing.Signals.Builders.WhiteNoiseBuilder 是一个用于生成白噪声信号的类，继承了 SignalBuilder 类。

### 属性

无

### 方法

#### 1. WhiteNoiseBuilder 构造函数
- `public WhiteNoiseBuilder()`
  - 构造 WhiteNoiseBuilder 实例。

#### 2. NextSample
- `public override float NextSample()`
  - 生成新的样本。
  - 返回值: 生成的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置样本生成器。

#### 4. Generate
- `protected override DiscreteSignal Generate()`
  - 生成信号，通过逐个生成所有样本。
  - 返回值: 生成的 `DiscreteSignal` 对象。

### 参数设置

可以在 `SignalBuilder.SetParameter(string, double)` 方法中设置以下参数：
- `"low", "lo", "min"`: 下限幅度，默认值为 -1.0。
- `"high", "hi", "max"`: 上限幅度，默认值为 1.0。

### 代码示例
以下是一个使用 WhiteNoiseBuilder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders;

public class WhiteNoiseBuilderExample
{
    public static void Main()
    {
        // 创建 WhiteNoiseBuilder 实例
        var whiteNoiseBuilder = new WhiteNoiseBuilder();

        // 设置参数
        whiteNoiseBuilder.SetParameter("low", -1.0f);
        whiteNoiseBuilder.SetParameter("high", 1.0f);

        // 生成样本并输出
        Console.WriteLine("Generated Samples:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(whiteNoiseBuilder.NextSample());
        }

        // 重置样本生成器
        whiteNoiseBuilder.Reset();

        // 生成样本并输出
        Console.WriteLine("Generated Samples after Reset:");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(whiteNoiseBuilder.NextSample());
        }
    }
}
```


---


> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Signals 命名空间。


## Vorcyc.Mathematics.SignalProcessing.Signals.ComplexDiscreteSignal 类

Vorcyc.Mathematics.SignalProcessing.Signals.ComplexDiscreteSignal 是一个用于表示有限复数值离散时间信号的类。信号以一定的采样率存储为两个数据数组（实部和虚部）。

### 属性

#### 1. SamplingRate
- `public int SamplingRate { get; }`
  - 获取采样率（每秒的样本数）。

#### 2. Real
- `public float[] Real { get; }`
  - 获取复数值样本的实部。

#### 3. Imag
- `public float[] Imag { get; }`
  - 获取复数值样本的虚部。

#### 4. Length
- `public int Length => Real.Length`
  - 获取信号的长度。

#### 5. Magnitude
- `public float[] Magnitude`
  - 获取复数值样本的幅度。

#### 6. Power
- `public float[] Power`
  - 获取复数值样本的功率（幅度的平方）。

#### 7. Phase
- `public float[] Phase`
  - 获取复数值样本的相位。

#### 8. PhaseUnwrapped
- `public float[] PhaseUnwrapped`
  - 获取复数值样本的展开相位。

### 方法

#### 1. ComplexDiscreteSignal 构造函数
- `public ComplexDiscreteSignal(int samplingRate, float[] real, float[] imag = null, bool allocateNew = false)`
  - 使用实部和虚部数组构造 ComplexDiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `real`: 复数值信号的实部数组。
    - `imag`: 复数值信号的虚部数组，默认为 null。
    - `allocateNew`: 如果应为数据分配新内存，则设置为 true，默认为 false。

- `public ComplexDiscreteSignal(int samplingRate, IEnumerable<float> real, IEnumerable<float> imag = null)`
  - 使用实部和虚部集合构造 ComplexDiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `real`: 复数值信号的实部集合。
    - `imag`: 复数值信号的虚部集合，默认为 null。

- `public ComplexDiscreteSignal(int samplingRate, IEnumerable<ComplexFp32> samples)`
  - 使用复数值样本集合构造 ComplexDiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 复数值样本集合。

- `public ComplexDiscreteSignal(int samplingRate, int length, float real = 0.0f, float imag = 0.0f)`
  - 构造具有指定长度并填充指定值的信号。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `length`: 样本数。
    - `real`: 每个样本的实部值，默认为 0.0。
    - `imag`: 每个样本的虚部值，默认为 0.0。

- `public ComplexDiscreteSignal(int samplingRate, IEnumerable<int> samples, float normalizeFactor = 1.0f)`
  - 使用整数样本集合构造信号，并在给定采样率下进行归一化。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 整数样本集合。
    - `normalizeFactor`: 每个样本将除以此值，默认为 1.0。

#### 2. Copy
- `public ComplexDiscreteSignal Copy()`
  - 创建信号的深拷贝。
  - 返回值: 信号的深拷贝。

#### 3. 索引器
- `public float this[int index] { get; set; }`
  - 样本索引器。仅适用于样本实部数组。谨慎使用。
  - 参数:
    - `index`: 样本索引。
  - 返回值: 样本的实部值。

- `public ComplexDiscreteSignal this[int startPos, int endPos] { get; }`
  - 创建信号的切片。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号的切片。

### 重载运算符

#### 1. operator +
- `public static ComplexDiscreteSignal operator +(ComplexDiscreteSignal s1, ComplexDiscreteSignal s2)`
  - 通过叠加信号 s1 和 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `s1`: 第一个信号。
    - `s2`: 第二个信号。
  - 返回值: 叠加后的新信号。

- `public static ComplexDiscreteSignal operator +(ComplexDiscreteSignal s, float constant)`
  - 通过将常数添加到信号 s 创建新信号。
  - 参数:
    - `s`: 信号。
    - `constant`: 要添加到每个样本的常数。
  - 返回值: 添加常数后的新信号。

#### 2. operator -
- `public static ComplexDiscreteSignal operator -(ComplexDiscreteSignal s, float constant)`
  - 通过从信号 s 中减去常数创建新信号。
  - 参数:
    - `s`: 信号。
    - `constant`: 要从每个样本中减去的常数。
  - 返回值: 减去常数后的新信号。

#### 3. operator *
- `public static ComplexDiscreteSignal operator *(ComplexDiscreteSignal s, float coeff)`
  - 通过将信号 s 乘以系数（放大/衰减）创建新信号。
  - 参数:
    - `s`: 信号。
    - `coeff`: 放大/衰减系数。
  - 返回值: 乘以系数后的新信号。

### 代码示例
以下是一个使用 ComplexDiscreteSignal 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals;

public class ComplexDiscreteSignalExample
{
    public static void Main()
    {
        // 定义采样率
        int samplingRate = 44100;

        // 定义实部和虚部数组
        float[] real = { 0.5f, 0.6f, 0.55f };
        float[] imag = { 0.1f, 0.2f, 0.15f };

        // 创建 ComplexDiscreteSignal 实例
        var signal = new ComplexDiscreteSignal(samplingRate, real, imag);

        // 输出信号的实部和虚部
        Console.WriteLine("Real Parts:");
        foreach (var r in signal.Real)
        {
            Console.WriteLine(r);
        }

        Console.WriteLine("Imaginary Parts:");
        foreach (var i in signal.Imag)
        {
            Console.WriteLine(i);
        }

        // 获取并输出信号的幅度
        var magnitude = signal.Magnitude;
        Console.WriteLine("Magnitude:");
        foreach (var m in magnitude)
        {
            Console.WriteLine(m);
        }

        // 获取并输出信号的相位
        var phase = signal.Phase;
        Console.WriteLine("Phase:");
        foreach (var p in phase)
        {
            Console.WriteLine(p);
        }

        // 创建信号的切片
        var slicedSignal = signal[1, 3];
        Console.WriteLine("Sliced Signal Real Parts:");
        foreach (var r in slicedSignal.Real)
        {
            Console.WriteLine(r);
        }

        // 创建信号的深拷贝
        var copiedSignal = signal.Copy();
        Console.WriteLine("Copied Signal Real Parts:");
        foreach (var r in copiedSignal.Real)
        {
            Console.WriteLine(r);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.ComplexDiscreteSignal&lt;T> 类

`Vorcyc.Mathematics.SignalProcessing.Signals.ComplexDiscreteSignal<T>` 是一个用于表示有限复数值离散时间信号的类。信号以一定的采样率存储为两个数据数组（实部和虚部）。

### 属性

#### 1. SamplingRate
- `public int SamplingRate { get; }`
  - 获取采样率（每秒的样本数）。

#### 2. Real
- `public T[] Real { get; }`
  - 获取复数值样本的实部。

#### 3. Imag
- `public T[] Imag { get; }`
  - 获取复数值样本的虚部。

#### 4. Length
- `public int Length => Real.Length`
  - 获取信号的长度。

#### 5. Magnitude
- `public T[] Magnitude`
  - 获取复数值样本的幅度。

#### 6. Power
- `public T[] Power`
  - 获取复数值样本的功率（幅度的平方）。

#### 7. Phase
- `public T[] Phase`
  - 获取复数值样本的相位。

#### 8. PhaseUnwrapped
- `public T[] PhaseUnwrapped`
  - 获取复数值样本的展开相位。

### 方法

#### 1. ComplexDiscreteSignal 构造函数
- `public ComplexDiscreteSignal(int samplingRate, T[] real, T[]? imag = null, bool allocateNew = false)`
  - 使用实部和虚部数组构造 ComplexDiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `real`: 复数值信号的实部数组。
    - `imag`: 复数值信号的虚部数组，默认为 null。
    - `allocateNew`: 如果应为数据分配新内存，则设置为 true，默认为 false。

- `public ComplexDiscreteSignal(int samplingRate, IEnumerable<T> real, IEnumerable<T>? imag = null)`
  - 使用实部和虚部集合构造 ComplexDiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `real`: 复数值信号的实部集合。
    - `imag`: 复数值信号的虚部集合，默认为 null。

- `public ComplexDiscreteSignal(int samplingRate, IEnumerable<Complex<T>> samples)`
  - 使用复数值样本集合构造 ComplexDiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 复数值样本集合。

- `public ComplexDiscreteSignal(int samplingRate, int length, T real = default, T imag = default)`
  - 构造具有指定长度并填充指定值的信号。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `length`: 样本数。
    - `real`: 每个样本的实部值，默认为 0.0。
    - `imag`: 每个样本的虚部值，默认为 0.0。

- `public ComplexDiscreteSignal(int samplingRate, IEnumerable<T> samples, T? normalizeFactor = null)`
  - 使用整数样本集合构造信号，并在给定采样率下进行归一化。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 整数样本集合。
    - `normalizeFactor`: 每个样本将除以此值，默认为 1.0。

#### 2. Copy
- `public ComplexDiscreteSignal<T> Copy()`
  - 创建信号的深拷贝。
  - 返回值: 信号的深拷贝。

#### 3. 索引器
- `public T this[int index] { get; set; }`
  - 样本索引器。仅适用于样本实部数组。谨慎使用。
  - 参数:
    - `index`: 样本索引。
  - 返回值: 样本的实部值。

- `public ComplexDiscreteSignal<T> this[int startPos, int endPos] { get; }`
  - 创建信号的切片。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号的切片。

### 重载运算符

#### 1. operator +
- `public static ComplexDiscreteSignal<T> operator +(ComplexDiscreteSignal<T> s1, ComplexDiscreteSignal<T> s2)`
  - 通过叠加信号 s1 和 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `s1`: 第一个信号。
    - `s2`: 第二个信号。
  - 返回值: 叠加后的新信号。

- `public static ComplexDiscreteSignal<T> operator +(ComplexDiscreteSignal<T> s, T constant)`
  - 通过将常数添加到信号 s 创建新信号。
  - 参数:
    - `s`: 信号。
    - `constant`: 要添加到每个样本的常数。
  - 返回值: 添加常数后的新信号。

#### 2. operator -
- `public static ComplexDiscreteSignal<T> operator -(ComplexDiscreteSignal<T> s, T constant)`
  - 通过从信号 s 中减去常数创建新信号。
  - 参数:
    - `s`: 信号。
    - `constant`: 要从每个样本中减去的常数。
  - 返回值: 减去常数后的新信号。

#### 3. operator *
- `public static ComplexDiscreteSignal<T> operator *(ComplexDiscreteSignal<T> s, T coeff)`
  - 通过将信号 s 乘以系数（放大/衰减）创建新信号。
  - 参数:
    - `s`: 信号。
    - `coeff`: 放大/衰减系数。
  - 返回值: 乘以系数后的新信号。

### 代码示例
以下是一个使用 ComplexDiscreteSignal<T> 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals;
public class ComplexDiscreteSignalExample
{
    public static void Main()
    {
        // 定义采样率
        int samplingRate = 44100;

        // 定义实部和虚部数组
        float[] real = { 0.5f, 0.6f, 0.55f };
        float[] imag = { 0.1f, 0.2f, 0.15f };

        // 创建 ComplexDiscreteSignal 实例
        var signal = new ComplexDiscreteSignal<float>(samplingRate, real, imag);

        // 输出信号的实部和虚部
        Console.WriteLine("Real Parts:");
        foreach (var r in signal.Real)
        {
            Console.WriteLine(r);
        }

        Console.WriteLine("Imaginary Parts:");
        foreach (var i in signal.Imag)
        {
            Console.WriteLine(i);
        }

        // 获取并输出信号的幅度
        var magnitude = signal.Magnitude;
        Console.WriteLine("Magnitude:");
        foreach (var m in magnitude)
        {
            Console.WriteLine(m);
        }

        // 获取并输出信号的相位
        var phase = signal.Phase;
        Console.WriteLine("Phase:");
        foreach (var p in phase)
        {
            Console.WriteLine(p);
        }

        // 创建信号的切片
        var slicedSignal = signal[1, 3];
        Console.WriteLine("Sliced Signal Real Parts:");
        foreach (var r in slicedSignal.Real)
        {
            Console.WriteLine(r);
        }

        // 创建信号的深拷贝
        var copiedSignal = signal.Copy();
        Console.WriteLine("Copied Signal Real Parts:");
        foreach (var r in copiedSignal.Real)
        {
            Console.WriteLine(r);
        }
    }
}

```


## Vorcyc.Mathematics.SignalProcessing.Signals.ComplexDiscreteSignalExtensions 类

Vorcyc.Mathematics.SignalProcessing.Signals.ComplexDiscreteSignalExtensions 提供了用于处理复数离散信号的扩展方法。

### 方法

#### 1. Delay
- `public static ComplexDiscreteSignal Delay(this ComplexDiscreteSignal signal, int delay)`
  - 创建 `signal` 的延迟副本，通过将其向右（正 `delay`）或向左（负 `delay`）移动。
  - 参数:
    - `signal`: 信号。
    - `delay`: 延迟（正数或负数的延迟样本数）。
  - 返回值: 延迟后的 `ComplexDiscreteSignal`。

- `public static ComplexDiscreteSignal<T> Delay<T>(this ComplexDiscreteSignal<T> signal, int delay) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 创建 `signal` 的延迟副本，通过将其向右（正 `delay`）或向左（负 `delay`）移动。
  - 参数:
    - `signal`: 信号。
    - `delay`: 延迟（正数或负数的延迟样本数）。
  - 返回值: 延迟后的 `ComplexDiscreteSignal<T>`。

#### 2. Superimpose
- `public static ComplexDiscreteSignal Superimpose(this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)`
  - 叠加 `signal1` 和 `signal2`。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 叠加后的 `ComplexDiscreteSignal`。

- `public static ComplexDiscreteSignal<T> Superimpose<T>(this ComplexDiscreteSignal<T> signal1, ComplexDiscreteSignal<T> signal2) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 叠加 `signal1` 和 `signal2`。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 叠加后的 `ComplexDiscreteSignal<T>`。

#### 3. Concatenate
- `public static ComplexDiscreteSignal Concatenate(this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)`
  - 连接 `signal1` 和 `signal2`。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 连接后的 `ComplexDiscreteSignal`。

- `public static ComplexDiscreteSignal<T> Concatenate<T>(this ComplexDiscreteSignal<T> signal1, ComplexDiscreteSignal<T> signal2) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 连接 `signal1` 和 `signal2`。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 连接后的 `ComplexDiscreteSignal<T>`。

#### 4. Repeat
- `public static ComplexDiscreteSignal Repeat(this ComplexDiscreteSignal signal, int n)`
  - 创建 `signal` 的副本，并重复 `n` 次。
  - 参数:
    - `signal`: 信号。
    - `n`: 重复次数。
  - 返回值: 重复后的 `ComplexDiscreteSignal`。

- `public static ComplexDiscreteSignal<T> Repeat<T>(this ComplexDiscreteSignal<T> signal, int n) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 创建 `signal` 的副本，并重复 `n` 次。
  - 参数:
    - `signal`: 信号。
    - `n`: 重复次数。
  - 返回值: 重复后的 `ComplexDiscreteSignal<T>`。

#### 5. Amplify
- `public static void Amplify(this ComplexDiscreteSignal signal, float coeff)`
  - 按 `coeff` 放大 `signal`。
  - 参数:
    - `signal`: 信号。
    - `coeff`: 放大系数。

- `public static void Amplify<T>(this ComplexDiscreteSignal<T> signal, T coeff) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 按 `coeff` 放大 `signal`。
  - 参数:
    - `signal`: 信号。
    - `coeff`: 放大系数。

#### 6. Attenuate
- `public static void Attenuate(this ComplexDiscreteSignal signal, float coeff)`
  - 按 `coeff` 衰减 `signal`。
  - 参数:
    - `signal`: 信号。
    - `coeff`: 衰减系数。

- `public static void Attenuate<T>(this ComplexDiscreteSignal<T> signal, T coeff) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 按 `coeff` 衰减 `signal`。
  - 参数:
    - `signal`: 信号。
    - `coeff`: 衰减系数。

#### 7. First
- `public static ComplexDiscreteSignal First(this ComplexDiscreteSignal signal, int n)`
  - 从 `signal` 的前 `n` 个样本创建新信号。
  - 参数:
    - `signal`: 信号。
    - `n`: 样本数。
  - 返回值: 新的 `ComplexDiscreteSignal`。

- `public static ComplexDiscreteSignal<T> First<T>(this ComplexDiscreteSignal<T> signal, int n) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 从 `signal` 的前 `n` 个样本创建新信号。
  - 参数:
    - `signal`: 信号。
    - `n`: 样本数。
  - 返回值: 新的 `ComplexDiscreteSignal<T>`。

#### 8. Last
- `public static ComplexDiscreteSignal Last(this ComplexDiscreteSignal signal, int n)`
  - 从 `signal` 的后 `n` 个样本创建新信号。
  - 参数:
    - `signal`: 信号。
    - `n`: 样本数。
  - 返回值: 新的 `ComplexDiscreteSignal`。

- `public static ComplexDiscreteSignal<T> Last<T>(this ComplexDiscreteSignal<T> signal, int n) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 从 `signal` 的后 `n` 个样本创建新信号。
  - 参数:
    - `signal`: 信号。
    - `n`: 样本数。
  - 返回值: 新的 `ComplexDiscreteSignal<T>`。

#### 9. ZeroPadded
- `public static ComplexDiscreteSignal ZeroPadded(this ComplexDiscreteSignal signal, int length)`
  - 从 `signal` 创建新的零填充复数离散信号。
  - 参数:
    - `signal`: 信号。
    - `length`: 零填充信号的长度。
  - 返回值: 零填充后的 `ComplexDiscreteSignal`。

- `public static ComplexDiscreteSignal<T> ZeroPadded<T>(this ComplexDiscreteSignal<T> signal, int length) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 从 `signal` 创建新的零填充复数离散信号。
  - 参数:
    - `signal`: 信号。
    - `length`: 零填充信号的长度。
  - 返回值: 零填充后的 `ComplexDiscreteSignal<T>`。

#### 10. Multiply
- `public static ComplexDiscreteSignal Multiply(this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)`
  - 执行 `signal1` 和 `signal2` 的复数乘法（按长度归一化）。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 乘法后的 `ComplexDiscreteSignal`。

- `public static ComplexDiscreteSignal<T> Multiply<T>(this ComplexDiscreteSignal<T> signal1, ComplexDiscreteSignal<T> signal2) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 执行 `signal1` 和 `signal2` 的复数乘法（按长度归一化）。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 乘法后的 `ComplexDiscreteSignal<T>`。

#### 11. Divide
- `public static ComplexDiscreteSignal Divide(this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)`
  - 执行 `signal1` 和 `signal2` 的复数除法（按长度归一化）。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 除法后的 `ComplexDiscreteSignal`。

- `public static ComplexDiscreteSignal<T> Divide<T>(this ComplexDiscreteSignal<T> signal1, ComplexDiscreteSignal<T> signal2) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 执行 `signal1` 和 `signal2` 的复数除法（按长度归一化）。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 除法后的 `ComplexDiscreteSignal<T>`。

#### 12. Unwrap
- `public static float[] Unwrap(this float[] phase, float tolerance = ConstantsFp32.PI)`
  - 展开复数值样本的相位。
  - 参数:
    - `phase`: 相位。
    - `tolerance`: 跳跃大小，默认为 `ConstantsFp32.PI`。
  - 返回值: 展开后的相位数组。

- `public static T[] Unwrap<T>(this T[] phase, T? tolerance = null) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 展开复数值样本的相位。
  - 参数:
    - `phase`: 相位。
    - `tolerance`: 跳跃大小，默认为 `T.Pi`。
  - 返回值: 展开后的相位数组。

#### 13. ToComplexNumbers
- `public static IEnumerable<ComplexFp32> ToComplexNumbers(this ComplexDiscreteSignal signal)`
  - 从 `signal` 样本中生成复数。
  - 参数:
    - `signal`: 复数离散信号。
  - 返回值: 复数序列。

- `public static IEnumerable<Complex<T>> ToComplexNumbers<T>(this ComplexDiscreteSignal<T> signal) where T : unmanaged, IFloatingPointIeee754<T>, IMinMaxValue<T>`
  - 从 `signal` 样本中生成复数。
  - 参数:
    - `signal`: 复数离散信号。
  - 返回值: 复数序列。

### 代码示例
以下是一个使用 ComplexDiscreteSignalExtensions 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals;

public class ComplexDiscreteSignalExtensionsExample
{
    public static void Main()
    {
        // 定义采样率
        int samplingRate = 44100;

        // 定义实部和虚部数组
        float[] real = { 0.5f, 0.6f, 0.55f };
        float[] imag = { 0.1f, 0.2f, 0.15f };

        // 创建 ComplexDiscreteSignal 实例
        var signal = new ComplexDiscreteSignal(samplingRate, real, imag);

        // 创建延迟信号
        var delayedSignal = signal.Delay(2);
        Console.WriteLine("Delayed Signal Real Parts:");
        foreach (var r in delayedSignal.Real)
        {
            Console.WriteLine(r);
        }

        // 叠加信号
        var superimposedSignal = signal.Superimpose(delayedSignal);
        Console.WriteLine("Superimposed Signal Real Parts:");
        foreach (var r in superimposedSignal.Real)
        {
            Console.WriteLine(r);
        }

        // 连接信号
        var concatenatedSignal = signal.Concatenate(delayedSignal);
        Console.WriteLine("Concatenated Signal Real Parts:");
        foreach (var r in concatenatedSignal.Real)
        {
            Console.WriteLine(r);
        }

        // 放大信号
        signal.Amplify(2.0f);
        Console.WriteLine("Amplified Signal Real Parts:");
        foreach (var r in signal.Real)
        {
            Console.WriteLine(r);
        }

        // 衰减信号
        signal.Attenuate(2.0f);
        Console.WriteLine("Attenuated Signal Real Parts:");
        foreach (var r in signal.Real)
        {
            Console.WriteLine(r);
        }

        // 获取前 n 个样本
        var firstSamples = signal.First(2);
        Console.WriteLine("First Samples Real Parts:");
        foreach (var r in firstSamples.Real)
        {
            Console.WriteLine(r);
        }

        // 获取后 n 个样本
        var lastSamples = signal.Last(2);
        Console.WriteLine("Last Samples Real Parts:");
        foreach (var r in lastSamples.Real)
        {
            Console.WriteLine(r);
        }

        // 零填充信号
        var zeroPaddedSignal = signal.ZeroPadded(8);
        Console.WriteLine("Zero Padded Signal Real Parts:");
        foreach (var r in zeroPaddedSignal.Real)
        {
            Console.WriteLine(r);
        }

        // 复数乘法
        var multipliedSignal = signal.Multiply(delayedSignal);
        Console.WriteLine("Multiplied Signal Real Parts:");
        foreach (var r in multipliedSignal.Real)
        {
            Console.WriteLine(r);
        }

        // 复数除法
        var dividedSignal = signal.Divide(delayedSignal);
        Console.WriteLine("Divided Signal Real Parts:");
        foreach (var r in dividedSignal.Real)
        {
            Console.WriteLine(r);
        }

        // 展开相位
        var unwrappedPhase = signal.Phase.Unwrap();
        Console.WriteLine("Unwrapped Phase:");
        foreach (var p in unwrappedPhase)
        {
            Console.WriteLine(p);
        }

        // 转换为复数
        var complexNumbers = signal.ToComplexNumbers();
        Console.WriteLine("Complex Numbers:");
        foreach (var c in complexNumbers)
        {
            Console.WriteLine($"Real: {c.Real}, Imaginary: {c.Imaginary}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.ComplexDiscreteSignal&lt;T> 类

`Vorcyc.Mathematics.SignalProcessing.Signals.ComplexDiscreteSignal<T>` 是一个用于表示有限复数值离散时间信号的类。信号以一定的采样率存储为两个数据数组（实部和虚部）。

### 属性

#### 1. SamplingRate
- `public int SamplingRate { get; }`
  - 获取采样率（每秒的样本数）。

#### 2. Real
- `public T[] Real { get; }`
  - 获取复数值样本的实部。

#### 3. Imag
- `public T[] Imag { get; }`
  - 获取复数值样本的虚部。

#### 4. Length
- `public int Length => Real.Length`
  - 获取信号的长度。

#### 5. Magnitude
- `public T[] Magnitude`
  - 获取复数值样本的幅度。

#### 6. Power
- `public T[] Power`
  - 获取复数值样本的功率（幅度的平方）。

#### 7. Phase
- `public T[] Phase`
  - 获取复数值样本的相位。

#### 8. PhaseUnwrapped
- `public T[] PhaseUnwrapped`
  - 获取复数值样本的展开相位。

### 方法

#### 1. ComplexDiscreteSignal 构造函数
- `public ComplexDiscreteSignal(int samplingRate, T[] real, T[]? imag = null, bool allocateNew = false)`
  - 使用实部和虚部数组构造 ComplexDiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `real`: 复数值信号的实部数组。
    - `imag`: 复数值信号的虚部数组，默认为 null。
    - `allocateNew`: 如果应为数据分配新内存，则设置为 true，默认为 false。

- `public ComplexDiscreteSignal(int samplingRate, IEnumerable<T> real, IEnumerable<T>? imag = null)`
  - 使用实部和虚部集合构造 ComplexDiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `real`: 复数值信号的实部集合。
    - `imag`: 复数值信号的虚部集合，默认为 null。

- `public ComplexDiscreteSignal(int samplingRate, IEnumerable<Complex<T>> samples)`
  - 使用复数值样本集合构造 ComplexDiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 复数值样本集合。

- `public ComplexDiscreteSignal(int samplingRate, int length, T real = default, T imag = default)`
  - 构造具有指定长度并填充指定值的信号。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `length`: 样本数。
    - `real`: 每个样本的实部值，默认为 0.0。
    - `imag`: 每个样本的虚部值，默认为 0.0。

- `public ComplexDiscreteSignal(int samplingRate, IEnumerable<T> samples, T? normalizeFactor = null)`
  - 使用整数样本集合构造信号，并在给定采样率下进行归一化。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 整数样本集合。
    - `normalizeFactor`: 每个样本将除以此值，默认为 1.0。

#### 2. Copy
- `public ComplexDiscreteSignal<T> Copy()`
  - 创建信号的深拷贝。
  - 返回值: 信号的深拷贝。

#### 3. 索引器
- `public T this[int index] { get; set; }`
  - 样本索引器。仅适用于样本实部数组。谨慎使用。
  - 参数:
    - `index`: 样本索引。
  - 返回值: 样本的实部值。

- `public ComplexDiscreteSignal<T> this[int startPos, int endPos] { get; }`
  - 创建信号的切片。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号的切片。

### 重载运算符

#### 1. operator +
- `public static ComplexDiscreteSignal<T> operator +(ComplexDiscreteSignal<T> s1, ComplexDiscreteSignal<T> s2)`
  - 通过叠加信号 s1 和 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `s1`: 第一个信号。
    - `s2`: 第二个信号。
  - 返回值: 叠加后的新信号。

- `public static ComplexDiscreteSignal<T> operator +(ComplexDiscreteSignal<T> s, T constant)`
  - 通过将常数添加到信号 s 创建新信号。
  - 参数:
    - `s`: 信号。
    - `constant`: 要添加到每个样本的常数。
  - 返回值: 添加常数后的新信号。

#### 2. operator -
- `public static ComplexDiscreteSignal<T> operator -(ComplexDiscreteSignal<T> s, T constant)`
  - 通过从信号 s 中减去常数创建新信号。
  - 参数:
    - `s`: 信号。
    - `constant`: 要从每个样本中减去的常数。
  - 返回值: 减去常数后的新信号。

#### 3. operator *
- `public static ComplexDiscreteSignal<T> operator *(ComplexDiscreteSignal<T> s, T coeff)`
  - 通过将信号 s 乘以系数（放大/衰减）创建新信号。
  - 参数:
    - `s`: 信号。
    - `coeff`: 放大/衰减系数。
  - 返回值: 乘以系数后的新信号。

### 代码示例
以下是一个使用 ComplexDiscreteSignal<T> 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals;

public class ComplexDiscreteSignalExample
{
    public static void Main()
    {
        // 定义采样率
        int samplingRate = 44100;

        // 定义实部和虚部数组
        float[] real = { 0.5f, 0.6f, 0.55f };
        float[] imag = { 0.1f, 0.2f, 0.15f };

        // 创建 ComplexDiscreteSignal 实例
        var signal = new ComplexDiscreteSignal<float>(samplingRate, real, imag);

        // 输出信号的实部和虚部
        Console.WriteLine("Real Parts:");
        foreach (var r in signal.Real)
        {
            Console.WriteLine(r);
        }

        Console.WriteLine("Imaginary Parts:");
        foreach (var i in signal.Imag)
        {
            Console.WriteLine(i);
        }

        // 获取并输出信号的幅度
        var magnitude = signal.Magnitude;
        Console.WriteLine("Magnitude:");
        foreach (var m in magnitude)
        {
            Console.WriteLine(m);
        }

        // 获取并输出信号的相位
        var phase = signal.Phase;
        Console.WriteLine("Phase:");
        foreach (var p in phase)
        {
            Console.WriteLine(p);
        }

        // 创建信号的切片
        var slicedSignal = signal[1, 3];
        Console.WriteLine("Sliced Signal Real Parts:");
        foreach (var r in slicedSignal.Real)
        {
            Console.WriteLine(r);
        }

        // 创建信号的深拷贝
        var copiedSignal = signal.Copy();
        Console.WriteLine("Copied Signal Real Parts:");
        foreach (var r in copiedSignal.Real)
        {
            Console.WriteLine(r);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.DiscreteSignal 类

`Vorcyc.Mathematics.SignalProcessing.Signals.DiscreteSignal` 是一个用于表示有限实数值离散时间信号的类。信号以一定的采样率存储为一个数据数组。

### 属性

#### 1. SamplingRate
- `public int SamplingRate { get; }`
  - 获取采样率（每秒的样本数）。

#### 2. SampleCount
- `public int SampleCount { get; }`
  - 获取样本量，分配后不变。

#### 3. ValidSampleCount
- `public int ValidSampleCount { get; }`
  - 获取有效样本量。

#### 4. Duration
- `public TimeSpan Duration { get; }`
  - 获取信号的持续时间。

#### 5. Samples
- `public PinnableArray<float> Samples { get; }`
  - 获取信号的样本数组。

#### 6. MemoryStrategy
- `public MemoryStrategy MemoryStrategy { get; }`
  - 获取信号的内存策略。

### 方法

#### 1. DiscreteSignal 构造函数
- `public DiscreteSignal(int samplingRate, TimeSpan duration, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和持续时间构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `duration`: 信号的持续时间。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, int count, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和样本数量构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `count`: 样本数量。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, float[] samples, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和样本数组构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 样本数组。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, float[] samples, int offset, int count, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率、样本数组、偏移量和样本数量构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 样本数组。
    - `offset`: 样本数组的偏移量。
    - `count`: 样本数量。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, IEnumerable<float> samples, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和样本集合构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 样本集合。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, ArraySegment<float> segment, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和样本段构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `segment`: 样本段。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, Span<float> span, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和样本跨度构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `span`: 样本跨度。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

#### 2. Clone
- `public DiscreteSignal Clone()`
  - 创建信号的深拷贝。
  - 返回值: 信号的深拷贝。

#### 3. 索引器
- `public float this[int index] { get; set; }`
  - 样本索引器。
  - 参数:
    - `index`: 样本索引。
  - 返回值: 样本值。

- `public DiscreteSignal this[int startPos, int endPos] { get; }`
  - 创建信号的切片。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号的切片。

- `public DiscreteSignal this[Range range] { get; }`
  - 创建信号的切片。
  - 参数:
    - `range`: 样本范围。
  - 返回值: 信号的切片。

#### 4. Energy
- `public float Energy(int startPos, int endPos)`
  - 计算信号片段的能量。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号片段的能量。

- `public float Energy(Range range)`
  - 计算信号片段的能量。
  - 参数:
    - `range`: 样本范围。
  - 返回值: 信号片段的能量。

- `public float Energy()`
  - 计算整个信号的能量。
  - 返回值: 整个信号的能量。

#### 5. Rms
- `public float Rms(int startPos, int endPos)`
  - 计算信号片段的均方根值。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号片段的均方根值。

- `public float Rms(Range range)`
  - 计算信号片段的均方根值。
  - 参数:
    - `range`: 样本范围。
  - 返回值: 信号片段的均方根值。

- `public float Rms()`
  - 计算整个信号的均方根值。
  - 返回值: 整个信号的均方根值。

#### 6. ZeroCrossingRate
- `public float ZeroCrossingRate(int startPos, int endPos)`
  - 计算信号片段的过零率。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号片段的过零率。

- `public float ZeroCrossingRate(Range range)`
  - 计算信号片段的过零率。
  - 参数:
    - `range`: 样本范围。
  - 返回值: 信号片段的过零率。

- `public float ZeroCrossingRate()`
  - 计算整个信号的过零率。
  - 返回值: 整个信号的过零率。

#### 7. Entropy
- `public float Entropy(int startPos, int endPos, int binCount = 32)`
  - 计算信号片段的香农熵。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
    - `binCount`: 直方图的箱数，默认为 32。
  - 返回值: 信号片段的香农熵。

- `public float Entropy(Range range, int binCount = 32)`
  - 计算信号片段的香农熵。
  - 参数:
    - `range`: 样本范围。
    - `binCount`: 直方图的箱数，默认为 32。
  - 返回值: 信号片段的香农熵。

- `public float Entropy(int binCount = 32)`
  - 计算整个信号的香农熵。
  - 参数:
    - `binCount`: 直方图的箱数，默认为 32。
  - 返回值: 整个信号的香农熵。

### 重载运算符

#### 1. operator +
- `public static DiscreteSignal operator +(DiscreteSignal s1, DiscreteSignal s2)`
  - 通过叠加信号 s1 和 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `s1`: 第一个信号。
    - `s2`: 第二个信号。
  - 返回值: 叠加后的新信号。

- `public static DiscreteSignal operator +(DiscreteSignal s, float constant)`
  - 通过将常数添加到信号 s 创建新信号。
  - 参数:
    - `s`: 信号。
    - `constant`: 要添加到每个样本的常数。
  - 返回值: 添加常数后的新信号。

#### 2. operator -
- `public static DiscreteSignal operator -(DiscreteSignal s)`
  - 创建信号 s 的取反副本。
  - 参数:
    - `s`: 信号。
  - 返回值: 取反后的新信号。

- `public static DiscreteSignal operator -(DiscreteSignal s1, DiscreteSignal s2)`
  - 通过从信号 s1 中减去信号 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `s1`: 第一个信号。
    - `s2`: 第二个信号。
  - 返回值: 减去后的新信号。

- `public static DiscreteSignal operator -(DiscreteSignal s, float constant)`
  - 通过从信号 s 中减去常数创建新信号。
  - 参数:
    - `s`: 信号。
    - `constant`: 要从每个样本中减去的常数。
  - 返回值: 减去常数后的新信号。

#### 3. operator *
- `public static DiscreteSignal operator *(DiscreteSignal s, float coeff)`
  - 通过将信号 s 乘以系数（放大/衰减）创建新信号。
  - 参数:
    - `s`: 信号。
    - `coeff`: 放大/衰减系数。
  - 返回值: 乘以系数后的新信号。

### 代码示例
以下是一个使用 DiscreteSignal 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals;

public class DiscreteSignalExample
{
    public static void Main()
    {
        // 定义采样率
        int samplingRate = 44100;

        // 定义样本数组
        float[] samples = { 0.5f, 0.6f, 0.55f };

        // 创建 DiscreteSignal 实例
        var signal = new DiscreteSignal(samplingRate, samples);

        // 输出信号的样本
        Console.WriteLine("Samples:");
        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 获取并输出信号的能量
        var energy = signal.Energy();
        Console.WriteLine("Energy:");
        Console.WriteLine(energy);

        // 获取并输出信号的均方根值
        var rms = signal.Rms();
        Console.WriteLine("RMS:");
        Console.WriteLine(rms);

        // 获取并输出信号的过零率
        var zeroCrossingRate = signal.ZeroCrossingRate();
        Console.WriteLine("Zero Crossing Rate:");
        Console.WriteLine(zeroCrossingRate);

        // 获取并输出信号的香农熵
        var entropy = signal.Entropy();
        Console.WriteLine("Entropy:");
        Console.WriteLine(entropy);

        // 创建信号的切片
        var slicedSignal = signal[1, 3];
        Console.WriteLine("Sliced Signal Samples:");
        foreach (var sample in slicedSignal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 创建信号的深拷贝
        var copiedSignal = signal.Clone();
        Console.WriteLine("Copied Signal Samples:");
        foreach (var sample in copiedSignal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.DiscreteSignal&lt;TSample> 类

`Vorcyc.Mathematics.SignalProcessing.Signals.DiscreteSignal<TSample>` 是一个用于表示有限实数值离散时间信号的类。信号以一定的采样率存储为一个数据数组。

### 属性

#### 1. SamplingRate
- `public int SamplingRate { get; }`
  - 获取采样率（每秒的样本数）。

#### 2. SampleCount
- `public int SampleCount { get; }`
  - 获取样本量，分配后不变。

#### 3. ValidSampleCount
- `public int ValidSampleCount { get; }`
  - 获取有效样本量。

#### 4. Duration
- `public TimeSpan Duration { get; }`
  - 获取信号的持续时间。

#### 5. Samples
- `public PinnableArray<TSample> Samples { get; }`
  - 获取信号的样本数组。

#### 6. MemoryStrategy
- `public MemoryStrategy MemoryStrategy { get; }`
  - 获取信号的内存策略。

### 方法

#### 1. DiscreteSignal 构造函数
- `public DiscreteSignal(int samplingRate, TimeSpan duration, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和持续时间构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `duration`: 信号的持续时间。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, int count, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和样本数量构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `count`: 样本数量。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, TSample[] samples, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和样本数组构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 样本数组。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, TSample[] samples, int offset, int count, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率、样本数组、偏移量和样本数量构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 样本数组。
    - `offset`: 样本数组的偏移量。
    - `count`: 样本数量。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, IEnumerable<TSample> samples, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和样本集合构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `samples`: 样本集合。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, ArraySegment<TSample> segment, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和样本段构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `segment`: 样本段。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

- `public DiscreteSignal(int samplingRate, Span<TSample> span, MemoryStrategy strategy = MemoryStrategy.Immediate)`
  - 使用采样率和样本跨度构造 DiscreteSignal 实例。
  - 参数:
    - `samplingRate`: 信号的采样率。
    - `span`: 样本跨度。
    - `strategy`: 内存策略，默认为 `MemoryStrategy.Immediate`。

#### 2. Clone
- `public DiscreteSignal<TSample> Clone()`
  - 创建信号的深拷贝。
  - 返回值: 信号的深拷贝。

#### 3. 索引器
- `public TSample this[int index] { get; set; }`
  - 样本索引器。
  - 参数:
    - `index`: 样本索引。
  - 返回值: 样本值。

- `public DiscreteSignal<TSample> this[int startPos, int endPos] { get; }`
  - 创建信号的切片。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号的切片。

- `public DiscreteSignal<TSample> this[Range range] { get; }`
  - 创建信号的切片。
  - 参数:
    - `range`: 样本范围。
  - 返回值: 信号的切片。

#### 4. Energy
- `public TSample Energy(int startPos, int endPos)`
  - 计算信号片段的能量。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号片段的能量。

- `public TSample Energy(Range range)`
  - 计算信号片段的能量。
  - 参数:
    - `range`: 样本范围。
  - 返回值: 信号片段的能量。

- `public TSample Energy()`
  - 计算整个信号的能量。
  - 返回值: 整个信号的能量。

#### 5. Rms
- `public TSample Rms(int startPos, int endPos)`
  - 计算信号片段的均方根值。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号片段的均方根值。

- `public TSample Rms(Range range)`
  - 计算信号片段的均方根值。
  - 参数:
    - `range`: 样本范围。
  - 返回值: 信号片段的均方根值。

- `public TSample Rms()`
  - 计算整个信号的均方根值。
  - 返回值: 整个信号的均方根值。

#### 6. ZeroCrossingRate
- `public TSample ZeroCrossingRate(int startPos, int endPos)`
  - 计算信号片段的过零率。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
  - 返回值: 信号片段的过零率。

- `public TSample ZeroCrossingRate(Range range)`
  - 计算信号片段的过零率。
  - 参数:
    - `range`: 样本范围。
  - 返回值: 信号片段的过零率。

- `public TSample ZeroCrossingRate()`
  - 计算整个信号的过零率。
  - 返回值: 整个信号的过零率。

#### 7. Entropy
- `public TSample Entropy(int startPos, int endPos, int binCount = 32)`
  - 计算信号片段的香农熵。
  - 参数:
    - `startPos`: 第一个样本的索引（包含）。
    - `endPos`: 最后一个样本的索引（不包含）。
    - `binCount`: 直方图的箱数，默认为 32。
  - 返回值: 信号片段的香农熵。

- `public TSample Entropy(Range range, int binCount = 32)`
  - 计算信号片段的香农熵。
  - 参数:
    - `range`: 样本范围。
    - `binCount`: 直方图的箱数，默认为 32。
  - 返回值: 信号片段的香农熵。

- `public TSample Entropy(int binCount = 32)`
  - 计算整个信号的香农熵。
  - 参数:
    - `binCount`: 直方图的箱数，默认为 32。
  - 返回值: 整个信号的香农熵。

### 重载运算符

#### 1. operator +
- `public static DiscreteSignal<TSample> operator +(DiscreteSignal<TSample> s1, DiscreteSignal<TSample> s2)`
  - 通过叠加信号 s1 和 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `s1`: 第一个信号。
    - `s2`: 第二个信号。
  - 返回值: 叠加后的新信号。

- `public static DiscreteSignal<TSample> operator +(DiscreteSignal<TSample> s, TSample constant)`
  - 通过将常数添加到信号 s 创建新信号。
  - 参数:
    - `s`: 信号。
    - `constant`: 要添加到每个样本的常数。
  - 返回值: 添加常数后的新信号。

#### 2. operator -
- `public static DiscreteSignal<TSample> operator -(DiscreteSignal<TSample> s)`
  - 创建信号 s 的取反副本。
  - 参数:
    - `s`: 信号。
  - 返回值: 取反后的新信号。

- `public static DiscreteSignal<TSample> operator -(DiscreteSignal<TSample> s1, DiscreteSignal<TSample> s2)`
  - 通过从信号 s1 中减去信号 s2 创建新信号。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `s1`: 第一个信号。
    - `s2`: 第二个信号。
  - 返回值: 减去后的新信号。

- `public static DiscreteSignal<TSample> operator -(DiscreteSignal<TSample> s, TSample constant)`
  - 通过从信号 s 中减去常数创建新信号。
  - 参数:
    - `s`: 信号。
    - `constant`: 要从每个样本中减去的常数。
  - 返回值: 减去常数后的新信号。

#### 3. operator *
- `public static DiscreteSignal<TSample> operator *(DiscreteSignal<TSample> s, TSample coeff)`
  - 通过将信号 s 乘以系数（放大/衰减）创建新信号。
  - 参数:
    - `s`: 信号。
    - `coeff`: 放大/衰减系数。
  - 返回值: 乘以系数后的新信号。

### 代码示例
以下是一个使用 DiscreteSignal<TSample> 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals;

public class DiscreteSignalExample
{
    public static void Main()
    {
        // 定义采样率
        int samplingRate = 44100;

        // 定义样本数组
        float[] samples = { 0.5f, 0.6f, 0.55f };

        // 创建 DiscreteSignal 实例
        var signal = new DiscreteSignal<float>(samplingRate, samples);

        // 输出信号的样本
        Console.WriteLine("Samples:");
        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 获取并输出信号的能量
        var energy = signal.Energy();
        Console.WriteLine("Energy:");
        Console.WriteLine(energy);

        // 获取并输出信号的均方根值
        var rms = signal.Rms();
        Console.WriteLine("RMS:");
        Console.WriteLine(rms);

        // 获取并输出信号的过零率
        var zeroCrossingRate = signal.ZeroCrossingRate();
        Console.WriteLine("Zero Crossing Rate:");
        Console.WriteLine(zeroCrossingRate);

        // 获取并输出信号的香农熵
        var entropy = signal.Entropy();
        Console.WriteLine("Entropy:");
        Console.WriteLine(entropy);

        // 创建信号的切片
        var slicedSignal = signal[1, 3];
        Console.WriteLine("Sliced Signal Samples:");
        foreach (var sample in slicedSignal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 创建信号的深拷贝
        var copiedSignal = signal.Clone();
        Console.WriteLine("Copied Signal Samples:");
        foreach (var sample in copiedSignal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Signals.DiscreteSignalExtensions 类

`Vorcyc.Mathematics.SignalProcessing.Signals.DiscreteSignalExtensions` 提供了用于处理离散信号的扩展方法。

### 方法

#### 1. Delay
- `public static DiscreteSignal Delay(this DiscreteSignal signal, int delay)`
  - 创建 `signal` 的延迟副本，通过将其向右（正 `delay`）或向左（负 `delay`）移动。
  - 参数:
    - `signal`: 信号。
    - `delay`: 延迟（正数或负数的延迟样本数）。
  - 返回值: 延迟后的 `DiscreteSignal`。

#### 2. Superimpose
- `public static DiscreteSignal Superimpose(this DiscreteSignal signal1, DiscreteSignal signal2)`
  - 叠加 `signal1` 和 `signal2`。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 叠加后的 `DiscreteSignal`。

#### 3. SuperimposeMany
- `public static DiscreteSignal SuperimposeMany(this DiscreteSignal signal1, DiscreteSignal signal2, int[] positions)`
  - 在给定的 `positions` 位置处多次叠加 `signal2` 和 `signal1`。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
    - `positions`: 插入 `signal2` 的位置（索引）。
  - 返回值: 叠加后的 `DiscreteSignal`。

#### 4. Subtract
- `public static DiscreteSignal Subtract(this DiscreteSignal signal1, DiscreteSignal signal2)`
  - 从 `signal1` 中减去 `signal2`。如果大小不同，则较小的信号将广播以适应较大的信号大小。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 减去后的 `DiscreteSignal`。

#### 5. Concatenate
- `public static DiscreteSignal Concatenate(this DiscreteSignal signal1, DiscreteSignal signal2)`
  - 连接 `signal1` 和 `signal2`。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
  - 返回值: 连接后的 `DiscreteSignal`。

#### 6. Repeat
- `public static DiscreteSignal Repeat(this DiscreteSignal signal, int n)`
  - 创建 `signal` 的副本，并重复 `n` 次。
  - 参数:
    - `signal`: 信号。
    - `n`: 重复次数。
  - 返回值: 重复后的 `DiscreteSignal`。

#### 7. Amplify
- `public static void Amplify(this DiscreteSignal signal, float coeff)`
  - 按 `coeff` 放大 `signal`。
  - 参数:
    - `signal`: 信号。
    - `coeff`: 放大系数。

#### 8. Attenuate
- `public static void Attenuate(this DiscreteSignal signal, float coeff)`
  - 按 `coeff` 衰减 `signal`。
  - 参数:
    - `signal`: 信号。
    - `coeff`: 衰减系数。

#### 9. Reverse
- `public static void Reverse(this DiscreteSignal signal)`
  - 反转 `signal`。
  - 参数:
    - `signal`: 信号。

#### 10. First
- `public static DiscreteSignal First(this DiscreteSignal signal, int n)`
  - 从 `signal` 的前 `n` 个样本创建新信号。
  - 参数:
    - `signal`: 信号。
    - `n`: 样本数。
  - 返回值: 新的 `DiscreteSignal`。

#### 11. Last
- `public static DiscreteSignal Last(this DiscreteSignal signal, int n)`
  - 从 `signal` 的后 `n` 个样本创建新信号。
  - 参数:
    - `signal`: 信号。
    - `n`: 样本数。
  - 返回值: 新的 `DiscreteSignal`。

#### 12. FullRectify
- `public static void FullRectify(this DiscreteSignal signal)`
  - 全波整流 `signal`。
  - 参数:
    - `signal`: 信号。

#### 13. HalfRectify
- `public static void HalfRectify(this DiscreteSignal signal)`
  - 半波整流 `signal`。
  - 参数:
    - `signal`: 信号。

#### 14. NormalizeMax
- `public static void NormalizeMax(this DiscreteSignal signal, int bitsPerSample = 0)`
  - 按最大绝对值归一化 `signal`（范围 [-1, 1]）。
  - 参数:
    - `signal`: 信号。
    - `bitsPerSample`: 采样位深，默认为 0。

#### 15. ToComplex
- `public static ComplexDiscreteSignal ToComplex(this DiscreteSignal signal)`
  - 从 `DiscreteSignal` 创建 `ComplexDiscreteSignal`，虚部将填充为零。
  - 参数:
    - `signal`: 实值信号。
  - 返回值: 复数值信号 `ComplexDiscreteSignal`。

#### 16. FadeInFadeOut
- `public static void FadeInFadeOut(this DiscreteSignal signal, double fadeInDuration, double fadeOutDuration)`
  - 对 `signal` 进行线性淡入淡出。
  - 参数:
    - `signal`: 信号。
    - `fadeInDuration`: 淡入持续时间（秒）。
    - `fadeOutDuration`: 淡出持续时间（秒）。

- `public static void FadeInFadeOut(this DiscreteSignal signal, TimeSpan fadeInDuration, TimeSpan fadeOutDuration)`
  - 对 `signal` 进行线性淡入淡出。
  - 参数:
    - `signal`: 信号。
    - `fadeInDuration`: 淡入持续时间。
    - `fadeOutDuration`: 淡出持续时间。

#### 17. FadeIn
- `public static void FadeIn(this DiscreteSignal signal, double duration)`
  - 对 `signal` 进行线性淡入。
  - 参数:
    - `signal`: 信号。
    - `duration`: 淡入持续时间（秒）。

- `public static void FadeIn(this DiscreteSignal signal, TimeSpan duration)`
  - 对 `signal` 进行线性淡入。
  - 参数:
    - `signal`: 信号。
    - `duration`: 淡入持续时间。

#### 18. FadeOut
- `public static void FadeOut(this DiscreteSignal signal, double duration)`
  - 对 `signal` 进行线性淡出。
  - 参数:
    - `signal`: 信号。
    - `duration`: 淡出持续时间（秒）。

- `public static void FadeOut(this DiscreteSignal signal, TimeSpan duration)`
  - 对 `signal` 进行线性淡出。
  - 参数:
    - `signal`: 信号。
    - `duration`: 淡出持续时间。

#### 19. Crossfade
- `public static DiscreteSignal Crossfade(this DiscreteSignal signal1, DiscreteSignal signal2, double duration)`
  - 在 `signal1` 和 `signal2` 之间进行线性交叉淡化，并返回交叉淡化后的信号，长度等于信号长度之和减去交叉淡化部分的长度。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
    - `duration`: 交叉淡化持续时间（秒）。
  - 返回值: 交叉淡化后的 `DiscreteSignal`。

- `public static DiscreteSignal Crossfade(this DiscreteSignal signal1, DiscreteSignal signal2, TimeSpan duration)`
  - 在 `signal1` 和 `signal2` 之间进行线性交叉淡化，并返回交叉淡化后的信号，长度等于信号长度之和减去交叉淡化部分的长度。
  - 参数:
    - `signal1`: 第一个信号。
    - `signal2`: 第二个信号。
    - `duration`: 交叉淡化持续时间。
  - 返回值: 交叉淡化后的 `DiscreteSignal`。

### 代码示例
以下是一个使用 DiscreteSignalExtensions 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Signals;

public class DiscreteSignalExtensionsExample
{
    public static void Main()
    {
        // 定义采样率
        int samplingRate = 44100;

        // 定义样本数组
        float[] samples = { 0.5f, 0.6f, 0.55f };

        // 创建 DiscreteSignal 实例
        var signal = new DiscreteSignal(samplingRate, samples);

        // 创建延迟信号
        var delayedSignal = signal.Delay(2);
        Console.WriteLine("Delayed Signal Samples:");

        foreach (var sample in delayedSignal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 叠加信号
        var superimposedSignal = signal.Superimpose(delayedSignal);
        Console.WriteLine("Superimposed Signal Samples:");

        foreach (var sample in superimposedSignal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 连接信号
        var concatenatedSignal = signal.Concatenate(delayedSignal);
        Console.WriteLine("Concatenated Signal Samples:");

        foreach (var sample in concatenatedSignal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 放大信号
        signal.Amplify(2.0f);
        Console.WriteLine("Amplified Signal Samples:");

        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 衰减信号
        signal.Attenuate(2.0f);
        Console.WriteLine("Attenuated Signal Samples:");

        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 获取前 n 个样本
        var firstSamples = signal.First(2);
        Console.WriteLine("First Samples:");

        foreach (var sample in firstSamples.Samples)
        {
            Console.WriteLine(sample);
        }

        // 获取后 n 个样本
        var lastSamples = signal.Last(2);
        Console.WriteLine("Last Samples:");

        foreach (var sample in lastSamples.Samples)
        {
            Console.WriteLine(sample);
        }

        // 全波整流信号
        signal.FullRectify();
        Console.WriteLine("Full Rectified Signal Samples:");

        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 半波整流信号
        signal.HalfRectify();
        Console.WriteLine("Half Rectified Signal Samples:");

        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 归一化信号
        signal.NormalizeMax();
        Console.WriteLine("Normalized Signal Samples:");

        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 淡入淡出信号
        signal.FadeInFadeOut(0.1, 0.1);
        Console.WriteLine("Fade In Fade Out Signal Samples:");

        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 交叉淡化信号
        var crossfadedSignal = signal.Crossfade(delayedSignal, 0.1);
        Console.WriteLine("Crossfaded Signal Samples:");

        foreach (var sample in crossfadedSignal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```
