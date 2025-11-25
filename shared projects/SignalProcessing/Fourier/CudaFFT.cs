//结果有问题，暂时不用

using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.Cuda.API;
using System.Runtime.InteropServices;

namespace Vorcyc.Mathematics.SignalProcessing.Fourier;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// This class requires <b>cudart64_110.dll</b> and <b>cufft64_10.dll</b> on your PC.
/// </remarks>
public class CudaFFT : IDisposable
{


    private readonly Context _context;

    private readonly CudaAccelerator _cudaAccelerator;

    private readonly CuFFT _cuFFT;

    /// <summary>
    /// Creates instance with default CUDA device.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public CudaFFT()
    {
        _context = Context.Create(builder => builder.Cuda());

        if (_context is null || _context.Devices.IsEmpty)
            throw new PlatformNotSupportedException("No CUDA device detected.");

        var device = _context.Devices[0];

        _cudaAccelerator = device.CreateAccelerator(_context) as CudaAccelerator;
        _cuFFT = new CuFFT();

    }



    public void Forward(float[] input, out Span<ComplexFp32> output)
    {
        using var stream = _cudaAccelerator.CreateStream() as CudaStream;
        using var inputBuffer = _cudaAccelerator.Allocate1D<float>(input);
        using var outputBuffer = _cudaAccelerator.Allocate1D<(float real, float img)>(input.Length);


        CuFFTException.ThrowIfFailed(_cuFFT.Plan1D(out var plan, input.Length, CuFFTType.CUFFT_R2C, batch: 1));

        var result = new (float real, float imaginary)[input.Length];

        using (plan)
        {
            plan.SetStream(stream);
            CuFFTException.ThrowIfFailed(
                plan.ExecR2C(inputBuffer.View.BaseView, outputBuffer.View.BaseView)
                );

            result = outputBuffer.GetAsArray1D(stream);//这点只认 (float real, float imaginary)[] 类型，有点限制！
        }

        output = ConvertToComplexSpan(result);
        WorkaroundKnownIssue(_cudaAccelerator, _cuFFT.API);
    }

    static Span<ComplexFp32> ConvertToComplexSpan((float real, float imaginary)[] tupleArray)
    {
        // 获取 tupleArray 的 Span
        Span<(float real, float imaginary)> tupleSpan = tupleArray;

        // 将 Span 转换为 ComplexFp32 的 Span
        Span<ComplexFp32> complexSpan = MemoryMarshal.Cast<(float real, float imaginary), ComplexFp32>(tupleSpan);

        return complexSpan;
    }

    static Span<(float real, float imaginary)> ConvertToTupleSpan(ComplexFp32[] complexArray)
    {
        // 获取 complexArray 的 Span
        Span<ComplexFp32> complexSpan = complexArray;

        // 将 Span 转换为 (float real, float imaginary) 的 Span
        Span<(float real, float imaginary)> tupleSpan = MemoryMarshal.Cast<ComplexFp32, (float real, float imaginary)>(complexSpan);

        return tupleSpan;
    }

    static Span<(float real, float imaginary)> ConvertToTupleSpan(Span<ComplexFp32> complexSpan)
    {
        // 将 Span 转换为 (float real, float imaginary) 的 Span
        Span<(float real, float imaginary)> tupleSpan = MemoryMarshal.Cast<ComplexFp32, (float real, float imaginary)>(complexSpan);

        return tupleSpan;
    }

    static void WorkaroundKnownIssue(CudaAccelerator accelerator, CuFFTAPI api)
    {
        // The CUDA release notes for 11.2 to 11.3 (inclusive) contains a known issue:
        // - cuFFT planning and plan estimation functions may not restore correct
        //   context affecting CUDA driver API applications.
        //
        // This workaround restores the accelerator context so that deallocation of
        // the memory buffers can be performed on the correct context.
        //
        // Based on the versions of CuFFT released, we would need to apply the
        // workaround to CuFFT v10.4.x.
        //
        // Release 11.1.1   CuFFT v10.3.0.105
        // Release 11.2     CuFFT v10.4.0.72
        // Release 11.3     CuFFT v10.4.2.58
        // Release 11.4     CuFFT v10.5.0.43
        //
        // However, based on actual testing, the issue still persists in later
        // versions. It appears to have been fixed in Release 12.0, which ships
        // with CuFFT v11. So, we will apply the workaround from v10.4.x and later
        // versions, up to v11 (exclusive).
        //
        CuFFTException.ThrowIfFailed(
            api.GetProperty(LibraryPropertyType.MAJOR_VERSION, out var major));
        CuFFTException.ThrowIfFailed(
            api.GetProperty(LibraryPropertyType.MINOR_VERSION, out var minor));
        if (major == 10 && minor >= 4)
        {
            CudaException.ThrowIfFailed(
                CudaAPI.CurrentAPI.SetCurrentContext(accelerator.NativePtr));
        }
    }



    public void Inverse(Span<ComplexFp32> input, out Span<ComplexFp32> output)
    {
        using var stream = _cudaAccelerator.CreateStream() as CudaStream;
        var tupleInput = ConvertToTupleSpan(input);
        using var inputBuffer = _cudaAccelerator.Allocate1D<(float real, float img)>(tupleInput.ToArray());
        using var outputBuffer = _cudaAccelerator.Allocate1D<(float real, float img)>(input.Length);

        CuFFTException.ThrowIfFailed(
            _cuFFT.API.Plan1D(
                out var plan,
                input.Length,
                CuFFTType.CUFFT_C2C,
                batch: 1));
        try
        {
            CuFFTException.ThrowIfFailed(
                _cuFFT.API.SetStream(plan, stream));
            CuFFTException.ThrowIfFailed(
                _cuFFT.API.ExecC2C(
                    plan,
                    inputBuffer.View.BaseView,
                    outputBuffer.View.BaseView,
                    CuFFTDirection.INVERSE));

            var result = outputBuffer.GetAsArray1D(stream);
            output = ConvertToComplexSpan(result);
        }
        finally
        {
            CuFFTException.ThrowIfFailed(
                _cuFFT.API.Destroy(plan));
        }
        WorkaroundKnownIssue(_cudaAccelerator, _cuFFT.API);
    }



    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            _context.Dispose();
            _cudaAccelerator.Dispose();

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~CudaFFT()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    //static ComplexFp32[] ConvertToComplexArray((float real, float imaginary)[] tupleArray)
    //{
    //    // 获取 tupleArray 的 Span
    //    var tupleSpan = tupleArray.AsSpan();

    //    // 将 Span 转换为 byte 数组
    //    Span<byte> byteSpan = MemoryMarshal.AsBytes(tupleSpan);

    //    // 将 byte 数组转换为 ComplexFp32 数组
    //    Span<ComplexFp32> complexSpan = MemoryMarshal.Cast<byte, ComplexFp32>(byteSpan);

    //    return complexSpan.ToArray();
    //}

    //static unsafe ComplexFp32[] ConvertToComplexArray_Unsafe((float real, float imaginary)[] tupleArray)
    //{
    //    // 获取数组长度
    //    int length = tupleArray.Length;

    //    // 分配 ComplexFp32 数组
    //    ComplexFp32[] complexArray = new ComplexFp32[length];

    //    // 获取指针
    //    fixed ((float real, float imaginary)* pTuple = tupleArray)
    //    fixed (ComplexFp32* pComplex = complexArray)
    //    {
    //        // 将 tupleArray 的内存块复制到 complexArray
    //        Buffer.MemoryCopy(pTuple, pComplex, length * sizeof(ComplexFp32), length * sizeof(ComplexFp32));
    //    }

    //    return complexArray;
    //}
}
