using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;
namespace Vorcyc.Mathematics.DeepLearning.Layers;
public static partial class Layers
{
    /// <summary>
    /// 瀵硅緭鍏ュ紶閲忔墽琛?Sigmoid 婵€娲诲嚱鏁版搷浣溿€?    /// </summary>
    /// <typeparam name="T">寮犻噺鍏冪礌鐨勭被鍨嬶紝蹇呴』瀹炵幇 <see cref="IBinaryFloatingPointIeee754{TSelf}"/> 鎺ュ彛銆?/typeparam>
    /// <param name="input">杈撳叆鐨勫紶閲忋€?/param>
    /// <returns>鎵ц Sigmoid 鎿嶄綔鍚庣殑寮犻噺銆?/returns>
    public static Tensor<T> Sigmoid<T>(Tensor<T> input)
        where T : IBinaryFloatingPointIeee754<T>
    {
        var height = input.Height;
        var width = input.Width;
        var result = new Tensor<T>(input.Width, input.Height, input.Depth);
        long workPer = (long)height * width;
        ForEachDepth(input.Depth, workPer, d =>
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
    /// 瀵硅緭鍏ュ紶閲忔墽琛?Sigmoid 婵€娲诲嚱鏁版搷浣溿€?    /// </summary>
    /// <param name="input">杈撳叆鐨勫紶閲忋€?/param>
    /// <returns>鎵ц Sigmoid 鎿嶄綔鍚庣殑寮犻噺銆?/returns>
    public static Tensor Sigmoid(Tensor input)
    {
        var height = input.Height;
        var width = input.Width;
        var result = new Tensor(input.Width, input.Height, input.Depth);
        long workPer = (long)height * width;
        ForEachDepth(input.Depth, workPer, d =>
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
