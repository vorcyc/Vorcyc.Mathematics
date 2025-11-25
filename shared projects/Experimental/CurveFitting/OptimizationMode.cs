namespace Vorcyc.Mathematics.Experimental.CurveFitting;

using System.Numerics;

public enum OptimizationMode
{
    /// <summary>
    /// 使用标准的托管代码。
    /// <para>支持所有约束为 <b><i>unmanaged</i></b> 和 <see cref="IFloatingPointIeee754{TSelf}"/> 的类型</para>
    /// </summary>
    Normal,
    /// <summary>
    /// 使用 SIMD 优化。
    /// <para>
    /// 仅支持 <see cref="float"/> 或 <see cref="double"/> 类型参数。
    /// </para>
    /// <para>当使用其它类型时（如 <see cref="Half"/>）会引发异常。</para>
    /// </summary>
    SIMD,

    ///// <summary>
    ///// 使用并行计算和 SIMD 优化。
    ///// </summary>
    //Parallel,
}