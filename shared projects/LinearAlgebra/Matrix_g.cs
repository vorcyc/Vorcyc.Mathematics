namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Text;
using Vorcyc.Mathematics.Framework;

/// <summary>
/// 表示一个二维矩阵。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 INumber 接口。</typeparam>
public class Matrix<T> : ICloneable<Matrix<T>>
    where T : struct, INumber<T>
{
    private Memory<T> _values;
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


    #region 构造器

    /// <summary>
    /// 使用指定的行数和列数构造一个矩阵。
    /// </summary>
    /// <param name="rows">行数。</param>
    /// <param name="columns">列数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(int rows, int columns)
    {
        Guard.AgainstNonPositive(rows, "Rows and columns must be positive integers.");
        Guard.AgainstNonPositive(columns, "Rows and columns must be positive integers.");

        _rows = rows;
        _columns = columns;
        _values = new Memory<T>(new T[rows * columns]);
    }

    /// <summary>
    /// 使用指定的大小构造一个方阵。
    /// </summary>
    /// <param name="size">矩阵的大小（行数和列数）。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(int size)
    {
        Guard.AgainstNonPositive(size, "Rows and columns must be positive integers.");

        _rows = size;
        _columns = size;
        _values = new Memory<T>(new T[size * size]);
    }

    /// <summary>
    /// 使用 T[,] 数据构造一个矩阵。
    /// </summary>
    /// <param name="initialValues">二维数组形式的数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(T[,] initialValues)
    {
        _rows = initialValues.GetLength(0);
        _columns = initialValues.GetLength(1);
        _values = new Memory<T>(new T[_rows * _columns]);

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                _values.Span[i * _columns + j] = initialValues[i, j];
            }
        }
    }

    #endregion


    #region Indexer

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
            return ref _values.Span[row * _columns + column];
        }
    }

    #endregion


    #region 隐式转换

    /// <summary>
    /// 隐式转换为 T[,](二维数组)。
    /// </summary>
    /// <param name="matrix">要转换的矩阵。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T[,](Matrix<T> matrix)
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

    /// <summary>
    /// 隐式转换为 T[][]（交错数组）。
    /// </summary>
    /// <param name="matrix"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T[][](Matrix<T> matrix)
    {
        T[][] result = new T[matrix.Rows][];
        for (int i = 0; i < matrix.Rows; i++)
        {
            result[i] = new T[matrix.Columns];
            for (int j = 0; j < matrix.Columns; j++)
            {
                result[i][j] = matrix[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// 隐式转换交错数组为矩阵。
    /// </summary>
    /// <param name="values">交错数组。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Matrix<T>(T[][] values)
    {
        int rows = values.Length;
        int columns = values[0].Length;
        T[,] array = new T[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                array[i, j] = values[i][j];
            }
        }

        return new Matrix<T>(array);
    }

    /// <summary>
    /// 隐式转换二维数组为矩阵。
    /// </summary>
    /// <param name="values">二维数组。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Matrix<T>(T[,] values)
    {
        return new Matrix<T>(values);
    }

    #endregion


    #region operators

    /// <summary>
    /// 矩阵加法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的和。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException("Matrices must have the same dimensions for addition.");

        Matrix<T> result = new(a.Rows, a.Columns);
        int length = a._values.Length;
        int vectorSize = System.Numerics.Vector<T>.Count;

        int i;
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var va = new System.Numerics.Vector<T>(a._values.Span.Slice(i, vectorSize));
            var vb = new System.Numerics.Vector<T>(b._values.Span.Slice(i, vectorSize));
            var vr = va + vb;
            vr.CopyTo(result._values.Span.Slice(i, vectorSize));
        }

        for (; i < length; i++)
        {
            result._values.Span[i] = a._values.Span[i] + b._values.Span[i];
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
    public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException("Matrices must have the same dimensions for subtraction.");

        Matrix<T> result = new(a.Rows, a.Columns);
        int length = a._values.Length;
        int vectorSize = System.Numerics.Vector<T>.Count;

        int i;
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var va = new System.Numerics.Vector<T>(a._values.Span.Slice(i, vectorSize));
            var vb = new System.Numerics.Vector<T>(b._values.Span.Slice(i, vectorSize));
            var vr = va - vb;
            vr.CopyTo(result._values.Span.Slice(i, vectorSize));
        }

        for (; i < length; i++)
        {
            result._values.Span[i] = a._values.Span[i] - b._values.Span[i];
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
    public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
    {
        if (a.Columns != b.Rows)
            throw new ArgumentException("Matrices must have compatible dimensions for multiplication.");

        Matrix<T> result = new(a.Rows, b.Columns);
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
    public static Matrix<T> operator *(Matrix<T> matrix, T scalar)
    {
        Matrix<T> result = new(matrix.Rows, matrix.Columns);
        int length = matrix._values.Length;
        int vectorSize = System.Numerics.Vector<T>.Count;

        int i;
        var vScalar = new System.Numerics.Vector<T>(scalar);
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var vm = new System.Numerics.Vector<T>(matrix._values.Span.Slice(i, vectorSize));
            var vr = vm * vScalar;
            vr.CopyTo(result._values.Span.Slice(i, vectorSize));
        }

        for (; i < length; i++)
        {
            result._values.Span[i] = matrix._values.Span[i] * scalar;
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
    public static Matrix<T> operator /(Matrix<T> matrix, T scalar)
    {
        Matrix<T> result = new(matrix.Rows, matrix.Columns);
        int length = matrix._values.Length;
        int vectorSize = System.Numerics.Vector<T>.Count;

        int i;
        var vScalar = new System.Numerics.Vector<T>(scalar);
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var vm = new System.Numerics.Vector<T>(matrix._values.Span.Slice(i, vectorSize));
            var vr = vm / vScalar;
            vr.CopyTo(result._values.Span.Slice(i, vectorSize));
        }

        for (; i < length; i++)
        {
            result._values.Span[i] = matrix._values.Span[i] / scalar;
        }

        return result;
    }

    #endregion


    #region GetRow or GetColumn

    /// <summary>
    /// 获取指定行的元素。
    /// </summary>
    /// <param name="rowIndex">行索引。</param>
    /// <returns>指定行的元素的 Span。</returns>
    /// <remarks>
    /// 在 GetRow 方法中，直接使用 <see cref="Span{T}"/> 构造函数创建对原数组的切片，这样返回的 <see cref="Span{T}"/> 可以引用到原数组中的元素。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= _rows)
        {
            throw new IndexOutOfRangeException("Row index is out of range.");
        }

        return _values.Span.Slice(rowIndex * _columns, _columns);
    }

    /// <summary>
    /// 获取指定列的元素。
    /// </summary>
    /// <param name="columnIndex">列索引。</param>
    /// <returns>指定列的元素的 ReadOnlySpan。</returns>
    /// <remarks>
    /// 在 GetColumn 方法中，由于列在内存中不是连续的，所以需要创建一个新的数组来存储列元素，然后返回这个数组的 <see cref="ReadOnlySpan{T}"/>。这样可以确保返回的列数据是只读的。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> GetColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= _columns)
        {
            throw new IndexOutOfRangeException("Column index is out of range.");
        }

        T[] column = new T[_rows];
        for (int i = 0; i < _rows; i++)
        {
            column[i] = this[i, columnIndex];
        }
        return new ReadOnlySpan<T>(column);
    }

    #endregion


    #region 矩阵操作

    /// <summary>
    /// 矩阵转置。
    /// </summary>
    /// <returns>转置后的矩阵。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix<T> Transpose()
    {
        Matrix<T> result = new(_columns, _rows);
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

        return CalculateDeterminant(_values.Span, _rows);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T CalculateDeterminant(Span<T> values, int n)
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
    public Matrix<T> Inverse()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("Matrix must be square to calculate inverse.");

        T det = Determinant();

        if (T.Abs(det) < T.CreateChecked(1e-10))
            throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

        Matrix<T> result = new(_rows, _columns);
        T[] adjoint = Adjoint(_values.Span, _rows);
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
    private T[] Adjoint(Span<T> values, int n)
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

    #endregion


    #region Decomposition

    /// <summary>
    /// LU分解。
    /// </summary>
    /// <param name="L">输出的下三角矩阵。</param>
    /// <param name="U">输出的上三角矩阵。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LUDecomposition(out Matrix<T> L, out Matrix<T> U)
    {
        int n = _rows;
        L = new Matrix<T>(n, n);
        U = new Matrix<T>(n, n);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i <= j)
                {
                    U[i, j] = _values.Span[i * n + j];
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
                    L[i, j] = _values.Span[i * n + j];
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
    public void QRDecomposition(out Matrix<T> Q, out Matrix<T> R)
    {
        int m = _rows;
        int n = _columns;
        Q = new Matrix<T>(m, m);
        R = new Matrix<T>(m, n);

        T[] A = _values.ToArray();

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
    public Matrix<T> CholeskyDecomposition()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("Matrix must be square for Cholesky decomposition.");

        Matrix<T> L = new(_rows, _columns);

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
                    L[i, j] = Sqrt(_values.Span[i * _columns + i] - sum);
                }
                else
                {
                    L[i, j] = (_values.Span[i * _columns + j] - sum) / L[j, j];
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

    #endregion


    #region Utility Methods


    /// <summary>
    /// 创建一个单位矩阵。
    /// </summary>
    /// <param name="size">矩阵的大小（行数和列数）。</param>
    /// <returns>单位矩阵。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> Eye(int size)
    {
        var eye = new Matrix<T>(size);

        for (var i = 0; i < size; i++)
        {
            eye[i, i] = T.One;
        }

        return eye;
    }

    /// <summary>
    /// 用随机数填充矩阵。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FillRandom()
    {
        var span = _values.Span;
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = T.CreateTruncating(Random.Shared.NextDouble());
        }
    }

    /// <summary>
    /// 创建一个伴随矩阵。
    /// </summary>
    /// <param name="a">输入数组，表示多项式的系数。</param>
    /// <returns>伴随矩阵。</returns>
    /// <exception cref="ArgumentException">当输入数组的长度小于2或第一个系数为零时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> Companion(T[] a)
    {
        if (a.Length < 2)
        {
            throw new ArgumentException("The size of input array must be at least 2!");
        }

        if (T.Abs(a[0]) < T.CreateSaturating(1e-30))
        {
            throw new ArgumentException("The first coefficient must not be zero!");
        }

        var size = a.Length - 1;

        var companion = new Matrix<T>(size);

        for (var i = 0; i < size; i++)
        {
            companion.GetRow(0)[i] = -a[i + 1] / a[0];
        }

        for (var i = 1; i < size; i++)
        {
            companion.GetRow(i)[i - 1] = T.One;
        }

        return companion;
    }

    /// <summary>
    /// 创建矩阵的深拷贝。
    /// </summary>
    /// <returns>矩阵的深拷贝。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix<T> Clone()
    {
        Matrix<T> clone = new(_rows, _columns);
        _values.CopyTo(clone._values);
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

    #endregion

}

