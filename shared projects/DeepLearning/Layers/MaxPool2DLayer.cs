using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

public static partial class Layers
{

    // In the current neural network only pooling with stride = 2 and kernel = 2 is used so only it was implemented.

    /// <summary>
    /// Performs a 2D max pooling operation on the input tensor.
    /// </summary>
    /// <typeparam name="T">The tensor element type, must implement <see cref="IBinaryFloatingPointIeee754{TSelf}"/> and <see cref="IMinMaxValue{TSelf}"/>.</typeparam>
    /// <param name="input">The input tensor.</param>
    /// <returns>The resulting tensor after the max pooling operation.</returns>
    public static Tensor<T> MaxPool2D<T>(Tensor<T> input)
        where T : IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var outputWidth = input.Width / 2;
        var outputHeight = input.Height / 2;
        var result = new Tensor<T>(outputWidth, outputHeight, input.Depth);

        long workPer = (long)outputWidth * outputHeight * 4;
        ForEachDepth(input.Depth, workPer, d =>
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
    /// Performs a 2D max pooling operation on the input tensor.
    /// </summary>
    /// <param name="input">The input tensor.</param>
    /// <returns>The resulting tensor after the max pooling operation.</returns>
    public static TensorFp32 MaxPool2D(TensorFp32 input)
    {
        var outputWidth = input.Width / 2;
        var outputHeight = input.Height / 2;
        var result = new TensorFp32(outputWidth, outputHeight, input.Depth);

        long workPer = (long)outputWidth * outputHeight * 4;
        ForEachDepth(input.Depth, workPer, d =>
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