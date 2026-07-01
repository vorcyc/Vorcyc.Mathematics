using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.SignalProcessing.Transforms;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

namespace Vorcyc.Mathematics.DeepLearning.Integration.Frontends;

/// <summary>
/// Fixed STFT magnitude frontend (N×1×1×L → N×T×F×1). Backward is stop-gradient.
/// </summary>
public sealed class BatchStftMagnitudeLayer : BatchLayerBase<float>
{
    private readonly Stft _stft;
    private readonly ThreadLocal<Stft> _parallelStft;
    private readonly int _windowSize;
    private readonly int _hopSize;
    private readonly int _frequencyBins;
    private readonly bool _normalizeSpectrum;

    /// <summary>
    /// Creates an STFT magnitude layer with the same parameters as <see cref="Stft"/>.
    /// </summary>
    public BatchStftMagnitudeLayer(
        int windowSize,
        int hopSize,
        WindowType window = WindowType.Hann,
        int fftSize = 0,
        bool normalizeSpectrum = true,
        string? name = null) : base(name)
    {
        if (windowSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowSize));
        }

        if (hopSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hopSize));
        }

        _windowSize = windowSize;
        _hopSize = hopSize;
        _normalizeSpectrum = normalizeSpectrum;
        _stft = new Stft(windowSize, hopSize, window, fftSize);
        _parallelStft = new ThreadLocal<Stft>(() => new Stft(windowSize, hopSize, window, fftSize));
        _frequencyBins = StftLayoutHelper.FrequencyBinCount(_stft.Size);
    }

    /// <summary>Gets FFT frequency bin count (F).</summary>
    public int FrequencyBins => _frequencyBins;

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<float>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
    {
        if (inputShape.Height != 1 || inputShape.Width != 1)
        {
            throw new ArgumentException("Expected waveform layout N×1×1×L.");
        }

        var frameCount = StftLayoutHelper.FrameCount(inputShape.Channels, _windowSize, _hopSize);
        return BatchShape.Image(inputShape.Batch, frameCount, _frequencyBins, 1);
    }

    /// <inheritdoc/>
    public override BatchTensor<float> Forward(BatchTensor<float> input, bool training = true)
    {
        FrontendTensorOps.RequireWaveformLayout(input);
        var outputShape = GetOutputShape(input.Shape);
        var output = new BatchTensor<float>(
            outputShape.Batch,
            outputShape.Height,
            outputShape.Width,
            outputShape.Channels);

        long workPer = (long)input.Channels * outputShape.Height * _frequencyBins;
        if (ComputingContextExecution.UseParallelIndexed(null, input.Batch, workPer))
        {
            ComputingContextExecution.ForEach(null, 0, input.Batch, n =>
                ProcessBatchItem(input, output, n, _parallelStft.Value!), workPer);
        }
        else
        {
            for (var n = 0; n < input.Batch; n++)
            {
                ProcessBatchItem(input, output, n, _stft);
            }
        }

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<float> Backward(BatchTensor<float> gradOutput)
    {
        EnsureCached();
        return FrontendTensorOps.ZeroGradLike(CachedInput!);
    }

    private void ProcessBatchItem(BatchTensor<float> input, BatchTensor<float> output, int batchIndex, Stft stft)
    {
        var waveform = new float[input.Channels];
        for (var i = 0; i < input.Channels; i++)
        {
            waveform[i] = input[batchIndex, 0, 0, i];
        }

        var frames = stft.Spectrogram(waveform, _normalizeSpectrum);
        CopyFrames(output, frames, batchIndex);
    }

    private void CopyFrames(BatchTensor<float> batch, IReadOnlyList<float[]> frames, int batchIndex)
    {
        if (frames.Count != batch.Height)
        {
            throw new InvalidOperationException("STFT frame count does not match output shape.");
        }

        for (var t = 0; t < frames.Count; t++)
        {
            var frame = frames[t];
            if (frame.Length != batch.Width)
            {
                throw new InvalidOperationException("STFT frequency bin count does not match output shape.");
            }

            for (var f = 0; f < frame.Length; f++)
            {
                batch[batchIndex, t, f, 0] = frame[f];
            }
        }
    }
}
