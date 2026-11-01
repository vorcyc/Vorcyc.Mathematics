namespace Vorcyc.Mathematics.SignalProcessing.Filters.Kalman;

using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

/// <summary>
/// Represents the causal Kalman filter of a slowly varying signal in white noise (<see cref="KalmanTrendModel"/>), sample by sample.
/// The output is the estimate of the level that uses the samples up to the current one; for the zero-phase estimate that also uses the samples
/// to come, see <see cref="KalmanTrend.Smooth"/>.
/// </summary>
/// <remarks>
/// <para>
/// Only the ratio of the process noise to the measurement noise decides the estimate: a small ratio gives a smooth, slow estimate. For a ratio that
/// is not known, <see cref="KalmanTrend.RatioForWindow"/> turns the number of samples that the estimate should average over into a ratio.
/// The measurement variance only scales the uncertainty of the estimate (<see cref="LevelStdDev"/>) and the gate.
/// </para>
/// <para>
/// A sample that is not finite is a missing one: the estimate coasts on the model (it stays where it is for <see cref="KalmanTrendModel.LocalLevel"/>
/// and continues along the slope for <see cref="KalmanTrendModel.LocalTrend"/>) and the uncertainty grows. The filter starts with the first finite
/// sample; before it <see cref="Process(float)"/> returns NaN. With a <c>gateSigma</c> above zero a sample that is far off the prediction is treated as
/// missing (an outlier); a signal that really has moved is followed again after a few samples.
/// </para>
/// </remarks>
public sealed class KalmanTrendFilter : IOnlineFilter
{
    private readonly KalmanTrendCore _core;
    private KalmanTrendState _state;
    private bool _started;

    /// <summary>
    /// Constructs <see cref="KalmanTrendFilter"/>.
    /// </summary>
    /// <param name="model">Model of the signal</param>
    /// <param name="processToMeasurementRatio">Variance of the process noise (of the level, or of the slope for <see cref="KalmanTrendModel.LocalTrend"/>) divided by the variance of the measurement noise, positive; useful values are 1e-6 … 1 (small: smooth and slow)</param>
    /// <param name="measurementVariance">Variance of the measurement noise in the units of the signal squared, positive</param>
    /// <param name="gateSigma">Outlier gate in standard deviations of the innovation: 0 for no gate, otherwise at least 1 (3 … 5 are usual)</param>
    public KalmanTrendFilter(
        KalmanTrendModel model = KalmanTrendModel.LocalLevel,
        double processToMeasurementRatio = 1e-3,
        double measurementVariance = 1.0,
        double gateSigma = 0.0)
    {
        KalmanTrend.CheckModel(model);
        KalmanTrend.CheckRatio(processToMeasurementRatio);
        if (!(double.IsFinite(measurementVariance) && measurementVariance > 0))
            throw new ArgumentOutOfRangeException(nameof(measurementVariance), measurementVariance, "The measurement variance must be a positive finite number.");
        KalmanTrend.CheckGate(gateSigma);

        Model = model;
        ProcessToMeasurementRatio = processToMeasurementRatio;
        MeasurementVariance = measurementVariance;
        GateSigma = gateSigma;
        _core = new KalmanTrendCore(model, processToMeasurementRatio, measurementVariance, gateSigma);
        Reset();
    }

    /// <summary>
    /// Gets the model.
    /// </summary>
    public KalmanTrendModel Model { get; }

    /// <summary>
    /// Gets the ratio of the process noise variance to the measurement noise variance.
    /// </summary>
    public double ProcessToMeasurementRatio { get; }

    /// <summary>
    /// Gets the variance of the measurement noise.
    /// </summary>
    public double MeasurementVariance { get; }

    /// <summary>
    /// Gets the outlier gate in standard deviations; 0 means no gate.
    /// </summary>
    public double GateSigma { get; }

    /// <summary>
    /// Gets the level estimate after the last sample; NaN before the first finite sample.
    /// </summary>
    public float Level { get; private set; }

    /// <summary>
    /// Gets the slope estimate in units per sample after the last sample (always 0 for <see cref="KalmanTrendModel.LocalLevel"/>); NaN before the first finite sample.
    /// </summary>
    public float Slope { get; private set; }

    /// <summary>
    /// Gets the standard deviation of the level estimate after the last sample; NaN before the first finite sample.
    /// </summary>
    public float LevelStdDev { get; private set; }

    /// <summary>
    /// Gets the innovation of the last sample (the sample minus the prediction) divided by its standard deviation; NaN if the sample was missing.
    /// </summary>
    public float StandardizedInnovation { get; private set; }

    /// <summary>
    /// Gets a value that tells whether the gate has rejected the last sample.
    /// </summary>
    public bool LastSampleRejected { get; private set; }

    /// <summary>
    /// Processes one sample and returns the level estimate.
    /// </summary>
    /// <param name="sample">Input sample; NaN or infinity is a missing sample</param>
    public float Process(float sample)
    {
        var finite = float.IsFinite(sample);
        if (!_started)
        {
            if (!finite)
            {
                return float.NaN;
            }

            _core.Init(ref _state, sample);
            _started = true;
        }

        _core.Step(ref _state, sample, finite, out var step);

        Level = (float)_state.X.Level;
        Slope = (float)_state.X.Slope;
        LevelStdDev = (float)Math.Sqrt(Math.Max(0, _state.X.P00));
        StandardizedInnovation = finite ? (float)(step.Innovation / Math.Sqrt(step.Variance)) : float.NaN;
        LastSampleRejected = (step.Flags & KalmanTrendStepFlags.Rejected) != 0;
        return Level;
    }

    /// <summary>
    /// Restarts the filter: the next finite sample starts it again.
    /// </summary>
    public void Reset()
    {
        _started = false;
        _state = default;
        Level = float.NaN;
        Slope = float.NaN;
        LevelStdDev = float.NaN;
        StandardizedInnovation = float.NaN;
        LastSampleRejected = false;
    }
}
