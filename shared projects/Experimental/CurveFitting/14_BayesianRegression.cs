using System.Numerics;

namespace Vorcyc.Mathematics.Experimental.CurveFitting;

public static class BayesianLinearRegression<T> where T : struct, IFloatingPointIeee754<T>
{
    public static BayesianFitResult<T> Fit(DataRow<T>[] X, T[] y, T alpha, T beta)
    {
        if (X == null || y == null) throw new ArgumentNullException("Input data cannot be null.");
        if (X.Length != y.Length) throw new ArgumentException("X and y must have the same length.");
        if (X.Length == 0) throw new ArgumentException("Input data cannot be empty.");

        int n = X.Length;         // 样本数
        int d = X[0].ColumnCount; // 特征数

        // 构造设计矩阵（添加偏置项）
        T[][] designMatrix = new T[n][];
        for (int i = 0; i < n; i++)
        {
            designMatrix[i] = new T[d + 1];
            for (int j = 0; j < d; j++)
                designMatrix[i][j] = X[i][j];
            designMatrix[i][d] = T.One; // 偏置项
        }

        // 计算后验分布
        T[][] XtX = MatrixMultiply(MatrixTranspose(designMatrix), designMatrix);
        T[][] A = MatrixAdd(ScalarMultiply(CreateIdentity(d + 1), alpha), ScalarMultiply(XtX, beta));
        T[][] S_N = MatrixInverse(A); // 后验协方差矩阵
        T[] Xty = VectorMultiply(MatrixTranspose(designMatrix), y);
        T[] m_N = VectorMultiply(ScalarMultiply(S_N, beta), Xty); // 后验均值

        // 计算均方误差
        T mse = ComputeMSE(designMatrix, y, m_N);

        // 预测函数
        Func<T[], T> predictMean = (x) =>
        {
            T[] xWithBias = new T[x.Length + 1];
            Array.Copy(x, xWithBias, x.Length);
            xWithBias[x.Length] = T.One;
            return VectorDot(xWithBias, m_N);
        };

        Func<T[], T> predictVariance = (x) =>
        {
            T[] xWithBias = new T[x.Length + 1];
            Array.Copy(x, xWithBias, x.Length);
            xWithBias[x.Length] = T.One;
            T var = T.One / beta + VectorDot(xWithBias, VectorMultiply(S_N, xWithBias));
            return var;
        };

        return new BayesianFitResult<T>(predictMean, predictVariance, m_N, S_N, mse);
    }

    // 矩阵运算辅助方法（与之前相同）
    private static T[][] MatrixMultiply(T[][] A, T[][] B)
    {
        int rowsA = A.Length, colsA = A[0].Length, colsB = B[0].Length;
        T[][] result = new T[rowsA][];
        for (int i = 0; i < rowsA; i++)
        {
            result[i] = new T[colsB];
            for (int j = 0; j < colsB; j++)
                for (int k = 0; k < colsA; k++)
                    result[i][j] += A[i][k] * B[k][j];
        }
        return result;
    }

    private static T[][] MatrixTranspose(T[][] A)
    {
        int rows = A.Length, cols = A[0].Length;
        T[][] result = new T[cols][];
        for (int i = 0; i < cols; i++)
        {
            result[i] = new T[rows];
            for (int j = 0; j < rows; j++)
                result[i][j] = A[j][i];
        }
        return result;
    }

    private static T[][] MatrixAdd(T[][] A, T[][] B)
    {
        int rows = A.Length, cols = A[0].Length;
        T[][] result = new T[rows][];
        for (int i = 0; i < rows; i++)
        {
            result[i] = new T[cols];
            for (int j = 0; j < cols; j++)
                result[i][j] = A[i][j] + B[i][j];
        }
        return result;
    }

    private static T[][] ScalarMultiply(T[][] A, T scalar)
    {
        int rows = A.Length, cols = A[0].Length;
        T[][] result = new T[rows][];
        for (int i = 0; i < rows; i++)
        {
            result[i] = new T[cols];
            for (int j = 0; j < cols; j++)
                result[i][j] = A[i][j] * scalar;
        }
        return result;
    }

    private static T[] VectorMultiply(T[][] A, T[] v)
    {
        int rows = A.Length;
        T[] result = new T[rows];
        for (int i = 0; i < rows; i++)
            result[i] = VectorDot(A[i], v);
        return result;
    }

