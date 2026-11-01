namespace Vorcyc.Mathematics.SignalProcessing.Filters.Kalman;

/// <summary>
/// Mean and covariance of the state (level, slope). The slope parts are not used by <see cref="KalmanTrendModel.LocalLevel"/>.
/// </summary>
internal struct KalmanTrendMoments
{
    public double Level;
    public double Slope;

    /// <summary>
    /// Variance of the level.
    /// </summary>
    public double P00;

    /// <summary>
    /// Covariance of the level and the slope.
    /// </summary>
    public double P01;

    /// <summary>
    /// Variance of the slope.
    /// </summary>
    public double P11;
}

/// <summary>
/// State of the forward recursion after a sample; copying it gives a checkpoint that the recursion can be replayed from.
/// </summary>
internal struct KalmanTrendState
{
    public KalmanTrendMoments X;

    /// <summary>
    /// Number of samples in a row that the gate has rejected.
    /// </summary>
    public int Rejected;
}

[Flags]
internal enum KalmanTrendStepFlags : byte
{
    None = 0,
    Missing = 1,
    Rejected = 2,
    Break = 4,
}

/// <summary>
/// What one step of the forward recursion saw.
/// </summary>
internal struct KalmanTrendStep
{
    /// <summary>
    /// The state predicted for this sample (after the inflation of a break), before the measurement is used.
    /// </summary>
    public KalmanTrendMoments Predicted;

    /// <summary>
    /// Measurement minus prediction; NaN for a missing sample.
    /// </summary>
    public double Innovation;

    /// <summary>
    /// Variance of the innovation; NaN for a missing sample.
    /// </summary>
    public double Variance;

    public KalmanTrendStepFlags Flags;
}

/// <summary>
/// The recursion of the Kalman filter and of the Rauch-Tung-Striebel smoother for <see cref="KalmanTrendModel"/>, in double precision and with
/// the closed 1 x 1 and 2 x 2 formulas. Samples that are not finite are missing: the state is predicted and no measurement is used.
/// </summary>
/// <remarks>
/// <para>
/// <b>Gate.</b> With a gate of <c>g</c> sigmas a measurement whose innovation exceeds <c>g</c> times its standard deviation is rejected like a missing
/// one (the filter coasts). A step of the signal would be rejected for ever, so after <see cref="Patience"/> rejections in a row the next measurement is
/// taken for a break: the variance of the state is inflated (the past is forgotten) and the measurement is used. The inflation belongs to the
/// prediction of that step, so the smoother sees it as a very large process noise and does not smooth across the break.
/// </para>
/// <para>
/// <b>Start.</b> The state starts at the first finite sample with a very large variance (<see cref="DiffuseFactor"/> times the measurement
/// variance): the first samples decide the level and the slope.
/// </para>
/// </remarks>
internal sealed class KalmanTrendCore
{
    /// <summary>
    /// Variance of the initial state and of the extra variance after a break, in units of the measurement variance: "any level, any slope".
    /// </summary>
    internal const double DiffuseFactor = 1e4;

    /// <summary>
    /// Number of measurements in a row that the gate rejects before it takes the next one for a break.
    /// </summary>
    internal const int Patience = 3;

    private readonly bool _trend;
    private readonly double _r;
    private readonly double _q;
    private readonly double _gate2;
    private readonly double _diffuse;

    /// <param name="model">Model</param>
    /// <param name="ratio">Process noise variance (of the level, or of the slope for the trend model) over the measurement noise variance</param>
    /// <param name="measurementVariance">Measurement noise variance, positive</param>
    /// <param name="gateSigma">Gate in standard deviations of the innovation, 0 for no gate</param>
    public KalmanTrendCore(KalmanTrendModel model, double ratio, double measurementVariance, double gateSigma)
    {
        _trend = model == KalmanTrendModel.LocalTrend;
        _r = measurementVariance;
        _q = ratio * measurementVariance;
        _gate2 = gateSigma * gateSigma;
        _diffuse = DiffuseFactor * measurementVariance;
    }

    public bool IsTrend => _trend;

    /// <summary>
    /// Starts the recursion at <paramref name="level"/>.
    /// </summary>
    public void Init(ref KalmanTrendState state, double level)
    {
        state.X.Level = level;
        state.X.Slope = 0;
        state.X.P00 = _diffuse;
        state.X.P01 = 0;
        state.X.P11 = _trend ? _diffuse : 0;
        state.Rejected = 0;
    }

