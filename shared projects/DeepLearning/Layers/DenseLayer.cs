using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    /// <summary>
    /// Performs a dense (fully connected) layer operation on the input tensor.
    /// </summary>
    /// <typeparam name="T">The tensor element type, must implement <see cref="IBinaryFloatingPointIeee754{TSelf}"/>.</typeparam>
    /// <param name="input">The input tensor.</param>
    /// <param name="weights">The array of weight tensors.</param>
    /// <param name="biases">The bias tensor.</param>
    /// <returns>The resulting tensor after the dense operation.</returns>
    public static Tensor<T> Dense<T>(Tensor<T> input, Tensor<T>[] weights, Tensor<T> biases)
        where T : IBinaryFloatingPointIeee754<T>
    {

        var height = input.Height;
        var width = input.Width;
        var result = new Tensor<T>(1, 1, weights.Length);

        long workPer = (long)height * width * input.Depth;
        ForEachDepth(weights.Length, workPer, d =>
        {
            var f = weights[d];
            var a = T.Zero;
            var i = 0;

            for (int id = 0; id < input.Depth; id++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        a += input[x, y, id] * f.Values[i];
                        i += 1;
                    }
                }
            }
            result.Values[d] = a + biases.Values[d];
        });

        return result;
    }


    /// <summary>
    /// Performs a dense (fully connected) layer operation on the input tensor.
    /// </summary>
    /// <param name="input">The input tensor.</param>
    /// <param name="weights">The array of weight tensors.</param>
    /// <param name="biases">The bias tensor.</param>
    /// <returns>The resulting tensor after the dense operation.</returns>
    public static TensorFp32 Dense(TensorFp32 input, TensorFp32[] weights, TensorFp32 biases)
    {

        var height = input.Height;
        var width = input.Width;
        var result = new TensorFp32(1, 1, weights.Length);

        long workPer = (long)height * width * input.Depth;
        ForEachDepth(weights.Length, workPer, d =>
        {
            var f = weights[d];
            var a = 0f;
            var i = 0;

            for (int id = 0; id < input.Depth; id++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        a += input[x, y, id] * f.Values[i];
                        i += 1;
                    }
                }
            }
            result.Values[d] = a + biases.Values[d];
        });

        return result;
    }

}