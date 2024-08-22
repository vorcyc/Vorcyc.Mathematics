namespace Vorcyc.Mathematics.Helpers;

/// <summary>
/// Provides extension methods for <see cref="IComparable{T}"/> and <see cref="IComparable"/>
/// make it easy for human.
/// </summary>
public static class IComparableExtension
{

    public static bool IsGreaterThan<T>(this T a, T b)
        where T : IComparable, IComparable<T>
    {
        return a.CompareTo(b) > 0;
    }

    public static bool IsGreaterThanOrEqual<T>(this T a, T b)
        where T : IComparable, IComparable<T>
    {
        return a.CompareTo(b) >= 0;
    }



    public static bool IsLessThan<T>(this T a, T b)
        where T : IComparable, IComparable<T>
    {
        return a.CompareTo(b) < 0;
    }


    public static bool IsLessThanOrEqual<T>(this T a, T b)
        where T : IComparable, IComparable<T>
    {
        return a.CompareTo(b) <= 0;
    }


    public static bool Equals<T>(this T a, T b)
        where T : IComparable, IComparable<T>
    => a.CompareTo(b) == 0;


    public static bool NotEquals<T>(this T a, T b)
        where T : IComparable, IComparable<T>
    => a.CompareTo(b) != 0;

    public static T Max<T>(this T a, T b)
        where T : IComparable, IComparable<T>
        => a.CompareTo(b) == 1 ? a : b;



    public static T Min<T>(this T a, T b)
        where T : IComparable, IComparable<T>
        => a.CompareTo(b) == -1 ? a : b;






}
