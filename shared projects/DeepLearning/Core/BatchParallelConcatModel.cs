namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;

/// <summary>
/// Runs two batch branches on the same input and concatenates their outputs along channels.
/// </summary>
public sealed class BatchParallelConcatModel<T>
    where T : unmanaged, IBinaryFloatingPointIeee754<T>
{
    private int _leftChannels;

    public BatchParallelConcatModel(BatchSequential<T> left, BatchSequential<T> right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public BatchSequential<T> Left { get; }
    public BatchSequential<T> Right { get; }

    public IReadOnlyList<Parameter<T>> Parameters
    {
        get
        {
            var list = new List<Parameter<T>>(Left.Parameters.Count + Right.Parameters.Count);
            list.AddRange(Left.Parameters);
            list.AddRange(Right.Parameters);
            return list;
        }
    }

    public void ZeroGradients()
    {
        Left.ZeroGradients();
        Right.ZeroGradients();
    }

    public BatchTensor<T> Forward(BatchTensor<T> input, bool training = true)
    {
        var leftOut = Left.Forward(input, training);
        _leftChannels = leftOut.Channels;
        var rightOut = Right.Forward(input, training);
        return BatchTensorUtilities.ConcatChannels(leftOut, rightOut);
    }

    public BatchTensor<T> Backward(BatchTensor<T> gradOutput)
    {
        int leftChannels = _leftChannels;
        int rightChannels = gradOutput.Channels - leftChannels;
        int plane = gradOutput.Height * gradOutput.Width;
        var gradLeft = new BatchTensor<T>(gradOutput.Batch, gradOutput.Height, gradOutput.Width, leftChannels);
        var gradRight = new BatchTensor<T>(gradOutput.Batch, gradOutput.Height, gradOutput.Width, rightChannels);

        for (int n = 0; n < gradOutput.Batch; n++)
        {
            int outOffset = n * plane * gradOutput.Channels;
            gradOutput.Values.Slice(outOffset, plane * leftChannels)
                .CopyTo(gradLeft.Values.Slice(n * plane * leftChannels, plane * leftChannels));
            gradOutput.Values.Slice(outOffset + plane * leftChannels, plane * rightChannels)
                .CopyTo(gradRight.Values.Slice(n * plane * rightChannels, plane * rightChannels));
        }

        var fromLeft = Left.Backward(gradLeft);
        var fromRight = Right.Backward(gradRight);
        var gradInput = new BatchTensor<T>(fromLeft.Batch, fromLeft.Height, fromLeft.Width, fromLeft.Channels);
        for (int i = 0; i < gradInput.Values.Length; i++)
        {
            gradInput.Values[i] = fromLeft.Values[i] + fromRight.Values[i];
        }

        return gradInput;
    }
}
