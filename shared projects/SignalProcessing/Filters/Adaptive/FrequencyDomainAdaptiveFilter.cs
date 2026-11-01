using Vorcyc.Mathematics.SignalProcessing.Fourier;

namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the partitioned block frequency-domain Adaptive filter (PBFDAF / multidelay filter, overlap-save):
/// the signals are processed in blocks of <c>blockSize</c> samples, the filter of <c>blockSize · partitions</c> taps is split into
/// <c>partitions</c> sub-filters that are convolved and adapted in the frequency domain, and every frequency bin is
/// normalized by the power of the input in this bin. It converges like <see cref="NlmsFilter"/> for white input and much faster for colored input,
/// and its cost per sample grows with log(blockSize) instead of the filter order, which makes it the choice for long filters
/// (e.g. acoustic echo paths with thousands of taps). With <c>partitions = 1</c> it is the classic FDAF.
/// </summary>
public sealed class FrequencyDomainAdaptiveFilter
{
    private readonly int _blockSize;
    private readonly int _partitions;
    private readonly int _fftSize;
    private readonly float _mu;
    private readonly float _forgetting;
    private readonly float _eps;
    private readonly bool _constrained;
    private readonly Fft _fft;

    /// <summary>
    /// Last 2 · blockSize input samples: the previous block followed by the current one.
    /// </summary>
    private readonly float[] _frame;

    /// <summary>
    /// Spectra of the last <c>partitions</c> input frames (ring buffer, <c>_head</c> is the newest).
    /// </summary>
    private readonly float[][] _xRe;
    private readonly float[][] _xIm;

    /// <summary>
    /// Spectra of the weights of every partition.
    /// </summary>
    private readonly float[][] _wRe;
    private readonly float[][] _wIm;

    /// <summary>
    /// Smoothed power of the newest input spectrum.
    /// </summary>
    private readonly float[] _power;

    private readonly float[] _yRe;
    private readonly float[] _yIm;
    private readonly float[] _eRe;
    private readonly float[] _eIm;
    private readonly float[] _gRe;
    private readonly float[] _gIm;
    private int _head;
    private bool _powerReady;

    /// <summary>
    /// Constructs <see cref="FrequencyDomainAdaptiveFilter"/> of <paramref name="blockSize"/> · <paramref name="partitions"/> taps.
    /// </summary>
    /// <param name="blockSize">Number of samples per block (a power of two); it is also the length of every partition and the number of samples per weight update</param>
    /// <param name="partitions">Number of partitions, the filter order is <paramref name="blockSize"/> · <paramref name="partitions"/></param>
    /// <param name="mu">Step size with the same meaning as for <see cref="NlmsFilter"/>: stable for 0 &lt; mu &lt; 2, usually 0.1 .. 1</param>
    /// <param name="forgetting">Forgetting factor of the power estimate of every bin, in [0, 0.999999]</param>
    /// <param name="eps">Epsilon, regularizes the per-bin normalization (added to the power of the whole regressor); keep it small compared to the expected power of the input over <paramref name="blockSize"/> · <paramref name="partitions"/> samples</param>
    /// <param name="constrained">Constrains the gradient of every partition to its own length. Turning it off saves two FFTs per partition and block, but the weights are biased (circular convolution aliasing) unless the impulse response is much shorter than the filter</param>
    public FrequencyDomainAdaptiveFilter(int blockSize, int partitions = 1, float mu = 0.5f, float forgetting = 0.9f, float eps = 1e-3f, bool constrained = true)
    {
        Guard.AgainstNotPowerOfTwo(blockSize, nameof(blockSize));
        Guard.AgainstNonPositive(partitions, nameof(partitions));
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.InRange(forgetting, 0f, 0.999999f, nameof(forgetting));
        AdaptiveArgument.NonNegative(eps, nameof(eps));

        _blockSize = blockSize;
        _partitions = partitions;
        _fftSize = 2 * blockSize;
        _mu = mu;
        _forgetting = forgetting;
        _eps = eps;
        _constrained = constrained;
        _fft = new Fft(_fftSize);

        _frame = new float[_fftSize];
        _xRe = new float[partitions][];
        _xIm = new float[partitions][];
        _wRe = new float[partitions][];
        _wIm = new float[partitions][];
        for (var p = 0; p < partitions; p++)
        {
            _xRe[p] = new float[_fftSize];
            _xIm[p] = new float[_fftSize];
            _wRe[p] = new float[_fftSize];
            _wIm[p] = new float[_fftSize];
        }

        _power = new float[_fftSize];
        _yRe = new float[_fftSize];
        _yIm = new float[_fftSize];
        _eRe = new float[_fftSize];
        _eIm = new float[_fftSize];
        _gRe = new float[_fftSize];
        _gIm = new float[_fftSize];
    }

