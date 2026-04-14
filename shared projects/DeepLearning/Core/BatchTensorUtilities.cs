namespace Vorcyc.Mathematics.DeepLearning;

using System.Numerics;

/// <summary>
/// Utility operations on <see cref="BatchTensor{T}"/> values.
/// </summary>
public static class BatchTensorUtilities
{
    /// <summary>
    /// Concatenates two NHWC tensors along the channel axis.
    /// </summary>
    public static BatchTensor<T> ConcatChannels<T>(BatchTensor<T> left, BatchTensor<T> right)
        where T : unmanaged, IBinaryFloatingPointIeee754<T>
    {
        if (left.Batch != right.Batch || left.Height != right.Height || left.Width != right.Width)
        {
            throw new ArgumentException("Tensors must share batch, height, and width for channel concat.");
        }

        var output = new BatchTensor<T>(left.Batch, left.Height, left.Width, left.Channels + right.Channels);
        int leftPlane = left.Height * left.Width * left.Channels;
        int rightPlane = right.Height * right.Width * right.Channels;

        for (int n = 0; n < left.Batch; n++)
        {
            int leftOffset = n * leftPlane;
            int rightOffset = n * rightPlane;
            int outOffset = n * left.Height * left.Width * output.Channels;
            left.Values.Slice(leftOffset, leftPlane).CopyTo(output.Values.Slice(outOffset, leftPlane));
            right.Values.Slice(rightOffset, rightPlane).CopyTo(output.Values.Slice(outOffset + leftPlane, rightPlane));
        }

        return output;
    }
}
