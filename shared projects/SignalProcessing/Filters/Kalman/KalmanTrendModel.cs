namespace Vorcyc.Mathematics.SignalProcessing.Filters.Kalman;

/// <summary>
/// Structural model of the signal that <see cref="KalmanTrendFilter"/> and <see cref="KalmanTrend"/> estimate: a slowly varying part
/// observed in white measurement noise, <c>z[t] = x[t] + v[t]</c>.
/// </summary>
public enum KalmanTrendModel
{
    /// <summary>
    /// Local level: the level is a random walk, <c>x[t] = x[t-1] + w[t]</c>. It follows steps and bends at once and has no sense of direction,
    /// so a smoothed signal is a gently wandering staircase. The causal filter of this model is an exponential moving average whose gain settles at
    /// <c>sqrt(ratio)</c> for a small ratio.
    /// </summary>
    LocalLevel = 0,

    /// <summary>
    /// Smooth trend (integrated random walk): the level follows a slope, <c>x[t] = x[t-1] + s[t-1]</c>, and the slope is a random walk,
    /// <c>s[t] = s[t-1] + w[t]</c>. The smoother of this model is the discrete cubic smoothing spline; it follows curved trends with far less bias than
    /// <see cref="LocalLevel"/> and estimates the derivative as well.
    /// </summary>
    LocalTrend = 1,
}

/// <summary>
/// Optional outputs of <see cref="KalmanTrend"/> besides the level estimate (every output costs one array of the length of the signal).
/// </summary>
[Flags]
public enum KalmanTrendOutputs
{
    /// <summary>
    /// Only the level estimate.
    /// </summary>
    None = 0,

    /// <summary>
    /// The slope estimate in units per sample (<see cref="KalmanTrendModel.LocalTrend"/> only).
    /// </summary>
    Slope = 1,

    /// <summary>
    /// The standard deviation of the level estimate.
    /// </summary>
    StdDev = 2,

    /// <summary>
    /// The standardized innovations: the one-step prediction errors of the forward pass divided by their standard deviation. For a correct model
    /// they are white with unit variance; a value beyond 3 flags an outlier or a change that the model does not expect.
    /// </summary>
    StandardizedInnovation = 4,

    /// <summary>
    /// All outputs.
    /// </summary>
    All = Slope | StdDev | StandardizedInnovation,
}
