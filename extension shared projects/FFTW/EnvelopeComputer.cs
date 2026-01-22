using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.Extensions.FFTW;


/// <summary>
/// Provides methods for computing signal envelopes using Hilbert transform and peak detection.
/// </summary>
public static class EnvelopeComputer
{
    /// <summary>
    /// Defines the method used to compute the lower envelope.
    /// </summary>
    public enum LowerEnvelopeMode
    {
        /// <summary>
        /// Lower envelope is the negative of upper envelope (assumes signal oscillates symmetrically around zero).
        /// </summary>
        Symmetric,

        /// <summary>
        /// Lower envelope is computed independently by negating the signal first, then applying Hilbert transform.
        /// </summary>
        Independent
    }

    //public static void TestEnvelopeWithManualAM()
    //{
    //    var fs = 100f; // 采样率 100Hz
    //    // 1. 创建信号：2秒，采样率100Hz，共200个样本
    //    Signal signal = new(TimeSpan.FromSeconds(2), fs);

    //    // 2. 生成多个波形叠加
    //    signal.GenerateWave(WaveShape.Sine, 20, Behaviour.ElementWiseAdd);
    //    signal.GenerateWave(WaveShape.Square, 10, Behaviour.ElementWiseAdd);
    //    signal.GenerateWave(WaveShape.Sawtooth, 15, Behaviour.ElementWiseAdd);
    //    signal.GenerateWave(WaveShape.Triangle, 30, Behaviour.ElementWiseAdd);

    //    // 3. 手动调幅：用 2Hz 正弦波调制（包络从 0 到 1 变化）
    //    //    调制公式：signal(t) = carrier(t) * (0.5 + 0.5 * sin(2π * 2 * t))
    //    //for (int i = 0; i < signal.Samples.Length; i++)
    //    //{
    //    //    float t = i / fs;  // 时间点（秒）
    //    //    float modulator = 0.5f + 0.5f * MathF.Sin(2 * MathF.PI * 2 * t);
    //    //    signal.Samples[i] *= modulator;
    //    //}

    //    // 4. 计算包络（使用独立计算模式获得更准确的下包络）
    //    //var (upper, lower) = ComputeHilbertEnvelope(signal.Samples, upperOnly: false, lowerMode: LowerEnvelopeMode.Independent);
    //    var (upper, lower) = ComputeHilbertEnvelope(signal.Samples);

    //    // 5. 打印对比
    //    var sb = new StringBuilder();
    //    Console.WriteLine("时间(s)\t原始信号\t\t上包络\t\t下包络");
    //    sb.AppendLine("时间(s)\t原始信号\t\t上包络\t\t下包络");

    //    for (int i = 0; i < signal.Samples.Length; i++)
    //    {
    //        float t = i / fs;
    //        var text = $"{t:F3}\t{signal.Samples[i],+10:F6}\t{upper[i],10:F6}\t{lower![i],10:F6}";
    //        Console.WriteLine(text);
    //        sb.AppendLine(text);
    //    }

    //    System.IO.File.WriteAllText("EnvelopeTestResults.csv", sb.ToString());
    //    Console.WriteLine("\nResults saved to EnvelopeTestResults.csv");
    //}

    /// <summary>
    /// Returns a new array whose elements are the negated values of the input array.
    /// </summary>
    /// <param name="values">The input values.</param>
    /// <returns>A new array containing the negated values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] Negate(ReadOnlySpan<float> values)
    {
        if (values.IsEmpty)
            return [];

        float[] result = new float[values.Length];
        Span<float> destination = result;

        int i = 0;

        if (Vector.IsHardwareAccelerated && values.Length >= Vector<float>.Count)
        {
            int width = Vector<float>.Count;
            int simdEnd = values.Length - (values.Length % width);
            Vector<float> zero = Vector<float>.Zero;

            for (; i < simdEnd; i += width)
            {
                Vector<float> vector = new(values.Slice(i, width));
                Vector.Subtract(zero, vector).CopyTo(destination.Slice(i, width));
            }
        }

        for (; i < values.Length; i++)
            destination[i] = -values[i];

        return result;
    }

