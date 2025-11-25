using Vorcyc.Mathematics;
using Vorcyc.Mathematics.SignalProcessing.Filters.Base;

namespace Vorcyc.Mathematics.SignalProcessing.Filters;

/// <summary>
/// Represents Hilbert filter.
/// </summary>
public class HilbertFilter : FirFilter
{
    /// <summary>
    /// Gets size of the filter.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Constructs <see cref="HilbertFilter"/> of given <paramref name="size"/>.
    /// </summary>
    /// <param name="size">Size of the filter</param>
    public HilbertFilter(int size = 128) : base(MakeKernel(size))
    {
        Size = size;
    }

    /// <summary>
    /// Generates filter kernel of given <paramref name="size"/>.
    /// </summary>
    /// <param name="size">Kernel size</param>
    private static IEnumerable<float> MakeKernel(int size)
    {
        var kernel = new float[size];

        kernel[0] = 0;
        for (var i = 1; i < size; i++)
        {
            kernel[i] = 2 * MathF.Pow(MathF.Sin(ConstantsFp32.PI * i / 2), 2) / (ConstantsFp32.PI * i);
        }

        return kernel;
    }
}
