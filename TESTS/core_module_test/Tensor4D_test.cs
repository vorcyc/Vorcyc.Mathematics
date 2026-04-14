using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.LinearAlgebra;

namespace core_module_test;

internal static class Tensor4D_test
{
    public static void Run()
    {
        TestIndexing();
        TestBatchTensorWrap();
        TestGetSampleBulkCopy();
        TestCopyChannelSingleChannel();
        TestTensorOpsIm2Col();
        TestGemmTransposeRight();
        Console.WriteLine("Tensor4D tests passed.");
    }

    static void TestIndexing()
    {
        var tensor = new Tensor4D<float>(2, 3, 4, 5);
        tensor[1, 2, 3, 4] = 42f;

        int expected = Tensor4D<float>.GetLinearIndex(tensor.Shape, 1, 2, 3, 4);
        if (tensor.Values[expected] != 42f)
            throw new InvalidOperationException("Tensor4D indexer failed.");

        if (tensor.Shape.ElementCount != 2 * 3 * 4 * 5)
            throw new InvalidOperationException("Tensor4DShape.ElementCount failed.");
    }

    static void TestBatchTensorWrap()
    {
        var batch = new BatchTensor<float>(2, 2, 2, 1);
        batch[1, 1, 1, 0] = 7f;

        var core = batch.AsTensor4D();
        if (core.Dim0 != 2 || core.Dim1 != 2 || core.Dim2 != 2 || core.Dim3 != 1)
            throw new InvalidOperationException("BatchTensor axis mapping failed.");

        if (core[1, 1, 1, 0] != 7f)
            throw new InvalidOperationException("BatchTensor should share Tensor4D storage.");

        var roundTrip = BatchTensor<float>.FromTensor4D(core);
        if (roundTrip[1, 1, 1, 0] != 7f)
            throw new InvalidOperationException("BatchTensor.FromTensor4D failed.");
    }

    static void TestGetSampleBulkCopy()
    {
        var batch = new BatchTensor<float>(2, 3, 4, 5);
        batch[0, 1, 2, 3] = 99f;
        batch[1, 2, 3, 4] = 55f;

        var sample0 = batch.GetSample(0);
        var sample1 = batch.GetSample(1);

        if (sample0[2, 1, 3] != 99f || sample1[3, 2, 4] != 55f)
            throw new InvalidOperationException("GetSample bulk copy failed.");

        var restored = new BatchTensor<float>(2, 3, 4, 5);
        restored.SetSample(0, sample0);
        restored.SetSample(1, sample1);

        if (restored[0, 1, 2, 3] != 99f || restored[1, 2, 3, 4] != 55f)
            throw new InvalidOperationException("SetSample bulk copy failed.");
    }

    static void TestCopyChannelSingleChannel()
    {
        var batch = new BatchTensor<float>(2, 2, 2, 1);
        batch.Values.Fill(1f);
        batch[0, 0, 0, 0] = 3f;
        batch[1, 1, 1, 0] = 9f;

        Span<float> channel = stackalloc float[8];
        batch.CopyChannelTo(0, channel);

        if (channel[0] != 3f || channel[^1] != 9f || channel[1] != 1f)
            throw new InvalidOperationException("CopyChannelTo fast path failed.");

        channel.Fill(2f);
        batch.CopyChannelFrom(0, channel);
        if (batch[0, 0, 0, 0] != 2f || batch[1, 1, 1, 0] != 2f)
            throw new InvalidOperationException("CopyChannelFrom fast path failed.");
    }

    static void TestTensorOpsIm2Col()
    {
        Span<float> input = [5f];
        Span<float> columns = stackalloc float[1];
        TensorOps.Im2ColNhwc(input, 1, 1, 1, 1, 1, 1, 1, 1, 1, columns);

        if (MathF.Abs(columns[0] - 5f) > 1e-6f)
            throw new InvalidOperationException("Im2ColNhwc failed.");

        Span<float> grad = stackalloc float[1];
        TensorOps.Col2ImNhwc(columns, 1, 1, 1, 1, 1, 1, 1, 1, 1, grad);
        if (MathF.Abs(grad[0] - 5f) > 1e-6f)
            throw new InvalidOperationException("Col2ImNhwc failed.");
    }

    static void TestGemmTransposeRight()
    {
        ReadOnlySpan<float> a = [1f, 2f, 3f, 4f];
        ReadOnlySpan<float> b = [5f, 6f, 7f, 8f, 9f, 10f];
        Span<float> output = stackalloc float[6];

        TensorOps.GemmTransposeRight(a, 2, 2, b, 3, output);

        float[] expected = [17f, 23f, 29f, 39f, 53f, 67f];
        for (int i = 0; i < expected.Length; i++)
        {
            if (MathF.Abs(output[i] - expected[i]) > 1e-5f)
                throw new InvalidOperationException("GemmTransposeRight failed.");
        }
    }
}
