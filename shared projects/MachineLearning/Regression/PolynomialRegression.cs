//25.3.2

//// 示例数据
//using Vorcyc.Mathematics.MachineLearning.Regression;

//double[] x = { 1.0, 2.0, 3.0, 4.0, 5.0 };
//double[] y = { 2.1, 4.0, 9.2, 16.3, 24.9 };

//// 创建二次多项式回归模型
//var model = new PolynomialRegression<double>(2);
//model.Fit(x, y);

//// 输出结果
//Console.WriteLine("Coefficients: " + string.Join(", ", model.Coefficients));
//Console.WriteLine("R²: " + model.RSquared);

//// 预测
//double prediction = model.Predict(6.0);
//Console.WriteLine("Prediction for x=6: " + prediction);

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// 多项式回归类，用于进行非线性回归分析。
/// </summary>
/// <remarks>
/// 多项式回归通过拟合一个多项式函数来建模自变量和因变量之间的非线性关系。
/// 该类使用 LU 分解求解正规方程，提供拟合和预测功能，并计算拟合优度（R²）。
/// </remarks>
public class PolynomialRegression<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly int _degree;
    private T[]? _coefficients = null;
    private T _rSquared;

    /// <summary>
    /// 初始化多项式回归类的新实例。
    /// </summary>
    /// <param name="degree">多项式的阶数，必须大于等于 0。</param>
    /// <exception cref="ArgumentException">当 degree 小于 0 时抛出。</exception>
    public PolynomialRegression(int degree)
    {
        if (degree < 0)
            throw new ArgumentException("多项式阶数必须大于等于 0。");
        _degree = degree;
    }

    /// <summary>
    /// 获取拟合后的多项式系数。
    /// </summary>
    public T[] Coefficients => _coefficients?.ToArray() ?? throw new InvalidOperationException("模型尚未拟合。");

    /// <summary>
    /// 获取拟合优度（R²），表示模型对数据的解释能力。
    /// </summary>
    public T RSquared => _rSquared;

    /// <summary>
    /// 拟合多项式回归模型。
    /// </summary>
    /// <param name="x">输入数据的自变量数组。</param>
    /// <param name="y">输入数据的因变量数组。</param>
    /// <exception cref="ArgumentException">当输入数组为空、长度不同或数据点不足时抛出。</exception>
    public void Fit(Span<T> x, Span<T> y)
    {
        if (x.Length == 0 || y.Length == 0)
            throw new ArgumentException("输入数组不能为空。");
        if (x.Length != y.Length)
            throw new ArgumentException("自变量和因变量数组的长度必须相同。");
        if (x.Length <= _degree)
            throw new ArgumentException("数据点数量必须大于多项式阶数。");

        int n = x.Length;
        var vandermondeMatrix = new Matrix<T>(n, _degree + 1);

        // 构建范德蒙德矩阵
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= _degree; j++)
            {
                vandermondeMatrix[i, j] = T.Pow(x[i], T.CreateChecked(j));
            }
        }

        var yMatrix = new Matrix<T>(n, 1);
        for (int i = 0; i < n; i++)
        {
            yMatrix[i, 0] = y[i];
        }

        // 计算正规方程 (XᵀX)β = Xᵀy
        var vandermondeMatrixT = vandermondeMatrix.Transpose();
        var A = vandermondeMatrixT * vandermondeMatrix;
        var b = vandermondeMatrixT * yMatrix;

        // 使用 LU 分解求解系数
        _coefficients = Solve(A, b);

        // 计算 R²
        _rSquared = CalculateRSquared(x, y);
    }

    /// <summary>
    /// 预测给定输入的输出。
    /// </summary>
    /// <param name="x">输入值。</param>
    /// <returns>预测的输出值。</returns>
    /// <exception cref="InvalidOperationException">当模型尚未拟合时抛出。</exception>
    public T Predict(T x)
    {
        if (_coefficients == null)
            throw new InvalidOperationException("模型尚未拟合。");

        T result = T.Zero;
        for (int i = 0; i <= _degree; i++)
        {
            result += _coefficients[i] * T.Pow(x, T.CreateChecked(i));
        }
        return result;
    }

    /// <summary>
    /// 使用 LU 分解求解线性方程组 Ax = b。
    /// </summary>
    private T[] Solve(Matrix<T> A, Matrix<T> b)
    {
        if (A.Rows != A.Columns || A.Rows != b.Rows || b.Columns != 1)
            throw new ArgumentException("矩阵维度不匹配。");

        int n = A.Rows;
        A.LUDecomposition(out Matrix<T> L, out Matrix<T> U, out int[] P);

        for (int i = 0; i < n; i++)
            if (T.Abs(U[i, i]) < T.CreateChecked(1e-10))
                throw new InvalidOperationException("矩阵不可逆。");

        var Pb = new T[n];
        for (int i = 0; i < n; i++)
            Pb[i] = b[P[i], 0];

        var y = new T[n];
        for (int i = 0; i < n; i++)
        {
            T sum = T.Zero;
            for (int k = 0; k < i; k++)
                sum += L[i, k] * y[k];
            y[i] = (Pb[i] - sum) / L[i, i];
        }

        var x = new T[n];
        for (int i = n - 1; i >= 0; i--)
        {
            T sum = T.Zero;
            for (int k = i + 1; k < n; k++)
                sum += U[i, k] * x[k];
            x[i] = (y[i] - sum) / U[i, i];
        }

        return x;
    }

    /// <summary>
    /// 计算 R² 值。
    /// </summary>
    private T CalculateRSquared(Span<T> x, Span<T> y)
    {
        T yMean = T.Zero;
        for (int i = 0; i < y.Length; i++)
            yMean += y[i];
        yMean /= T.CreateChecked(y.Length);

        T ssTot = T.Zero; // 总平方和
        T ssRes = T.Zero; // 残差平方和
        for (int i = 0; i < y.Length; i++)
        {
            T yi = y[i];
            T yPred = Predict(x[i]);
            ssTot += (yi - yMean) * (yi - yMean);
            ssRes += (yi - yPred) * (yi - yPred);
        }

        return T.One - (ssRes / ssTot);
    }
}