using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;



namespace Vorcyc.Mathematics.MachineLearning.Classfication;



/// <summary>

/// 高斯朴素贝叶斯分类器，适用于连续数值特征。

/// </summary>

public class GaussianNaiveBayes<T> : IBatchClassifier<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private readonly T _varianceFloor;

    private int[] _classLabels = [];

    private T[] _classPriors = [];

    private T[][] _classMeans = [];

    private T[][] _classVariances = [];

    private bool _isFitted;



    /// <summary>

    /// 初始化高斯朴素贝叶斯。

    /// </summary>

    /// <param name="varianceFloor">方差下界，避免除零。</param>

    public GaussianNaiveBayes(T varianceFloor = default)

    {

        _varianceFloor = varianceFloor.Equals(default) ? T.CreateChecked(1e-6) : varianceFloor;

    }



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Classification;



    /// <summary>

    /// 拟合模型。标签为非负整数。

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



        var classIndices = new Dictionary<int, List<int>>();

        for (int i = 0; i < rows; i++)

        {

            if (!classIndices.TryGetValue(y[i], out var list))

            {

                list = [];

                classIndices[y[i]] = list;

            }

            list.Add(i);

        }



        int numClasses = classIndices.Count;

        _classLabels = new int[numClasses];

        _classPriors = new T[numClasses];

        _classMeans = new T[numClasses][];

        _classVariances = new T[numClasses][];



        int classIndex = 0;

        foreach (var (classLabel, indices) in classIndices.OrderBy(kv => kv.Key))

        {

            _classLabels[classIndex] = classLabel;

            _classPriors[classIndex] = T.CreateChecked(indices.Count) / T.CreateChecked(rows);



            var means = new T[cols];

            var variances = new T[cols];



            for (int j = 0; j < cols; j++)

            {

                T sum = T.Zero;

                foreach (int i in indices)

                    sum += x[i, j];

                means[j] = sum / T.CreateChecked(indices.Count);

            }



            for (int j = 0; j < cols; j++)

            {

                T varSum = T.Zero;

                foreach (int i in indices)

                {

                    T diff = x[i, j] - means[j];

                    varSum += diff * diff;

                }

                T variance = varSum / T.CreateChecked(indices.Count);

                variances[j] = variance < _varianceFloor ? _varianceFloor : variance;

            }



            _classMeans[classIndex] = means;

            _classVariances[classIndex] = variances;

            classIndex++;

        }



        _isFitted = true;

    }



    /// <summary>

    /// 预测类别。

    /// </summary>

    public int Predict(T[] sample)

    {

        var logScores = new T[_classLabels.Length];

        ComputeLogPosteriors(sample, logScores);

        return ClassificationMath.ArgMaxClassScores(_classLabels, logScores);

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

        if (cols != _classMeans[0].Length)

            throw new ArgumentException("特征维度与模型不匹配。");

        if (predictions.Length < rows)

            throw new ArgumentException("predictions 长度不足。", nameof(predictions));



        var sample = new T[cols];

        var logScores = new T[_classLabels.Length];

        for (int i = 0; i < rows; i++)

        {

            for (int j = 0; j < cols; j++)

                sample[j] = x[i, j];

            ComputeLogPosteriors(sample, logScores);

            predictions[i] = ClassificationMath.ArgMaxClassScores(_classLabels, logScores);

        }

    }



    /// <summary>

    /// 返回各类别的后验概率（归一化）。

    /// </summary>

    public Dictionary<int, T> PredictProbabilities(T[] sample)

    {

        var logScores = new T[_classLabels.Length];

        ComputeLogPosteriors(sample, logScores);

        T maxLog = logScores[0];

        for (int i = 1; i < logScores.Length; i++)

            maxLog = T.Max(maxLog, logScores[i]);



        T sum = T.Zero;

        var unnormalized = new T[logScores.Length];

        for (int i = 0; i < logScores.Length; i++)

        {

            unnormalized[i] = T.Exp(logScores[i] - maxLog);

            sum += unnormalized[i];

        }



        var result = new Dictionary<int, T>(_classLabels.Length);

        for (int i = 0; i < _classLabels.Length; i++)

            result[_classLabels[i]] = unnormalized[i] / sum;

        return result;

    }



    private void ComputeLogPosteriors(T[] sample, Span<T> destination)

    {

        if (!_isFitted)

            throw new InvalidOperationException("模型尚未拟合。");

        if (sample == null || sample.Length != _classMeans[0].Length)

            throw new ArgumentException("特征维度与模型不匹配。", nameof(sample));

        if (destination.Length < _classLabels.Length)

            throw new ArgumentException("destination 长度不足。", nameof(destination));



        for (int c = 0; c < _classLabels.Length; c++)

        {

            T logLikelihood = T.Log(_classPriors[c]);

            var means = _classMeans[c];

            var variances = _classVariances[c];

            for (int j = 0; j < sample.Length; j++)

                logLikelihood += LogGaussian(sample[j], means[j], variances[j]);

            destination[c] = logLikelihood;

        }

    }



    private static T LogGaussian(T x, T mean, T variance)

    {

        T diff = x - mean;

        T twoPi = T.CreateChecked(2.0) * T.Pi;

        return T.CreateChecked(-0.5) * (T.Log(twoPi * variance) + (diff * diff) / variance);

    }

}


