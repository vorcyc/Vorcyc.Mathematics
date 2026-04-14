namespace Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

/// <summary>
/// Drum variation of the Karplus-Strong algorithm.
/// </summary>
public sealed class KarplusStrongDrumGenerator : KarplusStrongGenerator
{
    /// <summary>
    /// Probability of positive feedback branch.
    /// </summary>
    public float Probability { get; set; } = 0.5f;

    /// <inheritdoc />
    public override float NextSample()
    {
        var idx = ((int)_n) % _samples.Length;

        if (_rand.NextDouble() < 1 / StretchFactor)
        {
            if (_rand.NextDouble() < Probability)
            {
                _samples[idx] = 0.5f * (_samples[idx] + _prev) * Feedback;
            }
            else
            {
                _samples[idx] = -0.5f * (_samples[idx] + _prev) * Feedback;
            }
        }

        _prev = _samples[idx];
        _n++;

        return _prev;
    }
}
