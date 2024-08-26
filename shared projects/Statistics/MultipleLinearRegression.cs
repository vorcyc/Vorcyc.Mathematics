namespace Vorcyc.Mathematics.Statistics;

using System.Numerics;

//public static class MultipleLinearRegression<T>
//    where T : IFloatingPointIeee754<T>
//{


//    public static (T[] coefficients, T intercept) ComputeParameters(T[,] x, T[] y)
//    {
//        int rows = x.GetLength(0);
//        int cols = x.GetLength(1);

//        // 创建设计矩阵（添加一列全为1的列向量）
//        T[,] designMatrix = new T[rows, cols + 1];
//        for (int i = 0; i < rows; i++)
//        {
//            designMatrix[i, 0] = T.One; // 截距项
//            for (int j = 0; j < cols; j++)
//            {
//                designMatrix[i, j + 1] = x[i, j];
//            }
//        }

//        // 转置设计矩阵
//        T[,] transposedMatrix = TransposeMatrix(designMatrix);

//        // 计算 (X^T * X)
//        T[,] xtx = MultiplyMatrices(transposedMatrix, designMatrix);

//        // 计算 (X^T * y)
//        T[] xty = MultiplyMatrixVector(transposedMatrix, y);

//        // 计算 (X^T * X)^-1
//        T[,] xtxInverse = InvertMatrix(xtx);

//        // 计算回归系数 (X^T * X)^-1 * (X^T * y)
//        T[] coefficients = MultiplyMatrixVector(xtxInverse, xty);

//        // 截距是系数向量的第一个元素
//        T intercept = coefficients[0];

//        // 其余元素是回归系数
//        T[] regressionCoefficients = new T[cols];
//        Array.Copy(coefficients, 1, regressionCoefficients, 0, cols);

//        return (regressionCoefficients, intercept);
//    }


//    private static T[,] TransposeMatrix(T[,] matrix)
//    {
//        int rows = matrix.GetLength(0);
//        int cols = matrix.GetLength(1);
//        T[,] transposed = new T[cols, rows];
//        for (int i = 0; i < rows; i++)
//        {
//            for (int j = 0; j < cols; j++)
//            {
//                transposed[j, i] = matrix[i, j];
//            }
//        }
//        return transposed;
//    }

//    private static T[,] MultiplyMatrices(T[,] a, T[,] b)
//    {
//        int rowsA = a.GetLength(0);
//        int colsA = a.GetLength(1);
//        int colsB = b.GetLength(1);
//        T[,] result = new T[rowsA, colsB];
//        for (int i = 0; i < rowsA; i++)
//        {
//            for (int j = 0; j < colsB; j++)
//            {
//                for (int k = 0; k < colsA; k++)
//                {
//                    result[i, j] += a[i, k] * b[k, j];
//                }
//            }
//        }
//        return result;
//    }

//    private static T[] MultiplyMatrixVector(T[,] matrix, T[] vector)
//    {
//        int rows = matrix.GetLength(0);
//        int cols = matrix.GetLength(1);
//        T[] result = new T[rows];
//        for (int i = 0; i < rows; i++)
//        {
//            for (int j = 0; j < cols; j++)
//            {
//                result[i] += matrix[i, j] * vector[j];
//            }
//        }
//        return result;
//    }

//    private static T[,] InvertMatrix(T[,] matrix)
//    {
//        int n = matrix.GetLength(0);
//        T[,] result = new T[n, n];
//        T[,] identity = new T[n, n];
//        for (int i = 0; i < n; i++)
//        {
//            identity[i, i] = T.One;
//        }

//        for (int i = 0; i < n; i++)
//        {
//            T diag = matrix[i, i];
//            for (int j = 0; j < n; j++)
//            {
//                matrix[i, j] /= diag;
//                identity[i, j] /= diag;
//            }
//            for (int k = 0; k < n; k++)
//            {
//                if (k != i)
//                {
//                    T factor = matrix[k, i];
//                    for (int j = 0; j < n; j++)
//                    {
//                        matrix[k, j] -= factor * matrix[i, j];
//                        identity[k, j] -= factor * identity[i, j];
//                    }
//                }
//            }
//        }
//        return identity;
//    }


