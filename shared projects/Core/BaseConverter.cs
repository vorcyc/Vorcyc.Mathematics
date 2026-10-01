using System.Numerics;
namespace Vorcyc.Mathematics;
/// <summary>
/// Provides methods for converting any integer type that implements <see cref="IBinaryInteger{TSelf}"/> to a string representation in a specified base, and for parsing such string representations back into integers.
/// </summary>
public static class BaseConverter
{
    ///// <summary>
    ///// 转换一个正整数到2至36进制的字符串
    ///// </summary>
    ///// <param name="number">正整数</param>
    ///// <param name="baseNum">进制数</param>
    ///// <returns>字符串形式的进制数</returns>
    ///// <exception cref="ArgumentOutOfRangeException"/>
    ///// <example>
    ///// 以下代码演示如何使用本方法进行进制转换：
    ///// <code>
    ///// var r = Vorcyc.PowerLibrary.Mathematics.BaseConverter.ConvertTo(100, 16);
    ///// Console.WriteLine(r);   // => 64
    ///// r = Vorcyc.PowerLibrary.Mathematics.BaseConverter.ConvertTo(100, 8);
    ///// Console.WriteLine(r);   //  =>  144 
    ///// r = Vorcyc.PowerLibrary.Mathematics.BaseConverter.ConvertTo(100, 2);
    ///// Console.WriteLine(r);   //  =>  1100100
    ///// r = Vorcyc.PowerLibrary.Mathematics.BaseConverter.ConvertTo(19, 20);
    ///// Console.WriteLine(r);   //  =>  J
    ///// r = Vorcyc.PowerLibrary.Mathematics.BaseConverter.ConvertTo(36, 36);
    ///// Console.WriteLine(r);   //  =>  10
    ///// </code>
    ///// </example>
    //public static string ConvertTo(long number, short baseNum)
    //{
    //    int digitValue;
    //    System.Text.StringBuilder res = new System.Text.StringBuilder();
    //    const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    //    //检查进制数和源数
    //    if (number < 0)
    //        throw new ArgumentOutOfRangeException("值必须为正");
    //    else if (baseNum < 2 || baseNum > 36)
    //        throw new ArgumentOutOfRangeException("基数必须在2到36间");
    //    while (number > 0L)
    //    {
    //        digitValue = (int)(number % ((long)baseNum));
    //        number /= (long)baseNum;
    //        res.Insert(0, digits[digitValue]);
    //    }
    //    return res.ToString();
    //}

    /// <summary>
    /// Converts any integer type that implements <see cref="IBinaryInteger{TSelf}"/> to a string representation in the specified base.
    /// The base must be between 2 and 94.
    /// </summary>
    /// <typeparam name="TSelf">An integer type that implements <see cref="IBinaryInteger{TSelf}"/>.</typeparam>
    /// <param name="integer">The integer to convert.</param>
    /// <param name="baseNumber">The base. Must be greater than or equal to 2 and less than or equal to 94.</param>
    /// <returns>The string representation in the specified base.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToBaseString<TSelf>(this TSelf integer, TSelf baseNumber)
        where TSelf : IBinaryInteger<TSelf>
    {
        const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
        // Validate the base and source value
        if (integer < TSelf.Zero)
            throw new ArgumentOutOfRangeException(nameof(integer), "Value must be positive.");
        if (baseNumber < TSelf.CreateChecked(2) || baseNumber > TSelf.CreateChecked(digits.Length))
            throw new ArgumentOutOfRangeException(nameof(baseNumber), $"The base must be between 2 and {digits.Length}.");
        // Handle zero specially
        if (integer == TSelf.Zero)
            return "0";
        Span<char> buffer = stackalloc char[128]; // Large enough to hold any possible result
        int index = buffer.Length;
        while (integer > TSelf.Zero)
        {
            var (quotient, remainder) = TSelf.DivRem(integer, baseNumber);
            buffer[--index] = digits[int.CreateChecked(remainder)];
            integer = quotient;
        }
        return new string(buffer.Slice(index));
    }

    /// <summary>
    /// Converts a string representation in the specified base to any integer type that implements <see cref="IBinaryInteger{TSelf}"/>.
    /// The base must be between 2 and 94.
    /// </summary>
    /// <typeparam name="TSelf">An integer type that implements <see cref="IBinaryInteger{TSelf}"/>.</typeparam>
    /// <param name="value">The string to convert.</param>
    /// <param name="baseNumber">The base. Must be greater than or equal to 2 and less than or equal to 94.</param>
    /// <returns>The converted integer.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf FromBaseString<TSelf>(this string value, TSelf baseNumber)
        where TSelf : IBinaryInteger<TSelf>
    {
        const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
        // Validate the base and source value
        if (baseNumber < TSelf.CreateChecked(2) || baseNumber > TSelf.CreateChecked(digits.Length))
            throw new ArgumentOutOfRangeException(nameof(baseNumber), $"The base must be between 2 and {digits.Length}.");
        TSelf result = TSelf.Zero;
        TSelf baseValue = TSelf.One;
        for (int i = value.Length - 1; i >= 0; i--)
        {
            int digitValue = digits.IndexOf(value[i]);
            if (digitValue == -1)
                throw new ArgumentOutOfRangeException(nameof(value), $"The string contains an invalid character '{value[i]}'.");
            result += TSelf.CreateChecked(digitValue) * baseValue;
            baseValue *= baseNumber;
        }
        return result;
    }
}
