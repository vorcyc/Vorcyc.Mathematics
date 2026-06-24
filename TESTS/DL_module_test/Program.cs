using DL_module_test;
using Vorcyc.Mathematics.DeepLearning.Layers;
using Vorcyc.Mathematics.LinearAlgebra;

namespace DL_module_test;

class Program
{
    static void Main(string[] args)
    {
        var passed = 0;
        var failed = 0;

        RunTest("Upsample2D_FloatTensor", TestUpsample2D_FloatTensor(), ref passed, ref failed);
        RunTest("Upsample2D_GenericTensor", TestUpsample2D_GenericTensor(), ref passed, ref failed);
        RunTest("XorTraining", XorTraining_test.Run(), ref passed, ref failed);
        RunTest("ClassificationTraining", ClassificationTraining_test.Run(), ref passed, ref failed);
        RunTest("BatchTraining", BatchTraining_test.Run(), ref passed, ref failed);
        RunTest("BatchNorm", BatchNorm_test.Run(), ref passed, ref failed);
        RunTest("ModelSerializer", ModelSerializer_test.Run(), ref passed, ref failed);
        RunTest("BatchCnn", BatchCnn_test.Run(), ref passed, ref failed);
        RunTest("BatchConvBackward", BatchConvBackward_test.Run(), ref passed, ref failed);
        RunTest("CnnMlpTraining", CnnMlp_training_test.Run(), ref passed, ref failed);
        RunTest("CnnMlpModelSerializer", CnnMlpModelSerializer_test.Run(), ref passed, ref failed);
        RunTest("BatchConvIm2Col", BatchConvIm2Col_test.Run(), ref passed, ref failed);
        RunTest("BatchFc", BatchFc_test.Run(), ref passed, ref failed);
        RunTest("BatchClassification", BatchClassification_test.Run(), ref passed, ref failed);
        RunTest("ModelArchitectureV3", ModelArchitectureV3_test.Run(), ref passed, ref failed);
        RunTest("ModelRebuild", ModelRebuild_test.Run(), ref passed, ref failed);
        RunTest("BatchSparseClassification", BatchSparseClassification_test.Run(), ref passed, ref failed);
        RunTest("BatchParallelConcatFit", BatchParallelConcatFit_test.Run(), ref passed, ref failed);
        RunTest("SignalBatchTensor", SignalBatchTensor_test.Run(), ref passed, ref failed);
        RunTest("SpectrogramBatchTraining", SpectrogramBatch_training_test.Run(), ref passed, ref failed);
        RunTest("FeatureBatchBuilder", FeatureBatchBuilder_test.Run(), ref passed, ref failed);
        RunTest("MfccMeanTraining", MfccMean_training_test.Run(), ref passed, ref failed);
        RunTest("AudioFrontendLayers", AudioFrontendLayers_test.Run(), ref passed, ref failed);
        RunTest("AudioFrontendTraining", AudioFrontend_training_test.Run(), ref passed, ref failed);
        RunTest("WaveformScalarRegression", WaveformScalarRegression_test.Run(), ref passed, ref failed);
        RunTest("ComputingContextEquivalence", ComputingContext_equivalence_test.Run(), ref passed, ref failed);

        Console.WriteLine($"Results: {passed} passed, {failed} failed.");
        Environment.ExitCode = failed == 0 ? 0 : 1;
    }

    static void RunTest(string name, bool ok, ref int passed, ref int failed)
    {
        Console.WriteLine($"{name}: {(ok ? "PASS" : "FAIL")}");
        if (ok) passed++;
        else failed++;
    }

    static bool TestUpsample2D_FloatTensor()
    {
        var inputArray = new float[,,]
        {
            { { 1 }, { 2 } },
            { { 3 }, { 4 } }
        };
        var inputTensor = new Tensor(inputArray);

        var expectedArray = new float[,,]
        {
            { { 1 }, { 1 }, { 2 }, { 2 } },
            { { 1 }, { 1 }, { 2 }, { 2 } },
            { { 3 }, { 3 }, { 4 }, { 4 } },
            { { 3 }, { 3 }, { 4 }, { 4 } }
        };
        var expectedTensor = new Tensor(expectedArray);
        var resultTensor = Layers.Upsample2D(inputTensor);
        return TensorsEqual(expectedTensor, resultTensor);
    }

    static bool TestUpsample2D_GenericTensor()
    {
        var inputArray = new double[,,]
        {
            { { 1.0 }, { 2.0 } },
            { { 3.0 }, { 4.0 } }
        };
        var inputTensor = new Tensor<double>(inputArray);

        var expectedArray = new double[,,]
        {
            { { 1.0 }, { 1.0 }, { 2.0 }, { 2.0 } },
            { { 1.0 }, { 1.0 }, { 2.0 }, { 2.0 } },
            { { 3.0 }, { 3.0 }, { 4.0 }, { 4.0 } },
            { { 3.0 }, { 3.0 }, { 4.0 }, { 4.0 } }
        };
        var expectedTensor = new Tensor<double>(expectedArray);
        var resultTensor = Layers.Upsample2D(inputTensor);
        return TensorsEqual(expectedTensor, resultTensor);
    }

    static bool TensorsEqual(Tensor expected, Tensor actual)
    {
        if (expected.Width != actual.Width || expected.Height != actual.Height || expected.Depth != actual.Depth)
        {
            return false;
        }

        for (int d = 0; d < expected.Depth; d++)
        {
            for (int y = 0; y < expected.Height; y++)
            {
                for (int x = 0; x < expected.Width; x++)
                {
                    if (expected[x, y, d] != actual[x, y, d])
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    static bool TensorsEqual<T>(Tensor<T> expected, Tensor<T> actual)
        where T : System.Numerics.IBinaryFloatingPointIeee754<T>
    {
        if (expected.Width != actual.Width || expected.Height != actual.Height || expected.Depth != actual.Depth)
        {
            return false;
        }

        for (int d = 0; d < expected.Depth; d++)
        {
            for (int y = 0; y < expected.Height; y++)
            {
                for (int x = 0; x < expected.Width; x++)
                {
                    if (!expected[x, y, d].Equals(actual[x, y, d]))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}