//    public static T Predict(Span<T> x, T intercept, T[] coefficients)
//    {
//        T result = intercept;
//        for (int i = 0; i < coefficients.Length; i++)
//        {
//            result += coefficients[i] * x[i];
//        }
//        return result;
//    }
//}


///// <summary>
///// Provides methods for performing multiple linear regression on a dataset.
///// </summary>
///// <typeparam name="T">The numeric type that implements <see cref="IFloatingPointIeee754{TSelf}"/>.</typeparam>
//public static class MultipleLinearRegression<T>
//    where T : IFloatingPointIeee754<T>
//{
//    /// <summary>
//    /// Computes the regression coefficients and intercept for a multiple linear regression model.
//    /// </summary>
//    /// <param name="x">The matrix of independent variables.</param>
//    /// <param name="y">The vector of dependent variables.</param>
//    /// <returns>A tuple containing the regression coefficients and the intercept.</returns>
//    public static (T[] coefficients, T intercept) ComputeParameters(T[,] x, T[] y)
//    {
//        int rows = x.GetLength(0);
//        int cols = x.GetLength(1);

//        // 创建设计矩阵（添加一列全为1的列向量）
//        T[,] designMatrix = new T[rows, cols + 1];
//        for (int i = 0; i < rows; i++)
//        {
//            designMatrix[i, 0] = T.One; // 截距项
//            for (int j = 0; j < cols; j++)
//            {
//                designMatrix[i, j + 1] = x[i, j];
//            }
//        }

//        // 转置设计矩阵
//        T[,] transposedMatrix = TransposeMatrix(designMatrix);

//        // 计算 (X^T * X)
//        T[,] xtx = MultiplyMatrices(transposedMatrix, designMatrix);

//        // 计算 (X^T * y)
//        T[] xty = MultiplyMatrixVector(transposedMatrix, y);

//        // 计算 (X^T * X)^-1
//        T[,] xtxInverse = InvertMatrix(xtx);

//        // 计算回归系数 (X^T * X)^-1 * (X^T * y)
//        T[] coefficients = MultiplyMatrixVector(xtxInverse, xty);

//        // 截距是系数向量的第一个元素
//        T intercept = coefficients[0];

//        // 其余元素是回归系数
//        T[] regressionCoefficients = new T[cols];
//        Array.Copy(coefficients, 1, regressionCoefficients, 0, cols);

//        return (regressionCoefficients, intercept);
//    }

//    /// <summary>
//    /// Transposes the given matrix.
//    /// </summary>
//    /// <param name="matrix">The matrix to transpose.</param>
//    /// <returns>The transposed matrix.</returns>
//    private static T[,] TransposeMatrix(T[,] matrix)
//    {
//        int rows = matrix.GetLength(0);
//        int cols = matrix.GetLength(1);
//        T[,] transposed = new T[cols, rows];
//        for (int i = 0; i < rows; i++)
//        {
//            for (int j = 0; j < cols; j++)
//            {
//                transposed[j, i] = matrix[i, j];
//            }
//        }
//        return transposed;
//    }

//    /// <summary>
//    /// Multiplies two matrices.
//    /// </summary>
//    /// <param name="a">The first matrix.</param>
//    /// <param name="b">The second matrix.</param>
//    /// <returns>The product of the two matrices.</returns>
//    private static T[,] MultiplyMatrices(T[,] a, T[,] b)
//    {
//        int rowsA = a.GetLength(0);
//        int colsA = a.GetLength(1);
//        int colsB = b.GetLength(1);
//        T[,] result = new T[rowsA, colsB];
//        for (int i = 0; i < rowsA; i++)
//        {
//            for (int j = 0; j < colsB; j++)
//            {
//                for (int k = 0; k < colsA; k++)
//                {
//                    result[i, j] += a[i, k] * b[k, j];
//                }
//            }
//        }
//        return result;
//    }

