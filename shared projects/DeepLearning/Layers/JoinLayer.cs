using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    /// <summary>
    /// Joins two tensor layers.
    /// </summary>
    /// <typeparam name="T">The tensor element type, must implement <see cref="IBinaryFloatingPointIeee754{TSelf}"/>.</typeparam>
    /// <param name="input">The input tensor.</param>
    /// <param name="joint">The tensor to join.</param>
    /// <returns>The joined tensor.</returns>
    public static Tensor<T> JoinLayer<T>(Tensor<T> input, Tensor<T> joint)
          where T : IBinaryFloatingPointIeee754<T>
    {
        int height = input.Height;
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
    /// Joins two tensor layers.
    /// </summary>
    /// <param name="input">The input tensor.</param>
    /// <param name="joint">The tensor to join.</param>
    /// <returns>The joined tensor.</returns>
    public static TensorFp32 JoinLayer(TensorFp32 input, TensorFp32 joint)
    {
        int height = input.Height;
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