namespace Vorcyc.Mathematics.LanguageExtension;


/// <summary>
/// Provides extension methods for various language-related operations.
/// </summary>
public static class LanguageExtension
{
    /// <summary>
    /// Iterates over each element in the array and performs the specified action.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to iterate over.</param>
    /// <param name="action">The action to perform on each element. The first parameter is the index, and the second parameter is the element.</param>
    public static void Each<T>(this T[] array, Action<int, T> action)
    {
        for (int i = 0; i < array.Length; i++)
        {
            action?.Invoke(i, array[i]);
        }
    }

    /// <summary>
    /// Iterates over each element in the collection and performs the specified action.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection to iterate over.</param>
    /// <param name="action">The action to perform on each element. The first parameter is the index, and the second parameter is the element.</param>
    public static void Each<T>(this IEnumerable<T> collection, Action<int, T> action)
    {
        int index = 0;
        foreach (var item in collection)
        {
            action(index++, item);
        }
    }

    /// <summary>
    /// Iterates over each element in the span and performs the specified action.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="collection">The span to iterate over.</param>
    /// <param name="action">The action to perform on each element. The first parameter is the index, and the second parameter is the element.</param>
    public static void Each<T>(this Span<T> collection, Action<int, T> action)
    {
        int index = 0;
        foreach (var item in collection)
        {
            action(index++, item);
        }
    }

    #region ToBool (C/C++ language bridge)

    /// <summary>
    /// Converts an integer to a boolean value. This method is a C/C++ language bridge.
    /// </summary>
    /// <param name="x">The integer to convert.</param>
    /// <returns>True if the integer is non-zero; otherwise, false.</returns>
    public static bool ToBool(this int x) => x != 0;

    /// <summary>
    /// Converts an unsigned integer to a boolean value. This method is a C/C++ language bridge.
    /// </summary>
    /// <param name="x">The unsigned integer to convert.</param>
    /// <returns>True if the unsigned integer is non-zero; otherwise, false.</returns>
    public static bool ToBool(this uint x) => x != 0;

    /// <summary>
    /// Converts a long integer to a boolean value. This method is a C/C++ language bridge.
    /// </summary>
    /// <param name="x">The long integer to convert.</param>
    /// <returns>True if the long integer is non-zero; otherwise, false.</returns>
    public static bool ToBool(this long x) => x != 0;

    /// <summary>
    /// Converts an unsigned long integer to a boolean value. This method is a C/C++ language bridge.
    /// </summary>
    /// <param name="x">The unsigned long integer to convert.</param>
    /// <returns>True if the unsigned long integer is non-zero; otherwise, false.</returns>
    public static bool ToBool(this ulong x) => x != 0;

    #endregion
}
