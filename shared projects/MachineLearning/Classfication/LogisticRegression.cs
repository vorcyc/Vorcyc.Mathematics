using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;



namespace Vorcyc.Mathematics.MachineLearning.Classfication;



/// <summary>

/// 二分类逻辑回归，使用梯度下降拟合 sigmoid 模型。

/// </summary>

/// <typeparam name="T">浮点类型。</typeparam>

public class LogisticRegression<T> : IBatchClassifier<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private T[]? _weights;

    private T _bias;

    private bool _isFitted;



    /// <summary>

    /// 初始化逻辑回归模型。

    /// </summary>

    /// <param name="learningRate">学习率；为 default 时使用 0.01。</param>

    /// <param name="epochs">训练轮数。</param>

    /// <param name="lambda">L2 正则化系数。</param>

    /// <param name="batchSize">mini-batch 大小；≤0 表示全批量。</param>

    /// <param name="seed">mini-batch 洗牌种子；null 使用非确定性洗牌。</param>

    public LogisticRegression(

        T learningRate = default,

        int epochs = 1000,

        T lambda = default,

        int batchSize = 0,

        int? seed = null)

    {

        LearningRate = learningRate.Equals(default) ? T.CreateChecked(0.01) : learningRate;

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



    /// <summary>拟合后的权重。</summary>

    public IReadOnlyList<T> Weights =>

        _isFitted ? _weights! : throw new InvalidOperationException("模型尚未拟合。");



    /// <summary>拟合后的偏置。</summary>

    public T Bias => _isFitted ? _bias : throw new InvalidOperationException("模型尚未拟合。");



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Classification;



    /// <summary>

    /// 使用标签为 0 或 1 的数据拟合模型。

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



        _weights = new T[cols];

        _bias = T.Zero;



        var weightGrad = new T[cols];

        int batchSize = BatchSize <= 0 ? rows : Math.Min(BatchSize, rows);



        for (int epoch = 0; epoch < Epochs; epoch++)

        {

            var order = DataSplit.CreateShuffledIndices(rows, Seed.HasValue ? Seed.Value + epoch : null);



            for (int batchStart = 0; batchStart < rows; batchStart += batchSize)

            {

                int batchCount = Math.Min(batchSize, rows - batchStart);

                Array.Clear(weightGrad, 0, cols);

                T biasGrad = T.Zero;



                for (int b = 0; b < batchCount; b++)

                {

                    int i = order[batchStart + b];

                    T label = T.CreateChecked(y[i]);

                    T prediction = Sigmoid(_bias + NumericKernels.DotRow(x, i, _weights));

                    T error = prediction - label;



                    for (int j = 0; j < cols; j++)

                        weightGrad[j] += error * x[i, j];

                    biasGrad += error;

                }



                T invN = T.One / T.CreateChecked(batchCount);

                for (int j = 0; j < cols; j++)

                {

                    weightGrad[j] = weightGrad[j] * invN + Lambda * _weights[j];

                    _weights[j] -= LearningRate * weightGrad[j];

                }

                _bias -= LearningRate * (biasGrad * invN);

            }

        }



        _isFitted = true;

    }



    /// <summary>

    /// 预测属于正类 (1) 的概率。

    /// </summary>

    public T PredictProbability(T[] x)

    {

        if (!_isFitted)

            throw new InvalidOperationException("模型尚未拟合。");

        if (x == null || x.Length != _weights!.Length)

            throw new ArgumentException("特征维度与模型不匹配。", nameof(x));



        return Sigmoid(LinearScore(x));

    }



    /// <inheritdoc />

    int IClassifier<T>.Predict(T[] sample) => Predict(sample);



    /// <summary>

    /// 预测类别标签 (0 或 1)。

    /// </summary>

    public int Predict(T[] x, T threshold = default)

    {

        threshold = threshold.Equals(default) ? T.CreateChecked(0.5) : threshold;

        return PredictProbability(x) >= threshold ? 1 : 0;

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

        if (cols != _weights!.Length)

            throw new ArgumentException("特征维度与模型不匹配。");

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



        T threshold = T.CreateChecked(0.5);

        for (int i = 0; i < rows; i++)

        {

            T probability = Sigmoid(_bias + NumericKernels.DotRow(x, i, _weights!));

            predictions[i] = probability >= threshold ? 1 : 0;

        }

    }



    private T LinearScore(ReadOnlySpan<T> x) =>

        _bias + NumericKernels.Dot(_weights!, x);



    private static T Sigmoid(T z)

    {

        if (z >= T.Zero)

        {

            T expNegZ = T.Exp(-z);

            return T.One / (T.One + expNegZ);

        }



        T expZ = T.Exp(z);

        return expZ / (T.One + expZ);

    }

}


