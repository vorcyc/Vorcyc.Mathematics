namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the adaptive canceller of a sinusoidal interference of a known frequency (Widrow): the interference and its harmonics are modeled as
/// a sum of sines and cosines of the known frequencies with unknown, slowly varying amplitudes and phases, which an LMS filter estimates
/// and subtracts from the signal. It is the standard remedy for power line (50 / 60 Hz) hum: the notch is exactly as narrow as it is needed to follow the
/// amplitude and the phase of the interference, and the rest of the spectrum is not touched.
/// </summary>
/// <remarks>
/// With the normalized step <c>mu</c> and <c>K</c> harmonics every notch has a −3 dB width of about <c>mu / (2π K)</c> cycles per sample. The estimates of the
/// amplitudes converge within about <c>2 K / mu</c> samples. The frequency may be changed while the filter is running
/// (<see cref="Frequency"/>), the phase of the reference oscillator stays continuous.
/// </remarks>
public sealed class SinusoidalInterferenceCanceller
{
    private readonly int _harmonics;
    private readonly double _mu;
    private readonly double[] _weights;
    private double _frequency;
    private double _phase;

    /// <summary>
    /// Constructs <see cref="SinusoidalInterferenceCanceller"/>.
    /// </summary>
    /// <param name="frequency">Frequency of the interference in cycles per sample (frequency in Hz divided by the sampling rate), in (0, 0.5)</param>
    /// <param name="harmonics">Number of cancelled harmonics (1 for the fundamental only); all of them have to be below the Nyquist frequency</param>
    /// <param name="mu">Normalized step size, stable for 0 &lt; mu &lt; 2: larger values converge and track faster but also remove more of the signal around the interference</param>
    public SinusoidalInterferenceCanceller(double frequency, int harmonics = 1, double mu = 0.02)
    {
        Guard.AgainstNonPositive(harmonics, nameof(harmonics));
        AdaptiveArgument.Positive(mu, nameof(mu));
        var weightsLength = AdaptiveArgument.Count(2L * harmonics, nameof(harmonics));

        _harmonics = harmonics;
        _mu = mu;
        _weights = new double[weightsLength];
        Frequency = frequency;
    }

    /// <summary>
    /// Gets or sets the frequency of the interference in cycles per sample.
    /// </summary>
    public double Frequency
    {
        get => _frequency;
        set
        {
            if (!(value > 0 && value * _harmonics < 0.5))
                throw new ArgumentOutOfRangeException(nameof(value), "All harmonics must be below the Nyquist frequency: frequency · harmonics must be in (0, 0.5).");

            _frequency = value;
        }
    }

    /// <summary>
    /// Gets the number of harmonics.
    /// </summary>
    public int Harmonics => _harmonics;

    /// <summary>
    /// Gets the amplitude of the harmonic number <paramref name="harmonic"/> (1 is the fundamental) in the units of the signal.
    /// </summary>
    /// <param name="harmonic">Harmonic number, 1 … <see cref="Harmonics"/></param>
    public double Amplitude(int harmonic)
    {
        if (harmonic < 1 || harmonic > _harmonics)
            throw new ArgumentOutOfRangeException(nameof(harmonic));

        var c = _weights[2 * (harmonic - 1)];
        var s = _weights[2 * (harmonic - 1) + 1];
        return Math.Sqrt(c * c + s * s);
    }

    /// <summary>
    /// Processes one sample and returns the signal without the interference.
    /// </summary>
    /// <param name="input">Input sample</param>
    public float Process(float input) => Process(input, out _);

    /// <summary>
    /// Processes one sample and returns the signal without the interference.
    /// </summary>
    /// <param name="input">Input sample</param>
    /// <param name="interference">The estimate of the interference that was subtracted</param>
    public float Process(float input, out float interference)
    {
        var estimate = 0.0;
        for (var k = 1; k <= _harmonics; k++)
        {
            var angle = k * _phase;
            estimate += _weights[2 * (k - 1)] * Math.Cos(angle) + _weights[2 * (k - 1) + 1] * Math.Sin(angle);
        }

        var error = input - estimate;

        // a non-finite sample must not reach the weights; the reference oscillator goes on
        if (double.IsFinite(error))
        {
            // the references have the power 1 / 2 each: the normalization by their total power is the number of harmonics
            var step = _mu * error / _harmonics;
            for (var k = 1; k <= _harmonics; k++)
            {
                var angle = k * _phase;
                _weights[2 * (k - 1)] += step * Math.Cos(angle);
                _weights[2 * (k - 1) + 1] += step * Math.Sin(angle);
            }
        }

        _phase += 2 * Math.PI * _frequency;
        if (_phase > 2 * Math.PI)
        {
            _phase -= 2 * Math.PI;
        }

        interference = (float)estimate;
        return (float)error;
    }

    /// <summary>
    /// Processes a whole buffer.
    /// </summary>
    /// <param name="input">Input samples</param>
    /// <param name="output">Receives the signal without the interference (at least as long as <paramref name="input"/>)</param>
    public void ProcessAll(ReadOnlySpan<float> input, Span<float> output)
    {
        if (output.Length < input.Length)
            throw new ArgumentException("Output buffer is too short.", nameof(output));

        for (var i = 0; i < input.Length; i++)
        {
            output[i] = Process(input[i]);
        }
    }

    /// <summary>
    /// Forgets the estimated amplitudes and phases and restarts the reference oscillator.
    /// </summary>
    public void Reset()
    {
        Array.Clear(_weights);
        _phase = 0;
    }
}
