using System.Numerics;
using System.Text;

namespace Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// 表示一个二维矩阵。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
public class NewMatrix<T> : ICloneable<NewMatrix<T>>
    where T : struct, INumber<T>
{
    private T[] _values;
    private int _rows;
    private int _columns;

    /// <summary>
    /// 获取矩阵的行数。
    /// </summary>
    public int Rows => _rows;

    /// <summary>
    /// 获取矩阵的列数。
    /// </summary>
    public int Columns => _columns;

    /// <summary>
    /// 使用指定的行数和列数构造一个矩阵。
    /// </summary>
    /// <param name="rows">行数。</param>
    /// <param name="columns">列数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NewMatrix(int rows, int columns)
    {
        if (rows <= 0 || columns <= 0)
        {
            throw new ArgumentException("Rows and columns must be positive integers.");
        }

        _rows = rows;
        _columns = columns;
        _values = new T[rows * columns];
    }

    /// <summary>
    /// 使用 T[,] 数据构造一个矩阵。
    /// </summary>
    /// <param name="initialValues">二维数组形式的数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NewMatrix(T[,] initialValues)
    {
        _rows = initialValues.GetLength(0);
        _columns = initialValues.GetLength(1);
        _values = new T[_rows * _columns];

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                _values[i * _columns + j] = initialValues[i, j];
            }
        }
    }

    /// <summary>
    /// 获取或设置指定位置的元素。
    /// </summary>
    /// <param name="row">行索引。</param>
    /// <param name="column">列索引。</param>
    /// <returns>指定位置的元素。</returns>   
    public ref T this[int row, int column]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (row < 0 || row >= _rows || column < 0 || column >= _columns)
            {
                throw new IndexOutOfRangeException("Row or column is out of range.");
            }
            return ref _values[row * _columns + column];
        }
    }



    /// <summary>
    /// 隐式转换为 T[,]。
    /// </summary>
    /// <param name="matrix">要转换的矩阵。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T[,](NewMatrix<T> matrix)
    {
        T[,] result = new T[matrix.Rows, matrix.Columns];
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                result[i, j] = matrix[i, j];
            }
        }
        return result;
    }

    #region operators

    /// <summary>
    /// 矩阵加法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的和。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NewMatrix<T> operator +(NewMatrix<T> a, NewMatrix<T> b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException("Matrices must have the same dimensions for addition.");

        NewMatrix<T> result = new(a.Rows, a.Columns);
        int length = a._values.Length;
        int vectorSize = System.Numerics.Vector<T>.Count;

        int i;
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var va = new System.Numerics.Vector<T>(a._values, i);
            var vb = new System.Numerics.Vector<T>(b._values, i);
            var vr = va + vb;
            vr.CopyTo(result._values, i);
        }

        for (; i < length; i++)
        {
            result._values[i] = a._values[i] + b._values[i];
        }

        return result;
    }

    /// <summary>
    /// 矩阵减法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的差。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NewMatrix<T> operator -(NewMatrix<T> a, NewMatrix<T> b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException("Matrices must have the same dimensions for subtraction.");

        NewMatrix<T> result = new(a.Rows, a.Columns);
        int length = a._values.Length;
        int vectorSize = System.Numerics.Vector<T>.Count;

        int i;
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var va = new System.Numerics.Vector<T>(a._values, i);
            var vb = new System.Numerics.Vector<T>(b._values, i);
            var vr = va - vb;
            vr.CopyTo(result._values, i);
        }

        for (; i < length; i++)
        {
            result._values[i] = a._values[i] - b._values[i];
        }

        return result;
    }

    /// <summary>
    /// 矩阵乘法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的乘积。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NewMatrix<T> operator *(NewMatrix<T> a, NewMatrix<T> b)
    {
        if (a.Columns != b.Rows)
            throw new ArgumentException("Matrices must have compatible dimensions for multiplication.");

        NewMatrix<T> result = new(a.Rows, b.Columns);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < b.Columns; j++)
            {
                T sum = T.Zero;
                for (int k = 0; k < a.Columns; k++)
                {
                    sum += a[i, k] * b[k, j];
                }
                result[i, j] = sum;
            }
        }
        return result;
    }

    /// <summary>
    /// 矩阵乘法运算符，矩阵乘以标量。
    /// </summary>
    /// <param name="matrix">矩阵。</param>
    /// <param name="scalar">标量。</param>
    /// <returns>矩阵与标量的乘积。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NewMatrix<T> operator *(NewMatrix<T> matrix, T scalar)
    {
        NewMatrix<T> result = new(matrix.Rows, matrix.Columns);
        int length = matrix._values.Length;
        int vectorSize = System.Numerics.Vector<T>.Count;

        int i;
        var vScalar = new System.Numerics.Vector<T>(scalar);
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var vm = new System.Numerics.Vector<T>(matrix._values, i);
            var vr = vm * vScalar;
            vr.CopyTo(result._values, i);
        }

        for (; i < length; i++)
        {
            result._values[i] = matrix._values[i] * scalar;
        }

        return result;
    }

    /// <summary>
    /// 矩阵除法运算符，矩阵除以标量。
    /// </summary>
    /// <param name="matrix">矩阵。</param>
    /// <param name="scalar">标量。</param>
    /// <returns>矩阵与标量的商。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NewMatrix<T> operator /(NewMatrix<T> matrix, T scalar)
    {
        NewMatrix<T> result = new(matrix.Rows, matrix.Columns);
        int length = matrix._values.Length;
        int vectorSize = System.Numerics.Vector<T>.Count;

        int i;
        var vScalar = new System.Numerics.Vector<T>(scalar);
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var vm = new System.Numerics.Vector<T>(matrix._values, i);
            var vr = vm / vScalar;
            vr.CopyTo(result._values, i);
        }

        for (; i < length; i++)
        {
            result._values[i] = matrix._values[i] / scalar;
        }

        return result;
    }

    #endregion



    /// <summary>
    /// 用随机数填充矩阵。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FillRandom()
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = T.CreateTruncating(Random.Shared.NextDouble());
        }
    }

    /// <summary>
    /// 矩阵转置。
    /// </summary>
    /// <returns>转置后的矩阵。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NewMatrix<T> Transpose()
    {
        NewMatrix<T> result = new(_columns, _rows);
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                result[j, i] = this[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// 计算矩阵的行列式。
    /// </summary>
    /// <returns>矩阵的行列式。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Determinant()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("Matrix must be square to calculate determinant.");

        return CalculateDeterminant(_values, _rows);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T CalculateDeterminant(T[] values, int n)
    {
        if (n == 1)
            return values[0];
        if (n == 2)
            return values[0] * values[3] - values[1] * values[2];

        T det = T.Zero;
        T[] subMatrix = new T[(n - 1) * (n - 1)];
        for (int p = 0; p < n; p++)
        {
            int subIndex = 0;
            for (int i = 1; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j == p) continue;
                    subMatrix[subIndex++] = values[i * n + j];
                }
            }
            det += values[p] * CalculateDeterminant(subMatrix, n - 1) * (p % 2 == 0 ? T.One : -T.One);
        }
        return det;
    }

    /// <summary>
    /// 计算矩阵的逆矩阵。
    /// </summary>
    /// <returns>矩阵的逆矩阵。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NewMatrix<T> Inverse()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("Matrix must be square to calculate inverse.");

        T det = Determinant();

        if (T.Abs(det) < T.CreateChecked(1e-10))
            throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

        NewMatrix<T> result = new(_rows, _columns);
        T[] adjoint = Adjoint(_values, _rows);
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                result[i, j] = adjoint[i * _columns + j] / det;
            }
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T[] Adjoint(T[] values, int n)
    {
        T[] adjoint = new T[n * n];
        T[] subMatrix = new T[(n - 1) * (n - 1)];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                int subIndex = 0;
                for (int row = 0; row < n; row++)
                {
                    if (row == i) continue;
                    for (int col = 0; col < n; col++)
                    {
                        if (col == j) continue;
                        subMatrix[subIndex++] = values[row * n + col];
                    }
                }
                adjoint[j * n + i] = CalculateDeterminant(subMatrix, n - 1) * ((i + j) % 2 == 0 ? T.One : -T.One);
            }
        }
        return adjoint;
    }

    /// <summary>
    /// LU分解。
    /// </summary>
    /// <param name="L">输出的下三角矩阵。</param>
    /// <param name="U">输出的上三角矩阵。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LUDecomposition(out NewMatrix<T> L, out NewMatrix<T> U)
    {
        int n = _rows;
        L = new NewMatrix<T>(n, n);
        U = new NewMatrix<T>(n, n);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i <= j)
                {
                    U[i, j] = _values[i * n + j];
                    for (int k = 0; k < i; k++)
                    {
                        U[i, j] -= L[i, k] * U[k, j];
                    }
                    if (i == j)
                    {
                        L[i, j] = T.One;
                    }
                    else
                    {
                        L[i, j] = T.Zero;
                    }
                }
                else
                {
                    L[i, j] = _values[i * n + j];
                    for (int k = 0; k < j; k++)
                    {
                        L[i, j] -= L[i, k] * U[k, j];
                    }
                    L[i, j] /= U[j, j];
                    U[i, j] = T.Zero;
                }
            }
        }
    }

    /// <summary>
    /// QR分解。
    /// </summary>
    /// <param name="Q">输出的正交矩阵。</param>
    /// <param name="R">输出的上三角矩阵。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void QRDecomposition(out NewMatrix<T> Q, out NewMatrix<T> R)
    {
        int m = _rows;
        int n = _columns;
        Q = new NewMatrix<T>(m, m);
        R = new NewMatrix<T>(m, n);

        T[] A = (T[])_values.Clone();

        for (int k = 0; k < n; k++)
        {
            T norm = T.Zero;
            for (int i = 0; i < m; i++)
            {
                norm += A[i * n + k] * A[i * n + k];
            }

            norm = Sqrt(norm);

            R[k, k] = norm;
            for (int i = 0; i < m; i++)
            {
                Q[i, k] = A[i * n + k] / norm;
            }

            for (int j = k + 1; j < n; j++)
            {
                T dotProduct = T.Zero;
                for (int i = 0; i < m; i++)
                {
                    dotProduct += Q[i, k] * A[i * n + j];
                }
                R[k, j] = dotProduct;
                for (int i = 0; i < m; i++)
                {
                    A[i * n + j] -= Q[i, k] * dotProduct;
                }
            }
        }
    }

    /// <summary>
    /// Cholesky分解。
    /// </summary>
    /// <returns>下三角矩阵。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NewMatrix<T> CholeskyDecomposition()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("Matrix must be square for Cholesky decomposition.");

        NewMatrix<T> L = new(_rows, _columns);

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                T sum = T.Zero;
                for (int k = 0; k < j; k++)
                {
                    sum += L[i, k] * L[j, k];
                }

                if (i == j)
                {
                    L[i, j] = Sqrt(_values[i * _columns + i] - sum);
                }
                else
                {
                    L[i, j] = (_values[i * _columns + j] - sum) / L[j, j];
                }
            }
        }

        return L;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T Sqrt(T value)
    {
        if (typeof(T) == typeof(float))
        {
            return (T)(object)MathF.Sqrt((float)(object)value);
        }
        else if (typeof(T) == typeof(double))
        {
            return (T)(object)Math.Sqrt((double)(object)value);
        }
        else if (typeof(T) == typeof(decimal))
        {
            return (T)(object)Math.Sqrt((double)(object)value);
        }
        else if (typeof(T) == typeof(int))
        {
            return (T)(object)(int)Math.Sqrt((int)(object)value);
        }
        else if (typeof(T) == typeof(long))
        {
            return (T)(object)(long)Math.Sqrt((long)(object)value);
        }
        else if (typeof(T) == typeof(short))
        {
            return (T)(object)(short)Math.Sqrt((short)(object)value);
        }
        else
        {
            throw new NotSupportedException($"Sqrt is not supported for type {typeof(T)}.");
        }
    }

    /// <summary>
    /// 创建矩阵的深拷贝。
    /// </summary>
    /// <returns>矩阵的深拷贝。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NewMatrix<T> Clone()
    {
        NewMatrix<T> clone = new(_rows, _columns);
        Array.Copy(_values, clone._values, _values.Length);
        return clone;
    }

    /// <summary>
    /// 返回矩阵的字符串表示形式。
    /// </summary>
    /// <returns>矩阵的字符串表示形式。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                sb.Append(this[i, j]);
                if (j < _columns - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
