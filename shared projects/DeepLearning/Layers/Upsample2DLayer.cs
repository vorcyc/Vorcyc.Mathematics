using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    // In the current neural network only 2x upsampling is used so only it was implemented

    /// <summary>
    /// 对输入张量执行二维上采样操作。
    /// </summary>
    /// <typeparam name="T">张量元素的类型，必须实现 <see cref="IBinaryFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="input">输入的张量。</param>
    /// <returns>上采样后的张量。</returns>
    public static Tensor<T> Upsample2D<T>(Tensor<T> input)
        where T : IBinaryFloatingPointIeee754<T>
    {
        var result = new Tensor<T>(input.Width * 2, input.Height * 2, input.Depth);

        for (int d = 0; d < input.Depth; d++)
        {
            for (var ax = 0; ax < input.Width; ax++)
            {
                var x = 2 * ax;
                for (var ay = 0; ay < input.Height; ay++)
                {
                    var y = 2 * ay;
                    var a = input[ax, ay, d];

                    for (byte fx = 0; fx < 2; fx++)
                    {
                        for (byte fy = 0; fy < 2; fy++)
                        {
                            var oy = y + fy;
                            var ox = x + fx;
                            if (oy >= 0 && oy < result.Height && ox >= 0 && ox < result.Width)
                            {
                                result[ox, oy, d] = a;
                            }
                        }
                    }
                }
            }
        }
        return result;
    }


    /// <summary>
    /// 对输入张量执行二维上采样操作。
    /// </summary>
    /// <param name="input">输入的张量。</param>
    /// <returns>上采样后的张量。</returns>
    public static Tensor Upsample2D(Tensor input)
    {
        var result = new Tensor(input.Width * 2, input.Height * 2, input.Depth);

        for (int d = 0; d < input.Depth; d++)
        {
            for (var ax = 0; ax < input.Width; ax++)
            {
                var x = 2 * ax;
                for (var ay = 0; ay < input.Height; ay++)
                {
                    var y = 2 * ay;
                    var a = input[ax, ay, d];

                    for (byte fx = 0; fx < 2; fx++)
                    {
                        for (byte fy = 0; fy < 2; fy++)
                        {
                            var oy = y + fy;
                            var ox = x + fx;
                            if (oy >= 0 && oy < result.Height && ox >= 0 && ox < result.Width)
                            {
                                result[ox, oy, d] = a;
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

}