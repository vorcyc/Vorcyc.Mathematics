using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    /// <summary>
    /// 对输入张量执行密集（全连接）层操作。
    /// </summary>
    /// <typeparam name="T">张量元素的类型，必须实现 <see cref="IBinaryFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="input">输入的张量。</param>
    /// <param name="weights">权重张量的数组。</param>
    /// <param name="biases">偏置张量。</param>
    /// <returns>执行密集操作后的张量。</returns>
    public static Tensor<T> Dense<T>(Tensor<T> input, Tensor<T>[] weights, Tensor<T> biases)
        where T : IBinaryFloatingPointIeee754<T>
    {

        var height = input.Height;
        var width = input.Width;
        var result = new Tensor<T>(1, 1, weights.Length);

        Parallel.For(0, weights.Length, (int d) =>
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
    /// 对输入张量执行密集（全连接）层操作。
    /// </summary>
    /// <param name="input">输入的张量。</param>
    /// <param name="weights">权重张量的数组。</param>
    /// <param name="biases">偏置张量。</param>
    /// <returns>执行密集操作后的张量。</returns>
    public static Tensor Dense(Tensor input, Tensor[] weights, Tensor biases)
    {

        var height = input.Height;
        var width = input.Width;
        var result = new Tensor(1, 1, weights.Length);

        Parallel.For(0, weights.Length, (int d) =>
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