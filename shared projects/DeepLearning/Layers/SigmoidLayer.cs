using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    /// <summary>
    /// 对输入张量执行 Sigmoid 激活函数操作。
    /// </summary>
    /// <typeparam name="T">张量元素的类型，必须实现 <see cref="IBinaryFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="input">输入的张量。</param>
    /// <returns>执行 Sigmoid 操作后的张量。</returns>
    public static Tensor<T> Sigmoid<T>(Tensor<T> input)
        where T : IBinaryFloatingPointIeee754<T>
    {

        var height = input.Height;
        var width = input.Width;
        var result = new Tensor<T>(input.Width, input.Height, input.Depth);

        Parallel.For(0, input.Depth, (int d) =>
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var v = input[x, y, d];
                    //Result[x, y, d] = 1f / (1f + (float)Math.Exp(-v));
                    result[x, y, d] = T.One / (T.One + T.Exp(-v));
                }
            }
        });

        return result;
    }


    /// <summary>
    /// 对输入张量执行 Sigmoid 激活函数操作。
    /// </summary>
    /// <param name="input">输入的张量。</param>
    /// <returns>执行 Sigmoid 操作后的张量。</returns>
    public static Tensor Sigmoid(Tensor input)
    {

        var height = input.Height;
        var width = input.Width;
        var result = new Tensor(input.Width, input.Height, input.Depth);

        Parallel.For(0, input.Depth, (int d) =>
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var v = input[x, y, d];
                    result[x, y, d] = 1f / (1f + MathF.Exp(-v));
                }
            }
        });

        return result;
    }




}