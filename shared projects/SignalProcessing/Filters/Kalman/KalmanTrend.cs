namespace Vorcyc.Mathematics.SignalProcessing.Filters.Kalman;

/// <summary>
/// Represents the result of <see cref="KalmanTrend.Filter"/> and <see cref="KalmanTrend.Smooth"/>. Every array has the length of the signal;
/// the optional ones are <see langword="null"/> unless they were asked for (and applicable).
/// </summary>
public sealed class KalmanTrendResult
{
    internal KalmanTrendResult(
        float[] level,
        float[]? slope,
        float[]? stdDev,
        float[]? standardizedInnovation,
        double measurementVariance,
        double processVariance,
        int missingCount,
        int rejectedCount,
        int breakCount,
        bool isSmoothed)
    {
        Level = level;
        Slope = slope;
        StdDev = stdDev;
        StandardizedInnovation = standardizedInnovation;
        MeasurementVariance = measurementVariance;
        ProcessVariance = processVariance;
        MissingCount = missingCount;
        RejectedCount = rejectedCount;
        BreakCount = breakCount;
        IsSmoothed = isSmoothed;
    }

    /// <summary>
    /// Gets the level estimate (filtered or smoothed). A sample that is not finite does not get a measurement, but it does get an estimate: the filter
    /// coasts over it and the smoother bridges it. Before the first finite sample the filtered estimate is NaN.
    /// </summary>
    public float[] Level { get; }

    /// <summary>
    /// Gets the slope estimate in units per sample; only for <see cref="KalmanTrendModel.LocalTrend"/> and <see cref="KalmanTrendOutputs.Slope"/>.
    /// </summary>
    public float[]? Slope { get; }

    /// <summary>
    /// Gets the standard deviation of the level estimate; only for <see cref="KalmanTrendOutputs.StdDev"/>. It grows over a gap of missing samples
    /// and is smaller for the smoothed than for the filtered estimate.
    /// </summary>
    public float[]? StdDev { get; }

    /// <summary>
    /// Gets the standardized innovations (NaN for a missing sample); only for <see cref="KalmanTrendOutputs.StandardizedInnovation"/>.
    /// </summary>
    public float[]? StandardizedInnovation { get; }

    /// <summary>
    /// Gets the variance of the measurement noise that was used: the one given, or the estimate from the signal.
    /// </summary>
    public double MeasurementVariance { get; }

    /// <summary>
    /// Gets the variance of the process noise that was used: the ratio times <see cref="MeasurementVariance"/>.
    /// </summary>
    public double ProcessVariance { get; }

    /// <summary>
    /// Gets the number of samples that were not finite.
    /// </summary>
    public int MissingCount { get; }

    /// <summary>
    /// Gets the number of samples that the gate rejected as outliers (a sample that was taken for a break is not counted).
    /// </summary>
    public int RejectedCount { get; }

    /// <summary>
    /// Gets the number of breaks: the places where the gate gave up rejecting and took the signal for changed.
    /// </summary>
    public int BreakCount { get; }

    /// <summary>
    /// Gets a value that tells whether <see cref="Level"/> is the smoothed (true) or the filtered (false) estimate.
    /// </summary>
    public bool IsSmoothed { get; }
}

