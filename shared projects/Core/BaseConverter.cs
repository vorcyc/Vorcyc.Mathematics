using System.Numerics;

namespace Vorcyc.Mathematics;

/// <summary>
/// 提供将任意实现了 <see cref="IBinaryInteger{TSelf}"/> 接口的整数类型转换为指定进制的字符串表示和从指定进制的字符串表示转换为整数的方法。
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
    /// 将任意实现了 <see cref="IBinaryInteger{TSelf}"/> 接口的整数类型转换为指定进制的字符串表示。
    /// 进制的范围咋2到94之间。
    /// </summary>
    /// <typeparam name="TSelf">实现了 <see cref="IBinaryInteger{TSelf}"/> 接口的整数类型</typeparam>
    /// <param name="integer">要转换的整数</param>
    /// <param name="baseNumber">进制数。要求大于或等于2，小于或等于94</param>
    /// <returns>字符串形式的进制数</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToBaseString<TSelf>(this TSelf integer, TSelf baseNumber)
        where TSelf : IBinaryInteger<TSelf>
    {
        const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        // 检查进制数和源数
        if (integer < TSelf.Zero)
            throw new ArgumentOutOfRangeException(nameof(integer), "值必须为正");
        if (baseNumber < TSelf.CreateChecked(2) || baseNumber > TSelf.CreateChecked(digits.Length))
            throw new ArgumentOutOfRangeException(nameof(baseNumber), $"基数必须在2到{digits.Length}间");

        // 特殊处理零
        if (integer == TSelf.Zero)
            return "0";

        Span<char> buffer = stackalloc char[128]; // 足够大以容纳所有可能的结果
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
    /// 将指定进制的字符串表示转换为任意实现了 <see cref="IBinaryInteger{TSelf}"/> 接口的整数类型。
    /// 进制的范围在2到94之间。
    /// </summary>
    /// <typeparam name="TSelf">实现了 <see cref="IBinaryInteger{TSelf}"/> 接口的整数类型</typeparam>
    /// <param name="value">要转换的字符串</param>
    /// <param name="baseNumber">进制数。要求大于或等于2，小于或等于94</param>
    /// <returns>转换后的整数</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf FromBaseString<TSelf>(this string value, TSelf baseNumber)
        where TSelf : IBinaryInteger<TSelf>
    {
        const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        // 检查进制数和源数
        if (baseNumber < TSelf.CreateChecked(2) || baseNumber > TSelf.CreateChecked(digits.Length))
            throw new ArgumentOutOfRangeException(nameof(baseNumber), $"基数必须在2到{digits.Length}间");

        TSelf result = TSelf.Zero;
        TSelf baseValue = TSelf.One;

        for (int i = value.Length - 1; i >= 0; i--)
        {
            int digitValue = digits.IndexOf(value[i]);
            if (digitValue == -1)
                throw new ArgumentOutOfRangeException(nameof(value), $"字符串包含无效字符 '{value[i]}'");

            result += TSelf.CreateChecked(digitValue) * baseValue;
            baseValue *= baseNumber;
        }
        
        return result;
    }
}