using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    /// <summary>
    /// 对输入张量执行 ReLU 激活函数操作。
    /// </summary>
    /// <typeparam name="T">张量元素的类型，必须实现 <see cref="IBinaryFloatingPointIeee754{TSelf}"/>接口。</typeparam>
    /// <param name="input">输入的张量。</param>
    /// <returns>执行 ReLU 操作后的张量。</returns>
    public static Tensor<T> ReLU<T>(Tensor<T> input)
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
                    //Result[x, y, d] = (v > 0.0f) ? (v) : 0.0f;
                    result[x, y, d] = (v > T.Zero) ? v : T.Zero;
                }
            }
        });

        return result;
    }


    /// <summary>
    /// 对输入张量执行 ReLU 激活函数操作。
    /// </summary>
    /// <param name="input">输入的张量。</param>
    /// <returns>执行 ReLU 操作后的张量。</returns>
    public static Tensor ReLU(Tensor input)
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
                    result[x, y, d] = (v > 0f) ? v : 0f;
                }
            }
        });

        return result;
    }

}