//    /// <summary>
//    /// Multiplies a matrix by a vector.
//    /// </summary>
//    /// <param name="matrix">The matrix.</param>
//    /// <param name="vector">The vector.</param>
//    /// <returns>The product of the matrix and the vector.</returns>
//    private static T[] MultiplyMatrixVector(T[,] matrix, T[] vector)
//    {
//        int rows = matrix.GetLength(0);
//        int cols = matrix.GetLength(1);
//        T[] result = new T[rows];
//        for (int i = 0; i < rows; i++)
//        {
//            for (int j = 0; j < cols; j++)
//            {
//                result[i] += matrix[i, j] * vector[j];
//            }
//        }
//        return result;
//    }

//    /// <summary>
//    /// Inverts the given matrix.
//    /// </summary>
//    /// <param name="matrix">The matrix to invert.</param>
//    /// <returns>The inverted matrix.</returns>
//    private static T[,] InvertMatrix(T[,] matrix)
//    {
//        int n = matrix.GetLength(0);
//        T[,] result = new T[n, n];
//        T[,] identity = new T[n, n];
//        for (int i = 0; i < n; i++)
//        {
//            identity[i, i] = T.One;
//        }

//        for (int i = 0; i < n; i++)
//        {
//            T diag = matrix[i, i];
//            for (int j = 0; j < n; j++)
//            {
//                matrix[i, j] /= diag;
//                identity[i, j] /= diag;
//            }
//            for (int k = 0; k < n; k++)
//            {
//                if (k != i)
//                {
//                    T factor = matrix[k, i];
//                    for (int j = 0; j < n; j++)
//                    {
//                        matrix[k, j] -= factor * matrix[i, j];
//                        identity[k, j] -= factor * identity[i, j];
//                    }
//                }
//            }
//        }
//        return identity;
//    }

