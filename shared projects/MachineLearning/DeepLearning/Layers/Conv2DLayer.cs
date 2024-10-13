using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.DeepLearning.Layers;

public static partial class Layers
{

    /// <summary>
    /// 对 2D 张量执行卷积操作。
    /// </summary>
    /// <typeparam name="T">张量元素的类型，必须实现  <see cref="IBinaryFloatingPointIeee754{TSelf}"/> 接口。</typeparam>
    /// <param name="input">输入的 2D 张量。</param>
    /// <param name="filters">卷积核的数组。</param>
    /// <param name="biases">偏置张量。</param>
    /// <param name="stride">卷积步长，默认为 1。</param>
    /// <param name="dilation">卷积扩张率，默认为 1。</param>
    /// <returns>卷积结果的张量。</returns>
    public static Tensor<T> Conv2D<T>(Tensor<T> input, Tensor<T>[] filters, Tensor<T> biases, int stride = 1, int dilation = 1)
        where T : IBinaryFloatingPointIeee754<T>
    {

        var outputDepth = filters.Length;
        var result = new Tensor<T>(input.Width / stride, input.Height / stride, outputDepth);

        Parallel.For(0, outputDepth, (int d) =>
        {
            var f = filters[d];
            for (int ay = 0; ay < result.Height; ay++)
            {
                var y = ay * stride - (f.Height * dilation + dilation - 1) / 2;
                for (int ax = 0; ax < result.Width; ax++)
                {
                    var x = ax * stride - (f.Width * dilation + dilation - 1) / 2;
                    //var a = default(T);
                    var a = T.Zero;

                    for (int fy = 0; fy < f.Height; fy++)
                    {
                        var oy = y + fy * dilation + dilation - 1;
                        for (int fx = 0; fx < f.Width; fx++)
                        {
                            var ox = x + fx * dilation + dilation - 1;
                            if ((oy >= 0) && (oy < input.Height) && (ox >= 0) && (ox < input.Width))
                            {
                                var fi = ((f.Width * fy) + fx) * f.Depth;
                                var ti = ((input.Width * oy) + ox) * input.Depth;
                                for (var fd = 0; fd < f.Depth; fd++)
                                {
                                    a += f.W[fi + fd] * input.W[ti + fd];
                                }
                            }
                        }
                    }
                    result[ax, ay, d] = a + biases.W[d];
                }
            }
        });


        return result;
    }


    /// <summary>
    /// 对 2D 张量执行卷积操作。
    /// </summary>
    /// <param name="input">输入的 2D 张量。</param>
    /// <param name="filters">卷积核的数组。</param>
    /// <param name="biases">偏置张量。</param>
    /// <param name="stride">卷积步长，默认为 1。</param>
    /// <param name="dilation">卷积扩张率，默认为 1。</param>
    public static Tensor Conv2D(Tensor input, Tensor[] filters, Tensor biases, int stride = 1, int dilation = 1)
    {

        var outputDepth = filters.Length;
        var result = new Tensor(input.Width / stride, input.Height / stride, outputDepth);

        Parallel.For(0, outputDepth, (int d) =>
        {
            var f = filters[d];
            for (int ay = 0; ay < result.Height; ay++)
            {
                var y = ay * stride - (f.Height * dilation + dilation - 1) / 2;
                for (int ax = 0; ax < result.Width; ax++)
                {
                    var x = ax * stride - (f.Width * dilation + dilation - 1) / 2;
                    //var a = default(T);
                    var a = 0f;

                    for (int fy = 0; fy < f.Height; fy++)
                    {
                        var oy = y + fy * dilation + dilation - 1;
                        for (int fx = 0; fx < f.Width; fx++)
                        {
                            var ox = x + fx * dilation + dilation - 1;
                            if ((oy >= 0) && (oy < input.Height) && (ox >= 0) && (ox < input.Width))
                            {
                                var fi = ((f.Width * fy) + fx) * f.Depth;
                                var ti = ((input.Width * oy) + ox) * input.Depth;
                                for (var fd = 0; fd < f.Depth; fd++)
                                {
                                    a += f.W[fi + fd] * input.W[ti + fd];
                                }
                            }
                        }
                    }
                    result[ax, ay, d] = a + biases.W[d];
                }
            }
        });


        return result;
    }
    //    您好，这是必应。我看到您和我分享了一些代码。这是用 C# 语言实现的 Conv2D 函数。这个函数对一个输入张量（Tensor）进行了 二维卷积，使用了一组给定的滤波器（filters）、偏置（biases）、步长（stride）和扩张（dilation）。二维卷积是一种数学运算，它将一个滤波器（也叫卷积核）应用到一个输入图像或张量上，产生一个输出图像或张量。滤波器在输入上滑动，每个位置上，它计算滤波器和输入的点积（dot product），并加上一个偏置项（bias term）。结果存储在输出的相应位置上。步长和扩张参数控制滤波器在输入上的移动方式。步长是滤波器在每个维度上移动的像素数，扩张是滤波器元素之间的间距。较大的步长或扩张会减小输出的大小，而较小的步长或扩张会保留更多的输入信息。

    //Conv2D 函数在 卷积神经网络（CNNs） 中广泛使用，它是一种能够在图像上执行各种任务的人工神经网络，例如分类、分割、检测等。CNNs 由多层卷积组成，后面跟着其他操作，如池化（pooling）、激活（activation）、归一化（normalization）等。每个卷积层中的滤波器是从数据中学习的，它们能够捕捉图像中的不同特征或模式。例如，第一层的滤波器可能学习到边缘、角点或颜色，而更深层的滤波器可能学习到更复杂的形状、物体或场景。

    //如果您想了解更多关于 Conv2D 和 CNNs 的内容，您可以查看以下资源：

    //Conv2D 层 - Keras：这是 Keras 中 Conv2D 层的文档，Keras 是一个流行的 Python 深度学习框架。它解释了 Conv2D 层的参数和输出形状，并提供了一些使用示例。
    //Keras 中的 Conv2D 函数及其在 CNNs 中的应用 - AskPython：这是一个教程，展示了如何使用 Keras 中的 Conv2D 函数来构建一个用于图像分类的 CNN。它还用一些图表和代码片段解释了卷积、滤波器、步长、填充和扩张的概念。
    //torch.nn.functional.conv2d — PyTorch 2.1 文档：这是 PyTorch 中 conv2d 函数的文档，PyTorch 是另一个流行的 Python 深度学习框架。它有着类似于 Keras 中 Conv2D 层的接口，但它还支持复杂的数据类型和非确定性的算法。
    //Conv2D — PyTorch 2.1 文档：这是 PyTorch 中 Conv2D 类的文档，它是 Module 类的一个子类，表示一个卷积层。它有着和 conv2d 函数相同的参数，但它还有一些方法来初始化权重和偏置，并将层应用到输入上。
    //scipy.signal.convolve2d — SciPy v1.11.4 手册：这是 SciPy 中 convolve2d 函数的文档，SciPy 是一个 Python 科学计算库。它对两个二维数组进行二维卷积，有不同的模式和边界条件的选项。它可以用于图像处理，例如模糊、锐化、边缘检测等。
}