    /// <summary>
    /// Gets the number of samples per block.
    /// </summary>
    public int BlockSize => _blockSize;

    /// <summary>
    /// Gets the number of partitions.
    /// </summary>
    public int Partitions => _partitions;

    /// <summary>
    /// Gets the filter order (number of taps): block size · partitions.
    /// </summary>
    public int Order => _blockSize * _partitions;

    /// <summary>
    /// Gets a copy of the time-domain weights (impulse response of the filter).
    /// </summary>
    public float[] Kernel
    {
        get
        {
            var kernel = new float[Order];
            var re = new float[_fftSize];
            var im = new float[_fftSize];

            for (var p = 0; p < _partitions; p++)
            {
                Array.Copy(_wRe[p], re, _fftSize);
                Array.Copy(_wIm[p], im, _fftSize);
                _fft.InverseNorm(re, im);
                Array.Copy(re, 0, kernel, p * _blockSize, _blockSize);
            }

            return kernel;
        }
    }

    /// <summary>
    /// Inits the weights (impulse response) of the filter and clears the signal history.
    /// </summary>
    /// <param name="weights">Weights, <see cref="Order"/> finite values</param>
    /// <exception cref="ArgumentException"><paramref name="weights"/> does not have <see cref="Order"/> elements or holds NaN or infinity; the filter is not changed then.</exception>
    public void Init(ReadOnlySpan<float> weights)
    {
        if (weights.Length != Order)
            throw new ArgumentException("Weights array size must be equal to the filter order.", nameof(weights));
        AdaptiveArgument.AllFinite(weights, nameof(weights));

        for (var p = 0; p < _partitions; p++)
        {
            Array.Clear(_wRe[p]);
            Array.Clear(_wIm[p]);
            weights.Slice(p * _blockSize, _blockSize).CopyTo(_wRe[p]);
            _fft.Direct(_wRe[p], _wIm[p]);
        }

        Reset();
    }

    /// <summary>
    /// Clears the signal history and the power estimates; the weights are preserved.
    /// </summary>
    public void Reset()
    {
        Array.Clear(_frame);
        Array.Clear(_power);
        for (var p = 0; p < _partitions; p++)
        {
            Array.Clear(_xRe[p]);
            Array.Clear(_xIm[p]);
        }

        _head = 0;
        _powerReady = false;
    }

    /// <summary>
    /// Processes whole blocks of the input and desired signals; a tail shorter than a block is left untouched.
    /// </summary>
    /// <param name="input">Input signal</param>
    /// <param name="desired">Desired signal (same length as <paramref name="input"/>)</param>
    /// <param name="output">Receives the filter output (at least as long as the processed part of <paramref name="input"/>)</param>
    /// <returns>Number of processed samples, a multiple of <see cref="BlockSize"/></returns>
    public int ProcessAll(ReadOnlySpan<float> input, ReadOnlySpan<float> desired, Span<float> output)
    {
        if (desired.Length != input.Length)
            throw new ArgumentException("Desired signal must have the same length as the input signal.", nameof(desired));

        var processed = input.Length / _blockSize * _blockSize;
        if (output.Length < processed)
            throw new ArgumentException("Output buffer is too short.", nameof(output));

        for (var start = 0; start < processed; start += _blockSize)
        {
            ProcessBlock(input.Slice(start, _blockSize), desired.Slice(start, _blockSize), output.Slice(start, _blockSize));
        }

        return processed;
    }