/// <summary>
/// Kalman filter and Rauch-Tung-Striebel smoother for a slowly varying signal in white noise: the level of <see cref="KalmanTrendModel.LocalLevel"/>
/// or the trend of <see cref="KalmanTrendModel.LocalTrend"/>. Unlike a moving average or a Savitzky-Golay filter it copes with missing samples
/// (it bridges them and says how sure it is), it can reject outliers, and it gives the uncertainty of the estimate.
/// </summary>
/// <remarks>
/// <para>
/// The estimate depends on one number only, the ratio of the process noise to the measurement noise: a small ratio smooths strongly.
/// <see cref="EquivalentWindow"/> and <see cref="RatioForWindow"/> convert it to and from the number of samples that the smoother averages over,
/// <see cref="EstimateMeasurementVariance"/> estimates the measurement noise from the signal (it assumes that the signal is smooth compared with
/// the noise; it is used if no measurement variance is given).
/// </para>
/// <para>
/// <see cref="Filter"/> is causal and lags behind a moving signal; <see cref="Smooth"/> uses the whole signal and is zero-phase. The smoother stores a
/// checkpoint per 4096 samples and re-runs the filter block by block backwards, so besides the outputs it needs memory only in proportion to
/// <c>n / 4096</c>; it takes about three passes over the signal.
/// </para>
/// <para>
/// For an arbitrary linear model (matrices <c>A</c>, <c>H</c>, <c>Q</c>, <c>R</c>) or a nonlinear one use the classes
/// <see cref="StandardKalmanFilter2D{T}"/>, <see cref="ExtendedKalmanFilter1D{T}"/> and <see cref="UnscentedKalmanFilter1D{T}"/>.
/// </para>
/// </remarks>
public static class KalmanTrend
{
    /// <summary>
    /// Samples per block of the smoother.
    /// </summary>
    internal const int BlockSize = 4096;

    private const int MaxDifferences = 100_000;
    private const double MinRatio = 1e-20;
    private const double MaxRatio = 1e20;

    /// <summary>
    /// Runs the causal Kalman filter over <paramref name="samples"/>.
    /// </summary>
    /// <param name="samples">Signal; a sample that is NaN or infinity is a missing one</param>
    /// <param name="model">Model of the signal</param>
    /// <param name="processToMeasurementRatio">Process noise variance (of the level, or of the slope for <see cref="KalmanTrendModel.LocalTrend"/>) divided by the measurement noise variance, positive; useful values are 1e-6 … 1</param>
    /// <param name="measurementVariance">Variance of the measurement noise; NaN or 0 estimates it from the signal (<see cref="EstimateMeasurementVariance"/>)</param>
    /// <param name="gateSigma">Outlier gate in standard deviations of the innovation: 0 for no gate, otherwise at least 1 (3 … 5 are usual)</param>
    /// <param name="outputs">Outputs besides the level</param>
    public static KalmanTrendResult Filter(
        ReadOnlySpan<float> samples,
        KalmanTrendModel model,
        double processToMeasurementRatio,
        double measurementVariance = double.NaN,
        double gateSigma = 0,
        KalmanTrendOutputs outputs = KalmanTrendOutputs.None)
    {
        CheckArguments(model, processToMeasurementRatio, measurementVariance, gateSigma);

        var n = samples.Length;
        var variance = ResolveVariance(samples, measurementVariance);
        var core = new KalmanTrendCore(model, processToMeasurementRatio, variance, gateSigma);
        var (level, slope, stdDev, innovation) = Allocate(n, core.IsTrend, outputs);

        var first = FirstFinite(samples);
        if (first < 0)
        {
            return Unobserved(level, slope, stdDev, innovation, variance, processToMeasurementRatio, false);
        }

        // the filter does not know the samples to come, so it starts at the first finite one
        for (var t = 0; t < first; t++)
        {
            Write(float.NaN, float.NaN, float.NaN, float.NaN, level, slope, stdDev, innovation, t);
        }

        var state = default(KalmanTrendState);
        core.Init(ref state, samples[first]);
        int missing = first, rejected = 0, breaks = 0;
        for (var t = first; t < n; t++)
        {
            var x = samples[t];
            var finite = float.IsFinite(x);
            core.Step(ref state, x, finite, out var step);
            Count(step.Flags, ref missing, ref rejected, ref breaks);
            Write(
                (float)state.X.Level,
                (float)state.X.Slope,
                (float)Math.Sqrt(Math.Max(0, state.X.P00)),
                finite ? (float)(step.Innovation / Math.Sqrt(step.Variance)) : float.NaN,
                level, slope, stdDev, innovation, t);
        }

        return new KalmanTrendResult(level, slope, stdDev, innovation, variance, processToMeasurementRatio * variance, missing, rejected, breaks, false);
    }

