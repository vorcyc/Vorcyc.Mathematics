using Vorcyc.Mathematics;
using Vorcyc.Mathematics.DeepLearning;
using Vorcyc.Mathematics.DeepLearning.Integration;
using Vorcyc.Mathematics.DeepLearning.Integration.Frontends;
using Vorcyc.Mathematics.DeepLearning.Layers;
using Vorcyc.Mathematics.DeepLearning.Losses;
using Vorcyc.Mathematics.DeepLearning.Modules;
using Vorcyc.Mathematics.DeepLearning.Optimizers;
using Vorcyc.Mathematics.DeepLearning.Training;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.SignalProcessing.Signals;

namespace DL_module_test;

/// <summary>
/// Numerical equivalence: Normal vs Parallel/SIMD dispatch on identical weights and inputs.
/// </summary>
internal static class ComputingContext_equivalence_test
{
    private const float Tol = 1e-4f;
    private const float TrainTol = 5e-3f;

    public static bool Run()
    {
        var ok = true;
        ok &= Check("BatchNormLayer forward", BatchNormLayerForwardMatches);
        ok &= Check("BatchNormLayer backward", BatchNormLayerBackwardMatches);
        ok &= Check("BatchBatchNormLayer forward", BatchBatchNormLayerForwardMatches);
        ok &= Check("BatchBatchNormLayer backward", BatchBatchNormLayerBackwardMatches);
        ok &= Check("BatchConv2D forward", BatchConvForwardMatches);
        ok &= Check("BatchConv2D backward", BatchConvBackwardMatches);
        ok &= Check("BatchConv2D im2col forward", BatchConvIm2ColForwardMatches);
        ok &= Check("BatchConv2D im2col backward", BatchConvIm2ColBackwardMatches);
        ok &= Check("BatchMaxPool2D forward", BatchMaxPoolForwardMatches);
        ok &= Check("BatchMaxPool2D backward", BatchMaxPoolBackwardMatches);
        ok &= Check("BatchDepthwiseConv forward", BatchDepthwiseForwardMatches);
        ok &= Check("BatchDepthwiseConv backward", BatchDepthwiseBackwardMatches);
        ok &= Check("BatchTransposedConv forward", BatchTransposedConvForwardMatches);
        ok &= Check("BatchTransposedConv backward", BatchTransposedConvBackwardMatches);
        ok &= Check("BatchAvgPool2D backward", BatchAvgPoolBackwardMatches);
        ok &= Check("BatchGlobalAvgPool2D backward", BatchGlobalAvgPoolBackwardMatches);
        ok &= Check("BatchLayerNorm backward", BatchLayerNormBackwardMatches);
        ok &= Check("BatchSoftmax backward", BatchSoftmaxBackwardMatches);
        ok &= Check("BatchFullyConnected forward", BatchFullyConnectedForwardMatches);
        ok &= Check("BatchFullyConnected backward", BatchFullyConnectedBackwardMatches);
        ok &= Check("Legacy Conv2D", LegacyConv2DForwardMatches);
        ok &= Check("Legacy ReLU", LegacyReLUForwardMatches);
        ok &= Check("Legacy Dense", LegacyDenseForwardMatches);
        ok &= Check("Legacy MaxPool2D", LegacyMaxPoolForwardMatches);
        ok &= Check("Legacy BatchNorm", LegacyBatchNormForwardMatches);
        ok &= Check("PreEmphasis layer", PreEmphasisForwardMatches);
        ok &= Check("STFT layer", StftForwardMatches);
        ok &= Check("Mel filterbank layer", MelFilterbankForwardMatches);
        ok &= Check("Mel frontend stack", AudioFrontendForwardMatches);
        ok &= Check("MFCC parallel vs serial", MfccParallelMatchesSerial);
        ok &= Check("FitBatched reproducible", FitBatchedNormalIsReproducible);
        ok &= Check("FitBatched Normal vs Parallel", FitBatchedNormalVsParallel);
        ok &= Check("FitCnnMlp Normal vs Parallel", FitCnnMlpNormalVsParallel);
        ok &= Check("Fit scope vs explicit context", FitNormalVsExplicitContext);
        ok &= Check("FitBatchSequential context", FitBatchSequentialNormalVsParallel);
        return ok;
    }

