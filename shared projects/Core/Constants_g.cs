#if NET7_0_OR_GREATER

namespace Vorcyc.Mathematics;

using System.Numerics;

/// <summary>
/// Generic-type consts.
/// </summary>
/// <typeparam name="TNumber"></typeparam>
public static class Constants<TNumber>
    where TNumber : struct, IFloatingPointIeee754<TNumber>
{

    public readonly static TNumber E = TNumber.E;


    public readonly static TNumber Log2E = TNumber.Log2(TNumber.E);


    public readonly static TNumber Log10E = TNumber.Log10(TNumber.E);


    public readonly static TNumber Ln10 = TNumber.Log(TNumber.CreateChecked(2));


    public readonly static TNumber Pi = TNumber.Pi;


    public static readonly TNumber Two = TNumber.CreateTruncating(2);


    public readonly static TNumber Two_Pi = Two * Pi;




}

#endif