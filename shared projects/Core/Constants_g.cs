#if NET7_0_OR_GREATER

namespace Vorcyc.Mathematics;

using System.Numerics;

/// <summary>
/// Generic-type constants.
/// </summary>
/// <typeparam name="TNumber">The numeric type that implements <see cref="IFloatingPointIeee754{TNumber}"/>.</typeparam>
public static class Constants<TNumber>
    where TNumber : struct, IFloatingPointIeee754<TNumber>
{
    /// <summary>
    /// The mathematical constant e.
    /// </summary>
    public readonly static TNumber E = TNumber.E;

    /// <summary>
    /// The base-2 logarithm of e.
    /// </summary>
    public readonly static TNumber Log2E = TNumber.Log2(TNumber.E);

    /// <summary>
    /// The base-10 logarithm of e.
    /// </summary>
    public readonly static TNumber Log10E = TNumber.Log10(TNumber.E);

    /// <summary>
    /// The natural logarithm of 10.
    /// </summary>
    public readonly static TNumber Ln10 = TNumber.Log(TNumber.CreateChecked(2));

    /// <summary>
    /// The mathematical constant π (pi).
    /// </summary>
    public readonly static TNumber Pi = TNumber.Pi;

    /// <summary>
    /// The constant value 2.
    /// </summary>
    public static readonly TNumber Two = TNumber.CreateTruncating(2);

    /// <summary>
    /// The value of 2π (2 times pi).
    /// </summary>
    public readonly static TNumber Two_Pi = Two * Pi;
}

#endif