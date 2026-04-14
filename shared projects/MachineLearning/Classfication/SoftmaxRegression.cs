using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;

using Vorcyc.Mathematics.MachineLearning.Serialization;



namespace Vorcyc.Mathematics.MachineLearning.Classfication;



/// <summary>

/// 多分类 Softmax 回归（多项逻辑回归）。

/// </summary>

public class SoftmaxRegression<T> : IBatchClassifier<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private T[][]? _weights;

    private T[]? _biases;

    private int _numClasses;

    private bool _isFitted;



    /// <summary>

    /// 初始化 Softmax 回归。

    /// </summary>

    /// <param name="learningRate">学习率；default 时为 0.05。</param>

    /// <param name="epochs">训练轮数。</param>

    /// <param name="lambda">L2 正则化系数。</param>

    /// <param name="batchSize">mini-batch 大小；≤0 表示全批量。</param>

    /// <param name="seed">mini-batch 洗牌种子；null 使用非确定性洗牌。</param>

    public SoftmaxRegression(

        T learningRate = default,

        int epochs = 1000,

        T lambda = default,

        int batchSize = 0,

        int? seed = null)

    {

        LearningRate = learningRate.Equals(default) ? T.CreateChecked(0.05) : learningRate;

        Epochs = epochs;

        Lambda = lambda.Equals(default) ? T.Zero : lambda;

        BatchSize = batchSize;

        Seed = seed;

    }



    /// <summary>学习率。</summary>

    public T LearningRate { get; }



    /// <summary>训练轮数。</summary>

    public int Epochs { get; }



    /// <summary>L2 正则化系数。</summary>

    public T Lambda { get; }



    /// <summary>Mini-batch 大小；≤0 为全批量梯度下降。</summary>

    public int BatchSize { get; }



    /// <summary>Mini-batch 洗牌种子。</summary>

    public int? Seed { get; }



    /// <summary>类别数。</summary>

    public int NumClasses =>

        _isFitted ? _numClasses : throw new InvalidOperationException("模型尚未拟合。");



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Classification;



    /// <summary>

    /// 拟合多分类模型。标签为非负整数，从 0 开始连续编号。

    /// </summary>

    public void Fit(T[,] x, int[] y)

    {

        if (x == null || y == null)

            throw new ArgumentException("输入不能为 null。");



        int rows = x.GetLength(0);

        int cols = x.GetLength(1);

        if (rows == 0 || cols == 0 || y.Length == 0)

            throw new ArgumentException("训练数据不能为空。");

        if (rows != y.Length)

            throw new ArgumentException("样本数与标签数不匹配。");



        _numClasses = y.Max() + 1;

        if (y.Min() < 0)

            throw new ArgumentException("标签必须为非负整数。");



        _weights = new T[_numClasses][];

        for (int k = 0; k < _numClasses; k++)

            _weights[k] = new T[cols];

        _biases = new T[_numClasses];



        var probabilities = new T[_numClasses];

        var weightGrads = new T[_numClasses][];

        for (int k = 0; k < _numClasses; k++)

            weightGrads[k] = new T[cols];

        var biasGrads = new T[_numClasses];

        var rowScratch = new T[cols];



        int batchSize = BatchSize <= 0 ? rows : Math.Min(BatchSize, rows);



        for (int epoch = 0; epoch < Epochs; epoch++)

        {

            var order = DataSplit.CreateShuffledIndices(rows, Seed.HasValue ? Seed.Value + epoch : null);



            for (int batchStart = 0; batchStart < rows; batchStart += batchSize)

            {

                int batchCount = Math.Min(batchSize, rows - batchStart);

                for (int k = 0; k < _numClasses; k++)

                    Array.Clear(weightGrads[k], 0, cols);

                Array.Clear(biasGrads, 0, _numClasses);



                for (int b = 0; b < batchCount; b++)

                {

                    int i = order[batchStart + b];

                    CopyRow(x, i, cols, rowScratch);

                    ComputeProbabilities(rowScratch, probabilities);

                    int label = y[i];



                    for (int k = 0; k < _numClasses; k++)

                    {

                        T target = k == label ? T.One : T.Zero;

                        T error = probabilities[k] - target;

                        NumericKernels.AddScaled(weightGrads[k], rowScratch, error);

                        biasGrads[k] += error;

                    }

                }



                T invN = T.One / T.CreateChecked(batchCount);

                for (int k = 0; k < _numClasses; k++)

                {

                    for (int j = 0; j < cols; j++)

                    {

                        weightGrads[k][j] = weightGrads[k][j] * invN + Lambda * _weights[k][j];

                        _weights[k][j] -= LearningRate * weightGrads[k][j];

                    }

                    _biases[k] -= LearningRate * (biasGrads[k] * invN);

                }

            }

        }



        _isFitted = true;

    }



    /// <summary>

    /// 预测类别。

    /// </summary>

    public int Predict(T[] x)

    {

        var probs = PredictProbabilities(x);

        return ClassificationMath.ArgMax(probs);

    }



    /// <inheritdoc />

    public void PredictBatch(T[,] x, Span<int> predictions)

    {

        if (!_isFitted)

            throw new InvalidOperationException("模型尚未拟合。");

        if (x == null)

            throw new ArgumentNullException(nameof(x));



        int rows = x.GetLength(0);

        int cols = x.GetLength(1);

        if (cols != _weights![0].Length)

            throw new ArgumentException("特征维度与模型不匹配。");

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



        var probabilities = new T[_numClasses];

        for (int i = 0; i < rows; i++)

        {

            ComputeProbabilitiesFromRow(x, i, probabilities);

            predictions[i] = ClassificationMath.ArgMax(probabilities);

        }

    }



    /// <summary>

    /// 返回各类别概率。

    /// </summary>

    public T[] PredictProbabilities(T[] x)

    {

        if (!_isFitted)

            throw new InvalidOperationException("模型尚未拟合。");

        if (x == null || x.Length != _weights![0].Length)

            throw new ArgumentException("特征维度与模型不匹配。", nameof(x));



        var probabilities = new T[_numClasses];

        ComputeProbabilities(x, probabilities);

        return probabilities;

    }



    /// <summary>

    /// 导出模型快照（double 精度）。

    /// </summary>

    public SoftmaxRegressionSnapshot CaptureSnapshot()

    {

        if (!_isFitted)

            throw new InvalidOperationException("模型尚未拟合。");



        var weights = new double[_numClasses][];

        for (int k = 0; k < _numClasses; k++)

            weights[k] = _weights![k].Select(v => double.CreateChecked(v)).ToArray();



        return new SoftmaxRegressionSnapshot

        {

            NumClasses = _numClasses,

            Weights = weights,

            Biases = _biases!.Select(double.CreateChecked).ToArray(),

            LearningRate = double.CreateChecked(LearningRate),

            Epochs = Epochs,

            Lambda = double.CreateChecked(Lambda)

        };

    }



    /// <summary>

    /// 从快照恢复模型参数（用于推理，不重新训练）。

    /// </summary>

    public void RestoreFromSnapshot(SoftmaxRegressionSnapshot snapshot)

    {

        _numClasses = snapshot.NumClasses;

        _weights = new T[_numClasses][];

        for (int k = 0; k < _numClasses; k++)

            _weights[k] = snapshot.Weights[k].Select(v => T.CreateChecked(v)).ToArray();

        _biases = snapshot.Biases.Select(v => T.CreateChecked(v)).ToArray();

        _isFitted = true;

    }



    private static void CopyRow<TNum>(TNum[,] matrix, int row, int cols, Span<TNum> destination)

        where TNum : struct

    {

        for (int j = 0; j < cols; j++)

            destination[j] = matrix[row, j];

    }



    private void ComputeProbabilities(ReadOnlySpan<T> x, Span<T> destination)

    {

        for (int k = 0; k < _numClasses; k++)

            destination[k] = NumericKernels.Dot(_weights![k], x) + _biases![k];

        StableProbabilities.Softmax(destination);

    }



    private void ComputeProbabilitiesFromRow(T[,] x, int row, Span<T> destination)

    {

        for (int k = 0; k < _numClasses; k++)

            destination[k] = NumericKernels.DotRow(x, row, _weights![k]) + _biases![k];

        StableProbabilities.Softmax(destination);

    }

}


