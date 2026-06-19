namespace Vorcyc.Mathematics.DeepLearning.Modules;

using System.Numerics;
using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Batch normalization with running statistics for inference.
/// </summary>
/// <remarks>
/// Uses SIMD-accelerated per-channel statistics via <see cref="BatchNormMath"/>.
/// </remarks>
/// <typeparam name="T">Element type.</typeparam>
public sealed class BatchNormLayer<T> : LayerBase<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private readonly Parameter<T> _scale;
    private readonly Parameter<T> _shift;
    private T[]? _normalizedChannel;
    private T[]? _batchMean;
    private T[]? _batchVariance;

    public BatchNormLayer(int channels, string? name = null)
        : base(name)
    {
        if (channels <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channels));
        }

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
    public Parameter<T> Scale => _scale;
    public Parameter<T> Shift => _shift;

    /// <inheritdoc/>
    public override IReadOnlyList<Parameter<T>> Parameters => [_scale, _shift];

    /// <inheritdoc/>
    public override TensorShape GetOutputShape(TensorShape inputShape)
    {
        if (inputShape.Depth != Channels)
        {
            throw new ArgumentException($"Expected depth {Channels}, got {inputShape.Depth}.", nameof(inputShape));
        }

        return inputShape;
    }

    /// <inheritdoc/>
    public override Tensor<T> Forward(Tensor<T> input, bool training = true)
    {
        if (input.Depth != Channels)
        {
            throw new ArgumentException($"Expected depth {Channels}, got {input.Depth}.", nameof(input));
        }

        int spatialCount = input.Width * input.Height;
        var output = new Tensor<T>(input.Width, input.Height, input.Depth);
        _normalizedChannel = new T[spatialCount * Channels];
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
        Tensor<T> input,
        Tensor<T> output,
        int channel,
        int spatialCount,
        bool training)
    {
        var workspace = new T[spatialCount];
        var normalizedWorkspace = new T[spatialCount];
        ForwardChannel(input, output, channel, spatialCount, training, workspace, normalizedWorkspace);
    }
    private void ForwardChannel(
        Tensor<T> input,
        Tensor<T> output,
        int channel,
        int spatialCount,
        bool training,
        T[] workspace,
        T[] normalizedWorkspace)
    {
        ExtractChannel(input, channel, workspace);
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
        WriteChannel(output, channel, outputChannel);
        Array.Copy(normalizedWorkspace, 0, _normalizedChannel!, channel * spatialCount, spatialCount);
    }

    /// <inheritdoc/>
    public override Tensor<T> Backward(Tensor<T> gradOutput)
    {
        EnsureCached();
        if (_normalizedChannel is null || _batchMean is null || _batchVariance is null)
        {
            throw new InvalidOperationException("Training forward pass required before backward.");
        }

        var input = CachedInput!;
        int spatialCount = input.Width * input.Height;
        var countT = T.CreateTruncating(spatialCount);
        var gradInput = new Tensor<T>(input.Width, input.Height, input.Depth);
        var gradChannel = new T[spatialCount];
        var normChannel = new T[spatialCount];

        for (int c = 0; c < Channels; c++)
        {
            ExtractChannel(gradOutput, c, gradChannel);
            Array.Copy(_normalizedChannel, c * spatialCount, normChannel, 0, spatialCount);
            var variance = _batchVariance[c];
            var invStd = T.One / T.Sqrt(variance + Epsilon);
            var scale = _scale.Value[0, 0, c];

            _shift.Gradient[0, 0, c] += BatchNormMath.SumSimd(gradChannel);

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

            WriteChannel(gradInput, c, gradInChannel);
        }

        return gradInput;
    }

    private static void ExtractChannel(Tensor<T> tensor, int channel, Span<T> destination)
    {
        int index = 0;
        for (int y = 0; y < tensor.Height; y++)
        {
            for (int x = 0; x < tensor.Width; x++)
            {
                destination[index++] = tensor[x, y, channel];
            }
        }
    }

    private static void WriteChannel(Tensor<T> tensor, int channel, ReadOnlySpan<T> source)
    {
        int index = 0;
        for (int y = 0; y < tensor.Height; y++)
        {
            for (int x = 0; x < tensor.Width; x++)
            {
                tensor[x, y, channel] = source[index++];
            }
        }
    }
}
