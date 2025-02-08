using System.Numerics;

namespace Vorcyc.Mathematics.SignalProcessing;

public static partial class Scale
{
    /// <summary>
    /// 将幅度值转换为dB级别。
    /// </summary>
    /// <param name="value">幅度值</param>
    /// <param name="valueReference">参考幅度值</param>
    public static T ToDecibel<T>(T value, T valueReference)
        where T : struct, IFloatingPointIeee754<T>
    {
        return T.CreateChecked(20) * T.Log10(value / valueReference + T.Epsilon);
    }

    /// <summary>
    /// 将幅度值转换为dB级别（简化版）。
    /// </summary>
    /// <param name="value">幅度值</param>
    public static T ToDecibel<T>(T value)
        where T : struct, IFloatingPointIeee754<T>
    {
        return T.CreateChecked(20) * T.Log10(value);
    }

    /// <summary>
    /// 将功率转换为dB级别。
    /// </summary>
    /// <param name="value">功率值</param>
    /// <param name="valueReference">参考功率值。如果为null，则值为1</param>
    public static T ToDecibelPower<T>(T value, T? valueReference = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        var refValue = valueReference is null ? T.One : valueReference.Value;
        return T.CreateChecked(10) * T.Log10(value / refValue + T.Epsilon);
    }

    /// <summary>
    /// 将dB级别转换为幅度值。
    /// </summary>
    /// <param name="level">dB级别</param>
    /// <param name="valueReference">参考幅度值</param>
    public static T FromDecibel<T>(T level, T valueReference)
        where T : struct, IFloatingPointIeee754<T>
    {
        return valueReference * T.Pow(T.CreateChecked(10), level / T.CreateChecked(20));
    }

    /// <summary>
    /// 将dB级别转换为幅度值（简化版）。
    /// </summary>
    /// <param name="level">dB级别</param>
    public static T FromDecibel<T>(T level)
        where T : struct, IFloatingPointIeee754<T>
    {
        return T.Pow(T.CreateChecked(10), level / T.CreateChecked(20));
    }

    /// <summary>
    /// 将dB级别转换为功率值。
    /// </summary>
    /// <param name="level">dB级别</param>
    /// <param name="valueReference">参考功率值</param>
    public static T FromDecibelPower<T>(T level, T? valueReference = null)
        where T : struct, IFloatingPointIeee754<T>
    {
        var refValue = valueReference is null ? T.One : valueReference.Value;
        return refValue * T.Pow(T.CreateChecked(10), level / T.CreateChecked(10));
    }

    /// <summary>
    /// 将赫兹频率转换为对应的梅尔频率。
    /// </summary>
    public static T HerzToMel<T>(T herz)
        where T : struct, IFloatingPointIeee754<T>
    {
        return T.CreateChecked(1127) * T.Log(herz / T.CreateChecked(700) + T.One); // 实际上应该是1127.01048，但HTK和Kaldi似乎使用1127
    }

    /// <summary>
    /// 将梅尔频率转换为对应的赫兹频率。
    /// </summary>
    public static T MelToHerz<T>(T mel)
        where T : struct, IFloatingPointIeee754<T>
    {
        return (T.Exp(mel / T.CreateChecked(1127)) - T.One) * T.CreateChecked(700);
    }

    /// <summary>
    /// 将赫兹频率转换为梅尔频率（由M.Slaney建议）。
    /// </summary>
    public static T HerzToMelSlaney<T>(T herz)
        where T : struct, IFloatingPointIeee754<T>
    {
        T minHerz = T.Zero;
        T sp = T.CreateChecked(200) / T.CreateChecked(3);
        T minLogHerz = T.CreateChecked(1000);
        T minLogMel = (minLogHerz - minHerz) / sp;
        var logStep = T.Log(T.CreateChecked(6.4)) / T.CreateChecked(27);
        return herz < minLogHerz ? (herz - minHerz) / sp : minLogMel + T.Log(herz / minLogHerz) / logStep;
    }

    /// <summary>
    /// 将梅尔频率转换为赫兹频率（由M.Slaney建议）。
    /// </summary>
    public static T MelToHerzSlaney<T>(T mel)
        where T : struct, IFloatingPointIeee754<T>
    {
        T minHerz = T.Zero;
        T sp = T.CreateChecked(200.0 / 3);
        T minLogHerz = T.CreateChecked(1000);
        T minLogMel = (minLogHerz - minHerz) / sp;

        var logStep = T.Log(T.CreateChecked(6.4)) / T.CreateChecked(27);

        return mel < minLogMel ? minHerz + sp * mel : minLogHerz * T.Exp(logStep * (mel - minLogMel));
    }