    /// <summary>
    /// Runs the Kalman filter forwards and the Rauch-Tung-Striebel smoother backwards over <paramref name="samples"/>: the estimate of every sample
    /// uses all the samples (zero phase). The standardized innovations of the result are those of the forward pass.
    /// </summary>
    /// <param name="samples">Signal; a sample that is NaN or infinity is a missing one</param>
    /// <param name="model">Model of the signal</param>
    /// <param name="processToMeasurementRatio">Process noise variance (of the level, or of the slope for <see cref="KalmanTrendModel.LocalTrend"/>) divided by the measurement noise variance, positive; useful values are 1e-6 … 1</param>
    /// <param name="measurementVariance">Variance of the measurement noise; NaN or 0 estimates it from the signal (<see cref="EstimateMeasurementVariance"/>)</param>
    /// <param name="gateSigma">Outlier gate in standard deviations of the innovation: 0 for no gate, otherwise at least 1 (3 … 5 are usual); a rejected sample is bridged like a missing one</param>
    /// <param name="outputs">Outputs besides the level</param>
    public static KalmanTrendResult Smooth(
        ReadOnlySpan<float> samples,
        KalmanTrendModel model,
        double processToMeasurementRatio,
        double measurementVariance = double.NaN,
        double gateSigma = 0,
        KalmanTrendOutputs outputs = KalmanTrendOutputs.None)
    {
        CheckArguments(model, processToMeasurementRatio, measurementVariance, gateSigma);

        var n = samples.Length;
        var variance = ResolveVariance(samples, measurementVariance);
        var core = new KalmanTrendCore(model, processToMeasurementRatio, variance, gateSigma);
        var (level, slope, stdDev, innovation) = Allocate(n, core.IsTrend, outputs);

        var first = FirstFinite(samples);
        if (first < 0)
        {
            return Unobserved(level, slope, stdDev, innovation, variance, processToMeasurementRatio, true);
        }

        // forward pass: a checkpoint of the state every BlockSize samples, the innovations and the counts
        var blocks = (n + BlockSize - 1) / BlockSize;
        var checkpoints = new KalmanTrendState[blocks];
        var state = default(KalmanTrendState);
        core.Init(ref state, samples[first]);
        int missing = 0, rejected = 0, breaks = 0;
        for (var t = 0; t < n; t++)
        {
            if (t % BlockSize == 0)
            {
                checkpoints[t / BlockSize] = state;
            }

            var x = samples[t];
            var finite = float.IsFinite(x);
            core.Step(ref state, x, finite, out var step);
            Count(step.Flags, ref missing, ref rejected, ref breaks);
            if (innovation is not null)
            {
                innovation[t] = finite ? (float)(step.Innovation / Math.Sqrt(step.Variance)) : float.NaN;
            }
        }

        // backward pass: the filter is replayed block by block from its checkpoint (exactly the same arithmetic, so the same states) and the smoother
        // runs backwards through the block
        var wantStd = stdDev is not null;
        var filtered = new KalmanTrendMoments[BlockSize];
        var predicted = new KalmanTrendMoments[BlockSize];
        KalmanTrendMoments smoothedNext = default, predictedAfterBlock = default;
        for (var b = blocks - 1; b >= 0; b--)
        {
            var start = b * BlockSize;
            var end = Math.Min(n, start + BlockSize);

            var replay = checkpoints[b];
            for (var t = start; t < end; t++)
            {
                var x = samples[t];
                core.Step(ref replay, x, float.IsFinite(x), out var step);
                filtered[t - start] = replay.X;
                predicted[t - start] = step.Predicted;
            }

            for (var t = end - 1; t >= start; t--)
            {
                KalmanTrendMoments smoothed;
                if (t == n - 1)
                {
                    smoothed = filtered[t - start];
                }
                else
                {
                    var predictedNext = t + 1 < end ? predicted[t + 1 - start] : predictedAfterBlock;
                    core.Smooth(in filtered[t - start], in predictedNext, in smoothedNext, wantStd, out smoothed);
                }

                level[t] = (float)smoothed.Level;
                if (slope is not null)
                {
                    slope[t] = (float)smoothed.Slope;
                }

                if (stdDev is not null)
                {
                    stdDev[t] = (float)Math.Sqrt(Math.Max(0, smoothed.P00));
                }

                smoothedNext = smoothed;
            }

            predictedAfterBlock = predicted[0];
        }

        return new KalmanTrendResult(level, slope, stdDev, innovation, variance, processToMeasurementRatio * variance, missing, rejected, breaks, true);
    }

