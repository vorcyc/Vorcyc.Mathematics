﻿using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;
using Vorcyc.Mathematics.SignalProcessing.Operations.Convolution;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.SignalProcessing.Operations.Tsm;

/// <summary>
/// Represents TSM processor based on Waveform-Synchronized Overlap-Add (WSOLA) technique.
/// </summary>
public class Wsola : IFilter
{
    /// <summary>
    /// Stretch ratio.
    /// </summary>
    protected readonly double _stretch;

    /// <summary>
    /// Window size.
    /// </summary>
    private int _windowSize;

    /// <summary>
    /// Hop size at analysis stage (STFT decomposition).
    /// </summary>
    private int _hopAnalysis;

    /// <summary>
    /// Hop size at synthesis stage (STFT merging).
    /// </summary>
    private int _hopSynthesis;

    /// <summary>
    /// Maximum length of the fragment for search of the most similar waveform.
    /// </summary>
    private int _maxDelta;

    /// <summary>
    /// True if parameters were set by user (not by default).
    /// </summary>
    private readonly bool _userParameters;

    /// <summary>
    /// Internal convolver
    /// (will be used for evaluating auto-correlation if the window size is too big).
    /// </summary>
    private Convolver _convolver;

    /// <summary>
    /// Cross-correlation signal.
    /// </summary>
    private float[] _cc;

    /// <summary>
    /// Constructs <see cref="Wsola"/> TSM processor.
    /// </summary>
    /// <param name="stretch">Stretch ratio</param>
    /// <param name="windowSize">Window size</param>
    /// <param name="hopAnalysis">Hop size at analysis stage</param>
    /// <param name="maxDelta">Max delta in WSOLA algorithm</param>
    public Wsola(double stretch, int windowSize, int hopAnalysis, int maxDelta = 0)
    {
        _stretch = stretch;
        _windowSize = Math.Max(windowSize, 32);
        _hopAnalysis = Math.Max(hopAnalysis, 10);
        _hopSynthesis = (int)(_hopAnalysis * stretch);
        _maxDelta = maxDelta > 2 ? maxDelta : _hopSynthesis;

        _userParameters = true;

        PrepareConvolver();
    }

    /// <summary>
    /// Constructs <see cref="Wsola"/> TSM processor and auto-derives WSOLA parameters for given <paramref name="stretch"/> ratio.
    /// </summary>
    /// <param name="stretch">Stretch ratio</param>
    public Wsola(double stretch)
    {
        _stretch = stretch;

        // IMO these are good parameters for different stretch ratios

        if (_stretch > 1.5)        // parameters are for 22.05 kHz sampling rate, so they will be adjusted for an input signal
        {
            _windowSize = 1024;     // 46,4 ms
            _hopAnalysis = 128;     //  5,8 ms
        }
        else if (_stretch > 1.1)
        {
            _windowSize = 1536;     // 69,7 ms
            _hopAnalysis = 256;     // 10,6 ms
        }
        else if (_stretch > 0.6)
        {
            _windowSize = 1536;     // 69,7 ms
            _hopAnalysis = 690;     // 31,3 ms
        }
        else
        {
            _windowSize = 1024;     // 46,4 ms
            _hopAnalysis = 896;     // 40,6 ms
        }

        _hopSynthesis = (int)(_hopAnalysis * stretch);
        _maxDelta = _hopSynthesis;

        PrepareConvolver();
    }

    /// <summary>
    /// Prepares the internal convolver (for large window sizes).
    /// </summary>
    private void PrepareConvolver()
    {
        var fftSize = (_windowSize + _maxDelta - 1).NextPowerOf2();

        if (fftSize >= 512)
        {
            _convolver = new Convolver(fftSize);
            _cc = new float[fftSize];
        }
    }

    /// <summary>
    /// Processes entire <paramref name="signal"/> and returns new time-stretched signal.
    /// </summary>
    public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
    {
        // adjust default parameters for a new sampling rate

        if (signal.SamplingRate != 22050 && !_userParameters)
        {
            var factor = (float)signal.SamplingRate / 22050;

            _windowSize = (int)(_windowSize * factor);
            _hopAnalysis = (int)(_hopAnalysis * factor);
            _hopSynthesis = (int)(_hopAnalysis * _stretch);
            _maxDelta = (int)(_maxDelta * factor);

            PrepareConvolver();
        }

        var window = WindowBuilder.OfType(WindowType.Hann, _windowSize);
        var gain = _hopSynthesis / window.Select(w => w * w).Sum() * 0.75f;

        // and now WSOLA:

        var input = signal.Samples;
        var output = new float[(int)(_stretch * (input.Length + _windowSize))];

        var current = new float[_windowSize + _maxDelta];
        var prev = new float[_windowSize];

        int posSynthesis = 0;

        for (int posAnalysis = 0;
                 posAnalysis + _windowSize + _maxDelta + _hopSynthesis < input.Length;
                 posAnalysis += _hopAnalysis,
                 posSynthesis += _hopSynthesis)
        {
            int delta = 0;

            if (posAnalysis > _maxDelta / 2)
            {
                input.Values.FastCopyTo(current, _windowSize + _maxDelta, posAnalysis - _maxDelta / 2);

                delta = WaveformSimilarityPos(current, prev, _maxDelta);
            }
            else
            {
                input.Values.FastCopyTo(current, _windowSize + _maxDelta, posAnalysis);
            }

            int size = Math.Min(_windowSize, output.Length - posSynthesis);

            for (var j = 0; j < size; j++)
            {
                output[posSynthesis + j] += current[delta + j] * window[j];
            }

            for (var j = 0; j < _hopSynthesis; j++)
            {
                output[posSynthesis + j] *= gain;
            }

            input.Values.FastCopyTo(prev, _windowSize, posAnalysis + delta - _maxDelta / 2 + _hopSynthesis);
        }

        return new DiscreteSignal(signal.SamplingRate, output);
    }

    /// <summary>
    /// Finds position of the best waveform similarity.
    /// </summary>
    /// <param name="current">Current window</param>
    /// <param name="prev">Previous window</param>
    /// <param name="maxDelta">Max delta</param>
    protected int WaveformSimilarityPos(float[] current, float[] prev, int maxDelta)
    {
        var optimalShift = 0;
        var maxCorrelation = 0.0f;

        // for small window sizes cross-correlate directly:

        if (_convolver is null)
        {
            for (var i = 0; i < maxDelta; i++)
            {
                var xcorr = 0.0f;

                for (var j = 0; j < prev.Length; j++)
                {
                    xcorr += current[i + j] * prev[j];
                }

                if (xcorr > maxCorrelation)
                {
                    maxCorrelation = xcorr;
                    optimalShift = i;
                }
            }
        }
        // for very large window sizes better use FFT convolution:
        else
        {
            _convolver.CrossCorrelate(current, prev, _cc);

            for (int i = prev.Length - 1, j = 0; i < prev.Length + _maxDelta - 1; i++, j++)
            {
                if (_cc[i] > maxCorrelation)
                {
                    maxCorrelation = _cc[i];
                    optimalShift = j;
                }
            }
        }

        return optimalShift;
    }
}