    /// <summary>
    /// 将赫兹频率转换为对应的巴克频率（根据Traunmüller (1990)）。
    /// </summary>
    public static T HerzToBark<T>(T herz)
        where T : struct, IFloatingPointIeee754<T>
    {
        return (T.CreateChecked(26.81) * herz) / (T.CreateChecked(1960) + herz) - T.CreateChecked(0.53);
    }

    /// <summary>
    /// 将巴克频率转换为对应的赫兹频率（根据Traunmüller (1990)）。
    /// </summary>
    public static T BarkToHerz<T>(T bark)
        where T : struct, IFloatingPointIeee754<T>
    {
        return T.CreateChecked(1960) / (T.CreateChecked(26.81) / (bark + T.CreateChecked(0.53)) - T.One);
    }

    /// <summary>
    /// 将赫兹频率转换为对应的巴克频率（根据Wang (1992)）；用于M.Slaney的听觉工具箱。
    /// </summary>
    public static T HerzToBarkSlaney<T>(T herz)
        where T : struct, IFloatingPointIeee754<T>
    {
        return T.CreateChecked(6) * TrigonometryHelper.Asinh(herz / T.CreateChecked(600));
    }

    /// <summary>
    /// 将巴克频率转换为对应的赫兹频率（根据Wang (1992)）；用于M.Slaney的听觉工具箱。
    /// </summary>
    public static T BarkToHerzSlaney<T>(T bark)
        where T : struct, IFloatingPointIeee754<T>
    {
        return T.CreateChecked(600) * T.Sinh(bark / T.CreateChecked(6));
    }

    /// <summary>
    /// 将赫兹频率转换为对应的ERB频率。
    /// </summary>
    public static T HerzToErb<T>(T herz)
        where T : struct, IFloatingPointIeee754<T>
    {
        return T.CreateChecked(9.26449) * T.Log(T.One + herz) / T.CreateChecked(24.7 * 9.26449);
    }

    /// <summary>
    /// 将ERB频率转换为对应的赫兹频率。
    /// </summary>
    public static T ErbToHerz<T>(T erb)
        where T : struct, IFloatingPointIeee754<T>
    {
        return (T.Exp(erb / T.CreateChecked(9.26449)) - T.One) * T.CreateChecked(24.7 * 9.26449);
    }

    /// <summary>
    /// 将赫兹频率转换为八度（用于构建类似librosa的Chroma滤波器组）。
    /// </summary>
    public static T HerzToOctave<T>(T herz, T tuning = default, int binsPerOctave = 12)
        where T : struct, IFloatingPointIeee754<T>
    {
        var a440 = T.CreateChecked(440.0) * T.Pow(T.CreateChecked(2.0), tuning / T.CreateChecked(binsPerOctave));

        return T.Log(T.CreateChecked(16) * herz / a440, T.CreateChecked(2));
    }

    /// <summary>
    /// 返回感知响度权重（以dB为单位）。
    /// </summary>
    /// <param name="frequency">频率</param>
    /// <param name="weightingType">权重类型（A, B, C）</param>
    public static T LoudnessWeighting<T>(T frequency, string weightingType = "A")
        where T : struct, IFloatingPointIeee754<T>
    {
        var level2 = frequency * frequency;

        switch (weightingType.ToUpper())
        {
            case "B":
                {
                    var r = (level2 * frequency * T.CreateChecked(148693636)) /
                             (
                                (level2 + T.CreateChecked(424.36)) *
                                 T.Sqrt(level2 + T.CreateChecked(25122.25)) *
                                (level2 + T.CreateChecked(148693636))
                             );
                    return T.CreateChecked(20) * T.Log10(r) + T.CreateChecked(0.17);
                }

            case "C":
                {
                    var r = (level2 * T.CreateChecked(148693636)) /
                             (
                                 (level2 + T.CreateChecked(424.36)) *
                                 (level2 + T.CreateChecked(148693636))
                             );
                    return T.CreateChecked(20) * T.Log10(r) + T.CreateChecked(0.06);
                }

            default:
                {
                    var r = (level2 * level2 * T.CreateChecked(148693636)) /
                             (
                                 (level2 + T.CreateChecked(424.36)) *
                                  T.Sqrt((level2 + T.CreateChecked(11599.29)) * (level2 + T.CreateChecked(544496.41))) *
                                 (level2 + T.CreateChecked(148693636))
                             );
                    return T.CreateChecked(20) * T.Log10(r) + T.CreateChecked(2.0);
                }
        }
    }
}
