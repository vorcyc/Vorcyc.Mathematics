using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Effects.Base;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Effects
{
    // DAFX book [Udo Zoelzer], p.123-124.

    /// <summary>
    /// Represents Tube Distortion audio effect.
    /// </summary>
    public class TubeDistortionEffect : AudioEffect
    {
        /// <summary>
        /// Gets or sets input gain (in dB).
        /// </summary>
        public float InputGain
        {
            get => Scale.ToDecibel(_inputGain);
            set => _inputGain = Scale.FromDecibel(value);
        }
        private float _inputGain;

        /// <summary>
        /// Gets or sets output gain (in dB).
        /// </summary>
        public float OutputGain
        {
            get => Scale.ToDecibel(_outputGain);
            set => _outputGain = Scale.FromDecibel(value);
        }
        private float _outputGain;

        /// <summary>
        /// Gets or sets Q factor (Work point). 
        /// Controls the linearity of the transfer function for low input levels. 
        /// More negative - more linear.
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Gets or sets distortion's character. 
        /// Higher number - harder distortion.
        /// </summary>
        public float Dist { get; set; }

        /// <summary>
        /// Gets filter coefficient (close to 1.0) defining placement of poles 
        /// in the HP filter that removes DC component.
        /// </summary>
        public float Rh { get; }

        /// <summary>
        /// Gets filter coefficient (in range [0, 1]) defining placement of pole 
        /// in the LP filter used to simulate capacitances in tube amplifier.
        /// </summary>
        public float Rl { get; }

        /// <summary>
        /// Internal filter for output signal 
        /// that combines HP and LP filters mentioned above.
        /// </summary>
        private readonly LtiFilter _outputFilter;

        /// <summary>
        /// Constructs <see cref="TubeDistortionEffect"/>.
        /// </summary>
        /// <param name="inputGain">Input gain (in dB)</param>
        /// <param name="outputGain">Output gain (in dB)</param>
        /// <param name="q">Q factor (controls the linearity of the transfer function for low input levels. More negative means more linear)</param>
        /// <param name="dist">Distortion's character (higher number means harder distortion)</param>
        /// <param name="rh">Filter coefficient (close to 1.0) defining placement of poles in the HP filter that removes DC component</param>
        /// <param name="rl">Filter coefficient (in range [0, 1]) defining placement of pole in the LP filter used to simulate capacitances in tube amplifier</param>
        public TubeDistortionEffect(float inputGain = 20/*dB*/,
                                    float outputGain = -12/*dB*/,
                                    float q = -0.2f,
                                    float dist = 5,
                                    float rh = 0.995f,
                                    float rl = 0.5f)
        {
            InputGain = inputGain;
            OutputGain = outputGain;

            Q = q;
            Dist = dist;
            Rh = rh;
            Rl = rl;

            var filter1 = new IirFilter(new[] { 1.0f, -2f, 1f }, new[] { 1.0f, -2 * Rh, Rh * Rh });
            var filter2 = new IirFilter(new[] { 1.0f - Rl }, new[] { 1.0f, -Rl });

            _outputFilter = filter1 * filter2;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            float output;

            var q = sample * _inputGain;

            if (Math.Abs(Q) < 1e-10)
            {
                output = Math.Abs(q - Q) < 1e-10 ? 1.0f / Dist : (q / (1 - MathF.Exp(-Dist * q)));
            }
            else
            {
                output = Math.Abs(q - Q) < 1e-10f ?
                           (1.0f / Dist + Q / (1 - MathF.Exp(Dist * Q))) :
                           ((q - Q) / (1 - MathF.Exp(-Dist * (q - Q))) + Q / (1 - MathF.Exp(Dist * Q)));
            }

            output = _outputFilter.Process(output) * _outputGain;

            return output * Wet + sample * Dry;
        }

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            _outputFilter.Reset();
        }
    }
}
