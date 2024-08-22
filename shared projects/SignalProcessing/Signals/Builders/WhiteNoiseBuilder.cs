using Vorcyc.Mathematics.SignalProcessing.Signals.Builders.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Signals.Builders
{
    /// <summary>
    /// Represents white noise builder.
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"low", "lo", "min" (default: -1.0)</item>
    ///     <item>"high", "hi", "max" (default: 1.0)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class WhiteNoiseBuilder : SignalBuilder
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
        /// Constructs <see cref="WhiteNoiseBuilder"/>.
        /// </summary>
        public WhiteNoiseBuilder()
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
            return (Random.Shared.NextSingle() * (_high - _low) + _low);
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

        private readonly Random _rand = new Random();
    }
}
