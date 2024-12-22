namespace Vorcyc.Mathematics.MachineLearning;

using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;


/// <summary>
/// 提供对数据集进行多元线性回归的方法。
/// </summary>
/// <typeparam name="T">实现了 IFloatingPointIeee754 接口的数值类型。</typeparam>
/// <example>
/// 下面是一个使用 <c>MultipleLinearRegression</c> 类的示例代码：
/// <code>
/// static void Main()
/// {
///     double[,] x = new double[,]
///     {
///         { 1, 2 },
///         { 2, 3 },
///         { 3, 4 },
///         { 4, 5 },
///         { 5, 6 }
///     };
///
///     double[] y = new double[] { 2, 3, 5, 7, 11 };
///
///     var regression = new MultipleLinearRegression&lt;double&gt;();
///     regression.Learn(x, y);
///     Console.WriteLine(&quot;Intercept: &quot; + regression.Intercept);
///     Console.WriteLine(&quot;Coefficients:&quot;);
///     for (int i = 0; i &lt; regression.Coefficients.Length; i++)
///     {
///         Console.WriteLine(&quot;Coefficient &quot; + (i + 1) + &quot;: &quot; + regression.Coefficients[i]);
///     }
///
///     double[] newInput = new double[] { 6, 7 };
///     double prediction = regression.Predict(newInput);
///     Console.WriteLine(&quot;Predicted value for input &quot; + string.Join(&quot;, &quot;, newInput) + &quot;: &quot; + prediction);
/// }
/// </code>
/// </example>
public class MultipleLinearRegression<T> : IMachineLearning
    where T : struct, IFloatingPointIeee754<T>
{


    private T[]? _coefficients;

    private T? _intercept;


    /// <summary>
    /// 获取回归模型的回归系数。
    /// </summary>
    public T[]? Coefficients => _coefficients;

    /// <summary>
    /// 获取回归模型的截距。
    /// </summary>
    public T? Intercept => _intercept;


    public MachineLearningTask Task =>  MachineLearningTask.Regression;


    /// <summary>
    /// 学习多元线性回归模型的回归系数和截距。
    /// </summary>
    /// <param name="x">自变量矩阵。</param>
    /// <param name="y">因变量向量。</param>
    public void Learn(T[,] x, T[] y)
    {
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);

        // 创建设计矩阵（添加一列全为1的列向量）
        T[,] designMatrixData = new T[rows, cols + 1];
        for (int i = 0; i < rows; i++)
        {
            designMatrixData[i, 0] = T.One; // 截距项
            for (int j = 0; j < cols; j++)
            {
                designMatrixData[i, j + 1] = x[i, j];
            }
        }

        Matrix<T> designMatrix = new(designMatrixData);

        // 转置设计矩阵
        Matrix<T> transposedMatrix = designMatrix.Transpose();

        // 计算 (X^T * X)
        Matrix<T> xtx = transposedMatrix * designMatrix;

        // 计算 (X^T * y)
        T[] xtyData = new T[cols + 1];
        for (int i = 0; i < cols + 1; i++)
        {
            xtyData[i] = T.Zero;
            for (int j = 0; j < rows; j++)
            {
                xtyData[i] += transposedMatrix[i, j] * y[j];
            }
        }

        // 计算 (X^T * X)^-1
        Matrix<T> xtxInverse = xtx.Inverse();

        // 计算回归系数 (X^T * X)^-1 * (X^T * y)
        T[] coefficients = new T[cols + 1];
        for (int i = 0; i < cols + 1; i++)
        {
            coefficients[i] = T.Zero;
            for (int j = 0; j < cols + 1; j++)
            {
                coefficients[i] += xtxInverse[i, j] * xtyData[j];
            }
        }

        // 截距是系数向量的第一个元素
        _intercept = coefficients[0];

        // 其余元素是回归系数
        _coefficients = new T[cols];
        Array.Copy(coefficients, 1, _coefficients, 0, cols);
    }

    /// <summary>
    /// 根据给定的自变量和回归系数预测因变量的值。
    /// </summary>
    /// <param name="x">自变量。</param>
    /// <returns>预测的因变量值。</returns>
    public T? Predict(T[] x)
    {
        if (_intercept is null) return null;
        if (_coefficients is null) return null;

        T result = _intercept.Value;
        for (int i = 0; i < _coefficients.Length; i++)
        {
            result += _coefficients[i] * x[i];
        }
        return result;
    }
}
