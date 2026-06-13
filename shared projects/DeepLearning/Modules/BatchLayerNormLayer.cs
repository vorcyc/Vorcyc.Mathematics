namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Layer normalization over the channel dimension at each spatial position.
/// </summary>
public sealed class BatchLayerNormLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Parameter<T> _scale;
    private readonly Parameter<T> _shift;
    private BatchTensor<T>? _normalizedInput;
    private T[]? _mean;
    private T[]? _variance;

    public BatchLayerNormLayer(int channels, string? name = null)
        : base(name)
    {
        Channels = channels;
        _scale = new Parameter<T>(new Tensor<T>(1, 1, channels), $"{name}.scale");
        _shift = new Parameter<T>(new Tensor<T>(1, 1, channels), $"{name}.shift");
        _scale.Value.Fill(T.One);
        _shift.Value.Fill(T.Zero);
    }

    public int Channels { get; }
    public T Epsilon { get; set; } = T.CreateChecked(1e-5);

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [_scale, _shift];

    /// <inheritdoc/>
    public override BatchShape GetOutputShape(BatchShape inputShape)
    {
        if (inputShape.Channels != Channels)
        {
            throw new ArgumentException($"Expected {Channels} channels, got {inputShape.Channels}.");
        }

        return inputShape;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        int spatial = input.Batch * input.Height * input.Width;
        var output = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        _normalizedInput = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        _mean = new T[spatial];
        _variance = new T[spatial];
        int channels = Channels;
        var epsilon = Epsilon;

        // Each spatial position s owns a contiguous channel slice [s*C, s*C+C) in the
        // NHWC buffer and its own _mean[s]/_variance[s] — fully independent across s.
        ComputingContextExecution.ForEach(null, 0, spatial, s =>
        {
            int outBase = s * channels;
            var slice = input.Values.Slice(outBase, channels);

            BatchNormMath.ComputeMeanAndVariance(slice, out var mean, out var variance);
            _mean[s] = mean;
            _variance[s] = variance;
            var invStd = T.One / T.Sqrt(variance + epsilon);

            for (int c = 0; c < channels; c++)
            {
                var norm = (slice[c] - mean) * invStd;
                _normalizedInput.Values[outBase + c] = norm;
                output.Values[outBase + c] = norm * _scale.Value[0, 0, c] + _shift.Value[0, 0, c];
            }
        }, channels);

        CacheForward(input, output);
        return output;
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        if (_normalizedInput is null || _mean is null || _variance is null)
        {
            throw new InvalidOperationException("Forward must be called before Backward.");
        }

        int spatial = gradOutput.Batch * gradOutput.Height * gradOutput.Width;
        var gradInput = new BatchTensor<T>(gradOutput.Batch, gradOutput.Height, gradOutput.Width, gradOutput.Channels);
        int channels = Channels;
        var countT = T.CreateTruncating(Channels);
        var epsilon = Epsilon;

        // Kernel 1: gradInput — each spatial position s reads/writes only its own channel
        // slice and its own _variance[s]/_normalizedInput slice, so it is race-free over s.
        ComputingContextExecution.ForEach(null, 0, spatial, s =>
        {
            int baseIndex = s * channels;
            var variance = _variance[s];
            var invStd = T.One / T.Sqrt(variance + epsilon);

            T gradNormSum = T.Zero;
            T gradNormDot = T.Zero;
            for (int c = 0; c < channels; c++)
            {
                var scale = _scale.Value[0, 0, c];
                var gradNorm = gradOutput.Values[baseIndex + c] * scale;
                var norm = _normalizedInput.Values[baseIndex + c];
                gradNormSum += gradNorm;
                gradNormDot += gradNorm * norm;
            }

            for (int c = 0; c < channels; c++)
            {
                var scale = _scale.Value[0, 0, c];
                var gradNorm = gradOutput.Values[baseIndex + c] * scale;
                var norm = _normalizedInput.Values[baseIndex + c];
                gradInput.Values[baseIndex + c] = invStd / countT * (countT * gradNorm - gradNormSum - norm * gradNormDot);
            }
        }, channels * 3);

        // Kernel 2: scale/shift grads — channel c accumulates across all spatial positions
        // into its own _scale.Gradient[c]/_shift.Gradient[c], disjoint per c.
        ComputingContextExecution.ForEach(null, 0, channels, c =>
        {
            T shiftAcc = T.Zero;
            T scaleAcc = T.Zero;
            for (int s = 0; s < spatial; s++)
            {
                int idx = s * channels + c;
                var gradOut = gradOutput.Values[idx];
                shiftAcc += gradOut;
                scaleAcc += gradOut * _normalizedInput.Values[idx];
            }

            _shift.Gradient[0, 0, c] += shiftAcc;
            _scale.Gradient[0, 0, c] += scaleAcc;
        }, spatial);

        return gradInput;
    }
}
