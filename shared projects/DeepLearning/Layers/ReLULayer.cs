using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    /// <summary>
    /// Performs a ReLU activation function operation on the input tensor.
    /// </summary>
    /// <typeparam name="T">The tensor element type, must implement <see cref="IBinaryFloatingPointIeee754{TSelf}"/>.</typeparam>
    /// <param name="input">The input tensor.</param>
    /// <returns>The resulting tensor after the ReLU operation.</returns>
    public static Tensor<T> ReLU<T>(Tensor<T> input)
        where T : IBinaryFloatingPointIeee754<T>
    {

        var height = input.Height;
        var width = input.Width;
        var result = new Tensor<T>(input.Width, input.Height, input.Depth);

        long workPer = (long)height * width;
        ForEachDepth(input.Depth, workPer, d =>
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var v = input[x, y, d];
                    result[x, y, d] = (v > T.Zero) ? v : T.Zero;
                }
            }
        });

        return result;
    }


    /// <summary>
    /// Performs a ReLU activation function operation on the input tensor.
    /// </summary>
    /// <param name="input">The input tensor.</param>
    /// <returns>The resulting tensor after the ReLU operation.</returns>
    public static TensorFp32 ReLU(TensorFp32 input)
    {
        var height = input.Height;
        var width = input.Width;
        var result = new TensorFp32(input.Width, input.Height, input.Depth);

        long workPer = (long)height * width;
        ForEachDepth(input.Depth, workPer, d =>
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var v = input[x, y, d];
                    result[x, y, d] = (v > 0f) ? v : 0f;
                }
            }
        });

        return result;
    }

}