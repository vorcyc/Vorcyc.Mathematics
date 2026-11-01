namespace Vorcyc.Mathematics.SignalProcessing.Filters.Adaptive;

/// <summary>
/// Represents the Block LMS Adaptive filter: the gradient e · x is accumulated over a block of samples and the weights
/// are updated once per block with the average gradient, w += mu · (1 / blockSize) · sum(e · x).
/// The weights stay constant inside a block. For a frequency-domain implementation of long filters see
/// <see cref="FrequencyDomainAdaptiveFilter"/>.
/// </summary>
public class BlockLmsFilter : AdaptiveFilter
{
    private readonly int _blockSize;
    private readonly float _mu;
    private readonly float _leakage;
    private readonly float[] _gradient;
    private int _count;

    /// <summary>
    /// Constructs <see cref="BlockLmsFilter"/> of given <paramref name="order"/>.
    /// </summary>
    /// <param name="order">Filter order</param>
    /// <param name="blockSize">Number of samples per weight update; 1 gives <see cref="LmsFilter"/></param>
    /// <param name="mu">Step size applied to the average gradient of a block; the stability rules of <see cref="LmsFilter"/> apply, whatever the block size is</param>
    /// <param name="leakage">Leakage, not negative; <paramref name="leakage"/> · <paramref name="mu"/> must not exceed 1</param>
    public BlockLmsFilter(int order, int blockSize = 16, float mu = 0.005f, float leakage = 0) : base(order)
    {
        Guard.AgainstNonPositive(blockSize, nameof(blockSize));
        AdaptiveArgument.Positive(mu, nameof(mu));
        AdaptiveArgument.Leakage(leakage, mu);

        _blockSize = blockSize;
        _mu = mu;
        _leakage = leakage;
        _gradient = new float[_kernelSize];
    }

    /// <summary>
    /// Gets the number of samples per weight update.
    /// </summary>
    public int BlockSize => _blockSize;

    /// <summary>
    /// Drops the gradient accumulated in the current block.
    /// </summary>
    protected override void ResetState()
    {
        Array.Clear(_gradient);
        _count = 0;
    }

    /// <summary>
    /// Processes one sample of input and desired signals; the coefficients are adapted at the end of every block.
    /// </summary>
    /// <param name="input">Sample of input signal</param>
    /// <param name="desired">Sample of desired signal</param>
    public override float Process(float input, float desired)
    {
        var y = Push(input, out var offset);

        var e = desired - y;

        // a sample with a non-finite error would stay in the gradient of the block: it is dropped and does not count for the block
        if (!float.IsFinite(e))
        {
            return y;
        }

        var x = Regressor(offset);
        for (var i = 0; i < x.Length; i++)
        {
            _gradient[i] += e * x[i];
        }

        if (++_count == _blockSize)
        {
            var decay = 1 - _leakage * _mu;
            var step = _mu / _blockSize;

            for (var i = 0; i < x.Length; i++)
            {
                SetWeight(i, decay * _b[i] + step * _gradient[i]);
            }

            Array.Clear(_gradient);
            _count = 0;
        }

        return y;
    }
}
