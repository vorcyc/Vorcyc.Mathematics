当前位置 : [根目录](README.md)/[信号处理模块](Module_SignalProcessing.md)/[其它](Module_SignalProcessing_Others.md)

# 信号处理模块 - Signal Processing Module
## 其它 - Others

> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing

---

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FractionalDelayLine 类](#vorcycmathematicssignalprocessingfractionaldelayline-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Lpc 类](#vorcycmathematicssignalprocessinglpc-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Scale 类](#vorcycmathematicssignalprocessingscale-类)  

---


## Vorcyc.Mathematics.SignalProcessing.FractionalDelayLine 类

`Vorcyc.Mathematics.SignalProcessing.FractionalDelayLine` 表示分数延迟线，用于实现信号处理中的分数延迟。

### 属性

#### 1. InterpolationMode
- `public InterpolationMode InterpolationMode { get; set; }`
  - 获取或设置插值模式。

#### 2. Size
- `public int Size { get; }`
  - 获取延迟线的大小（样本数）。

### 方法

#### 1. FractionalDelayLine 构造函数
- `public FractionalDelayLine(int size, InterpolationMode interpolationMode = InterpolationMode.Linear)`
  - 构造一个 `FractionalDelayLine` 实例，并为其样本保留给定的大小。
  - 参数:
    - `size`: 延迟线大小（样本数）。
    - `interpolationMode`: 插值模式。

- `public FractionalDelayLine(int samplingRate, double maxDelay, InterpolationMode interpolationMode = InterpolationMode.Linear)`
  - 构造一个 `FractionalDelayLine` 实例，并为其样本保留与给定最大延迟时间相对应的大小。
  - 参数:
    - `samplingRate`: 采样率。
    - `maxDelay`: 最大延迟时间（秒）。
    - `interpolationMode`: 插值模式。

#### 2. Write
- `public void Write(float sample)`
  - 将样本写入延迟线。
  - 参数:
    - `sample`: 样本。

#### 3. Read
- `public float Read(double delay)`
  - 从延迟线读取与给定延迟时间相对应的样本。
  - 参数:
    - `delay`: 延迟时间（秒）。
  - 返回值: 样本值。

#### 4. Reset
- `public void Reset()`
  - 重置延迟线。

#### 5. Ensure
- `public void Ensure(int size)`
  - 调整延迟线的大小以确保新的大小。如果新的大小不超过当前延迟线的大小，则不执行任何操作。
  - 参数:
    - `size`: 新的大小。

- `public void Ensure(int samplingRate, double maxDelay)`
  - 调整延迟线的大小以确保与给定最大延迟时间相对应的新大小。如果新的大小不超过当前延迟线的大小，则不执行任何操作。
  - 参数:
    - `samplingRate`: 采样率。
    - `maxDelay`: 最大延迟时间（秒）。

- `public void Ensure(int samplingRate, float maxDelay)`
  - 调整延迟线的大小以确保与给定最大延迟时间相对应的新大小。如果新的大小不超过当前延迟线的大小，则不执行任何操作。
  - 参数:
    - `samplingRate`: 采样率。
    - `maxDelay`: 最大延迟时间（秒）。

### 代码示例
以下是一个使用 FractionalDelayLine 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using System.Drawing;
using Vorcyc.Mathematics.SignalProcessing;

public class FractionalDelayLineExample
{
    public static void Main()
    {
        // 定义延迟线大小和插值模式
        int size = 10;
        var interpolationMode = InterpolationMode.Linear;

        // 创建 FractionalDelayLine 实例
        var delayLine = new FractionalDelayLine(size, interpolationMode);

        // 写入样本到延迟线
        for (int i = 0; i < size; i++)
        {
            delayLine.Write(i);
        }

        // 读取样本从延迟线
        double delay = 2.5;
        var sample = delayLine.Read(delay);
        Console.WriteLine($"Sample at delay {delay}: {sample}");

        // 重置延迟线
        delayLine.Reset();

        // 确保延迟线的新大小
        int newSize = 20;
        delayLine.Ensure(newSize);
        Console.WriteLine($"New size of delay line: {delayLine.Size}");

        // 确保延迟线的新大小（基于采样率和最大延迟时间）
        int samplingRate = 44100;
        double maxDelay = 0.5;
        delayLine.Ensure(samplingRate, maxDelay);
        Console.WriteLine($"New size of delay line (based on sampling rate and max delay): {delayLine.Size}");
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Lpc 类

`Vorcyc.Mathematics.SignalProcessing.Lpc` 提供了与线性预测编码 (LPC) 相关的函数。

### 方法

#### 1. LevinsonDurbin
- `public static float LevinsonDurbin(float[] input, float[] a, int order, int offset = 0)`
  - 使用 Levinson-Durbin 算法评估 LP 系数并返回预测误差。
  - 参数:
    - `input`: 自相关向量。
    - `a`: LP 系数。
    - `order`: LPC 的阶数。
    - `offset`: 自相关向量的可选偏移量。
  - 返回值: 预测误差。

#### 2. ToCepstrum
- `public static void ToCepstrum(float[] lpc, float gain, float[] lpcc)`
  - 将 LPC 系数转换为 LPC 倒谱 (LPCC)。
  - 参数:
    - `lpc`: LPC 向量。
    - `gain`: 增益。
    - `lpcc`: LPC 倒谱。

#### 3. FromCepstrum
- `public static float FromCepstrum(float[] lpcc, float[] lpc)`
  - 将 LPC 倒谱转换为 LPC 系数并返回增益。
  - 参数:
    - `lpcc`: LPC 倒谱。
    - `lpc`: LPC 向量。
  - 返回值: 增益。

#### 4. EstimateOrder
- `public static int EstimateOrder(int samplingRate)`
  - 根据最佳实践估计给定采样率的 LPC 阶数。
  - 参数:
    - `samplingRate`: 采样率。
  - 返回值: LPC 阶数。

#### 5. ToLsf
- `public static void ToLsf(float[] lpc, float[] lsf)`
  - 将 LPC 系数转换为线谱频率 (LSF)。`lsf` 的长度必须等于 `lpc` 的长度，最后一个元素将是 PI。
  - 参数:
    - `lpc`: LPC 向量。
    - `lsf`: 线谱频率。

#### 6. FromLsf
- `public static void FromLsf(float[] lsf, float[] lpc)`
  - 将线谱频率 (LSF) 转换为 LPC 系数。`lsf` 的长度必须等于 `lpc` 的长度，最后一个元素必须是 PI。
  - 参数:
    - `lsf`: 线谱频率。
    - `lpc`: LPC 向量。

### 代码示例
以下是一个使用 Lpc 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;

public class LpcExample
{
    public static void Main()
    {
        // 定义自相关向量和 LPC 阶数
        var input = new float[] { 1.0f, 0.9f, 0.7f, 0.5f, 0.3f }; var order = 4;

        // 计算 LPC 系数
        var a = new float[order + 1];
        var error = Lpc.LevinsonDurbin(input, a, order);
        Console.WriteLine("LPC Coefficients:");

        foreach (var coeff in a)
        {
            Console.WriteLine(coeff);
        }
        Console.WriteLine($"Prediction Error: {error}");

        // 将 LPC 系数转换为 LPC 倒谱
        var lpcc = new float[order + 1];
        var gain = 1.0f;
        Lpc.ToCepstrum(a, gain, lpcc);
        Console.WriteLine("LPC Cepstrum:");

        foreach (var coeff in lpcc)
        {
            Console.WriteLine(coeff);
        }

        // 将 LPC 倒谱转换回 LPC 系数
        var lpcFromCepstrum = new float[order + 1];
        var gainFromCepstrum = Lpc.FromCepstrum(lpcc, lpcFromCepstrum);
        Console.WriteLine("LPC Coefficients from Cepstrum:");

        foreach (var coeff in lpcFromCepstrum)
        {
            Console.WriteLine(coeff);
        }
        Console.WriteLine($"Gain from Cepstrum: {gainFromCepstrum}");

        // 估计给定采样率的 LPC 阶数
        var samplingRate = 44100;
        var estimatedOrder = Lpc.EstimateOrder(samplingRate);
        Console.WriteLine($"Estimated LPC Order for sampling rate {samplingRate}: {estimatedOrder}");

        // 将 LPC 系数转换为线谱频率 (LSF)
        var lsf = new float[order + 1];
        Lpc.ToLsf(a, lsf);
        Console.WriteLine("Line Spectral Frequencies (LSF):");

        foreach (var freq in lsf)
        {
            Console.WriteLine(freq);
        }

        // 将线谱频率 (LSF) 转换回 LPC 系数
        var lpcFromLsf = new float[order + 1];
        Lpc.FromLsf(lsf, lpcFromLsf);
        Console.WriteLine("LPC Coefficients from LSF:");

        foreach (var coeff in lpcFromLsf)
        {
            Console.WriteLine(coeff);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Scale 类

Vorcyc.Mathematics.SignalProcessing.Scale 提供了用于在不同尺度之间进行转换的方法，包括分贝、MIDI音高、mel频率、bark频率和ERB频率等。

### 方法

#### 1. ToDecibel
- `public static double ToDecibel(double value, double valueReference)`
  - 将幅度值转换为分贝级别。
  - 参数:
    - `value`: 幅度值。
    - `valueReference`: 参考幅度值。
  - 返回值: 分贝级别。

- `public static double ToDecibel(double value)`
  - 将幅度值转换为分贝级别（简化版本）。
  - 参数:
    - `value`: 幅度值。
  - 返回值: 分贝级别。

- `public static float ToDecibel(float value)`
  - 将幅度值转换为分贝级别（简化版本）。
  - 参数:
    - `value`: 幅度值。
  - 返回值: 分贝级别。

#### 2. ToDecibelPower
- `public static double ToDecibelPower(double value, double valueReference = 1.0)`
  - 将功率值转换为分贝级别。
  - 参数:
    - `value`: 功率值。
    - `valueReference`: 参考功率值。
  - 返回值: 分贝级别。

- `public static float ToDecibelPower(float value, float valueReference = 1.0f)`
  - 将功率值转换为分贝级别。
  - 参数:
    - `value`: 功率值。
    - `valueReference`: 参考功率值。
  - 返回值: 分贝级别。

#### 3. FromDecibel
- `public static double FromDecibel(double level, double valueReference)`
  - 将分贝级别转换为幅度值。
  - 参数:
    - `level`: 分贝级别。
    - `valueReference`: 参考幅度值。
  - 返回值: 幅度值。

- `public static float FromDecibel(float level, float valueReference)`
  - 将分贝级别转换为幅度值。
  - 参数:
    - `level`: 分贝级别。
    - `valueReference`: 参考幅度值。
  - 返回值: 幅度值。

- `public static double FromDecibel(double level)`
  - 将分贝级别转换为幅度值（简化版本）。
  - 参数:
    - `level`: 分贝级别。
  - 返回值: 幅度值。

- `public static float FromDecibel(float level)`
  - 将分贝级别转换为幅度值（简化版本）。
  - 参数:
    - `level`: 分贝级别。
  - 返回值: 幅度值。

#### 4. FromDecibelPower
- `public static double FromDecibelPower(double level, double valueReference = 1.0)`
  - 将分贝级别转换为功率值。
  - 参数:
    - `level`: 分贝级别。
    - `valueReference`: 参考功率值。
  - 返回值: 功率值。

#### 5. PitchToFreq
- `public static double PitchToFreq(int pitch)`
  - 将MIDI音高转换为频率（以Hz为单位）。
  - 参数:
    - `pitch`: 音高。
  - 返回值: 频率（Hz）。

#### 6. FreqToPitch
- `public static int FreqToPitch(double freq)`
  - 将频率转换为MIDI音高。
  - 参数:
    - `freq`: 频率（Hz）。
  - 返回值: 音高。

#### 7. NoteToFreq
- `public static double NoteToFreq(string note, int octave)`
  - 将音乐音符（如 "G", 3）转换为频率（Hz）。
  - 参数:
    - `note`: 音符（A-G#）。
    - `octave`: 八度（0-8）。
  - 返回值: 频率（Hz）。

#### 8. FreqToNote
- `public static (string note, int octave) FreqToNote(double freq)`
  - 将频率（Hz）转换为音符（如 "G", 3）。
  - 参数:
    - `freq`: 频率（Hz）。
  - 返回值: 音符和八度。

#### 9. HerzToMel
- `public static double HerzToMel(double herz)`
  - 将赫兹频率转换为mel频率。
  - 参数:
    - `herz`: 赫兹频率。
  - 返回值: mel频率。

- `public static float HerzToMel(float herz)`
  - 将赫兹频率转换为mel频率。
  - 参数:
    - `herz`: 赫兹频率。
  - 返回值: mel频率。

#### 10. MelToHerz
- `public static double MelToHerz(double mel)`
  - 将mel频率转换为赫兹频率。
  - 参数:
    - `mel`: mel频率。
  - 返回值: 赫兹频率。

- `public static float MelToHerz(float mel)`
  - 将mel频率转换为赫兹频率。
  - 参数:
    - `mel`: mel频率。
  - 返回值: 赫兹频率。

#### 11. HerzToMelSlaney
- `public static double HerzToMelSlaney(double herz)`
  - 将赫兹频率转换为mel频率（M.Slaney建议）。
  - 参数:
    - `herz`: 赫兹频率。
  - 返回值: mel频率。

- `public static float HerzToMelSlaney(float herz)`
  - 将赫兹频率转换为mel频率（M.Slaney建议）。
  - 参数:
    - `herz`: 赫兹频率。
  - 返回值: mel频率。

#### 12. MelToHerzSlaney
- `public static double MelToHerzSlaney(double mel)`
  - 将mel频率转换为赫兹频率（M.Slaney建议）。
  - 参数:
    - `mel`: mel频率。
  - 返回值: 赫兹频率。

- `public static float MelToHerzSlaney(float mel)`
  - 将mel频率转换为赫兹频率（M.Slaney建议）。
  - 参数:
    - `mel`: mel频率。
  - 返回值: 赫兹频率。

#### 13. HerzToBark
- `public static double HerzToBark(double herz)`
  - 将赫兹频率转换为bark频率（根据Traunmüller (1990)）。
  - 参数:
    - `herz`: 赫兹频率。
  - 返回值: bark频率。

- `public static float HerzToBark(float herz)`
  - 将赫兹频率转换为bark频率（根据Traunmüller (1990)）。
  - 参数:
    - `herz`: 赫兹频率。
  - 返回值: bark频率。

#### 14. BarkToHerz
- `public static double BarkToHerz(double bark)`
  - 将bark频率转换为赫兹频率（根据Traunmüller (1990)）。
  - 参数:
    - `bark`: bark频率。
  - 返回值: 赫兹频率。

- `public static float BarkToHerz(float bark)`
  - 将bark频率转换为赫兹频率（根据Traunmüller (1990)）。
  - 参数:
    - `bark`: bark频率。
  - 返回值: 赫兹频率。

#### 15. HerzToBarkSlaney
- `public static double HerzToBarkSlaney(double herz)`
  - 将赫兹频率转换为bark频率（根据Wang (1992)）。
  - 参数:
    - `herz`: 赫兹频率。
  - 返回值: bark频率。

- `public static float HerzToBarkSlaney(float herz)`
  - 将赫兹频率转换为bark频率（根据Wang (1992)）。
  - 参数:
    - `herz`: 赫兹频率。
  - 返回值: bark频率。

#### 16. BarkToHerzSlaney
- `public static double BarkToHerzSlaney(double bark)`
  - 将bark频率转换为赫兹频率（根据Wang (1992)）。
  - 参数:
    - `bark`: bark频率。
  - 返回值: 赫兹频率。

- `public static float BarkToHerzSlaney(float bark)`
  - 将bark频率转换为赫兹频率（根据Wang (1992)）。
  - 参数:
    - `bark`: bark频率。
  - 返回值: 赫兹频率。

#### 17. HerzToErb
- `public static double HerzToErb(double herz)`
  - 将赫兹频率转换为ERB频率。
  - 参数:
    - `herz`: 赫兹频率。
  - 返回值: ERB频率。

- `public static float HerzToErb(float herz)`
  - 将赫兹频率转换为ERB频率。
  - 参数:
    - `herz`: 赫兹频率。
  - 返回值: ERB频率。

#### 18. ErbToHerz
- `public static double ErbToHerz(double erb)`
  - 将ERB频率转换为赫兹频率。
  - 参数:
    - `erb`: ERB频率。
  - 返回值: 赫兹频率。

- `public static float ErbToHerz(float erb)`
  - 将ERB频率转换为赫兹频率。
  - 参数:
    - `erb`: ERB频率。
  - 返回值: 赫兹频率。

#### 19. HerzToOctave
- `public static double HerzToOctave(double herz, double tuning = 0, int binsPerOctave = 12)`
  - 将赫兹频率转换为八度（用于构建类似librosa的Chroma滤波器）。
  - 参数:
    - `herz`: 赫兹频率。
    - `tuning`: 调音。
    - `binsPerOctave`: 每八度的音阶数。
  - 返回值: 八度。

- `public static float HerzToOctave(float herz, float tuning = 0, int binsPerOctave = 12)`
  - 将赫兹频率转换为八度（用于构建类似librosa的Chroma滤波器）。
  - 参数:
    - `herz`: 赫兹频率。
    - `tuning`: 调音。
    - `binsPerOctave`: 每八度的音阶数。
  - 返回值: 八度。

#### 20. LoudnessWeighting
- `public static double LoudnessWeighting(double frequency, string weightingType = "A")`
  - 返回感知响度权重（以分贝为单位）。
  - 参数:
    - `frequency`: 频率。
    - `weightingType`: 权重类型（A, B, C）。
  - 返回值: 响度权重（分贝）。

- `public static float LoudnessWeighting(float frequency, string weightingType = "A")`
  - 返回感知响度权重（以分贝为单位）。
  - 参数:
    - `frequency`: 频率。
    - `weightingType`: 权重类型（A, B, C）。
  - 返回值: 响度权重（分贝）。

### 示例代码
以下是一个使用 `Scale` 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing;
using static System.Formats.Asn1.AsnWriter;

public class ScaleExample
{
    public static void Main()
    {
        // 将幅度值转换为分贝级别
        double magnitude = 0.5;
        double decibel = Scale.ToDecibel(magnitude);
        Console.WriteLine($"Magnitude {magnitude} to Decibel: {decibel}");

        // 将分贝级别转换为幅度值
        double level = -6.0;
        double magnitudeFromDb = Scale.FromDecibel(level);
        Console.WriteLine($"Decibel {level} to Magnitude: {magnitudeFromDb}");

        // 将MIDI音高转换为频率
        int pitch = 69;
        double frequency = Scale.PitchToFreq(pitch);
        Console.WriteLine($"Pitch {pitch} to Frequency: {frequency} Hz");

        // 将频率转换为MIDI音高
        double freq = 440.0;
        int midiPitch = Scale.FreqToPitch(freq);
        Console.WriteLine($"Frequency {freq} Hz to Pitch: {midiPitch}");

        // 将赫兹频率转换为mel频率
        double herz = 1000.0;
        double mel = Scale.HerzToMel(herz);
        Console.WriteLine($"Herz {herz} to Mel: {mel}");

        // 将mel频率转换为赫兹频率
        double herzFromMel = Scale.MelToHerz(mel);
        Console.WriteLine($"Mel {mel} to Herz: {herzFromMel}");
    }
}
```
