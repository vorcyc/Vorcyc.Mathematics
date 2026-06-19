using BenchmarkDotNet.Attributes;
using Vorcyc.Mathematics.MachineLearning;
using Vorcyc.Mathematics.MachineLearning.Classfication;
using Vorcyc.Mathematics.MachineLearning.Regression;

namespace basic_benchmark;

/// <summary>
/// 瀵规瘮 ML 妯″潡閫愭牱鏈?Predict 涓?PredictBatch 鎵归噺璺緞銆?
/// 杩愯锛氬湪 Program.cs 涓彇娑堟敞閲?BenchmarkRunner.Run&lt;MachineLearningBatchBenchmark&gt;();
/// </summary>
[MemoryDiagnoser]
[WarmupCount(2)]
[IterationCount(8)]
public class MachineLearningBatchBenchmark
{
    [Params(256, 1024)]
    public int QueryRows { get; set; }

    private double[,] _xTrain = null!;
    private int[] _yTrain = null!;
    private double[,] _xQuery = null!;
    private double[,] _xRegTrain = null!;
    private double[] _yRegTrain = null!;
    private double[,] _xRegQuery = null!;

    private SoftmaxRegression<double> _softmax = null!;
    private NumericRandomForest<double> _forest = null!;
    private KnnClassifier<double> _knn = null!;
    private MultipleLinearRegression<double> _mlr = null!;
    private KnnRegressor<double> _knnReg = null!;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(17);
        int trainRows = 600;
        int cols = 8;
        int classes = 3;

        _xTrain = new double[trainRows, cols];
        _yTrain = new int[trainRows];
        for (int i = 0; i < trainRows; i++)
        {
            int label = i % classes;
            _yTrain[i] = label;
            for (int j = 0; j < cols; j++)
                _xTrain[i, j] = label * 2.0 + random.NextDouble() + j * 0.1;
        }

        _xQuery = new double[QueryRows, cols];
        for (int i = 0; i < QueryRows; i++)
            for (int j = 0; j < cols; j++)
                _xQuery[i, j] = random.NextDouble() * classes * 2.0;

        _softmax = new SoftmaxRegression<double>(learningRate: 0.1, epochs: 200);
        _softmax.Fit(_xTrain, _yTrain);

        _forest = new NumericRandomForest<double>(numTrees: 20, maxFeatures: 4, maxDepth: 8, seed: 3);
        _forest.Fit(_xTrain, _yTrain);

        _knn = new KnnClassifier<double>(k: 5);
        _knn.Fit(_xTrain, _yTrain);

        _xRegTrain = new double[trainRows, cols];
        _yRegTrain = new double[trainRows];
        for (int i = 0; i < trainRows; i++)
        {
            double target = 1.0;
            for (int j = 0; j < cols; j++)
            {
                _xRegTrain[i, j] = random.NextDouble() * 5.0;
                target += (j + 1) * 0.2 * _xRegTrain[i, j];
            }
            _yRegTrain[i] = target;
        }

        _xRegQuery = new double[QueryRows, cols];
        for (int i = 0; i < QueryRows; i++)
            for (int j = 0; j < cols; j++)
                _xRegQuery[i, j] = random.NextDouble() * 5.0;

        _mlr = new MultipleLinearRegression<double>();
        _mlr.Fit(_xRegTrain, _yRegTrain);

        _knnReg = new KnnRegressor<double>(k: 7);
        _knnReg.Fit(_xRegTrain, _yRegTrain);
    }

    [Benchmark(Baseline = true)]
    public int[] Softmax_RowPredict()
    {
        int rows = _xQuery.GetLength(0);
        int cols = _xQuery.GetLength(1);
        var preds = new int[rows];
        var sample = new double[cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                sample[j] = _xQuery[i, j];
            preds[i] = _softmax.Predict(sample);
        }
        return preds;
    }

    [Benchmark]
    public int[] Softmax_PredictBatch() => _softmax.PredictBatch(_xQuery);

    [Benchmark]
    public int[] Forest_PredictBatch() => _forest.PredictBatch(_xQuery);

    [Benchmark]
    public int[] KnnClassifier_PredictBatch() => _knn.PredictBatch(_xQuery);

    [Benchmark]
    public double[] Mlr_PredictBatch() => _mlr.PredictBatch(_xRegQuery);

    [Benchmark]
    public double[] KnnRegressor_PredictBatch() => _knnReg.PredictBatch(_xRegQuery);
}
