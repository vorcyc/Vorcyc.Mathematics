using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    /// <summary>
    /// Fuses two tensors.
    /// </summary>
    /// <typeparam name="T">The tensor element type, must implement <see cref="IBinaryFloatingPointIeee754{TSelf}"/>.</typeparam>
    /// <param name="input">The input tensor.</param>
    /// <param name="joint">The tensor to fuse.</param>
    /// <returns>The fused tensor.</returns>
    public static Tensor<T> Fusion<T>(Tensor<T> input, Tensor<T> joint)
        where T : IBinaryFloatingPointIeee754<T>
    {
        var height = input.Height;
        var width = input.Width;
        var result = new Tensor<T>(input.Width, input.Height, input.Depth + joint.Depth);

        long workPer = (long)height * width;
        ForEachDepth(input.Depth, workPer, d =>
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var v = input[x, y, d];
                    result[x, y, d] = v;
                }
            }
        });

        ForEachDepth(joint.Depth, workPer, d =>
        {
            var v = joint[0, 0, d];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[x, y, d + input.Depth] = v;
                }
            }
        });


        return result;
    }


    /// <summary>
    /// Fuses two tensors.
    /// </summary>
    /// <param name="input">The input tensor.</param>
    /// <param name="joint">The tensor to fuse.</param>
    /// <returns>The fused tensor.</returns>
    public static TensorFp32 Fusion(TensorFp32 input, TensorFp32 joint)
    {
        var height = input.Height;
        var width = input.Width;
        var result = new TensorFp32(input.Width, input.Height, input.Depth + joint.Depth);

        long workPer = (long)height * width;
        ForEachDepth(input.Depth, workPer, d =>
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var v = input[x, y, d];
                    result[x, y, d] = v;
                }
            }
        });

        ForEachDepth(joint.Depth, workPer, d =>
        {
            var v = joint[0, 0, d];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[x, y, d + input.Depth] = v;
                }
            }
        });


        return result;
    }

}