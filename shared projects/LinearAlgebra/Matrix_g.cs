
namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Text;


/// <summary>
/// 表示一个二维矩阵，提供高效的矩阵操作和分解方法。
/// </summary>
/// <typeparam name="T">数值类型，必须实现 <see cref="INumber{T}"/> 和 <see cref="IFloatingPointIeee754{T}"/> 接口。</typeparam>
/// <remarks>
/// 该类支持基本的矩阵运算（如加、减、乘法）、矩阵分解（如 LU、QR、Cholesky）以及实用工具方法。
/// 优化了性能，利用向量化运算和内存高效性，适用于数值计算和机器学习任务。
/// </remarks>
public class Matrix<T> : ICloneable<Matrix<T>>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly T[] _values; // 存储矩阵元素的连续数组
    private readonly int _rows;   // 矩阵行数
    private readonly int _columns; // 矩阵列数

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
    /// <param name="rows">行数，必须为正整数。</param>
    /// <param name="columns">列数，必须为正整数。</param>
    /// <exception cref="ArgumentException">当 <paramref name="rows"/> 或 <paramref name="columns"/> 小于等于 0 时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(int rows, int columns)
    {
        GuardAgainstNonPositive(rows, nameof(rows));
        GuardAgainstNonPositive(columns, nameof(columns));

        _rows = rows;
        _columns = columns;
        _values = new T[rows * columns];
    }

    /// <summary>
    /// 使用指定的大小构造一个方阵。
    /// </summary>
    /// <param name="size">矩阵的大小（行数和列数），必须为正整数。</param>
    /// <exception cref="ArgumentException">当 <paramref name="size"/> 小于等于 0 时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(int size) : this(size, size) { }

    ///// <summary>
    ///// 使用二维数组构造一个矩阵。
    ///// </summary>
    ///// <param name="initialValues">二维数组形式的初始数据。</param>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public Matrix(T[,] initialValues)
    //{
    //    _rows = initialValues.GetLength(0);
    //    _columns = initialValues.GetLength(1);
    //    _values = new T[_rows * _columns];
    //    initialValues.CopyTo(_values, 0);
    //}

    /// <summary>
    /// 使用二维数组构造一个矩阵。
    /// </summary>
    /// <param name="initialValues">二维数组形式的初始数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(T[,] initialValues)
    {
        _rows = initialValues.GetLength(0);
        _columns = initialValues.GetLength(1);
        _values = new T[_rows * _columns];
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                //_values[i * _columns + j] = initialValues[i, j];
                this[i, j] = initialValues[i, j];
            }
        }
    }




    #endregion

    #region Indexer

    /// <summary>
    /// 获取或设置指定位置的元素。
    /// </summary>
    /// <param name="row">行索引，从 0 开始。</param>
    /// <param name="column">列索引，从 0 开始。</param>
    /// <returns>指定位置的元素的引用。</returns>
    /// <exception cref="IndexOutOfRangeException">当 <paramref name="row"/> 或 <paramref name="column"/> 超出范围时抛出。</exception>
    public ref T this[int row, int column]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ValidateIndices(row, column);
            return ref _values[row * _columns + column];
        }
    }

    #endregion

    #region 隐式转换

    /// <summary>
    /// 隐式转换为二维数组 <see cref="T[,]"/>。
    /// </summary>
    /// <param name="matrix">要转换的矩阵。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T[,](Matrix<T> matrix)
    {
        var result = new T[matrix.Rows, matrix.Columns];
        matrix._values.CopyTo(result, 0);
        return result;
    }

    /// <summary>
    /// 隐式转换为交错数组 <see cref="T[][]"/>。
    /// </summary>
    /// <param name="matrix">要转换的矩阵。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T[][](Matrix<T> matrix)
    {
        var result = new T[matrix.Rows][];
        for (int i = 0; i < matrix.Rows; i++)
        {
            result[i] = new T[matrix.Columns];
            matrix.GetRow(i).CopyTo(result[i]);
        }
        return result;
    }

    /// <summary>
    /// 隐式从交错数组 <see cref="T[][]"/> 转换为矩阵。
    /// </summary>
    /// <param name="values">交错数组形式的初始数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Matrix<T>(T[][] values)
    {
        int rows = values.Length;
        int columns = values[0].Length;
        var matrix = new Matrix<T>(rows, columns);
        for (int i = 0; i < rows; i++)
            values[i].CopyTo(matrix.GetRow(i));
        return matrix;
    }

    /// <summary>
    /// 隐式从二维数组 <see cref="T[,]"/> 转换为矩阵。
    /// </summary>
    /// <param name="values">二维数组形式的初始数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Matrix<T>(T[,] values) => new Matrix<T>(values);

    #endregion

    #region operators

    /// <summary>
    /// 矩阵加法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的和。</returns>
    /// <exception cref="ArgumentException">当矩阵维度不匹配时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
    {
        ValidateDimensions(a, b, "加法");
        var result = new Matrix<T>(a.Rows, a.Columns);
        VectorAdd(a._values, b._values, result._values);
        return result;
    }

    /// <summary>
    /// 矩阵减法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的差。</returns>
    /// <exception cref="ArgumentException">当矩阵维度不匹配时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
    {
        ValidateDimensions(a, b, "减法");
        var result = new Matrix<T>(a.Rows, a.Columns);
        VectorSubtract(a._values, b._values, result._values);
        return result;
    }

    /// <summary>
    /// 矩阵乘法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的乘积。</returns>
    /// <exception cref="ArgumentException">当矩阵维度不匹配时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
    {
        if (a.Columns != b.Rows)
            throw new ArgumentException("矩阵维度不匹配，无法相乘。");

        var result = new Matrix<T>(a.Rows, b.Columns);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < b.Columns; j++)
            {
                T sum = T.Zero;
                for (int k = 0; k < a.Columns; k++)
                    sum += a[i, k] * b[k, j];
                result[i, j] = sum;
            }
        }
        return result;
    }

    /// <summary>
    /// 矩阵与标量乘法运算符。
    /// </summary>
    /// <param name="matrix">矩阵。</param>
    /// <param name="scalar">标量值。</param>
    /// <returns>矩阵与标量的乘积。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator *(Matrix<T> matrix, T scalar)
    {
        var result = new Matrix<T>(matrix.Rows, matrix.Columns);
        VectorMultiplyScalar(matrix._values, scalar, result._values);
        return result;
    }

    /// <summary>
    /// 矩阵除以标量运算符。
    /// </summary>
    /// <param name="matrix">矩阵。</param>
    /// <param name="scalar">标量值。</param>
    /// <returns>矩阵与标量的商。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator /(Matrix<T> matrix, T scalar)
    {
        var result = new Matrix<T>(matrix.Rows, matrix.Columns);
        VectorDivideScalar(matrix._values, scalar, result._values);
        return result;
    }

    #endregion

    #region GetRow or GetColumn

    /// <summary>
    /// 获取指定行的元素。
    /// </summary>
    /// <param name="rowIndex">行索引，从 0 开始。</param>
    /// <returns>指定行的元素的 <see cref="Span{T}"/>。</returns>
    /// <exception cref="IndexOutOfRangeException">当 <paramref name="rowIndex"/> 超出范围时抛出。</exception>
    /// <remarks>
    /// 返回的 <see cref="Span{T}"/> 直接引用底层数组，避免复制，提高性能。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetRow(int rowIndex)
    {
        ValidateRowIndex(rowIndex);
        return _values.AsSpan(rowIndex * _columns, _columns);
    }

    /// <summary>
    /// 获取指定列的元素。
    /// </summary>
    /// <param name="columnIndex">列索引，从 0 开始。</param>
    /// <returns>指定列的元素数组。</returns>
    /// <exception cref="IndexOutOfRangeException">当 <paramref name="columnIndex"/> 超出范围时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] GetColumn(int columnIndex)
    {
        ValidateColumnIndex(columnIndex);
        var column = new T[_rows];
        for (int i = 0; i < _rows; i++)
            column[i] = this[i, columnIndex];
        return column;
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
        var result = new Matrix<T>(_columns, _rows);
        for (int i = 0; i < _rows; i++)
            for (int j = 0; j < _columns; j++)
                result[j, i] = this[i, j];
        return result;
    }

    /// <summary>
    /// 计算矩阵的行列式。
    /// </summary>
    /// <returns>矩阵的行列式值。</returns>
    /// <exception cref="InvalidOperationException">当矩阵不是方阵时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Determinant()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("矩阵必须是方阵才能计算行列式。");
        return CalculateDeterminant(_values.AsSpan(), _rows);
    }

    /// <summary>
    /// 递归计算矩阵的行列式（内部方法）。
    /// </summary>
    /// <param name="values">矩阵元素数据的 Span。</param>
    /// <param name="n">矩阵阶数。</param>
    /// <returns>行列式值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T CalculateDeterminant(Span<T> values, int n)
    {
        if (n == 1) return values[0];
        if (n == 2) return values[0] * values[3] - values[1] * values[2];

        T det = T.Zero;
        var subMatrix = new T[(n - 1) * (n - 1)];
        for (int p = 0; p < n; p++)
        {
            int subIndex = 0;
            for (int i = 1; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (j != p)
                        subMatrix[subIndex++] = values[i * n + j];
            det += values[p] * CalculateDeterminant(subMatrix, n - 1) * (p % 2 == 0 ? T.One : -T.One);
        }
        return det;
    }

    /// <summary>
    /// 计算矩阵的逆矩阵。
    /// </summary>
    /// <returns>逆矩阵。</returns>
    /// <exception cref="InvalidOperationException">当矩阵不是方阵或不可逆时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix<T> Inverse()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("矩阵必须是方阵才能计算逆矩阵。");

        T det = Determinant();
        if (T.Abs(det) < T.CreateChecked(1e-10))
            throw new InvalidOperationException("矩阵不可逆。");

        var result = new Matrix<T>(_rows, _columns);
        var adjoint = Adjoint(_values.AsSpan(), _rows);
        for (int i = 0; i < _rows; i++)
            for (int j = 0; j < _columns; j++)
                result[i, j] = adjoint[i * _columns + j] / det;
        return result;
    }

    /// <summary>
    /// 计算矩阵的伴随矩阵（内部方法）。
    /// </summary>
    /// <param name="values">矩阵元素数据的 Span。</param>
    /// <param name="n">矩阵阶数。</param>
    /// <returns>伴随矩阵的元素数组。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T[] Adjoint(Span<T> values, int n)
    {
        var adjoint = new T[n * n];
        var subMatrix = new T[(n - 1) * (n - 1)];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                int subIndex = 0;
                for (int row = 0; row < n; row++)
                    if (row != i)
                        for (int col = 0; col < n; col++)
                            if (col != j)
                                subMatrix[subIndex++] = values[row * n + col];
                adjoint[j * n + i] = CalculateDeterminant(subMatrix, n - 1) * ((i + j) % 2 == 0 ? T.One : -T.One);
            }
        }
        return adjoint;
    }

    #endregion

    #region Decomposition

    /// <summary>
    /// 执行 LU 分解（带部分主元选择）。
    /// </summary>
    /// <param name="L">输出的下三角矩阵。</param>
    /// <param name="U">输出的上三角矩阵。</param>
    /// <param name="P">输出的置换向量，表示行交换顺序。</param>
    /// <exception cref="InvalidOperationException">当矩阵不是方阵或不可逆时抛出。</exception>
    /// <remarks>
    /// 该方法使用部分主元选择（Partial Pivoting）提高数值稳定性，分解结果满足 PA = LU。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LUDecomposition(out Matrix<T> L, out Matrix<T> U, out int[] P)
    {
        if (_rows != _columns)
            throw new InvalidOperationException("矩阵必须是方阵。");

        int n = _rows;
        L = new Matrix<T>(n, n);
        U = new Matrix<T>(n, n);
        P = new int[n];
        var A = Clone();

        for (int i = 0; i < n; i++) P[i] = i;

        for (int k = 0; k < n; k++)
        {
            T max = T.Abs(A[k, k]);
            int pivot = k;
            for (int i = k + 1; i < n; i++)
                if (T.Abs(A[i, k]) > max)
                {
                    max = T.Abs(A[i, k]);
                    pivot = i;
                }

            Console.WriteLine($"k={k}, max={max}, pivot={pivot}"); // 调试信息
            if (max < T.CreateChecked(1e-6))
                throw new InvalidOperationException("矩阵不可逆。");

            if (pivot != k)
            {
                SwapRows(A, k, pivot);
                (P[k], P[pivot]) = (P[pivot], P[k]);
            }

            L[k, k] = T.One;
            for (int i = k + 1; i < n; i++)
            {
                L[i, k] = A[i, k] / A[k, k];
                for (int j = k + 1; j < n; j++)
                    A[i, j] -= L[i, k] * A[k, j];
                A[i, k] = T.Zero;
            }

            for (int j = k; j < n; j++)
                U[k, j] = A[k, j];
        }
    }

    /// <summary>
    /// 执行 QR 分解。
    /// </summary>
    /// <param name="Q">输出的正交矩阵。</param>
    /// <param name="R">输出的上三角矩阵。</param>
    /// <remarks>
    /// 使用 Gram-Schmidt 正交化方法分解矩阵，满足 A = QR。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void QRDecomposition(out Matrix<T> Q, out Matrix<T> R)
    {
        int m = _rows;
        int n = _columns;
        Q = new Matrix<T>(m, m);
        R = new Matrix<T>(m, n);

        var A = _values.ToArray();
        for (int k = 0; k < n; k++)
        {
            T norm = T.Zero;
            for (int i = 0; i < m; i++)
                norm += A[i * n + k] * A[i * n + k];
            norm = T.Sqrt(norm);

            R[k, k] = norm;
            for (int i = 0; i < m; i++)
                Q[i, k] = A[i * n + k] / norm;

            for (int j = k + 1; j < n; j++)
            {
                T dotProduct = T.Zero;
                for (int i = 0; i < m; i++)
                    dotProduct += Q[i, k] * A[i * n + j];
                R[k, j] = dotProduct;
                for (int i = 0; i < m; i++)
                    A[i * n + j] -= Q[i, k] * dotProduct;
            }
        }
    }

    /// <summary>
    /// 执行 Cholesky 分解。
    /// </summary>
    /// <returns>下三角矩阵 L，满足 A = LLᵀ。</returns>
    /// <exception cref="InvalidOperationException">当矩阵不是方阵或不是正定时抛出。</exception>
    /// <remarks>
    /// 该方法适用于对称正定矩阵。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix<T> CholeskyDecomposition()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("矩阵必须是方阵。");

        var L = new Matrix<T>(_rows, _columns);
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                T sum = T.Zero;
                for (int k = 0; k < j; k++)
                    sum += L[i, k] * L[j, k];

                if (i == j)
                {
                    T diag = this[i, i] - sum;
                    if (diag <= T.Zero)
                        throw new InvalidOperationException("矩阵不是正定矩阵。");
                    L[i, j] = T.Sqrt(diag);
                }
                else
                    L[i, j] = (this[i, j] - sum) / L[j, j];
            }
        }
        return L;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// 创建一个单位矩阵。
    /// </summary>
    /// <param name="size">矩阵的大小（行数和列数），必须为正整数。</param>
    /// <returns>单位矩阵。</returns>
    /// <exception cref="ArgumentException">当 <paramref name="size"/> 小于等于 0 时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> Eye(int size)
    {
        GuardAgainstNonPositive(size, nameof(size));
        var eye = new Matrix<T>(size);
        for (int i = 0; i < size; i++)
            eye[i, i] = T.One;
        return eye;
    }

    /// <summary>
    /// 用随机数填充矩阵。
    /// </summary>
    /// <remarks>
    /// 随机数范围为 [0, 1)，由 <see cref="Random.Shared"/> 生成。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FillRandom()
    {
        var span = _values.AsSpan();
        for (int i = 0; i < span.Length; i++)
            span[i] = T.CreateTruncating(Random.Shared.NextDouble());
    }

    /// <summary>
    /// 创建一个伴随矩阵。
    /// </summary>
    /// <param name="a">输入数组，表示多项式的系数。</param>
    /// <returns>伴随矩阵。</returns>
    /// <exception cref="ArgumentException">当输入数组长度小于 2 或第一个系数接近零时抛出。</exception>
    /// <remarks>
    /// 伴随矩阵用于表示多项式的特征值问题。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> Companion(T[] a)
    {
        if (a.Length < 2)
            throw new ArgumentException("输入数组长度必须至少为 2。");
        if (T.Abs(a[0]) < T.CreateSaturating(1e-30))
            throw new ArgumentException("第一个系数不能为零。");

        int size = a.Length - 1;
        var companion = new Matrix<T>(size);
        for (int i = 0; i < size; i++)
            companion[0, i] = -a[i + 1] / a[0];
        for (int i = 1; i < size; i++)
            companion[i, i - 1] = T.One;
        return companion;
    }

    /// <summary>
    /// 创建矩阵的深拷贝。
    /// </summary>
    /// <returns>矩阵的深拷贝。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix<T> Clone()
    {
        var clone = new Matrix<T>(_rows, _columns);
        _values.CopyTo(clone._values, 0);
        return clone;
    }

    /// <summary>
    /// 返回矩阵的字符串表示形式。
    /// </summary>
    /// <returns>矩阵的字符串表示，每行用换行符分隔，元素间用逗号分隔。</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                sb.Append(this[i, j]);
                if (j < _columns - 1) sb.Append(", ");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 使用向量化方式执行矩阵加法。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void VectorAdd(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result)
    {
        int vectorSize = Vector<T>.Count;
        int i = 0;
        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = new Vector<T>(a.Slice(i));
            var vb = new Vector<T>(b.Slice(i));
            (va + vb).CopyTo(result.Slice(i));
        }
        for (; i < a.Length; i++)
            result[i] = a[i] + b[i];
    }

    /// <summary>
    /// 使用向量化方式执行矩阵减法。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void VectorSubtract(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> result)
    {
        int vectorSize = Vector<T>.Count;
        int i = 0;
        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = new Vector<T>(a.Slice(i));
            var vb = new Vector<T>(b.Slice(i));
            (va - vb).CopyTo(result.Slice(i));
        }
        for (; i < a.Length; i++)
            result[i] = a[i] - b[i];
    }

    /// <summary>
    /// 使用向量化方式执行矩阵与标量乘法。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void VectorMultiplyScalar(ReadOnlySpan<T> source, T scalar, Span<T> result)
    {
        int vectorSize = Vector<T>.Count;
        var vScalar = new Vector<T>(scalar);
        int i = 0;
        for (; i <= source.Length - vectorSize; i += vectorSize)
        {
            var vSource = new Vector<T>(source.Slice(i));
            (vSource * vScalar).CopyTo(result.Slice(i));
        }
        for (; i < source.Length; i++)
            result[i] = source[i] * scalar;
    }

    /// <summary>
    /// 使用向量化方式执行矩阵与标量除法。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void VectorDivideScalar(ReadOnlySpan<T> source, T scalar, Span<T> result)
    {
        int vectorSize = Vector<T>.Count;
        var vScalar = new Vector<T>(scalar);
        int i = 0;
        for (; i <= source.Length - vectorSize; i += vectorSize)
        {
            var vSource = new Vector<T>(source.Slice(i));
            (vSource / vScalar).CopyTo(result.Slice(i));
        }
        for (; i < source.Length; i++)
            result[i] = source[i] / scalar;
    }

    /// <summary>
    /// 交换矩阵的两行。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SwapRows(Matrix<T> matrix, int row1, int row2)
    {
        var span = matrix._values.AsSpan();
        int start1 = row1 * matrix.Columns;
        int start2 = row2 * matrix.Columns;
        for (int j = 0; j < matrix.Columns; j++)
            (span[start1 + j], span[start2 + j]) = (span[start2 + j], span[start1 + j]);
    }

    /// <summary>
    /// 验证索引是否在有效范围内。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateIndices(int row, int column)
    {
        if ((uint)row >= (uint)_rows || (uint)column >= (uint)_columns)
            throw new IndexOutOfRangeException("索引超出范围。");
    }

    /// <summary>
    /// 验证行索引是否有效。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateRowIndex(int row) => ValidateIndices(row, 0);

    /// <summary>
    /// 验证列索引是否有效。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateColumnIndex(int column) => ValidateIndices(0, column);

    /// <summary>
    /// 检查值是否为正整数。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GuardAgainstNonPositive(int value, string name)
    {
        if (value <= 0)
            throw new ArgumentException($"{name} 必须为正整数。");
    }

    /// <summary>
    /// 验证两个矩阵的维度是否匹配。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateDimensions(Matrix<T> a, Matrix<T> b, string operation)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException($"矩阵维度不匹配，无法执行 {operation}。");
    }

    #endregion
}