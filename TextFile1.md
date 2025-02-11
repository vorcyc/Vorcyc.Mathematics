## Vorcyc.Mathematics.SignalProcessing.Features.Spectral ��

Vorcyc.Mathematics.SignalProcessing.Features.Spectral ��һ����̬�࣬�ṩ�����������ķ�����

### ����

#### 1. Centroid
- `public static float Centroid(float[] spectrum, float[] frequencies)`
  - ���������ġ�
  - ����:
    - `spectrum`: Ƶ�ס�
    - `frequencies`: ����Ƶ�ʡ�
  - ����ֵ: �����ġ�

#### 2. Spread
- `public static float Spread(float[] spectrum, float[] frequencies)`
  - ��������չ��
  - ����:
    - `spectrum`: Ƶ�ס�
    - `frequencies`: ����Ƶ�ʡ�
  - ����ֵ: ����չ��

#### 3. Decrease
- `public static float Decrease(float[] spectrum)`
  - ������˥����
  - ����:
    - `spectrum`: Ƶ�ס�
  - ����ֵ: ��˥����

#### 4. Flatness
- `public static float Flatness(float[] spectrum, float minLevel = 1e-10f)`
  - ������ƽ̹�ȡ�
  - ����:
    - `spectrum`: Ƶ�ס�
    - `minLevel`: �����ֵ��
  - ����ֵ: ��ƽ̹�ȡ�

#### 5. Noiseness
- `public static float Noiseness(float[] spectrum, float[] frequencies, float noiseFrequency = 3000)`
  - �����������ȡ�
  - ����:
    - `spectrum`: Ƶ�ס�
    - `frequencies`: ����Ƶ�ʡ�
    - `noiseFrequency`: ����������Ƶ�ʡ�
  - ����ֵ: �������ȡ�

#### 6. Rolloff
- `public static float Rolloff(float[] spectrum, float[] frequencies, float rolloffPercent = 0.85f)`
  - �����׹���Ƶ�ʡ�
  - ����:
    - `spectrum`: Ƶ�ס�
    - `frequencies`: ����Ƶ�ʡ�
    - `rolloffPercent`: �����ٷֱȡ�
  - ����ֵ: �׹���Ƶ�ʡ�

#### 7. Crest
- `public static float Crest(float[] spectrum)`
  - �����׷�ȡ�
  - ����:
    - `spectrum`: Ƶ�ס�
  - ����ֵ: �׷�ȡ�

#### 8. Contrast
- `public static float[] Contrast(float[] spectrum, float[] frequencies, float minFrequency = 200, int bandCount = 6)`
  - �����״��е��׶Աȶ����顣
  - ����:
    - `spectrum`: Ƶ�ס�
    - `frequencies`: ����Ƶ�ʡ�
    - `minFrequency`: ��ʼƵ�ʡ�
    - `bandCount`: �״�������
  - ����ֵ: �׶Աȶ����顣

#### 9. Contrast
- `public static float Contrast(float[] spectrum, float[] frequencies, int bandNo, float minFrequency = 200)`
  - �����״�����Ϊ `bandNo` ���׶Աȶȡ�
  - ����:
    - `spectrum`: Ƶ�ס�
    - `frequencies`: ����Ƶ�ʡ�
    - `bandNo`: �״�������
    - `minFrequency`: ��ʼƵ�ʡ�
  - ����ֵ: �׶Աȶȡ�

#### 10. Entropy
- `public static float Entropy(float[] spectrum)`
  - ����Ƶ�׵���ũ�أ�Ƶ�ױ���Ϊ�����ܶȺ�������
  - ����:
    - `spectrum`: Ƶ�ס�
  - ����ֵ: Ƶ�׵���ũ�ء�

### ����ʾ��
������һ��ʹ�� Spectral ���ж��������ʾ��������ʾ���м�����ע�ͣ�

```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Features;

public class SpectralExample
{
    public static void Main()
    {
        // ����Ƶ�׺�����Ƶ��
        float[] spectrum = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f, 0.85f, 1.0f };
        float[] frequencies = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };

        // ����������
        float centroid = Spectral.Centroid(spectrum, frequencies);
        Console.WriteLine($"Spectral Centroid: {centroid}");

        // ��������չ
        float spread = Spectral.Spread(spectrum, frequencies);
        Console.WriteLine($"Spectral Spread: {spread}");

        // ������˥��
        float decrease = Spectral.Decrease(spectrum);
        Console.WriteLine($"Spectral Decrease: {decrease}");

        // ������ƽ̹��
        float flatness = Spectral.Flatness(spectrum);
        Console.WriteLine($"Spectral Flatness: {flatness}");

        // ������������
        float noiseness = Spectral.Noiseness(spectrum, frequencies);
        Console.WriteLine($"Spectral Noiseness: {noiseness}");

        // �����׹���Ƶ��
        float rolloff = Spectral.Rolloff(spectrum, frequencies);
        Console.WriteLine($"Spectral Rolloff: {rolloff}");

        // �����׷��
        float crest = Spectral.Crest(spectrum);
        Console.WriteLine($"Spectral Crest: {crest}");

        // �����׶Աȶ�����
        float[] contrast = Spectral.Contrast(spectrum, frequencies);
        Console.WriteLine("Spectral Contrast:");
        foreach (var value in contrast)
        {
            Console.WriteLine(value);
        }

        // ����Ƶ�׵���ũ��
        float entropy = Spectral.Entropy(spectrum);
        Console.WriteLine($"Spectral Entropy: {entropy}");
    }
}
```