//    /// <summary>
//    /// Predicts the dependent variable value for a given set of independent variables.
//    /// </summary>
//    /// <param name="x">The independent variables.</param>
//    /// <param name="intercept">The intercept of the regression model.</param>
//    /// <param name="coefficients">The regression coefficients.</param>
//    /// <returns>The predicted value of the dependent variable.</returns>
//    public static T Predict(Span<T> x, T intercept, T[] coefficients)
//    {
//        T result = intercept;
//        for (int i = 0; i < coefficients.Length; i++)
//        {
//            result += coefficients[i] * x[i];
//        }
//        return result;
//    }
//}


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
///     var (coefficients, intercept) = MultipleLinearRegression&lt;double&gt;.ComputeParameters(x, y);
///     Console.WriteLine(&quot;Intercept: &quot; + intercept);
///     Console.WriteLine(&quot;Coefficients:&quot;);
///     for (int i = 0; i &lt; coefficients.Length; i++)
///     {
///         Console.WriteLine(&quot;Coefficient &quot; + (i + 1) + &quot;: &quot; + coefficients[i]);
///     }
///
///     double[] newInput = new double[] { 6, 7 };
///     double prediction = MultipleLinearRegression&lt;double&gt;.Predict(coefficients, intercept, newInput);
///     Console.WriteLine(&quot;Predicted value for input &quot; + string.Join(&quot;, &quot;, newInput) + &quot;: &quot; + prediction);
/// }
/// </code>
/// </example>
public static class MultipleLinearRegression<T>
    where T : IFloatingPointIeee754<T>
{
    /// <summary>
    /// 计算多元线性回归模型的回归系数和截距。
    /// </summary>
    /// <param name="x">自变量矩阵。</param>
    /// <param name="y">因变量向量。</param>
    /// <returns>包含回归系数和截距的元组。</returns>
    public static (T[] coefficients, T intercept) ComputeParameters(T[,] x, T[] y)
    {
        int rows = x.GetLength(0);
        int cols = x.GetLength(1);

        // 创建设计矩阵（添加一列全为1的列向量）
        T[,] designMatrix = new T[rows, cols + 1];
        for (int i = 0; i < rows; i++)
        {
            designMatrix[i, 0] = T.One; // 截距项
            for (int j = 0; j < cols; j++)
            {
                designMatrix[i, j + 1] = x[i, j];
            }
        }

        // 转置设计矩阵
        T[,] transposedMatrix = TransposeMatrix(designMatrix);

        // 计算 (X^T * X)
        T[,] xtx = MultiplyMatrices(transposedMatrix, designMatrix);

        // 计算 (X^T * y)
        T[] xty = MultiplyMatrixVector(transposedMatrix, y);

        // 计算 (X^T * X)^-1
        T[,] xtxInverse = InvertMatrix(xtx);

        // 计算回归系数 (X^T * X)^-1 * (X^T * y)
        T[] coefficients = MultiplyMatrixVector(xtxInverse, xty);

        // 截距是系数向量的第一个元素
        T intercept = coefficients[0];

        // 其余元素是回归系数
        T[] regressionCoefficients = new T[cols];
        Array.Copy(coefficients, 1, regressionCoefficients, 0, cols);

        return (regressionCoefficients, intercept);
    }

    /// <summary>
    /// 转置给定的矩阵。
    /// </summary>
    /// <param name="matrix">要转置的矩阵。</param>
    /// <returns>转置后的矩阵。</returns>
    private static T[,] TransposeMatrix(T[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        T[,] transposed = new T[cols, rows];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                transposed[j, i] = matrix[i, j];
            }
        }
        return transposed;
    }

    /// <summary>
    /// 矩阵相乘。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的乘积。</returns>
    private static T[,] MultiplyMatrices(T[,] a, T[,] b)
    {
        int rowsA = a.GetLength(0);
        int colsA = a.GetLength(1);
        int colsB = b.GetLength(1);
        T[,] result = new T[rowsA, colsB];
        for (int i = 0; i < rowsA; i++)
        {
            for (int j = 0; j < colsB; j++)
            {
                for (int k = 0; k < colsA; k++)
                {
                    result[i, j] += a[i, k] * b[k, j];
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 矩阵与向量相乘。
    /// </summary>
    /// <param name="matrix">矩阵。</param>
    /// <param name="vector">向量。</param>
    /// <returns>矩阵与向量的乘积。</returns>
    private static T[] MultiplyMatrixVector(T[,] matrix, T[] vector)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        T[] result = new T[rows];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }
        return result;
    }

    /// <summary>
    /// 计算给定矩阵的逆矩阵。
    /// </summary>
    /// <param name="matrix">要求逆的矩阵。</param>
    /// <returns>逆矩阵。</returns>
    private static T[,] InvertMatrix(T[,] matrix)
    {
        int n = matrix.GetLength(0);
        T[,] result = new T[n, n];
        T[,] identity = new T[n, n];
        for (int i = 0; i < n; i++)
        {
            identity[i, i] = T.One;
        }

        for (int i = 0; i < n; i++)
        {
            T diag = matrix[i, i];
            for (int j = 0; j < n; j++)
            {
                matrix[i, j] /= diag;
                identity[i, j] /= diag;
            }
            for (int k = 0; k < n; k++)
            {
                if (k != i)
                {
                    T factor = matrix[k, i];
                    for (int j = 0; j < n; j++)
                    {
                        matrix[k, j] -= factor * matrix[i, j];
                        identity[k, j] -= factor * identity[i, j];
                    }
                }
            }
        }
        return identity;
    }

    /// <summary>
    /// 根据给定的自变量和回归系数预测因变量的值。
    /// </summary>
    /// <param name="x">自变量。</param>
    /// <param name="intercept">回归模型的截距。</param>
    /// <param name="coefficients">回归系数。</param>
    /// <returns>预测的因变量值。</returns>
    public static T Predict(Span<T> x, T intercept, T[] coefficients)
    {
        T result = intercept;
        for (int i = 0; i < coefficients.Length; i++)
        {
            result += coefficients[i] * x[i];
        }
        return result;
    }
}
