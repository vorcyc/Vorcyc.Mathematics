当前位置 : [根目录](README.md)/[信号处理模块](Module_SignalProcessing.md)/[音频特征提取](Module_SignalProcessing_FeatureExtractors.md)

# 信号处理模块 - Signal Processing Module
## 音频特征提取 - Feature Extractors

`Vorcyc.Mathematics.SignalProcessing.FeatureExtractors` 命名空间包含多种音频特征提取器类，包括 `Mpeg7SpectralFeaturesExtractor`、`SpectralFeaturesExtractor`、`TimeDomainFeaturesExtractor`、`AmsExtractor`、`ChromaExtractor`、`FilterbankExtractor`、`LpccExtractor`、`LpcExtractor`、`MfccExtractor`、`PitchExtractor`、`PlpExtractor`、`PnccExtractor`、`SpnccExtractor` 和 `WaveletExtractor`。这些类提供了丰富的音频特征提取功能，从时域特征、谱特征到各种倒谱系数和小波变换系数，适用于各种音频信号处理需求。


> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.FeatureExtractors

---

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi.Mpeg7SpectralFeaturesExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorsmultimpeg7spectralfeaturesextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi.SpectralFeaturesExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorsmultispectralfeaturesextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.Multi.TimeDomainFeaturesExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorsmultitimedomainfeaturesextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.AmsExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorsamsextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.ChromaExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorschromaextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.FilterbankExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorsfilterbankextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.LpccExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorslpccextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.LpcExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorslpcextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.MfccExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorsmfccextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.PitchExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorspitchextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.PlpExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorsplpextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.PnccExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorspnccextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.SpnccExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorsspnccextractor-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.FeatureExtractors.WaveletExtractor 类](#vorcycmathematicssignalprocessingfeatureextractorswaveletextractor-类)  

---

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