    /// <summary>
    /// Estimates the variance of the measurement noise from the signal: the robust spread (median absolute deviation) of the differences of
    /// neighbouring samples, which for a smooth signal in white noise have twice its variance. Outliers do not matter; a signal that is not smooth
    /// compared with the noise (a tone, say) makes the estimate too large.
    /// </summary>
    /// <param name="samples">Signal; samples that are not finite are skipped</param>
    /// <returns>The variance, or 0 if the signal is too short to tell (fewer than 3 usable differences)</returns>
    public static double EstimateMeasurementVariance(ReadOnlySpan<float> samples)
    {
        var n = samples.Length;
        if (n < 4)
        {
            return 0;
        }

        var stride = Math.Max(1, (n - 1) / MaxDifferences);
        var differences = new double[(n - 1 + stride - 1) / stride];
        var count = 0;
        for (var i = 0; i + 1 < n; i += stride)
        {
            float a = samples[i], b = samples[i + 1];
            if (float.IsFinite(a) && float.IsFinite(b))
            {
                differences[count++] = (double)b - a;
            }
        }

        if (count < 3)
        {
            return 0;
        }

        var d = differences.AsSpan(0, count);
        double mean = 0, m2 = 0;
        for (var i = 0; i < count; i++)
        {
            var delta = d[i] - mean;
            mean += delta / (i + 1);
            m2 += delta * (d[i] - mean);
        }

        d.Sort();
        var median = Median(d);
        for (var i = 0; i < count; i++)
        {
            d[i] = Math.Abs(d[i] - median);
        }

        d.Sort();
        var sigma = 1.4826 * Median(d);
        var variance = sigma * sigma / 2;

        // more than half of the differences are equal (a constant or a coarsely quantized signal): the plain spread is all there is
        return variance > 0 ? variance : m2 / (count - 1) / 2;
    }

