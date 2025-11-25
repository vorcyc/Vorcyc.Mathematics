using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Effects
{
    /// <summary>
    /// Represents pitch shift audio effect 
    /// based on overlap-add filtering and pitch shifting in frequency domain.
    /// </summary>
    public class PitchShiftVocoderEffect : OverlapAddFilter
    {
        /// <summary>
        /// Gets or sets pitch shift ratio.
        /// </summary>
        public float Shift { get; set; }

        /// <summary>
        /// Frequency resolution.
        /// </summary>
        private readonly float _freqResolution;

        /// <summary>
        /// Array of spectrum magnitudes (at the current step).
        /// </summary>
        private readonly float[] _mag;

        /// <summary>
        /// Array of spectrum phases (at the current step).
        /// </summary>
        private readonly float[] _phase;

        /// <summary>
        /// Array of phases computed at the previous step.
        /// </summary>
        private readonly float[] _prevPhase;

        /// <summary>
        /// Array of new synthesized phases (at the current step).
        /// </summary>
        private readonly float[] _phaseTotal;

        /// <summary>
        /// Constructs <see cref="PitchShiftVocoderEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="shift">Pitch shift ratio</param>
        /// <param name="fftSize">FFT size</param>
        /// <param name="hopSize">Hop length</param>
        public PitchShiftVocoderEffect(int samplingRate, float shift, int fftSize = 1024, int hopSize = 64) : base(hopSize, fftSize)
        {
            Shift = (float)shift;

            _gain = 2 * ConstantsFp32.PI / (_fftSize * _window.Select(w => w * w).Sum() / _hopSize);

            _freqResolution = samplingRate / _fftSize;

            _mag = new float[_fftSize / 2 + 1];
            _phase = new float[_fftSize / 2 + 1];
            _prevPhase = new float[_fftSize / 2 + 1];
            _phaseTotal = new float[_fftSize / 2 + 1];
        }

        /// <summary>
        /// Processes one spectrum at each Overlap-Add STFT step.
        /// </summary>
        /// <param name="re">Real parts of input spectrum</param>
        /// <param name="im">Imaginary parts of input spectrum</param>
        /// <param name="filteredRe">Real parts of output spectrum</param>
        /// <param name="filteredIm">Imaginary parts of output spectrum</param>
        protected override void ProcessSpectrum(float[] re, float[] im, float[] filteredRe, float[] filteredIm)
        {
            var nextPhase = (2 * ConstantsFp32.PI * _hopSize / _fftSize);

            for (var j = 1; j <= _fftSize / 2; j++)
            {
                _mag[j] = MathF.Sqrt(re[j] * re[j] + im[j] * im[j]);
                _phase[j] = MathF.Atan2(im[j], re[j]);

                var delta = _phase[j] - _prevPhase[j];

                _prevPhase[j] = _phase[j];

                delta -= j * nextPhase;
                var deltaWrapped = VMath.Mod(delta + ConstantsFp32.PI, 2 * ConstantsFp32.PI) - ConstantsFp32.PI;

                _phase[j] = _freqResolution * (j + (float)deltaWrapped / nextPhase);
            }

            Array.Clear(re, 0, _fftSize);
            Array.Clear(im, 0, _fftSize);

            // "stretch" spectrum:

            var stretchPos = 0;
            for (var j = 0; j <= _fftSize / 2 && stretchPos <= _fftSize / 2; j++)
            {
                re[stretchPos] += _mag[j];
                im[stretchPos] = _phase[j] * Shift;

                stretchPos = (int)(j * Shift);
            }

            for (var j = 1; j <= _fftSize / 2; j++)
            {
                var mag = re[j];
                var freqIndex = (im[j] - j * _freqResolution) / _freqResolution;

                _phaseTotal[j] += nextPhase * (freqIndex + j);

                filteredRe[j] = mag * MathF.Cos(_phaseTotal[j]);
                filteredIm[j] = mag * MathF.Sin(_phaseTotal[j]);
            }
        }

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            Array.Clear(_prevPhase, 0, _prevPhase.Length);
            Array.Clear(_phaseTotal, 0, _phaseTotal.Length);
        }
    }
}
