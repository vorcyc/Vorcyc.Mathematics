namespace Vorcyc.Mathematics.LanguageExtension;

public static class LanguageExtension
{


    public static void Each<T>(this T[] array, Action<int, T> action)
    {
        for (int i = 0; i < array.Length; i++)
        {
            action?.Invoke(i, array[i]);
        }
    }


    public static void Each<T>(this IEnumerable<T> collection, Action<int, T> action)
    {
        int index = 0;
        foreach (var item in collection)
        {
            action(index++, item);
        }
    }


    public static bool ToBool(this int x) => x != 0;

    public static bool ToBool(this uint x) => x != 0;

    public static bool ToBool(this long x) => x != 0;

    public static bool ToBool(this ulong x) => x != 0;

}