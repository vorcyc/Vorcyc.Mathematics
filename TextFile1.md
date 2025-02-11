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