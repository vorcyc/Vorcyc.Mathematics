namespace Vorcyc.Mathematics.LanguageExtension;

public static class PrintHelper
{

    //public static string ToString<T>(this T[] array)
    //{
    //    var sb = new StringBuilder();
    //    sb.Append('[');
    //    foreach (var item in array)
    //    {
    //        Console.ForegroundColor = ConsoleColor.Red;
    //        sb.Append(item.ToString());
    //        Console.ResetColor();

    //        sb.Append(',');
    //    }
    //    sb.Remove(sb.Length - 1, 1);
    //    sb.Append(']');
    //    return sb.ToString();
    //}


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



    public static void Print(this object obj, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.Write(obj);
        Console.ResetColor();
    }


    public static void Print(this string obj, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.Write(obj);
        Console.ResetColor();
    }


    public static void PrintLine(this object obj, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.WriteLine(obj);
        Console.ResetColor();
    }

    public static void PrintLine(this object obj, string infoText, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.WriteLine($"{infoText} {obj}");
        Console.ResetColor();
    }

    public static void PrintLine(this string obj, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.WriteLine(obj);
        Console.ResetColor();
    }

    public static void PrintLine(this string obj, string infoText, ConsoleColor foreground = ConsoleColor.Gray)
    {
        Console.ForegroundColor = foreground;
        Console.WriteLine($"{infoText} {obj}");
        Console.ResetColor();
    }

}