    /// <summary>
    /// One sample: predicts the state and, if the sample is finite and passes the gate, corrects it with the measurement.
    /// </summary>
    public void Step(ref KalmanTrendState state, double z, bool finite, out KalmanTrendStep step)
    {
        // prediction
        KalmanTrendMoments p;
        if (_trend)
        {
            p.Level = state.X.Level + state.X.Slope;
            p.Slope = state.X.Slope;
            p.P00 = state.X.P00 + 2 * state.X.P01 + state.X.P11;
            p.P01 = state.X.P01 + state.X.P11;
            p.P11 = state.X.P11 + _q;
        }
        else
        {
            p.Level = state.X.Level;
            p.Slope = 0;
            p.P00 = state.X.P00 + _q;
            p.P01 = 0;
            p.P11 = 0;
        }

        step = default;
        var update = false;
        var innovation = double.NaN;
        var variance = double.NaN;

        if (finite)
        {
            innovation = z - p.Level;
            variance = p.P00 + _r;
            update = true;

            if (_gate2 > 0 && innovation * innovation > _gate2 * variance)
            {
                if (state.Rejected < Patience)
                {
                    update = false;
                    state.Rejected++;
                    step.Flags |= KalmanTrendStepFlags.Rejected;
                }
                else
                {
                    // the measurements have disagreed with the prediction for a while: the signal has changed, forget the past
                    p.P00 += _diffuse;
                    if (_trend)
                    {
                        p.P11 += _diffuse;
                    }

                    variance = p.P00 + _r;
                    state.Rejected = 0;
                    step.Flags |= KalmanTrendStepFlags.Break;
                }
            }
            else
            {
                state.Rejected = 0;
            }
        }
        else
        {
            step.Flags |= KalmanTrendStepFlags.Missing;
        }

        step.Predicted = p;
        step.Innovation = innovation;
        step.Variance = variance;

        // correction
        if (!update)
        {
            state.X = p;
            return;
        }

        var shrink = _r / variance;     // 1 - K0, kept as a quotient so that the variance cannot become negative
        if (_trend)
        {
            var k1 = p.P01 / variance;
            state.X.Level = p.Level + p.P00 / variance * innovation;
            state.X.Slope = p.Slope + k1 * innovation;
            state.X.P00 = p.P00 * shrink;
            state.X.P01 = p.P01 * shrink;
            var p11 = p.P11 - p.P01 * k1;
            state.X.P11 = p11 > 0 ? p11 : 0;
        }
        else
        {
            state.X.Level = p.Level + p.P00 / variance * innovation;
            state.X.Slope = 0;
            state.X.P00 = p.P00 * shrink;
            state.X.P01 = 0;
            state.X.P11 = 0;
        }
    }

    /// <summary>
    /// One step of the Rauch-Tung-Striebel recursion backwards: the smoothed moments at t from the filtered moments at t, and the predicted
    /// and the smoothed moments at t + 1.
    /// </summary>
    /// <param name="filtered">Filtered moments at t</param>
    /// <param name="predictedNext">Moments predicted for t + 1 (including the inflation of a break)</param>
    /// <param name="smoothedNext">Smoothed moments at t + 1</param>
    /// <param name="covariance">False skips the covariance (the mean of the smoothed state does not depend on it)</param>
    /// <param name="smoothed">Smoothed moments at t</param>
    public void Smooth(
        in KalmanTrendMoments filtered,
        in KalmanTrendMoments predictedNext,
        in KalmanTrendMoments smoothedNext,
        bool covariance,
        out KalmanTrendMoments smoothed)
    {
        smoothed = default;

        if (!_trend)
        {
            var c = predictedNext.P00 > 0 ? filtered.P00 / predictedNext.P00 : 0;
            smoothed.Level = filtered.Level + c * (smoothedNext.Level - predictedNext.Level);
            if (covariance)
            {
                smoothed.P00 = filtered.P00 + c * c * (smoothedNext.P00 - predictedNext.P00);
            }

            return;
        }

        // gain C = P F' (P_pred)^-1, F = [[1, 1], [0, 1]]
        var a = predictedNext.P00;
        var b = predictedNext.P01;
        var d = predictedNext.P11;
        var det = a * d - b * b;

        double c00 = 0, c01 = 0, c10 = 0, c11 = 0;
        if (det > 0 && double.IsFinite(det))
        {
            var m00 = filtered.P00 + filtered.P01;
            var m01 = filtered.P01;
            var m10 = filtered.P01 + filtered.P11;
            var m11 = filtered.P11;
            var inv = 1 / det;
            c00 = (m00 * d - m01 * b) * inv;
            c01 = (m01 * a - m00 * b) * inv;
            c10 = (m10 * d - m11 * b) * inv;
            c11 = (m11 * a - m10 * b) * inv;
        }

        var dl = smoothedNext.Level - predictedNext.Level;
        var ds = smoothedNext.Slope - predictedNext.Slope;
        smoothed.Level = filtered.Level + c00 * dl + c01 * ds;
        smoothed.Slope = filtered.Slope + c10 * dl + c11 * ds;

        if (covariance)
        {
            var d00 = smoothedNext.P00 - predictedNext.P00;
            var d01 = smoothedNext.P01 - predictedNext.P01;
            var d11 = smoothedNext.P11 - predictedNext.P11;

            // C D C'
            var t00 = c00 * d00 + c01 * d01;
            var t01 = c00 * d01 + c01 * d11;
            var t10 = c10 * d00 + c11 * d01;
            var t11 = c10 * d01 + c11 * d11;
            smoothed.P00 = filtered.P00 + t00 * c00 + t01 * c01;
            smoothed.P01 = filtered.P01 + t00 * c10 + t01 * c11;
            smoothed.P11 = filtered.P11 + t10 * c10 + t11 * c11;
        }
    }
}
