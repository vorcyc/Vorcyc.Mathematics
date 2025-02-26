using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.MachineLearning.Regression;

/// <summary>
/// 提供对数据集进行多元线性回归的方法。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
/// <remarks>
/// 多元线性回归通过最小二乘法拟合模型 y = β₀ + β₁x₁ + β₂x₂ + ... + βₙxₙ，其中 β₀ 是截距，β₁...βₙ 是回归系数。
/// 该实现使用矩阵运算求解，支持多维输入数据。
/// 
/// 优化版本包括：
/// - 使用 LU 分解代替矩阵求逆，提升数值稳定性。
/// - 添加 R² 计算，提供模型评估。
/// - 统一方法名为 <see cref="Fit(T[,], T[])"/>，与其他回归类一致。
/// </remarks>
/// <example>
/// 下面是一个使用 <see cref="MultipleLinearRegression{T}"/> 类的示例：
/// <code>
/// var x = new double[,]
/// {
///     { 1, 2 },
///     { 2, 3 },
///     { 3, 4 },
///     { 4, 5 },
///     { 5, 6 }
/// };
/// var y = new double[] { 2, 3, 5, 7, 11 };
/// var regression = new MultipleLinearRegression&lt;double&gt;();
/// regression.Fit(x, y);
/// Console.WriteLine($"Intercept: {regression.Intercept}");
/// Console.WriteLine("Coefficients: " + string.Join(", ", regression.Coefficients));
/// Console.WriteLine($"R²: {regression.RSquared}");
/// var newInput = new double[] { 6, 7 };
/// var prediction = regression.Predict(newInput);
/// Console.WriteLine($"Prediction for {string.Join(", ", newInput)}: {prediction}");
/// </code>
/// </example>
public class MultipleLinearRegression<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{
    private T[] _coefficients;    // 回归系数
    private T _intercept;         // 截距
    private bool _isFitted;       // 是否已拟合
    private T _rSquared;          // 决定系数 R²
    private T[,] _lastX;          // 上次拟合的自变量矩阵
    private T[] _lastY;           // 上次拟合的因变量数组

    /// <summary>
    /// 初始化 <see cref="MultipleLinearRegression{T}"/> 类的新实例。
    /// </summary>
    public MultipleLinearRegression()
    {
        _isFitted = false;
    }

    /// <summary>
    /// 获取回归模型的回归系数。
    /// </summary>
    public IReadOnlyList<T> Coefficients => _isFitted ? _coefficients : throw new InvalidOperationException("模型尚未拟合。");

    /// <summary>
    /// 获取回归模型的截距。
    /// </summary>
    public T Intercept => _isFitted ? _intercept : throw new InvalidOperationException("模型尚未拟合。");

    /// <summary>
    /// 获取模型的决定系数 R²，表示模型对数据的解释能力。
    /// </summary>
    public T RSquared => _isFitted ? _rSquared : throw new InvalidOperationException("模型尚未拟合。");

    /// <summary>
    /// 获取机器学习任务类型。
    /// </summary>
    public MachineLearningTask Task => MachineLearningTask.Regression;

    /// <summary>
    /// 拟合多元线性回归模型，计算回归系数和截距。
    /// </summary>
    /// <param name="x">自变量矩阵，行表示数据点，列表示特征。</param>
    /// <param name="y">因变量数组。</param>
    /// <exception cref="ArgumentException">当 <paramref name="x"/> 或 <paramref name="y"/> 为 null、空，或维度不匹配时抛出。</exception>
    /// <exception cref="InvalidOperationException">当矩阵不可逆时抛出。</exception>
    public void Fit(T[,] x, T[] y)
    {
        if (x == null || y == null)
            throw new ArgumentException("自变量矩阵或因变量数组不能为 null。");
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);
        if (rows == 0 || cols == 0 || y.Length == 0)
            throw new ArgumentException("自变量矩阵或因变量数组不能为空。");
        if (rows != y.Length)
            throw new ArgumentException("自变量矩阵的行数必须等于因变量数组的长度。");

        _lastX = x;
        _lastY = y;

        var designMatrix = new Matrix<T>(rows, cols + 1);
        for (int i = 0; i < rows; i++)
        {
            designMatrix[i, 0] = T.One;
            for (int j = 0; j < cols; j++)
                designMatrix[i, j + 1] = x[i, j];
        }

        var xt = designMatrix.Transpose();
        var xtx = xt * designMatrix;
        var xty = new Matrix<T>(cols + 1, 1);
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols + 1; j++)
                xty[j, 0] += xt[j, i] * y[i];

        var coefficients = Solve(xtx, xty);

        try
        {
            _intercept = coefficients[0];
            _coefficients = new T[cols];
            Array.Copy(coefficients, 1, _coefficients, 0, cols);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("系数赋值失败: " + ex.Message);
        }

        // 设置 _isFitted 为 true
        _isFitted = true;

        try
        {
            _rSquared = ComputeRSquared(x, y);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("R² 计算失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 根据给定的自变量预测因变量的值。
    /// </summary>
    /// <param name="x">自变量数组。</param>
    /// <returns>预测的因变量值。</returns>
    /// <exception cref="InvalidOperationException">当模型尚未拟合时抛出。</exception>
    /// <exception cref="ArgumentException">当 <paramref name="x"/> 的维度与模型不匹配时抛出。</exception>
    public T Predict(T[] x)
    {
        if (!_isFitted)
            throw new InvalidOperationException("模型尚未拟合。");
        if (x == null || x.Length != _coefficients.Length)
            throw new ArgumentException("输入自变量的维度必须与模型匹配。", nameof(x));

        T result = _intercept;
        for (int i = 0; i < _coefficients.Length; i++)
        {
            result += _coefficients[i] * x[i];
        }
        return result;
    }

    /// <summary>
    /// 使用 LU 分解求解线性方程组 Ax = b。
    /// </summary>
    /// <param name="A">系数矩阵。</param>
    /// <param name="b">常数矩阵。</param>
    /// <returns>方程组的解。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T[] Solve(Matrix<T> A, Matrix<T> b)
    {
        A.LUDecomposition(out Matrix<T> L, out Matrix<T> U, out int[] P);
        int n = A.Rows;

        // 检查矩阵是否可逆
        for (int i = 0; i < n; i++)
        {
            if (T.Abs(U[i, i]) < T.CreateChecked(1e-10))
                throw new InvalidOperationException("矩阵不可逆，无法求解。");
        }

        // 前向代入 Ly = Pb
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

        // 后向代入 Ux = y
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
    /// 计算模型的决定系数 R²。
    /// </summary>
    private T ComputeRSquared(T[,] x, T[] y)
    {
        T ssTot = T.Zero, ssRes = T.Zero;
        T avgY = T.Zero;
        int n = y.Length;

        // 计算 y 的平均值
        for (int i = 0; i < n; i++)
            avgY += y[i];
        avgY /= T.CreateChecked(n);

        // 计算总平方和和残差平方和
        T[] xRow = new T[x.GetLength(1)];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < xRow.Length; j++)
                xRow[j] = x[i, j];
            T yPred = Predict(xRow);
            T yActual = y[i];
            ssTot += (yActual - avgY) * (yActual - avgY);
            ssRes += (yActual - yPred) * (yActual - yPred);
        }

        return ssTot != T.Zero ? T.One - (ssRes / ssTot) : T.Zero;
    }
}
