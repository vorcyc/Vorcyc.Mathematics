using Vorcyc.Mathematics.SignalProcessing.Effects.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

namespace Vorcyc.Mathematics.SignalProcessing.Effects
{
    /// <summary>
    /// Represents Tremolo audio effect.
    /// </summary>
    public class TremoloEffect : AudioEffect
    {
        /// <summary>
        /// Gets or sets depth.
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Gets or sets tremolo frequency (modulation frequency) (in Hz).
        /// </summary>
        public float Frequency
        {
            get => _frequency;
            set
            {
                _frequency = value;
                Lfo.SetLfoFrequency(value);
            }
        }
        private float _frequency;

        /// <summary>
        /// Gets or sets tremolo index (modulation index).
        /// </summary>
        public float Index
        {
            get => _index;
            set
            {
                _index = value;
                Lfo.SetLfoRange(0, value * 2);
            }
        }
        private float _index;

        /// <summary>
        /// Gets or sets LFO signal generator.
        /// </summary>
        public ISampleGenerator Lfo { get; set; }

        /// <summary>
        /// Constructs <see cref="TremoloEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="depth">Depth</param>
        /// <param name="frequency">Tremolo frequency (modulation frequency) (in Hz)</param>
        /// <param name="tremoloIndex">Tremolo index (modulation index)</param>
        public TremoloEffect(int samplingRate, float depth = 0.5f, float frequency = 10/*Hz*/, float tremoloIndex = 0.5f)
        {
            Lfo = new CosineOscillator { SamplingRate = samplingRate };

            Depth = depth;
            Frequency = frequency;
            Index = tremoloIndex;
        }

        /// <summary>
        /// Constructs <see cref="TremoloEffect"/> from <paramref name="lfo"/>.
        /// </summary>
        /// <param name="lfo">LFO signal generator</param>
        /// <param name="depth">Depth</param>
        public TremoloEffect(ISampleGenerator lfo, float depth = 0.5f)
        {
            Lfo = lfo;
            Depth = depth;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = sample * (1 - Depth + Depth * Lfo.NextSample());

            return output * Wet + sample * Dry;
        }

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            Lfo.Reset();
        }
    }
}
