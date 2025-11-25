namespace Vorcyc.Mathematics.Utilities;

/// <summary>
/// 提供用于打印各种类型对象的扩展方法。
/// </summary>
public static class PrintHelper
{
    /// <summary>
    /// 打印数组的每个元素，并在元素之间添加逗号。
    /// </summary>
    /// <typeparam name="T">数组中元素的类型。</typeparam>
    /// <param name="array">要打印的数组。</param>
    /// <param name="elementColor">元素的控制台颜色。</param>
    public static void PrintLine<T>(this T[] array, ConsoleColor elementColor = ConsoleColor.Red)
    {
        Console.Write('[');
        array.Each((index, item) =>
        {
            Console.ForegroundColor = elementColor;
            Console.Write(item.ToString());
            Console.ResetColor();
            if (index < array.Length - 1)
                Console.Write(',');
        });
        Console.WriteLine(']');
    }

    /// <summary>
    /// 打印集合的每个元素，并在元素之间添加逗号。
    /// </summary>
    /// <typeparam name="T">集合中元素的类型。</typeparam>
    /// <param name="array">要打印的集合。</param>
    /// <param name="elementColor">元素的控制台颜色。</param>
    public static void PrintLine<T>(this IEnumerable<T> array, ConsoleColor elementColor = ConsoleColor.Red)
    {
        Console.Write('[');
        array.Each((index, item) =>
        {
            Console.ForegroundColor = elementColor;
            Console.Write(item.ToString());
            Console.ResetColor();
            if (index < array.Count() - 1)
                Console.Write(',');
        });
        Console.WriteLine(']');
    }

    /// <summary>
    /// 打印Span的每个元素，并在元素之间添加逗号。
    /// </summary>
    /// <typeparam name="T">Span中元素的类型。</typeparam>
    /// <param name="array">要打印的Span。</param>
    /// <param name="elementColor">元素的控制台颜色。</param>
    public static void PrintLine<T>(this scoped Span<T> array, ConsoleColor elementColor = ConsoleColor.Red)
    {
        Console.Write('[');
        for (int i = 0; i < array.Length; i++)
        {
            Console.ForegroundColor = elementColor;
            Console.Write(array[i].ToString());
            Console.ResetColor();
            if (i < array.Length - 1)
                Console.Write(',');
        }
        Console.WriteLine(']');
    }

    /// <summary>
    /// 打印对象到控制台。
    /// </summary>
    /// <param name="obj">要打印的对象。</param>
    /// <param name="foreground">前景色。</param>
    public static void Print(this object obj, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.Write(obj);
        Console.ResetColor();
    }

    /// <summary>
    /// 打印字符串到控制台。
    /// </summary>
    /// <param name="obj">要打印的字符串。</param>
    /// <param name="foreground">前景色。</param>
    public static void Print(this string obj, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.Write(obj);
        Console.ResetColor();
    }

    /// <summary>
    /// 打印对象到控制台并换行。
    /// </summary>
    /// <param name="obj">要打印的对象。</param>
    /// <param name="foreground">前景色。</param>
    public static void PrintLine(this object obj, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.WriteLine(obj);
        Console.ResetColor();
    }

    /// <summary>
    /// 打印带有信息文本的对象到控制台并换行。
    /// </summary>
    /// <param name="obj">要打印的对象。</param>
    /// <param name="infoText">附加的信息文本。</param>
    /// <param name="foreground">前景色。</param>
    public static void PrintLine(this object obj, string infoText, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.WriteLine($"{infoText} {obj}");
        Console.ResetColor();
    }

    /// <summary>
    /// 打印字符串到控制台并换行。
    /// </summary>
    /// <param name="obj">要打印的字符串。</param>
    /// <param name="foreground">前景色。</param>
    public static void PrintLine(this string obj, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.WriteLine(obj);
        Console.ResetColor();
    }

    /// <summary>
    /// 打印带有信息文本的字符串到控制台并换行。
    /// </summary>
    /// <param name="obj">要打印的字符串。</param>
    /// <param name="infoText">附加的信息文本。</param>
    /// <param name="foreground">前景色。</param>
    public static void PrintLine(this string obj, string infoText, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.WriteLine($"{infoText} {obj}");
        Console.ResetColor();
    }
}