    private static bool Check(string name, Func<bool> test)
    {
        if (test())
        {
            Console.WriteLine($"  [equiv] OK: {name}");
            return true;
        }

        Console.WriteLine($"  [equiv] FAIL: {name}");
        return false;
    }

    private static bool CompareForwardBatch(BatchLayerBase<float> a, BatchLayerBase<float> b, BatchTensor<float> input, float tol)
    {
        BatchTensor<float> normal, parallel;
        using (ComputingScope.Enter(ComputingContext.Normal))
            normal = a.Forward(input, training: true);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            parallel = b.Forward(input, training: true);
        return ComputingContext_assert.BatchTensorsClose(normal, parallel, tol, out _);
    }

    private static bool CompareBackwardBatch(BatchLayerBase<float> a, BatchLayerBase<float> b, BatchTensor<float> input, BatchTensor<float> gradOut, float tol)
    {
        using (ComputingScope.Enter(ComputingContext.Normal))
        {
            a.Forward(input, training: true);
        }

        using (ComputingScope.Enter(ComputingContext.Parallel))
        {
            b.Forward(input, training: true);
        }

        BatchTensor<float> gradInA, gradInB;
        using (ComputingScope.Enter(ComputingContext.Normal))
            gradInA = a.Backward(gradOut);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            gradInB = b.Backward(gradOut);

        return ComputingContext_assert.BatchTensorsClose(gradInA, gradInB, tol, out _)
            && ComputingContext_assert.GradientsClose(a.Parameters, b.Parameters, tol, out _);
    }

    private static bool BatchNormLayerForwardMatches()
    {
        var input = MakeFloatTensor(48, 48, 24);
        var (a, b) = (new BatchNormLayer<float>(24), new BatchNormLayer<float>(24));
        InitBatchNormSame(a, b);
        Tensor<float> normal, parallel;
        using (ComputingScope.Enter(ComputingContext.Normal))
            normal = a.Forward(input, training: true);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            parallel = b.Forward(input, training: true);
        return ComputingContext_assert.TensorsClose(normal, parallel, Tol, out _);
    }

    private static bool BatchNormLayerBackwardMatches()
    {
        var input = MakeFloatTensor(32, 32, 16);
        var gradOut = MakeFloatTensor(32, 32, 16);
        var (a, b) = (new BatchNormLayer<float>(16), new BatchNormLayer<float>(16));
        InitBatchNormSame(a, b);

        using (ComputingScope.Enter(ComputingContext.Normal))
            a.Forward(input, training: true);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            b.Forward(input, training: true);

        Tensor<float> gradA, gradB;
        using (ComputingScope.Enter(ComputingContext.Normal))
            gradA = a.Backward(gradOut);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            gradB = b.Backward(gradOut);

        return ComputingContext_assert.TensorsClose(gradA, gradB, Tol, out _)
            && ComputingContext_assert.GradientsClose(a.Parameters, b.Parameters, Tol, out _);
    }

    private static bool BatchBatchNormLayerForwardMatches()
    {
        var input = MakeBatchTensor(2, 32, 32, 16);
        var (a, b) = (new BatchBatchNormLayer<float>(16), new BatchBatchNormLayer<float>(16));
        InitBatchParametersSame(a, b);
        return CompareForwardBatch(a, b, input, Tol);
    }

