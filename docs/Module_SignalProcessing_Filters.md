当前位置 : [根目录](README.md)/[信号处理模块](Module_SignalProcessing.md)/[一维滤波器](Module_SignalProcessing_Filters.md)

# 信号处理模块 - Signal Processing Module
## 一维滤波器 - Filters

> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Filters

---

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.LmfFilter 类](#vorcycmathematicssignalprocessingfiltersadaptivelmffilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.LmsFilter 类](#vorcycmathematicssignalprocessingfiltersadaptivelmsfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.NlmfFilter 类](#vorcycmathematicssignalprocessingfiltersadaptivenlmffilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.NlmsFilter 类](#vorcycmathematicssignalprocessingfiltersadaptivenlmsfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.RlsFilter 类](#vorcycmathematicssignalprocessingfiltersadaptiverlsfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.SignLmsFilter 类](#vorcycmathematicssignalprocessingfiltersadaptivesignlmsfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.VariableStepLmsFilter 类](#vorcycmathematicssignalprocessingfiltersadaptivevariablesteplmsfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Base.FilterChain 类](#vorcycmathematicssignalprocessingfiltersbasefilterchain-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Base.FirFilter 类](#vorcycmathematicssignalprocessingfiltersbasefirfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Base.IirFilter 类](#vorcycmathematicssignalprocessingfiltersbaseiirfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Base.StereoFilter 类](#vorcycmathematicssignalprocessingfiltersbasestereofilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Base.TransferFunction 类](#vorcycmathematicssignalprocessingfiltersbasetransferfunction-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Base.ZiFilter 类](#vorcycmathematicssignalprocessingfiltersbasezifilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.BandPassFilter 类](#vorcycmathematicssignalprocessingfiltersbesselbandpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.BandStopFilter 类](#vorcycmathematicssignalprocessingfiltersbesselbandstopfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.HighPassFilter 类](#vorcycmathematicssignalprocessingfiltersbesselhighpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.LowPassFilter 类](#vorcycmathematicssignalprocessingfiltersbessellowpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.PrototypeBessel 类](#vorcycmathematicssignalprocessingfiltersbesselprototypebessel-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.AllPassFilter 类](#vorcycmathematicssignalprocessingfiltersbiquadallpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.BandPassFilter 类](#vorcycmathematicssignalprocessingfiltersbiquadbandpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.BiQuadFilter 类](#vorcycmathematicssignalprocessingfiltersbiquadbiquadfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.HighPassFilter 类](#vorcycmathematicssignalprocessingfiltersbiquadhighpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.HighShelfFilter 类](#vorcycmathematicssignalprocessingfiltersbiquadhighshelffilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.LowPassFilter 类](#vorcycmathematicssignalprocessingfiltersbiquadlowpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.LowShelfFilter 类](#vorcycmathematicssignalprocessingfiltersbiquadlowshelffilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.NotchFilter 类](#vorcycmathematicssignalprocessingfiltersbiquadnotchfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.PeakFilter 类](#vorcycmathematicssignalprocessingfiltersbiquadpeakfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.BandPassFilter 类](#vorcycmathematicssignalprocessingfiltersbutterworthbandpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.BandStopFilter 类](#vorcycmathematicssignalprocessingfiltersbutterworthbandstopfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.HighPassFilter 类](#vorcycmathematicssignalprocessingfiltersbutterworthhighpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.LowPassFilter 类](#vorcycmathematicssignalprocessingfiltersbutterworthlowpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.PrototypeButterworth 类](#vorcycmathematicssignalprocessingfiltersbutterworthprototypebutterworth-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.BandPassFilter 类](#vorcycmathematicssignalprocessingfilterschebyshevi-bandpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.BandStopFilter 类](#vorcycmathematicssignalprocessingfilterschebyshevi-bandstopfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.HighPassFilter 类](#vorcycmathematicssignalprocessingfilterschebyshevi-highpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.LowPassFilter 类](#vorcycmathematicssignalprocessingfilterschebyshevi-lowpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.PrototypeChebyshevI 类](#vorcycmathematicssignalprocessingfilterschebyshevi-prototypechebyshevi-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.BandPassFilter 类](#vorcycmathematicssignalprocessingfilterschebyshevii-bandpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.BandStopFilter 类](#vorcycmathematicssignalprocessingfilterschebyshevii-bandstopfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.HighPassFilter 类](#vorcycmathematicssignalprocessingfilterschebyshevii-highpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.LowPassFilter 类](#vorcycmathematicssignalprocessingfilterschebyshevii-lowpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.PrototypeChebyshevII 类](#vorcycmathematicssignalprocessingfilterschebyshevii-prototypechebyshevii-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.BandPassFilter 类](#vorcycmathematicssignalprocessingfilterstellipticbandpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.BandStopFilter 类](#vorcycmathematicssignalprocessingfilterstellipticbandstopfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.HighPassFilter 类](#vorcycmathematicssignalprocessingfilterstelliptichighpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.LowPassFilter 类](#vorcycmathematicssignalprocessingfilterstellipticlowpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.PrototypeElliptic 类](#vorcycmathematicssignalprocessingfilterstellipticprototypeelliptic-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Fda.DesignFilter 类](#vorcycmathematicssignalprocessingfiltersfdadesignfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Fda.FilterBanks 类](#vorcycmathematicssignalprocessingfiltersfdafilterbanks-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Fda.Remez 类](#vorcycmathematicssignalprocessingfiltersfdaremez-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Fda.VtlnWarper 类](#vorcycmathematicssignalprocessingfiltersfdavtlnwarper-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.HighPassFilter 类](#vorcycmathematicssignalprocessingfiltersonepolehighpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.LowPassFilter 类](#vorcycmathematicssignalprocessingfiltersonepolelowpassfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.OnePoleFilter 类](#vorcycmathematicssignalprocessingfiltersonepoleonepolefilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.Polyphase.PolyphaseSystem 类](#vorcycmathematicssignalprocessingfilterspolyphasepolyphasesystem-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.CombFeedbackFilter 类](#vorcycmathematicssignalprocessingfilterscombfeedbackfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.CombFeedforwardFilter 类](#vorcycmathematicssignalprocessingfilterscombfeedforwardfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.DcRemovalFilter 类](#vorcycmathematicssignalprocessingfiltersdcremovalfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.DeEmphasisFilter 类](#vorcycmathematicssignalprocessingfiltersdeemphasisfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.HilbertFilter 类](#vorcycmathematicssignalprocessingfiltershilbertfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.MedianFilter 类](#vorcycmathematicssignalprocessingfiltersmedianfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.MedianFilter2 类](#vorcycmathematicssignalprocessingfiltersmedianfilter2-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.MovingAverageFilter 类](#vorcycmathematicssignalprocessingfiltersmovingaveragefilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.MovingAverageRecursiveFilter 类](#vorcycmathematicssignalprocessingfiltersmovingaveragerecursivefilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.PreEmphasisFilter 类](#vorcycmathematicssignalprocessingfilterspreemphasisfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.RastaFilter 类](#vorcycmathematicssignalprocessingfiltersrastafilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.SavitzkyGolayFilter 类](#vorcycmathematicssignalprocessingfilterssavitzkygolayfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.ThiranFilter 类](#vorcycmathematicssignalprocessingfiltersthiranfilter-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Filters.WienerFilter 类](#vorcycmathematicssignalprocessingfilterswienerfilter-类)  

---


> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.LmfFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.LmfFilter 是一个用于实现最小均方四次（Least-Mean-Fourth）自适应滤波算法的类。

### 属性

#### 1. Mu
- `private readonly float _mu`
  - 获取或设置滤波器的步长因子。

#### 2. Leakage
- `private readonly float _leakage`
  - 获取或设置滤波器的泄漏因子。

### 方法

#### 1. 构造函数
- `public LmfFilter(int order, float mu = 0.75f, float leakage = 0)`
  - 构造一个给定阶数的 LmfFilter 实例。
  - 参数:
    - `order`: 滤波器阶数。
    - `mu`: 步长因子，默认值为 0.75。
    - `leakage`: 泄漏因子，默认值为 0。

#### 2. Process
- `public override float Process(float input, float desired)`
  - 处理输入和期望信号的一个样本，并自适应调整滤波器系数。
  - 参数:
    - `input`: 输入信号的样本。
    - `desired`: 期望信号的样本。
  - 返回值: 滤波器输出的样本。

### 代码示例
以下是一个使用 LmfFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

public class LmfFilterExample
{
    public static void Main()
    {
        // 定义滤波器阶数、步长因子和泄漏因子
        int order = 4;
        float mu = 0.75f;
        float leakage = 0.01f;

        // 创建 LmfFilter 实例
        LmfFilter filter = new LmfFilter(order, mu, leakage);

        // 定义输入信号和期望信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
        float[] desiredSignal = { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f };

        // 处理信号并输出结果
        for (int i = 0; i < inputSignal.Length; i++)
        {
            float output = filter.Process(inputSignal[i], desiredSignal[i]);
            Console.WriteLine($"Input: {inputSignal[i]}, Desired: {desiredSignal[i]}, Output: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.LmsFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.LmsFilter 是一个用于实现最小均方（Least-Mean-Squares）自适应滤波算法的类。

### 属性

#### 1. Mu
- `private readonly float _mu`
  - 获取或设置滤波器的步长因子。

#### 2. Leakage
- `private readonly float _leakage`
  - 获取或设置滤波器的泄漏因子。

### 方法

#### 1. 构造函数
- `public LmsFilter(int order, float mu = 0.75f, float leakage = 0)`
  - 构造一个给定阶数的 LmsFilter 实例。
  - 参数:
    - `order`: 滤波器阶数。
    - `mu`: 步长因子，默认值为 0.75。
    - `leakage`: 泄漏因子，默认值为 0。

#### 2. Process
- `public override float Process(float input, float desired)`
  - 处理输入和期望信号的一个样本，并自适应调整滤波器系数。
  - 参数:
    - `input`: 输入信号的样本。
    - `desired`: 期望信号的样本。
  - 返回值: 滤波器输出的样本。

### 代码示例
以下是一个使用 LmsFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

public class LmsFilterExample
{
    public static void Main()
    {
        // 定义滤波器阶数、步长因子和泄漏因子
        int order = 4;
        float mu = 0.75f;
        float leakage = 0.01f;

        // 创建 LmsFilter 实例
        LmsFilter filter = new LmsFilter(order, mu, leakage);

        // 定义输入信号和期望信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
        float[] desiredSignal = { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f };

        // 处理信号并输出结果
        for (int i = 0; i < inputSignal.Length; i++)
        {
            float output = filter.Process(inputSignal[i], desiredSignal[i]);
            Console.WriteLine($"Input: {inputSignal[i]}, Desired: {desiredSignal[i]}, Output: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.NlmfFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.NlmfFilter 是一个用于实现归一化最小均方四次（Normalized Least-Mean-Fourth）自适应滤波算法的类。

### 属性

#### 1. Mu
- `private readonly float _mu`
  - 获取或设置滤波器的步长因子。

#### 2. Eps
- `private readonly float _eps`
  - 获取或设置滤波器的归一化因子。

#### 3. Leakage
- `private readonly float _leakage`
  - 获取或设置滤波器的泄漏因子。

### 方法

#### 1. 构造函数
- `public NlmfFilter(int order, float mu = 0.75f, float eps = 1, float leakage = 0)`
  - 构造一个给定阶数的 NlmfFilter 实例。
  - 参数:
    - `order`: 滤波器阶数。
    - `mu`: 步长因子，默认值为 0.75。
    - `eps`: 归一化因子，默认值为 1。
    - `leakage`: 泄漏因子，默认值为 0。

#### 2. Process
- `public override float Process(float input, float desired)`
  - 处理输入和期望信号的一个样本，并自适应调整滤波器系数。
  - 参数:
    - `input`: 输入信号的样本。
    - `desired`: 期望信号的样本。
  - 返回值: 滤波器输出的样本。

### 代码示例
以下是一个使用 NlmfFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

public class NlmfFilterExample
{
    public static void Main()
    {
        // 定义滤波器阶数、步长因子、归一化因子和泄漏因子
        int order = 4;
        float mu = 0.75f;
        float eps = 1.0f;
        float leakage = 0.01f;

        // 创建 NlmfFilter 实例
        NlmfFilter filter = new NlmfFilter(order, mu, eps, leakage);

        // 定义输入信号和期望信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
        float[] desiredSignal = { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f };

        // 处理信号并输出结果
        for (int i = 0; i < inputSignal.Length; i++)
        {
            float output = filter.Process(inputSignal[i], desiredSignal[i]);
            Console.WriteLine($"Input: {inputSignal[i]}, Desired: {desiredSignal[i]}, Output: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.NlmsFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.NlmsFilter 是一个用于实现归一化最小均方（Normalized Least-Mean-Squares）自适应滤波算法的类。

### 属性

#### 1. Mu
- `private readonly float _mu`
  - 获取或设置滤波器的步长因子。

#### 2. Eps
- `private readonly float _eps`
  - 获取或设置滤波器的归一化因子。

#### 3. Leakage
- `private readonly float _leakage`
  - 获取或设置滤波器的泄漏因子。

### 方法

#### 1. 构造函数
- `public NlmsFilter(int order, float mu = 0.75f, float eps = 1, float leakage = 0)`
  - 构造一个给定阶数的 NlmsFilter 实例。
  - 参数:
    - `order`: 滤波器阶数。
    - `mu`: 步长因子，默认值为 0.75。
    - `eps`: 归一化因子，默认值为 1。
    - `leakage`: 泄漏因子，默认值为 0。

#### 2. Process
- `public override float Process(float input, float desired)`
  - 处理输入和期望信号的一个样本，并自适应调整滤波器系数。
  - 参数:
    - `input`: 输入信号的样本。
    - `desired`: 期望信号的样本。
  - 返回值: 滤波器输出的样本。

### 代码示例
以下是一个使用 NlmsFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

public class NlmsFilterExample
{
    public static void Main()
    {
        // 定义滤波器阶数、步长因子、归一化因子和泄漏因子
        int order = 4;
        float mu = 0.75f;
        float eps = 1.0f;
        float leakage = 0.01f;

        // 创建 NlmsFilter 实例
        NlmsFilter filter = new NlmsFilter(order, mu, eps, leakage);

        // 定义输入信号和期望信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
        float[] desiredSignal = { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f };

        // 处理信号并输出结果
        for (int i = 0; i < inputSignal.Length; i++)
        {
            float output = filter.Process(inputSignal[i], desiredSignal[i]);
            Console.WriteLine($"Input: {inputSignal[i]}, Desired: {desiredSignal[i]}, Output: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.RlsFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.RlsFilter 是一个用于实现递归最小二乘（Recursive-Least-Squares）自适应滤波算法的类。

### 属性

#### 1. Lambda
- `private readonly float _lambda`
  - 获取或设置滤波器的遗忘因子。

#### 2. InverseCorrMatrix
- `private readonly float[,] _p`
  - 获取或设置滤波器的逆相关矩阵。

#### 3. Gains
- `private readonly float[] _gains`
  - 获取或设置滤波器的增益系数矩阵。

#### 4. TemporaryMatrices
- `private readonly float[,] _dp, _tmp`
  - 获取或设置滤波器计算的临时矩阵。

### 方法

#### 1. 构造函数
- `public RlsFilter(int order, float lambda = 0.99f, float initCorrMatrix = 1e2f)`
  - 构造一个给定阶数的 RlsFilter 实例。
  - 参数:
    - `order`: 滤波器阶数。
    - `lambda`: 遗忘因子，默认值为 0.99。
    - `initCorrMatrix`: 初始化逆相关矩阵的值，默认值为 1e2f。

#### 2. Process
- `public override float Process(float input, float desired)`
  - 处理输入和期望信号的一个样本，并自适应调整滤波器系数。
  - 参数:
    - `input`: 输入信号的样本。
    - `desired`: 期望信号的样本。
  - 返回值: 滤波器输出的样本。

### 代码示例
以下是一个使用 RlsFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

public class RlsFilterExample
{
    public static void Main()
    {
        // 定义滤波器阶数、遗忘因子和初始化逆相关矩阵的值
        int order = 4;
        float lambda = 0.99f;
        float initCorrMatrix = 1e2f;

        // 创建 RlsFilter 实例
        RlsFilter filter = new RlsFilter(order, lambda, initCorrMatrix);

        // 定义输入信号和期望信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
        float[] desiredSignal = { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f };

        // 处理信号并输出结果
        for (int i = 0; i < inputSignal.Length; i++)
        {
            float output = filter.Process(inputSignal[i], desiredSignal[i]);
            Console.WriteLine($"Input: {inputSignal[i]}, Desired: {desiredSignal[i]}, Output: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.SignLmsFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.SignLmsFilter 是一个用于实现符号最小均方（Sign Least-Mean-Squares）自适应滤波算法的类。

### 属性

#### 1. Mu
- `private readonly float _mu`
  - 获取或设置滤波器的步长因子。

#### 2. Leakage
- `private readonly float _leakage`
  - 获取或设置滤波器的泄漏因子。

### 方法

#### 1. 构造函数
- `public SignLmsFilter(int order, float mu = 0.75f, float leakage = 0)`
  - 构造一个给定阶数的 SignLmsFilter 实例。
  - 参数:
    - `order`: 滤波器阶数。
    - `mu`: 步长因子，默认值为 0.75。
    - `leakage`: 泄漏因子，默认值为 0。

#### 2. Process
- `public override float Process(float input, float desired)`
  - 处理输入和期望信号的一个样本，并自适应调整滤波器系数。
  - 参数:
    - `input`: 输入信号的样本。
    - `desired`: 期望信号的样本。
  - 返回值: 滤波器输出的样本。

### 代码示例
以下是一个使用 SignLmsFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

public class SignLmsFilterExample
{
    public static void Main()
    {
        // 定义滤波器阶数、步长因子和泄漏因子
        int order = 4;
        float mu = 0.75f;
        float leakage = 0.01f;

        // 创建 SignLmsFilter 实例
        SignLmsFilter filter = new SignLmsFilter(order, mu, leakage);

        // 定义输入信号和期望信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
        float[] desiredSignal = { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f };

        // 处理信号并输出结果
        for (int i = 0; i < inputSignal.Length; i++)
        {
            float output = filter.Process(inputSignal[i], desiredSignal[i]);
            Console.WriteLine($"Input: {inputSignal[i]}, Desired: {desiredSignal[i]}, Output: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.VariableStepLmsFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive.VariableStepLmsFilter 是一个用于实现具有可变步长的最小均方（Least-Mean-Squares）自适应滤波算法的类。

### 属性

#### 1. Mu
- `private readonly float[] _mu`
  - 获取或设置滤波器的步长因子数组。

#### 2. Leakage
- `private readonly float _leakage`
  - 获取或设置滤波器的泄漏因子。

### 方法

#### 1. 构造函数
- `public VariableStepLmsFilter(int order, float[] mu = null, float leakage = 0)`
  - 构造一个给定阶数的 VariableStepLmsFilter 实例。
  - 参数:
    - `order`: 滤波器阶数。
    - `mu`: 步长因子数组，默认为每个阶数 0.75。
    - `leakage`: 泄漏因子，默认值为 0。

#### 2. Process
- `public override float Process(float input, float desired)`
  - 处理输入和期望信号的一个样本，并自适应调整滤波器系数。
  - 参数:
    - `input`: 输入信号的样本。
    - `desired`: 期望信号的样本。
  - 返回值: 滤波器输出的样本。

### 代码示例
以下是一个使用 VariableStepLmsFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

public class VariableStepLmsFilterExample
{
    public static void Main()
    {
        // 定义滤波器阶数、步长因子数组和泄漏因子
        int order = 4;
        float[] mu = { 0.75f, 0.8f, 0.85f, 0.9f };
        float leakage = 0.01f;

        // 创建 VariableStepLmsFilter 实例
        VariableStepLmsFilter filter = new VariableStepLmsFilter(order, mu, leakage);

        // 定义输入信号和期望信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
        float[] desiredSignal = { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f };

        // 处理信号并输出结果
        for (int i = 0; i < inputSignal.Length; i++)
        {
            float output = filter.Process(inputSignal[i], desiredSignal[i]);
            Console.WriteLine($"Input: {inputSignal[i]}, Desired: {desiredSignal[i]}, Output: {output}");
        }
    }
}
```

---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.Base 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Filters.Base.FilterChain 类

Vorcyc.Mathematics.SignalProcessing.Filters.Base.FilterChain 是一个用于实现顺序连接滤波器链的类。

### 属性

#### 1. Filters
- `private readonly List<IOnlineFilter> _filters`
  - 获取或设置滤波器链中的滤波器列表。

### 方法

#### 1. 构造函数
- `public FilterChain(IEnumerable<IOnlineFilter> filters = null)`
  - 从在线滤波器集合构造 FilterChain 实例。
  - 参数:
    - `filters`: 在线滤波器集合，默认为空。

- `public FilterChain(IEnumerable<TransferFunction> tfs)`
  - 从传递函数集合（例如，SOS 段）构造 FilterChain 实例。此构造函数在内部创建 IirFilter 对象。
  - 参数:
    - `tfs`: 传递函数集合。

#### 2. Add
- `public void Add(IOnlineFilter filter)`
  - 将滤波器添加到链中。
  - 参数:
    - `filter`: 在线滤波器。

#### 3. Insert
- `public void Insert(int index, IOnlineFilter filter)`
  - 在链中指定索引处插入滤波器。
  - 参数:
    - `index`: 滤波器在链中的索引。
    - `filter`: 在线滤波器。

#### 4. RemoveAt
- `public void RemoveAt(int index)`
  - 从链中移除指定索引处的滤波器。
  - 参数:
    - `index`: 滤波器在链中的索引。

#### 5. Process
- `public float Process(float sample)`
  - 通过滤波器链处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 6. Reset
- `public void Reset()`
  - 重置链中的所有滤波器。

#### 7. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

### 代码示例
以下是一个使用 FilterChain 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

public class FilterChainExample
{
    public static void Main()
    {
        // 创建一些示例滤波器
        IOnlineFilter filter1 = new LowPassFilter(0.5f);
        IOnlineFilter filter2 = new HighPassFilter(0.2f);

        // 创建 FilterChain 实例并添加滤波器
        FilterChain filterChain = new FilterChain();
        filterChain.Add(filter1);
        filterChain.Add(filter2);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filterChain.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 重置滤波器链
        filterChain.Reset();
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Base.FirFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Base.FirFilter 是一个用于实现有限脉冲响应（Finite Impulse Response, FIR）滤波器的类。

### 属性

#### 1. Kernel
- `public float[] Kernel`
  - 获取滤波器核（脉冲响应）的副本。

#### 2. Tf
- `public override TransferFunction Tf { get; protected set; }`
  - 获取或设置滤波器的传递函数。

#### 3. KernelSizeForBlockConvolution
- `public int KernelSizeForBlockConvolution { get; set; } = 64`
  - 获取或设置在自动模式下切换到重叠保存算法的最小核长度。

### 方法

#### 1. 构造函数
- `public FirFilter(IEnumerable<float> kernel)`
  - 从给定的核构造 FirFilter 实例。
  - 参数:
    - `kernel`: FIR 滤波器核。

- `public FirFilter(IEnumerable<double> kernel)`
  - 从 64 位核构造 FirFilter 实例。
  - 参数:
    - `kernel`: FIR 滤波器核。

- `public FirFilter(TransferFunction tf)`
  - 从传递函数构造 FirFilter 实例。
  - 参数:
    - `tf`: 传递函数。

#### 2. ApplyTo
- `public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 3. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. ProcessAllSamples
- `public float[] ProcessAllSamples(float[] samples)`
  - 处理所有样本。
  - 参数:
    - `samples`: 样本数组。
  - 返回值: 处理后的样本数组。

- `public float[] ProcessAllSamples(Span<float> samples)`
  - 处理所有样本。
  - 参数:
    - `samples`: 样本数组。
  - 返回值: 处理后的样本数组。

#### 5. ApplyFilterDirectly
- `protected DiscreteSignal ApplyFilterDirectly(DiscreteSignal signal)`
  - 直接应用滤波器。
  - 参数:
    - `signal`: 输入信号。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 6. ChangeKernel
- `public void ChangeKernel(float[] kernel)`
  - 更改滤波器核。
  - 参数:
    - `kernel`: 新的核。

#### 7. Reset
- `public override void Reset()`
  - 重置滤波器。

#### 8. 运算符重载
- `public static FirFilter operator *(FirFilter filter1, FirFilter filter2)`
  - 创建两个 FIR 滤波器的串联连接。
  - 参数:
    - `filter1`: 第一个 FIR 滤波器。
    - `filter2`: 第二个 FIR 滤波器。
  - 返回值: 串联连接的 FIR 滤波器。

- `public static IirFilter operator *(FirFilter filter1, IirFilter filter2)`
  - 创建 FIR 滤波器和 IIR 滤波器的串联连接。
  - 参数:
    - `filter1`: FIR 滤波器。
    - `filter2`: IIR 滤波器。
  - 返回值: 串联连接的 IIR 滤波器。

- `public static FirFilter operator +(FirFilter filter1, FirFilter filter2)`
  - 创建两个 FIR 滤波器的并联连接。
  - 参数:
    - `filter1`: 第一个 FIR 滤波器。
    - `filter2`: 第二个 FIR 滤波器。
  - 返回值: 并联连接的 FIR 滤波器。

- `public static IirFilter operator +(FirFilter filter1, IirFilter filter2)`
  - 创建 FIR 滤波器和 IIR 滤波器的并联连接。
  - 参数:
    - `filter1`: FIR 滤波器。
    - `filter2`: IIR 滤波器。
  - 返回值: 并联连接的 IIR 滤波器。

### 代码示例
以下是一个使用 FirFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

public class FirFilterExample
{
    public static void Main()
    {
        // 定义滤波器核
        float[] kernel = { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };

        // 创建 FirFilter 实例
        FirFilter filter = new FirFilter(kernel);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 重置滤波器
        filter.Reset();
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Base.IirFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Base.IirFilter 是一个用于实现无限脉冲响应（Infinite Impulse Response, IIR）滤波器的类。

### 属性

#### 1. Tf
- `public override TransferFunction Tf { get; protected set; }`
  - 获取或设置滤波器的传递函数。

#### 2. DefaultImpulseResponseLength
- `public int DefaultImpulseResponseLength { get; set; } = 512`
  - 获取或设置截断脉冲响应的默认长度。

### 方法

#### 1. 构造函数
- `public IirFilter(IEnumerable<float> b, IEnumerable<float> a)`
  - 从分子和分母系数构造 IirFilter 实例。
  - 参数:
    - `b`: 传递函数的分子系数。
    - `a`: 传递函数的分母系数。

- `public IirFilter(IEnumerable<double> b, IEnumerable<double> a)`
  - 从 64 位分子和分母系数构造 IirFilter 实例。
  - 参数:
    - `b`: 传递函数的分子系数。
    - `a`: 传递函数的分母系数。

- `public IirFilter(TransferFunction tf)`
  - 从传递函数构造 IirFilter 实例。
  - 参数:
    - `tf`: 传递函数。

#### 2. ApplyTo
- `public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 3. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. ApplyFilterDirectly
- `protected DiscreteSignal ApplyFilterDirectly(DiscreteSignal signal)`
  - 直接应用滤波器。
  - 参数:
    - `signal`: 输入信号。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 5. ChangeNumeratorCoeffs
- `public void ChangeNumeratorCoeffs(float[] b)`
  - 更改滤波器的分子系数。
  - 参数:
    - `b`: 新的分子系数。

#### 6. ChangeDenominatorCoeffs
- `public void ChangeDenominatorCoeffs(float[] a)`
  - 更改滤波器的分母系数。
  - 参数:
    - `a`: 新的分母系数。

#### 7. Change
- `public void Change(TransferFunction tf)`
  - 更改滤波器的传递函数。
  - 参数:
    - `tf`: 新的传递函数。

#### 8. Reset
- `public override void Reset()`
  - 重置滤波器。

#### 9. Normalize
- `public void Normalize()`
  - 归一化传递函数（将所有滤波器系数除以传递函数分母的第一个系数）。

#### 10. 运算符重载
- `public static IirFilter operator *(IirFilter filter1, LtiFilter filter2)`
  - 创建 IIR 滤波器和任意 LTI 滤波器的串联连接。
  - 参数:
    - `filter1`: IIR 滤波器。
    - `filter2`: LTI 滤波器。
  - 返回值: 串联连接的 IIR 滤波器。

- `public static IirFilter operator +(IirFilter filter1, LtiFilter filter2)`
  - 创建 IIR 滤波器和任意 LTI 滤波器的并联连接。
  - 参数:
    - `filter1`: IIR 滤波器。
    - `filter2`: LTI 滤波器。
  - 返回值: 并联连接的 IIR 滤波器。

### 代码示例
以下是一个使用 IirFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

public class IirFilterExample
{
    public static void Main()
    {
        // 定义滤波器的分子和分母系数
        float[] b = { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };
        float[] a = { 1.0f, -0.5f, 0.25f, -0.125f, 0.0625f };

        // 创建 IirFilter 实例
        IirFilter filter = new IirFilter(b, a);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 重置滤波器
        filter.Reset();
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Base.StereoFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Base.StereoFilter 是一个用于处理交错立体声缓冲区数据的滤波器类。StereoFilter 包含两个独立的滤波器：一个用于左声道信号，另一个用于右声道信号。

### 属性

#### 1. FilterLeft
- `private readonly IOnlineFilter _filterLeft`
  - 获取或设置左声道信号的滤波器。

#### 2. FilterRight
- `private readonly IOnlineFilter _filterRight`
  - 获取或设置右声道信号的滤波器。

#### 3. IsRight
- `private bool _isRight`
  - 内部标志，用于在左声道和右声道之间切换。

### 方法

#### 1. 构造函数
- `public StereoFilter(IOnlineFilter filterLeft, IOnlineFilter filterRight)`
  - 从两个独立的滤波器构造 StereoFilter 实例。
  - 参数:
    - `filterLeft`: 左声道信号的滤波器。
    - `filterRight`: 右声道信号的滤波器。

#### 2. Process
- `public float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public void Reset()`
  - 重置滤波器。

#### 4. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

### 代码示例
以下是一个使用 StereoFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

public class StereoFilterExample
{
    public static void Main()
    {
        // 创建一些示例滤波器
        IOnlineFilter filterLeft = new LowPassFilter(0.5f);
        IOnlineFilter filterRight = new HighPassFilter(0.2f);

        // 创建 StereoFilter 实例
        StereoFilter stereoFilter = new StereoFilter(filterLeft, filterRight);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = stereoFilter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 重置滤波器
        stereoFilter.Reset();
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Base.TransferFunction 类

Vorcyc.Mathematics.SignalProcessing.Filters.Base.TransferFunction 是一个用于表示线性时不变（LTI）滤波器传递函数的类。

### 属性

#### 1. Numerator
- `public float[] Numerator { get; protected set; }`
  - 获取传递函数的分子。

#### 2. Denominator
- `public float[] Denominator { get; protected set; }`
  - 获取传递函数的分母。

#### 3. CalculateZpIterations
- `public int CalculateZpIterations { get; set; } = VMath.PolyRootsIterations`
  - 获取或设置计算零点/极点（多项式根）的最大迭代次数。默认值为 25000。

#### 4. Zeros
- `public ComplexFp32[] Zeros => _zeros ?? TfToZp(Numerator, CalculateZpIterations)`
  - 获取零点（'z' 在 'zpk' 表示法中）。

#### 5. Poles
- `public ComplexFp32[] Poles => _poles ?? TfToZp(Denominator, CalculateZpIterations)`
  - 获取极点（'p' 在 'zpk' 表示法中）。

#### 6. Gain
- `public float Gain => Numerator[0]`
  - 获取增益（'k' 在 'zpk' 表示法中）。

#### 7. StateSpace
- `public StateSpace StateSpace { get; }`
  - 获取状态空间表示。

#### 8. Zi
- `public float[] Zi { get; }`
  - 获取与阶跃响应的稳态对应的初始状态。

### 方法

#### 1. 构造函数
- `public TransferFunction(float[] numerator, float[] denominator = null)`
  - 从分子和分母构造 TransferFunction 实例。
  - 参数:
    - `numerator`: 传递函数的分子。
    - `denominator`: 传递函数的分母，默认为 `[1.0f]`。

- `public TransferFunction(ComplexFp32[] zeros, ComplexFp32[] poles, float gain = 1)`
  - 从零点、极点和增益构造 TransferFunction 实例。
  - 参数:
    - `zeros`: 零点。
    - `poles`: 极点。
    - `gain`: 增益，默认为 1。

- `public TransferFunction(ComplexDiscreteSignal zeros, ComplexDiscreteSignal poles, float gain = 1)`
  - 从零点、极点和增益构造 TransferFunction 实例。
  - 参数:
    - `zeros`: 零点。
    - `poles`: 极点。
    - `gain`: 增益，默认为 1。

- `public TransferFunction(StateSpace stateSpace)`
  - 从状态空间表示构造 TransferFunction 实例。
  - 参数:
    - `stateSpace`: 状态空间表示。

#### 2. ImpulseResponse
- `public float[] ImpulseResponse(int length = 512)`
  - 计算给定长度的脉冲响应。
  - 参数:
    - `length`: 脉冲响应的长度。
  - 返回值: 脉冲响应数组。

#### 3. FrequencyResponse
- `public ComplexDiscreteSignal FrequencyResponse(int length = 512)`
  - 计算给定长度的频率响应。
  - 参数:
    - `length`: 频率响应的长度。
  - 返回值: 频率响应的 `ComplexDiscreteSignal` 对象。

#### 4. GroupDelay
- `public float[] GroupDelay(int length = 512)`
  - 计算给定长度的群延迟。
  - 参数:
    - `length`: 群延迟数组的长度。
  - 返回值: 群延迟数组。

#### 5. PhaseDelay
- `public float[] PhaseDelay(int length = 512)`
  - 计算给定长度的相位延迟。
  - 参数:
    - `length`: 相位延迟数组的长度。
  - 返回值: 相位延迟数组。

#### 6. NormalizeAt
- `public void NormalizeAt(float freq)`
  - 在给定频率处归一化频率响应（将分子归一化以将频率响应映射到 [0, 1]）。
  - 参数:
    - `freq`: 频率。

#### 7. Normalize
- `public void Normalize()`
  - 归一化分子和分母（将它们除以分母的第一个系数）。

#### 8. 运算符重载
- `public static TransferFunction operator *(TransferFunction tf1, TransferFunction tf2)`
  - 创建两个传递函数的串联连接。
  - 参数:
    - `tf1`: 第一个传递函数。
    - `tf2`: 第二个传递函数。
  - 返回值: 串联连接的传递函数。

- `public static TransferFunction operator +(TransferFunction tf1, TransferFunction tf2)`
  - 创建两个传递函数的并联连接。
  - 参数:
    - `tf1`: 第一个传递函数。
    - `tf2`: 第二个传递函数。
  - 返回值: 并联连接的传递函数。

#### 9. FromCsv
- `public static TransferFunction FromCsv(Stream stream, char delimiter = ',')`
  - 从 CSV 流加载传递函数的分子和分母。
  - 参数:
    - `stream`: 输入流。
    - `delimiter`: 分隔符，默认为 `,`。
  - 返回值: `TransferFunction` 实例。

#### 10. ToCsv
- `public void ToCsv(Stream stream, char delimiter = ',')`
  - 将传递函数的分子和分母序列化到 CSV 流。
  - 参数:
    - `stream`: 输出流。
    - `delimiter`: 分隔符，默认为 `,`。

### 代码示例
以下是一个使用 TransferFunction 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

public class TransferFunctionExample
{
    public static void Main()
    {
        // 定义传递函数的分子和分母
        float[] numerator = { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };
        float[] denominator = { 1.0f, -0.5f, 0.25f, -0.125f, 0.0625f };

        // 创建 TransferFunction 实例
        TransferFunction tf = new TransferFunction(numerator, denominator);

        // 计算脉冲响应
        float[] impulseResponse = tf.ImpulseResponse();
        Console.WriteLine("Impulse Response:");
        foreach (var value in impulseResponse)
        {
            Console.WriteLine(value);
        }

        // 计算频率响应
        var frequencyResponse = tf.FrequencyResponse();
        Console.WriteLine("Frequency Response:");
        foreach (var value in frequencyResponse.Real)
        {
            Console.WriteLine(value);
        }

        // 计算群延迟
        float[] groupDelay = tf.GroupDelay();
        Console.WriteLine("Group Delay:");
        foreach (var value in groupDelay)
        {
            Console.WriteLine(value);
        }

        // 计算相位延迟
        float[] phaseDelay = tf.PhaseDelay();
        Console.WriteLine("Phase Delay:");
        foreach (var value in phaseDelay)
        {
            Console.WriteLine(value);
        }

        // 归一化传递函数
        tf.Normalize();
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Base.ZiFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Base.ZiFilter 是一个基于状态向量实现的 LTI 滤波器的特殊实现。ZiFilter 允许设置初始状态（滤波器延迟的初始条件），并提供零相位滤波的附加方法。

### 属性

#### 1. Zi
- `public float[] Zi => _zi`
  - 获取状态向量。

#### 2. Tf
- `public override TransferFunction Tf { get; protected set; }`
  - 获取或设置滤波器的传递函数。

### 方法

#### 1. 构造函数
- `public ZiFilter(IEnumerable<float> b, IEnumerable<float> a)`
  - 从分子和分母构造 ZiFilter 实例。
  - 参数:
    - `b`: 传递函数的分子。
    - `a`: 传递函数的分母。

- `public ZiFilter(IEnumerable<double> b, IEnumerable<double> a)`
  - 从 64 位分子和分母构造 ZiFilter 实例。
  - 参数:
    - `b`: 传递函数的分子。
    - `a`: 传递函数的分母。

- `public ZiFilter(TransferFunction tf)`
  - 从传递函数构造 ZiFilter 实例。
  - 参数:
    - `tf`: 传递函数。

#### 2. Init
- `public virtual void Init(float[] zi)`
  - 使用初始条件初始化滤波器。
  - 参数:
    - `zi`: 初始条件向量。

- `public virtual void Init(double[] zi)`
  - 使用初始条件初始化滤波器。
  - 参数:
    - `zi`: 初始条件向量。

#### 3. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. ZeroPhase
- `public DiscreteSignal ZeroPhase(DiscreteSignal signal, int padLength = 0)`
  - 执行零相位滤波。
  - 参数:
    - `signal`: 输入信号。
    - `padLength`: 在应用滤波器之前在两端扩展信号的元素数量。默认值为 3 * (max{len(numerator), len(denominator)} - 1)。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 5. ChangeNumeratorCoeffs
- `public void ChangeNumeratorCoeffs(float[] b)`
  - 更改滤波器的分子系数。
  - 参数:
    - `b`: 新的分子系数。

#### 6. ChangeDenominatorCoeffs
- `public void ChangeDenominatorCoeffs(float[] a)`
  - 更改滤波器的分母系数。
  - 参数:
    - `a`: 新的分母系数。

#### 7. Change
- `public void Change(TransferFunction tf)`
  - 更改滤波器的传递函数。
  - 参数:
    - `tf`: 新的传递函数。

#### 8. Reset
- `public override void Reset()`
  - 重置滤波器。

#### 9. ApplyTo
- `public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

### 代码示例
以下是一个使用 ZiFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

public class ZiFilterExample
{
    public static void Main()
    {
        // 定义滤波器的分子和分母系数
        float[] b = { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };
        float[] a = { 1.0f, -0.5f, 0.25f, -0.125f, 0.0625f };

        // 创建 ZiFilter 实例
        ZiFilter filter = new ZiFilter(b, a);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 执行零相位滤波
        var signal = new DiscreteSignal(44100, inputSignal);
        var zeroPhaseSignal = filter.ZeroPhase(signal);
        Console.WriteLine("Zero Phase Filtered Signal:");
        foreach (var sample in zeroPhaseSignal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 重置滤波器
        filter.Reset();
    }
}
```
---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.Bassel 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.BandPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.BandPassFilter 是一个用于实现带通贝塞尔滤波器的类。

### 属性

#### 1. FrequencyLow
- `public float FrequencyLow { get; private set; }`
  - 获取低截止频率。

#### 2. FrequencyHigh
- `public float FrequencyHigh { get; private set; }`
  - 获取高截止频率。

#### 3. Order
- `public int Order => (_a.Length - 1) / 2`
  - 获取滤波器阶数。

### 方法

#### 1. 构造函数
- `public BandPassFilter(float frequencyLow, float frequencyHigh, int order)`
  - 构造具有给定阶数和截止频率的 BandPassFilter 实例。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order)`
  - 生成传递函数。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
  - 返回值: 生成的 `TransferFunction` 对象。

#### 3. Change
- `public void Change(float frequencyLow, float frequencyHigh)`
  - 更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。

### 代码示例
以下是一个使用 BandPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Bessel;

public class BandPassFilterExample
{
    public static void Main()
    {
        // 定义低截止频率、高截止频率和滤波器阶数
        float frequencyLow = 0.1f;
        float frequencyHigh = 0.3f;
        int order = 4;

        // 创建 BandPassFilter 实例
        BandPassFilter filter = new BandPassFilter(frequencyLow, frequencyHigh, order);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率
        filter.Change(0.2f, 0.4f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.BandStopFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.BandStopFilter 是一个用于实现带阻贝塞尔滤波器的类。

### 属性

#### 1. FrequencyLow
- `public float FrequencyLow { get; private set; }`
  - 获取低截止频率。

#### 2. FrequencyHigh
- `public float FrequencyHigh { get; private set; }`
  - 获取高截止频率。

#### 3. Order
- `public int Order => (_a.Length - 1) / 2`
  - 获取滤波器阶数。

### 方法

#### 1. 构造函数
- `public BandStopFilter(float frequencyLow, float frequencyHigh, int order)`
  - 构造具有给定阶数和截止频率的 BandStopFilter 实例。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order)`
  - 生成传递函数。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
  - 返回值: 生成的 `TransferFunction` 对象。

#### 3. Change
- `public void Change(float frequencyLow, float frequencyHigh)`
  - 更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。

### 代码示例
以下是一个使用 BandStopFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Bessel;

public class BandStopFilterExample
{
    public static void Main()
    {
        // 定义低截止频率、高截止频率和滤波器阶数
        float frequencyLow = 0.1f;
        float frequencyHigh = 0.3f;
        int order = 4;

        // 创建 BandStopFilter 实例
        BandStopFilter filter = new BandStopFilter(frequencyLow, frequencyHigh, order);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率
        filter.Change(0.2f, 0.4f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.HighPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.HighPassFilter 是一个用于实现高通贝塞尔滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; private set; }`
  - 获取截止频率。

#### 2. Order
- `public int Order => _a.Length - 1`
  - 获取滤波器阶数。

### 方法

#### 1. 构造函数
- `public HighPassFilter(float frequency, int order)`
  - 构造具有给定阶数和截止频率的 HighPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequency, int order)`
  - 生成传递函数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
  - 返回值: 生成的 `TransferFunction` 对象。

#### 3. Change
- `public void Change(float frequency)`
  - 更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。

### 代码示例
以下是一个使用 HighPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Bessel;

public class HighPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率和滤波器阶数
        float frequency = 0.2f;
        int order = 4;

        // 创建 HighPassFilter 实例
        HighPassFilter filter = new HighPassFilter(frequency, order);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率
        filter.Change(0.3f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.LowPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.LowPassFilter 是一个用于实现低通贝塞尔滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; private set; }`
  - 获取截止频率。

#### 2. Order
- `public int Order => _a.Length - 1`
  - 获取滤波器阶数。

### 方法

#### 1. 构造函数
- `public LowPassFilter(float frequency, int order)`
  - 构造具有给定阶数和截止频率的 LowPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequency, int order)`
  - 生成传递函数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
  - 返回值: 生成的 `TransferFunction` 对象。

#### 3. Change
- `public void Change(float frequency)`
  - 更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。

### 代码示例
以下是一个使用 LowPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Bessel;

public class LowPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率和滤波器阶数
        float frequency = 0.2f;
        int order = 4;

        // 创建 LowPassFilter 实例
        LowPassFilter filter = new LowPassFilter(frequency, order);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率
        filter.Change(0.3f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.PrototypeBessel 类

Vorcyc.Mathematics.SignalProcessing.Filters.Bessel.PrototypeBessel 是一个用于生成贝塞尔滤波器原型的静态类。

### 方法

#### 1. Reverse
- `public static float Reverse(int k, int n)`
  - 获取 n 阶贝塞尔多项式的第 k 个系数。
  - 参数:
    - `k`: 系数的索引。
    - `n`: 贝塞尔多项式的阶数。
  - 返回值: 第 k 个系数。

#### 2. Poles
- `public static ComplexFp32[] Poles(int order)`
  - 计算给定阶数的贝塞尔滤波器的模拟极点。
  - 参数:
    - `order`: 滤波器阶数。
  - 返回值: 极点数组。

### 代码示例
以下是一个使用 PrototypeBessel 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Bessel;

public class PrototypeBesselExample
{
    public static void Main()
    {
        // 定义贝塞尔多项式的阶数
        int order = 4;

        // 获取贝塞尔多项式的系数
        for (int k = 0; k <= order; k++)
        {
            float coefficient = PrototypeBessel.Reverse(k, order);
            Console.WriteLine($"Coefficient k={k}, n={order}: {coefficient}");
        }

        // 计算贝塞尔滤波器的极点
        var poles = PrototypeBessel.Poles(order);
        Console.WriteLine("Poles:");
        foreach (var pole in poles)
        {
            Console.WriteLine(pole);
        }
    }
}
```

---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.AllPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.AllPassFilter 是一个用于实现双二阶全通滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; protected set; }`
  - 获取中心频率。

#### 2. Q
- `public float Q { get; protected set; }`
  - 获取 Q 因子。

### 方法

#### 1. 构造函数
- `public AllPassFilter(float frequency, float q = 1)`
  - 构造 AllPassFilter 实例。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。

#### 2. SetCoefficients
- `private void SetCoefficients(float frequency, float q)`
  - 设置滤波器系数。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子。

#### 3. Change
- `public void Change(float frequency, float q = 1)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。

### 代码示例
以下是一个使用 AllPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

public class AllPassFilterExample
{
    public static void Main()
    {
        // 定义中心频率和 Q 因子
        float frequency = 0.2f;
        float q = 1.0f;

        // 创建 AllPassFilter 实例
        AllPassFilter filter = new AllPassFilter(frequency, q);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的中心频率和 Q 因子
        filter.Change(0.3f, 0.8f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.BandPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.BandPassFilter 是一个用于实现双二阶带通滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; protected set; }`
  - 获取中心频率。

#### 2. Q
- `public float Q { get; protected set; }`
  - 获取 Q 因子。

### 方法

#### 1. BandPassFilter
- `public BandPassFilter(float frequency, float q = 1)`
  - 构造 BandPassFilter 实例。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。

#### 2. SetCoefficients
- `private void SetCoefficients(float frequency, float q)`
  - 设置滤波器系数。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子。

#### 3. Change
- `public void Change(float frequency, float q = 1)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。

### 代码示例
以下是一个使用 BandPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;
public class BandPassFilterExample
{
    public static void Main()
    {
        // 定义中心频率和 Q 因子
        float frequency = 0.2f;
        float q = 1.0f;

        // 创建 BandPassFilter 实例
        BandPassFilter filter = new BandPassFilter(frequency, q);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的中心频率和 Q 因子
        filter.Change(0.3f, 0.8f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.BiQuadFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.BiQuadFilter 是一个用于实现双二阶 IIR 滤波器的类。

### 属性

无公开属性。

### 方法

#### 1. 构造函数
- `protected BiQuadFilter()`
  - 构造一个默认的 BiQuadFilter 实例，使用默认系数。

- `public BiQuadFilter(float b0, float b1, float b2, float a0, float a1, float a2)`
  - 从滤波器系数（分子 {B0, B1, B2} 和分母 {A0, A1, A2}）构造 BiQuadFilter 实例。
  - 参数:
    - `b0`: B0 系数。
    - `b1`: B1 系数。
    - `b2`: B2 系数。
    - `a0`: A0 系数。
    - `a1`: A1 系数。
    - `a2`: A2 系数。

#### 2. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置滤波器。

#### 4. Change
- `public void Change(float b0, float b1, float b2, float a0, float a1, float a2)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `b0`: B0 系数。
    - `b1`: B1 系数。
    - `b2`: B2 系数。
    - `a0`: A0 系数。
    - `a1`: A1 系数。
    - `a2`: A2 系数。

### 代码示例
以下是一个使用 BiQuadFilter 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

public class BiQuadFilterExample
{
    public static void Main()
    {
        // 定义滤波器的分子和分母系数
        float b0 = 0.2f;
        float b1 = 0.2f;
        float b2 = 0.2f;
        float a0 = 1.0f;
        float a1 = -0.5f;
        float a2 = 0.25f;

        // 创建 BiQuadFilter 实例
        BiQuadFilter filter = new BiQuadFilter(b0, b1, b2, a0, a1, a2);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的系数
        filter.Change(0.3f, 0.3f, 0.3f, 1.0f, -0.4f, 0.2f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }

        // 重置滤波器
        filter.Reset();
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.HighPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.HighPassFilter 是一个用于实现双二阶高通滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; protected set; }`
  - 获取截止频率。

#### 2. Q
- `public float Q { get; protected set; }`
  - 获取 Q 因子。

### 方法

#### 1. 构造函数
- `public HighPassFilter(float frequency, float q = 1)`
  - 构造 HighPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。

#### 2. SetCoefficients
- `private void SetCoefficients(float frequency, float q)`
  - 设置滤波器系数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `q`: Q 因子。

#### 3. Change
- `public void Change(float frequency, float q = 1)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。

### 代码示例
以下是一个使用 HighPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

public class HighPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率和 Q 因子
        float frequency = 0.2f;
        float q = 1.0f;

        // 创建 HighPassFilter 实例
        HighPassFilter filter = new HighPassFilter(frequency, q);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率和 Q 因子
        filter.Change(0.3f, 0.8f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.HighShelfFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.HighShelfFilter 是一个用于实现双二阶高架滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; protected set; }`
  - 获取架频率。

#### 2. Q
- `public float Q { get; protected set; }`
  - 获取 Q 因子。

#### 3. Gain
- `public float Gain { get; protected set; }`
  - 获取增益（以 dB 为单位）。

### 方法

#### 1. 构造函数
- `public HighShelfFilter(float frequency, float q = 1, float gain = 1.0f)`
  - 构造 HighShelfFilter 实例。
  - 参数:
    - `frequency`: 归一化架频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。
    - `gain`: 增益（以 dB 为单位），默认为 1.0f。

#### 2. SetCoefficients
- `private void SetCoefficients(float frequency, float q, float gain)`
  - 设置滤波器系数。
  - 参数:
    - `frequency`: 归一化架频率，范围 [0..0.5]。
    - `q`: Q 因子。
    - `gain`: 增益（以 dB 为单位）。

#### 3. Change
- `public void Change(float frequency, float q = 1, float gain = 1.0f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化架频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。
    - `gain`: 增益（以 dB 为单位），默认为 1.0f。

### 代码示例
以下是一个使用 HighShelfFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

public class HighShelfFilterExample
{
    public static void Main()
    {
        // 定义架频率、Q 因子和增益
        float frequency = 0.2f;
        float q = 1.0f;
        float gain = 3.0f;

        // 创建 HighShelfFilter 实例
        HighShelfFilter filter = new HighShelfFilter(frequency, q, gain);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的架频率、Q 因子和增益
        filter.Change(0.3f, 0.8f, 6.0f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.LowPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.LowPassFilter 是一个用于实现双二阶低通滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; protected set; }`
  - 获取截止频率。

#### 2. Q
- `public float Q { get; protected set; }`
  - 获取 Q 因子。

### 方法

#### 1. 构造函数
- `public LowPassFilter(float frequency, float q = 1)`
  - 构造 LowPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。

#### 2. SetCoefficients
- `private void SetCoefficients(float frequency, float q)`
  - 设置滤波器系数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `q`: Q 因子。

#### 3. Change
- `public void Change(float frequency, float q = 1)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。

### 代码示例
以下是一个使用 LowPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

public class LowPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率和 Q 因子
        float frequency = 0.2f;
        float q = 1.0f;

        // 创建 LowPassFilter 实例
        LowPassFilter filter = new LowPassFilter(frequency, q);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率和 Q 因子
        filter.Change(0.3f, 0.8f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.LowShelfFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.LowShelfFilter 是一个用于实现双二阶低架滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; protected set; }`
  - 获取架频率。

#### 2. Q
- `public float Q { get; protected set; }`
  - 获取 Q 因子。

#### 3. Gain
- `public float Gain { get; protected set; }`
  - 获取增益（以 dB 为单位）。

### 方法

#### 1. 构造函数
- `public LowShelfFilter(float frequency, float q = 1, float gain = 1.0f)`
  - 构造 LowShelfFilter 实例。
  - 参数:
    - `frequency`: 归一化架频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。
    - `gain`: 增益（以 dB 为单位），默认为 1.0f。

#### 2. SetCoefficients
- `private void SetCoefficients(float frequency, float q, float gain)`
  - 设置滤波器系数。
  - 参数:
    - `frequency`: 归一化架频率，范围 [0..0.5]。
    - `q`: Q 因子。
    - `gain`: 增益（以 dB 为单位）。

#### 3. Change
- `public void Change(float frequency, float q = 1, float gain = 1.0f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化架频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。
    - `gain`: 增益（以 dB 为单位），默认为 1.0f。

### 代码示例
以下是一个使用 LowShelfFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

public class LowShelfFilterExample
{
    public static void Main()
    {
        // 定义架频率、Q 因子和增益
        float frequency = 0.2f;
        float q = 1.0f;
        float gain = 3.0f;

        // 创建 LowShelfFilter 实例
        LowShelfFilter filter = new LowShelfFilter(frequency, q, gain);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的架频率、Q 因子和增益
        filter.Change(0.3f, 0.8f, 6.0f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.NotchFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.NotchFilter 是一个用于实现双二阶陷波滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; protected set; }`
  - 获取中心频率。

#### 2. Q
- `public float Q { get; protected set; }`
  - 获取 Q 因子。

### 方法

#### 1. NotchFilter
- `public NotchFilter(float frequency, float q = 1)`
  - 构造 NotchFilter 实例。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。

#### 2. SetCoefficients
- `private void SetCoefficients(float frequency, float q)`
  - 设置滤波器系数。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子。

#### 3. Change
- `public void Change(float frequency, float q = 1)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。

### 代码示例
以下是一个使用 NotchFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

public class NotchFilterExample
{
    public static void Main()
    {
        // 定义中心频率和 Q 因子
        float frequency = 0.2f;
        float q = 1.0f;

        // 创建 NotchFilter 实例
        NotchFilter filter = new NotchFilter(frequency, q);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的中心频率和 Q 因子
        filter.Change(0.3f, 0.8f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.PeakFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad.PeakFilter 是一个用于实现双二阶峰值均衡滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; protected set; }`
  - 获取中心频率。

#### 2. Q
- `public float Q { get; protected set; }`
  - 获取 Q 因子。

#### 3. Gain
- `public float Gain { get; protected set; }`
  - 获取增益（以 dB 为单位）。

### 方法

#### 1. PeakFilter
- `public PeakFilter(float frequency, float q = 1, float gain = 1.0f)`
  - 构造 PeakFilter 实例。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。
    - `gain`: 增益（以 dB 为单位），默认为 1.0f。

#### 2. SetCoefficients
- `private void SetCoefficients(float frequency, float q, float gain)`
  - 设置滤波器系数。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子。
    - `gain`: 增益（以 dB 为单位）。

#### 3. Change
- `public void Change(float frequency, float q = 1, float gain = 1.0f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化中心频率，范围 [0..0.5]。
    - `q`: Q 因子，默认为 1。
    - `gain`: 增益（以 dB 为单位），默认为 1.0f。

### 代码示例
以下是一个使用 PeakFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.BiQuad;

public class PeakFilterExample
{
    public static void Main()
    {
        // 定义中心频率、Q 因子和增益
        float frequency = 0.2f;
        float q = 1.0f;
        float gain = 3.0f;

        // 创建 PeakFilter 实例
        PeakFilter filter = new PeakFilter(frequency, q, gain);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的中心频率、Q 因子和增益
        filter.Change(0.3f, 0.8f, 6.0f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.BandPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.BandPassFilter 是一个用于实现带通巴特沃斯滤波器的类。

### 属性

#### 1. FrequencyLow
- `public float FrequencyLow { get; private set; }`
  - 获取低截止频率。

#### 2. FrequencyHigh
- `public float FrequencyHigh { get; private set; }`
  - 获取高截止频率。

#### 3. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. BandPassFilter
- `public BandPassFilter(float frequencyLow, float frequencyHigh, int order)`
  - 构造 BandPassFilter 实例。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order)`
  - 生成传递函数。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequencyLow, float frequencyHigh)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。

### 代码示例
以下是一个使用 BandPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;

public class BandPassFilterExample
{
    public static void Main()
    {
        // 定义低截止频率、高截止频率和滤波器阶数
        float frequencyLow = 0.1f;
        float frequencyHigh = 0.3f;
        int order = 4;

        // 创建 BandPassFilter 实例
        BandPassFilter filter = new BandPassFilter(frequencyLow, frequencyHigh, order);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的低截止频率和高截止频率
        filter.Change(0.2f, 0.4f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}

```
## Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.BandStopFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.BandStopFilter 是一个用于实现带阻巴特沃斯滤波器的类。

### 属性

#### 1. FrequencyLow
- `public float FrequencyLow { get; private set; }`
  - 获取低截止频率。

#### 2. FrequencyHigh
- `public float FrequencyHigh { get; private set; }`
  - 获取高截止频率。

#### 3. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. BandStopFilter
- `public BandStopFilter(float frequencyLow, float frequencyHigh, int order)`
  - 构造 BandStopFilter 实例。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order)`
  - 生成传递函数。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequencyLow, float frequencyHigh)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。

### 代码示例
以下是一个使用 BandStopFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;
public class BandStopFilterExample
{
    public static void Main()
    {
        // 定义低截止频率、高截止频率和滤波器阶数
        float frequencyLow = 0.1f;
        float frequencyHigh = 0.3f;
        int order = 4;

        // 创建 BandStopFilter 实例
        BandStopFilter filter = new BandStopFilter(frequencyLow, frequencyHigh, order);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的低截止频率和高截止频率
        filter.Change(0.2f, 0.4f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.HighPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.HighPassFilter 是一个用于实现高通巴特沃斯滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; private set; }`
  - 获取截止频率。

#### 2. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. HighPassFilter
- `public HighPassFilter(float frequency, int order)`
  - 构造 HighPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequency, int order)`
  - 生成传递函数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequency)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。

### 代码示例
以下是一个使用 HighPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;

public class HighPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率和滤波器阶数
        float frequency = 0.2f;
        int order = 4;

        // 创建 HighPassFilter 实例
        HighPassFilter filter = new HighPassFilter(frequency, order);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率
        filter.Change(0.3f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.LowPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.LowPassFilter 是一个用于实现低通巴特沃斯滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; private set; }`
  - 获取截止频率。

#### 2. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. LowPassFilter
- `public LowPassFilter(float frequency, int order)`
  - 构造 LowPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequency, int order)`
  - 生成传递函数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequency)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。

### 代码示例
以下是一个使用 LowPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;

public class LowPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率和滤波器阶数
        float frequency = 0.2f;
        int order = 4;

        // 创建 LowPassFilter 实例
        LowPassFilter filter = new LowPassFilter(frequency, order);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率
        filter.Change(0.3f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.PrototypeButterworth 类

Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth.PrototypeButterworth 是一个用于生成巴特沃斯滤波器原型的静态类。

### 方法

#### 1. Poles
- `public static ComplexFp32[] Poles(int order)`
  - 计算给定阶数的巴特沃斯滤波器的模拟极点。
  - 参数:
    - `order`: 滤波器阶数。
  - 返回值: 包含巴特沃斯滤波器极点的 ComplexFp32 数组。

### 代码示例
以下是一个使用 PrototypeButterworth 类中 Poles 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Butterworth;

public class PrototypeButterworthExample
{
    public static void Main()
    {
        // 定义滤波器阶数
        int order = 4;

        // 计算巴特沃斯滤波器的极点
        ComplexFp32[] poles = PrototypeButterworth.Poles(order);

        // 输出极点
        foreach (var pole in poles)
        {
            Console.WriteLine($"Pole: {pole.Real} + {pole.Imaginary}i");
        }
    }
}
```
---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.BandPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.BandPassFilter 是一个用于实现带通切比雪夫-I型滤波器的类。

### 属性

#### 1. FrequencyLow
- `public float FrequencyLow { get; private set; }`
  - 获取低截止频率。

#### 2. FrequencyHigh
- `public float FrequencyHigh { get; private set; }`
  - 获取高截止频率。

#### 3. Ripple
- `public float Ripple { get; private set; }`
  - 获取纹波（以 dB 为单位）。

#### 4. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. BandPassFilter
- `public BandPassFilter(float frequencyLow, float frequencyHigh, int order, float ripple = 0.1f)`
  - 构造 BandPassFilter 实例。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order, float ripple = 0.1f)`
  - 生成传递函数。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequencyLow, float frequencyHigh, float ripple = 0.1f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

### 代码示例
以下是一个使用 BandPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;
public class BandPassFilterExample
{
    public static void Main()
    {
        // 定义低截止频率、高截止频率、滤波器阶数和纹波
        float frequencyLow = 0.1f;
        float frequencyHigh = 0.3f;
        int order = 4;
        float ripple = 0.1f;

        // 创建 BandPassFilter 实例
        BandPassFilter filter = new BandPassFilter(frequencyLow, frequencyHigh, order, ripple);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的低截止频率、高截止频率和纹波
        filter.Change(0.2f, 0.4f, 0.2f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.BandStopFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.BandStopFilter 是一个用于实现带阻切比雪夫-I型滤波器的类。

### 属性

#### 1. FrequencyLow
- `public float FrequencyLow { get; private set; }`
  - 获取低截止频率。

#### 2. FrequencyHigh
- `public float FrequencyHigh { get; private set; }`
  - 获取高截止频率。

#### 3. Ripple
- `public float Ripple { get; private set; }`
  - 获取纹波（以 dB 为单位）。

#### 4. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. BandStopFilter
- `public BandStopFilter(float frequencyLow, float frequencyHigh, int order, float ripple = 0.1f)`
  - 构造 BandStopFilter 实例。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order, float ripple = 0.1f)`
  - 生成传递函数。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequencyLow, float frequencyHigh, float ripple = 0.1f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

### 代码示例
以下是一个使用 BandStopFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;

public class BandStopFilterExample
{
    public static void Main()
    {
        // 定义低截止频率、高截止频率、滤波器阶数和纹波
        float frequencyLow = 0.1f;
        float frequencyHigh = 0.3f;
        int order = 4;
        float ripple = 0.1f;

        // 创建 BandStopFilter 实例
        BandStopFilter filter = new BandStopFilter(frequencyLow, frequencyHigh, order, ripple);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的低截止频率、高截止频率和纹波
        filter.Change(0.2f, 0.4f, 0.2f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.HighPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.HighPassFilter 是一个用于实现高通切比雪夫-I型滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; private set; }`
  - 获取截止频率。

#### 2. Ripple
- `public float Ripple { get; private set; }`
  - 获取纹波（以 dB 为单位）。

#### 3. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. HighPassFilter
- `public HighPassFilter(float frequency, int order, float ripple = 0.1f)`
  - 构造 HighPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequency, int order, float ripple = 0.1f)`
  - 生成传递函数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequency, float ripple = 0.1f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

### 代码示例
以下是一个使用 HighPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;
public class HighPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率、滤波器阶数和纹波
        float frequency = 0.2f;
        int order = 4;
        float ripple = 0.1f;

        // 创建 HighPassFilter 实例
        HighPassFilter filter = new HighPassFilter(frequency, order, ripple);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率和纹波
        filter.Change(0.3f, 0.2f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.LowPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.LowPassFilter 是一个用于实现低通切比雪夫-I型滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; private set; }`
  - 获取截止频率。

#### 2. Ripple
- `public float Ripple { get; private set; }`
  - 获取纹波（以 dB 为单位）。

#### 3. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. LowPassFilter
- `public LowPassFilter(float frequency, int order, float ripple = 0.1f)`
  - 构造 LowPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequency, int order, float ripple = 0.1f)`
  - 生成传递函数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequency, float ripple = 0.1f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

### 代码示例
以下是一个使用 LowPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;

public class LowPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率、滤波器阶数和纹波
        float frequency = 0.2f;
        int order = 4;
        float ripple = 0.1f;

        // 创建 LowPassFilter 实例
        LowPassFilter filter = new LowPassFilter(frequency, order, ripple);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率和纹波
        filter.Change(0.3f, 0.2f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.PrototypeChebyshevI 类

Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI.PrototypeChebyshevI 是一个用于生成切比雪夫-I型滤波器原型的静态类。

### 方法

#### 1. Poles
- `public static ComplexFp32[] Poles(int order, float ripple = 0.1f)`
  - 计算给定阶数的切比雪夫-I型滤波器的模拟极点。
  - 参数:
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。
  - 返回值: 包含切比雪夫-I型滤波器极点的 ComplexFp32 数组。

### 代码示例
以下是一个使用 PrototypeChebyshevI 类中 Poles 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevI;

public class PrototypeChebyshevIExample
{
    public static void Main()
    {
        // 定义滤波器阶数和纹波
        int order = 4;
        float ripple = 0.1f;

        // 计算切比雪夫-I型滤波器的极点
        ComplexFp32[] poles = PrototypeChebyshevI.Poles(order, ripple);

        // 输出极点
        foreach (var pole in poles)
        {
            Console.WriteLine($"Pole: {pole.Real} + {pole.Imaginary}i");
        }
    }
}
```

---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.BandPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.BandPassFilter 是一个用于实现带通切比雪夫-II型滤波器的类。

### 属性

#### 1. FrequencyLow
- `public float FrequencyLow { get; private set; }`
  - 获取低截止频率。

#### 2. FrequencyHigh
- `public float FrequencyHigh { get; private set; }`
  - 获取高截止频率。

#### 3. Ripple
- `public float Ripple { get; private set; }`
  - 获取纹波（以 dB 为单位）。

#### 4. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. BandPassFilter
- `public BandPassFilter(float frequencyLow, float frequencyHigh, int order, float ripple = 0.1f)`
  - 构造 BandPassFilter 实例。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order, float ripple = 0.1f)`
  - 生成传递函数。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequencyLow, float frequencyHigh, float ripple = 0.1f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

### 代码示例
以下是一个使用 BandPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

public class BandPassFilterExample
{
    public static void Main()
    {
        // 定义低截止频率、高截止频率、滤波器阶数和纹波
        float frequencyLow = 0.1f;
        float frequencyHigh = 0.3f;
        int order = 4;
        float ripple = 0.1f;

        // 创建 BandPassFilter 实例
        BandPassFilter filter = new BandPassFilter(frequencyLow, frequencyHigh, order, ripple);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的低截止频率、高截止频率和纹波
        filter.Change(0.2f, 0.4f, 0.2f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.BandStopFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.BandStopFilter 是一个用于实现带阻切比雪夫-II型滤波器的类。

### 属性

#### 1. FrequencyLow
- `public float FrequencyLow { get; private set; }`
  - 获取低截止频率。

#### 2. FrequencyHigh
- `public float FrequencyHigh { get; private set; }`
  - 获取高截止频率。

#### 3. Ripple
- `public float Ripple { get; private set; }`
  - 获取纹波（以 dB 为单位）。

#### 4. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. BandStopFilter
- `public BandStopFilter(float frequencyLow, float frequencyHigh, int order, float ripple = 0.1f)`
  - 构造 BandStopFilter 实例。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order, float ripple = 0.1f)`
  - 生成传递函数。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequencyLow, float frequencyHigh, float ripple = 0.1f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

### 代码示例
以下是一个使用 BandStopFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

public class BandStopFilterExample
{
    public static void Main()
    {
        // 定义低截止频率、高截止频率、滤波器阶数和纹波
        float frequencyLow = 0.1f;
        float frequencyHigh = 0.3f;
        int order = 4;
        float ripple = 0.1f;

        // 创建 BandStopFilter 实例
        BandStopFilter filter = new BandStopFilter(frequencyLow, frequencyHigh, order, ripple);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的低截止频率、高截止频率和纹波
        filter.Change(0.2f, 0.4f, 0.2f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.HighPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.HighPassFilter 是一个用于实现高通切比雪夫-II型滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; private set; }`
  - 获取截止频率。

#### 2. Ripple
- `public float Ripple { get; private set; }`
  - 获取纹波（以 dB 为单位）。

#### 3. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. HighPassFilter
- `public HighPassFilter(float frequency, int order, float ripple = 0.1f)`
  - 构造 HighPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequency, int order, float ripple = 0.1f)`
  - 生成传递函数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequency, float ripple = 0.1f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

### 代码示例
以下是一个使用 HighPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

public class HighPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率、滤波器阶数和纹波
        float frequency = 0.2f;
        int order = 4;
        float ripple = 0.1f;

        // 创建 HighPassFilter 实例
        HighPassFilter filter = new HighPassFilter(frequency, order, ripple);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率和纹波
        filter.Change(0.3f, 0.2f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.LowPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.LowPassFilter 是一个用于实现低通切比雪夫-II型滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; private set; }`
  - 获取截止频率。

#### 2. Ripple
- `public float Ripple { get; private set; }`
  - 获取纹波（以 dB 为单位）。

#### 3. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. LowPassFilter
- `public LowPassFilter(float frequency, int order, float ripple = 0.1f)`
  - 构造 LowPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequency, int order, float ripple = 0.1f)`
  - 生成传递函数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequency, float ripple = 0.1f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。

### 代码示例
以下是一个使用 LowPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

public class LowPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率、滤波器阶数和纹波
        float frequency = 0.2f;
        int order = 4;
        float ripple = 0.1f;

        // 创建 LowPassFilter 实例
        LowPassFilter filter = new LowPassFilter(frequency, order, ripple);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率和纹波
        filter.Change(0.3f, 0.2f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.PrototypeChebyshevII 类

Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII.PrototypeChebyshevII 是一个用于生成切比雪夫-II型滤波器原型的静态类。

### 方法

#### 1. Poles
- `public static ComplexFp32[] Poles(int order, float ripple = 0.1f)`
  - 计算给定阶数的切比雪夫-II型滤波器的模拟极点。
  - 参数:
    - `order`: 滤波器阶数。
    - `ripple`: 纹波（以 dB 为单位），默认为 0.1f。
  - 返回值: 包含切比雪夫-II型滤波器极点的 ComplexFp32 数组。

#### 2. Zeros
- `public static ComplexFp32[] Zeros(int order)`
  - 计算给定阶数的切比雪夫-II型滤波器的模拟零点。
  - 参数:
    - `order`: 滤波器阶数。
  - 返回值: 包含切比雪夫-II型滤波器零点的 ComplexFp32 数组。

### 代码示例
以下是一个使用 PrototypeChebyshevII 类中 Poles 和 Zeros 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.ChebyshevII;

public class PrototypeChebyshevIIExample
{
    public static void Main()
    {
        // 定义滤波器阶数和纹波
        int order = 4;
        float ripple = 0.1f;

        // 计算切比雪夫-II型滤波器的极点
        ComplexFp32[] poles = PrototypeChebyshevII.Poles(order, ripple);

        // 输出极点
        foreach (var pole in poles)
        {
            Console.WriteLine($"Pole: {pole.Real} + {pole.Imaginary}i");
        }

        // 计算切比雪夫-II型滤波器的零点
        ComplexFp32[] zeros = PrototypeChebyshevII.Zeros(order);

        // 输出零点
        foreach (var zero in zeros)
        {
            Console.WriteLine($"Zero: {zero.Real} + {zero.Imaginary}i");
        }
    }
}

```

---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.BandPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.BandPassFilter 是一个用于实现带通椭圆滤波器的类。

### 属性

#### 1. FrequencyLow
- `public float FrequencyLow { get; private set; }`
  - 获取低截止频率。

#### 2. FrequencyHigh
- `public float FrequencyHigh { get; private set; }`
  - 获取高截止频率。

#### 3. RipplePassband
- `public float RipplePassband { get; private set; }`
  - 获取通带纹波（以 dB 为单位）。

#### 4. RippleStopband
- `public float RippleStopband { get; private set; }`
  - 获取阻带纹波（以 dB 为单位）。

#### 5. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. BandPassFilter
- `public BandPassFilter(float frequencyLow, float frequencyHigh, int order, float ripplePass = 1f, float rippleStop = 20f)`
  - 构造 BandPassFilter 实例。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order, float ripplePass = 1, float rippleStop = 20)`
  - 生成传递函数。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequencyLow, float frequencyHigh, float ripplePass = 1, float rippleStop = 20f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。

### 代码示例
以下是一个使用 BandPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic;
public class BandPassFilterExample
{
    public static void Main()
    {
        // 定义低截止频率、高截止频率、滤波器阶数、通带纹波和阻带纹波
        float frequencyLow = 0.1f;
        float frequencyHigh = 0.3f;
        int order = 4;
        float ripplePass = 1f;
        float rippleStop = 20f;

        // 创建 BandPassFilter 实例
        BandPassFilter filter = new BandPassFilter(frequencyLow, frequencyHigh, order, ripplePass, rippleStop);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的低截止频率、高截止频率、通带纹波和阻带纹波
        filter.Change(0.2f, 0.4f, 0.5f, 25f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.BandStopFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.BandStopFilter 是一个用于实现带阻椭圆滤波器的类。

### 属性

#### 1. FrequencyLow
- `public float FrequencyLow { get; private set; }`
  - 获取低截止频率。

#### 2. FrequencyHigh
- `public float FrequencyHigh { get; private set; }`
  - 获取高截止频率。

#### 3. RipplePassband
- `public float RipplePassband { get; private set; }`
  - 获取通带纹波（以 dB 为单位）。

#### 4. RippleStopband
- `public float RippleStopband { get; private set; }`
  - 获取阻带纹波（以 dB 为单位）。

#### 5. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. BandStopFilter
- `public BandStopFilter(float frequencyLow, float frequencyHigh, int order, float ripplePass = 1f, float rippleStop = 20f)`
  - 构造 BandStopFilter 实例。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequencyLow, float frequencyHigh, int order, float ripplePass = 1f, float rippleStop = 20f)`
  - 生成传递函数。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequencyLow, float frequencyHigh, float ripplePass = 1f, float rippleStop = 20f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。

### 代码示例
以下是一个使用 BandStopFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic;

public class BandStopFilterExample
{
    public static void Main()
    {
        // 定义低截止频率、高截止频率、滤波器阶数、通带纹波和阻带纹波
        float frequencyLow = 0.1f;
        float frequencyHigh = 0.3f;
        int order = 4;
        float ripplePass = 1f;
        float rippleStop = 20f;

        // 创建 BandStopFilter 实例
        BandStopFilter filter = new BandStopFilter(frequencyLow, frequencyHigh, order, ripplePass, rippleStop);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的低截止频率、高截止频率、通带纹波和阻带纹波
        filter.Change(0.2f, 0.4f, 0.5f, 25f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}

```

## Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.HighPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.HighPassFilter 是一个用于实现高通椭圆滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; private set; }`
  - 获取截止频率。

#### 2. RipplePassband
- `public float RipplePassband { get; private set; }`
  - 获取通带纹波（以 dB 为单位）。

#### 3. RippleStopband
- `public float RippleStopband { get; private set; }`
  - 获取阻带纹波（以 dB 为单位）。

#### 4. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. HighPassFilter
- `public HighPassFilter(float frequency, int order, float ripplePass = 1f, float rippleStop = 20f)`
  - 构造 HighPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequency, int order, float ripplePass = 1f, float rippleStop = 20f)`
  - 生成传递函数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequency, float ripplePass = 1f, float rippleStop = 20f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。

### 代码示例
以下是一个使用 HighPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic;

public class HighPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率、滤波器阶数、通带纹波和阻带纹波
        float frequency = 0.2f;
        int order = 4;
        float ripplePass = 1f;
        float rippleStop = 20f;

        // 创建 HighPassFilter 实例
        HighPassFilter filter = new HighPassFilter(frequency, order, ripplePass, rippleStop);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率、通带纹波和阻带纹波
        filter.Change(0.3f, 0.5f, 25f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.LowPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.LowPassFilter 是一个用于实现低通椭圆滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; private set; }`
  - 获取截止频率。

#### 2. RipplePassband
- `public float RipplePassband { get; private set; }`
  - 获取通带纹波（以 dB 为单位）。

#### 3. RippleStopband
- `public float RippleStopband { get; private set; }`
  - 获取阻带纹波（以 dB 为单位）。

#### 4. Order
- `public int Order { get; }`
  - 获取滤波器阶数。

### 方法

#### 1. LowPassFilter
- `public LowPassFilter(float frequency, int order, float ripplePass = 1, float rippleStop = 20f)`
  - 构造 LowPassFilter 实例。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。

#### 2. MakeTf
- `private static TransferFunction MakeTf(float frequency, int order, float ripplePass = 1, float rippleStop = 20f)`
  - 生成传递函数。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `order`: 滤波器阶数。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。
  - 返回值: 传递函数。

#### 3. Change
- `public void Change(float frequency, float ripplePass = 1, float rippleStop = 20f)`
  - 在线更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。

### 代码示例
以下是一个使用 LowPassFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic;
public class LowPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率、滤波器阶数、通带纹波和阻带纹波
        float frequency = 0.2f;
        int order = 4;
        float ripplePass = 1f;
        float rippleStop = 20f;

        // 创建 LowPassFilter 实例
        LowPassFilter filter = new LowPassFilter(frequency, order, ripplePass, rippleStop);

        // 定义输入信号
        float[] inputSignal = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output: {output}");
        }

        // 更改滤波器的截止频率、通带纹波和阻带纹波
        filter.Change(0.3f, 0.5f, 25f);

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            float output = filter.Process(sample);
            Console.WriteLine($"Input: {sample}, Output after change: {output}");
        }
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.PrototypeElliptic 类

Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic.PrototypeElliptic 是一个用于生成椭圆滤波器原型的静态类。

### 方法

#### 1. Poles
- `public static ComplexFp32[] Poles(int order, float ripplePass = 1f, float rippleStop = 20f)`
  - 计算给定阶数的椭圆滤波器的模拟极点。
  - 参数:
    - `order`: 滤波器阶数。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。
  - 返回值: 包含椭圆滤波器极点的 ComplexFp32 数组。

#### 2. Zeros
- `public static ComplexFp32[] Zeros(int order, float ripplePass = 1, float rippleStop = 20)`
  - 计算给定阶数的椭圆滤波器的模拟零点。
  - 参数:
    - `order`: 滤波器阶数。
    - `ripplePass`: 通带纹波（以 dB 为单位），默认为 1f。
    - `rippleStop`: 阻带纹波（以 dB 为单位），默认为 20f。
  - 返回值: 包含椭圆滤波器零点的 ComplexFp32 数组。

#### 3. Landen
- `public static float[] Landen(float k, int iterCount = 5)`
  - 计算 Landen 序列。
  - 参数:
    - `k`: 参数 K。
    - `iterCount`: 迭代次数，默认为 5。
  - 返回值: Landen 序列的浮点数组。

#### 4. Cde
- `public static ComplexFp32 Cde(ComplexFp32 x, float[] landen)`
  - 计算 Cde。
  - 参数:
    - `x`: 参数 X。
    - `landen`: Landen 序列。
  - 返回值: 计算结果的 ComplexFp32 值。

#### 5. Sne
- `public static ComplexFp32 Sne(ComplexFp32 x, float[] landen)`
  - 计算 Sne。
  - 参数:
    - `x`: 参数 X。
    - `landen`: Landen 序列。
  - 返回值: 计算结果的 ComplexFp32 值。

#### 6. Asne
- `public static ComplexFp32 Asne(ComplexFp32 x, float k, int iterCount = 5)`
  - 计算逆 Sne。
  - 参数:
    - `x`: 参数 X。
    - `k`: 参数 K。
    - `iterCount`: 迭代次数，默认为 5。
  - 返回值: 计算结果的 ComplexFp32 值。

### 代码示例
以下是一个使用 PrototypeElliptic 类中 Poles 和 Zeros 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Elliptic;

public class PrototypeEllipticExample
{
    public static void Main()
    {
        // 定义滤波器阶数、通带纹波和阻带纹波
        int order = 4;
        float ripplePass = 1f;
        float rippleStop = 20f;

        // 计算椭圆滤波器的极点
        ComplexFp32[] poles = PrototypeElliptic.Poles(order, ripplePass, rippleStop);

        // 输出极点
        foreach (var pole in poles)
        {
            Console.WriteLine($"Pole: {pole.Real} + {pole.Imaginary}i");
        }

        // 计算椭圆滤波器的零点
        ComplexFp32[] zeros = PrototypeElliptic.Zeros(order, ripplePass, rippleStop);

        // 输出零点
        foreach (var zero in zeros)
        {
            Console.WriteLine($"Zero: {zero.Real} + {zero.Imaginary}i");
        }
    }
}
```


---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.Fda 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Filters.Fda.DesignFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.Fda.DesignFilter 是一个提供滤波器设计和分析方法的静态类。

### 方法

#### 1. FirWinFdLp
- `public static float[] FirWinFdLp(int order, float frequency, float delay, WindowType window = WindowType.Blackman)`
  - 使用 sinc-window 方法设计理想的低通分数延迟 FIR 滤波器。
  - 参数:
    - `order`: 滤波器阶数。
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `delay`: 分数延迟。
    - `window`: 窗函数，默认为 `WindowType.Blackman`。
  - 返回值: 滤波器系数数组。

#### 2. FirWinFdHp
- `public static float[] FirWinFdHp(int order, float frequency, float delay, WindowType window = WindowType.Blackman)`
  - 使用 sinc-window 方法设计理想的高通分数延迟 FIR 滤波器。
  - 参数:
    - `order`: 滤波器阶数。
    - `frequency`: 归一化截止频率，范围 [0..0.5]。
    - `delay`: 分数延迟。
    - `window`: 窗函数，默认为 `WindowType.Blackman`。
  - 返回值: 滤波器系数数组。

#### 3. FirWinFdBp
- `public static float[] FirWinFdBp(int order, float frequencyLow, float frequencyHigh, float delay, WindowType window = WindowType.Blackman)`
  - 使用 sinc-window 方法设计理想的带通分数延迟 FIR 滤波器。
  - 参数:
    - `order`: 滤波器阶数。
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `delay`: 分数延迟。
    - `window`: 窗函数，默认为 `WindowType.Blackman`。
  - 返回值: 滤波器系数数组。

#### 4. FirWinFdBs
- `public static float[] FirWinFdBs(int order, float frequencyLow, float frequencyHigh, float delay, WindowType window = WindowType.Blackman)`
  - 使用 sinc-window 方法设计理想的带阻分数延迟 FIR 滤波器。
  - 参数:
    - `order`: 滤波器阶数。
    - `frequencyLow`: 归一化低截止频率，范围 [0..0.5]。
    - `frequencyHigh`: 归一化高截止频率，范围 [0..0.5]。
    - `delay`: 分数延迟。
    - `window`: 窗函数，默认为 `WindowType.Blackman`。
  - 返回值: 滤波器系数数组。

#### 5. FirWinFdAp
- `public static float[] FirWinFdAp(int order, float delay, WindowType window = WindowType.Blackman)`
  - 使用 sinc-window 方法设计理想的全通分数延迟 FIR 滤波器。
  - 参数:
    - `order`: 滤波器阶数。
    - `delay`: 分数延迟。
    - `window`: 窗函数，默认为 `WindowType.Blackman`。
  - 返回值: 滤波器系数数组。

#### 6. NormalizeKernel
- `public static void NormalizeKernel(float[] kernel, float frequency = 0)`
  - 在给定频率下归一化频率响应（将核系数归一化以将频率响应映射到 [0, 1]）。
  - 参数:
    - `kernel`: 核系数数组。
    - `frequency`: 频率，默认为 0。

### 代码示例
以下是一个使用 DesignFilter 类中 FirWinFdLp 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

public class DesignFilterExample
{
    public static void Main()
    {
        // 定义滤波器阶数、截止频率和分数延迟
        int order = 64;
        float frequency = 0.25f;
        float delay = 0.5f;

        // 设计低通分数延迟 FIR 滤波器
        float[] filterCoefficients = DesignFilter.FirWinFdLp(order, frequency, delay);

        // 输出滤波器系数
        foreach (var coeff in filterCoefficients)
        {
            Console.WriteLine($"Coefficient: {coeff}");
        }
    }
}

```
## Vorcyc.Mathematics.SignalProcessing.Filters.Fda.FilterBanks 类

Vorcyc.Mathematics.SignalProcessing.Filters.Fda.FilterBanks 是一个提供滤波器组设计方法的静态类。

### 方法

#### 1. Triangular
- `public static float[][] Triangular(int fftSize, int samplingRate, (float, float, float)[] frequencies, VtlnWarper? vtln = null, Func<float, float>? mapper = null)`
  - 基于给定的频率生成三角形滤波器组权重。
  - 参数:
    - `fftSize`: 假定的 FFT 大小。
    - `samplingRate`: 假定的信号采样率。
    - `frequencies`: 每个滤波器的频率元组数组（左、中、右）。
    - `vtln`: VTLN 频率扭曲器，默认为 null。
    - `mapper`: 频率尺度映射器（例如 herz-to-mel），仅用于适当加权，默认为 null。
  - 返回值: 滤波器组权重数组。

#### 2. Rectangular
- `public static float[][] Rectangular(int fftSize, int samplingRate, (float, float, float)[] frequencies, VtlnWarper? vtln = null, Func<float, float>? mapper = null)`
  - 基于给定的频率生成矩形滤波器组权重。
  - 参数:
    - `fftSize`: 假定的 FFT 大小。
    - `samplingRate`: 假定的信号采样率。
    - `frequencies`: 每个滤波器的频率元组数组（左、中、右）。
    - `vtln`: VTLN 频率扭曲器，默认为 null。
    - `mapper`: 频率尺度映射器（例如 herz-to-mel），默认为 null。
  - 返回值: 滤波器组权重数组。

#### 3. Trapezoidal
- `public static float[][] Trapezoidal(int fftSize, int samplingRate, (float, float, float)[] frequencies, VtlnWarper? vtln = null, Func<float, float>? mapper = null)`
  - 基于给定的频率生成 FIR 带通（接近梯形）滤波器组。
  - 参数:
    - `fftSize`: 假定的 FFT 大小。
    - `samplingRate`: 假定的信号采样率。
    - `frequencies`: 每个滤波器的频率元组数组（左、中、右）。
    - `vtln`: VTLN 频率扭曲器，默认为 null。
    - `mapper`: 频率尺度映射器（例如 herz-to-mel），默认为 null。
  - 返回值: 滤波器组权重数组。

#### 4. BiQuad
- `public static float[][] BiQuad(int fftSize, int samplingRate, (float, float, float)[] frequencies)`
  - 基于给定的频率生成 BiQuad 带通重叠滤波器。
  - 参数:
    - `fftSize`: 假定的 FFT 大小。
    - `samplingRate`: 假定的信号采样率。
    - `frequencies`: 每个滤波器的频率元组数组（左、中、右）。
  - 返回值: 滤波器组权重数组。

#### 5. HerzBands
- `public static (float, float, float)[] HerzBands(int combFilterCount, int samplingRate, float lowFreq = 0, float highFreq = 0f, bool overlap = false)`
  - 返回 Herz 频率尺度上均匀分布的频带的频率元组。
  - 参数:
    - `combFilterCount`: 滤波器数量。
    - `samplingRate`: 假定的信号采样率。
    - `lowFreq`: 频率范围的下限，默认为 0。
    - `highFreq`: 频率范围的上限，默认为 0。
    - `overlap`: 指示频带是否应重叠的标志，默认为 false。
  - 返回值: 频率元组数组。

#### 6. MelBands
- `public static (float, float, float)[] MelBands(int melFilterCount, int samplingRate, float lowFreq = 0f, float highFreq = 0f, bool overlap = true)`
  - 返回 Mel 频率尺度上均匀分布的频带的频率元组。
  - 参数:
    - `melFilterCount`: Mel 滤波器数量。
    - `samplingRate`: 假定的信号采样率。
    - `lowFreq`: 频率范围的下限，默认为 0。
    - `highFreq`: 频率范围的上限，默认为 0。
    - `overlap`: 指示频带是否应重叠的标志，默认为 true。
  - 返回值: 频率元组数组。

#### 7. MelBandsSlaney
- `public static (float, float, float)[] MelBandsSlaney(int melFilterCount, int samplingRate, float lowFreq = 0, float highFreq = 0, bool overlap = true)`
  - 返回 Mel 频率尺度上均匀分布的频带的频率元组（根据 M.Slaney 的公式）。
  - 参数:
    - `melFilterCount`: Mel 滤波器数量。
    - `samplingRate`: 假定的信号采样率。
    - `lowFreq`: 频率范围的下限，默认为 0。
    - `highFreq`: 频率范围的上限，默认为 0。
    - `overlap`: 指示频带是否应重叠的标志，默认为 true。
  - 返回值: 频率元组数组。

#### 8. BarkBands
- `public static (float, float, float)[] BarkBands(int barkFilterCount, int samplingRate, float lowFreq = 0f, float highFreq = 0f, bool overlap = true)`
  - 返回 Bark 频率尺度上均匀分布的频带的频率元组（Traunmueller, 1990）。
  - 参数:
    - `barkFilterCount`: Bark 滤波器数量。
    - `samplingRate`: 假定的信号采样率。
    - `lowFreq`: 频率范围的下限，默认为 0。
    - `highFreq`: 频率范围的上限，默认为 0。
    - `overlap`: 指示频带是否应重叠的标志，默认为 true。
  - 返回值: 频率元组数组。

#### 9. BarkBandsSlaney
- `public static (float, float, float)[] BarkBandsSlaney(int barkFilterCount, int samplingRate, float lowFreq = 0, float highFreq = 0f, bool overlap = true)`
  - 返回 Bark 频率尺度上均匀分布的频带的频率元组（Wang, 1992）。
  - 参数:
    - `barkFilterCount`: Bark 滤波器数量。
    - `samplingRate`: 假定的信号采样率。
    - `lowFreq`: 频率范围的下限，默认为 0。
    - `highFreq`: 频率范围的上限，默认为 0。
    - `overlap`: 指示频带是否应重叠的标志，默认为 true。
  - 返回值: 频率元组数组。

#### 10. CriticalBands
- `public static (float, float, float)[] CriticalBands(int filterCount, int samplingRate, float lowFreq = 0f, float highFreq = 0f)`
  - 返回临界频带的频率元组。
  - 参数:
    - `filterCount`: 滤波器数量。
    - `samplingRate`: 假定的信号采样率。
    - `lowFreq`: 频率范围的下限，默认为 0。
    - `highFreq`: 频率范围的上限，默认为 0。
  - 返回值: 频率元组数组。

#### 11. OctaveBands
- `public static (float, float, float)[] OctaveBands(int octaveCount, int samplingRate, float lowFreq = 0f, float highFreq = 0f, bool overlap = false)`
  - 返回倍频程频带的频率元组。
  - 参数:
    - `octaveCount`: 倍频程滤波器数量。
    - `samplingRate`: 假定的信号采样率。
    - `lowFreq`: 频率范围的下限，默认为 0。
    - `highFreq`: 频率范围的上限，默认为 0。
    - `overlap`: 指示频带是否应重叠的标志，默认为 false。
  - 返回值: 频率元组数组。

#### 12. Chroma
- `public static float[][] Chroma(int fftSize, int samplingRate, int chromaCount = 12, float tuning = 0, float centerOctave = 5.0f, float octaveWidth = 2, int norm = 2, bool baseC = true)`
  - 生成色度特征滤波器组。
  - 参数:
    - `fftSize`: 假定的 FFT 大小。
    - `samplingRate`: 假定的采样率。
    - `chromaCount`: 色度特征数量，默认为 12。
    - `tuning`: 从 A440 偏离的调音，以色度 bin 的分数表示，默认为 0。
    - `centerOctave`: 如果 octaveWidth=0，则忽略 centerOctave。否则，它是高斯窗的中心，默认为 5.0f。
    - `octaveWidth`: 如果 octaveWidth=0，则形状为矩形。否则，它是高斯窗的宽度，默认为 2。
    - `norm`: 规范化：0 - 无规范化，1 - 应用 L1 规范化，2 - 应用 L2 规范化，默认为 2。
    - `baseC`: 如果 baseC=true，则滤波器组将从 'C' 开始。否则，滤波器组将从 'A' 开始，默认为 true。
  - 返回值: 滤波器组权重数组。

#### 13. MelBankSlaney
- `public static float[][] MelBankSlaney(int filterCount, int fftSize, int samplingRate, float lowFreq = 0f, float highFreq = 0f, bool normalizeGain = true, VtlnWarper? vtln = null)`
  - 创建重叠的三角形 Mel 滤波器（如 Malcolm Slaney 所建议）。
  - 参数:
    - `filterCount`: Mel 滤波器数量。
    - `fftSize`: 假定的 FFT 大小。
    - `samplingRate`: 假定的采样率。
    - `lowFreq`: 频率范围的下限，默认为 0。
    - `highFreq`: 频率范围的上限，默认为 0。
    - `normalizeGain`: 如果应规范化增益，则为 true；如果所有滤波器应具有相同的高度 1.0，则为 false，默认为 true。
    - `vtln`: VTLN 频率扭曲器，默认为 null。
  - 返回值: 滤波器组权重数组。

#### 14. BarkBankSlaney
- `public static float[][] BarkBankSlaney(int filterCount, int fftSize, int samplingRate, float lowFreq = 0, float highFreq = 0, float width = 1)`
  - 创建重叠的梯形 Bark 滤波器（如 Malcolm Slaney 所建议）。
  - 参数:
    - `filterCount`: Bark 滤波器数量。
    - `fftSize`: 假定的 FFT 大小。
    - `samplingRate`: 假定的采样率。
    - `lowFreq`: 频率范围的下限，默认为 0。
    - `highFreq`: 频率范围的上限，默认为 0。
    - `width`: 每个频带的恒定宽度，默认为 1。
  - 返回值: 滤波器组权重数组。

#### 15. Erb
- `public static float[][] Erb(int erbFilterCount, int fftSize, int samplingRate, float lowFreq = 0, float highFreq = 0, bool normalizeGain = true)`
  - 创建重叠的 ERB 滤波器。
  - 参数:
    - `erbFilterCount`: ERB 滤波器数量。
    - `fftSize`: 假定的 FFT 大小。
    - `samplingRate`: 假定的采样率。
    - `lowFreq`: 频率范围的下限，默认为 0。
    - `highFreq`: 频率范围的上限，默认为 0。
    - `normalizeGain`: 如果应规范化增益，则为 true；如果所有滤波器应具有相同的高度 1.0，则为 false，默认为 true。
  - 返回值: 滤波器组权重数组。

#### 16. Normalize
- `public static void Normalize(int filterCount, (float, float, float)[] frequencies, float[][] filterBank)`
  - 规范化权重（使每个频带中的能量大致相等）。
  - 参数:
    - `filterCount`: 滤波器数量。
    - `frequencies`: 每个滤波器的频率元组数组（左、中、右）。
    - `filterBank`: 滤波器组。

#### 17. Apply
- `public static void Apply(float[][] filterbank, float[] spectrum, float[] filtered)`
  - 将滤波器应用于频谱并填充结果滤波后的频谱。
  - 参数:
    - `filterbank`: 滤波器组。
    - `spectrum`: 频谱。
    - `filtered`: 滤波后的信号频谱。

#### 18. Apply
- `public static float[][] Apply(float[][] filterbank, IList<float[]> spectrogram)`
  - 将滤波器应用于给定序列中的所有频谱。
  - 参数:
    - `filterbank`: 滤波器组。
    - `spectrogram`: 滤波后信号的输出频谱。
  - 返回值: 滤波后的频谱数组。

#### 19. ApplyAndLog
- `public static void ApplyAndLog(float[][] filterbank, float[] spectrum, float[] filtered, float floor = float.Epsilon)`
  - 将滤波器应用于频谱，然后对结果频谱执行 Ln()。
  - 参数:
    - `filterbank`: 滤波器组。
    - `spectrum`: 频谱。
    - `filtered`: 滤波后的信号频谱。
    - `floor`: 对数操作的阈值，默认为 float.Epsilon。

#### 20. ApplyAndLog10
- `public static void ApplyAndLog10(float[][] filterbank, float[] spectrum, float[] filtered, float floor = float.Epsilon)`
  - 将滤波器应用于频谱，然后对结果频谱执行 Log10()。
  - 参数:
    - `filterbank`: 滤波器组。
    - `spectrum`: 频谱。
    - `filtered`: 滤波后的信号频谱。
    - `floor`: 对数操作的阈值，默认为 float.Epsilon。

#### 21. ApplyAndToDecibel
- `public static void ApplyAndToDecibel(float[][] filterbank, float[] spectrum, float[] filtered, float minLevel = 1e-10f)`
  - 将滤波器应用于频谱，然后对结果频谱执行 10*Log10()（添加以与 librosa 结果比较 MFCC 系数）。
  - 参数:
    - `filterbank`: 滤波器组。
    - `spectrum`: 频谱。
    - `filtered`: 滤波后的信号频谱。
    - `minLevel`: 对数操作的阈值，默认为 1e-10f。

#### 22. ApplyAndPow
- `public static void ApplyAndPow(float[][] filterbank, float[] spectrum, float[] filtered, float power = 1.0f / 3)`
  - 将滤波器应用于频谱，然后对结果频谱执行 Pow(x, power)。例如，在 PLP 中：power=1/3（立方根）。
  - 参数:
    - `filterbank`: 滤波器组。
    - `spectrum`: 频谱。
    - `filtered`: 滤波后的信号频谱。
    - `power`: 幂，默认为 1.0f / 3。

### 代码示例
以下是一个使用 FilterBanks 类中 Triangular 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

public class FilterBanksExample
{
    public static void Main()
    {
        // 定义 FFT 大小、采样率和频率元组
        int fftSize = 512;
        int samplingRate = 16000;
        (float, float, float)[] frequencies = { (100, 200, 300), (200, 300, 400), (300, 400, 500) };

        // 生成三角形滤波器组权重
        float[][] filterBank = FilterBanks.Triangular(fftSize, samplingRate, frequencies);

        // 输出滤波器组权重
        for (int i = 0; i < filterBank.Length; i++)
        {
            Console.WriteLine($"Filter {i + 1}:");
            foreach (var weight in filterBank[i])
            {
                Console.WriteLine(weight);
            }
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Fda.Remez 类

Vorcyc.Mathematics.SignalProcessing.Filters.Fda.Remez 是一个基于 Remez (Parks-McClellan) 算法的最优等波纹滤波器设计器。

### 属性

#### 1. Order
- `public int Order { get; private set; }`
  - 获取滤波器阶数。

#### 2. Iterations
- `public int Iterations { get; private set; }`
  - 获取实际迭代次数。

#### 3. K
- `public int K { get; private set; }`
  - 获取极值频率的数量 (K = Order/2 + 2)。

#### 4. InterpolatedResponse
- `public float[] InterpolatedResponse { get; private set; }`
  - 获取插值频率响应。

#### 5. Error
- `public float[] Error { get; private set; }`
  - 获取误差数组。

#### 6. ExtremalFrequencies
- `public float[] ExtremalFrequencies`
  - 获取极值频率数组。

### 方法

#### 1. Remez
- `public Remez(int order, float[] frequencies, float[] desired, float[] weights, int gridDensity = 16)`
  - 构造 Remez 滤波器设计器。
  - 参数:
    - `order`: 滤波器阶数。
    - `frequencies`: 归一化频率数组。
    - `desired`: 给定频率下的期望响应值数组。
    - `weights`: 给定频率下的权重数组。
    - `gridDensity`: 网格密度，默认为 16。

#### 2. Design
- `public float[] Design(int maxIterations = 100)`
  - 设计最优等波纹滤波器并返回设计的滤波器的核。
  - 参数:
    - `maxIterations`: 最大迭代次数，默认为 100。
  - 返回值: 滤波器系数数组。

#### 3. DbToPassbandWeight
- `public static float DbToPassbandWeight(float ripple)`
  - 将波纹（以 dB 为单位）转换为通带权重。
  - 参数:
    - `ripple`: 波纹（以 dB 为单位）。
  - 返回值: 通带权重。

#### 4. DbToStopbandWeight
- `public static float DbToStopbandWeight(float ripple)`
  - 将波纹（以 dB 为单位）转换为阻带权重。
  - 参数:
    - `ripple`: 波纹（以 dB 为单位）。
  - 返回值: 阻带权重。

#### 5. EstimateOrder
- `public static int EstimateOrder(float fp, float fa, float dp, float da)`
  - 估计低通滤波器的阶数。
  - 参数:
    - `fp`: 通带边缘频率。
    - `fa`: 阻带边缘频率。
    - `dp`: 通带权重。
    - `da`: 阻带权重。
  - 返回值: 滤波器阶数。

#### 6. EstimateOrder
- `public static int EstimateOrder(float[] frequencies, float[] deltas)`
  - 估计具有自定义频带的滤波器的阶数。
  - 参数:
    - `frequencies`: 边缘频率数组。
    - `deltas`: 权重数组。
  - 返回值: 滤波器阶数。

### 代码示例
以下是一个使用 Remez 类中 Design 方法的示例，并在示例中加入了注释：

```csharp
using System;
using System.Text.RegularExpressions;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

public class RemezExample
{
    public static void Main()
    {
        // 定义滤波器阶数、频率、期望响应和权重
        int order = 57;
        float[] frequencies = { 0, 0.15f, 0.17f, 0.5f };
        float[] desired = { 1, 0 };
        float[] weights = { 0.01f, 0.1f };

        // 创建 Remez 实例
        var remez = new Remez(order, frequencies, desired, weights);

        // 设计滤波器并获取滤波器系数
        float[] kernel = remez.Design();

        // 输出滤波器系数
        foreach (var coeff in kernel)
        {
            Console.WriteLine($"Coefficient: {coeff}");
        }

        // 输出其他属性
        Console.WriteLine($"Iterations: {remez.Iterations}");
        Console.WriteLine($"Extremal Frequencies: {string.Join(", ", remez.ExtremalFrequencies)}");
        Console.WriteLine($"Interpolated Response: {string.Join(", ", remez.InterpolatedResponse)}");
        Console.WriteLine($"Error: {string.Join(", ", remez.Error)}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.Fda.VtlnWarper 类

Vorcyc.Mathematics.SignalProcessing.Filters.Fda.VtlnWarper 是一个用于实现类似于 Kaldi 实现的声道长度归一化 (VTLN) 的类。

### 属性

#### 1. _lowFreq
- `private readonly float _lowFreq`
  - 低频率。

#### 2. _highFreq
- `private readonly float _highFreq`
  - 高频率。

#### 3. _lowVtln
- `private readonly float _lowVtln`
  - VTLN 的低频率。

#### 4. _highVtln
- `private readonly float _highVtln`
  - VTLN 的高频率。

#### 5. _scale
- `private readonly float _scale`
  - 计算的中间参数。

#### 6. _scaleLeft
- `private readonly float _scaleLeft`
  - 计算的中间参数。

#### 7. _scaleRight
- `private readonly float _scaleRight`
  - 计算的中间参数。

### 方法

#### 1. VtlnWarper
- `public VtlnWarper(float alpha, float lowFrequency, float highFrequency, float lowVtln, float highVtln)`
  - 构造 VtlnWarper 实例。
  - 参数:
    - `alpha`: 扭曲因子。
    - `lowFrequency`: 低频率。
    - `highFrequency`: 高频率。
    - `lowVtln`: VTLN 的低频率。
    - `highVtln`: VTLN 的高频率。

#### 2. Warp
- `public float Warp(float frequency)`
  - 扭曲给定的频率。
  - 参数:
    - `frequency`: 要扭曲的频率。
  - 返回值: 扭曲后的频率。

### 代码示例
以下是一个使用 VtlnWarper 类中 Warp 方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;

public class VtlnWarperExample
{
    public static void Main()
    {
        // 定义扭曲因子和频率范围
        float alpha = 1.2f;
        float lowFrequency = 100f;
        float highFrequency = 8000f;
        float lowVtln = 200f;
        float highVtln = 6000f;

        // 创建 VtlnWarper 实例
        var vtlnWarper = new VtlnWarper(alpha, lowFrequency, highFrequency, lowVtln, highVtln);

        // 扭曲频率并输出结果
        float[] frequencies = { 150f, 300f, 500f, 1000f, 4000f, 7000f };
        foreach (var freq in frequencies)
        {
            float warpedFreq = vtlnWarper.Warp(freq);
            Console.WriteLine($"Original Frequency: {freq}, Warped Frequency: {warpedFreq}");
        }
    }
}
```

---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.OnePole 命名空间。


## Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.HighPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.HighPassFilter 是一个用于实现单极高通滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; protected set; }`
  - 获取截止频率。

### 方法

#### 1. HighPassFilter
- `public HighPassFilter(float frequency)`
  - 构造 HighPassFilter 实例。
  - 参数:
    - `frequency`: 截止频率。

#### 2. Change
- `public void Change(float frequency)`
  - 更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 截止频率。

### 私有方法

#### 1. SetCoefficients
- `private void SetCoefficients(float frequency)`
  - 根据给定的截止频率设置滤波器系数。
  - 参数:
    - `frequency`: 截止频率。

### 代码示例
以下是一个使用 HighPassFilter 类中 Change 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.OnePole;

public class HighPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率
        float frequency = 0.1f;

        // 创建 HighPassFilter 实例
        var highPassFilter = new HighPassFilter(frequency);

        // 更改截止频率
        highPassFilter.Change(0.2f);

        // 输出当前截止频率
        Console.WriteLine($"Current Cutoff Frequency: {highPassFilter.Frequency}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.LowPassFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.LowPassFilter 是一个用于实现单极低通滤波器的类。

### 属性

#### 1. Frequency
- `public float Frequency { get; protected set; }`
  - 获取截止频率。

### 方法

#### 1. LowPassFilter
- `public LowPassFilter(float frequency)`
  - 构造 LowPassFilter 实例。
  - 参数:
    - `frequency`: 截止频率。

#### 2. Change
- `public void Change(float frequency)`
  - 更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `frequency`: 截止频率。

### 私有方法

#### 1. SetCoefficients
- `private void SetCoefficients(float frequency)`
  - 根据给定的截止频率设置滤波器系数。
  - 参数:
    - `frequency`: 截止频率。

### 代码示例
以下是一个使用 LowPassFilter 类中 Change 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.OnePole;

public class LowPassFilterExample
{
    public static void Main()
    {
        // 定义截止频率
        float frequency = 0.1f;

        // 创建 LowPassFilter 实例
        var lowPassFilter = new LowPassFilter(frequency);

        // 更改截止频率
        lowPassFilter.Change(0.2f);

        // 输出当前截止频率
        Console.WriteLine($"Current Cutoff Frequency: {lowPassFilter.Frequency}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.OnePoleFilter 类

`Vorcyc.Mathematics.SignalProcessing.Filters.OnePole.OnePoleFilter` 是一个单极 IIR 滤波器类。

### 方法

#### 1. OnePoleFilter 构造函数
- `protected OnePoleFilter()`
  - 构造 `OnePoleFilter` 实例，使用默认的滤波器系数。

#### 2. OnePoleFilter 构造函数
- `public OnePoleFilter(float b, float a)`
  - 构造 `OnePoleFilter` 实例，使用指定的滤波器系数。
  - 参数:
    - `b`: 分子系数。
    - `a`: 极点。

#### 3. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. Reset
- `public override void Reset()`
  - 重置滤波器。

### 示例

以下是一个使用 OnePoleFilter 类构建和处理样本的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.OnePole;

public class OnePoleFilterExample
{
    public static void Main()
    {
        // 创建 OnePoleFilter 实例
        var filter = new OnePoleFilter(0.5f, 0.3f);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 处理样本
        Console.WriteLine("Processed Samples:");
        foreach (var sample in samples)
        {
            var processedSample = filter.Process(sample);
            Console.WriteLine(processedSample);
        }

        // 重置滤波器
        filter.Reset();
    }
}

```

---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters.Polyphase 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Filters.Polyphase.PolyphaseSystem 类

Vorcyc.Mathematics.SignalProcessing.Filters.Polyphase.PolyphaseSystem 是一个多相滤波器系统类。

### 属性

#### 1. Filters
- `public FirFilter[] Filters { get; private set; }`
  - 获取传递函数为 E(z^k) 的多相滤波器。

#### 2. MultirateFilters
- `public FirFilter[] MultirateFilters { get; private set; }`
  - 获取用于多速率处理的传递函数为 E(z) 的多相滤波器。

### 方法

#### 1. PolyphaseSystem 构造函数
- `public PolyphaseSystem(float[] kernel, int n, int type = 1)`
  - 构造 PolyphaseSystem 实例，使用指定的滤波器核和滤波器数量。
  - 参数:
    - `kernel`: 滤波器核。
    - `n`: 多相滤波器的数量。
    - `type`: 多相系统类型（1 或 2）。

#### 2. Decimate
- `public DiscreteSignal Decimate(DiscreteSignal signal)`
  - 对输入信号进行多相抽取（适用于类型 I 系统）。
  - 参数:
    - `signal`: 输入信号。
  - 返回值: 抽取后的信号。

#### 3. Interpolate
- `public DiscreteSignal Interpolate(DiscreteSignal signal)`
  - 对输入信号进行多相插值（适用于类型 II 系统）。
  - 参数:
    - `signal`: 输入信号。
  - 返回值: 插值后的信号。

#### 4. Process
- `public float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 5. Reset
- `public void Reset()`
  - 重置多相滤波器。

#### 6. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 处理整个信号并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

### 示例

以下是一个使用 PolyphaseSystem 类构建处理样本的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters.Polyphase;

public class PolyphaseSystemExample
{
    public static void Main()
    {
        // 定义滤波器核
        float[] kernel = { 1, 2, 3, 4, 3, 2, 1 };

        // 创建 PolyphaseSystem 实例
        var polyphaseSystem = new PolyphaseSystem(kernel, 3);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 进行多相抽取
        var decimatedSignal = polyphaseSystem.Decimate(signal);

        // 输出抽取后的信号样本
        Console.WriteLine("Decimated Signal:");
        foreach (var sample in decimatedSignal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 进行多相插值
        var interpolatedSignal = polyphaseSystem.Interpolate(signal);

        // 输出插值后的信号样本
        Console.WriteLine("Interpolated Signal:");
        foreach (var sample in interpolatedSignal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 重置多相滤波器
        polyphaseSystem.Reset();
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.CombFeedbackFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.CombFeedbackFilter 是一个用于实现反馈梳状滤波器的类。

### 属性

无公开属性。

### 方法

#### 1. CombFeedbackFilter 构造函数
- `public CombFeedbackFilter(int m, float b0 = 1.0f, float am = 0.6f)`
  - 构造 CombFeedbackFilter 实例。
  - 参数:
    - `m`: 延迟。
    - `b0`: 系数 b0，默认为 1.0f。
    - `am`: 系数 am，默认为 0.6f。

#### 2. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. ApplyTo
- `public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 4. Change
- `public void Change(float b0, float am)`
  - 更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `b0`: 系数 b0。
    - `am`: 系数 am。

### 代码示例
以下是一个使用 CombFeedbackFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class CombFeedbackFilterExample
{
    public static void Main()
    {
        // 创建 CombFeedbackFilter 实例
        var combFilter = new CombFeedbackFilter(4, 1.0f, 0.6f);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, inputSignal);

        // 处理信号并输出结果
        foreach (var sample in signal.Samples)
        {
            var processedSample = combFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }

        // 更改滤波器系数
        combFilter.Change(0.8f, 0.4f);

        // 处理信号并输出结果
        foreach (var sample in signal.Samples)
        {
            var processedSample = combFilter.Process(sample);
            Console.WriteLine($"Processed Sample after change: {processedSample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.CombFeedforwardFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.CombFeedforwardFilter 是一个用于实现前馈梳状滤波器的类。

### 属性

无公开属性。

### 方法

#### 1. CombFeedforwardFilter 构造函数
- `public CombFeedforwardFilter(int m, float b0 = 1f, float bm = 0.5f, bool normalize = true)`
  - 构造 CombFeedforwardFilter 实例。
  - 参数:
    - `m`: 延迟。
    - `b0`: 系数 b0，默认为 1f。
    - `bm`: 系数 bm，默认为 0.5f。
    - `normalize`: 是否归一化频率响应，默认为 true。

#### 2. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. ApplyTo
- `public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 4. Change
- `public void Change(float b0, float bm)`
  - 更改滤波器系数（保持滤波器状态）。
  - 参数:
    - `b0`: 系数 b0。
    - `bm`: 系数 bm。

### 代码示例
以下是一个使用 CombFeedforwardFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class CombFeedforwardFilterExample
{
    public static void Main()
    {
        // 创建 CombFeedforwardFilter 实例
        var combFilter = new CombFeedforwardFilter(4, 1f, 0.5f);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, inputSignal);

        // 处理信号并输出结果
        foreach (var sample in signal.Samples)
        {
            var processedSample = combFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }

        // 更改滤波器系数
        combFilter.Change(0.8f, 0.4f);

        // 处理信号并输出结果
        foreach (var sample in signal.Samples)
        {
            var processedSample = combFilter.Process(sample);
            Console.WriteLine($"Processed Sample after change: {processedSample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.DcRemovalFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.DcRemovalFilter 是一个用于实现直流偏移去除的 IIR 滤波器类。

### 属性

无公开属性。

### 方法

#### 1. DcRemovalFilter 构造函数
- `public DcRemovalFilter(float r = 0.995f)`
  - 构造 DcRemovalFilter 实例。
  - 参数:
    - `r`: R 系数，通常在 [0.9, 1] 范围内，默认为 0.995f。

#### 2. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置滤波器。

### 代码示例
以下是一个使用 DcRemovalFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class DcRemovalFilterExample
{
    public static void Main()
    {
        // 创建 DcRemovalFilter 实例
        var dcRemovalFilter = new DcRemovalFilter(0.995f);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            var processedSample = dcRemovalFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }

        // 重置滤波器
        dcRemovalFilter.Reset();
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.Filters.DeEmphasisFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.DeEmphasisFilter 是一个用于实现去加重的 IIR 滤波器类。

### 属性

无公开属性。

### 方法

#### 1. DeEmphasisFilter 构造函数
- `public DeEmphasisFilter(float a = 0.97f, bool normalize = false)`
  - 构造 DeEmphasisFilter 实例。
  - 参数:
    - `a`: 去加重系数，默认为 0.97f。
    - `normalize`: 是否归一化频率响应，默认为 false。

### 代码示例
以下是一个使用 DeEmphasisFilter 类中构造函数的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class DeEmphasisFilterExample
{
    public static void Main()
    { 
        // 创建 DeEmphasisFilter 实例
        var deEmphasisFilter = new DeEmphasisFilter(0.97f, true);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            var processedSample = deEmphasisFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }

        // 重置滤波器
        deEmphasisFilter.Reset();
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.Filters.HilbertFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.HilbertFilter 是一个用于实现 Hilbert 滤波器的类。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取滤波器的大小。

### 方法

#### 1. HilbertFilter 构造函数
- `public HilbertFilter(int size = 128)`
  - 构造 HilbertFilter 实例。
  - 参数:
    - `size`: 滤波器的大小，默认为 128。

#### 2. MakeKernel
- `private static IEnumerable<float> MakeKernel(int size)`
  - 生成给定大小的滤波器核。
  - 参数:
    - `size`: 核的大小。
  - 返回值: 滤波器核的浮点数数组。

### 代码示例
以下是一个使用 HilbertFilter 类中构造函数的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class HilbertFilterExample
{
    public static void Main()
    {
        // 创建 HilbertFilter 实例
        var hilbertFilter = new HilbertFilter(128);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            var processedSample = hilbertFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.MedianFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.MedianFilter 是一个用于实现中值滤波器的类。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取中值滤波器的大小。

### 方法

#### 1. MedianFilter 构造函数
- `public MedianFilter(int size = 9)`
  - 构造 MedianFilter 实例。
  - 参数:
    - `size`: 滤波器的大小，默认为 9。

#### 2. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 3. Process
- `public float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. Reset
- `public void Reset()`
  - 重置滤波器。

### 代码示例
以下是一个使用 MedianFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class MedianFilterExample
{
    public static void Main()
    {
        // 创建 MedianFilter 实例
        var medianFilter = new MedianFilter(9);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            var processedSample = medianFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }

        // 重置滤波器
        medianFilter.Reset();
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.MedianFilter2 类

Vorcyc.Mathematics.SignalProcessing.Filters.MedianFilter2 是一个用于实现中值滤波器的类。它的实现比 `MedianFilter` 类稍快，但仅适用于较小的滤波器尺寸（不超过 5 左右）。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取中值滤波器的大小。

### 方法

#### 1. MedianFilter2 构造函数
- `public MedianFilter2(int size = 9)`
  - 构造 MedianFilter2 实例。
  - 参数:
    - `size`: 滤波器的大小，默认为 9。

#### 2. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 3. Process
- `public float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. Reset
- `public void Reset()`
  - 重置滤波器。

### 代码示例
以下是一个使用 MedianFilter2 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class MedianFilter2Example
{
    public static void Main()
    {
        // 创建 MedianFilter2 实例
        var medianFilter2 = new MedianFilter2(9);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            var processedSample = medianFilter2.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }

        // 重置滤波器
        medianFilter2.Reset();
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.MovingAverageFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.MovingAverageFilter 是一个用于实现非递归移动平均滤波器的类。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取滤波器的大小。

### 方法

#### 1. MovingAverageFilter 构造函数
- `public MovingAverageFilter(int size = 9)`
  - 构造 MovingAverageFilter 实例。
  - 参数:
    - `size`: 滤波器的大小，默认为 9。

#### 2. MakeKernel
- `private static IEnumerable<float> MakeKernel(int size)`
  - 生成给定大小的滤波器核。
  - 参数:
    - `size`: 核的大小。
  - 返回值: 滤波器核的浮点数数组。

### 代码示例
以下是一个使用 MovingAverageFilter 类中构造函数的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class MovingAverageFilterExample
{
    public static void Main()
    {
        // 创建 MovingAverageFilter 实例
        var movingAverageFilter = new MovingAverageFilter(9);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            var processedSample = movingAverageFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.MovingAverageRecursiveFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.MovingAverageRecursiveFilter 是一个用于实现快速递归移动平均滤波器的类。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取滤波器的大小。

### 方法

#### 1. MovingAverageRecursiveFilter 构造函数
- `public MovingAverageRecursiveFilter(int size = 9)`
  - 构造 MovingAverageRecursiveFilter 实例。
  - 参数:
    - `size`: 滤波器的大小，默认为 9。

#### 2. MakeNumerator
- `private static float[] MakeNumerator(int size)`
  - 生成给定大小的滤波器分子。
  - 参数:
    - `size`: 分子的大小。
  - 返回值: 滤波器分子的浮点数数组。

#### 3. ApplyTo
- `public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 4. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 5. Reset
- `public override void Reset()`
  - 重置滤波器。

### 代码示例
以下是一个使用 MovingAverageRecursiveFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class MovingAverageRecursiveFilterExample
{
    public static void Main()
    {
        // 创建 MovingAverageRecursiveFilter 实例
        var movingAverageRecursiveFilter = new MovingAverageRecursiveFilter(9);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, inputSignal);

        // 处理信号并输出结果
        foreach (var sample in signal.Samples)
        {
            var processedSample = movingAverageRecursiveFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }

        // 重置滤波器
        movingAverageRecursiveFilter.Reset();
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.PreEmphasisFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.PreEmphasisFilter 是一个用于实现预加重 FIR 滤波器的类。

### 属性

无公开属性。

### 方法

#### 1. PreEmphasisFilter 构造函数
- `public PreEmphasisFilter(float a = 0.97f, bool normalize = false)`
  - 构造 PreEmphasisFilter 实例。
  - 参数:
    - `a`: 预加重系数，默认为 0.97f。
    - `normalize`: 是否归一化频率响应，默认为 false。

#### 2. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. ApplyTo
- `public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 4. Reset
- `public override void Reset()`
  - 重置滤波器。

### 代码示例
以下是一个使用 PreEmphasisFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class PreEmphasisFilterExample
{
    public static void Main()
    {
        // 创建 PreEmphasisFilter 实例
        var preEmphasisFilter = new PreEmphasisFilter(0.97f, true);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, inputSignal);

        // 处理信号并输出结果
        foreach (var sample in signal.Samples)
        {
            var processedSample = preEmphasisFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }

        // 重置滤波器
        preEmphasisFilter.Reset();
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.Filters.RastaFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.RastaFilter 是一个用于实现 RASTA 滤波器（用于鲁棒语音处理）的 IIR 滤波器类。

### 属性

无公开属性。

### 方法

#### 1. RastaFilter 构造函数
- `public RastaFilter(float pole = 0.98f)`
  - 构造 RastaFilter 实例。
  - 参数:
    - `pole`: 极点，默认为 0.98f。

### 代码示例
以下是一个使用 RastaFilter 类中构造函数的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class RastaFilterExample
{
    public static void Main()
    {
        // 创建 RastaFilter 实例
        var rastaFilter = new RastaFilter(0.98f);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            var processedSample = rastaFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.SavitzkyGolayFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.SavitzkyGolayFilter 是一个用于实现 Savitzky-Golay 滤波器的 FIR 滤波器类。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取滤波器的大小。

### 方法

#### 1. SavitzkyGolayFilter 构造函数
- `public SavitzkyGolayFilter(int size, int deriv = 0)`
  - 构造 SavitzkyGolayFilter 实例。
  - 参数:
    - `size`: 滤波器的大小（必须是范围 [5..31] 内的奇数）。
    - `deriv`: 导数（必须是 0、1 或 2），默认为 0。

### 代码示例
以下是一个使用 SavitzkyGolayFilter 类中构造函数的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class SavitzkyGolayFilterExample
{
    public static void Main()
    {
        // 创建 SavitzkyGolayFilter 实例
        var sgFilter = new SavitzkyGolayFilter(11, 0);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            var processedSample = sgFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.ThiranFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.ThiranFilter 是一个用于实现 N 阶 Thiran 全通插值滤波器的 IIR 滤波器类。

### 属性

无公开属性。

### 方法

#### 1. ThiranFilter 构造函数
- `public ThiranFilter(int order, float delta)`
  - 构造 ThiranFilter 实例。
  - 参数:
    - `order`: 滤波器阶数。
    - `delta`: 分数延迟。

#### 2. MakeTf
- `private static TransferFunction MakeTf(int order, float delta)`
  - 生成传递函数。
  - 参数:
    - `order`: 滤波器阶数。
    - `delta`: 分数延迟。
  - 返回值: 生成的 `TransferFunction` 对象。

#### 3. ThiranCoefficient
- `private static float ThiranCoefficient(int k, int n, float delta)`
  - 计算传递函数分母的第 k 个系数。
  - 参数:
    - `k`: 系数索引。
    - `n`: 滤波器阶数。
    - `delta`: 分数延迟。
  - 返回值: 计算的系数。

### 代码示例
以下是一个使用 ThiranFilter 类中构造函数的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class ThiranFilterExample
{
    public static void Main()
    {
        // 定义滤波器阶数和分数延迟
        int order = 13; float delta = 13.4f;

        // 创建 ThiranFilter 实例
        var thiranFilter = new ThiranFilter(order, delta);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 处理信号并输出结果
        foreach (var sample in inputSignal)
        {
            var processedSample = thiranFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Filters.WienerFilter 类

Vorcyc.Mathematics.SignalProcessing.Filters.WienerFilter 是一个用于实现维纳滤波器的类。其实现与 `scipy.signal.wiener()` 相同。

### 属性

无公开属性。

### 方法

#### 1. WienerFilter 构造函数
- `public WienerFilter(int size = 3, float noise = 0.0f)`
  - 构造 WienerFilter 实例。
  - 参数:
    - `size`: 维纳滤波器的大小，默认为 3。
    - `noise`: 估计的噪声功率，默认为 0.0f。

#### 2. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将滤波器应用于整个信号，并返回新的滤波信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 3. Process
- `public float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. Reset
- `public void Reset()`
  - 重置滤波器。

### 代码示例
以下是一个使用 WienerFilter 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Filters;

public class WienerFilterExample
{
    public static void Main()
    {
        // 创建 WienerFilter 实例
        var wienerFilter = new WienerFilter(3, 0.0f);

        // 定义输入信号
        float[] inputSignal = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, inputSignal);

        // 处理信号并输出结果
        foreach (var sample in signal.Samples)
        {
            var processedSample = wienerFilter.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }

        // 重置滤波器
        wienerFilter.Reset();
    }
}
```