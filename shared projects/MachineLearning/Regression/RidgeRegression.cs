//25.3.2

namespace Vorcyc.Mathematics.MachineLearning.Regression;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// 岭回归类，用于进行带有L2正则化的线性回归分析。
/// </summary>
/// <typeparam name="T">元素类型，必须实现 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
/// <remarks>
/// 岭回归是一种线性回归的变体，通过在损失函数中添加L2正则化项来防止过拟合。
/// 该类提供了拟合和预测功能，允许用户使用岭回归模型对数据进行分析和预测。
/// 
/// 使用方法：
/// 1. 创建一个 <see cref="RidgeRegression{T}"/> 类的实例，指定正则化参数和多项式阶数。
/// 2. 使用 <see cref="Fit(Span{T}, Span{T})"/> 方法拟合模型，传入自变量和因变量的数据。
/// 3. 使用 <see cref="Predict(T)"/> 方法对新的自变量值进行预测，得到预测的因变量值。
/// 
/// 注意事项：
/// - 输入数据的自变量和因变量数组长度必须相同。
/// - 该实现使用范德蒙德矩阵构建模型，并通过LU分解求解带正则化的线性方程组。
/// - 正则化参数 <see cref="_lambda"/> 必须非负。
/// </remarks>
public class RidgeRegression<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly T _lambda;       // 正则化参数
    private readonly int _degree;     // 多项式阶数
    private T[]? _coefficients;        // 回归系数

    /// <summary>
    /// 初始化岭回归类的新实例。
    /// </summary>
    /// <param name="lambda">正则化参数，必须非负。</param>
    /// <param name="degree">多项式阶数，默认值为 1（线性回归）。</param>
    /// <exception cref="ArgumentException">当 <paramref name="lambda"/> 小于零或 <paramref name="degree"/> 小于零时抛出。</exception>
    public RidgeRegression(T lambda, int degree = 1)
    {
        if (lambda < T.Zero)
            throw new ArgumentException("正则化参数必须非负。", nameof(lambda));
        if (degree < 0)
            throw new ArgumentException("多项式阶数必须非负。", nameof(degree));

        _lambda = lambda;
        _degree = degree;
    }

    /// <summary>
    /// 获取拟合后的系数。
    /// </summary>
    public IReadOnlyList<T> Coefficients => _coefficients;

    /// <summary>
    /// 拟合岭回归模型。
    /// </summary>
    /// <param name="x">输入数据的自变量数组。</param>
    /// <param name="y">输入数据的因变量数组。</param>
    /// <exception cref="ArgumentException">当输入数组为空、长度不同或数据点不足以拟合指定阶数时抛出。</exception>
    /// <exception cref="InvalidOperationException">当矩阵不可逆时抛出。</exception>
    public void Fit(Span<T> x, Span<T> y)
    {
        if (x.Length == 0 || y.Length == 0)
            throw new ArgumentException("输入数组不能为空。");
        if (x.Length != y.Length)
            throw new ArgumentException("自变量和因变量数组的长度必须相同。");
        if (x.Length <= _degree)
            throw new ArgumentException("数据点数量必须大于多项式阶数。");

        int n = x.Length;
        var vandermondeMatrix = new Matrix<T>(n, _degree + 1); // n 行，阶数+1 列

        // 构建范德蒙德矩阵
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= _degree; j++)
            {
                vandermondeMatrix[i, j] = T.Pow(x[i], T.CreateChecked(j));
            }
        }

        // 构造目标矩阵
        var yMatrix = new Matrix<T>(n, 1);
        for (int i = 0; i < n; i++)
        {
            yMatrix[i, 0] = y[i];
        }

        // 计算正规方程 (XᵀX + λI)β = Xᵀy
        var vandermondeMatrixT = vandermondeMatrix.Transpose();
        var A = vandermondeMatrixT * vandermondeMatrix;

        // 添加 L2 正则化项 (λI)
        for (int i = 0; i <= _degree; i++)
        {
            A[i, i] += _lambda;
        }

        var b = vandermondeMatrixT * yMatrix;

        // 求解系数
        _coefficients = Solve(A, b);
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
    /// <param name="A">系数矩阵。</param>
    /// <param name="b">常数矩阵。</param>
    /// <returns>方程组的解。</returns>
    /// <exception cref="ArgumentException">当矩阵维度不匹配时抛出。</exception>
    /// <exception cref="InvalidOperationException">当矩阵不可逆时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T[] Solve(Matrix<T> A, Matrix<T> b)
    {
        if (A.Rows != A.Columns || A.Rows != b.Rows || b.Columns != 1)
            throw new ArgumentException("矩阵维度不匹配。");

        int n = A.Rows;
        A.LUDecomposition(out Matrix<T> L, out Matrix<T> U, out int[] P);

        // 检查矩阵是否可逆
        for (int i = 0; i < n; i++)
        {
            if (T.Abs(U[i, i]) < T.CreateChecked(1e-10))
                throw new InvalidOperationException("矩阵不可逆，无法求解。");
        }

        // 应用置换向量 P 到 b
        var Pb = new T[n];
        for (int i = 0; i < n; i++)
            Pb[i] = b[P[i], 0];

        // 前向代入 Ly = Pb
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
}