    private static T VectorDot(T[] a, T[] b)
    {
        T sum = T.Zero;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    private static T[][] CreateIdentity(int size)
    {
        T[][] result = new T[size][];
        for (int i = 0; i < size; i++)
        {
            result[i] = new T[size];
            result[i][i] = T.One;
        }
        return result;
    }

    private static T[][] MatrixInverse(T[][] A)
    {
        int n = A.Length;
        T[][] augmented = new T[n][];
        for (int i = 0; i < n; i++)
        {
            augmented[i] = new T[2 * n];
            Array.Copy(A[i], 0, augmented[i], 0, n);
            augmented[i][i + n] = T.One;
        }

        for (int i = 0; i < n; i++)
        {
            T pivot = augmented[i][i];
            for (int j = 0; j < 2 * n; j++)
                augmented[i][j] /= pivot;
            for (int k = 0; k < n; k++)
            {
                if (k != i)
                {
                    T factor = augmented[k][i];
                    for (int j = 0; j < 2 * n; j++)
                        augmented[k][j] -= factor * augmented[i][j];
                }
            }
        }

        T[][] result = new T[n][];
        for (int i = 0; i < n; i++)
        {
            result[i] = new T[n];
            Array.Copy(augmented[i], n, result[i], 0, n);
        }
        return result;
    }

    private static T ComputeMSE(T[][] X, T[] y, T[] w)
    {
        T sumSquaredError = T.Zero;
        for (int i = 0; i < X.Length; i++)
        {
            T pred = VectorDot(X[i], w);
            T error = y[i] - pred;
            sumSquaredError += error * error;
        }
        return sumSquaredError / T.CreateChecked(X.Length);
    }



    internal static void RunTests()
    {
        // 生成模拟数据
        int nSamples = 50;
        DataRow<double>[] X = new DataRow<double>[nSamples];
        double[] y = new double[nSamples];
        Random rand = new Random(42);
        double trueSlope = 2.5;
        double trueIntercept = 1.0;

        for (int i = 0; i < nSamples; i++)
        {
            double x = i / 5.0;
            X[i] = new DataRow<double>(x);
            y[i] = trueSlope * x + trueIntercept + (rand.NextDouble() - 0.5); // 添加噪声
        }

        // 训练模型
        double alpha = 1e-6;
        double beta = 1.0;
        var result = BayesianLinearRegression<double>.Fit(X, y, alpha, beta);

        // 输出参数估计
        Console.WriteLine("参数估计:");
        Console.WriteLine($"Slope: {result.MeanParameters[0]:F3} ± {Math.Sqrt(result.CovarianceMatrix[0][0]):F3}");
        Console.WriteLine($"Intercept: {result.MeanParameters[1]:F3} ± {Math.Sqrt(result.CovarianceMatrix[1][1]):F3}");
        Console.WriteLine($"Mean Squared Error: {result.MeanSquaredError:F3}");

        // 测试拟合效果
        Console.WriteLine("\n拟合效果测试:");
        double[] xTestPoints = { 0.0, 2.0, 5.0, 8.0, 10.0 };
        Console.WriteLine("x\tTrue y\tPredicted y\t95% Confidence Interval");
        foreach (double x in xTestPoints)
        {
            double trueY = trueSlope * x + trueIntercept;
            double[] xInput = { x };
            double predMean = result.PredictMean(xInput);
            double predStd = Math.Sqrt(result.PredictVariance(xInput));
            double ciLower = predMean - 1.96 * predStd;
            double ciUpper = predMean + 1.96 * predStd;

            Console.WriteLine($"{x:F1}\t{trueY:F3}\t{predMean:F3}\t[{ciLower:F3}, {ciUpper:F3}]");
        }

        // 计算 R² 分数
        double yMean = y.Average();
        double ssTot = y.Sum(yi => (yi - yMean) * (yi - yMean));
        double ssRes = 0.0;
        for (int i = 0; i < nSamples; i++)
        {
            double pred = result.PredictMean(X[i].ToArray());
            ssRes += (y[i] - pred) * (y[i] - pred);
        }
        double rSquared = 1 - ssRes / ssTot;
        Console.WriteLine($"\nR² Score: {rSquared:F3}");
    }
}