    private static bool BatchBatchNormLayerBackwardMatches()
    {
        var input = MakeBatchTensor(2, 16, 16, 8);
        var gradOut = MakeBatchTensor(2, 16, 16, 8);
        var (a, b) = (new BatchBatchNormLayer<float>(8), new BatchBatchNormLayer<float>(8));
        InitBatchParametersSame(a, b);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool BatchConvForwardMatches()
    {
        var input = MakeBatchTensor(2, 8, 8, 3);
        var (a, b) = (
            new BatchConvolution2DLayer<float>(3, 4, kernelSize: 3),
            new BatchConvolution2DLayer<float>(3, 4, kernelSize: 3));
        InitBatchParametersSame(a, b);
        return CompareForwardBatch(a, b, input, Tol);
    }

    private static bool BatchConvBackwardMatches()
    {
        var input = MakeBatchTensor(2, 5, 5, 2);
        var (a, b) = (
            new BatchConvolution2DLayer<float>(2, 3, kernelSize: 3),
            new BatchConvolution2DLayer<float>(2, 3, kernelSize: 3));
        InitBatchParametersSame(a, b);
        var output = a.Forward(input, training: true);
        var gradOut = MakeBatchTensor(output.Batch, output.Height, output.Width, output.Channels);
        FillPattern(gradOut.Values, 0.11f);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool BatchConvIm2ColForwardMatches()
    {
        var input = MakeBatchTensor(2, 10, 10, 2);
        var (a, b) = (
            new BatchConvolution2DLayer<float>(2, 3, kernelSize: 5),
            new BatchConvolution2DLayer<float>(2, 3, kernelSize: 5));
        InitBatchParametersSame(a, b);
        return CompareForwardBatch(a, b, input, Tol);
    }

    private static bool BatchConvIm2ColBackwardMatches()
    {
        var input = MakeBatchTensor(2, 8, 8, 2);
        var (a, b) = (
            new BatchConvolution2DLayer<float>(2, 2, kernelSize: 5),
            new BatchConvolution2DLayer<float>(2, 2, kernelSize: 5));
        InitBatchParametersSame(a, b);
        var output = a.Forward(input, training: true);
        var gradOut = MakeBatchTensor(output.Batch, output.Height, output.Width, output.Channels);
        FillPattern(gradOut.Values, 0.09f);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool BatchMaxPoolForwardMatches()
    {
        var input = MakeBatchTensor(4, 8, 8, 3);
        var (a, b) = (new BatchMaxPool2DLayer<float>(), new BatchMaxPool2DLayer<float>());
        return CompareForwardBatch(a, b, input, Tol);
    }

    private static bool BatchDepthwiseForwardMatches()
    {
        var input = MakeBatchTensor(3, 12, 12, 4);
        var (a, b) = (
            new BatchDepthwiseConvolution2DLayer<float>(4, kernelSize: 3),
            new BatchDepthwiseConvolution2DLayer<float>(4, kernelSize: 3));
        InitBatchParametersSame(a, b);
        return CompareForwardBatch(a, b, input, Tol);
    }

    private static bool BatchMaxPoolBackwardMatches()
    {
        var input = MakeBatchTensor(4, 8, 8, 3);
        var (a, b) = (new BatchMaxPool2DLayer<float>(), new BatchMaxPool2DLayer<float>());
        var output = a.Forward(input, training: true);
        var gradOut = MakeBatchTensor(output.Batch, output.Height, output.Width, output.Channels);
        FillPattern(gradOut.Values, 0.13f);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool BatchDepthwiseBackwardMatches()
    {
        var input = MakeBatchTensor(3, 10, 10, 4);
        var (a, b) = (
            new BatchDepthwiseConvolution2DLayer<float>(4, kernelSize: 3),
            new BatchDepthwiseConvolution2DLayer<float>(4, kernelSize: 3));
        InitBatchParametersSame(a, b);
        var output = a.Forward(input, training: true);
        var gradOut = MakeBatchTensor(output.Batch, output.Height, output.Width, output.Channels);
        FillPattern(gradOut.Values, 0.08f);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool BatchTransposedConvForwardMatches()
    {
        var input = MakeBatchTensor(2, 6, 6, 2);
        var (a, b) = (
            new BatchTransposedConvolution2DLayer<float>(2, 3, kernelSize: 3, stride: 2),
            new BatchTransposedConvolution2DLayer<float>(2, 3, kernelSize: 3, stride: 2));
        InitBatchParametersSame(a, b);
        return CompareForwardBatch(a, b, input, Tol);
    }

    private static bool BatchTransposedConvBackwardMatches()
    {
        var input = MakeBatchTensor(2, 5, 5, 2);
        var (a, b) = (
            new BatchTransposedConvolution2DLayer<float>(2, 3, kernelSize: 3, stride: 2),
            new BatchTransposedConvolution2DLayer<float>(2, 3, kernelSize: 3, stride: 2));
        InitBatchParametersSame(a, b);
        var output = a.Forward(input, training: true);
        var gradOut = MakeBatchTensor(output.Batch, output.Height, output.Width, output.Channels);
        FillPattern(gradOut.Values, 0.06f);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool BatchAvgPoolBackwardMatches()
    {
        var input = MakeBatchTensor(4, 8, 8, 3);
        var (a, b) = (new BatchAvgPool2DLayer<float>(), new BatchAvgPool2DLayer<float>());
        var output = a.Forward(input, training: true);
        var gradOut = MakeBatchTensor(output.Batch, output.Height, output.Width, output.Channels);
        FillPattern(gradOut.Values, 0.12f);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool BatchGlobalAvgPoolBackwardMatches()
    {
        var input = MakeBatchTensor(4, 7, 7, 5);
        var (a, b) = (new BatchGlobalAveragePool2DLayer<float>(), new BatchGlobalAveragePool2DLayer<float>());
        var output = a.Forward(input, training: true);
        var gradOut = MakeBatchTensor(output.Batch, output.Height, output.Width, output.Channels);
        FillPattern(gradOut.Values, 0.21f);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool BatchLayerNormBackwardMatches()
    {
        var input = MakeBatchTensor(4, 6, 6, 8);
        var (a, b) = (new BatchLayerNormLayer<float>(8), new BatchLayerNormLayer<float>(8));
        InitBatchParametersSame(a, b);
        var output = a.Forward(input, training: true);
        var gradOut = MakeBatchTensor(output.Batch, output.Height, output.Width, output.Channels);
        FillPattern(gradOut.Values, 0.05f);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool BatchSoftmaxBackwardMatches()
    {
        var input = MakeBatchTensor(4, 6, 6, 5);
        var (a, b) = (new BatchSoftmaxLayer<float>(), new BatchSoftmaxLayer<float>());
        var output = a.Forward(input, training: true);
        var gradOut = MakeBatchTensor(output.Batch, output.Height, output.Width, output.Channels);
        FillPattern(gradOut.Values, 0.17f);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool BatchFullyConnectedForwardMatches()
    {
        var batchInput = MakeBatchTensor(32, 1, 1, 8);
        var (a, b) = (
            new BatchFullyConnectedLayer<float>(8, 5),
            new BatchFullyConnectedLayer<float>(8, 5));
        InitBatchParametersSame(a, b);
        return CompareForwardBatch(a, b, batchInput, Tol);
    }

    private static bool BatchFullyConnectedBackwardMatches()
    {
        var input = MakeBatchTensor(16, 1, 1, 6);
        var (a, b) = (
            new BatchFullyConnectedLayer<float>(6, 4),
            new BatchFullyConnectedLayer<float>(6, 4));
        InitBatchParametersSame(a, b);
        var output = a.Forward(input, training: true);
        var gradOut = MakeBatchTensor(output.Batch, output.Height, output.Width, output.Channels);
        FillPattern(gradOut.Values, 0.07f);
        return CompareBackwardBatch(a, b, input, gradOut, Tol);
    }

    private static bool LegacyConv2DForwardMatches()
    {
        var input = MakeLegacyTensor(16, 16, 4);
        var filters = new Tensor[3];
        for (int i = 0; i < filters.Length; i++)
        {
            filters[i] = MakeLegacyTensor(3, 3, 4);
            FillPattern(filters[i].Values, 0.07f * (i + 1));
        }

        var bias = MakeLegacyTensor(1, 1, 3);
        FillPattern(bias.Values, 0.02f);

        Tensor normal, parallel;
        using (ComputingScope.Enter(ComputingContext.Normal))
            normal = Layers.Conv2D(input, filters, bias);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            parallel = Layers.Conv2D(input, filters, bias);
        return ComputingContext_assert.TensorsClose(normal, parallel, Tol, out _);
    }

    private static bool LegacyReLUForwardMatches()
    {
        var input = MakeLegacyTensor(8, 8, 6);
        Tensor normal, parallel;
        using (ComputingScope.Enter(ComputingContext.Normal))
            normal = Layers.ReLU(input);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            parallel = Layers.ReLU(input);
        return ComputingContext_assert.TensorsClose(normal, parallel, Tol, out _);
    }

    private static bool LegacyDenseForwardMatches()
    {
        var input = MakeLegacyTensor(4, 4, 3);
        var weights = new Tensor[2];
        weights[0] = MakeLegacyTensor(1, 1, 3 * 4 * 4);
        weights[1] = MakeLegacyTensor(1, 1, 3 * 4 * 4);
        FillPattern(weights[0].Values, 0.01f);
        FillPattern(weights[1].Values, 0.02f);
        var bias = MakeLegacyTensor(1, 1, 2);
        FillPattern(bias.Values, 0.03f);

        Tensor normal, parallel;
        using (ComputingScope.Enter(ComputingContext.Normal))
            normal = Layers.Dense(input, weights, bias);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            parallel = Layers.Dense(input, weights, bias);
        return ComputingContext_assert.TensorsClose(normal, parallel, Tol, out _);
    }

    private static bool LegacyMaxPoolForwardMatches()
    {
        var input = MakeLegacyTensor(8, 8, 4);
        Tensor normal, parallel;
        using (ComputingScope.Enter(ComputingContext.Normal))
            normal = Layers.MaxPool2D(input);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            parallel = Layers.MaxPool2D(input);
        return ComputingContext_assert.TensorsClose(normal, parallel, Tol, out _);
    }

    private static bool LegacyBatchNormForwardMatches()
    {
        var input = MakeLegacyTensor(12, 12, 8);
        var mean = MakeLegacyTensor(1, 1, 8);
        var variance = MakeLegacyTensor(1, 1, 8);
        var shift = MakeLegacyTensor(1, 1, 8);
        var scale = MakeLegacyTensor(1, 1, 8);
        FillPattern(mean.Values, 0.1f);
        FillPattern(variance.Values, 0.5f);
        FillPattern(shift.Values, 0.01f);
        FillPattern(scale.Values, 1.1f);

        Tensor normal, parallel;
        using (ComputingScope.Enter(ComputingContext.Normal))
            normal = Layers.BatchNorm(input, mean, variance, shift, scale);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            parallel = Layers.BatchNorm(input, mean, variance, shift, scale);
        return ComputingContext_assert.TensorsClose(normal, parallel, Tol, out _);
    }

    private static bool PreEmphasisForwardMatches()
    {
        const float rate = 8000f;
        var input = BatchTensorSignalExtensions.FromSignal(MakeSine(512, rate, 300f));
        var layer = new BatchPreEmphasisLayer(0.97f);
        BatchTensor<float> normal, parallel;
        using (ComputingScope.Enter(ComputingContext.Normal))
            normal = layer.Forward(input);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            parallel = layer.Forward(input);
        return ComputingContext_assert.BatchTensorsClose(normal, parallel, Tol, out _);
    }

    private static bool StftForwardMatches()
    {
        const float rate = 8000f;
        var input = BatchTensorSignalExtensions.FromSignal(MakeSine(1024, rate, 440f));
        var layer = new BatchStftMagnitudeLayer(256, 128);
        BatchTensor<float> normal, parallel;
        using (ComputingScope.Enter(ComputingContext.Normal))
            normal = layer.Forward(input);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            parallel = layer.Forward(input);
        return ComputingContext_assert.BatchTensorsClose(normal, parallel, Tol, out _);
    }

    private static bool MelFilterbankForwardMatches()
    {
        const float rate = 8000f;
        var stft = new BatchStftMagnitudeLayer(256, 128);
        var input = BatchTensorSignalExtensions.FromSignal(MakeSine(1024, rate, 500f));
        var spectral = stft.Forward(input);
        var stack = AudioFrontendLayers.CreateMelSpectrogramStack((int)rate, 256, 128, 16);
        var melLayer = (BatchMelFilterbankLayer)stack[2];

        BatchTensor<float> normal, parallel;
        using (ComputingScope.Enter(ComputingContext.Normal))
            normal = melLayer.Forward(spectral);
        using (ComputingScope.Enter(ComputingContext.Parallel))
            parallel = melLayer.Forward(spectral);
        return ComputingContext_assert.BatchTensorsClose(normal, parallel, Tol, out _);
    }

    private static bool AudioFrontendForwardMatches()
    {
        const float rate = 8000f;
        var input = BatchTensorSignalExtensions.FromSignal(MakeSine(2048, rate, 440f));
        var stack = AudioFrontendLayers.CreateMelSpectrogramStack((int)rate, 256, 128, 16);

        BatchTensor<float> RunStack(ComputingContext context)
        {
            using (ComputingScope.Enter(context))
            {
                var t = input;
                foreach (var layer in stack)
                {
                    t = layer.Forward(t);
                }

                return t;
            }
        }

        return ComputingContext_assert.BatchTensorsClose(
            RunStack(ComputingContext.Normal),
            RunStack(ComputingContext.Parallel),
            Tol,
            out _);
    }

    private static bool MfccParallelMatchesSerial()
    {
        const float rate = 8000f;
        var samples = MakeSine(8192, rate, 330f).Samples.ToArray();
        var extractor = AudioTrainingSamples.CreateDefaultMfccExtractor((int)rate, 13);
        var serial = extractor.ComputeFrom(samples);

        List<float[]> parallelNormal = extractor.ParallelComputeFrom(samples, context: ComputingContext.Normal);
        List<float[]> parallelFast = extractor.ParallelComputeFrom(samples, context: ComputingContext.Parallel);

        return ComputingContext_assert.FeatureListsClose(serial, parallelNormal, Tol, out _)
            && ComputingContext_assert.FeatureListsClose(serial, parallelFast, Tol, out _);
    }

    private static bool FitBatchedNormalIsReproducible()
    {
        var sample = MakeTrainingBatch();
        var modelA = CreateBnSequential();
        var modelB = CreateBnSequential();
        InitSequentialSame(modelA, modelB);

        var trainer = new Trainer<float>();
        trainer.FitBatched(modelA, new MeanSquaredErrorLoss<float>(), new SgdOptimizer<float>(0.01f),
            [sample], epochs: 50, computingContext: ComputingContext.Normal);
        trainer.FitBatched(modelB, new MeanSquaredErrorLoss<float>(), new SgdOptimizer<float>(0.01f),
            [sample], epochs: 50, computingContext: ComputingContext.Normal);

        return ComputingContext_assert.TensorsClose(
            modelA.Forward(sample.Input, training: false),
            modelB.Forward(sample.Input, training: false),
            Tol,
            out _);
    }

    private static bool FitBatchedNormalVsParallel()
    {
        var sample = MakeTrainingBatch();
        var modelNormal = CreateBnSequential();
        var modelParallel = CreateBnSequential();
        InitSequentialSame(modelNormal, modelParallel);

        var trainer = new Trainer<float>();
        trainer.FitBatched(modelNormal, new MeanSquaredErrorLoss<float>(), new SgdOptimizer<float>(0.01f),
            [sample], epochs: 80, computingContext: ComputingContext.Normal);
        trainer.FitBatched(modelParallel, new MeanSquaredErrorLoss<float>(), new SgdOptimizer<float>(0.01f),
            [sample], epochs: 80, computingContext: ComputingContext.Parallel);

        return ComputingContext_assert.TensorsClose(
            modelNormal.Forward(sample.Input, training: false),
            modelParallel.Forward(sample.Input, training: false),
            TrainTol,
            out _);
    }

    private static bool FitCnnMlpNormalVsParallel()
    {
        var batch = MakeCnnBatch();
        var targets = TensorUtilities.FromBatchVectors([1f, 0f], batchSize: 2, features: 1);
        var sample = new BatchTrainingSample<float>(batch, targets);

        var modelNormal = CreateCnnMlpModel();
        var modelParallel = CreateCnnMlpModel();
        InitCnnMlpSame(modelNormal, modelParallel);

        var trainer = new Trainer<float>();
        trainer.FitCnnMlp(modelNormal, new MeanSquaredErrorLoss<float>(), new AdamOptimizer<float>(0.05f),
            [sample], epochs: 200, computingContext: ComputingContext.Normal);
        trainer.FitCnnMlp(modelParallel, new MeanSquaredErrorLoss<float>(), new AdamOptimizer<float>(0.05f),
            [sample], epochs: 200, computingContext: ComputingContext.Parallel);

        var outNormal = modelNormal.Forward(batch, training: false);
        var outParallel = modelParallel.Forward(batch, training: false);
        return ComputingContext_assert.TensorsClose(outNormal, outParallel, TrainTol, out _);
    }

    private static bool FitNormalVsExplicitContext()
    {
        var dataset = new[]
        {
            new TrainingSample<float>(TensorUtilities.FromVector(0.2f, 0.3f), TensorUtilities.FromVector(0.5f)),
            new TrainingSample<float>(TensorUtilities.FromVector(0.7f, 0.1f), TensorUtilities.FromVector(0.2f)),
        };

        var modelScope = CreateSmallSequential();
        var modelParam = CreateSmallSequential();
        InitSequentialSame(modelScope, modelParam);

        var trainer = new Trainer<float>();
        using (ComputingScope.Enter(ComputingContext.Normal))
            trainer.Fit(modelScope, new MeanSquaredErrorLoss<float>(), new SgdOptimizer<float>(0.05f),
                dataset, epochs: 30, shuffle: false);
        trainer.Fit(modelParam, new MeanSquaredErrorLoss<float>(), new SgdOptimizer<float>(0.05f),
            dataset, epochs: 30, shuffle: false, computingContext: ComputingContext.Normal);

        return ComputingContext_assert.TensorsClose(
            modelScope.Forward(dataset[0].Input, training: false),
            modelParam.Forward(dataset[0].Input, training: false),
            Tol,
            out _);
    }

    private static bool FitBatchSequentialNormalVsParallel()
    {
        const float rate = 8000f;
        const int featureCount = 13;
        var low = MakeSine(1024, rate, 180f);
        var high = MakeSine(1024, rate, 1400f);
        var mfcc = AudioTrainingSamples.CreateDefaultMfccExtractor((int)rate, featureCount);
        var sample = AudioTrainingSamples.FeatureMeanClassification(
            [low, high], classIndices: [0, 1], numClasses: 2, extractor: mfcc);

        var modelNormal = new BatchSequential<float>(
            new BatchFullyConnectedLayer<float>(featureCount, 8),
            new BatchReLUActivation<float>(),
            new BatchFullyConnectedLayer<float>(8, 2));
        var modelParallel = new BatchSequential<float>(
            new BatchFullyConnectedLayer<float>(featureCount, 8),
            new BatchReLUActivation<float>(),
            new BatchFullyConnectedLayer<float>(8, 2));
        InitBatchSequentialSame(modelNormal, modelParallel);

        var trainer = new Trainer<float>();
        trainer.FitBatchSequential(modelNormal, new BatchCategoricalCrossEntropyLoss<float>(), new AdamOptimizer<float>(0.05f),
            [sample], epochs: 100, computingContext: ComputingContext.Normal);
        trainer.FitBatchSequential(modelParallel, new BatchCategoricalCrossEntropyLoss<float>(), new AdamOptimizer<float>(0.05f),
            [sample], epochs: 100, computingContext: ComputingContext.Parallel);

        return ComputingContext_assert.BatchTensorsClose(
            modelNormal.Forward(sample.Input, training: false),
            modelParallel.Forward(sample.Input, training: false),
            TrainTol,
            out _);
    }

    private static CnnMlpModel<float> CreateCnnMlpModel()
    {
        var backbone = new BatchSequential<float>(
            new BatchConvolution2DLayer<float>(1, 4, kernelSize: 3),
            new BatchReLUActivation<float>(),
            new BatchMaxPool2DLayer<float>(),
            new BatchFlattenLayer<float>());
        var head = new Sequential<float>(
            new FullyConnectedLayer<float>(16, 4),
            new SigmoidActivation<float>(),
            new FullyConnectedLayer<float>(4, 1),
            new SigmoidActivation<float>());
        return new CnnMlpModel<float>(backbone, head);
    }

    private static BatchTensor<float> MakeCnnBatch()
    {
        var batch = new BatchTensor<float>(2, 4, 4, 1);
        for (int n = 0; n < 2; n++)
        {
            for (int h = 0; h < 4; h++)
            {
                for (int w = 0; w < 4; w++)
                {
                    bool left = w < 2;
                    batch[n, h, w, 0] = (n == 0 ? left : !left) ? 1f : 0f;
                }
            }
        }

        return batch;
    }

    private static void InitCnnMlpSame(CnnMlpModel<float> a, CnnMlpModel<float> b)
    {
        for (int i = 0; i < a.Parameters.Count; i++)
        {
            FillPattern(a.Parameters[i].Value.Values, 0.04f + i * 0.001f);
            a.Parameters[i].Value.Values.CopyTo(b.Parameters[i].Value.Values);
        }
    }

    private static void InitBatchSequentialSame(BatchSequential<float> a, BatchSequential<float> b)
    {
        for (int i = 0; i < a.Parameters.Count; i++)
        {
            FillPattern(a.Parameters[i].Value.Values, 0.06f);
            a.Parameters[i].Value.Values.CopyTo(b.Parameters[i].Value.Values);
        }
    }

    private static Sequential<float> CreateBnSequential() => new(
        new FullyConnectedLayer<float>(4, 8),
        new BatchNormLayer<float>(8),
        new ReLUActivation<float>(),
        new FullyConnectedLayer<float>(8, 2));

    private static Sequential<float> CreateSmallSequential() => new(
        new FullyConnectedLayer<float>(2, 4),
        new ReLUActivation<float>(),
        new FullyConnectedLayer<float>(4, 1));

    private static TrainingSample<float> MakeTrainingBatch()
    {
        var inputs = TensorUtilities.FromBatchVectors(
            Enumerable.Range(0, 16 * 4).Select(i => (float)Math.Sin(i * 0.13f)).ToArray(),
            batchSize: 16,
            features: 4);
        var targets = TensorUtilities.FromBatchVectors(
            Enumerable.Range(0, 16 * 2).Select(i => (float)Math.Cos(i * 0.07f)).ToArray(),
            batchSize: 16,
            features: 2);
        return new TrainingSample<float>(inputs, targets);
    }

    private static Signal MakeSine(int length, float rate, float frequency)
    {
        var signal = new Signal(length, rate);
        signal.GenerateWave(WaveShape.Sine, frequency, Behaviour.Replace);
        return signal;
    }

    private static Tensor<float> MakeFloatTensor(int width, int height, int depth)
    {
        var t = new Tensor<float>(width, height, depth);
        FillPattern(t.Values, 0.03f);
        return t;
    }

    private static Tensor MakeLegacyTensor(int width, int height, int depth)
    {
        var t = new Tensor(width, height, depth);
        FillPattern(t.Values, 0.03f);
        return t;
    }

    private static BatchTensor<float> MakeBatchTensor(int batch, int height, int width, int channels)
    {
        var t = new BatchTensor<float>(batch, height, width, channels);
        FillPattern(t.Values, 0.04f);
        return t;
    }

    private static void FillPattern(Span<float> values, float scale)
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = scale * (i % 17 - 8);
        }
    }

    private static void InitBatchNormSame(BatchNormLayer<float> a, BatchNormLayer<float> b)
    {
        InitBatchNormSame(a.Scale.Value, a.Shift.Value, b.Scale.Value, b.Shift.Value);
        a.RunningMean.Fill(0f);
        a.RunningVariance.Fill(1f);
        b.RunningMean.Fill(0f);
        b.RunningVariance.Fill(1f);
    }

    private static void InitBatchNormSame(Tensor<float> scaleA, Tensor<float> shiftA, Tensor<float> scaleB, Tensor<float> shiftB)
    {
        scaleA.Fill(1.05f);
        shiftA.Fill(0.02f);
        scaleB.Fill(1.05f);
        shiftB.Fill(0.02f);
    }

    private static void InitBatchParametersSame(BatchLayerBase<float> a, BatchLayerBase<float> b)
    {
        for (int i = 0; i < a.Parameters.Count; i++)
        {
            var va = a.Parameters[i].Value.Values;
            var vb = b.Parameters[i].Value.Values;
            for (int j = 0; j < va.Length; j++)
            {
                vb[j] = va[j] = 0.02f * (j % 11 + 1);
            }
        }
    }

    private static void InitBatchParametersSame(BatchBatchNormLayer<float> a, BatchBatchNormLayer<float> b)
    {
        InitBatchParametersSame((BatchLayerBase<float>)a, (BatchLayerBase<float>)b);
        a.RunningMean.Fill(0f);
        a.RunningVariance.Fill(1f);
        b.RunningMean.Fill(0f);
        b.RunningVariance.Fill(1f);
    }

    private static void InitSequentialSame(Sequential<float> a, Sequential<float> b)
    {
        for (int i = 0; i < a.Parameters.Count; i++)
        {
            FillPattern(a.Parameters[i].Value.Values, 0.05f);
            a.Parameters[i].Value.Values.CopyTo(b.Parameters[i].Value.Values);
        }
    }
}
