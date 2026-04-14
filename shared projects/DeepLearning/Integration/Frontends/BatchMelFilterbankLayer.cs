using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;

namespace Vorcyc.Mathematics.DeepLearning.Integration.Frontends;

/// <summary>
/// Fixed mel filterbank on power spectra (N×T×F×1 → N×T×M×1).
/// </summary>
public sealed class BatchMelFilterbankLayer : BatchLayerBase<float>
{
    private readonly float[][] _filterBank;
    private readonly int _inputBins;
    private readonly int _melBands;

    /// <summary>
    /// Creates a mel projection layer from a [M×F] filterbank matrix.
    /// </summary>
    public BatchMelFilterbankLayer(float[][] filterBank, string? name = null) : base(name)
    {
        ArgumentNullException.ThrowIfNull(filterBank);
        if (filterBank.Length == 0 || filterBank[0].Length == 0)
        {
            throw new ArgumentException("Filterbank must not be empty.", nameof(filterBank));
        }

        _filterBank = filterBank;
        _melBands = filterBank.Length;
        _inputBins = filterBank[0].Length;

        for (var m = 1; m < _melBands; m++)
        {
            if (filterBank[m].Length != _inputBins)
            {
                throw new ArgumentException("All filterbank rows must have the same length.", nameof(filterBank));
            }
        }
    }

    /// <summary>Gets mel band count (M).</summary>
    public int MelBands => _melBands;

    /// <summary>Gets input frequency bin count (F).</summary>
    public int InputBins => _inputBins;

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<float>> Parameters => [];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
    {
        if (inputShape.Width != _inputBins || inputShape.Channels != 1)
        {
            throw new ArgumentException($"Expected spectral layout N×T×{_inputBins}×1.");
        }

        return BatchShape.Image(inputShape.Batch, inputShape.Height, _melBands, 1);
    }

    /// <inheritdoc/>
    public override BatchTensor<float> Forward(BatchTensor<float> input, bool training = true)
    {
        FrontendTensorOps.RequireSpectralLayout(input, _inputBins);
        var output = new BatchTensor<float>(input.Batch, input.Height, _melBands, 1);

        long workPer = (long)input.Height * _melBands * _inputBins;
        ComputingContextExecution.ForEach(null, 0, input.Batch, n =>
        {
            for (var t = 0; t < input.Height; t++)
            {
                for (var m = 0; m < _melBands; m++)
                {
                    var sum = 0f;
                    var weights = _filterBank[m];
                    for (var f = 0; f < _inputBins; f++)
                    {
                        sum += weights[f] * input[n, t, f, 0];
                    }

                    output[n, t, m, 0] = sum;
                }
            }
        }, workPer);

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<float> Backward(BatchTensor<float> gradOutput)
    {
        EnsureCached();
        var input = CachedInput!;
        var gradInput = new BatchTensor<float>(input.Batch, input.Height, input.Width, input.Channels);

        for (var n = 0; n < input.Batch; n++)
        {
            for (var t = 0; t < input.Height; t++)
            {
                for (var f = 0; f < _inputBins; f++)
                {
                    var sum = 0f;
                    for (var m = 0; m < _melBands; m++)
                    {
                        sum += _filterBank[m][f] * gradOutput[n, t, m, 0];
                    }

                    gradInput[n, t, f, 0] = sum;
                }
            }
        }

        return gradInput;
    }
}
