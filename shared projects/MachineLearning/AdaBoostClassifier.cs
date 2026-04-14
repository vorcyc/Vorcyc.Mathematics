using System.Numerics;

using Vorcyc.Mathematics.MachineLearning.Internal;



namespace Vorcyc.Mathematics.MachineLearning;



/// <summary>

/// AdaBoost 多分类器（SAMME），弱学习器为数值决策桩。

/// </summary>

public class AdaBoostClassifier<T> : IClassifier<T>

    where T : struct, IFloatingPointIeee754<T>

{

    private DecisionStump[]? _stumps;

    private T[]? _alphas;

    private int _numClasses;



    /// <summary>

    /// 初始化 AdaBoost 分类器。

    /// </summary>

    /// <param name="nEstimators">弱学习器数量。</param>

    public AdaBoostClassifier(int nEstimators = 50)

    {

        if (nEstimators <= 0)

            throw new ArgumentOutOfRangeException(nameof(nEstimators));

        NEstimators = nEstimators;

    }



    /// <summary>弱学习器数量。</summary>

    public int NEstimators { get; }



    /// <inheritdoc />

    public MachineLearningTask Task => MachineLearningTask.Classification;



    /// <summary>

    /// 拟合多分类模型。标签为非负整数。

    /// </summary>

    public void Fit(T[,] x, int[] y)

    {

        if (x == null || y == null)

            throw new ArgumentException("输入不能为 null。");



        int rows = x.GetLength(0);

        if (rows == 0 || y.Length == 0 || rows != y.Length)

            throw new ArgumentException("样本数与标签数不匹配。");

        if (y.Min() < 0)

            throw new ArgumentException("标签必须为非负整数。");



        _numClasses = y.Max() + 1;

        _stumps = new DecisionStump[NEstimators];

        _alphas = new T[NEstimators];



        var weights = new T[rows];

        T uniform = T.One / T.CreateChecked(rows);

        for (int i = 0; i < rows; i++)

            weights[i] = uniform;



        var allIndices = Enumerable.Range(0, rows).ToArray();



        for (int m = 0; m < NEstimators; m++)

        {

            var stump = DecisionStump.FitBest(x, y, weights, allIndices, _numClasses)

                ?? throw new InvalidOperationException("无法训练弱学习器，请检查数据。");



            T error = T.Zero;

            for (int i = 0; i < rows; i++)

            {

                if (stump.Predict(Array2DHelpers.GetRow(x, i)) != y[i])

                    error += weights[i];

            }



            if (error >= T.One - T.CreateChecked(1e-10))

                error = T.One - T.CreateChecked(1e-10);

            if (error <= T.CreateChecked(1e-10))

                error = T.CreateChecked(1e-10);



            T alpha = T.Log((T.One - error) / error);

            if (_numClasses > 2)

                alpha += T.Log(T.CreateChecked(_numClasses - 1));



            _stumps[m] = stump;

            _alphas[m] = alpha;



            T weightSum = T.Zero;

            for (int i = 0; i < rows; i++)

            {

                if (stump.Predict(Array2DHelpers.GetRow(x, i)) != y[i])

                    weights[i] *= T.Exp(alpha);

                weightSum += weights[i];

            }



            for (int i = 0; i < rows; i++)

                weights[i] /= weightSum;

        }

    }



    /// <summary>

    /// 预测类别。

    /// </summary>

    public int Predict(T[] sample)

    {

        if (_stumps == null || _alphas == null)

            throw new InvalidOperationException("模型尚未拟合。");

        if (sample == null)

            throw new ArgumentNullException(nameof(sample));



        var votes = new T[_numClasses];

        for (int m = 0; m < _stumps.Length; m++)

        {

            int predicted = _stumps[m].Predict(sample);

            votes[predicted] += _alphas[m];

        }



        return ClassificationMath.ArgMax(votes);

    }



    private sealed class DecisionStump

    {

        public int FeatureIndex;

        public T Threshold = T.Zero;

        public int LeftClass;

        public int RightClass;



        public int Predict(T[] sample) =>

            sample[FeatureIndex] <= Threshold ? LeftClass : RightClass;



        public static DecisionStump? FitBest(

            T[,] x,

            int[] y,

            T[] weights,

            int[] indices,

            int numClasses)

        {

            DecisionStump? best = null;

            T bestError = T.CreateChecked(double.MaxValue);



            bool found = CartThresholdSearch.TryFindBestSplit(

                x,

                indices,

                allowedFeatures: null,

                (feature, threshold, left, right) =>

                {

                    T error = ComputeWeightedError(

                        x, y, weights, feature, threshold, left, right, numClasses,

                        out int leftClass, out int rightClass);



                    if (error < bestError)

                    {

                        bestError = error;

                        best = new DecisionStump

                        {

                            FeatureIndex = feature,

                            Threshold = threshold,

                            LeftClass = leftClass,

                            RightClass = rightClass

                        };

                    }



                    return error;

                },

                out _,

                out _,

                out _,

                out _);



            return found ? best : null;

        }



        private static T ComputeWeightedError(

            T[,] x,

            int[] y,

            T[] weights,

            int feature,

            T threshold,

            ReadOnlySpan<int> left,

            ReadOnlySpan<int> right,

            int numClasses,

            out int leftClass,

            out int rightClass)

        {

            var leftCounts = new T[numClasses];

            var rightCounts = new T[numClasses];



            foreach (int i in left)

                leftCounts[y[i]] += weights[i];

            foreach (int i in right)

                rightCounts[y[i]] += weights[i];



            leftClass = ClassificationMath.WeightedArgMax(leftCounts);

            rightClass = ClassificationMath.WeightedArgMax(rightCounts);



            T error = T.Zero;

            foreach (int i in left)

            {

                if (leftClass != y[i])

                    error += weights[i];

            }

            foreach (int i in right)

            {

                if (rightClass != y[i])

                    error += weights[i];

            }

            return error;

        }

    }

}


