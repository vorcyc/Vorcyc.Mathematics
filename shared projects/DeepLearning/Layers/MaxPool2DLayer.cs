using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    // In the current neural network only pooling with stride = 2 and kernel = 2 is used so only it was implemented.

    /// <summary>
    /// 对输入张量执行二维最大池化操作。
    /// </summary>
    /// <typeparam name="T">张量元素的类型，必须实现 <see cref="IBinaryFloatingPointIeee754{TSelf}"/> 和 <see cref="IMinMaxValue{TSelf}"/> 接口。</typeparam>
    /// <param name="input">输入的张量。</param>
    /// <returns>执行最大池化操作后的张量。</returns>
    public static Tensor<T> MaxPool2D<T>(Tensor<T> input)
        where T : IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var outputWidth = input.Width / 2;
        var outputHeight = input.Height / 2;
        var result = new Tensor<T>(outputWidth, outputHeight, input.Depth);

        Parallel.For(0, input.Depth, (int d) =>
        {
            for (int ax = 0; ax < outputWidth; ax++)
            {
                var x = 0;
                x += 2 * ax;
                for (int ay = 0; ay < outputHeight; ay++)
                {
                    var y = 0;
                    y += 2 * ay;
                    //float a = float.MinValue;
                    T a = T.MinValue;

                    for (int fx = 0; fx < 2; fx++)
                    {
                        for (int fy = 0; fy < 2; fy++)
                        {
                            var oy = y + fy;
                            var ox = x + fx;
                            if (oy >= 0 && oy < input.Height && ox >= 0 && ox < input.Width)
                            {
                                var v = input[ox, oy, d];
                                if (v > a)
                                {
                                    a = v;
                                }
                            }
                        }
                    }

                    var n = ((outputWidth * ay) + ax) * input.Depth + d;
                    result[ax, ay, d] = a;
                }
            }
        });
        return result;
    }


    /// <summary>
    /// 对输入张量执行二维最大池化操作。
    /// </summary>
    /// <param name="input">输入的张量。</param>
    /// <returns>执行最大池化操作后的张量。</returns>
    public static Tensor MaxPool2D(Tensor input)
    {
        var outputWidth = input.Width / 2;
        var outputHeight = input.Height / 2;
        var result = new Tensor(outputWidth, outputHeight, input.Depth);

        Parallel.For(0, input.Depth, (int d) =>
        {
            for (int ax = 0; ax < outputWidth; ax++)
            {
                var x = 0;
                x += 2 * ax;
                for (int ay = 0; ay < outputHeight; ay++)
                {
                    var y = 0;
                    y += 2 * ay;
                    float a = float.MinValue;

                    for (int fx = 0; fx < 2; fx++)
                    {
                        for (int fy = 0; fy < 2; fy++)
                        {
                            var oy = y + fy;
                            var ox = x + fx;
                            if (oy >= 0 && oy < input.Height && ox >= 0 && ox < input.Width)
                            {
                                var v = input[ox, oy, d];
                                if (v > a)
                                {
                                    a = v;
                                }
                            }
                        }
                    }

                    var n = ((outputWidth * ay) + ax) * input.Depth + d;
                    result[ax, ay, d] = a;
                }
            }
        });
        return result;
    }

}