using System;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Fda
{
    /// <summary>
    /// Vocal Tract Length Normalization (VTLN) similar to Kaldi implementation.
    /// </summary>
    public class VtlnWarper
    {
        /// <summary>
        /// Lower frequency.
        /// </summary>
        private readonly float _lowFreq;

        /// <summary>
        /// Upper frequency.
        /// </summary>
        private readonly float _highFreq;

        /// <summary>
        /// Lower frequency for VTLN.
        /// </summary>
        private readonly float _lowVtln;

        /// <summary>
        /// Upper frequency for VTLN.
        /// </summary>
        private readonly float _highVtln;

        //
        // Intermediate parameters for calculations
        //
        private readonly float _scale;
        private readonly float _scaleLeft;
        private readonly float _scaleRight;

        /// <summary>
        /// Constructs <see cref="VtlnWarper"/>.
        /// </summary>
        /// <param name="alpha">Warping factor</param>
        /// <param name="lowFrequency">Lower frequency</param>
        /// <param name="highFrequency">Upper frequency</param>
        /// <param name="lowVtln">Lower frequency for VTLN</param>
        /// <param name="highVtln">Upper frequency for VTLN</param>
        public VtlnWarper(float alpha,
                          float lowFrequency,
                          float highFrequency,
                          float lowVtln,
                          float highVtln)
        {
            _lowFreq = lowFrequency;
            _highFreq = highFrequency;

            _lowVtln = lowVtln * Math.Max(1, alpha);
            _highVtln = highVtln * Math.Min(1, alpha);

            _scale = 1 / alpha;
            _scaleLeft = (_scale * _lowVtln - lowFrequency) / (_lowVtln - lowFrequency);
            _scaleRight = (highFrequency - _scale * _highVtln) / (highFrequency - _highVtln);
        }

        /// <summary>
        /// Warps <paramref name="frequency"/>.
        /// </summary>
        public float Warp(float frequency)
        {
            if (frequency < _lowVtln)
            {
                return _lowFreq + _scaleLeft * (frequency - _lowFreq);
            }
            else if (frequency < _highVtln)
            {
                return _scale * frequency;
            }

            return _highFreq + _scaleRight * (frequency - _highFreq);
        }
    }
}