    /// <summary>
    /// Returns a new array whose elements are the negated values of the input array.
    /// </summary>
    /// <param name="values">The input values.</param>
    /// <returns>A new array containing the negated values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double[] Negate(ReadOnlySpan<double> values)
    {
        if (values.IsEmpty)
            return [];

        double[] result = new double[values.Length];
        Span<double> destination = result;

        int i = 0;

        if (Vector.IsHardwareAccelerated && values.Length >= Vector<double>.Count)
        {
            int width = Vector<double>.Count;
            int simdEnd = values.Length - (values.Length % width);
            Vector<double> zero = Vector<double>.Zero;

            for (; i < simdEnd; i += width)
            {
                Vector<double> vector = new(values.Slice(i, width));
                Vector.Subtract(zero, vector).CopyTo(destination.Slice(i, width));
            }
        }

        for (; i < values.Length; i++)
            destination[i] = -values[i];

        return result;
    }

    #region Hilbert Envelope Single

    /// <summary>
    /// Computes the Hilbert envelope of a real signal using the Hilbert transform.
    /// </summary>
    /// <param name="realSignal">The input real signal.</param>
    /// <param name="upperOnly">If true, only computes the upper envelope.</param>
    /// <param name="lowerMode">The method used to compute the lower envelope.</param>
    /// <returns>A tuple containing the upper envelope and optionally the lower envelope.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float[] Upper, float[]? Lower) ComputeHilbertEnvelope(
        ReadOnlySpan<float> realSignal,
        bool upperOnly = false,
        LowerEnvelopeMode lowerMode = LowerEnvelopeMode.Symmetric)
    {
        int n = realSignal.Length;
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(n, 0);

        float[] hilbert = HilbertTransform.ComputeHilbertTransform(realSignal);
        float[] upper = ComputeEnvelopeFromHilbert(realSignal, hilbert);

        if (upperOnly)
            return (upper, null);

        float[] lower;
        if (lowerMode == LowerEnvelopeMode.Symmetric)
        {
            // Simple negation: assumes symmetric oscillation around zero
            lower = Negate(upper);
        }
        else // Independent
        {
            // Independent calculation: negate signal, compute Hilbert envelope, then negate result
            float[] negatedSignal = Negate(realSignal);
            float[] negatedHilbert = HilbertTransform.ComputeHilbertTransform(negatedSignal);
            float[] negatedEnvelope = ComputeEnvelopeFromHilbert(negatedSignal, negatedHilbert);

            lower = Negate(negatedEnvelope.AsSpan());
        }

        return (upper, lower);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float[] ComputeEnvelopeFromHilbert(ReadOnlySpan<float> realSignal, ReadOnlySpan<float> hilbertSignal)
    {
        int n = realSignal.Length;
        ArgumentOutOfRangeException.ThrowIfNotEqual(realSignal.Length, hilbertSignal.Length);

        float[] envelope = new float[n];
        for (int i = 0; i < n; i++)
            envelope[i] = MathF.Sqrt(realSignal[i] * realSignal[i] + hilbertSignal[i] * hilbertSignal[i]);

        return envelope;
    }

    #endregion

    #region Hilbert Envelope Double

    /// <summary>
    /// Computes the Hilbert envelope of a real signal using the Hilbert transform.
    /// </summary>
    /// <param name="realSignal">The input real signal.</param>
    /// <param name="upperOnly">If true, only computes the upper envelope.</param>
    /// <param name="lowerMode">The method used to compute the lower envelope.</param>
    /// <returns>A tuple containing the upper envelope and optionally the lower envelope.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (double[] Upper, double[]? Lower) ComputeHilbertEnvelope(
        ReadOnlySpan<double> realSignal,
        bool upperOnly = false,
        LowerEnvelopeMode lowerMode = LowerEnvelopeMode.Symmetric)
    {
        int n = realSignal.Length;
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(n, 0);

        double[] hilbert = HilbertTransform.ComputeHilbertTransform(realSignal);
        double[] upper = ComputeEnvelopeFromHilbert(realSignal, hilbert);

        if (upperOnly)
            return (upper, null);

        double[] lower;
        if (lowerMode == LowerEnvelopeMode.Symmetric)
        {
            // Simple negation: assumes symmetric oscillation around zero
            lower = Negate(upper);
        }
        else // Independent
        {
            // Independent calculation: negate signal, compute Hilbert envelope, then negate result
            double[] negatedSignal = Negate(realSignal);
            double[] negatedHilbert = HilbertTransform.ComputeHilbertTransform(negatedSignal);
            double[] negatedEnvelope = ComputeEnvelopeFromHilbert(negatedSignal, negatedHilbert);

            lower = Negate(negatedEnvelope.AsSpan());
        }

        return (upper, lower);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double[] ComputeEnvelopeFromHilbert(ReadOnlySpan<double> realSignal, ReadOnlySpan<double> hilbertSignal)
    {
        int n = realSignal.Length;
        ArgumentOutOfRangeException.ThrowIfNotEqual(realSignal.Length, hilbertSignal.Length);

        double[] envelope = new double[n];
        for (int i = 0; i < n; i++)
            envelope[i] = Math.Sqrt(realSignal[i] * realSignal[i] + hilbertSignal[i] * hilbertSignal[i]);

        return envelope;
    }

    #endregion

    #region Peak Envelope

    /// <summary>
    /// Computes upper and lower envelopes by connecting local maxima and minima using cubic interpolation.
    /// </summary>
    /// <param name="signal">The input signal.</param>
    /// <returns>A tuple containing the upper and lower envelopes.</returns>
    public static (float[] Upper, float[] Lower) ComputePeakEnvelope(ReadOnlySpan<float> signal)
    {
        int n = signal.Length;
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(n, 0);

        // 1. Find local maxima and minima
        List<(int Index, float Value)> maxima = [(0, signal[0])];
        List<(int Index, float Value)> minima = [(0, signal[0])];

        for (int i = 1; i < n - 1; i++)
        {
            if (signal[i] >= signal[i - 1] && signal[i] >= signal[i + 1])
                maxima.Add((i, signal[i]));
            if (signal[i] <= signal[i - 1] && signal[i] <= signal[i + 1])
                minima.Add((i, signal[i]));
        }

        // Add endpoints to ensure full coverage
        maxima.Add((n - 1, signal[n - 1]));
        minima.Add((n - 1, signal[n - 1]));

        // 2. Interpolate to form smooth envelopes
        float[] upper = InterpolateCubic(maxima, n);
        float[] lower = InterpolateCubic(minima, n);

        return (upper, lower);
    }

    /// <summary>
    /// Performs monotone cubic interpolation over the given control points
    /// to produce an array of the specified length.
    /// </summary>
    private static float[] InterpolateCubic(List<(int Index, float Value)> points, int length)
    {
        float[] result = new float[length];

        if (points.Count < 2)
        {
            // Not enough points for interpolation; fill with the single value
            float val = points.Count == 1 ? points[0].Value : 0f;
            Array.Fill(result, val);
            return result;
        }

        // Use piecewise cubic Hermite interpolation (simple approach)
        int seg = 0;
        for (int i = 0; i < length; i++)
        {
            // Advance segment
            while (seg < points.Count - 2 && i > points[seg + 1].Index)
                seg++;

            int x0 = points[seg].Index;
            int x1 = points[seg + 1].Index;
            float y0 = points[seg].Value;
            float y1 = points[seg + 1].Value;

            if (x1 == x0)
            {
                result[i] = y0;
            }
            else
            {
                // Linear interpolation (simple and robust; upgrade to cubic spline if needed)
                float t = (float)(i - x0) / (x1 - x0);
                // Smoothstep for smoother curve (Hermite basis)
                t = t * t * (3f - 2f * t);
                result[i] = y0 + (y1 - y0) * t;
            }
        }

        return result;
    }

    #endregion
}