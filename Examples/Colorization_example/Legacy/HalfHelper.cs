namespace Colorization_example.Legacy;

/// <summary>Converts IEEE 754 half-precision values to single-precision floats.</summary>
internal static class HalfHelper
{
    private static readonly uint[] MantissaTable = GenerateMantissaTable();
    private static readonly uint[] ExponentTable = GenerateExponentTable();
    private static readonly ushort[] OffsetTable = GenerateOffsetTable();

    public static float HalfToSingle(ushort half)
    {
        uint result = MantissaTable[OffsetTable[half >> 10] + (half & 0x3ff)] + ExponentTable[half >> 10];
        return BitConverter.UInt32BitsToSingle(result);
    }

    private static uint ConvertMantissa(int i)
    {
        uint m = (uint)(i << 13);
        uint e = 0;
        while ((m & 0x00800000) == 0)
        {
            e -= 0x00800000;
            m <<= 1;
        }

        m &= unchecked((uint)~0x00800000);
        e += 0x38800000;
        return m | e;
    }

    private static uint[] GenerateMantissaTable()
    {
        var mantissaTable = new uint[2048];
        mantissaTable[0] = 0;
        for (int i = 1; i < 1024; i++)
        {
            mantissaTable[i] = ConvertMantissa(i);
        }

        for (int i = 1024; i < 2048; i++)
        {
            mantissaTable[i] = (uint)(0x38000000 + ((i - 1024) << 13));
        }

        return mantissaTable;
    }

    private static uint[] GenerateExponentTable()
    {
        var exponentTable = new uint[64];
        exponentTable[0] = 0;
        for (int i = 1; i < 31; i++)
        {
            exponentTable[i] = (uint)(i << 23);
        }

        exponentTable[31] = 0x47800000;
        exponentTable[32] = 0x80000000;
        for (int i = 33; i < 63; i++)
        {
            exponentTable[i] = (uint)(0x80000000 + ((i - 32) << 23));
        }

        exponentTable[63] = 0xc7800000;
        return exponentTable;
    }

    private static ushort[] GenerateOffsetTable()
    {
        var offsetTable = new ushort[64];
        offsetTable[0] = 0;
        for (int i = 1; i < 32; i++)
        {
            offsetTable[i] = 1024;
        }

        offsetTable[32] = 0;
        for (int i = 33; i < 64; i++)
        {
            offsetTable[i] = 1024;
        }

        return offsetTable;
    }
}
