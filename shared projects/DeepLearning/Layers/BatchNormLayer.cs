//using System.Numerics;
//using System.Runtime.InteropServices;
//using Vorcyc.Mathematics.LinearAlgebra;

//namespace Vorcyc.Mathematics.DeepLearning.Layers;

/////<summary>Implements the neural network layers.</summary>
//public static partial class Layers
//{

//    /// <summary>
//    /// 对输入张量进行批量归一化。
//    /// </summary>
//    /// <typeparam name="T">张量元素的类型，必须实现 <see cref="IBinaryFloatingPointIeee754{TSelf}"/>接口。</typeparam>
//    /// <param name="input">输入的张量。</param>
//    /// <param name="mean">均值张量。</param>
//    /// <param name="variance">方差张量。</param>
//    /// <param name="shift">偏移量张量。</param>
//    /// <param name="scale">缩放因子张量。</param>
//    /// <returns>归一化后的张量。</returns>
//    public static Tensor<T> BatchNorm<T>(Tensor<T> input, Tensor<T> mean, Tensor<T> variance, Tensor<T> shift, Tensor<T> scale)
//     where T : unmanaged, IBinaryFloatingPointIeee754<T>
//    {
//        var v = T.CreateChecked(1e-5); // 常数

//        var normalized = new Tensor<T>(input.Width, input.Height, input.Depth);

//        // 处理每个深度通道
//        for (int d = 0; d < input.Depth; d++)
//        {
//            var meanValue = mean.Values[d];
//            var varianceValue = variance.Values[d];
//            var shiftValue = shift.Values[d];
//            var scaleValue = scale.Values[d];
//            var sqrtVariance = T.Sqrt(v + varianceValue);

//            int vectorSize = System.Numerics.Vector<T>.Count;
//            int fullVectorCount = input.Width * input.Height / vectorSize;

//            // 将 input.Values 转换为 Span<Vector<T>>
//            var inputSpan = input.Values.Slice(d * input.Width * input.Height);
//            var normalizedSpan = normalized.Values.Slice(d * input.Width * input.Height);
//            var inputVecSpan = System.Runtime.InteropServices.MemoryMarshal.Cast<T, System.Numerics.Vector<T>>(inputSpan);
//            var normalizedVecSpan = System.Runtime.InteropServices.MemoryMarshal.Cast<T, System.Numerics.Vector<T>>(normalizedSpan);

//            // 使用 SIMD 处理大部分数据
//            for (int i = 0; i < fullVectorCount; i++)
//            {
//                var inputVec = inputVecSpan[i];
//                var meanVec = new System.Numerics.Vector<T>(meanValue);
//                var varianceVec = new System.Numerics.Vector<T>(sqrtVariance);
//                var shiftVec = new System.Numerics.Vector<T>(shiftValue);
//                var scaleVec = new System.Numerics.Vector<T>(scaleValue);

//                var normalizedVec = (inputVec - meanVec) / varianceVec * scaleVec + shiftVec;
//                normalizedVec.CopyTo(normalizedSpan.Slice(i * vectorSize));
//            }

//            // 处理剩余的数据
//            for (int i = fullVectorCount * vectorSize; i < input.Width * input.Height; i++)
//            {
//                normalizedSpan[i] = (inputSpan[i] - meanValue) / sqrtVariance * scaleValue + shiftValue;
//            }
//        }

//        return normalized;
//    }



//    /// <summary>
//    /// 对输入张量进行批量归一化。
//    /// </summary>
//    /// <param name="input">输入的张量。</param>
//    /// <param name="mean">均值张量。</param>
//    /// <param name="variance">方差张量。</param>
//    /// <param name="shift">偏移量张量。</param>
//    /// <param name="scale">缩放因子张量。</param>
//    /// <returns>归一化后的张量。</returns>
//    public static Tensor BatchNorm(Tensor input, Tensor mean, Tensor variance, Tensor shift, Tensor scale)
//    {
//        var v = 1e-5f; // 常数

//        var normalized = new Tensor(input.Width, input.Height, input.Depth);

//        // 处理每个深度通道
//        Parallel.For(0, input.Depth, (int d) =>
//        {
//            var meanValue = mean.Values[d];
//            var varianceValue = variance.Values[d];
//            var shiftValue = shift.Values[d];
//            var scaleValue = scale.Values[d];
//            var sqrtVariance = MathF.Sqrt(v + varianceValue);

//            int vectorSize = System.Numerics.Vector<float>.Count;
//            int fullVectorCount = input.Width * input.Height / vectorSize;

//            // 将 input.Values 和 normalized.Values 转换为 Span<Vector<float>>
//            var inputSpan = input.Values.AsSpan(d * input.Width * input.Height, input.Width * input.Height);
//            var normalizedSpan = normalized.Values.AsSpan(d * input.Width * input.Height, input.Width * input.Height);
//            var inputVecSpan = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(inputSpan);
//            var normalizedVecSpan = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(normalizedSpan);

//            // 使用 SIMD 处理大部分数据
//            for (int i = 0; i < fullVectorCount; i++)
//            {
//                var inputVec = inputVecSpan[i];
//                var meanVec = new System.Numerics.Vector<float>(meanValue);
//                var varianceVec = new System.Numerics.Vector<float>(sqrtVariance);
//                var shiftVec = new System.Numerics.Vector<float>(shiftValue);
//                var scaleVec = new System.Numerics.Vector<float>(scaleValue);

//                var normalizedVec = (inputVec - meanVec) / varianceVec * scaleVec + shiftVec;
//                normalizedVec.CopyTo(normalizedSpan.Slice(i * vectorSize));
//            }

//            // 处理剩余的数据
//            for (int i = fullVectorCount * vectorSize; i < input.Width * input.Height; i++)
//            {
//                normalizedSpan[i] = (inputSpan[i] - meanValue) / sqrtVariance * scaleValue + shiftValue;
//            }
//        });

//        return normalized;
//    }

//}


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