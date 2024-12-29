namespace Vorcyc.Mathematics;


/// <summary>
/// Provides extension methods for various bitwise operations.
/// </summary>
public static class BitMathExtension
{



    #region is power of 2

    /// <summary>
    /// Verifies a number is a power of two
    /// </summary>
    /// <param name="x">Number to check</param>
    /// <returns>true if number is a power two (i.e.:1,2,4,8,16,...)</returns>
    public static bool IsPowerOf2(this uint x)
    {
        return ((x != 0) && (x & (x - 1)) == 0);
    }


    /// <summary>
    /// Verifies a number is a power of two
    /// </summary>
    /// <param name="x">Number to check</param>
    /// <returns>true if number is a power two (i.e.:1,2,4,8,16,...)</returns>
    public static bool IsPowerOf2(this ulong x)
    {
        return ((x != 0) && (x & (x - 1)) == 0);
    }



    /// <summary>
    /// Verifies a number is a power of two
    /// </summary>
    /// <param name="x">Number to check</param>
    /// <returns>true if number is a power two (i.e.:1,2,4,8,16,...)</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <see href="x"/> is less than zero</exception>
    public static bool IsPowerOf2(this int x)
    {
        if (x < 0)
            throw new ArgumentOutOfRangeException(nameof(x), "Must be greater than or equal to zero");
        return IsPowerOf2((uint)x);
    }


    /// <summary>
    /// Verifies a number is a power of two
    /// </summary>
    /// <param name="x">Number to check</param>
    /// <returns>true if number is a power two (i.e.:1,2,4,8,16,...)</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <see pref="x"/> is less than zero</exception>
    public static bool IsPowerOf2(this long x)
    {
        if (x < 0)
            throw new ArgumentOutOfRangeException(nameof(x), "Must be greater than or equal to zero");
        return IsPowerOf2((ulong)x);
    }


    #endregion


    #region Next Power Of 2

    /// <summary>
    /// Get Next power of number.
    /// </summary>
    /// <param name="x">Number to check</param>
    /// <returns>A power of two number</returns>
    /// <exception cref="ArgumentOutOfRangeException">Threows if x is less than zero</exception>
    public static int NextPowerOf2(this int x)
    {
        if (x < 0)
            throw new ArgumentOutOfRangeException(nameof(x), "Must be greater than or equal to zero");

        if (x == 0)
            return 1;

        x -= 1;
        x |= (x >> 1);
        x |= (x >> 2);
        x |= (x >> 4);
        x |= (x >> 8);
        x |= (x >> 16);
        return x + 1;
    }




    /// <summary>
    /// Rounds a number up to the nearest power of 2.
    /// If the value is a power of two, the same value is returned.
    /// If the value is larger than the largest power of 2. It is rounded down.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// Method based on a method found at: http://graphics.stanford.edu/~seander/bithacks.htm
    /// Subtitle: Round up to the next highest power of 2 
    /// </remarks>
    public static ulong NextPowerOf2(this ulong value)
    {
        if (value == 0)
            return 1;
        if (value > (1ul << 62))
            return 1ul << 63;
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        value |= value >> 32;
        value++;
        return value;
    }


    /// <summary>
    /// Rounds a number up to the nearest power of 2.
    /// If the value is a power of two, the same value is returned.
    /// If the value is larger than the largest power of 2. It is rounded down.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// Method based on a method found at: http://graphics.stanford.edu/~seander/bithacks.htm
    /// Subtitle: Round up to the next highest power of 2 
    /// </remarks>
    public static uint NextPowerOf2(this uint value)
    {
        if (value == 0)
            return 1;
        if (value > (1u << 30))
            return 1u << 31;
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        value++;
        return value;
    }



    #endregion


    #region  previous power of 2 

    /// <summary>
    ///   Returns the previous power of 2 after the input value x.
    /// </summary>
    /// 
    /// <param name="x">Input value x.</param>
    /// 
    /// <returns>Returns the previous power of 2 after the input value x.</returns>
    /// <remarks>
    /// 来自于Accord.net framework
    /// </remarks>
    public static int PreviousPowerOf2(this int x)
    {
        return NextPowerOf2(x + 1) / 2;
    }



    /// <summary>
    /// Rounds a number down to the nearest power of 2.
    /// If the value is a power of two, the same value is returned.
    /// If value is zero, one is returned.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ulong PreviousPowerOf2(this ulong value)
    {
        if (value == 0ul)
            return 1;
        return (1ul << 63) >> CountLeadingZeros(value);
    }

    /// <summary>
    /// Rounds a number down to the nearest power of 2.
    /// If the value is a power of two, the same value is returned.
    /// If value is zero, 1 is returned.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static uint PreviousPowerOf2(this uint value)
    {
        if (value == 0u)
            return 1;
        return (1u << 31) >> CountLeadingZeros(value);
    }

