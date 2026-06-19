//using System.Numerics;
//using System.Runtime.InteropServices;
//using Vorcyc.Mathematics.LinearAlgebra;
//namespace Vorcyc.Mathematics.DeepLearning.Layers;
/////<summary>Implements the neural network layers.</summary>
//public static partial class Layers
//{
//    /// <summary>
//    /// 瀵硅緭鍏ュ紶閲忚繘琛屾壒閲忓綊涓€鍖栥€?//    /// </summary>
//    /// <typeparam name="T">寮犻噺鍏冪礌鐨勭被鍨嬶紝蹇呴』瀹炵幇 <see cref="IBinaryFloatingPointIeee754{TSelf}"/>鎺ュ彛銆?/typeparam>
//    /// <param name="input">杈撳叆鐨勫紶閲忋€?/param>
//    /// <param name="mean">鍧囧€煎紶閲忋€?/param>
//    /// <param name="variance">鏂瑰樊寮犻噺銆?/param>
//    /// <param name="shift">鍋忕Щ閲忓紶閲忋€?/param>
//    /// <param name="scale">缂╂斁鍥犲瓙寮犻噺銆?/param>
//    /// <returns>褰掍竴鍖栧悗鐨勫紶閲忋€?/returns>
//    public static Tensor<T> BatchNorm<T>(Tensor<T> input, Tensor<T> mean, Tensor<T> variance, Tensor<T> shift, Tensor<T> scale)
//     where T : unmanaged, IBinaryFloatingPointIeee754<T>
//    {
//        var v = T.CreateChecked(1e-5); // 甯告暟
//        var normalized = new Tensor<T>(input.Width, input.Height, input.Depth);
//        // 澶勭悊姣忎釜娣卞害閫氶亾
//        for (int d = 0; d < input.Depth; d++)
//        {
//            var meanValue = mean.Values[d];
//            var varianceValue = variance.Values[d];
//            var shiftValue = shift.Values[d];
//            var scaleValue = scale.Values[d];
//            var sqrtVariance = T.Sqrt(v + varianceValue);
//            int vectorSize = System.Numerics.Vector<T>.Count;
//            int fullVectorCount = input.Width * input.Height / vectorSize;
//            // 灏?input.Values 杞崲涓?Span<Vector<T>>
//            var inputSpan = input.Values.Slice(d * input.Width * input.Height);
//            var normalizedSpan = normalized.Values.Slice(d * input.Width * input.Height);
//            var inputVecSpan = System.Runtime.InteropServices.MemoryMarshal.Cast<T, System.Numerics.Vector<T>>(inputSpan);
//            var normalizedVecSpan = System.Runtime.InteropServices.MemoryMarshal.Cast<T, System.Numerics.Vector<T>>(normalizedSpan);
//            // 浣跨敤 SIMD 澶勭悊澶ч儴鍒嗘暟鎹?//            for (int i = 0; i < fullVectorCount; i++)
//            {
//                var inputVec = inputVecSpan[i];
//                var meanVec = new System.Numerics.Vector<T>(meanValue);
//                var varianceVec = new System.Numerics.Vector<T>(sqrtVariance);
//                var shiftVec = new System.Numerics.Vector<T>(shiftValue);
//                var scaleVec = new System.Numerics.Vector<T>(scaleValue);
//                var normalizedVec = (inputVec - meanVec) / varianceVec * scaleVec + shiftVec;
//                normalizedVec.CopyTo(normalizedSpan.Slice(i * vectorSize));
//            }
//            // 澶勭悊鍓╀綑鐨勬暟鎹?//            for (int i = fullVectorCount * vectorSize; i < input.Width * input.Height; i++)
//            {
//                normalizedSpan[i] = (inputSpan[i] - meanValue) / sqrtVariance * scaleValue + shiftValue;
//            }
//        }
//        return normalized;
//    }

