using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    /// <summary>
    /// 合并两个张量。
    /// </summary>
    /// <typeparam name="T">张量元素的类型，必须实现 <see cref="IBinaryFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="input">输入的张量。</param>
    /// <param name="joint">要合并的张量。</param>
    /// <returns>合并后的张量。</returns>
    public static Tensor<T> Fusion<T>(Tensor<T> input, Tensor<T> joint)
        where T : IBinaryFloatingPointIeee754<T>
    {
        var height = input.Height;
        var width = input.Width;
        var result = new Tensor<T>(input.Width, input.Height, input.Depth + joint.Depth);

        Parallel.For(0, input.Depth, (int d) =>
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

        Parallel.For(0, joint.Depth, (int d) =>
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
    /// 合并两个张量。
    /// </summary>
    /// <param name="input">输入的张量。</param>
    /// <param name="joint">要合并的张量。</param>
    /// <returns>合并后的张量。</returns>
    public static Tensor Fusion(Tensor input, Tensor joint)
    {
        var height = input.Height;
        var width = input.Width;
        var result = new Tensor(input.Width, input.Height, input.Depth + joint.Depth);

        Parallel.For(0, input.Depth, (int d) =>
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

        Parallel.For(0, joint.Depth, (int d) =>
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