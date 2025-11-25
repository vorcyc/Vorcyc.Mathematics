当前位置 : [根目录](README.md)/[信号处理模块](Module_SignalProcessing.md)/[其他音频特征计算](Module_SignalProcessing_Features.md)

# 信号处理模块 - Signal Processing Module
## 其他音频特征计算 - Features

`Vorcyc.Mathematics.SignalProcessing.Features` 命名空间包含多种音频特征计算类，包括 `Harmonic`、`Perceptual`、`Pitch` 和 `Spectral`。这些类提供了丰富的音频特征计算功能，如谐波特征、感知特征、音高估计和谱特征，适用于各种音频信号处理需求。

> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Features 命名空间。

---

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Features.Harmonic 类](#vorcycmathematicssignalprocessingfeaturesharmonic-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Features.Perceptual 类](#vorcycmathematicssignalprocessingfeaturesperceptual-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Features.Pitch 类](#vorcycmathematicssignalprocessingfeaturespitch-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Features.Spectral 类](#vorcycmathematicssignalprocessingfeaturesspectral-类)  

---


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