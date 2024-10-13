namespace Vorcyc.Mathematics.Experimental.Signals;

/// <summary>
/// 定义信号生成时的行为选项。
/// </summary>
public enum Behaviour
{
    /// <summary>
    /// 替换现有信号。
    /// </summary>
    Replace,

    /// <summary>
    /// 与现有信号逐元素相加。
    /// </summary>
    ElementWiseAdd,

    /// <summary>
    /// 与现有信号逐元素相减。
    /// </summary>
    ElementWiseSubtract,

    /// <summary>
    /// 与现有信号逐元素相乘。
    /// </summary>
    ElementWiseMultiply,

    /// <summary>
    /// 与现有信号逐元素相除。
    /// </summary>
    ElementWiseDivide,
}

/// <summary>
/// 定义不同的波形类型。
/// </summary>
public enum WaveShape
{
    /// <summary>
    /// 正弦波。
    /// </summary>
    Sine,

    /// <summary>
    /// 余弦波。
    /// </summary>
    Cosine,

    /// <summary>
    /// 方波。
    /// </summary>
    Square,

    /// <summary>
    /// 锯齿波。
    /// </summary>
    Sawtooth,

    /// <summary>
    /// 三角波。
    /// </summary>
    Triangle,

    /// <summary>
    /// 白噪声。
    /// </summary>
    WhiteNoise,

    /// <summary>
    /// 粉红噪声。
    /// </summary>
    PinkNoise
}


public static class SignalExtension
{