//    /// <summary>
//    /// 瀵硅緭鍏ュ紶閲忚繘琛屾壒閲忓綊涓€鍖栥€?//    /// </summary>
//    /// <param name="input">杈撳叆鐨勫紶閲忋€?/param>
//    /// <param name="mean">鍧囧€煎紶閲忋€?/param>
//    /// <param name="variance">鏂瑰樊寮犻噺銆?/param>
//    /// <param name="shift">鍋忕Щ閲忓紶閲忋€?/param>
//    /// <param name="scale">缂╂斁鍥犲瓙寮犻噺銆?/param>
//    /// <returns>褰掍竴鍖栧悗鐨勫紶閲忋€?/returns>
//    public static Tensor BatchNorm(Tensor input, Tensor mean, Tensor variance, Tensor shift, Tensor scale)
//    {
//        var v = 1e-5f; // 甯告暟
//        var normalized = new Tensor(input.Width, input.Height, input.Depth);
//        // 澶勭悊姣忎釜娣卞害閫氶亾
//        Parallel.For(0, input.Depth, (int d) =>
//        {
//            var meanValue = mean.Values[d];
//            var varianceValue = variance.Values[d];
//            var shiftValue = shift.Values[d];
//            var scaleValue = scale.Values[d];
//            var sqrtVariance = MathF.Sqrt(v + varianceValue);
//            int vectorSize = System.Numerics.Vector<float>.Count;
//            int fullVectorCount = input.Width * input.Height / vectorSize;
//            // 灏?input.Values 鍜?normalized.Values 杞崲涓?Span<Vector<float>>
//            var inputSpan = input.Values.AsSpan(d * input.Width * input.Height, input.Width * input.Height);
//            var normalizedSpan = normalized.Values.AsSpan(d * input.Width * input.Height, input.Width * input.Height);
//            var inputVecSpan = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(inputSpan);
//            var normalizedVecSpan = MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(normalizedSpan);
//            // 浣跨敤 SIMD 澶勭悊澶ч儴鍒嗘暟鎹?//            for (int i = 0; i < fullVectorCount; i++)
//            {
//                var inputVec = inputVecSpan[i];
//                var meanVec = new System.Numerics.Vector<float>(meanValue);
//                var varianceVec = new System.Numerics.Vector<float>(sqrtVariance);
//                var shiftVec = new System.Numerics.Vector<float>(shiftValue);
//                var scaleVec = new System.Numerics.Vector<float>(scaleValue);
//                var normalizedVec = (inputVec - meanVec) / varianceVec * scaleVec + shiftVec;
//                normalizedVec.CopyTo(normalizedSpan.Slice(i * vectorSize));
//            }
//            // 澶勭悊鍓╀綑鐨勬暟鎹?//            for (int i = fullVectorCount * vectorSize; i < input.Width * input.Height; i++)
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
    /// 瀵硅緭鍏ュ紶閲忚繘琛屾壒閲忓綊涓€鍖栥€?    /// </summary>
    /// <typeparam name="T">寮犻噺鍏冪礌鐨勭被鍨嬶紝蹇呴』瀹炵幇 <see cref="IBinaryFloatingPointIeee754{TSelf}"/>鎺ュ彛銆?/typeparam>
    /// <param name="input">杈撳叆鐨勫紶閲忋€?/param>
    /// <param name="mean">鍧囧€煎紶閲忋€?/param>
    /// <param name="variance">鏂瑰樊寮犻噺銆?/param>
    /// <param name="shift">鍋忕Щ閲忓紶閲忋€?/param>
    /// <param name="scale">缂╂斁鍥犲瓙寮犻噺銆?/param>
    /// <returns>褰掍竴鍖栧悗鐨勫紶閲忋€?/returns>
    public static Tensor<T> BatchNorm<T>(Tensor<T> input, Tensor<T> mean, Tensor<T> variance, Tensor<T> shift, Tensor<T> scale)
        where T : IBinaryFloatingPointIeee754<T>
    {
        var v = T.CreateChecked(1e-5);//甯告暟
        var normalized = new Tensor<T>(input.Width, input.Height, input.Depth);
        long workPer = (long)input.Height * input.Width;
        ForEachDepth(input.Depth, workPer, d =>
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
    /// 瀵硅緭鍏ュ紶閲忚繘琛屾壒閲忓綊涓€鍖栥€?    /// </summary>
    /// <param name="input">杈撳叆鐨勫紶閲忋€?/param>
    /// <param name="mean">鍧囧€煎紶閲忋€?/param>
    /// <param name="variance">鏂瑰樊寮犻噺銆?/param>
    /// <param name="shift">鍋忕Щ閲忓紶閲忋€?/param>
    /// <param name="scale">缂╂斁鍥犲瓙寮犻噺銆?/param>
    /// <returns>褰掍竴鍖栧悗鐨勫紶閲忋€?/returns>
    public static Tensor BatchNorm(Tensor input, Tensor mean, Tensor variance, Tensor shift, Tensor scale)
    {
        var v = 1e-5f;//甯告暟
        var normalized = new Tensor(input.Width, input.Height, input.Depth);
        long workPer = (long)input.Height * input.Width;
        ForEachDepth(input.Depth, workPer, d =>
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
