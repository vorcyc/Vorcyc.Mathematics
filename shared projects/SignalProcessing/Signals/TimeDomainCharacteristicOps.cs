using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Statistics;

namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// Time-domain characteristic calculations with optional <see cref="ComputingContext"/>.
/// </summary>
public static class TimeDomainCharacteristicOps
{
    /// <summary>Amplitude (max − min) with optional execution policy.</summary>
    public static float GetAmplitude(ReadOnlySpan<float> samples, ComputingContext? context = null)
    {
        if (ComputingContextExecution.UseParallel(context, samples.Length))
        {
            var span = Materialize(samples);
            return span.CompareMax(context) - span.CompareMin(context);
        }

        var writable = Materialize(samples);
        return ComputingContext.Resolve(context).ResolveCpuMode(samples.Length) == CpuExecutionMode.Normal
            ? ITimeDomainCharacteristics.GetAmplitude_Normal(writable)
            : ITimeDomainCharacteristics.GetAmplitude_SIMD(writable);
    }

    /// <summary>Total power (sum of squares) with optional execution policy.</summary>
    public static float GetTotalPower(ReadOnlySpan<float> samples, ComputingContext? context = null)
    {
        if (ComputingContextExecution.UseParallel(context, samples.Length))
        {
            return SumOfSquaresParallel(samples, context);
        }

        return ComputingContext.Resolve(context).ResolveCpuMode(samples.Length) == CpuExecutionMode.Normal
            ? ITimeDomainCharacteristics.GetTotalPower_Normal(samples)
            : ITimeDomainCharacteristics.GetTotalPower_SIMD(samples);
    }

    /// <summary>Average power with optional execution policy.</summary>
    public static float GetAveragePower(ReadOnlySpan<float> samples, ComputingContext? context = null)
        => GetTotalPower(samples, context) / samples.Length;

    /// <summary>Total energy with optional execution policy.</summary>
    public static float GetTotalEnergy(ReadOnlySpan<float> samples, ComputingContext? context = null)
        => GetTotalPower(samples, context);

    /// <summary>Average energy with optional execution policy.</summary>
    public static float GetAverageEnergy(ReadOnlySpan<float> samples, ComputingContext? context = null)
        => GetAveragePower(samples, context);

    /// <summary>RMS with optional execution policy.</summary>
    public static float GetRms(ReadOnlySpan<float> samples, ComputingContext? context = null)
        => MathF.Sqrt(GetAverageEnergy(samples, context));

    private static float SumOfSquaresParallel(ReadOnlySpan<float> samples, ComputingContext? context)
    {
        var data = samples.ToArray();
        int workers = ComputingContextExecution.ParallelWorkerCount(context);
        var partials = new float[workers];
        int length = data.Length;
        int chunk = (length + workers - 1) / workers;

        Parallel.For(0, workers, worker =>
        {
            int start = worker * chunk;
            if (start >= length)
            {
                return;
            }

            int end = Math.Min(start + chunk, length);
            float local = 0f;
            for (int i = start; i < end; i++)
            {
                local += data[i] * data[i];
            }

            partials[worker] = local;
        });

        float sum = 0f;
        for (var i = 0; i < partials.Length; i++)
        {
            sum += partials[i];
        }

        return sum;
    }

    private static Span<float> Materialize(ReadOnlySpan<float> samples)
        => samples.ToArray().AsSpan();
}

/// <summary>
/// <see cref="ComputingContext"/> helpers for single-threaded time-domain signals.
/// </summary>
public static class SingleThreadTimeDomainSignalComputingContextExtensions
{
    /// <summary>Gets RMS using an optional execution policy (bypasses cached property).</summary>
    public static float GetRms(this ISingleThreadTimeDomainSignal signal, ComputingContext? context = null)
        => TimeDomainCharacteristicOps.GetRms(signal.Samples, context);

    /// <summary>Gets amplitude using an optional execution policy.</summary>
    public static float GetAmplitude(this ISingleThreadTimeDomainSignal signal, ComputingContext? context = null)
        => TimeDomainCharacteristicOps.GetAmplitude(signal.Samples, context);

    /// <summary>Gets average power using an optional execution policy.</summary>
    public static float GetAveragePower(this ISingleThreadTimeDomainSignal signal, ComputingContext? context = null)
        => TimeDomainCharacteristicOps.GetAveragePower(signal.Samples, context);

    /// <summary>Gets total power using an optional execution policy.</summary>
    public static float GetTotalPower(this ISingleThreadTimeDomainSignal signal, ComputingContext? context = null)
        => TimeDomainCharacteristicOps.GetTotalPower(signal.Samples, context);
}
