# 统计模块 - Statistics Module

Vorcyc.Mathematics.Statistics类是一个全面的工具类，用于对数值数据进行统计分析，支持多种数据结构和数值类型。它包括查找极值、计算总和、平均值、方差，以及识别最大值和最小值及其索引的方法。该类在可能的情况下利用硬件加速以优化性能。

> 本模块的所有类型均的根命名空间为：Vorcyc.Mathematics.SignalProcessing

---

> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Effects

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Base.AudioEffect 类](#vorcycmathematicssignalprocessingeffectsbaseaudioeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Base.WetDryMixer 类](#vorcycmathematicssignalprocessingeffectsbasewetdrymixer-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.BinauralPanEffect 类](#vorcycmathematicssignalprocessingeffectsstereobinauralpaneffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.ItdIldPanEffect 类](#vorcycmathematicssignalprocessingeffectsstereoitdildpaneffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.PanEffect 类](#vorcycmathematicssignalprocessingeffectsstereopaneffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.PingPongDelayEffect 类](#vorcycmathematicssignalprocessingeffectsstereopingpongdelayeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.StereoDelayEffect 类](#vorcycmathematicssignalprocessingeffectsstereostereodelayeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.StereoEffect 类](#vorcycmathematicssignalprocessingeffectsstereostereoeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.BitCrusherEffect 类](#vorcycmathematicssignalprocessingeffectsbitcrushereffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.ChorusEffect 类](#vorcycmathematicssignalprocessingeffectschoruseffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.DelayEffect 类](#vorcycmathematicssignalprocessingeffectsdelayeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.DistortionEffect 类](#vorcycmathematicssignalprocessingeffectsdistortioneffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.EchoEffect 类](#vorcycmathematicssignalprocessingeffectsechoeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.FlangerEffect 类](#vorcycmathematicssignalprocessingeffectsflangereffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.MorphEffect 类](#vorcycmathematicssignalprocessingeffectsmorpheffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.PhaserEffect 类](#vorcycmathematicssignalprocessingeffectsphasereffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.PitchShiftEffect 类](#vorcycmathematicssignalprocessingeffectspitchshifteffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.PitchShiftVocoderEffect 类](#vorcycmathematicssignalprocessingeffectspitchshiftvocodereffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.RobotEffect 类](#vorcycmathematicssignalprocessingeffectsroboteffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.TremoloEffect 类](#vorcycmathematicssignalprocessingeffectstremoloeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.TubeDistortionEffect 类](#vorcycmathematicssignalprocessingeffectstubedistortioneffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.VibratoEffect 类](#vorcycmathematicssignalprocessingeffectsvibratoeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.WahwahEffect 类](#vorcycmathematicssignalprocessingeffectswahwaheffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.WhisperEffect 类](#vorcycmathematicssignalprocessingeffectswhispereffect-类)  



---

> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Effects

## Vorcyc.Mathematics.SignalProcessing.Effects.Base.AudioEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.Base.AudioEffect 是一个用于音频效果的抽象类，继承了 WetDryMixer 类，并实现了 IFilter 和 IOnlineFilter 接口。

### 方法

#### 1. Process
- `public abstract float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 2. Reset
- `public abstract void Reset()`
  - 重置效果。

#### 3. ApplyTo
- `public virtual DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将效果应用于整个信号，并返回处理后的新信号。
  - 参数:
    - `signal`: 信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 4. Apply
- `public virtual void Apply(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将效果应用于整个信号（就地处理）。
  - 参数:
    - `signal`: 信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。

### 代码示例
以下是一个使用 AudioEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects.Base;

public class CustomAudioEffect : AudioEffect
{
    public override float Process(float sample)
    { // 自定义处理逻辑
        return sample * 0.5f;
    }
    public override void Reset()
    {
        // 自定义重置逻辑
    }
}
public class AudioEffectExample
{
    public static void Main()
    {
        // 定义信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(1000, samples);

        // 创建自定义音频效果实例
        var effect = new CustomAudioEffect();

        // 将效果应用于整个信号
        var processedSignal = effect.ApplyTo(signal);

        // 输出处理后的信号样本
        Console.WriteLine("Processed Signal:");
        foreach (var sample in processedSignal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.Base.WetDryMixer 类

Vorcyc.Mathematics.SignalProcessing.Effects.Base.WetDryMixer 是一个实现了湿/干混合逻辑的基类，实现了 IMixable 接口。

### 属性

#### 1. Wet
- `public float Wet { get; set; } = 1f`
  - 获取或设置湿增益（默认值为 1）。

#### 2. Dry
- `public float Dry { get; set; } = 0f`
  - 获取或设置干增益（默认值为 0）。

### 方法

#### 1. WetDryMix
- `public void WetDryMix(float mix, MixingRule mixingRule = MixingRule.Linear)`
  - 设置湿/干混合（范围 [0..1]）。
  - 参数:
    - `mix`: 湿/干混合比例。
    - `mixingRule`: 混合规则，默认为 `MixingRule.Linear`。

#### 2. WetDryDb
- `public void WetDryDb(float wetDb, float dryDb)`
  - 设置湿/干增益（以分贝为单位）并应用线性混合规则。
  - 参数:
    - `wetDb`: 湿增益（以分贝为单位）。
    - `dryDb`: 干增益（以分贝为单位）。

### 代码示例
以下是一个使用 WetDryMixer 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects.Base;

public class CustomWetDryMixer : WetDryMixer
{
    public void ApplyEffect(float[] samples)
    {
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = Wet * samples[i] + Dry * samples[i];
        }
    }
}
public class WetDryMixerExample
{
    public static void Main()
    { // 定义信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };

        // 创建自定义湿/干混合器实例
        var mixer = new CustomWetDryMixer();

        // 设置湿/干混合比例
        mixer.WetDryMix(0.7f, MixingRule.Sin3Db);

        // 应用效果
        mixer.ApplyEffect(samples);

        // 输出处理后的信号样本
        Console.WriteLine("Processed Signal:");
        foreach (var sample in samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.BinauralPanEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.BinauralPanEffect 是一个用于双耳声像处理的音频效果类，支持 HRIR/BRIR 插值和可选的分频滤波器。

### 属性

#### 1. Azimuth
- `public float Azimuth { get; set; }`
  - 获取或设置方位角（theta）。

#### 2. Elevation
- `public float Elevation { get; set; }`
  - 获取或设置仰角（phi）。

### 方法

#### 1. BinauralPanEffect 构造函数
- `public BinauralPanEffect(float[] azimuths, float[] elevations, float[][][] leftHrirs, float[][][] rightHrirs)`
  - 构造 BinauralPanEffect 实例。
  - 参数:
    - `azimuths`: 方位角数组（必须按升序排序）。
    - `elevations`: 仰角数组（必须按升序排序）。
    - `leftHrirs`: 左耳 HRIR 集合。
    - `rightHrirs`: 右耳 HRIR 集合。

#### 2. UseCrossover
- `public void UseCrossover(bool useCrossover)`
  - 打开或关闭分频滤波。
  - 参数:
    - `useCrossover`: 是否使用分频滤波。

#### 3. SetCrossoverParameters
- `public void SetCrossoverParameters(float freq, int samplingRate)`
  - 更新分频滤波器的频率（仅适用于 BiQuadFilters）。
  - 参数:
    - `freq`: 频率。
    - `samplingRate`: 采样率。

#### 4. SetCrossoverFilters
- `public void SetCrossoverFilters(IOnlineFilter lowpassLeft, IOnlineFilter highpassLeft, IOnlineFilter lowpassRight, IOnlineFilter highpassRight)`
  - 设置自定义分频滤波器。
  - 参数:
    - `lowpassLeft`: 左声道的低通滤波器。
    - `highpassLeft`: 左声道的高通滤波器。
    - `lowpassRight`: 右声道的低通滤波器。
    - `highpassRight`: 右声道的高通滤波器。

#### 5. UpdateHrir
- `protected void UpdateHrir(float azimuth, float elevation)`
  - 更新 HRIR（使用 HRIR 表进行插值）。
  - 参数:
    - `azimuth`: 方位角（theta）。
    - `elevation`: 仰角（phi）。

#### 6. Process
- `public override void Process(ref float left, ref float right)`
  - 处理每个声道的一个样本：[输入左声道，输入右声道] -> [输出左声道，输出右声道]。
  - 参数:
    - `left`: 左声道输入样本。
    - `right`: 右声道输入样本。

#### 7. Reset
- `public override void Reset()`
  - 重置双耳声像效果。

### 代码示例
以下是一个使用 BinauralPanEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects.Stereo;

public class BinauralPanEffectExample
{
    public static void Main()
    {
        // 定义 HRIR 数据
        float[] azimuths = { -80, -65, -55, -45, -40, -35, -30, -25, -20, -15, -10, -5, 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 55, 65, 80 };
        float[] elevations = { -45, -39, -34, -28, -23, -17, -11, -6, 0, 6, 11, 17, 23, 28, 34, 39, 45, 51, 56, 62, 68, 73, 79, 84, 90, 96, 101, 107, 113, 118, 124, 129, 135, 141, 146, 152, 158, 163, 169, 174, 180, 186, 191, 197, 203, 208, 214, 219, 225, 231 };
        float[][][] leftHrirs = new float[azimuths.Length][][];
        float[][][] rightHrirs = new float[azimuths.Length][][];

        // 初始化 HRIR 数据（示例数据）
        for (int i = 0; i < azimuths.Length; i++)
        {
            leftHrirs[i] = new float[elevations.Length][];
            rightHrirs[i] = new float[elevations.Length][];
            for (int j = 0; j < elevations.Length; j++)
            {
                leftHrirs[i][j] = new float[128]; // 示例长度
                rightHrirs[i][j] = new float[128]; // 示例长度
            }
        }

        // 创建 BinauralPanEffect 实例
        var binauralPanEffect = new BinauralPanEffect(azimuths, elevations, leftHrirs, rightHrirs);

        // 设置方位角和仰角
        binauralPanEffect.Azimuth = 30;
        binauralPanEffect.Elevation = 45;

        // 打开分频滤波
        binauralPanEffect.UseCrossover(true);

        // 处理样本
        float leftSample = 0.5f;
        float rightSample = 0.6f;
        binauralPanEffect.Process(ref leftSample, ref rightSample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Left Sample: {leftSample}");
        Console.WriteLine($"Processed Right Sample: {rightSample}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.ItdIldPanEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.ItdIldPanEffect 是一个基于 ITD-ILD（Interaural Time Difference - Interaural Level Difference）的立体声声像音频效果类。

### 属性

#### 1. HeadRadius
- `public float HeadRadius { get; }`
  - 获取头部半径。

#### 2. Pan
- `public float Pan { get; set; }`
  - 获取或设置声像。

### 方法

#### 1. ItdIldPanEffect 构造函数
- `public ItdIldPanEffect(int samplingRate, float pan, InterpolationMode interpolationMode = InterpolationMode.Linear, double reserveDelay = 0.005, float headRadius = 8.5e-2f)`
  - 构造 ItdIldPanEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `pan`: 声像。
    - `interpolationMode`: 分数延迟线的插值模式，默认为 `InterpolationMode.Linear`。
    - `reserveDelay`: 保留延迟的最大时间（以秒为单位），默认为 0.005 秒。
    - `headRadius`: 头部半径，默认为 8.5e-2f。

#### 2. Process
- `public override void Process(ref float left, ref float right)`
  - 处理每个声道的一个样本：[输入左声道，输入右声道] -> [输出左声道，输出右声道]。
  - 参数:
    - `left`: 左声道输入样本。
    - `right`: 右声道输入样本。

#### 3. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 ItdIldPanEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects.Stereo;

public class ItdIldPanEffectExample
{
    public static void Main()
    {
        // 创建 ItdIldPanEffect 实例
        var itdIldPanEffect = new ItdIldPanEffect(44100, 0.5f);

        // 设置声像
        itdIldPanEffect.Pan = 0.8f;

        // 处理样本
        float leftSample = 0.5f;
        float rightSample = 0.6f;
        itdIldPanEffect.Process(ref leftSample, ref rightSample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Left Sample: {leftSample}");
        Console.WriteLine($"Processed Right Sample: {rightSample}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.PanEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.PanEffect 是一个用于立体声声像处理的音频效果类。

### 属性

#### 1. Pan
- `public float Pan { get; set; }`
  - 获取或设置声像（范围 [-1..1]）。

#### 2. PanRule
- `public PanRule PanRule { get; set; }`
  - 获取或设置声像规则（声像定律）。

### 方法

#### 1. PanEffect 构造函数
- `public PanEffect(float pan, PanRule panRule)`
  - 构造 PanEffect 实例。
  - 参数:
    - `pan`: 声像。
    - `panRule`: 声像规则（声像定律）。

#### 2. Process
- `public override void Process(ref float left, ref float right)`
  - 处理每个声道的一个样本：[输入左声道，输入右声道] -> [输出左声道，输出右声道]。
  - 参数:
    - `left`: 左声道输入样本。
    - `right`: 右声道输入样本。

#### 3. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 PanEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects.Stereo;

public class PanEffectExample
{
    public static void Main()
    {
        // 创建 PanEffect 实例
        var panEffect = new PanEffect(0.5f, PanRule.ConstantPower);

        // 设置声像
        panEffect.Pan = -0.3f;

        // 处理样本
        float leftSample = 0.5f;
        float rightSample = 0.6f;
        panEffect.Process(ref leftSample, ref rightSample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Left Sample: {leftSample}");
        Console.WriteLine($"Processed Right Sample: {rightSample}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.PingPongDelayEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.PingPongDelayEffect 是一个用于立体声乒乓延迟音频效果的类。

### 属性

#### 1. Pan
- `public float Pan { get; set; }`
  - 获取或设置声像。

#### 2. Delay
- `public float Delay { get; set; }`
  - 获取或设置延迟时间（以秒为单位）。

#### 3. Feedback
- `public float Feedback { get; set; }`
  - 获取或设置反馈系数。

### 方法

#### 1. PingPongDelayEffect 构造函数
- `public PingPongDelayEffect(int samplingRate, float pan, float delay, float feedback = 0.5f, InterpolationMode interpolationMode = InterpolationMode.Nearest, float reserveDelay = 0)`
  - 构造 PingPongDelayEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `pan`: 声像。
    - `delay`: 延迟时间（以秒为单位）。
    - `feedback`: 反馈系数，默认为 0.5。
    - `interpolationMode`: 分数延迟线的插值模式，默认为 `InterpolationMode.Nearest`。
    - `reserveDelay`: 保留延迟的最大时间，默认为 0 秒。

#### 2. Process
- `public override void Process(ref float left, ref float right)`
  - 处理每个声道的一个样本：[输入左声道，输入右声道] -> [输出左声道，输出右声道]。
  - 参数:
    - `left`: 左声道输入样本。
    - `right`: 右声道输入样本。

#### 3. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 PingPongDelayEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects.Stereo;

public class PingPongDelayEffectExample
{
    public static void Main()
    {
        // 创建 PingPongDelayEffect 实例
        var pingPongDelayEffect = new PingPongDelayEffect(44100, 0.5f, 0.3f, 0.6f);

        // 设置声像和延迟
        pingPongDelayEffect.Pan = 0.8f;
        pingPongDelayEffect.Delay = 0.4f;
        pingPongDelayEffect.Feedback = 0.7f;

        // 处理样本
        float leftSample = 0.5f;
        float rightSample = 0.6f;
        pingPongDelayEffect.Process(ref leftSample, ref rightSample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Left Sample: {leftSample}");
        Console.WriteLine($"Processed Right Sample: {rightSample}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.StereoDelayEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.StereoDelayEffect 是一个用于立体声延迟音频效果的类。

### 属性

#### 1. DelayLeft
- `public float DelayLeft { get; set; }`
  - 获取或设置左声道延迟时间（以秒为单位）。

#### 2. DelayRight
- `public float DelayRight { get; set; }`
  - 获取或设置右声道延迟时间（以秒为单位）。

#### 3. FeedbackLeft
- `public float FeedbackLeft { get; set; }`
  - 获取或设置左声道反馈系数。

#### 4. FeedbackRight
- `public float FeedbackRight { get; set; }`
  - 获取或设置右声道反馈系数。

#### 5. Pan
- `public float Pan { get; set; }`
  - 获取或设置声像。

### 方法

#### 1. StereoDelayEffect 构造函数
- `public StereoDelayEffect(int samplingRate, float pan, float delayLeft, float delayRight, float feedbackLeft = 0.5f, float feedbackRight = 0.5f, InterpolationMode interpolationMode = InterpolationMode.Nearest, float reserveDelay = 0)`
  - 构造 StereoDelayEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `pan`: 声像。
    - `delayLeft`: 左声道延迟时间（以秒为单位）。
    - `delayRight`: 右声道延迟时间（以秒为单位）。
    - `feedbackLeft`: 左声道反馈系数，默认为 0.5。
    - `feedbackRight`: 右声道反馈系数，默认为 0.5。
    - `interpolationMode`: 分数延迟线的插值模式，默认为 `InterpolationMode.Nearest`。
    - `reserveDelay`: 保留延迟的最大时间，默认为 0 秒。

#### 2. Process
- `public override void Process(ref float left, ref float right)`
  - 处理每个声道的一个样本：[输入左声道，输入右声道] -> [输出左声道，输出右声道]。
  - 参数:
    - `left`: 左声道输入样本。
    - `right`: 右声道输入样本。

#### 3. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 StereoDelayEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects.Stereo;

public class StereoDelayEffectExample
{
    public static void Main()
    {
        // 创建 StereoDelayEffect 实例
        var stereoDelayEffect = new StereoDelayEffect(44100, 0.5f, 0.3f, 0.4f, 0.6f, 0.7f);

      // 设置声像和延迟
        stereoDelayEffect.Pan = 0.8f;
        stereoDelayEffect.DelayLeft = 0.4f;
        stereoDelayEffect.DelayRight = 0.5f;
        stereoDelayEffect.FeedbackLeft = 0.7f;
        stereoDelayEffect.FeedbackRight = 0.8f;

        // 处理样本
        float leftSample = 0.5f;
        float rightSample = 0.6f;
        stereoDelayEffect.Process(ref leftSample, ref rightSample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Left Sample: {leftSample}");
        Console.WriteLine($"Processed Right Sample: {rightSample}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.StereoEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.StereoEffect 是一个用于立体声音频效果的抽象类，继承了 WetDryMixer 类。

### 方法

#### 1. Process
- `public abstract void Process(ref float left, ref float right)`
  - 处理每个声道的一个样本：[输入左声道，输入右声道] -> [输出左声道，输出右声道]。
  - 参数:
    - `left`: 左声道输入样本。
    - `right`: 右声道输入样本。

#### 2. Process
- `public virtual void Process(float sample, out float left, out float right)`
  - 处理单声道样本：输入样本 -> [输出左声道，输出右声道]。
  - 参数:
    - `sample`: 单声道输入样本。
    - `left`: 左声道输出样本。
    - `right`: 右声道输出样本。

#### 3. Process
- `public virtual void Process(float[] inputLeft, float[] inputRight, float[] outputLeft, float[] outputRight, int count = 0, int inputPos = 0, int outputPos = 0)`
  - 处理每个声道的样本块：[输入左声道，输入右声道] -> [输出左声道，输出右声道]。
  - 参数:
    - `inputLeft`: 左声道输入样本块。
    - `inputRight`: 右声道输入样本块。
    - `outputLeft`: 左声道输出样本块。
    - `outputRight`: 右声道输出样本块。
    - `count`: 要处理的样本数量，默认为 0。
    - `inputPos`: 输入起始索引，默认为 0。
    - `outputPos`: 输出起始索引，默认为 0。

#### 4. Process
- `public virtual void Process(float[] input, float[] outputLeft, float[] outputRight, int count = 0, int inputPos = 0, int outputPos = 0)`
  - 处理单声道样本块：[输入] -> [输出左声道，输出右声道]。
  - 参数:
    - `input`: 单声道输入样本块。
    - `outputLeft`: 左声道输出样本块。
    - `outputRight`: 右声道输出样本块。
    - `count`: 要处理的样本数量，默认为 0。
    - `inputPos`: 输入起始索引，默认为 0。
    - `outputPos`: 输出起始索引，默认为 0。

#### 5. ApplyTo
- `public virtual (DiscreteSignal, DiscreteSignal) ApplyTo(DiscreteSignal signal)`
  - 将效果应用于整个信号，并返回输出信号的元组 [左信号，右信号]。
  - 参数:
    - `signal`: 输入信号。

#### 6. ApplyTo
- `public virtual (DiscreteSignal, DiscreteSignal) ApplyTo(DiscreteSignal leftSignal, DiscreteSignal rightSignal)`
  - 将效果应用于左、右声道的整个信号，并返回输出信号的元组 [左信号，右信号]。
  - 参数:
    - `leftSignal`: 左声道输入信号。
    - `rightSignal`: 右声道输入信号。

#### 7. Reset
- `public abstract void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 StereoEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects.Stereo;

public class CustomStereoEffect : StereoEffect
{
    public override void Process(ref float left, ref float right)
    {
        // 自定义处理逻辑
        left *= 0.5f;
        right *= 0.5f;
    }
    public override void Reset()
    {
        // 自定义重置逻辑
    }
}
public class StereoEffectExample
{
    public static void Main()
    {
        // 创建 CustomStereoEffect 实例 var stereoEffect = new CustomStereoEffect();
        // 处理样本
        float leftSample = 0.5f;
        float rightSample = 0.6f;
        stereoEffect.Process(ref leftSample, ref rightSample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Left Sample: {leftSample}");
        Console.WriteLine($"Processed Right Sample: {rightSample}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.BitCrusherEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.BitCrusherEffect 是一个用于比特压缩（失真）音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. BitDepth
- `public int BitDepth { get; set; }`
  - 获取或设置比特深度（位数）。

### 方法

#### 1. BitCrusherEffect 构造函数
- `public BitCrusherEffect(int bitDepth)`
  - 构造 BitCrusherEffect 实例。
  - 参数:
    - `bitDepth`: 比特深度（位数）。

#### 2. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 BitCrusherEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects;
public class BitCrusherEffectExample
{
    public static void Main()
    {
        // 创建 BitCrusherEffect 实例
        var bitCrusherEffect = new BitCrusherEffect(8);

        // 设置比特深度
        bitCrusherEffect.BitDepth = 6;

        // 处理样本
        float sample = 0.5f;
        float processedSample = bitCrusherEffect.Process(sample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Sample: {processedSample}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.ChorusEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.ChorusEffect 是一个用于合唱音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. Widths
- `public float[] Widths { get; set; }`
  - 获取或设置每个声部的宽度（最大延迟，以秒为单位）。

#### 2. LfoFrequencies
- `public float[] LfoFrequencies { get; set; }`
  - 获取或设置每个声部的 LFO 频率。

### 方法

#### 1. ChorusEffect 构造函数
- `public ChorusEffect(int samplingRate, float[] lfoFrequencies, float[] widths)`
  - 构造 ChorusEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `lfoFrequencies`: 每个声部的 LFO 频率。
    - `widths`: 每个声部的宽度（最大延迟，以秒为单位）。

#### 2. ChorusEffect 构造函数
- `public ChorusEffect(int samplingRate, SignalBuilder[] lfos, float[] widths)`
  - 从 LFO 构造 ChorusEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `lfos`: LFO（以信号生成器的形式）。
    - `widths`: 每个声部的宽度（最大延迟，以秒为单位）。

#### 3. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 ChorusEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class ChorusEffectExample
{
    public static void Main()
    {
        // 创建 ChorusEffect 实例
        float[] lfoFrequencies = { 0.25f, 0.3f, 0.35f };
        float[] widths = { 0.002f, 0.003f, 0.004f };
        var chorusEffect = new ChorusEffect(44100, lfoFrequencies, widths);

        // 处理样本
        float sample = 0.5f;
        float processedSample = chorusEffect.Process(sample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Sample: {processedSample}");
    }
}
```
## Vorcyc.Mathematics.SignalProcessing.Effects.DelayEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.DelayEffect 是一个用于延迟音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. Delay
- `public float Delay { get; set; }`
  - 获取或设置延迟时间（以秒为单位）。

#### 2. Feedback
- `public float Feedback { get; set; }`
  - 获取或设置反馈参数。

### 方法

#### 1. DelayEffect 构造函数
- `public DelayEffect(int samplingRate, float delay, float feedback = 0.5f, InterpolationMode interpolationMode = InterpolationMode.Nearest, float reserveDelay = 0f)`
  - 构造 DelayEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `delay`: 延迟时间（以秒为单位）。
    - `feedback`: 反馈参数，默认为 0.5。
    - `interpolationMode`: 分数延迟线的插值模式，默认为 `InterpolationMode.Nearest`。
    - `reserveDelay`: 保留延迟的最大时间，默认为 0 秒。

#### 2. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 DelayEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class DelayEffectExample
{
    public static void Main()
    {
        // 创建 DelayEffect 实例
        var delayEffect = new DelayEffect(44100, 0.5f, 0.6f);

        // 设置延迟和反馈
        delayEffect.Delay = 0.4f;
        delayEffect.Feedback = 0.7f;

        // 处理样本
        float sample = 0.5f;
        float processedSample = delayEffect.Process(sample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Sample: {processedSample}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.DistortionEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.DistortionEffect 是一个用于失真音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. Mode
- `public DistortionMode Mode { get; set; }`
  - 获取或设置失真模式（软/硬削波、指数、全波/半波整流）。

#### 2. InputGain
- `public float InputGain { get; set; }`
  - 获取或设置输入增益（以 dB 为单位）。

#### 3. OutputGain
- `public float OutputGain { get; set; }`
  - 获取或设置输出增益（以 dB 为单位）。

### 方法

#### 1. DistortionEffect 构造函数
- `public DistortionEffect(DistortionMode mode, float inputGain = 12f, float outputGain = -12f)`
  - 构造 DistortionEffect 实例。
  - 参数:
    - `mode`: 失真模式。
    - `inputGain`: 输入增益（以 dB 为单位），默认为 12 dB。
    - `outputGain`: 输出增益（以 dB 为单位），默认为 -12 dB。

#### 2. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 DistortionEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class DistortionEffectExample
{
    public static void Main()
    {
        // 创建 DistortionEffect 实例
        var distortionEffect = new DistortionEffect(DistortionMode.HardClipping, 12f, -12f);

        // 设置失真模式和增益
        distortionEffect.Mode = DistortionMode.SoftClipping;
        distortionEffect.InputGain = 10f;
        distortionEffect.OutputGain = -10f;

        // 处理样本
        float sample = 0.5f;
        float processedSample = distortionEffect.Process(sample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Sample: {processedSample}");
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Effects.EchoEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.EchoEffect 是一个用于回声音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. Delay
- `public float Delay { get; set; }`
  - 获取或设置延迟时间（以秒为单位）。

#### 2. Feedback
- `public float Feedback { get; set; }`
  - 获取或设置反馈参数。

### 方法

#### 1. EchoEffect 构造函数
- `public EchoEffect(int samplingRate, float delay, float feedback = 0.5f, InterpolationMode interpolationMode = InterpolationMode.Nearest, float reserveDelay = 0f)`
  - 构造 EchoEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `delay`: 延迟时间（以秒为单位）。
    - `feedback`: 反馈参数，默认为 0.5。
    - `interpolationMode`: 分数延迟线的插值模式，默认为 `InterpolationMode.Nearest`。
    - `reserveDelay`: 保留延迟的最大时间，默认为 0 秒。

#### 2. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 EchoEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class EchoEffectExample
{
    public static void Main()
    {
        // 创建 EchoEffect 实例
        var echoEffect = new EchoEffect(44100, 0.5f, 0.6f);

        // 设置延迟和反馈
        echoEffect.Delay = 0.4f;
        echoEffect.Feedback = 0.7f;

        // 处理样本
        float sample = 0.5f;
        float processedSample = echoEffect.Process(sample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Sample: {processedSample}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.FlangerEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.FlangerEffect 是一个用于镶边音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. Width
- `public float Width { get; set; }`
  - 获取或设置宽度（以秒为单位）。

#### 2. LfoFrequency
- `public float LfoFrequency { get; set; }`
  - 获取或设置 LFO 频率（以 Hz 为单位）。

#### 3. Lfo
- `public SignalBuilder Lfo { get; set; }`
  - 获取或设置 LFO 信号生成器。

#### 4. Depth
- `public float Depth { get; set; }`
  - 获取或设置深度。

#### 5. Feedback
- `public float Feedback { get; set; }`
  - 获取或设置反馈参数。

#### 6. Inverted
- `public bool Inverted { get; set; }`
  - 获取或设置反转模式标志。

#### 7. InterpolationMode
- `public InterpolationMode InterpolationMode { get; set; }`
  - 获取或设置插值模式。

### 方法

#### 1. FlangerEffect 构造函数
- `public FlangerEffect(int samplingRate, float lfoFrequency = 1f, float width = 0.003f, float depth = 0.5f, float feedback = 0, bool inverted = false, InterpolationMode interpolationMode = InterpolationMode.Linear, float reserveWidth = 0f)`
  - 构造 FlangerEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `lfoFrequency`: LFO 频率（以 Hz 为单位），默认为 1 Hz。
    - `width`: 宽度（以秒为单位），默认为 0.003 秒。
    - `depth`: 深度，默认为 0.5。
    - `feedback`: 反馈参数，默认为 0。
    - `inverted`: 反转模式，默认为 false。
    - `interpolationMode`: 分数延迟线的插值模式，默认为 `InterpolationMode.Linear`。
    - `reserveWidth`: 保留宽度的最大时间，默认为 0 秒。

#### 2. FlangerEffect 构造函数
- `public FlangerEffect(int samplingRate, SignalBuilder lfo, float width = 0.003f, float depth = 0.5f, float feedback = 0, bool inverted = false, InterpolationMode interpolationMode = InterpolationMode.Linear, float reserveWidth = 0f)`
  - 从 LFO 构造 FlangerEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `lfo`: LFO 信号生成器。
    - `width`: 宽度（以秒为单位），默认为 0.003 秒。
    - `depth`: 深度，默认为 0.5。
    - `feedback`: 反馈参数，默认为 0。
    - `inverted`: 反转模式，默认为 false。
    - `interpolationMode`: 分数延迟线的插值模式，默认为 `InterpolationMode.Linear`。
    - `reserveWidth`: 保留宽度的最大时间，默认为 0 秒。

#### 3. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 FlangerEffect 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects;
public class FlangerEffectExample
{
    public static void Main()
    {
        // 创建 FlangerEffect 实例
        var flangerEffect = new FlangerEffect(44100, 0.25f, 0.003f, 0.5f, 0.6f, false);

        // 设置参数
        flangerEffect.Width = 0.004f;
        flangerEffect.LfoFrequency = 0.5f;
        flangerEffect.Depth = 0.7f;
        flangerEffect.Feedback = 0.8f;
        flangerEffect.Inverted = true;

        // 处理样本
        float sample = 0.5f;
        float processedSample = flangerEffect.Process(sample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Sample: {processedSample}");
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Effects.MorphEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.MorphEffect 是一个用于混合（变形）两个声音信号的音频效果类，继承了 AudioEffect 类。

### 属性

无公开属性。

### 方法

#### 1. MorphEffect 构造函数
- `public MorphEffect(int hopSize, int fftSize = 0)`
  - 构造 MorphEffect 实例。
  - 参数:
    - `hopSize`: 跳跃长度（样本数）。
    - `fftSize`: FFT 大小，默认为 0。

#### 2. Process
- `public float Process(float sample, float mix)`
  - 处理一个输入信号样本和一个要混合的信号样本。
  - 参数:
    - `sample`: 输入信号样本。
    - `mix`: 要与输入信号混合的信号样本。
  - 返回值: 处理后的样本。

#### 3. ProcessFrame
- `protected void ProcessFrame()`
  - 处理一个帧（块）。

#### 4. Reset
- `public override void Reset()`
  - 重置效果。

#### 5. ApplyTo
- `public DiscreteSignal ApplyTo(DiscreteSignal signal, DiscreteSignal mix)`
  - 将效果应用于整个输入信号和混合信号。
  - 参数:
    - `signal`: 输入信号。
    - `mix`: 要与输入信号混合的信号。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 6. Process
- `public override float Process(float sample)`
  - 处理一个样本。此方法在 MorphEffect 类中未实现。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

### 代码示例
以下是一个使用 MorphEffect 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects;
using Vorcyc.Mathematics.SignalProcessing;

public class MorphEffectExample
{
    public static void Main()
    {
        // 创建 MorphEffect 实例
        var morphEffect = new MorphEffect(256, 1024);
        // 定义输入信号和混合信号
        float[] inputSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        float[] mixSamples = { 0.4f, 0.5f, 0.45f, 0.6f, 0.55f };
        var inputSignal = new DiscreteSignal(44100, inputSamples);
        var mixSignal = new DiscreteSignal(44100, mixSamples);

        // 将效果应用于整个信号
        var processedSignal = morphEffect.ApplyTo(inputSignal, mixSignal);

        // 输出处理后的信号样本
        Console.WriteLine("Processed Signal:");
        foreach (var sample in processedSignal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Effects.PhaserEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.PhaserEffect 是一个用于移相音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. Q
- `public float Q { get; set; }`
  - 获取或设置 Q 因子（也称为质量因子，谐振）。

#### 2. LfoFrequency
- `public float LfoFrequency { get; set; }`
  - 获取或设置 LFO 频率（以 Hz 为单位）。

#### 3. MinFrequency
- `public float MinFrequency { get; set; }`
  - 获取或设置最小 LFO 频率（以 Hz 为单位）。

#### 4. MaxFrequency
- `public float MaxFrequency { get; set; }`
  - 获取或设置最大 LFO 频率（以 Hz 为单位）。

#### 5. Lfo
- `public SignalBuilder Lfo { get; set; }`
  - 获取或设置 LFO 信号生成器。

### 方法

#### 1. PhaserEffect 构造函数
- `public PhaserEffect(int samplingRate, float lfoFrequency = 1.0f, float minFrequency = 300, float maxFrequency = 3000, float q = 0.5f)`
  - 构造 PhaserEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `lfoFrequency`: LFO 频率（以 Hz 为单位），默认为 1.0 Hz。
    - `minFrequency`: 最小 LFO 频率（以 Hz 为单位），默认为 300 Hz。
    - `maxFrequency`: 最大 LFO 频率（以 Hz 为单位），默认为 3000 Hz。
    - `q`: Q 因子（也称为质量因子，谐振），默认为 0.5。

#### 2. PhaserEffect 构造函数
- `public PhaserEffect(int samplingRate, SignalBuilder lfo, float q = 0.5f)`
  - 从 LFO 构造 PhaserEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `lfo`: LFO 信号生成器。
    - `q`: Q 因子（也称为质量因子，谐振），默认为 0.5。

#### 3. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 PhaserEffect 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class PhaserEffectExample
{
    public static void Main()
    {
        // 创建 PhaserEffect 实例
        var phaserEffect = new PhaserEffect(44100, 0.5f, 300, 3000, 0.7f);
        // 设置参数
        phaserEffect.LfoFrequency = 0.8f;
        phaserEffect.MinFrequency = 200;
        phaserEffect.MaxFrequency = 4000;
        phaserEffect.Q = 0.6f;

        // 处理样本
        float sample = 0.5f;
        float processedSample = phaserEffect.Process(sample);

        // 输出处理后的样本
        Console.WriteLine($"Processed Sample: {processedSample}");
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Effects.PitchShiftEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.PitchShiftEffect 是一个用于离线音高移位音频效果的类，基于可用的 TSM 算法和线性插值。PitchShiftEffect 不实现在线处理（方法 Process(float)）。

### 属性

#### 1. Shift
- `public float Shift { get; set; }`
  - 获取或设置音高移位比率。

#### 2. Tsm
- `public TsmAlgorithm Tsm { get; set; }`
  - 获取或设置时间尺度修改算法。

#### 3. WindowSize
- `public int WindowSize { get; set; }`
  - 获取或设置窗口大小（帧长度）。

#### 4. HopSize
- `public int HopSize { get; set; }`
  - 获取或设置跳跃长度。

### 方法

#### 1. PitchShiftEffect 构造函数
- `public PitchShiftEffect(float shift, int windowSize = 1024, int hopSize = 128, TsmAlgorithm tsm = TsmAlgorithm.PhaseVocoderPhaseLocking)`
  - 构造 PitchShiftEffect 实例。
  - 参数:
    - `shift`: 音高移位比率。
    - `windowSize`: 窗口大小（帧长度），默认为 1024。
    - `hopSize`: 跳跃长度，默认为 128。
    - `tsm`: 时间尺度修改算法，默认为 `TsmAlgorithm.PhaseVocoderPhaseLocking`。

#### 2. ApplyTo
- `public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将效果应用于整个信号，并返回新的音高移位信号。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。
  - 返回值: 处理后的 `DiscreteSignal` 对象。

#### 3. Apply
- `public override void Apply(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)`
  - 将效果应用于整个信号（就地处理）。
  - 参数:
    - `signal`: 输入信号。
    - `method`: 过滤方法，默认为 `FilteringMethod.Auto`。

#### 4. Process
- `public override float Process(float sample)`
  - 处理一个样本。此方法在 PitchShiftEffect 类中未实现。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 5. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 PitchShiftEffect 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class PitchShiftEffectExample
{
    public static void Main()
    {
        // 创建 PitchShiftEffect 实例
        var pitchShiftEffect = new PitchShiftEffect(1.2f, 1024, 128, TsmAlgorithm.PhaseVocoderPhaseLocking);
        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 将效果应用于整个信号
        var processedSignal = pitchShiftEffect.ApplyTo(signal);

        // 输出处理后的信号样本
        Console.WriteLine("Processed Signal:");
        foreach (var sample in processedSignal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.Effects.PitchShiftVocoderEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.PitchShiftVocoderEffect 是一个基于重叠加法滤波和频域音高移位的音频效果类，继承了 OverlapAddFilter 类。

### 属性

#### 1. Shift
- `public float Shift { get; set; }`
  - 获取或设置音高移位比率。

### 方法

#### 1. PitchShiftVocoderEffect 构造函数
- `public PitchShiftVocoderEffect(int samplingRate, float shift, int fftSize = 1024, int hopSize = 64)`
  - 构造 PitchShiftVocoderEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `shift`: 音高移位比率。
    - `fftSize`: FFT 大小，默认为 1024。
    - `hopSize`: 跳跃长度，默认为 64。

#### 2. ProcessSpectrum
- `protected override void ProcessSpectrum(float[] re, float[] im, float[] filteredRe, float[] filteredIm)`
  - 在每个重叠加法 STFT 步骤中处理一个频谱。
  - 参数:
    - `re`: 输入频谱的实部。
    - `im`: 输入频谱的虚部。
    - `filteredRe`: 输出频谱的实部。
    - `filteredIm`: 输出频谱的虚部。

#### 3. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 PitchShiftVocoderEffect 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class PitchShiftVocoderEffectExample
{
    public static void Main()
    {
        // 创建 PitchShiftVocoderEffect
        实例 var pitchShiftVocoderEffect = new PitchShiftVocoderEffect(44100, 1.2f, 1024, 64);
        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 将效果应用于整个信号
        var processedSignal = pitchShiftVocoderEffect.ApplyTo(signal);

        // 输出处理后的信号样本
        Console.WriteLine("Processed Signal:");
        foreach (var sample in processedSignal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Effects.RobotEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.RobotEffect 是一个用于语音机器人化的音频效果类，继承了 OverlapAddFilter 类。

### 方法

#### 1. RobotEffect 构造函数
- `public RobotEffect(int hopSize, int fftSize = 0)`
  - 构造 RobotEffect 实例。
  - 参数:
    - `hopSize`: 跳跃长度（样本数）。
    - `fftSize`: FFT 大小，默认为 0。

#### 2. ProcessSpectrum
- `protected override void ProcessSpectrum(float[] re, float[] im, float[] filteredRe, float[] filteredIm)`
  - 在每个重叠加法 STFT 步骤中处理一个频谱（简单地将相位设置为 0）。
  - 参数:
    - `re`: 输入频谱的实部。
    - `im`: 输入频谱的虚部。
    - `filteredRe`: 输出频谱的实部。
    - `filteredIm`: 输出频谱的虚部。

### 代码示例
以下是一个使用 RobotEffect 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class RobotEffectExample
{
    public static void Main()
    {
        // 创建 RobotEffect 实例
        var robotEffect = new RobotEffect(256, 1024);
        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 将效果应用于整个信号
        var processedSignal = robotEffect.ApplyTo(signal);

        // 输出处理后的信号样本
        Console.WriteLine("Processed Signal:");
        foreach (var sample in processedSignal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Effects.TremoloEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.TremoloEffect 是一个用于颤音音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. Depth
- `public float Depth { get; set; }`
  - 获取或设置深度。

#### 2. Frequency
- `public float Frequency { get; set; }`
  - 获取或设置颤音频率（调制频率）（以 Hz 为单位）。

#### 3. Index
- `public float Index { get; set; }`
  - 获取或设置颤音指数（调制指数）。

#### 4. Lfo
- `public SignalBuilder Lfo { get; set; }`
  - 获取或设置 LFO 信号生成器。

### 方法

#### 1. TremoloEffect 构造函数
- `public TremoloEffect(int samplingRate, float depth = 0.5f, float frequency = 10f, float tremoloIndex = 0.5f)`
  - 构造 TremoloEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `depth`: 深度，默认为 0.5。
    - `frequency`: 颤音频率（调制频率）（以 Hz 为单位），默认为 10 Hz。
    - `tremoloIndex`: 颤音指数（调制指数），默认为 0.5。

#### 2. TremoloEffect 构造函数
- `public TremoloEffect(SignalBuilder lfo, float depth = 0.5f)`
  - 从 LFO 构造 TremoloEffect 实例。
  - 参数:
    - `lfo`: LFO 信号生成器。
    - `depth`: 深度，默认为 0.5。

#### 3. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 TremoloEffect 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class TremoloEffectExample
{
    public static void Main()
    {
        // 创建 TremoloEffect 实例
        var tremoloEffect = new TremoloEffect(44100, 0.5f, 10f, 0.5f);
        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 处理样本
        foreach (var sample in signal.Samples)
        {
            var processedSample = tremoloEffect.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Effects.TubeDistortionEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.TubeDistortionEffect 是一个用于管失真音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. InputGain
- `public float InputGain { get; set; }`
  - 获取或设置输入增益（以 dB 为单位）。

#### 2. OutputGain
- `public float OutputGain { get; set; }`
  - 获取或设置输出增益（以 dB 为单位）。

#### 3. Q
- `public float Q { get; set; }`
  - 获取或设置 Q 因子（工作点）。控制低输入电平的传递函数的线性度。越负越线性。

#### 4. Dist
- `public float Dist { get; set; }`
  - 获取或设置失真的特性。数值越高，失真越硬。

#### 5. Rh
- `public float Rh { get; }`
  - 获取滤波器系数（接近 1.0），定义高通滤波器中极点的位置，用于去除直流分量。

#### 6. Rl
- `public float Rl { get; }`
  - 获取滤波器系数（范围 [0, 1]），定义低通滤波器中极点的位置，用于模拟管放大器中的电容。

### 方法

#### 1. TubeDistortionEffect 构造函数
- `public TubeDistortionEffect(float inputGain = 20f, float outputGain = -12f, float q = -0.2f, float dist = 5, float rh = 0.995f, float rl = 0.5f)`
  - 构造 TubeDistortionEffect 实例。
  - 参数:
    - `inputGain`: 输入增益（以 dB 为单位），默认为 20 dB。
    - `outputGain`: 输出增益（以 dB 为单位），默认为 -12 dB。
    - `q`: Q 因子，默认为 -0.2。
    - `dist`: 失真的特性，默认为 5。
    - `rh`: 高通滤波器系数，默认为 0.995。
    - `rl`: 低通滤波器系数，默认为 0.5。

#### 2. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 3. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 TubeDistortionEffect 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.Effects;
public class TubeDistortionEffectExample
{
    public static void Main()
    {
        // 创建 TubeDistortionEffect 实例
        var tubeDistortionEffect = new TubeDistortionEffect(20f, -12f, -0.2f, 5, 0.995f, 0.5f);
        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 处理样本
        foreach (var sample in signal.Samples)
        {
            var processedSample = tubeDistortionEffect.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Effects.VibratoEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.VibratoEffect 是一个用于颤音音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. Width
- `public float Width { get; set; }`
  - 获取或设置宽度（以秒为单位）。

#### 2. LfoFrequency
- `public float LfoFrequency { get; set; }`
  - 获取或设置 LFO 频率（以 Hz 为单位）。

#### 3. Lfo
- `public SignalBuilder Lfo { get; set; }`
  - 获取或设置 LFO 信号生成器。

#### 4. InterpolationMode
- `public InterpolationMode InterpolationMode { get; set; }`
  - 获取或设置插值模式。

### 方法

#### 1. VibratoEffect 构造函数
- `public VibratoEffect(int samplingRate, float lfoFrequency = 1f, float width = 0.003f, InterpolationMode interpolationMode = InterpolationMode.Linear, float reserveWidth = 0f)`
  - 构造 VibratoEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `lfoFrequency`: LFO 频率（以 Hz 为单位），默认为 1 Hz。
    - `width`: 宽度（以秒为单位），默认为 0.003 秒。
    - `interpolationMode`: 插值模式，默认为 `InterpolationMode.Linear`。
    - `reserveWidth`: 保留宽度的最大时间，默认为 0 秒。

#### 2. VibratoEffect 构造函数
- `public VibratoEffect(int samplingRate, SignalBuilder lfo, float width = 0.003f, InterpolationMode interpolationMode = InterpolationMode.Linear, float reserveWidth = 0f)`
  - 从 LFO 构造 VibratoEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `lfo`: LFO 信号生成器。
    - `width`: 宽度（以秒为单位），默认为 0.003 秒。
    - `interpolationMode`: 插值模式，默认为 `InterpolationMode.Linear`。
    - `reserveWidth`: 保留宽度的最大时间，默认为 0 秒。

#### 3. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 VibratoEffect 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class VibratoEffectExample
{
    public static void Main()
    {
        // 创建 VibratoEffect 实例
        var vibratoEffect = new VibratoEffect(44100, 1f, 0.003f, InterpolationMode.Linear, 0f);
        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 处理样本
        foreach (var sample in signal.Samples)
        {
            var processedSample = vibratoEffect.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.Effects.WahwahEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.WahwahEffect 是一个用于 Wah-Wah 音频效果的类，继承了 AudioEffect 类。

### 属性

#### 1. LfoFrequency
- `public float LfoFrequency { get; set; }`
  - 获取或设置 LFO 频率（以 Hz 为单位）。

#### 2. MinFrequency
- `public float MinFrequency { get; set; }`
  - 获取或设置最小 LFO 频率（以 Hz 为单位）。

#### 3. MaxFrequency
- `public float MaxFrequency { get; set; }`
  - 获取或设置最大 LFO 频率（以 Hz 为单位）。

#### 4. Q
- `public float Q { get; set; }`
  - 获取或设置 Q 因子（也称为质量因子，谐振）。

#### 5. Lfo
- `public SignalBuilder Lfo { get; set; }`
  - 获取或设置 LFO 信号生成器。

### 方法

#### 1. WahwahEffect 构造函数
- `public WahwahEffect(int samplingRate, float lfoFrequency = 1.0f, float minFrequency = 300, float maxFrequency = 1500, float q = 0.5f)`
  - 构造 WahwahEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `lfoFrequency`: LFO 频率（以 Hz 为单位），默认为 1.0 Hz。
    - `minFrequency`: 最小 LFO 频率（以 Hz 为单位），默认为 300 Hz。
    - `maxFrequency`: 最大 LFO 频率（以 Hz 为单位），默认为 1500 Hz。
    - `q`: Q 因子（也称为质量因子，谐振），默认为 0.5。

#### 2. WahwahEffect 构造函数
- `public WahwahEffect(int samplingRate, SignalBuilder lfo, float q = 0.5f)`
  - 从 LFO 构造 WahwahEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `lfo`: LFO 信号生成器。
    - `q`: Q 因子（也称为质量因子，谐振），默认为 0.5。

#### 3. Process
- `public override float Process(float sample)`
  - 处理一个样本。
  - 参数:
    - `sample`: 输入样本。
  - 返回值: 处理后的样本。

#### 4. Reset
- `public override void Reset()`
  - 重置效果。

### 代码示例
以下是一个使用 WahwahEffect 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class WahwahEffectExample
{
    public static void Main()
    {
        // 创建 WahwahEffect 实例
        var wahwahEffect = new WahwahEffect(44100, 1.0f, 300, 1500, 0.5f);
        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 处理样本
        foreach (var sample in signal.Samples)
        {
            var processedSample = wahwahEffect.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.Effects.WhisperEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.WhisperEffect 是一个用于语音耳语化的音频效果类，继承了 OverlapAddFilter 类。

### 方法

#### 1. WhisperEffect 构造函数
- `public WhisperEffect(int hopSize, int fftSize = 0)`
  - 构造 WhisperEffect 实例。
  - 参数:
    - `hopSize`: 跳跃长度（样本数）。
    - `fftSize`: FFT 大小，默认为 0。

#### 2. ProcessSpectrum
- `protected override void ProcessSpectrum(float[] re, float[] im, float[] filteredRe, float[] filteredIm)`
  - 在每个重叠加法 STFT 步骤中处理一个频谱。
  - 参数:
    - `re`: 输入频谱的实部。
    - `im`: 输入频谱的虚部。
    - `filteredRe`: 输出频谱的实部。
    - `filteredIm`: 输出频谱的虚部。

### 代码示例
以下是一个使用 WhisperEffect 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class WhisperEffectExample
{
    public static void Main()
    {
        // 创建 WhisperEffect 实例        
        var whisperEffect = new WhisperEffect(256, 1024);
        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 将效果应用于整个信号
        var processedSignal = whisperEffect.ApplyTo(signal);

        // 输出处理后的信号样本
        Console.WriteLine("Processed Signal:");
        foreach (var sample in processedSignal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```

---

> 以下类型都位于 Vorcyc.Mathematics.SignalProcessing.FeatureExtractors 命名空间。

## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi.Mpeg7SpectralFeaturesExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi.Mpeg7SpectralFeaturesExtractor 是一个遵循 MPEG-7 推荐标准的特征提取器类，用于评估以下特征：
- 谱特征（MPEG-7）
- 谐波特征
- 感知特征

### 属性

#### 1. FeatureSet
- `public const string FeatureSet`
  - 支持的谱特征全集。

#### 2. HarmonicSet
- `public const string HarmonicSet`
  - 支持的谐波特征全集。

#### 3. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称）。

### 方法

#### 1. Mpeg7SpectralFeaturesExtractor 构造函数
- `public Mpeg7SpectralFeaturesExtractor(MultiFeatureOptions options)`
  - 从配置构造 Mpeg7SpectralFeaturesExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. IncludeHarmonicFeatures
- `public void IncludeHarmonicFeatures(string featureList, int peakCount = 10, Func<float[], float> pitchEstimator = null, Action<float[], int[], float[], int, float> peaksDetector = null, float lowPitch = 80, float highPitch = 400)`
  - 将一组谐波特征添加到提取器的列表中。
  - 参数:
    - `featureList`: 新增谐波特征的字符串名称/注释。
    - `peakCount`: 最大谐波峰值数量，默认为 10。
    - `pitchEstimator`: 用于音高估计的函数，默认为 `Pitch.FromSpectralPeaks`。
    - `peaksDetector`: 用于峰值检测的函数，默认为 `Harmonic.Peaks`。
    - `lowPitch`: 预期音高范围的下限频率，默认为 80 Hz。
    - `highPitch`: 预期音高范围的上限频率，默认为 400 Hz。

#### 3. AddHarmonicFeature
- `public void AddHarmonicFeature(string name, Func<float[], int[], float[], float> algorithm)`
  - 将用户定义的谐波特征添加到提取器的列表中（以及其计算例程）。
  - 参数:
    - `name`: 特征名称/注释。
    - `algorithm`: 特征计算例程。

#### 4. SetPitchTrack
- `public void SetPitchTrack(float[] pitchTrack)`
  - 设置预计算的音高数组。
  - 参数:
    - `pitchTrack`: 预计算的音高数组。

#### 5. ComputeFrom
- `public override int ComputeFrom(float[] samples, int startSample, int endSample, IList<float[]> vectors)`
  - 从样本中计算 MPEG-7 特征向量，并将其存储在向量中。
  - 参数:
    - `samples`: 样本数组。
    - `startSample`: 要处理的数组中的第一个样本的索引。
    - `endSample`: 要处理的数组中的最后一个样本的索引（不包括）。
    - `vectors`: 用于存储结果特征向量的预分配序列。
  - 返回值: 实际计算的特征向量数量。

#### 6. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的 MPEG-7 特征。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个特征向量）。

#### 7. IsParallelizable
- `public override bool IsParallelizable()`
  - 如果手动设置了音高数组，则返回 false，因为在这种情况下，Mpeg7SpectralFeaturesExtractor 不支持并行化。在所有其他情况下返回 true。

#### 8. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。如果提取器不支持并行化，则返回 null。

### 代码示例
以下是一个使用 Mpeg7SpectralFeaturesExtractor 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi;

public class Mpeg7SpectralFeaturesExtractorExample
{
    public static void Main()
    {
        // 创建 MultiFeatureOptions 实例
        var options = new MultiFeatureOptions { SamplingRate = 44100, FeatureList = "centroid, spread, flatness, noiseness, rolloff, crest, entropy, decrease, loudness, sharpness", FrameDuration = 0.025f, HopDuration = 0.01f, FftSize = 1024, PreEmphasis = 0.97f, Window = WindowType.Hamming };
        // 创建 Mpeg7SpectralFeaturesExtractor 实例
        var featureExtractor = new Mpeg7SpectralFeaturesExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = featureExtractor.ComputeFrom(signal);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi.SpectralFeaturesExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi.SpectralFeaturesExtractor 是一个用于提取谱特征的类。

### 属性

#### 1. FeatureSet
- `public const string FeatureSet`
  - 支持的谱特征全集。

#### 2. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称）。

### 方法

#### 1. SpectralFeaturesExtractor 构造函数
- `public SpectralFeaturesExtractor(MultiFeatureOptions options)`
  - 从配置构造 SpectralFeaturesExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. AddFeature
- `public void AddFeature(string name, Func<float[], float[], float> algorithm)`
  - 将用户定义的谱特征添加到提取器的列表中（以及其计算例程）。
  - 参数:
    - `name`: 特征名称/注释。
    - `algorithm`: 特征计算例程。

#### 3. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的谱特征。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个特征向量）。

#### 4. IsParallelizable
- `public override bool IsParallelizable()`
  - 返回 true，因为 SpectralFeaturesExtractor 始终支持并行化。

#### 5. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。

### 代码示例
以下是一个使用 SpectralFeaturesExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi;

public class SpectralFeaturesExtractorExample
{
    public static void Main()
    {
        // 创建 MultiFeatureOptions 实例
        var options = new MultiFeatureOptions { SamplingRate = 44100, FeatureList = "centroid, spread, flatness, noiseness, rolloff, crest, entropy, decrease, c1, c2, c3, c4, c5, c6", FrameDuration = 0.025f, HopDuration = 0.01f, FftSize = 1024, PreEmphasis = 0.97f, Window = WindowType.Hamming };
        // 创建 SpectralFeaturesExtractor 实例
        var featureExtractor = new SpectralFeaturesExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = featureExtractor.ComputeFrom(signal);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi.TimeDomainFeaturesExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi.TimeDomainFeaturesExtractor 是一个用于提取时域特征（能量、RMS、ZCR、熵）的类。

### 属性

#### 1. FeatureSet
- `public const string FeatureSet`
  - 支持的时域特征全集。

#### 2. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称）。

### 方法

#### 1. TimeDomainFeaturesExtractor 构造函数
- `public TimeDomainFeaturesExtractor(MultiFeatureOptions options)`
  - 从配置构造 TimeDomainFeaturesExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. AddFeature
- `public void AddFeature(string name, Func<DiscreteSignal, int, int, float> algorithm)`
  - 将用户定义的时域特征添加到提取器的列表中（以及其计算例程）。
  - 参数:
    - `name`: 特征名称/注释。
    - `algorithm`: 特征计算例程。

#### 3. ComputeFrom
- `public override int ComputeFrom(float[] samples, int startSample, int endSample, IList<float[]> vectors)`
  - 从样本中计算特征向量，并将其存储在向量中。
  - 参数:
    - `samples`: 样本数组。
    - `startSample`: 要处理的数组中的第一个样本的索引。
    - `endSample`: 要处理的数组中的最后一个样本的索引（不包括）。
    - `vectors`: 用于存储结果特征向量的预分配序列。
  - 返回值: 实际计算的特征向量数量。

#### 4. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 处理一个帧中的时域特征。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个特征向量）。
  - 注意: TimeDomainFeaturesExtractor 不提供此功能，请调用 ComputeFrom() 方法。

#### 5. IsParallelizable
- `public override bool IsParallelizable()`
  - 返回 true，因为 TimeDomainFeaturesExtractor 始终支持并行化。

#### 6. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。

### 代码示例
以下是一个使用 TimeDomainFeaturesExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi;

public class TimeDomainFeaturesExtractorExample
{
    public static void Main()
    {
        // 创建 MultiFeatureOptions 实例
        var options = new MultiFeatureOptions { SamplingRate = 44100, FeatureList = "energy, rms, zcr, entropy", FrameDuration = 0.025f, HopDuration = 0.01f };
        // 创建 TimeDomainFeaturesExtractor 实例
        var featureExtractor = new TimeDomainFeaturesExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = featureExtractor.ComputeFrom(signal.Samples, 0, signal.Length, new List<float[]>());

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.AmsExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.AmsExtractor 是一个用于提取幅度调制谱（AMS）的特征提取器类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称）。

#### 2. Filterbank
- `public float[][] Filterbank { get; }`
  - 获取滤波器组矩阵，维度为 [filterCount * (fftSize/2 + 1)]。

#### 3. Envelopes
- `public float[][] Envelopes { get; }`
  - 获取不同频带中的信号包络。

### 方法

#### 1. AmsExtractor 构造函数
- `public AmsExtractor(AmsOptions options)`
  - 从配置构造 AmsExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ComputeFrom
- `public override List<float[]> ComputeFrom(float[] samples, int startSample, int endSample)`
  - 从样本中计算调制谱特征向量，并返回计算的特征向量列表。
  - 参数:
    - `samples`: 样本数组。
    - `startSample`: 要处理的数组中的第一个样本的索引。
    - `endSample`: 要处理的数组中的最后一个样本的索引（不包括）。
  - 返回值: 计算的特征向量列表。

#### 3. MakeSpectrum2D
- `public float[][] MakeSpectrum2D(float[] featureVector)`
  - 从其展平版本创建 2D 调制谱。轴为：[短时频率] x [调制频率]。
  - 参数:
    - `featureVector`: AMS 特征向量。
  - 返回值: 2D 调制谱。

#### 4. VectorsAtHerz
- `public List<float[]> VectorsAtHerz(IList<float[]> featureVectors, float herz = 4)`
  - 获取与特定调制频率（默认值为 4 Hz）对应的短时谱序列。
  - 参数:
    - `featureVectors`: AMS 特征向量序列。
    - `herz`: 调制频率。
  - 返回值: 短时谱序列。

#### 5. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 处理一个帧中的数据块。AmsExtractor 不提供此功能，请调用 ComputeFrom() 方法。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个特征向量）。

#### 6. ComputeFrom
- `public override int ComputeFrom(float[] samples, int startSample, int endSample, IList<float[]> vectors)`
  - 处理一个样本。AmsExtractor 不提供此功能，请调用 ComputeFrom() 方法。
  - 参数:
    - `samples`: 样本数组。
    - `startSample`: 要处理的数组中的第一个样本的索引。
    - `endSample`: 要处理的数组中的最后一个样本的索引（不包括）。
    - `vectors`: 用于存储结果特征向量的预分配序列。

### 代码示例
以下是一个使用 AmsExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;
public class AmsExtractorExample
{
    public static void Main()
    {
        // 创建 AmsOptions 实例
        var options = new AmsOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, ModulationFftSize = 1024, ModulationHopSize = 128 };
        // 创建 AmsExtractor 实例
        var amsExtractor = new AmsExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = amsExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }

        // 获取 2D 调制谱
        var spectrum2D = amsExtractor.MakeSpectrum2D(featureVectors[0]);
        Console.WriteLine("2D Modulation Spectrum:");
        foreach (var row in spectrum2D)
        {
            Console.WriteLine(string.Join(", ", row));
        }

        // 获取特定调制频率的短时谱序列
        var vectorsAt4Hz = amsExtractor.VectorsAtHerz(featureVectors, 4);
        Console.WriteLine("Vectors at 4 Hz:");
        foreach (var vector in vectorsAt4Hz)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.ChromaExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.ChromaExtractor 是一个用于提取色度特征的类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称）。
  - 如果色度数量为 12 且 baseC 为 true，则返回 "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"。
  - 如果色度数量为 12 且 baseC 为 false，则返回 "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#"。
  - 否则，返回 "chroma1", "chroma2", 等等。

#### 2. FilterBank
- `public float[][] FilterBank { get; }`
  - 获取滤波器组矩阵，维度为 [ChromaCount * (blockSize/2 + 1)]。

### 方法

#### 1. ChromaExtractor 构造函数
- `public ChromaExtractor(ChromaOptions options)`
  - 从配置构造 ChromaExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的色度特征向量。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个色度特征向量）。

#### 3. IsParallelizable
- `public override bool IsParallelizable()`
  - 返回 true，因为 ChromaExtractor 始终支持并行化。

#### 4. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。

### 代码示例
以下是一个使用 ChromaExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;

public class ChromaExtractorExample
{
    public static void Main()
    {
        // 创建 ChromaOptions 实例
        var options = new ChromaOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, FftSize = 1024, FeatureCount = 12, BaseC = true };
        // 创建 ChromaExtractor 实例
        var chromaExtractor = new ChromaExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = chromaExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.FilterbankExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.FilterbankExtractor 是一个用于计算每个帧中由给定滤波器组定义的频带中的谱能量的特征提取器类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称），例如 "fb0", "fb1", "fb2", 等等。

#### 2. FilterBank
- `public float[][] FilterBank { get; }`
  - 获取滤波器组矩阵，维度为 [filterbankSize * (blockSize/2 + 1)]。

### 方法

#### 1. FilterbankExtractor 构造函数
- `public FilterbankExtractor(FilterbankOptions options)`
  - 从配置构造 FilterbankExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的滤波器组通道输出向量。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个特征向量）。

#### 3. IsParallelizable
- `public override bool IsParallelizable()`
  - 返回 true，因为 FilterbankExtractor 始终支持并行化。

#### 4. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。

### 代码示例
以下是一个使用 FilterbankExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;

public class FilterbankExtractorExample
{
    public static void Main()
    {
        // 创建 FilterbankOptions 实例
        var options = new FilterbankOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, FftSize = 1024, FilterBankSize = 20, LowFrequency = 20, HighFrequency = 20000, NonLinearity = NonLinearityType.Log10, SpectrumType = SpectrumType.Power, LogFloor = 1e-10f };
        // 创建 FilterbankExtractor 实例
        var filterbankExtractor = new FilterbankExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = filterbankExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.LpccExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.LpccExtractor 是一个用于提取线性预测倒谱系数（LPCC）的特征提取器类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称），例如 "lpcc0", "lpcc1", 等等。

### 方法

#### 1. LpccExtractor 构造函数
- `public LpccExtractor(LpccOptions options)`
  - 从配置构造 LpccExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的 LPCC 特征向量。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个 LPCC 特征向量）。

#### 3. IsParallelizable
- `public override bool IsParallelizable()`
  - 返回 true，因为 LpccExtractor 始终支持并行化。

#### 4. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。

### 代码示例
以下是一个使用 LpccExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;

public class LpccExtractorExample
{
    public static void Main()
    {
        // 创建 LpccOptions 实例
        var options = new LpccOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, FeatureCount = 13, LpcOrder = 12, LifterSize = 22, PreEmphasis = 0.97f, Window = WindowType.Hamming };
        // 创建 LpccExtractor 实例
        var lpccExtractor = new LpccExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = lpccExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.LpcExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.LpcExtractor 是一个用于提取线性预测编码（LPC）系数的特征提取器类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称），例如 "error", "lpc1", "lpc2", 等等。

### 方法

#### 1. LpcExtractor 构造函数
- `public LpcExtractor(LpcOptions options)`
  - 从配置构造 LpcExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的 LPC 特征向量。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个 LPC 特征向量）。

#### 3. IsParallelizable
- `public override bool IsParallelizable()`
  - 返回 true，因为 LpcExtractor 始终支持并行化。

#### 4. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。

### 代码示例
以下是一个使用 LpcExtractor 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;

public class LpcExtractorExample
{
    public static void Main()
    {
        // 创建 LpcOptions 实例
        var options = new LpcOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, LpcOrder = 12, PreEmphasis = 0.97f, Window = WindowType.Hamming };
        // 创建 LpcExtractor 实例
        var lpcExtractor = new LpcExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = lpcExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.MfccExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.MfccExtractor 是一个用于提取梅尔频率倒谱系数（MFCC）的特征提取器类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称），例如 "mfcc0", "mfcc1", "mfcc2", 等等。
  - 如果包含能量特征，则第一个特征名称为 "log_En"。

#### 2. FilterBank
- `public float[][] FilterBank { get; }`
  - 获取滤波器组矩阵，维度为 [filterbankSize * (fftSize/2 + 1)]。默认情况下是梅尔滤波器组。

### 方法

#### 1. MfccExtractor 构造函数
- `public MfccExtractor(MfccOptions options)`
  - 从配置构造 MfccExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的 MFCC 特征向量。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个 MFCC 特征向量）。

#### 3. IsParallelizable
- `public override bool IsParallelizable()`
  - 返回 true，因为 MfccExtractor 始终支持并行化。

#### 4. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。

### 代码示例
以下是一个使用 MfccExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;

public class MfccExtractorExample
{
    public static void Main()
    {
        // 创建 MfccOptions 实例
        var options = new MfccOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, FeatureCount = 13, FilterBankSize = 24, FftSize = 1024, LifterSize = 22, PreEmphasis = 0.97f, DctType = "2N", NonLinearity = NonLinearityType.LogE, SpectrumType = SpectrumType.Power, Window = WindowType.Hamming, LogFloor = 1e-10f, IncludeEnergy = true, LogEnergyFloor = 1e-10f };
        // 创建 MfccExtractor 实例
        var mfccExtractor = new MfccExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = mfccExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.PitchExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.PitchExtractor 是一个用于提取和跟踪音高的特征提取器类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称），例如 "pitch"。

### 方法

#### 1. PitchExtractor 构造函数
- `public PitchExtractor(PitchOptions options)`
  - 从配置构造 PitchExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的音高特征向量。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个音高特征向量）。

#### 3. IsParallelizable
- `public override bool IsParallelizable()`
  - 返回 true，因为 PitchExtractor 始终支持并行化。

#### 4. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。

### 代码示例
以下是一个使用 PitchExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;

public class PitchExtractorExample
{
    public static void Main()
    {
        // 创建 PitchOptions 实例
        var options = new PitchOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, LowFrequency = 80, HighFrequency = 400, PreEmphasis = 0.97f, Window = WindowType.Hamming };
        // 创建 PitchExtractor 实例
        var pitchExtractor = new PitchExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = pitchExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.PlpExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.PlpExtractor 是一个用于提取感知线性预测系数（PLP-RASTA）的特征提取器类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称），例如 "plp0", "plp1", "plp2", 等等。
  - 如果包含能量特征，则第一个特征名称为 "log_En"。

#### 2. FilterBank
- `public float[][] FilterBank { get; }`
  - 获取滤波器组矩阵，维度为 [filterbankSize * (fftSize/2 + 1)]。默认情况下是 Bark 滤波器组。

### 方法

#### 1. PlpExtractor 构造函数
- `public PlpExtractor(PlpOptions options)`
  - 从配置构造 PlpExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的 PLP-RASTA 特征向量。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个 PLP 特征向量）。

#### 3. Reset
- `public override void Reset()`
  - 重置提取器。

#### 4. IsParallelizable
- `public override bool IsParallelizable()`
  - 返回 false 在 RASTA 过滤模式下（即 RASTA 系数不为 0），因为在这种情况下 PlpExtractor 不支持并行化。在所有其他情况下返回 true。

#### 5. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。如果提取器不支持并行化，则返回 null。

### 代码示例
以下是一个使用 PlpExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;

public class PlpExtractorExample
{
    public static void Main()
    {
        // 创建 PlpOptions 实例
        var options = new PlpOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, FeatureCount = 13, LpcOrder = 12, Rasta = 0.1f, FilterBankSize = 24, FftSize = 1024, LifterSize = 22, PreEmphasis = 0.97f, Window = WindowType.Hamming, IncludeEnergy = true, LogEnergyFloor = 1e-10f };
        // 创建 PlpExtractor 实例
        var plpExtractor = new PlpExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = plpExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.PnccExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.PnccExtractor 是一个用于提取功率归一化倒谱系数（PNCC）的特征提取器类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称），例如 "pncc0", "pncc1", "pncc2", 等等。
  - 如果包含能量特征，则第一个特征名称为 "log_En"。

#### 2. M
- `public int M { get; set; } = 2`
  - 获取或设置中值时间功率的窗口长度（2 * M + 1）。

#### 3. N
- `public int N { get; set; } = 4`
  - 获取或设置频谱平滑的窗口长度（2 * N + 1）。

#### 4. LambdaA
- `public float LambdaA { get; set; } = 0.999f`
  - 获取或设置在非对称噪声抑制公式中的 lambda_a。

#### 5. LambdaB
- `public float LambdaB { get; set; } = 0.5f`
  - 获取或设置在非对称噪声抑制公式中的 lambda_b。

#### 6. LambdaT
- `public float LambdaT { get; set; } = 0.85f`
  - 获取或设置时间掩蔽公式中的遗忘因子。

#### 7. LambdaMu
- `public float LambdaMu { get; set; } = 0.999f`
  - 获取或设置公式中的遗忘因子。

#### 8. C
- `public float C { get; set; } = 2`
  - 获取或设置检测激励/非激励段的阈值。

#### 9. MuT
- `public float MuT { get; set; } = 0.2f`
  - 获取或设置公式中的乘数。

#### 10. FilterBank
- `public float[][] FilterBank { get; }`
  - 获取滤波器组矩阵，维度为 [filterbankSize * (fftSize/2 + 1)]。默认情况下是 Gammatone 滤波器组。

### 方法

#### 1. PnccExtractor 构造函数
- `public PnccExtractor(PnccOptions options)`
  - 从配置构造 PnccExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的 PNCC 特征向量。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个 PNCC 特征向量）。

#### 3. Reset
- `public override void Reset()`
  - 重置提取器。

### 代码示例
以下是一个使用 PnccExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;

public class PnccExtractorExample
{
    public static void Main()
    {
        // 创建 PnccOptions 实例
        var options = new PnccOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, FeatureCount = 13, FilterBankSize = 24, FftSize = 1024, PreEmphasis = 0.97f, Window = WindowType.Hamming, IncludeEnergy = true, LogEnergyFloor = 1e-10f, Power = 15 };
        // 创建 PnccExtractor 实例
        var pnccExtractor = new PnccExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = pnccExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.SpnccExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.SpnccExtractor 是一个用于提取简化功率归一化倒谱系数（SPNCC）的特征提取器类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称），例如 "spncc0", "spncc1", "spncc2", 等等。
  - 如果包含能量特征，则第一个特征名称为 "log_En"。

#### 2. LambdaMu
- `public float LambdaMu { get; set; } = 0.999f`
  - 获取或设置公式中的遗忘因子。

#### 3. FilterBank
- `public float[][] FilterBank { get; }`
  - 获取滤波器组矩阵，默认情况下是 Gammatone 滤波器组。

### 方法

#### 1. SpnccExtractor 构造函数
- `public SpnccExtractor(PnccOptions options)`
  - 从配置构造 SpnccExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的 SPNCC 特征向量。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个 SPNCC 特征向量）。

#### 3. Reset
- `public override void Reset()`
  - 重置提取器。

### 代码示例
以下是一个使用 SpnccExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;

public class SpnccExtractorExample
{
    public static void Main()
    {
        // 创建 PnccOptions 实例
        var options = new PnccOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, FeatureCount = 13, FilterBankSize = 24, FftSize = 1024, PreEmphasis = 0.97f, Window = WindowType.Hamming, IncludeEnergy = true, LogEnergyFloor = 1e-10f, Power = 15 };
        // 创建 SpnccExtractor 实例
        var spnccExtractor = new SpnccExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = spnccExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```



## Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.WaveletExtractor 类

Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.WaveletExtractor 是一个用于提取小波变换系数的特征提取器类。

### 属性

#### 1. FeatureDescriptions
- `public override List<string> FeatureDescriptions { get; }`
  - 获取特征的字符串注释（或简单名称），例如 "w0", "w1", "w2", 等等。

#### 2. FilterBank
- `public float[][] FilterBank { get; }`
  - 获取滤波器组矩阵。

### 方法

#### 1. WaveletExtractor 构造函数
- `public WaveletExtractor(WaveletOptions options)`
  - 从配置构造 WaveletExtractor 实例。
  - 参数:
    - `options`: 特征提取器配置选项。

#### 2. ProcessFrame
- `public override void ProcessFrame(float[] block, float[] features)`
  - 计算一个帧中的小波变换系数特征向量。
  - 参数:
    - `block`: 数据块。
    - `features`: 在数据块中计算的特征（一个小波变换系数特征向量）。

#### 3. IsParallelizable
- `public override bool IsParallelizable()`
  - 返回 true，因为 WaveletExtractor 始终支持并行化。

#### 4. ParallelCopy
- `public override FeatureExtractor ParallelCopy()`
  - 创建提取器的线程安全副本以进行并行计算。

### 代码示例
以下是一个使用 WaveletExtractor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.FeatureExtractors;

public class WaveletExtractorExample
{
    public static void Main()
    {
        // 创建 WaveletOptions 实例
        var options = new WaveletOptions { SamplingRate = 44100, FrameDuration = 0.025f, HopDuration = 0.01f, WaveletName = "db4", FeatureCount = 13, FwtSize = 1024, FwtLevel = 5, PreEmphasis = 0.97f, Window = WindowType.Hamming };
        // 创建 WaveletExtractor 实例
        var waveletExtractor = new WaveletExtractor(options);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 计算特征向量
        var featureVectors = waveletExtractor.ComputeFrom(signal.Samples, 0, signal.Length);

        // 输出计算的特征向量
        Console.WriteLine("Feature Vectors:");
        foreach (var vector in featureVectors)
        {
            Console.WriteLine(string.Join(", ", vector));
        }
    }
}
```


---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Features 命名空间。

## Vorcyc.Mathematics.SignalProcessing.Features.Harmonic 类

Vorcyc.Mathematics.SignalProcessing.Features.Harmonic 是一个静态类，提供计算谐波谱特征的方法。

### 方法

#### 1. Peaks
- `public static void Peaks(float[] spectrum, int[] peaks, float[] peakFrequencies, int samplingRate, float pitch = -1)`
  - 查找频谱中的谐波峰值（峰值索引和频率）。
  - 参数:
    - `spectrum`: 频谱。
    - `peaks`: 用于存储计算的峰值位置的数组。
    - `peakFrequencies`: 用于存储计算的峰值频率的数组。
    - `samplingRate`: 采样率。
    - `pitch`: 以 Hz 为单位的音高，如果音高未知，则为负数。

#### 2. Centroid
- `public static float Centroid(float[] spectrum, int[] peaks, float[] peakFrequencies)`
  - 计算谐波质心。
  - 参数:
    - `spectrum`: 频谱。
    - `peaks`: 峰值位置（频谱中的索引）。
    - `peakFrequencies`: 峰值频率。
  - 返回值: 谐波质心。

#### 3. Spread
- `public static float Spread(float[] spectrum, int[] peaks, float[] peakFrequencies)`
  - 计算谐波扩展。
  - 参数:
    - `spectrum`: 频谱。
    - `peaks`: 峰值位置（频谱中的索引）。
    - `peakFrequencies`: 峰值频率。
  - 返回值: 谐波扩展。

#### 4. Inharmonicity
- `public static float Inharmonicity(float[] spectrum, int[] peaks, float[] peakFrequencies)`
  - 计算非谐性。
  - 参数:
    - `spectrum`: 频谱。
    - `peaks`: 峰值位置（频谱中的索引）。
    - `peakFrequencies`: 峰值频率。
  - 返回值: 非谐性。

#### 5. OddToEvenRatio
- `public static float OddToEvenRatio(float[] spectrum, int[] peaks)`
  - 计算奇偶比。
  - 参数:
    - `spectrum`: 频谱。
    - `peaks`: 峰值位置（频谱中的索引）。
  - 返回值: 奇偶比。

#### 6. Tristimulus
- `public static float Tristimulus(float[] spectrum, int[] peaks, int n)`
  - 计算三刺激（第 n 个分量）。
  - 参数:
    - `spectrum`: 频谱。
    - `peaks`: 峰值位置（频谱中的索引）。
    - `n`: 三刺激分量索引：1、2 或 3。
  - 返回值: 三刺激分量。

### 代码示例
以下是一个使用 Harmonic 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Features;

public class HarmonicExample
{
    public static void Main()
    {
        // 定义频谱
        float[] spectrum = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f, 0.85f, 1.0f }; int[] peaks = new int[5];
        float[] peakFrequencies = new float[5]; int samplingRate = 44100;
        // 查找谐波峰值
        Harmonic.Peaks(spectrum, peaks, peakFrequencies, samplingRate);

        // 计算谐波质心
        float centroid = Harmonic.Centroid(spectrum, peaks, peakFrequencies);
        Console.WriteLine($"Harmonic Centroid: {centroid}");

        // 计算谐波扩展
        float spread = Harmonic.Spread(spectrum, peaks, peakFrequencies);
        Console.WriteLine($"Harmonic Spread: {spread}");

        // 计算非谐性
        float inharmonicity = Harmonic.Inharmonicity(spectrum, peaks, peakFrequencies);
        Console.WriteLine($"Inharmonicity: {inharmonicity}");

        // 计算奇偶比
        float oddToEvenRatio = Harmonic.OddToEvenRatio(spectrum, peaks);
        Console.WriteLine($"Odd to Even Ratio: {oddToEvenRatio}");

        // 计算三刺激分量
        float tristimulus1 = Harmonic.Tristimulus(spectrum, peaks, 1);
        float tristimulus2 = Harmonic.Tristimulus(spectrum, peaks, 2);
        float tristimulus3 = Harmonic.Tristimulus(spectrum, peaks, 3);
        Console.WriteLine($"Tristimulus 1: {tristimulus1}");
        Console.WriteLine($"Tristimulus 2: {tristimulus2}");
        Console.WriteLine($"Tristimulus 3: {tristimulus3}");
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Features.Perceptual 类

Vorcyc.Mathematics.SignalProcessing.Features.Perceptual 是一个静态类，提供计算感知音频特征的方法。

### 方法

#### 1. Loudness
- `public static float Loudness(float[] spectralBands)`
  - 计算感知响度（特定响度的总和：N'(z) = E(z)^0.23）。
  - 参数:
    - `spectralBands`: 给定频带中的能量数组。
  - 返回值: 感知响度。

#### 2. Sharpness
- `public static float Sharpness(float[] spectralBands)`
  - 计算感知锐度（本质上是频谱质心的等效物）。
  - 参数:
    - `spectralBands`: 给定频带中的能量数组。
  - 返回值: 感知锐度。

### 代码示例
以下是一个使用 Perceptual 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Features;

public class PerceptualExample
{
    public static void Main()
    {
        // 定义频带能量
        float[] spectralBands = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        // 计算感知响度
        float loudness = Perceptual.Loudness(spectralBands);
        Console.WriteLine($"Loudness: {loudness}");

        // 计算感知锐度
        float sharpness = Perceptual.Sharpness(spectralBands);
        Console.WriteLine($"Sharpness: {sharpness}");
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Features.Pitch 类

Vorcyc.Mathematics.SignalProcessing.Features.Pitch 是一个静态类，提供音高估计和跟踪的方法。

### 方法

#### 1. FromAutoCorrelation
- `public static float FromAutoCorrelation(float[] samples, int samplingRate, int startPos = 0, int endPos = -1, float low = 80, float high = 400)`
  - 使用自相关方法从样本中估计音高。
  - 参数:
    - `samples`: 样本数组。
    - `samplingRate`: 采样率。
    - `startPos`: 要处理的数组中的第一个样本的索引。
    - `endPos`: 要处理的数组中的最后一个样本的索引（不包括）。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
  - 返回值: 估计的音高。

#### 2. FromAutoCorrelation
- `public static float FromAutoCorrelation(DiscreteSignal signal, int startPos = 0, int endPos = -1, float low = 80, float high = 400)`
  - 使用自相关方法从信号中估计音高。
  - 参数:
    - `signal`: 信号。
    - `startPos`: 要处理的信号中的第一个样本的索引。
    - `endPos`: 要处理的信号中的最后一个样本的索引（不包括）。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
  - 返回值: 估计的音高。

#### 3. FromZeroCrossingsSchmitt
- `public static float FromZeroCrossingsSchmitt(float[] samples, int samplingRate, int startPos = 0, int endPos = -1, float lowSchmittThreshold = -1e10f, float highSchmittThreshold = 1e10f)`
  - 基于零交叉率和施密特触发器从样本中估计音高。
  - 参数:
    - `samples`: 样本数组。
    - `samplingRate`: 采样率。
    - `startPos`: 要处理的数组中的第一个样本的索引。
    - `endPos`: 要处理的数组中的最后一个样本的索引（不包括）。
    - `lowSchmittThreshold`: 施密特触发器的下限阈值。
    - `highSchmittThreshold`: 施密特触发器的上限阈值。
  - 返回值: 估计的音高。

#### 4. FromZeroCrossingsSchmitt
- `public static float FromZeroCrossingsSchmitt(DiscreteSignal signal, int startPos = 0, int endPos = -1, float lowSchmittThreshold = -1e10f, float highSchmittThreshold = 1e10f)`
  - 基于零交叉率和施密特触发器从信号中估计音高。
  - 参数:
    - `signal`: 信号。
    - `startPos`: 要处理的信号中的第一个样本的索引。
    - `endPos`: 要处理的信号中的最后一个样本的索引（不包括）。
    - `lowSchmittThreshold`: 施密特触发器的下限阈值。
    - `highSchmittThreshold`: 施密特触发器的上限阈值。
  - 返回值: 估计的音高。

#### 5. FromYin
- `public static float FromYin(float[] samples, int samplingRate, int startPos = 0, int endPos = -1, float low = 80, float high = 400, float cmdfThreshold = 0.2f)`
  - 使用 YIN 算法从样本中估计音高。
  - 参数:
    - `samples`: 样本数组。
    - `samplingRate`: 采样率。
    - `startPos`: 要处理的数组中的第一个样本的索引。
    - `endPos`: 要处理的数组中的最后一个样本的索引（不包括）。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
    - `cmdfThreshold`: CMDF 阈值。
  - 返回值: 估计的音高。

#### 6. FromYin
- `public static float FromYin(DiscreteSignal signal, int startPos = 0, int endPos = -1, float low = 80, float high = 400, float cmdfThreshold = 0.2f)`
  - 使用 YIN 算法从信号中估计音高。
  - 参数:
    - `signal`: 信号。
    - `startPos`: 要处理的信号中的第一个样本的索引。
    - `endPos`: 要处理的信号中的最后一个样本的索引（不包括）。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
    - `cmdfThreshold`: CMDF 阈值。
  - 返回值: 估计的音高。

#### 7. FromHss
- `public static float FromHss(DiscreteSignal signal, int startPos = 0, int endPos = -1, float low = 80, float high = 400, int fftSize = 0)`
  - 使用谐波和频谱（HSS）方法从信号中估计音高。
  - 参数:
    - `signal`: 信号。
    - `startPos`: 要处理的信号中的第一个样本的索引。
    - `endPos`: 要处理的信号中的最后一个样本的索引（不包括）。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
    - `fftSize`: FFT 大小。
  - 返回值: 估计的音高。

#### 8. FromHss
- `public static float FromHss(float[] spectrum, int samplingRate, float low = 80, float high = 400)`
  - 使用谐波和频谱（HSS）方法从频谱中估计音高。
  - 参数:
    - `spectrum`: 频谱。
    - `samplingRate`: 采样率。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
  - 返回值: 估计的音高。

#### 9. FromHps
- `public static float FromHps(DiscreteSignal signal, int startPos = 0, int endPos = -1, float low = 80, float high = 400, int fftSize = 0)`
  - 使用谐波积和频谱（HPS）方法从信号中估计音高。
  - 参数:
    - `signal`: 信号。
    - `startPos`: 要处理的信号中的第一个样本的索引。
    - `endPos`: 要处理的信号中的最后一个样本的索引（不包括）。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
    - `fftSize`: FFT 大小。
  - 返回值: 估计的音高。

#### 10. FromHps
- `public static float FromHps(float[] spectrum, int samplingRate, float low = 80, float high = 400)`
  - 使用谐波积和频谱（HPS）方法从频谱中估计音高。
  - 参数:
    - `spectrum`: 频谱。
    - `samplingRate`: 采样率。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
  - 返回值: 估计的音高。

#### 11. FromSpectralPeaks
- `public static float FromSpectralPeaks(DiscreteSignal signal, int startPos = 0, int endPos = -1, float low = 80, float high = 400, int fftSize = 0)`
  - 基于频谱峰值从信号中估计音高。
  - 参数:
    - `signal`: 信号。
    - `startPos`: 要处理的信号中的第一个样本的索引。
    - `endPos`: 要处理的信号中的最后一个样本的索引（不包括）。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
    - `fftSize`: FFT 大小。
  - 返回值: 估计的音高。

#### 12. FromSpectralPeaks
- `public static float FromSpectralPeaks(float[] spectrum, int samplingRate, float low = 80, float high = 400)`
  - 基于频谱峰值从频谱中估计音高。
  - 参数:
    - `spectrum`: 频谱。
    - `samplingRate`: 采样率。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
  - 返回值: 估计的音高。

#### 13. FromCepstrum
- `public static float FromCepstrum(DiscreteSignal signal, int startPos = 0, int endPos = -1, float low = 80, float high = 400, int cepstrumSize = 256, int fftSize = 1024)`
  - 基于倒谱从信号中估计音高。
  - 参数:
    - `signal`: 信号。
    - `startPos`: 要处理的信号中的第一个样本的索引。
    - `endPos`: 要处理的信号中的最后一个样本的索引（不包括）。
    - `low`: 预期音高范围的下限频率。
    - `high`: 预期音高范围的上限频率。
    - `cepstrumSize`: 倒谱大小。
    - `fftSize`: FFT 大小。
  - 返回值: 估计的音高。

### 代码示例
以下是一个使用 Pitch 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using Vorcyc.Mathematics.SignalProcessing.Features;

public class PitchExample
{
    public static void Main()
    {
        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f }; var signal = new DiscreteSignal(44100, samples);
        // 使用自相关方法估计音高
        float pitchAutoCorrelation = Pitch.FromAutoCorrelation(signal);
        Console.WriteLine($"Pitch (AutoCorrelation): {pitchAutoCorrelation} Hz");

        // 使用零交叉率和施密特触发器估计音高
        float pitchZeroCrossingsSchmitt = Pitch.FromZeroCrossingsSchmitt(signal);
        Console.WriteLine($"Pitch (ZeroCrossingsSchmitt): {pitchZeroCrossingsSchmitt} Hz");

        // 使用 YIN 算法估计音高
        float pitchYin = Pitch.FromYin(signal);
        Console.WriteLine($"Pitch (Yin): {pitchYin} Hz");

        // 使用谐波和频谱（HSS）方法估计音高
        float pitchHss = Pitch.FromHss(signal);
        Console.WriteLine($"Pitch (Hss): {pitchHss} Hz");

        // 使用谐波积和频谱（HPS）方法估计音高
        float pitchHps = Pitch.FromHps(signal);
        Console.WriteLine($"Pitch (Hps): {pitchHps} Hz");

        // 基于频谱峰值估计音高
        float pitchSpectralPeaks = Pitch.FromSpectralPeaks(signal);
        Console.WriteLine($"Pitch (SpectralPeaks): {pitchSpectralPeaks} Hz");

        // 基于倒谱估计音高
        float pitchCepstrum = Pitch.FromCepstrum(signal);
        Console.WriteLine($"Pitch (Cepstrum): {pitchCepstrum} Hz");
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Features.Spectral 类

Vorcyc.Mathematics.SignalProcessing.Features.Spectral 是一个静态类，提供计算谱特征的方法。

### 方法

#### 1. Centroid
- `public static float Centroid(float[] spectrum, float[] frequencies)`
  - 计算谱质心。
  - 参数:
    - `spectrum`: 频谱。
    - `frequencies`: 中心频率。
  - 返回值: 谱质心。

#### 2. Spread
- `public static float Spread(float[] spectrum, float[] frequencies)`
  - 计算谱扩展。
  - 参数:
    - `spectrum`: 频谱。
    - `frequencies`: 中心频率。
  - 返回值: 谱扩展。

#### 3. Decrease
- `public static float Decrease(float[] spectrum)`
  - 计算谱衰减。
  - 参数:
    - `spectrum`: 频谱。
  - 返回值: 谱衰减。

#### 4. Flatness
- `public static float Flatness(float[] spectrum, float minLevel = 1e-10f)`
  - 计算谱平坦度。
  - 参数:
    - `spectrum`: 频谱。
    - `minLevel`: 振幅阈值。
  - 返回值: 谱平坦度。

#### 5. Noiseness
- `public static float Noiseness(float[] spectrum, float[] frequencies, float noiseFrequency = 3000)`
  - 计算谱噪声度。
  - 参数:
    - `spectrum`: 频谱。
    - `frequencies`: 中心频率。
    - `noiseFrequency`: 噪声的下限频率。
  - 返回值: 谱噪声度。

#### 6. Rolloff
- `public static float Rolloff(float[] spectrum, float[] frequencies, float rolloffPercent = 0.85f)`
  - 计算谱滚降频率。
  - 参数:
    - `spectrum`: 频谱。
    - `frequencies`: 中心频率。
    - `rolloffPercent`: 滚降百分比。
  - 返回值: 谱滚降频率。

#### 7. Crest
- `public static float Crest(float[] spectrum)`
  - 计算谱峰度。
  - 参数:
    - `spectrum`: 频谱。
  - 返回值: 谱峰度。

#### 8. Contrast
- `public static float[] Contrast(float[] spectrum, float[] frequencies, float minFrequency = 200, int bandCount = 6)`
  - 计算谱带中的谱对比度数组。
  - 参数:
    - `spectrum`: 频谱。
    - `frequencies`: 中心频率。
    - `minFrequency`: 起始频率。
    - `bandCount`: 谱带数量。
  - 返回值: 谱对比度数组。

#### 9. Contrast
- `public static float Contrast(float[] spectrum, float[] frequencies, int bandNo, float minFrequency = 200)`
  - 计算谱带索引为 `bandNo` 的谱对比度。
  - 参数:
    - `spectrum`: 频谱。
    - `frequencies`: 中心频率。
    - `bandNo`: 谱带索引。
    - `minFrequency`: 起始频率。
  - 返回值: 谱对比度。

#### 10. Entropy
- `public static float Entropy(float[] spectrum)`
  - 计算频谱的香农熵（频谱被视为概率密度函数）。
  - 参数:
    - `spectrum`: 频谱。
  - 返回值: 频谱的香农熵。

### 代码示例
以下是一个使用 Spectral 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Features;

public class SpectralExample
{
    public static void Main()
    {
        // 定义频谱和中心频率
        float[] spectrum = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f, 0.85f, 1.0f };
        float[] frequencies = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };

        // 计算谱质心
        float centroid = Spectral.Centroid(spectrum, frequencies);
        Console.WriteLine($"Spectral Centroid: {centroid}");

        // 计算谱扩展
        float spread = Spectral.Spread(spectrum, frequencies);
        Console.WriteLine($"Spectral Spread: {spread}");

        // 计算谱衰减
        float decrease = Spectral.Decrease(spectrum);
        Console.WriteLine($"Spectral Decrease: {decrease}");

        // 计算谱平坦度
        float flatness = Spectral.Flatness(spectrum);
        Console.WriteLine($"Spectral Flatness: {flatness}");

        // 计算谱噪声度
        float noiseness = Spectral.Noiseness(spectrum, frequencies);
        Console.WriteLine($"Spectral Noiseness: {noiseness}");

        // 计算谱滚降频率
        float rolloff = Spectral.Rolloff(spectrum, frequencies);
        Console.WriteLine($"Spectral Rolloff: {rolloff}");

        // 计算谱峰度
        float crest = Spectral.Crest(spectrum);
        Console.WriteLine($"Spectral Crest: {crest}");

        // 计算谱对比度数组
        float[] contrast = Spectral.Contrast(spectrum, frequencies);
        Console.WriteLine("Spectral Contrast:");
        foreach (var value in contrast)
        {
            Console.WriteLine(value);
        }

        // 计算频谱的香农熵
        float entropy = Spectral.Entropy(spectrum);
        Console.WriteLine($"Spectral Entropy: {entropy}");
    }
}
```


---

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Filters 命名空间。

