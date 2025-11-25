using Vorcyc.Mathematics.SignalProcessing.Signals.Builders.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Signals.Builders
{
    /// <summary>
    /// Represents red (Brownian) noise builder.
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"low", "lo", "min" (default: -1.0)</item>
    ///     <item>"high", "hi", "max" (default: 1.0)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class RedNoiseBuilder : SignalBuilder
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
        /// Constructs <see cref="RedNoiseBuilder"/>.
        /// </summary>
        public RedNoiseBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<float>>
            {
                { "low, lo, min",  param => _low = param },
                { "high, hi, max", param => _high = param }
            };

            _low = -1.0f;
            _high = 1.0f;
        }

        /// <summary>
        /// Generates new sample.
        /// </summary>
        public override float NextSample()
        {
            // fancy filtering for obtaining the red noise

            var mean = (_low + _high) / 2;
            _low -= mean;
            _high -= mean;

            var white = Random.Shared.NextSingle() * (_high - _low) + _low;

            var red = (_prev + (0.02f * white)) / 1.02f;
            _prev = red;
            return (red * 3.5f + mean);
        }

        /// <summary>
        /// Resets sample generator.
        /// </summary>
        public override void Reset()
        {
            _prev = 0;
        }

        /// <summary>
        /// Generates signal by generating all its samples one-by-one. 
        /// Upper amplitude must be greater than lower amplitude.
        /// </summary>
        protected override DiscreteSignal Generate()
        {
            Guard.AgainstInvalidRange(_low, _high, "Upper amplitude", "Lower amplitude");
            return base.Generate();
        }

        private float _prev;

    }
}