    #endregion


    #region [ Count Bits ]

    /// <summary>
    /// Counts the number of bits that are set
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CountBitsSet(this uint value)
    {
        uint count;
        for (count = 0; value > 0; value >>= 1)
        {
            count += value & 1;
        }
        return (int)count;
    }

    /// <summary>
    /// Counts the number of bits that are set
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CountBitsSet(this ulong value)
    {
        ulong count;
        for (count = 0; value > 0; value >>= 1)
        {
            count += value & 1;
        }
        return (int)count;
    }

    /// <summary>
    /// Counts the number of bits that are not set
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CountBitsCleared(this uint value)
    {
        return CountBitsSet(~value);
    }

    /// <summary>
    /// Counts the number of bits that are not set
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CountBitsCleared(this ulong value)
    {
        return CountBitsSet(~value);
    }

    #endregion



    /// <summary>
    /// Creates a bit mask for a number with the given number of bits.
    /// </summary>
    /// <param name="bitCount"></param>
    /// <returns></returns>
    public static ulong CreateBitMask(this int bitCount)
    {
        if (bitCount == 0)
            return 0;
        return ulong.MaxValue >> (64 - bitCount);
    }


    #region [ Count Leading/Trailing Zeros ]

    /// <summary>
    /// Counts the number of consecutive 0's starting from the lowest bit working up.
    /// </summary>
    /// <remarks>
    /// Unfortunately, c# cannot call the cpu instruction ctz
    /// Example from http://en.wikipedia.org/wiki/Find_first_set
    /// </remarks>
    public static int CountTrailingZeros(this uint value)
    {
        if (value == 0)
            return 32;
        int position = 0;
        if ((value & 0xffffu) == 0u)
        {
            value >>= 16;
            position += 16;
        }
        if ((value & 0xffu) == 0u)
        {
            value >>= 8;
            position += 8;
        }
        if ((value & 0xfu) == 0u)
        {
            value >>= 4;
            position += 4;
        }
        if ((value & 0x3u) == 0u)
        {
            value >>= 2;
            position += 2;
        }
        if ((value & 0x1u) == 0u)
        {
            position += 1;
        }
        return position;
    }

    /// <summary>
    /// Counts the number of consecutive 0's starting from the lowest bit working up.
    /// </summary>
    /// <remarks>
    /// Unfortunately, c# cannot call the cpu instruction ctz
    /// Example from http://en.wikipedia.org/wiki/Find_first_set
    /// </remarks>
    public static int CountTrailingZeros(this ulong value)
    {
        if (value == 0)
            return 64;
        int position = 0;
        if ((value & 0xfffffffful) == 0ul)
        {
            value >>= 32;
            position += 32;
        }
        if ((value & 0xfffful) == 0ul)
        {
            value >>= 16;
            position += 16;
        }
        if ((value & 0xfful) == 0ul)
        {
            value >>= 8;
            position += 8;
        }
        if ((value & 0xful) == 0ul)
        {
            value >>= 4;
            position += 4;
        }
        if ((value & 0x3ul) == 0ul)
        {
            value >>= 2;
            position += 2;
        }
        if ((value & 0x1ul) == 0ul)
        {
            position += 1;
        }
        return position;
    }

    /// <summary>
    /// Counts the number of consecutive 0's starting from the highest bit working down.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// Unfortunately, c# cannot call the cpu instruction clz
    /// Example from http://en.wikipedia.org/wiki/Find_first_set
    /// </remarks>
    public static int CountLeadingZeros(this uint value)
    {
        if (value == 0ul)
            return 32;
        int position = 0;
        if ((value & 0xFFFF0000u) == 0u)
        {
            value <<= 16;
            position += 16;
        }
        if ((value & 0xFF000000u) == 0u)
        {
            value <<= 8;
            position += 8;
        }
        if ((value & 0xF0000000) == 0u)
        {
            value <<= 4;
            position += 4;
        }
        if ((value & 0xC0000000) == 0u)
        {
            value <<= 2;
            position += 2;
        }
        if ((value & 0x80000000) == 0u)
        {
            position += 1;
        }
        return position;
    }

    /// <summary>
    /// Counts the number of consecutive 0's starting from the highest bit working down.
    /// </summary>
    /// <remarks>
    /// Unfortunately, c# cannot call the cpu instruction clz
    /// Example from http://en.wikipedia.org/wiki/Find_first_set
    /// </remarks>
    public static int CountLeadingZeros(this ulong value)
    {
        if (value == 0ul)
            return 64;
        int position = 0;
        if ((value & 0xFFFFFFFF00000000ul) == 0ul)
        {
            value <<= 32;
            position += 32;
        }
        if ((value & 0xFFFF000000000000ul) == 0ul)
        {
            value <<= 16;
            position += 16;
        }
        if ((value & 0xFF00000000000000ul) == 0ul)
        {
            value <<= 8;
            position += 8;
        }
        if ((value & 0xF000000000000000ul) == 0ul)
        {
            value <<= 4;
            position += 4;
        }
        if ((value & 0xC000000000000000ul) == 0ul)
        {
            value <<= 2;
            position += 2;
        }
        if ((value & 0x8000000000000000ul) == 0ul)
        {
            position += 1;
        }
        return position;
    }

