> 当前位置 : [根目录](README.md)/[信号处理模块](Module_SignalProcessing.md)/[音效处理](Module_SignalProcessing_Effects.md)

# 信号处理模块 - Signal Processing Module
## 音效处理 - Audio Effect

> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Effects

---

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Base.AudioEffect 类](#vorcycmathematicssignalprocessingeffectsbaseaudioeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Base.WetDryMixer 类](#vorcycmathematicssignalprocessingeffectsbasewetdrymixer-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.BinauralPanEffect 类](#vorcycmathematicssignalprocessingeffectsstereobinauralpaneffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.ItdIldPanEffect 类](#vorcycmathematicssignalprocessingeffectsstereoitdildpaneffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.PanEffect 类](#vorcycmathematicssignalprocessingeffectsstereopaneffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.PingPongDelayEffect 类](#vorcycmathematicssignalprocessingeffectsstereopingpongdelayeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.StereoDelayEffect 类](#vorcycmathematicssignalprocessingeffectsstereostereodelayeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.Stereo.StereoEffect 类](#vorcycmathematicssignalprocessingeffectsstereostereoeffect-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Effects.AutowahEffect 类](#vorcycmathematicssignalprocessingeffectsautowaheffect-类)  
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


## Vorcyc.Mathematics.SignalProcessing.Effects.AutowahEffect 类

Vorcyc.Mathematics.SignalProcessing.Effects.AutowahEffect 是一个用于自动哇音效的音频效果类，结合了包络跟随器和哇音效果。

### 属性

#### 1. Q
- `public float Q { get; set; }`
  - 获取或设置 Q 因子（也称为质量因子，谐振）。

#### 2. MinFrequency
- `public float MinFrequency { get; set; }`
  - 获取或设置最小 LFO 频率（以 Hz 为单位）。

#### 3. MaxFrequency
- `public float MaxFrequency { get; set; }`
  - 获取或设置最大 LFO 频率（以 Hz 为单位）。

#### 4. AttackTime
- `public float AttackTime { get; set; }`
  - 获取或设置攻击时间（以秒为单位）。

#### 5. ReleaseTime
- `public float ReleaseTime { get; set; }`
  - 获取或设置释放时间（以秒为单位）。

### 方法

#### 1. AutowahEffect 构造函数
- `public AutowahEffect(int samplingRate, float minFrequency = 30, float maxFrequency = 2000, float q = 0.5f, float attackTime = 0.01f, float releaseTime = 0.05f)`
  - 构造 AutowahEffect 实例。
  - 参数:
    - `samplingRate`: 采样率。
    - `minFrequency`: 最小 LFO 频率（以 Hz 为单位），默认为 30 Hz。
    - `maxFrequency`: 最大 LFO 频率（以 Hz 为单位），默认为 2000 Hz。
    - `q`: Q 因子（也称为质量因子，谐振），默认为 0.5。
    - `attackTime`: 攻击时间（以秒为单位），默认为 0.01 秒。
    - `releaseTime`: 释放时间（以秒为单位），默认为 0.05 秒。

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
以下是一个使用 AutowahEffect 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Effects;

public class AutowahEffectExample
{
    public static void Main()
    {
        // 创建 AutowahEffect 实例
        var autowahEffect = new AutowahEffect(44100, 30, 2000, 0.5f, 0.01f, 0.05f);

        // 定义输入信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var signal = new DiscreteSignal(44100, samples);

        // 处理样本
        foreach (var sample in signal.Samples)
        {
            var processedSample = autowahEffect.Process(sample);
            Console.WriteLine($"Processed Sample: {processedSample}");
        }
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