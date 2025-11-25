using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Signals.Builders.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Signals.Builders
{
    /// <summary>
    /// Represents builder of sinusoidal signals.
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"low", "lo", "min" (default: -1.0)</item>
    ///     <item>"high", "hi", "max" (default: 1.0)</item>
    ///     <item>"frequency", "freq" (default: 100.0 Hz)</item>
    ///     <item>"phase", "phi" (default: 0.0)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class SineBuilder : SignalBuilder
    {
        /// <summary>
        /// Lower amplitude level.
        /// </summary>
        private float _low;

        /// <summary>
        /// Upper amplitude level.
        /// </summary>
        private float _high;

        /// <summary>
        /// Frequency of the sinusoid (in Hz).
        /// </summary>
        private float _frequency;

        /// <summary>
        /// Initial phase of the sinusoid (in radians).
        /// </summary>
        private float _phase;

        /// <summary>
        /// Constructs <see cref="SineBuilder"/>.
        /// </summary>
        public SineBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<float>>
            {
                { "low, lo, min",    param => _low = param },
                { "high, hi, max",   param => _high = param },
                { "frequency, freq", param => _frequency = param },
                { "phase, phi",      param => _phase = param }
            };

            _low = -1.0f;
            _high = 1.0f;
            _frequency = 100.0f;
            _phase = 0.0f;
        }

        /// <summary>
        /// Generates new sample.
        /// </summary>
        public override float NextSample()
        {
            var sample = MathF.Sin(2 * ConstantsFp32.PI * _frequency / SamplingRate * _n + _phase);

            // map it to [min, max] range:

            sample = _low + (_high - _low) * (1 + sample) / 2;

            _n++;
            return sample;
        }

        /// <summary>
        /// Resets sample generator.
        /// </summary>
        public override void Reset()
        {
            _n = 0;
        }

        /// <summary>
        /// Generates signal by generating all its samples one-by-one. 
        /// Frequency must be greater than zero.
        /// </summary>
        protected override DiscreteSignal Generate()
        {
            Guard.AgainstNonPositive(_frequency, "Frequency");
            return base.Generate();
        }

        private int _n;
    }
}