    #endregion



    #region [ Count Leading/Trailing Ones ]

    /// <summary>
    /// Counts the number of consecutive 1's starting from the lowest bit working up.
    /// </summary>
    public static int CountTrailingOnes(this uint value)
    {
        return CountTrailingZeros(~value);
    }

    /// <summary>
    /// Counts the number of consecutive 1's starting from the lowest bit working up.
    /// </summary>
    public static int CountTrailingOnes(this ulong value)
    {
        return CountTrailingZeros(~value);
    }

    /// <summary>
    /// Counts the number of consecutive 1's starting from the highest bit working down.
    /// </summary>
    public static int CountLeadingOnes(this uint value)
    {
        return CountLeadingZeros(~value);
    }

    /// <summary>
    /// Counts the number of consecutive 1's starting from the highest bit working down.
    /// </summary>
    public static int CountLeadingOnes(this ulong value)
    {
        return CountLeadingZeros(~value);
    }

    #endregion



    /// <summary>
    /// Returns the bit position for every bit that is set in the provided value.
    /// Bit positions are defined as 0-63;
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IEnumerable<int> GetSetBitPositions(this ulong value)
    {
        // Once value becomes zero, the remainder of the loop can be short-cut
        for (int x = 0; value != 0; x++, value >>= 1)
        {
            if ((value & 1) == 1)
                yield return x;
        }
    }

    /// <summary>
    /// Returns the bit position for every bit that is set in the provided value.
    /// Bit positions are defined as 0-31;
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IEnumerable<int> GetSetBitPositions(this uint value)
    {
        // Once value becomes zero, the remainder of the loop can be short-cut
        for (int x = 0; value != 0; x++, value >>= 1)
        {
            if ((value & 1) == 1)
                yield return x;
        }
    }

    /// <summary>
    /// Returns the bit position for every bit that is cleared in the provided value.
    /// Bit positions are defined as 0-31;
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IEnumerable<int> GetClearedBitPositions(this uint value)
    {
        return GetSetBitPositions(~value);
    }

    /// <summary>
    /// Returns the bit position for every bit that is cleared in the provided value.
    /// Bit positions are defined as 0-63;
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IEnumerable<int> GetClearedBitPositions(this ulong value)
    {
        return GetSetBitPositions(~value);
    }


    #region Odd Even

    /// <summary>
    /// 判断是否是奇数
    /// </summary>
    public static bool IsOdd(this long value)
    {
        //vb: (value And 1) = 1
        return (value & 1L) == 1;
    }

    /// <summary>
    /// 判断是否是奇数
    /// </summary>
    public static bool IsOdd(this ulong value)
    {
        //vb: (value And 1) = 1
        return (value & 1UL) == 1;
    }

    /// <summary>
    /// 判断是否是奇数
    /// </summary>
    public static bool IsOdd(this int value)
    {
        //vb: (value And 1) = 1
        return (value & 1) == 1;
    }


    /// <summary>
    /// 判断是否是奇数
    /// </summary>
    public static bool IsOdd(this uint value)
    {
        //vb: (value And 1) = 1
        return (value & 1u) == 1U;
    }



    /// <summary>
    /// 判断是否是偶数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsEven(this long value)
    {
        //vb: Not (value And 1) = 1
        //return ~(value & 1L) == 1;
        //上面错了，18.7.9发现
        //~ 是按位取反
        //! 才是逻辑非
        return !((value & 1L) == 1L);
    }



    /// <summary>
    /// 判断是否是偶数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsEven(this ulong value)
    {
        //vb: Not (value And 1) = 1
        //return ~(value & 1L) == 1;
        //上面错了，18.7.9发现
        //~ 是按位取反
        //! 才是逻辑非
        return !((value & 1UL) == 1ul);
    }



    /// <summary>
    /// 判断是否是偶数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsEven(this int value)
    {
        //vb: Not (value And 1) = 1
        //return ~(value & 1L) == 1;
        //上面错了，18.7.9发现
        //~ 是按位取反
        //! 才是逻辑非
        return !((value & 1) == 1);
    }



    /// <summary>
    /// 判断是否是偶数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsEven(this uint value)
    {
        //vb: Not (value And 1) = 1
        //return ~(value & 1L) == 1;
        //上面错了，18.7.9发现
        //~ 是按位取反
        //! 才是逻辑非
        return !((value & 1U) == 1U);
    }





    #endregion





}