    /// <summary>
    /// Gets the number of samples of the equivalent moving average of the smoother: the smoother reduces the variance of white measurement noise
    /// by the same factor as a moving average of that many samples. It is the width of the exact (infinitely long) steady-state smoother, so it describes
    /// the interior of a signal; near the ends the window is shorter.
    /// </summary>
    /// <remarks>
    /// About <c>4 / sqrt(ratio)</c> samples for <see cref="KalmanTrendModel.LocalLevel"/> and <c>3.77 / ratio^(1/4)</c> samples for
    /// <see cref="KalmanTrendModel.LocalTrend"/> if the ratio is small. The causal filter averages over about half as many.
    /// </remarks>
    /// <param name="model">Model</param>
    /// <param name="processToMeasurementRatio">Process noise variance over measurement noise variance, positive (below 1e-20 it is taken as 1e-20)</param>
    public static double EquivalentWindow(KalmanTrendModel model, double processToMeasurementRatio)
    {
        CheckModel(model);
        CheckRatio(processToMeasurementRatio);

        // the smoothing kernel has the frequency response H = 1 / (1 + a |1 - e^(-iw)|^2) (level) or 1 / (1 + a |1 - e^(-iw)|^4) (trend), a = 1 / ratio;
        // the window is 1 / sum(h^2) = 2 pi / integral(H^2)
        var a = 1 / Math.Clamp(processToMeasurementRatio, MinRatio, MaxRatio);
        if (model == KalmanTrendModel.LocalLevel)
        {
            return Math.Pow(1 + 4 * a, 1.5) / (1 + 2 * a);
        }

        // H^2 is smooth and periodic: the trapezoidal rule converges geometrically, with a step that is a fraction of the width a^(-1/4) of the peak
        var m = (int)Math.Clamp(2048 + 24 * Math.Pow(a, 0.25), 2048, 8_000_000);
        var h = Math.PI / m;
        double sum = 0;
        for (var k = 0; k <= m; k++)
        {
            var s = Math.Sin(0.5 * k * h);
            var s2 = s * s;
            var f = 1 / (1 + 16 * a * s2 * s2);
            sum += k == 0 || k == m ? 0.5 * f * f : f * f;
        }

        return Math.PI / (sum * h);
    }

    /// <summary>
    /// Gets the ratio of the process noise to the measurement noise for which the smoother averages over <paramref name="window"/> samples
    /// (the inverse of <see cref="EquivalentWindow"/>).
    /// </summary>
    /// <param name="model">Model</param>
    /// <param name="window">Number of samples of the equivalent moving average, a finite number greater than 1</param>
    /// <returns>The ratio, limited to 1e-20 … 1e20</returns>
    public static double RatioForWindow(KalmanTrendModel model, double window)
    {
        CheckModel(model);
        if (!(double.IsFinite(window) && window > 1))
            throw new ArgumentOutOfRangeException(nameof(window), window, "The window must be a finite number greater than 1.");

        // the window shrinks as the ratio grows; the asymptotics for a long window give the neighbourhood, then the log of the ratio is bisected
        var guess = model == KalmanTrendModel.LocalLevel ? Math.Pow(4 / window, 2) : Math.Pow(3.7712 / window, 4);
        double low = Math.Clamp(guess / 100, MinRatio, MaxRatio), high = Math.Clamp(guess * 100, MinRatio, MaxRatio);
        while (low > MinRatio && EquivalentWindow(model, low) < window)
        {
            low = Math.Max(MinRatio, low / 10);
        }

        while (high < MaxRatio && EquivalentWindow(model, high) > window)
        {
            high = Math.Min(MaxRatio, high * 10);
        }

        if (EquivalentWindow(model, low) < window)
        {
            return low;
        }

        if (EquivalentWindow(model, high) > window)
        {
            return high;
        }

        double logLow = Math.Log(low), logHigh = Math.Log(high);
        for (var i = 0; i < 100 && logHigh - logLow > 1e-12; i++)
        {
            var mid = 0.5 * (logLow + logHigh);
            if (EquivalentWindow(model, Math.Exp(mid)) > window)
            {
                logLow = mid;
            }
            else
            {
                logHigh = mid;
            }
        }

        return Math.Exp(0.5 * (logLow + logHigh));
    }

    internal static void CheckModel(KalmanTrendModel model)
    {
        if (model is not (KalmanTrendModel.LocalLevel or KalmanTrendModel.LocalTrend))
            throw new ArgumentOutOfRangeException(nameof(model), model, "Unknown model.");
    }

    internal static void CheckRatio(double processToMeasurementRatio)
    {
        if (!(double.IsFinite(processToMeasurementRatio) && processToMeasurementRatio > 0))
            throw new ArgumentOutOfRangeException(nameof(processToMeasurementRatio), processToMeasurementRatio, "The ratio must be a positive finite number.");
    }

