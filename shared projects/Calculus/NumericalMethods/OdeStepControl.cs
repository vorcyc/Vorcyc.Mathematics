namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

using System.Numerics;

/// <summary>自适应 ODE 步长控制工具。</summary>
internal static class OdeStepControl
{
    /// <summary>根据接受步与缩放误差估算下一步长。</summary>
    public static T GrowStep<T>(
        T acceptedH, T scaledError, T currentH,
        T minStep, T maxStep, T safety,
        T maxGrowFactor, T quarterExponent, T errorFloor)
        where T : struct, IFloatingPointIeee754<T>
    {
        if (scaledError <= T.Zero)
            return currentH;

        T grow = T.Min(maxGrowFactor,
            safety * T.Pow(T.One / T.Max(scaledError, errorFloor), quarterExponent));
        return T.CopySign(T.Min(T.Max(T.Abs(acceptedH) * grow, minStep), maxStep), acceptedH);
    }
}
