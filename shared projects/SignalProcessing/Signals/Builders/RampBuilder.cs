using Vorcyc.Mathematics.SignalProcessing.Signals.Builders.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Signals.Builders
{
    /// <summary>
    /// Represents builder of straight line signals: y[n] = slope * n + intercept. 
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"slope", "k" (default: 0.0)</item>
    ///     <item>"intercept", "b" (default: 0.0)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class RampBuilder : SignalBuilder
    {
        /// <summary>
        /// Slope.
        /// </summary>
        private float _slope;

        /// <summary>
        /// Intercept.
        /// </summary>
        private float _intercept;

        /// <summary>
        /// Constructs <see cref="RampBuilder"/>.
        /// </summary>
        public RampBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<float>>
            {
                { "slope, k",     param => _slope = param },
                { "intercept, b", param => _intercept = param }
            };

            _slope = 0.0f;
            _intercept = 0.0f;
        }

        /// <summary>
        /// Generates new sample.
        /// </summary>
        public override float NextSample()
        {
            var sample = _slope * _n + _intercept;
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

        private int _n;
    }
}
