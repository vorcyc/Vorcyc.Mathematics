using Vorcyc.Mathematics;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.OnePole
{
    /// <summary>
    /// Represents one-pole highpass filter.
    /// </summary>
    public class HighPassFilter : OnePoleFilter
    {
        /// <summary>
        /// Gets cutoff frequency.
        /// </summary>
        public float Frequency { get; protected set; }

        /// <summary>
        /// Constructs <see cref="HighPassFilter"/> with given cutoff <paramref name="frequency"/>.
        /// </summary>
        /// <param name="frequency">Cutoff frequency</param>
        public HighPassFilter(float frequency)
        {
            SetCoefficients(frequency);
        }

        /// <summary>
        /// Sets filter coefficients based on given cutoff <paramref name="frequency"/>.
        /// </summary>
        /// <param name="frequency">Cutoff frequency</param>
        private void SetCoefficients(float frequency)
        {
            Frequency = frequency;

            _a[0] = 1;
            _a[1] = MathF.Exp(-2 * ConstantsFp32.PI * (0.5f - frequency));

            _b[0] = 1 - _a[1];
        }

        /// <summary>
        /// Changes filter coefficients (preserving the state of the filter).
        /// </summary>
        /// <param name="frequency">Cutoff frequency</param>
        public void Change(float frequency)
        {
            SetCoefficients(frequency);
        }
    }
}
