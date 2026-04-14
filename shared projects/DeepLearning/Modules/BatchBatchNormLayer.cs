namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Batch normalization for <see cref="BatchTensor{T}"/> in NHWC layout with SIMD statistics.
/// </summary>
public sealed class BatchBatchNormLayer<T> : BatchLayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Parameter<T> _scale;
    private readonly Parameter<T> _shift;
    private BatchTensor<T>? _normalizedInput;
    private T[]? _batchMean;
    private T[]? _batchVariance;

    public BatchBatchNormLayer(int channels, string? name = null)
        : base(name)
    {
        Channels = channels;
        _scale = new Parameter<T>(new Tensor<T>(1, 1, channels), $"{name}.scale");
        _shift = new Parameter<T>(new Tensor<T>(1, 1, channels), $"{name}.shift");
        _scale.Value.Fill(T.One);
        _shift.Value.Fill(T.Zero);
        RunningMean = new Tensor<T>(1, 1, channels);
        RunningVariance = new Tensor<T>(1, 1, channels);
        RunningMean.Fill(T.Zero);
        RunningVariance.Fill(T.One);
    }

    public int Channels { get; }
    public T Momentum { get; set; } = T.CreateChecked(0.1);
    public T Epsilon { get; set; } = T.CreateChecked(1e-5);
    public Tensor<T> RunningMean { get; }
    public Tensor<T> RunningVariance { get; }

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
        if (input.Channels != Channels)
        {
            throw new ArgumentException($"Expected {Channels} channels, got {input.Channels}.");
        }

        int spatialCount = input.Batch * input.Height * input.Width;
        var output = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        _normalizedInput = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        _batchMean = new T[Channels];
        _batchVariance = new T[Channels];

        if (ComputingContextExecution.UseParallelIndexed(null, Channels, spatialCount))
        {
            ComputingContextExecution.ForEach(null, 0, Channels, c =>
                ForwardChannel(input, output, c, spatialCount, training), spatialCount);
        }
        else
        {
            var workspace = new T[spatialCount];
            var normalizedWorkspace = new T[spatialCount];
            for (int c = 0; c < Channels; c++)
            {
                ForwardChannel(input, output, c, spatialCount, training, workspace, normalizedWorkspace);
            }
        }

        CacheForward(input, output);
        return output;
    }

    private void ForwardChannel(
        BatchTensor<T> input,
        BatchTensor<T> output,
        int channel,
        int spatialCount,
        bool training)
    {
        var workspace = new T[spatialCount];
        var normalizedWorkspace = new T[spatialCount];
        ForwardChannel(input, output, channel, spatialCount, training, workspace, normalizedWorkspace);
    }

    private void ForwardChannel(
        BatchTensor<T> input,
        BatchTensor<T> output,
        int channel,
        int spatialCount,
        bool training,
        T[] workspace,
        T[] normalizedWorkspace)
    {
        input.CopyChannelTo(channel, workspace);
        T mean;
        T variance;
        if (training)
        {
            BatchNormMath.ComputeMeanAndVariance(workspace, out mean, out variance);
            _batchMean![channel] = mean;
            _batchVariance![channel] = variance;
            RunningMean[0, 0, channel] = Momentum * RunningMean[0, 0, channel] + (T.One - Momentum) * mean;
            RunningVariance[0, 0, channel] = Momentum * RunningVariance[0, 0, channel] + (T.One - Momentum) * variance;
        }
        else
        {
            mean = RunningMean[0, 0, channel];
            variance = RunningVariance[0, 0, channel];
        }

        var invStd = T.One / T.Sqrt(variance + Epsilon);
        var scale = _scale.Value[0, 0, channel];
        var shift = _shift.Value[0, 0, channel];
        var outputChannel = new T[spatialCount];
        BatchNormMath.NormalizeScaleShift(workspace, normalizedWorkspace, outputChannel, mean, invStd, scale, shift);
        output.CopyChannelFrom(channel, outputChannel);
        _normalizedInput!.CopyChannelFrom(channel, normalizedWorkspace);
    }

    /// <inheritdoc/>
    public override BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        EnsureCached();
        if (_normalizedInput is null || _batchMean is null || _batchVariance is null)
        {
            throw new InvalidOperationException("Training forward pass required before backward.");
        }

        var input = CachedInput!;
        int spatialCount = input.Batch * input.Height * input.Width;
        var gradChannel = new T[spatialCount];
        var normChannel = new T[spatialCount];
        var gradInput = new BatchTensor<T>(input.Batch, input.Height, input.Width, input.Channels);
        var countT = T.CreateTruncating(spatialCount);

        for (int c = 0; c < Channels; c++)
        {
            gradOutput.CopyChannelTo(c, gradChannel);
            _normalizedInput.CopyChannelTo(c, normChannel);
            var variance = _batchVariance[c];
            var invStd = T.One / T.Sqrt(variance + Epsilon);
            var scale = _scale.Value[0, 0, c];

            T gradShift = BatchNormMath.SumSimd(gradChannel);
            _shift.Gradient[0, 0, c] += gradShift;

            T gradScale = T.Zero;
            BatchNormMath.AccumulateDotSimd(gradChannel, normChannel, ref gradScale);
            _scale.Gradient[0, 0, c] += gradScale;

            T gradNormSum = T.Zero;
            T gradNormDot = T.Zero;
            for (int i = 0; i < spatialCount; i++)
            {
                var gradNorm = gradChannel[i] * scale;
                gradNormSum += gradNorm;
                gradNormDot += gradNorm * normChannel[i];
            }

            var gradInChannel = new T[spatialCount];
            for (int i = 0; i < spatialCount; i++)
            {
                var gradNorm = gradChannel[i] * scale;
                gradInChannel[i] = invStd / countT * (countT * gradNorm - gradNormSum - normChannel[i] * gradNormDot);
            }

            gradInput.CopyChannelFrom(c, gradInChannel);
        }

        return gradInput;
    }
}
