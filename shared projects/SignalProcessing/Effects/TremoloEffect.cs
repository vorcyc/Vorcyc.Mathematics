using Vorcyc.Mathematics.SignalProcessing.Effects.Base;
using Vorcyc.Mathematics.SignalProcessing.Signals.Generators;

namespace Vorcyc.Mathematics.SignalProcessing.Effects
{
    /// <summary>
    /// Represents Tremolo audio effect.
    /// </summary>
    public class TremoloEffect : AudioEffect
    {
        // Stored parameters for deferred initialization
        private float _frequencyHz;
        private float _tremoloIndex;
        private bool _useCustomLfo;
        private ISampleGenerator? _customLfo;

        /// <summary>
        /// Gets or sets depth.
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Gets or sets tremolo frequency (modulation frequency) (in Hz).
        /// </summary>
        public float Frequency
        {
            get => _frequencyHz;
            set
            {
                _frequencyHz = value;
                Lfo?.SetLfoFrequency(value);
            }
        }

        /// <summary>
        /// Gets or sets tremolo index (modulation index).
        /// </summary>
        public float Index
        {
            get => _tremoloIndex;
            set
            {
                _tremoloIndex = value;
                Lfo?.SetLfoRange(0, value * 2);
            }
        }

        /// <summary>
        /// Gets or sets LFO signal generator.
        /// </summary>
        public ISampleGenerator? Lfo { get; set; }

        /// <summary>
        /// Constructs <see cref="TremoloEffect"/> with deferred sampling rate initialization.
        /// Call <see cref="SetSamplingRate"/> before using.
        /// </summary>
        /// <param name="depth">Depth</param>
        /// <param name="frequency">Tremolo frequency (modulation frequency) (in Hz)</param>
        /// <param name="tremoloIndex">Tremolo index (modulation index)</param>
        public TremoloEffect(float depth = 0.5f, float frequency = 10/*Hz*/, float tremoloIndex = 0.5f)
        {
            Depth = depth;
            _frequencyHz = frequency;
            _tremoloIndex = tremoloIndex;
            _useCustomLfo = false;
        }

        /// <summary>
        /// Constructs <see cref="TremoloEffect"/> with deferred sampling rate initialization from custom LFO.
        /// Call <see cref="SetSamplingRate"/> before using.
        /// </summary>
        /// <param name="lfo">LFO signal generator</param>
        /// <param name="depth">Depth</param>
        public TremoloEffect(ISampleGenerator lfo, float depth = 0.5f)
        {
            _customLfo = lfo;
            Depth = depth;
            _useCustomLfo = true;
        }

        /// <summary>
        /// Constructs <see cref="TremoloEffect"/> with immediate sampling rate.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="depth">Depth</param>
        /// <param name="frequency">Tremolo frequency (modulation frequency) (in Hz)</param>
        /// <param name="tremoloIndex">Tremolo index (modulation index)</param>
        public TremoloEffect(int samplingRate, float depth = 0.5f, float frequency = 10/*Hz*/, float tremoloIndex = 0.5f)
            : this(depth, frequency, tremoloIndex)
        {
            SetSamplingRate(samplingRate);
        }

        /// <summary>
        /// Sets sampling rate and initializes LFO.
        /// </summary>
        public override void SetSamplingRate(int samplingRate)
        {
            if (_useCustomLfo && _customLfo != null)
            {
                Lfo = _customLfo;
                if (Lfo is CosineOscillator cosine)
                    cosine.SamplingRate = samplingRate;
            }
            else
            {
                Lfo = new CosineOscillator { SamplingRate = samplingRate };
                Lfo.SetLfoFrequency(_frequencyHz);
                Lfo.SetLfoRange(0, _tremoloIndex * 2);
            }
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            if (Lfo == null)
                throw new InvalidOperationException("Sampling rate not set. Call SetSamplingRate first.");

            var output = sample * (1 - Depth + Depth * Lfo.NextSample());

            return output * Wet + sample * Dry;
        }

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            Lfo?.Reset();
        }
    }
}