    /// <summary>
    /// Processes one block of the input and desired signals, adapts the weights and returns the output of the filter
    /// (computed with the weights before this adaptation).
    /// </summary>
    /// <param name="input">Block of the input signal, <see cref="BlockSize"/> samples</param>
    /// <param name="desired">Block of the desired signal, <see cref="BlockSize"/> samples</param>
    /// <param name="output">Receives the filter output, at least <see cref="BlockSize"/> samples</param>
    public void ProcessBlock(ReadOnlySpan<float> input, ReadOnlySpan<float> desired, Span<float> output)
    {
        var b = _blockSize;
        var m = _fftSize;

        if (input.Length != b)
            throw new ArgumentException("Input block must have BlockSize samples.", nameof(input));
        if (desired.Length != b)
            throw new ArgumentException("Desired block must have BlockSize samples.", nameof(desired));
        if (output.Length < b)
            throw new ArgumentException("Output buffer is too short.", nameof(output));

        // the newest frame is [previous block, current block]
        Array.Copy(_frame, b, _frame, 0, b);
        input.CopyTo(_frame.AsSpan(b, b));

        _head = (_head + _partitions - 1) % _partitions;
        var newestRe = _xRe[_head];
        var newestIm = _xIm[_head];
        Array.Copy(_frame, newestRe, m);
        Array.Clear(newestIm);
        _fft.Direct(newestRe, newestIm);

        // Y = sum over the partitions of W_p · X_p
        Array.Clear(_yRe);
        Array.Clear(_yIm);
        for (var p = 0; p < _partitions; p++)
        {
            var slot = (_head + p) % _partitions;
            var xr = _xRe[slot];
            var xi = _xIm[slot];
            var wr = _wRe[p];
            var wi = _wIm[p];
            for (var k = 0; k < m; k++)
            {
                _yRe[k] += wr[k] * xr[k] - wi[k] * xi[k];
                _yIm[k] += wr[k] * xi[k] + wi[k] * xr[k];
            }
        }

        // overlap-save: only the last block of the circular convolution is valid
        _fft.InverseNorm(_yRe, _yIm);
        for (var i = 0; i < b; i++)
        {
            output[i] = _yRe[b + i];
        }

        // error block, zero-padded in front
        Array.Clear(_eRe);
        Array.Clear(_eIm);
        var finiteError = true;
        for (var i = 0; i < b; i++)
        {
            _eRe[b + i] = desired[i] - output[i];
            finiteError &= float.IsFinite(_eRe[b + i]);
        }

        // A non-finite sample (the input stays in the frame and in the spectra of the partitions for a while, so the output is not finite as long as it does) would end in the power
        // estimate and in the weights for good: such a block does not adapt anything.
        if (!finiteError)
        {
            return;
        }

        _fft.Direct(_eRe, _eIm);

        for (var k = 0; k < m; k++)
        {
            var instantaneous = newestRe[k] * newestRe[k] + newestIm[k] * newestIm[k];
            _power[k] = _powerReady ? _forgetting * _power[k] + (1 - _forgetting) * instantaneous : instantaneous;
        }
        _powerReady = true;

        for (var p = 0; p < _partitions; p++)
        {
            var slot = (_head + p) % _partitions;
            var xr = _xRe[slot];
            var xi = _xIm[slot];

            // normalized gradient conj(X_p) · E / (power of the regressor + eps): partitions · power / 2 is the power of the whole regressor, as in NLMS
            for (var k = 0; k < m; k++)
            {
                var norm = 0.5f * _partitions * _power[k] + _eps;
                if (norm > 0)
                {
                    _gRe[k] = (xr[k] * _eRe[k] + xi[k] * _eIm[k]) / norm;
                    _gIm[k] = (xr[k] * _eIm[k] - xi[k] * _eRe[k]) / norm;
                }
                else
                {
                    // eps = 0 and an empty bin
                    _gRe[k] = 0;
                    _gIm[k] = 0;
                }
            }

            if (_constrained)
            {
                _fft.InverseNorm(_gRe, _gIm);
                Array.Clear(_gRe, b, b);
                Array.Clear(_gIm);
                _fft.Direct(_gRe, _gIm);
            }

            var wr = _wRe[p];
            var wi = _wIm[p];
            for (var k = 0; k < m; k++)
            {
                wr[k] += _mu * _gRe[k];
                wi[k] += _mu * _gIm[k];
            }
        }
    }
}
