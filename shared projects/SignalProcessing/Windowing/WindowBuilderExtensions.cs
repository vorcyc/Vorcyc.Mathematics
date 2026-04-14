using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace Vorcyc.Mathematics.SignalProcessing.Windowing;

/// <summary>
/// Provides extension methods for applying windows to signals and arrays of samples.
/// </summary>
public static class WindowBuilderExtensions
{
    /// <summary>
    /// Applies window to array of <paramref name="samples"/>.
    /// </summary>
    /// <param name="samples">Samples</param>
    /// <param name="windowSamples">Window coefficients</param>
    public static void ApplyWindow(this float[] samples, float[] windowSamples)
    {
        for (var k = 0; k < windowSamples.Length; k++)
        {
            samples[k] *= windowSamples[k];
        }
    }

    /// <summary>
    /// Applies window to array of <paramref name="samples"/>.
    /// </summary>
    /// <param name="samples">Samples</param>
    /// <param name="windowSamples">Window coefficients</param>
    public static void ApplyWindow(this double[] samples, double[] windowSamples)
    {
        for (var k = 0; k < windowSamples.Length; k++)
        {
            samples[k] *= windowSamples[k];
        }
    }

    /// <summary>
    /// Applies window to <paramref name="signal"/>.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="windowSamples">Window coefficients</param>
    public static void ApplyWindow(this Signal signal, float[] windowSamples)
    {
        signal.Samples.ApplyWindow(windowSamples);
        signal.NotifySamplesModified();
    }

    /// <summary>
    /// Applies window with optional <paramref name="parameters"/> to array of <paramref name="samples"/>.
    /// </summary>
    /// <param name="samples">Samples</param>
    /// <param name="window">Window type</param>
    /// <param name="parameters">Window parameters</param>
    public static void ApplyWindow(this float[] samples, WindowType window, params object[] parameters)
    {
        var windowSamples = WindowBuilder.OfType(window, samples.Length, parameters);
        samples.ApplyWindow(windowSamples);
    }

    /// <summary>
    /// Applies window with optional <paramref name="parameters"/> to array of <paramref name="samples"/>.
    /// </summary>
    /// <param name="samples">Samples</param>
    /// <param name="window">Window type</param>
    /// <param name="parameters">Window parameters</param>
    public static void ApplyWindow(this double[] samples, WindowType window, params object[] parameters)
    {
        var windowSamples = WindowBuilder.OfType(window, samples.Length, parameters).ToDoubles();
        samples.ApplyWindow(windowSamples);
    }

    /// <summary>
    /// Applies window with optional <paramref name="parameters"/> to <paramref name="signal"/>.
    /// </summary>
    /// <param name="signal">Signal</param>
    /// <param name="window">Window type</param>
    /// <param name="parameters">Window parameters</param>
    public static void ApplyWindow(this Signal signal, WindowType window, params object[] parameters)
    {
        var windowSamples = WindowBuilder.OfType(window, signal.Length, parameters);
        signal.ApplyWindow(windowSamples);
    }

    /// <summary>
    /// Applies window in-place to a span of samples.
    /// </summary>
    public static void ApplyWindow(this Span<float> samples, float[] windowSamples)
    {
        for (var k = 0; k < windowSamples.Length; k++)
        {
            samples[k] *= windowSamples[k];
        }
    }

    /// <summary>
    /// Applies window to legacy <see cref="DiscreteSignal"/>.
    /// </summary>
    [Obsolete("Use Signal instead.")]
    public static void ApplyWindow(this DiscreteSignal signal, float[] windowSamples)
    {
        signal.Samples.Values.ApplyWindow(windowSamples);
    }

    /// <summary>
    /// Applies window with optional <paramref name="parameters"/> to legacy <see cref="DiscreteSignal"/>.
    /// </summary>
    [Obsolete("Use Signal instead.")]
    public static void ApplyWindow(this DiscreteSignal signal, WindowType window, params object[] parameters)
    {
        var windowSamples = WindowBuilder.OfType(window, signal.SampleCount, parameters);
        signal.Samples.Values.ApplyWindow(windowSamples);
    }
}
