namespace Vorcyc.Mathematics.SignalProcessing.Signals;
/// <summary>
/// 瀹氫箟淇″彿鐢熸垚鏃剁殑琛屼负閫夐」銆?
/// </summary>
public enum Behaviour
{
    /// <summary>
    /// 鏇挎崲鐜版湁淇″彿銆?
    /// </summary>
    Replace,
    /// <summary>
    /// 涓庣幇鏈変俊鍙烽€愬厓绱犵浉鍔犮€?
    /// </summary>
    ElementWiseAdd,
    /// <summary>
    /// 涓庣幇鏈変俊鍙烽€愬厓绱犵浉鍑忋€?
    /// </summary>
    ElementWiseSubtract,
    /// <summary>
    /// 涓庣幇鏈変俊鍙烽€愬厓绱犵浉涔樸€?
    /// </summary>
    ElementWiseMultiply,
    /// <summary>
    /// 涓庣幇鏈変俊鍙烽€愬厓绱犵浉闄ゃ€?
    /// </summary>
    ElementWiseDivide,
}
/// <summary>
/// 瀹氫箟涓嶅悓鐨勬尝褰㈢被鍨嬨€?
/// </summary>
public enum WaveShape
{
    /// <summary>
    /// 姝ｅ鸡娉€?
    /// </summary>
    Sine,
    /// <summary>
    /// 浣欏鸡娉€?
    /// </summary>
    Cosine,
    /// <summary>
    /// 鏂规尝銆?
    /// </summary>
    Square,
    /// <summary>
    /// 閿娇娉€?
    /// </summary>
    Sawtooth,
    /// <summary>
    /// 涓夎娉€?
    /// </summary>
    Triangle,
    /// <summary>
    /// 鐧藉櫔澹般€?
    /// </summary>
    WhiteNoise,
    /// <summary>
    /// 绮夌孩鍣０銆?
    /// </summary>
    PinkNoise,
    /// <summary>
    /// Linear frequency chirp (sweep). <c>frequency</c> is the start frequency;
    /// end frequency defaults to <c>min(frequency×10, 0.45×Nyquist)</c>.
    /// </summary>
    Chirp,
    /// <summary>
    /// Periodic pulse train. <c>frequency</c> is the pulse rate (period = 1/f);
    /// duty cycle defaults to 50%.
    /// </summary>
    Pulse,
    /// <summary>
    /// Linear ramp from -1 to +1 over the signal length. <c>frequency</c> is ignored.
    /// </summary>
    Ramp,
    /// <summary>
    /// Scaled sinc. <c>frequency</c> controls the sinc width (same as <see cref="Generators.SincGenerator.Frequency"/>).
    /// </summary>
    Sinc,
    /// <summary>
    /// Red (Brownian) noise. <c>frequency</c> is ignored.
    /// </summary>
    RedNoise,
    /// <summary>
    /// Additive white Gaussian noise (AWGN). <c>frequency</c> is ignored.
    /// </summary>
    Awgn,
}
/// <summary>
/// Provides extension methods for generating and applying various waveforms and noise types to time-domain signals.
/// </summary>
/// <remarks>This static class contains methods that extend the functionality of time-domain signal objects,
/// enabling the generation of standard waveforms such as sine, cosine, square, sawtooth, triangle, as well as white and
/// pink noise. The generated signals can be applied to the target signal using different element-wise operations, such
/// as replacement, addition, subtraction, multiplication, or division. These methods are intended for use in digital
/// signal processing scenarios where programmatic waveform synthesis or signal manipulation is required.</remarks>
public static class SignalGeneratingExtension
{
    /// <summary>
    /// Generates a waveform of the specified shape and frequency, and applies it to the signal using the specified
    /// behavior.
    /// </summary>
    /// <remarks>The method modifies the samples of the provided signal in place according to the specified behavior.
    /// When using noise shapes, the frequency parameter may be ignored.</remarks>
    /// <param name="signal">The signal to which the generated waveform will be applied.</param>
    /// <param name="shape">The shape of the waveform to generate (sine, cosine, square, sawtooth, triangle,
    /// white/pink/red noise, AWGN, chirp, pulse, ramp, sinc).</param>
    /// <param name="frequency">The frequency of the waveform to generate, in hertz. The interpretation of this value depends on the waveform shape.</param>
    /// <param name="behaviour">The behavior that determines how the generated waveform is combined with the existing signal. Defaults to <see
    /// cref="Behaviour.Replace"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="shape"/> is not a valid <see cref="WaveShape"/> value.</exception>
    public static void GenerateWave(this ISingleThreadTimeDomainSignal signal, WaveShape shape, float frequency, Behaviour behaviour = Behaviour.Replace)
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
            case WaveShape.Chirp:
                GenerateChirp(signal, frequency, action);
                break;
            case WaveShape.Pulse:
                GeneratePulse(signal, frequency, action);
                break;
            case WaveShape.Ramp:
                GenerateRamp(signal, action);
                break;
            case WaveShape.Sinc:
                GenerateSinc(signal, frequency, action);
                break;
            case WaveShape.RedNoise:
                GenerateRedNoise(signal, action);
                break;
            case WaveShape.Awgn:
                GenerateAwgn(signal, action);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
        }
        signal.NotifySamplesModified();
    }
    //public static void GenerateWave(this IModifiableTimeDomainSignal signal, WaveShape shape, float frequency, Behaviour behaviour = Behaviour.Replace)
    //{
    //    Action<int, float> action = null;
    //    switch (behaviour)
    //    {
    //        case Behaviour.Replace:
    //            using (var view = signal.Samples)
    //            {
    //                action = (index, sample) => view.Span[index] = sample;
    //            }
    //            break;
    //        case Behaviour.ElementWiseAdd:
    //            action = (index, sample) => signal.Samples[index] += sample;
    //            break;
    //        case Behaviour.ElementWiseSubtract:
    //            action = (index, sample) => signal.Samples[index] -= sample;
    //            break;
    //        case Behaviour.ElementWiseMultiply:
    //            action = (index, sample) => signal.Samples[index] *= sample;
    //            break;
    //        case Behaviour.ElementWiseDivide:
    //            action = (index, sample) => signal.Samples[index] /= sample;
    //            break;
    //        default:
    //            break;
    //    }
    //    switch (shape)
    //    {
    //        case WaveShape.Sine:
    //            GenerateSineWave(signal, frequency, action);
    //            break;
    //        case WaveShape.Cosine:
    //            GenerateCosineWave(signal, frequency, action);
    //            break;
    //        case WaveShape.Square:
    //            GenerateSquareWave(signal, frequency, action);
    //            break;
    //        case WaveShape.Sawtooth:
    //            GenerateSawtoothWave(signal, frequency, action);
    //            break;
    //        case WaveShape.Triangle:
    //            GenerateTriangleWave(signal, frequency, action);
    //            break;
    //        case WaveShape.WhiteNoise:
    //            GenerateWhiteNoise(signal, action);
    //            break;
    //        case WaveShape.PinkNoise:
    //            GeneratePinkNoise(signal, action);
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
    //    }
    //}
    ///// <summary>
    ///// 鐢熸垚鎸囧畾娉㈠舰锛屽苟鏍规嵁琛屼负瀵逛俊鍙疯繘琛屽鐞嗐€?
    ///// </summary>
    ///// <param name="signal">琛ㄧず淇″彿鐨勫璞°€?/param>
    ///// <param name="shape">娉㈠舰绫诲瀷銆?/param>
    ///// <param name="frequency">娉㈠舰鐨勯鐜囥€?/param>
    ///// <param name="behaviour">澶勭悊琛屼负銆?/param>
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
    /// 鐢熸垚姝ｅ鸡娉紝骞跺姣忎釜鐢熸垚鐨勫€兼墽琛屾寚瀹氭搷浣溿€?
    /// </summary>
    /// <param name="signal">琛ㄧず淇″彿鐨勫璞°€?/param>
    /// <param name="frequency">姝ｅ鸡娉㈢殑棰戠巼銆?/param>
    /// <param name="action">瀵规瘡涓敓鎴愮殑鍊兼墽琛岀殑鎿嶄綔銆?/param>
    internal static void GenerateSineWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)
    {
        float increment = ConstantsFp32.TWO_PI * frequency / signal.SamplingRate;
        float phase = 0f;
        for (int i = 0; i < signal.Length; i++)
        {
            var value = MathF.Sin(phase);
            action(i, value);
            phase += increment;
            // 纭繚鐩镐綅鍦?鍒?蟺涔嬮棿
            if (phase >= ConstantsFp32.TWO_PI)
            {
                phase -= ConstantsFp32.TWO_PI;
            }
        }
    }
    /// <summary>
    /// 鐢熸垚浣欏鸡娉紝骞跺姣忎釜鐢熸垚鐨勫€兼墽琛屾寚瀹氭搷浣溿€?
    /// </summary>
    /// <param name="signal">琛ㄧず淇″彿鐨勫璞°€?/param>
    /// <param name="frequency">浣欏鸡娉㈢殑棰戠巼銆?/param>
    /// <param name="action">瀵规瘡涓敓鎴愮殑鍊兼墽琛岀殑鎿嶄綔銆?/param>
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
    /// 鐢熸垚鏂规尝锛屽苟瀵规瘡涓敓鎴愮殑鍊兼墽琛屾寚瀹氭搷浣溿€?
    /// </summary>
    /// <param name="signal">琛ㄧず淇″彿鐨勫璞°€?/param>
    /// <param name="frequency">鏂规尝鐨勯鐜囥€?/param>
    /// <param name="action">瀵规瘡涓敓鎴愮殑鍊兼墽琛岀殑鎿嶄綔銆?/param>
    internal static void GenerateSquareWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)
    {
        //float increment = ConstantsFp32.TWO_PI * frequency / signal.SamplingRate;
        //for (int i = 0; i < signal.Length; i++)
        //{
        //    var value = MathF.Sign(MathF.Sin(increment * i));
        //    action(i, value);
        //}
        //var multiple = 2.0f * frequency / signal.SamplingRate;
        //for (int i = 0; i < signal.Length; i++)
        //{
        //    var sampleSaw = ((i * multiple) % 2) - 1;
        //    var sample = sampleSaw > 0 ? 1 : -1;
        //    action(i, sample);
        //}
        var period = signal.SamplingRate / frequency;
        for (int i = 0; i < signal.Length; i++)
        {
            var sample = (i % period) < (period / 2) ? 1.0f : -1.0f;
            action(i, sample);
        }
    }
    /// <summary>
    /// 鐢熸垚閿娇娉紝骞跺姣忎釜鐢熸垚鐨勫€兼墽琛屾寚瀹氭搷浣溿€?
    /// </summary>
    /// <param name="signal">琛ㄧず淇″彿鐨勫璞°€?/param>
    /// <param name="frequency">閿娇娉㈢殑棰戠巼銆?/param>
    /// <param name="action">瀵规瘡涓敓鎴愮殑鍊兼墽琛岀殑鎿嶄綔銆?/param>
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
    /// 鐢熸垚涓夎娉紝骞跺姣忎釜鐢熸垚鐨勫€兼墽琛屾寚瀹氭搷浣溿€?
    /// </summary>
    /// <param name="signal">琛ㄧず淇″彿鐨勫璞°€?/param>
    /// <param name="frequency">涓夎娉㈢殑棰戠巼銆?/param>
    /// <param name="action">瀵规瘡涓敓鎴愮殑鍊兼墽琛岀殑鎿嶄綔銆?/param>
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
    /// 鐢熸垚鐧藉櫔澹帮紝骞跺姣忎釜鐢熸垚鐨勫€兼墽琛屾寚瀹氭搷浣溿€?
    /// </summary>
    /// <param name="signal">琛ㄧず淇″彿鐨勫璞°€?/param>
    /// <param name="action">瀵规瘡涓敓鎴愮殑鍊兼墽琛岀殑鎿嶄綔銆?/param>
    internal static void GenerateWhiteNoise(ITimeDomainSignal signal, Action<int, float> action)
    {
        for (int i = 0; i < signal.Length; i++)
        {
            var value = 2f * Random.Shared.NextSingle() - 1f; // 鐢熸垚鑼冨洿鍦?-1 鍒?1 涔嬮棿鐨勯殢鏈烘暟
            action(i, value);
        }
    }
    /// <summary>
    /// Pink (1/f) noise via Paul Kellet's refined filter, approximately in [-1, 1].
    /// </summary>
    internal static void GeneratePinkNoise(ITimeDomainSignal signal, Action<int, float> action)
    {
        var gen = new Generators.PinkNoiseGenerator { Min = -1f, Max = 1f };
        ApplyGenerator(signal, gen, action);
    }

    /// <summary>
    /// Applies samples from <paramref name="generator"/> via <paramref name="action"/>.
    /// </summary>
    private static void ApplyGenerator(ITimeDomainSignal signal, Generators.ISampleGenerator generator, Action<int, float> action)
    {
        generator.Reset();
        for (int i = 0; i < signal.Length; i++)
            action(i, generator.NextSample());
    }

    /// <summary>
    /// Linear chirp: start = <paramref name="frequency"/>, end = min(frequency×10, 0.45×Nyquist).
    /// </summary>
    internal static void GenerateChirp(ITimeDomainSignal signal, float frequency, Action<int, float> action)
    {
        float start = MathF.Max(frequency, 0f);
        float nyquist = signal.SamplingRate * 0.5f;
        float end = MathF.Min(MathF.Max(start * 10f, start + 1f), nyquist * 0.45f);
        var gen = new Generators.ChirpOscillator
        {
            SamplingRate = signal.SamplingRate,
            Length = signal.Length,
            StartFrequency = start,
            EndFrequency = end,
            Min = -1f,
            Max = 1f,
        };
        ApplyGenerator(signal, gen, action);
    }

    /// <summary>
    /// Pulse train at <paramref name="frequency"/> Hz with 50% duty cycle.
    /// </summary>
    internal static void GeneratePulse(ITimeDomainSignal signal, float frequency, Action<int, float> action)
    {
        float freq = MathF.Max(frequency, 1e-6f);
        float period = 1f / freq;
        var gen = new Generators.PulseWaveGenerator
        {
            SamplingRate = signal.SamplingRate,
            Period = period,
            PulseDuration = period * 0.5f,
            Min = -1f,
            Max = 1f,
        };
        ApplyGenerator(signal, gen, action);
    }

    /// <summary>
    /// Linear ramp from -1 to +1 over the signal length.
    /// </summary>
    internal static void GenerateRamp(ITimeDomainSignal signal, Action<int, float> action)
    {
        int n = Math.Max(signal.Length - 1, 1);
        var gen = new Generators.RampGenerator
        {
            Slope = 2f / n,
            Intercept = -1f,
        };
        ApplyGenerator(signal, gen, action);
    }

    /// <summary>
    /// Scaled sinc with the given frequency parameter.
    /// </summary>
    internal static void GenerateSinc(ITimeDomainSignal signal, float frequency, Action<int, float> action)
    {
        var gen = new Generators.SincGenerator
        {
            SamplingRate = signal.SamplingRate,
            Frequency = MathF.Max(frequency, 0f),
            Min = -1f,
            Max = 1f,
        };
        ApplyGenerator(signal, gen, action);
    }

    /// <summary>
    /// Red (Brownian) noise in approximately [-1, 1].
    /// </summary>
    internal static void GenerateRedNoise(ITimeDomainSignal signal, Action<int, float> action)
    {
        var gen = new Generators.RedNoiseGenerator { Min = -1f, Max = 1f };
        ApplyGenerator(signal, gen, action);
    }

    /// <summary>
    /// Additive white Gaussian noise (mean 0, σ ≈ 0.25).
    /// </summary>
    internal static void GenerateAwgn(ITimeDomainSignal signal, Action<int, float> action)
    {
        var gen = new Generators.AwgnGenerator { Mean = 0f, Sigma = 0.25f };
        ApplyGenerator(signal, gen, action);
    }
}
