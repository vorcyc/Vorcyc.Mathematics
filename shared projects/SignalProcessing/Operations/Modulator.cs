using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Signals;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

namespace Vorcyc.Mathematics.SignalProcessing.Operations;

/// <summary>
/// Provides various modulation methods:
/// <list type="bullet">
///     <item>ring</item>
///     <item>amplitude</item>
///     <item>frequency</item>
///     <item>phase</item>
/// </list>
/// </summary>
public class Modulator
{
    /// <summary>
    /// Does ring modulation (RM) and returns RM signal.
    /// </summary>
    /// <param name="carrier">Carrier signal</param>
    /// <param name="modulator">Modulator signal</param>
    public static Signal Ring(Signal carrier, Signal modulator)
    {
        if (MathF.Abs(carrier.SamplingRate - modulator.SamplingRate) > 1e-5f)
        {
            throw new ArgumentException("Sampling rates must be the same!");
        }

        var output = new float[carrier.Length];
        for (var i = 0; i < carrier.Length; i++)
        {
            output[i] = carrier[i] * modulator[i];
        }

        return Signal.FromCopy(output, carrier.SamplingRate);
    }

    /// <summary>
    /// Does amplitude modulation (AM) and returns AM signal.
    /// </summary>
    /// <param name="carrier">Carrier signal</param>
    /// <param name="modulatorFrequency">Modulator frequency</param>
    /// <param name="modulationIndex">Modulation index (depth)</param>
    public static Signal Amplitude(Signal carrier,
                                   float modulatorFrequency = 20/*Hz*/,
                                   float modulationIndex = 0.5f)
    {
        var fs = carrier.SamplingRate;
        var mf = modulatorFrequency;
        var mi = modulationIndex;

        var output = new float[carrier.Length];
        for (var i = 0; i < carrier.Length; i++)
        {
            output[i] = carrier[i] * (1 + mi * MathF.Cos(2 * MathF.PI * mf / fs * i));
        }

        return Signal.FromCopy(output, fs);
    }

    /// <summary>
    /// Does frequency modulation (FM) and returns FM signal.
    /// </summary>
    /// <param name="baseband">Baseband signal</param>
    /// <param name="carrierAmplitude">Carrier amplitude</param>
    /// <param name="carrierFrequency">Carrier frequency</param>
    /// <param name="deviation">Frequency deviation</param>
    public static Signal Frequency(Signal baseband,
                                   float carrierAmplitude,
                                   float carrierFrequency,
                                   float deviation = 0.1f/*Hz*/)
    {
        var fs = baseband.SamplingRate;
        var ca = carrierAmplitude;
        var cf = carrierFrequency;

        var integral = 0.0;
        var output = new float[baseband.Length];
        for (var i = 0; i < baseband.Length; i++)
        {
            integral += baseband[i];
            output[i] = (float)(ca * Math.Cos(2 * Math.PI * cf / fs * i + 2 * Math.PI * deviation * integral));
        }

        return Signal.FromCopy(output, fs);
    }

    /// <summary>
    /// Does sinusoidal frequency modulation (FM) and returns sinusoidal FM signal.
    /// </summary>
    /// <param name="carrierFrequency">Carrier signal frequency</param>
    /// <param name="carrierAmplitude">Carrier signal amplitude</param>
    /// <param name="modulatorFrequency">Modulator frequency</param>
    /// <param name="modulationIndex">Modulation index (depth)</param>
    /// <param name="length">Length of FM signal</param>
    /// <param name="samplingRate">Sampling rate</param>
    public static Signal FrequencySinusoidal(
                                    float carrierFrequency,
                                    float carrierAmplitude,
                                    float modulatorFrequency,
                                    float modulationIndex,
                                    int length,
                                    float samplingRate = 1f)
    {
        var fs = samplingRate;
        var ca = carrierAmplitude;
        var cf = carrierFrequency;
        var mf = modulatorFrequency;
        var mi = modulationIndex;

        var output = new float[length];
        for (var i = 0; i < length; i++)
        {
            output[i] = ca * MathF.Cos(2 * MathF.PI * cf / fs * i +
                                       mi * MathF.Sin(2 * MathF.PI * mf / fs * i));
        }

        return Signal.FromCopy(output, fs);
    }

    /// <summary>
    /// Does linear frequency modulation (FM) and returns FM signal.
    /// </summary>
    /// <param name="carrierFrequency">Carrier signal frequency</param>
    /// <param name="carrierAmplitude">Carrier signal amplitude</param>
    /// <param name="modulationIndex">Modulation index (depth)</param>
    /// <param name="length">Length of FM signal</param>
    /// <param name="samplingRate">Sampling rate</param>
    public static Signal FrequencyLinear(
                                    float carrierFrequency,
                                    float carrierAmplitude,
                                    float modulationIndex,
                                    int length,
                                    float samplingRate = 1f)
    {
        var fs = samplingRate;
        var ca = carrierAmplitude;
        var cf = carrierFrequency;
        var mi = modulationIndex;

        var output = new float[length];
        for (var i = 0; i < length; i++)
        {
            output[i] = ca * MathF.Cos(2 * MathF.PI * (cf / fs + mi * i) * i / fs);
        }

        return Signal.FromCopy(output, fs);
    }

    /// <summary>
    /// Does phase modulation (PM) and returns PM signal.
    /// </summary>
    /// <param name="baseband">Baseband signal</param>
    /// <param name="carrierAmplitude">Carrier amplitude</param>
    /// <param name="carrierFrequency">Carrier frequency</param>
    /// <param name="deviation">Frequency deviation</param>
    public static Signal Phase(Signal baseband,
                               float carrierAmplitude,
                               float carrierFrequency,
                               float deviation = 0.8f)
    {
        var fs = baseband.SamplingRate;
        var ca = carrierAmplitude;
        var cf = carrierFrequency;

        var output = new float[baseband.Length];
        for (var i = 0; i < baseband.Length; i++)
        {
            output[i] = ca * MathF.Cos(2 * MathF.PI * cf / fs * i + deviation * baseband[i]);
        }

        return Signal.FromCopy(output, fs);
    }

    /// <summary>
    /// Does simple amplitude demodulation of <paramref name="signal"/> based on Hilbert transform.
    /// </summary>
    public static Signal DemodulateAmplitude(Signal signal)
    {
        var mag = new float[signal.Length];
        new HilbertTransform(signal.Length).AnalyticMagnitude(signal.Samples, mag);

        return Signal.FromCopy(mag, signal.SamplingRate) - 1.0f;
    }

    /// <summary>
    /// Does simple frequency demodulation pf <paramref name="signal"/> based on Hilbert transform.
    /// </summary>
    public static Signal DemodulateFrequency(Signal signal)
    {
        var diff = new float[signal.Length];
        VMath.Diff(signal.Samples, diff);

        var mag = new float[signal.Length];
        new HilbertTransform(signal.Length).AnalyticMagnitude(diff, mag);

        return Signal.FromCopy(mag, signal.SamplingRate) - 1.0f;
    }
}