    /// <summary>
    /// 生成指定波形，并根据行为对信号进行处理。
    /// </summary>
    /// <param name="signal">表示信号的对象。</param>
    /// <param name="shape">波形类型。</param>
    /// <param name="frequency">波形的频率。</param>
    public static void GenerateWave(this ITimeDomainSignal signal, WaveShape shape, float frequency, Behaviour behaviour = Behaviour.Replace)
    {
        Action<int, float> action = null;
        switch (behaviour)
        {
            case Behaviour.Replace:
                action = (index, sample) => signal.Samples[index] = sample;
                break;
            case Behaviour.ElementWiseAdd:
                action = (index, sample) => signal.Samples[index] += sample;
                break;
            case Behaviour.ElementWiseSubtract:
                action = (index, sample) => signal.Samples[index] -= sample;
                break;
            case Behaviour.ElementWiseMultiply:
                action = (index, sample) => signal.Samples[index] *= sample;
                break;
            case Behaviour.ElementWiseDivide:
                action = (index, sample) => signal.Samples[index] /= sample;
                break;
            default:
                break;
        }

        switch (shape)
        {
            case WaveShape.Sine:
                GenerateSineWave(signal, frequency, action);
                break;
            case WaveShape.Cosine:
                GenerateCosineWave(signal, frequency, action);
                break;
            case WaveShape.Square:
                GenerateSquareWave(signal, frequency, action);
                break;
            case WaveShape.Sawtooth:
                GenerateSawtoothWave(signal, frequency, action);
                break;
            case WaveShape.Triangle:
                GenerateTriangleWave(signal, frequency, action);
                break;
            case WaveShape.WhiteNoise:
                GenerateWhiteNoise(signal, action);
                break;
            case WaveShape.PinkNoise:
                GeneratePinkNoise(signal, action);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(shape), shape, null);


        }
    }

    ///// <summary>
    ///// 生成指定波形，并根据行为对信号进行处理。
    ///// </summary>
    ///// <param name="signal">表示信号的对象。</param>
    ///// <param name="shape">波形类型。</param>
    ///// <param name="frequency">波形的频率。</param>
    ///// <param name="behaviour">处理行为。</param>
    //public static void GenerateWave(this ITimeDomainSignal signal, WaveShape shape, float frequency, Behaviour behaviour = Behaviour.Replace)
    //{
    //    switch (shape)
    //    {
    //        case WaveShape.SineWave:
    //            switch (behaviour)
    //            {
    //                case Behaviour.Replace:
    //                    GenerateSineWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] = sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseAdd:
    //                    GenerateSineWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] += sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseSubtract:
    //                    GenerateSineWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] -= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseMultiply:
    //                    GenerateSineWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] *= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseDivide:
    //                    GenerateSineWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] /= sample;
    //                    });
    //                    break;
    //                default:
    //                    break;
    //            }
    //            break;
    //        case WaveShape.CosineWave:
    //            switch (behaviour)
    //            {
    //                case Behaviour.Replace:
    //                    GenerateCosineWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] = sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseAdd:
    //                    GenerateCosineWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] += sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseSubtract:
    //                    GenerateCosineWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] -= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseMultiply:
    //                    GenerateCosineWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] *= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseDivide:
    //                    GenerateCosineWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] /= sample;
    //                    });
    //                    break;
    //                default:
    //                    break;
    //            }
    //            break;
    //        case WaveShape.SquareWave:
    //            switch (behaviour)
    //            {
    //                case Behaviour.Replace:
    //                    GenerateSquareWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] = sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseAdd:
    //                    GenerateSquareWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] += sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseSubtract:
    //                    GenerateSquareWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] -= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseMultiply:
    //                    GenerateSquareWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] *= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseDivide:
    //                    GenerateSquareWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] /= sample;
    //                    });
    //                    break;
    //                default:
    //                    break;
    //            }
    //            break;
    //        case WaveShape.SawtoothWave:
    //            switch (behaviour)
    //            {
    //                case Behaviour.Replace:
    //                    GenerateSawtoothWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] = sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseAdd:
    //                    GenerateSawtoothWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] += sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseSubtract:
    //                    GenerateSawtoothWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] -= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseMultiply:
    //                    GenerateSawtoothWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] *= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseDivide:
    //                    GenerateSawtoothWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] /= sample;
    //                    });
    //                    break;
    //                default:
    //                    break;
    //            }
    //            break;
    //        case WaveShape.TriangleWave:
    //            switch (behaviour)
    //            {
    //                case Behaviour.Replace:
    //                    GenerateTriangleWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] = sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseAdd:
    //                    GenerateTriangleWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] += sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseSubtract:
    //                    GenerateTriangleWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] -= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseMultiply:
    //                    GenerateTriangleWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] *= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseDivide:
    //                    GenerateTriangleWave(signal, frequency, (index, sample) =>
    //                    {
    //                        signal.Samples[index] /= sample;
    //                    });
    //                    break;
    //                default:
    //                    break;
    //            }
    //            break;
    //        case WaveShape.WhiteNoise:
    //            switch (behaviour)
    //            {
    //                case Behaviour.Replace:
    //                    GenerateWhiteNoise(signal, (index, sample) =>
    //                    {
    //                        signal.Samples[index] = sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseAdd:
    //                    GenerateWhiteNoise(signal, (index, sample) =>
    //                    {
    //                        signal.Samples[index] += sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseSubtract:
    //                    GenerateWhiteNoise(signal, (index, sample) =>
    //                    {
    //                        signal.Samples[index] -= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseMultiply:
    //                    GenerateWhiteNoise(signal, (index, sample) =>
    //                    {
    //                        signal.Samples[index] *= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseDivide:
    //                    GenerateWhiteNoise(signal, (index, sample) =>
    //                    {
    //                        signal.Samples[index] /= sample;
    //                    });
    //                    break;
    //                default:
    //                    break;
    //            }
    //            break;
    //        case WaveShape.PinkNoise:
    //            switch (behaviour)
    //            {
    //                case Behaviour.Replace:
    //                    GeneratePinkNoise(signal, (index, sample) =>
    //                    {
    //                        signal.Samples[index] = sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseAdd:
    //                    GeneratePinkNoise(signal, (index, sample) =>
    //                    {
    //                        signal.Samples[index] += sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseSubtract:
    //                    GeneratePinkNoise(signal, (index, sample) =>
    //                    {
    //                        signal.Samples[index] -= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseMultiply:
    //                    GeneratePinkNoise(signal, (index, sample) =>
    //                    {
    //                        signal.Samples[index] *= sample;
    //                    });
    //                    break;
    //                case Behaviour.ElementWiseDivide:
    //                    GeneratePinkNoise(signal, (index, sample) =>
    //                    {
    //                        signal.Samples[index] /= sample;
    //                    });
    //                    break;
    //                default:
    //                    break;
    //            }
    //            break;
    //        default:
    //            break;
    //    }
    //}



    /// <summary>
    /// 生成正弦波，并对每个生成的值执行指定操作。
    /// </summary>
    /// <param name="signal">表示信号的对象。</param>
    /// <param name="frequency">正弦波的频率。</param>
    /// <param name="action">对每个生成的值执行的操作。</param>
    internal static void GenerateSineWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)
    {
        float increment = ConstantsFp32.TWO_PI * frequency / signal.SamplingRate;
        float phase = 0f;

        for (int i = 0; i < signal.Length; i++)
        {
            var value = MathF.Sin(phase);
            action(i, value);
            phase += increment;

            // 确保相位在0到2π之间
            if (phase >= ConstantsFp32.TWO_PI)
            {
                phase -= ConstantsFp32.TWO_PI;
            }
        }
    }

    /// <summary>
    /// 生成余弦波，并对每个生成的值执行指定操作。
    /// </summary>
    /// <param name="signal">表示信号的对象。</param>
    /// <param name="frequency">余弦波的频率。</param>
    /// <param name="action">对每个生成的值执行的操作。</param>
    internal static void GenerateCosineWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)
    {
        float omega = 2 * ConstantsFp32.PI * frequency / signal.SamplingRate;

        for (int i = 0; i < signal.Length; i++)
        {
            var value = MathF.Cos(omega * i);
            action(i, value);
        }

    }

    /// <summary>
    /// 生成方波，并对每个生成的值执行指定操作。
    /// </summary>
    /// <param name="signal">表示信号的对象。</param>
    /// <param name="frequency">方波的频率。</param>
    /// <param name="action">对每个生成的值执行的操作。</param>
    internal static void GenerateSquareWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)
    {
        float increment = ConstantsFp32.TWO_PI * frequency / signal.SamplingRate;

        for (int i = 0; i < signal.Length; i++)
        {
            var value = MathF.Sign(MathF.Sin(increment * i));
            action(i, value);
        }
    }

    /// <summary>
    /// 生成锯齿波，并对每个生成的值执行指定操作。
    /// </summary>
    /// <param name="signal">表示信号的对象。</param>
    /// <param name="frequency">锯齿波的频率。</param>
    /// <param name="action">对每个生成的值执行的操作。</param>
    internal static void GenerateSawtoothWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)
    {
        float increment = frequency / signal.SamplingRate;

        for (int i = 0; i < signal.Length; i++)
        {
            var value = 2 * (i * increment - MathF.Floor(0.5f + i * increment));
            action(i, value);
        }
    }

    /// <summary>
    /// 生成三角波，并对每个生成的值执行指定操作。
    /// </summary>
    /// <param name="signal">表示信号的对象。</param>
    /// <param name="frequency">三角波的频率。</param>
    /// <param name="action">对每个生成的值执行的操作。</param>
    internal static void GenerateTriangleWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)
    {
        float increment = frequency / signal.SamplingRate;

        for (int i = 0; i < signal.Length; i++)
        {
            var value = 2 * MathF.Abs(2 * (i * increment - MathF.Floor(i * increment + 0.5f))) - 1;
            action(i, value);
        }
    }

    /// <summary>
    /// 生成白噪声，并对每个生成的值执行指定操作。
    /// </summary>
    /// <param name="signal">表示信号的对象。</param>
    /// <param name="action">对每个生成的值执行的操作。</param>
    internal static void GenerateWhiteNoise(ITimeDomainSignal signal, Action<int, float> action)
    {
        for (int i = 0; i < signal.Length; i++)
        {
            var value = 2 * Random.Shared.NextSingle() - 1; // 生成范围在 -1 到 1 之间的随机数
            action(i, value);
        }
    }

    /// <summary>
    /// 生成粉红噪声，并对每个生成的值执行指定操作。
    /// </summary>
    /// <param name="signal">表示信号的对象。</param>
    /// <param name="action">对每个生成的值执行的操作。</param>
    internal static void GeneratePinkNoise(ITimeDomainSignal signal, Action<int, float> action)
    {
        float[] whiteNoise = new float[signal.Length];

        // 生成白噪声
        for (int i = 0; i < signal.Length; i++)
        {
            whiteNoise[i] = 2 * Random.Shared.NextSingle() - 1; // 生成范围在 -1 到 1 之间的随机数
        }

        // 使用简单的一极滤波器生成粉红噪声
        float b0 = 0.99765f, b1 = 0.96300f, b2 = 0.57000f;
        float a0 = 0.96400f, a1 = 0.76700f, a2 = 0.53500f;
        float[] noiseHistory = new float[7];

        for (int i = 0; i < signal.Length; i++)
        {
            noiseHistory[0] = b0 * whiteNoise[i] + b1 * noiseHistory[1] + b2 * noiseHistory[2] - a0 * noiseHistory[3] - a1 * noiseHistory[4] - a2 * noiseHistory[5];
            action(i, noiseHistory[0]);
            Array.Copy(noiseHistory, 0, noiseHistory, 1, 6);
        }
    }

}