    internal static void CheckGate(double gateSigma)
    {
        if (!(gateSigma == 0 || (double.IsFinite(gateSigma) && gateSigma >= 1)))
            throw new ArgumentOutOfRangeException(nameof(gateSigma), gateSigma, "The gate must be 0 (no gate) or at least 1 standard deviation.");
    }

    private static void CheckArguments(KalmanTrendModel model, double ratio, double measurementVariance, double gateSigma)
    {
        CheckModel(model);
        CheckRatio(ratio);
        if (!(double.IsNaN(measurementVariance) || (double.IsFinite(measurementVariance) && measurementVariance >= 0)))
            throw new ArgumentOutOfRangeException(nameof(measurementVariance), measurementVariance, "The measurement variance must be NaN or 0 (estimate it) or a positive finite number.");
        CheckGate(gateSigma);
    }

    private static double ResolveVariance(ReadOnlySpan<float> samples, double measurementVariance)
    {
        if (measurementVariance > 0)
        {
            return measurementVariance;
        }

        var estimate = EstimateMeasurementVariance(samples);
        if (estimate > 0 && double.IsFinite(estimate))
        {
            return estimate;
        }

        // nothing to estimate it from (a constant signal, or too few samples): a tiny fraction of the power, or 1 for a signal of zeros
        double sum = 0;
        var count = 0;
        foreach (var value in samples)
        {
            if (float.IsFinite(value))
            {
                sum += (double)value * value;
                count++;
            }
        }

        var power = count > 0 ? sum / count : 0;
        return power > 0 && double.IsFinite(power) ? 1e-12 * power : 1.0;
    }

    private static (float[] Level, float[]? Slope, float[]? StdDev, float[]? Innovation) Allocate(int n, bool trend, KalmanTrendOutputs outputs)
        => (
            new float[n],
            trend && (outputs & KalmanTrendOutputs.Slope) != 0 ? new float[n] : null,
            (outputs & KalmanTrendOutputs.StdDev) != 0 ? new float[n] : null,
            (outputs & KalmanTrendOutputs.StandardizedInnovation) != 0 ? new float[n] : null);

    private static KalmanTrendResult Unobserved(
        float[] level, float[]? slope, float[]? stdDev, float[]? innovation,
        double measurementVariance, double ratio, bool smoothed)
    {
        for (var t = 0; t < level.Length; t++)
        {
            Write(float.NaN, float.NaN, float.NaN, float.NaN, level, slope, stdDev, innovation, t);
        }

        return new KalmanTrendResult(level, slope, stdDev, innovation, measurementVariance, ratio * measurementVariance, level.Length, 0, 0, smoothed);
    }

    private static void Write(
        float levelValue, float slopeValue, float stdDevValue, float innovationValue,
        float[] level, float[]? slope, float[]? stdDev, float[]? innovation, int t)
    {
        level[t] = levelValue;
        if (slope is not null)
        {
            slope[t] = slopeValue;
        }

        if (stdDev is not null)
        {
            stdDev[t] = stdDevValue;
        }

        if (innovation is not null)
        {
            innovation[t] = innovationValue;
        }
    }

    private static void Count(KalmanTrendStepFlags flags, ref int missing, ref int rejected, ref int breaks)
    {
        if ((flags & KalmanTrendStepFlags.Missing) != 0)
        {
            missing++;
        }

        if ((flags & KalmanTrendStepFlags.Rejected) != 0)
        {
            rejected++;
        }

        if ((flags & KalmanTrendStepFlags.Break) != 0)
        {
            breaks++;
        }
    }

    private static int FirstFinite(ReadOnlySpan<float> samples)
    {
        for (var i = 0; i < samples.Length; i++)
        {
            if (float.IsFinite(samples[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private static double Median(ReadOnlySpan<double> sorted)
    {
        var n = sorted.Length;
        return (n & 1) == 1 ? sorted[n / 2] : 0.5 * (sorted[n / 2 - 1] + sorted[n / 2]);
    }
}
