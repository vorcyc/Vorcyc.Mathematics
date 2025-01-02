using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.DeepLearning.Layers;

///<summary>Implements the neural network layers.</summary>
public static partial class Layers
{

    /// <summary>
    /// 对输入张量进行批量归一化。
    /// </summary>
    /// <typeparam name="T">张量元素的类型，必须实现 <see cref="IBinaryFloatingPointIeee754{TSelf}"/>接口。</typeparam>
    /// <param name="input">输入的张量。</param>
    /// <param name="mean">均值张量。</param>
    /// <param name="variance">方差张量。</param>
    /// <param name="shift">偏移量张量。</param>
    /// <param name="scale">缩放因子张量。</param>
    /// <returns>归一化后的张量。</returns>
    public static Tensor<T> BatchNorm<T>(Tensor<T> input, Tensor<T> mean, Tensor<T> variance, Tensor<T> shift, Tensor<T> scale)
        where T : IBinaryFloatingPointIeee754<T>
    {
        var v = T.CreateChecked(1e-5);//常数

        var normalized = new Tensor<T>(input.Width, input.Height, input.Depth);
        Parallel.For(0, input.Depth, (int d) =>
        {
            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    normalized[x, y, d] = (input[x, y, d] - mean.Values[d]) / T.Sqrt(v + variance.Values[d]) * scale.Values[d] + shift.Values[d];
                }
            }
        });
        return normalized;
    }

    /// <summary>
    /// 对输入张量进行批量归一化。
    /// </summary>
    /// <param name="input">输入的张量。</param>
    /// <param name="mean">均值张量。</param>
    /// <param name="variance">方差张量。</param>
    /// <param name="shift">偏移量张量。</param>
    /// <param name="scale">缩放因子张量。</param>
    /// <returns>归一化后的张量。</returns>
    public static Tensor BatchNorm(Tensor input, Tensor mean, Tensor variance, Tensor shift, Tensor scale)
    {
        var v = 1e-5f;//常数

        var normalized = new Tensor(input.Width, input.Height, input.Depth);
        Parallel.For(0, input.Depth, (int d) =>
        {
            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    normalized[x, y, d] = (input[x, y, d] - mean.Values[d]) / MathF.Sqrt(v + variance.Values[d]) * scale.Values[d] + shift.Values[d];
                }
            }
        });
        return normalized;
    }










}