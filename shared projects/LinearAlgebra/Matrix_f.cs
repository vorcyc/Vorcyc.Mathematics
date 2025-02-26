namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

/// <summary>
/// 表示一个二维矩阵，使用单精度浮点数（float）存储元素，提供高效的矩阵操作和分解方法。
/// </summary>
/// <remarks>
/// 该类支持基本的矩阵运算（如加、减、乘法）、矩阵分解（如 LU、QR、Cholesky）以及实用工具方法。
/// 通过向量化运算和内存优化，适用于数值计算和机器学习任务。
/// </remarks>
public class Matrix : ICloneable<Matrix>
{
    private readonly float[] _values; // 存储矩阵元素的连续数组
    private readonly int _rows;       // 矩阵行数
    private readonly int _columns;    // 矩阵列数

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
        if (rows <= 0 || columns <= 0)
            throw new ArgumentException("行数和列数必须为正整数。");

        _rows = rows;
        _columns = columns;
        _values = new float[rows * columns];
    }

    /// <summary>
    /// 使用指定的大小构造一个方阵。
    /// </summary>
    /// <param name="size">矩阵的大小（行数和列数），必须为正整数。</param>
    /// <exception cref="ArgumentException">当 <paramref name="size"/> 小于等于 0 时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(int size) : this(size, size) { }

    /// <summary>
    /// 使用二维数组构造一个矩阵。
    /// </summary>
    /// <param name="initialValues">二维数组形式的初始数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(float[,] initialValues)
    {
        _rows = initialValues.GetLength(0);
        _columns = initialValues.GetLength(1);
        _values = new float[_rows * _columns];
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                _values[i * _columns + j] = initialValues[i, j];
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
    public ref float this[int row, int column]
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
    /// 隐式转换为二维数组 <see cref="float[,]"/>。
    /// </summary>
    /// <param name="matrix">要转换的矩阵。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float[,](Matrix matrix)
    {
        var result = new float[matrix.Rows, matrix.Columns];
        matrix._values.CopyTo(result, 0);
        return result;
    }

    /// <summary>
    /// 隐式转换为交错数组 <see cref="float[][]"/>。
    /// </summary>
    /// <param name="matrix">要转换的矩阵。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float[][](Matrix matrix)
    {
        var result = new float[matrix.Rows][];
        for (int i = 0; i < matrix.Rows; i++)
        {
            result[i] = new float[matrix.Columns];
            matrix.GetRow(i).CopyTo(result[i]);
        }
        return result;
    }

    /// <summary>
    /// 隐式从交错数组 <see cref="float[][]"/> 转换为矩阵。
    /// </summary>
    /// <param name="values">交错数组形式的初始数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Matrix(float[][] values)
    {
        int rows = values.Length;
        int columns = values[0].Length;
        var matrix = new Matrix(rows, columns);
        for (int i = 0; i < rows; i++)
            values[i].CopyTo(matrix.GetRow(i));
        return matrix;
    }

    /// <summary>
    /// 隐式从二维数组 <see cref="float[,]"/> 转换为矩阵。
    /// </summary>
    /// <param name="values">二维数组形式的初始数据。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Matrix(float[,] values) => new Matrix(values);

    #endregion

    #region 运算符

    /// <summary>
    /// 矩阵加法运算符。
    /// </summary>
    /// <param name="a">第一个矩阵。</param>
    /// <param name="b">第二个矩阵。</param>
    /// <returns>两个矩阵的和。</returns>
    /// <exception cref="ArgumentException">当矩阵维度不匹配时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix operator +(Matrix a, Matrix b)
    {
        ValidateDimensions(a, b, "加法");
        var result = new Matrix(a.Rows, a.Columns);
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
    public static Matrix operator -(Matrix a, Matrix b)
    {
        ValidateDimensions(a, b, "减法");
        var result = new Matrix(a.Rows, a.Columns);
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
    public static Matrix operator *(Matrix a, Matrix b)
    {
        if (a.Columns != b.Rows)
            throw new ArgumentException("矩阵维度不匹配，无法相乘。");

        var result = new Matrix(a.Rows, b.Columns);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < b.Columns; j++)
            {
                float sum = 0;
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
    public static Matrix operator *(Matrix matrix, float scalar)
    {
        var result = new Matrix(matrix.Rows, matrix.Columns);
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
    public static Matrix operator /(Matrix matrix, float scalar)
    {
        var result = new Matrix(matrix.Rows, matrix.Columns);
        VectorDivideScalar(matrix._values, scalar, result._values);
        return result;
    }

    #endregion

    #region GetRow or GetColumn

    /// <summary>
    /// 获取指定行的元素。
    /// </summary>
    /// <param name="rowIndex">行索引，从 0 开始。</param>
    /// <returns>指定行的元素的 <see cref="Span{float}"/>。</returns>
    /// <exception cref="IndexOutOfRangeException">当 <paramref name="rowIndex"/> 超出范围时抛出。</exception>
    /// <remarks>
    /// 返回的 <see cref="Span{float}"/> 直接引用底层数组，避免复制，提高性能。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<float> GetRow(int rowIndex)
    {
        ValidateRowIndex(rowIndex);
        return _values.AsSpan(rowIndex * _columns, _columns);
    }

    /// <summary>
    /// 获取指定列的元素。
    /// </summary>
    /// <param name="columnIndex">列索引，从 0 开始。</param>
    /// <returns>指定列的元素的 <see cref="ReadOnlySpan{float}"/>。</returns>
    /// <exception cref="IndexOutOfRangeException">当 <paramref name="columnIndex"/> 超出范围时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<float> GetColumn(int columnIndex)
    {
        ValidateColumnIndex(columnIndex);
        var column = new float[_rows];
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
    public Matrix Transpose()
    {
        var result = new Matrix(_columns, _rows);
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
    public float Determinant()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("矩阵必须是方阵才能计算行列式。");
        return CalculateDeterminant(_values.AsSpan(), _rows);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float CalculateDeterminant(Span<float> values, int n)
    {
        if (n == 1) return values[0];
        if (n == 2) return values[0] * values[3] - values[1] * values[2];

        float det = 0;
        var subMatrix = new float[(n - 1) * (n - 1)];
        for (int p = 0; p < n; p++)
        {
            int subIndex = 0;
            for (int i = 1; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (j != p)
                        subMatrix[subIndex++] = values[i * n + j];
            det += values[p] * CalculateDeterminant(subMatrix, n - 1) * (p % 2 == 0 ? 1 : -1);
        }
        return det;
    }

    /// <summary>
    /// 计算矩阵的逆矩阵。
    /// </summary>
    /// <returns>逆矩阵。</returns>
    /// <exception cref="InvalidOperationException">当矩阵不是方阵或不可逆时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix Inverse()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("矩阵必须是方阵才能计算逆矩阵。");

        float det = Determinant();
        if (Math.Abs(det) < 1e-10f)
            throw new InvalidOperationException("矩阵不可逆。");

        var result = new Matrix(_rows, _columns);
        var adjoint = Adjoint(_values.AsSpan(), _rows);
        for (int i = 0; i < _rows; i++)
            for (int j = 0; j < _columns; j++)
                result[i, j] = adjoint[i * _columns + j] / det;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float[] Adjoint(Span<float> values, int n)
    {
        var adjoint = new float[n * n];
        var subMatrix = new float[(n - 1) * (n - 1)];
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
                adjoint[j * n + i] = CalculateDeterminant(subMatrix, n - 1) * ((i + j) % 2 == 0 ? 1 : -1);
            }
        }
        return adjoint;
    }

    #endregion

    #region 分解方法

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
    public void LUDecomposition(out Matrix L, out Matrix U, out int[] P)
    {
        if (_rows != _columns)
            throw new InvalidOperationException("矩阵必须是方阵。");

        int n = _rows;
        L = new Matrix(n, n);
        U = new Matrix(n, n);
        P = new int[n];
        var A = Clone();

        for (int i = 0; i < n; i++) P[i] = i;

        for (int k = 0; k < n; k++)
        {
            float max = Math.Abs(A[k, k]);
            int pivot = k;
            for (int i = k + 1; i < n; i++)
                if (Math.Abs(A[i, k]) > max)
                {
                    max = Math.Abs(A[i, k]);
                    pivot = i;
                }

            if (max < 1e-10f)
                throw new InvalidOperationException("矩阵不可逆。");

            if (pivot != k)
            {
                SwapRows(A, k, pivot);
                (P[k], P[pivot]) = (P[pivot], P[k]);
            }

            L[k, k] = 1;
            for (int i = k + 1; i < n; i++)
            {
                L[i, k] = A[i, k] / A[k, k];
                for (int j = k + 1; j < n; j++)
                    A[i, j] -= L[i, k] * A[k, j];
                A[i, k] = 0;
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
    public void QRDecomposition(out Matrix Q, out Matrix R)
    {
        int m = _rows;
        int n = _columns;
        Q = new Matrix(m, m);
        R = new Matrix(m, n);

        var A = _values.ToArray();
        for (int k = 0; k < n; k++)
        {
            float norm = 0;
            for (int i = 0; i < m; i++)
                norm += A[i * n + k] * A[i * n + k];
            norm = MathF.Sqrt(norm);

            R[k, k] = norm;
            for (int i = 0; i < m; i++)
                Q[i, k] = A[i * n + k] / norm;

            for (int j = k + 1; j < n; j++)
            {
                float dotProduct = 0;
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
    public Matrix CholeskyDecomposition()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("矩阵必须是方阵。");

        var L = new Matrix(_rows, _columns);
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                float sum = 0;
                for (int k = 0; k < j; k++)
                    sum += L[i, k] * L[j, k];

                if (i == j)
                {
                    float diag = this[i, i] - sum;
                    if (diag <= 0)
                        throw new InvalidOperationException("矩阵不是正定矩阵。");
                    L[i, j] = MathF.Sqrt(diag);
                }
                else
                    L[i, j] = (this[i, j] - sum) / L[j, j];
            }
        }
        return L;
    }

    #endregion

    #region 实用方法

    /// <summary>
    /// 创建一个单位矩阵。
    /// </summary>
    /// <param name="size">矩阵的大小（行数和列数），必须为正整数。</param>
    /// <returns>单位矩阵。</returns>
    /// <exception cref="ArgumentException">当 <paramref name="size"/> 小于等于 0 时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix Eye(int size)
    {
        if (size <= 0)
            throw new ArgumentException("矩阵大小必须为正整数。");
        var eye = new Matrix(size);
        for (int i = 0; i < size; i++)
            eye[i, i] = 1;
        return eye;
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
    public static Matrix Companion(float[] a)
    {
        if (a.Length < 2)
            throw new ArgumentException("输入数组长度必须至少为 2。");
        if (Math.Abs(a[0]) < 1e-30f)
            throw new ArgumentException("第一个系数不能为零。");

        int size = a.Length - 1;
        var companion = new Matrix(size);
        for (int i = 0; i < size; i++)
            companion[0, i] = -a[i + 1] / a[0];
        for (int i = 1; i < size; i++)
            companion[i, i - 1] = 1;
        return companion;
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
            span[i] = Random.Shared.NextSingle();
    }

    /// <summary>
    /// 创建矩阵的深拷贝。
    /// </summary>
    /// <returns>矩阵的深拷贝。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix Clone()
    {
        var clone = new Matrix(_rows, _columns);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void VectorAdd(ReadOnlySpan<float> a, ReadOnlySpan<float> b, Span<float> result)
    {
        int vectorSize = Vector<float>.Count;
        int i = 0;
        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = new Vector<float>(a.Slice(i));
            var vb = new Vector<float>(b.Slice(i));
            (va + vb).CopyTo(result.Slice(i));
        }
        for (; i < a.Length; i++)
            result[i] = a[i] + b[i];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void VectorSubtract(ReadOnlySpan<float> a, ReadOnlySpan<float> b, Span<float> result)
    {
        int vectorSize = Vector<float>.Count;
        int i = 0;
        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = new Vector<float>(a.Slice(i));
            var vb = new Vector<float>(b.Slice(i));
            (va - vb).CopyTo(result.Slice(i));
        }
        for (; i < a.Length; i++)
            result[i] = a[i] - b[i];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void VectorMultiplyScalar(ReadOnlySpan<float> source, float scalar, Span<float> result)
    {
        int vectorSize = Vector<float>.Count;
        var vScalar = new Vector<float>(scalar);
        int i = 0;
        for (; i <= source.Length - vectorSize; i += vectorSize)
        {
            var vSource = new Vector<float>(source.Slice(i));
            (vSource * vScalar).CopyTo(result.Slice(i));
        }
        for (; i < source.Length; i++)
            result[i] = source[i] * scalar;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void VectorDivideScalar(ReadOnlySpan<float> source, float scalar, Span<float> result)
    {
        int vectorSize = Vector<float>.Count;
        var vScalar = new Vector<float>(scalar);
        int i = 0;
        for (; i <= source.Length - vectorSize; i += vectorSize)
        {
            var vSource = new Vector<float>(source.Slice(i));
            (vSource / vScalar).CopyTo(result.Slice(i));
        }
        for (; i < source.Length; i++)
            result[i] = source[i] / scalar;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SwapRows(Matrix matrix, int row1, int row2)
    {
        var span = matrix._values.AsSpan();
        int start1 = row1 * matrix.Columns;
        int start2 = row2 * matrix.Columns;
        for (int j = 0; j < matrix.Columns; j++)
            (span[start1 + j], span[start2 + j]) = (span[start2 + j], span[start1 + j]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateIndices(int row, int column)
    {
        if ((uint)row >= (uint)_rows || (uint)column >= (uint)_columns)
            throw new IndexOutOfRangeException("索引超出范围。");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateRowIndex(int row) => ValidateIndices(row, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateColumnIndex(int column) => ValidateIndices(0, column);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateDimensions(Matrix a, Matrix b, string operation)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException($"矩阵维度不匹配，无法执行 {operation}。");
    }

    #endregion
}