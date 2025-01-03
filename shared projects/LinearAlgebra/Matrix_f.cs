
namespace Vorcyc.Mathematics.LinearAlgebra;

using System.Text;
using Vorcyc.Mathematics.Framework;

/// <summary>
/// 表示一个二维矩阵。
/// </summary>
public class Matrix : ICloneable<Matrix>
{

    private Memory<float> _values;
    private int _rows;
    private int _columns;

    /// <summary>
    /// Gets the number of rows in the matrix.
    /// </summary>
    public int Rows => _rows;

    /// <summary>
    /// Gets the number of columns in the matrix.
    /// </summary>
    public int Columns => _columns;


    #region 构造器

    /// <summary>
    /// Constructs a matrix with the specified number of rows and columns.
    /// </summary>
    /// <param name="rows">Number of rows.</param>
    /// <param name="columns">Number of columns.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(int rows, int columns)
    {
        if (rows <= 0 || columns <= 0)
            throw new ArgumentException("Rows and columns must be positive integers.");

        _rows = rows;
        _columns = columns;
        _values = new Memory<float>(new float[rows * columns]);
    }

    /// <summary>
    /// Constructs a square matrix with the specified size.
    /// </summary>
    /// <param name="size">Size of the matrix (number of rows and columns).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(int size) : this(size, size) { }

    /// <summary>
    /// Constructs a matrix from a 2D array.
    /// </summary>
    /// <param name="initialValues">2D array of initial values.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(float[,] initialValues)
    {
        _rows = initialValues.GetLength(0);
        _columns = initialValues.GetLength(1);
        _values = new Memory<float>(new float[_rows * _columns]);

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
    /// Gets or sets the element at the specified position.
    /// </summary>
    /// <param name="row">Row index.</param>
    /// <param name="column">Column index.</param>
    /// <returns>The element at the specified position.</returns>
    public ref float this[int row, int column]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (row < 0 || row >= _rows || column < 0 || column >= _columns)
                throw new IndexOutOfRangeException("Row or column is out of range.");
            return ref _values.Span[row * _columns + column];
        }
    }

    #endregion


    #region Implicit Conversions

    /// <summary>
    /// Implicitly converts the matrix to a 2D array.
    /// </summary>
    /// <param name="matrix">The matrix to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float[,](Matrix matrix)
    {
        float[,] result = new float[matrix.Rows, matrix.Columns];
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
    /// Implicitly converts the matrix to a jagged array.
    /// </summary>
    /// <param name="matrix">The matrix to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float[][](Matrix matrix)
    {
        float[][] result = new float[matrix.Rows][];
        for (int i = 0; i < matrix.Rows; i++)
        {
            result[i] = new float[matrix.Columns];
            for (int j = 0; j < matrix.Columns; j++)
            {
                result[i][j] = matrix[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// Implicitly converts a jagged array to a matrix.
    /// </summary>
    /// <param name="values">The jagged array to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Matrix(float[][] values)
    {
        int rows = values.Length;
        int columns = values[0].Length;
        float[,] array = new float[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                array[i, j] = values[i][j];
            }
        }

        return new Matrix(array);
    }

    /// <summary>
    /// Implicitly converts a 2D array to a matrix.
    /// </summary>
    /// <param name="values">The 2D array to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Matrix(float[,] values)
    {
        return new Matrix(values);
    }

    #endregion


    #region Operators

    /// <summary>
    /// Matrix addition operator.
    /// </summary>
    /// <param name="a">The first matrix.</param>
    /// <param name="b">The second matrix.</param>
    /// <returns>The sum of the two matrices.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix operator +(Matrix a, Matrix b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException("Matrices must have the same dimensions for addition.");

        Matrix result = new(a.Rows, a.Columns);
        int length = a._values.Length;
        int vectorSize = System.Numerics.Vector<float>.Count;

        int i;
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var va = new System.Numerics.Vector<float>(a._values.Span.Slice(i, vectorSize));
            var vb = new System.Numerics.Vector<float>(b._values.Span.Slice(i, vectorSize));
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
    /// Matrix subtraction operator.
    /// </summary>
    /// <param name="a">The first matrix.</param>
    /// <param name="b">The second matrix.</param>
    /// <returns>The difference of the two matrices.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix operator -(Matrix a, Matrix b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException("Matrices must have the same dimensions for subtraction.");

        Matrix result = new(a.Rows, a.Columns);
        int length = a._values.Length;
        int vectorSize = System.Numerics.Vector<float>.Count;

        int i;
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var va = new System.Numerics.Vector<float>(a._values.Span.Slice(i, vectorSize));
            var vb = new System.Numerics.Vector<float>(b._values.Span.Slice(i, vectorSize));
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
    /// Matrix multiplication operator.
    /// </summary>
    /// <param name="a">The first matrix.</param>
    /// <param name="b">The second matrix.</param>
    /// <returns>The product of the two matrices.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix operator *(Matrix a, Matrix b)
    {
        if (a.Columns != b.Rows)
            throw new ArgumentException("Matrices must have compatible dimensions for multiplication.");

        Matrix result = new(a.Rows, b.Columns);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < b.Columns; j++)
            {
                float sum = 0;
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
    /// Matrix-scalar multiplication operator.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The product of the matrix and the scalar.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix operator *(Matrix matrix, float scalar)
    {
        Matrix result = new(matrix.Rows, matrix.Columns);
        int length = matrix._values.Length;
        int vectorSize = System.Numerics.Vector<float>.Count;

        int i;
        var vScalar = new System.Numerics.Vector<float>(scalar);
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var vm = new System.Numerics.Vector<float>(matrix._values.Span.Slice(i, vectorSize));
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
    /// Matrix-scalar division operator.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The quotient of the matrix and the scalar.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix operator /(Matrix matrix, float scalar)
    {
        Matrix result = new(matrix.Rows, matrix.Columns);
        int length = matrix._values.Length;
        int vectorSize = System.Numerics.Vector<float>.Count;

        int i;
        var vScalar = new System.Numerics.Vector<float>(scalar);
        for (i = 0; i <= length - vectorSize; i += vectorSize)
        {
            var vm = new System.Numerics.Vector<float>(matrix._values.Span.Slice(i, vectorSize));
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
    /// Gets the elements of the specified row.
    /// </summary>
    /// <param name="rowIndex">Row index.</param>
    /// <returns>A span of the elements in the specified row.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<float> GetRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= _rows)
            throw new IndexOutOfRangeException("Row index is out of range.");

        return _values.Span.Slice(rowIndex * _columns, _columns);
    }

    /// <summary>
    /// Gets the elements of the specified column.
    /// </summary>
    /// <param name="columnIndex">Column index.</param>
    /// <returns>A read-only span of the elements in the specified column.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<float> GetColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= _columns)
            throw new IndexOutOfRangeException("Column index is out of range.");

        float[] column = new float[_rows];
        for (int i = 0; i < _rows; i++)
        {
            column[i] = this[i, columnIndex];
        }
        return new ReadOnlySpan<float>(column);
    }

    #endregion


    #region Matrix Operations

    /// <summary>
    /// Transposes the matrix.
    /// </summary>
    /// <returns>The transposed matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix Transpose()
    {
        Matrix result = new(_columns, _rows);
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
    /// Calculates the determinant of the matrix.
    /// </summary>
    /// <returns>The determinant of the matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Determinant()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("Matrix must be square to calculate determinant.");

        return CalculateDeterminant(_values.Span, _rows);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float CalculateDeterminant(Span<float> values, int n)
    {
        if (n == 1)
            return values[0];
        if (n == 2)
            return values[0] * values[3] - values[1] * values[2];

        float det = 0;
        float[] subMatrix = new float[(n - 1) * (n - 1)];
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
            det += values[p] * CalculateDeterminant(subMatrix, n - 1) * (p % 2 == 0 ? 1 : -1);
        }
        return det;
    }

    /// <summary>
    /// Calculates the inverse of the matrix.
    /// </summary>
    /// <returns>The inverse of the matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix Inverse()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("Matrix must be square to calculate inverse.");

        float det = Determinant();

        if (Math.Abs(det) < 1e-10)
            throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

        Matrix result = new(_rows, _columns);
        float[] adjoint = Adjoint(_values.Span, _rows);
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
    private float[] Adjoint(Span<float> values, int n)
    {
        float[] adjoint = new float[n * n];
        float[] subMatrix = new float[(n - 1) * (n - 1)];
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
                adjoint[j * n + i] = CalculateDeterminant(subMatrix, n - 1) * ((i + j) % 2 == 0 ? 1 : -1);
            }
        }
        return adjoint;
    }

    #endregion


    #region Decomposition

    /// <summary>
    /// LU decomposition.
    /// </summary>
    /// <param name="L">Output lower triangular matrix.</param>
    /// <param name="U">Output upper triangular matrix.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LUDecomposition(out Matrix L, out Matrix U)
    {
        int n = _rows;
        L = new Matrix(n, n);
        U = new Matrix(n, n);

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
                        L[i, j] = 1;
                    }
                    else
                    {
                        L[i, j] = 0;
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
                    U[i, j] = 0;
                }
            }
        }
    }

    /// <summary>
    /// QR decomposition.
    /// </summary>
    /// <param name="Q">Output orthogonal matrix.</param>
    /// <param name="R">Output upper triangular matrix.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void QRDecomposition(out Matrix Q, out Matrix R)
    {
        int m = _rows;
        int n = _columns;
        Q = new Matrix(m, m);
        R = new Matrix(m, n);

        float[] A = _values.ToArray();

        for (int k = 0; k < n; k++)
        {
            float norm = 0;
            for (int i = 0; i < m; i++)
            {
                norm += A[i * n + k] * A[i * n + k];
            }

            norm = MathF.Sqrt(norm);

            R[k, k] = norm;
            for (int i = 0; i < m; i++)
            {
                Q[i, k] = A[i * n + k] / norm;
            }

            for (int j = k + 1; j < n; j++)
            {
                float dotProduct = 0;
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
    /// Cholesky decomposition.
    /// </summary>
    /// <returns>Lower triangular matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix CholeskyDecomposition()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("Matrix must be square for Cholesky decomposition.");

        Matrix L = new(_rows, _columns);

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                float sum = 0f;
                for (int k = 0; k < j; k++)
                {
                    sum += L[i, k] * L[j, k];
                }

                if (i == j)
                {
                    L[i, j] = MathF.Sqrt(_values.Span[i * _columns + i] - sum);
                }
                else
                {
                    L[i, j] = (_values.Span[i * _columns + j] - sum) / L[j, j];
                }
            }
        }

        return L;
    }

    #endregion


    #region Utility Methods

    /// <summary>
    /// Creates an identity matrix of the specified size.
    /// </summary>
    /// <param name="size">The size of the matrix (number of rows and columns).</param>
    /// <returns>The identity matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix Eye(int size)
    {
        var eye = new Matrix(size);

        for (var i = 0; i < size; i++)
        {
            eye[i, i] = 1;
        }

        return eye;
    }

    /// <summary>
    /// 创建一个伴随矩阵。
    /// </summary>
    /// <param name="a">输入数组，表示多项式的系数。</param>
    /// <returns>伴随矩阵。</returns>
    /// <exception cref="ArgumentException">当输入数组的长度小于2或第一个系数为零时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix Companion(float[] a)
    {
        if (a.Length < 2)
        {
            throw new ArgumentException("The size of input array must be at least 2!");
        }

        if (Math.Abs(a[0]) < 1e-30)
        {
            throw new ArgumentException("The first coefficient must not be zero!");
        }

        var size = a.Length - 1;

        var companion = new Matrix(size);

        for (var i = 0; i < size; i++)
        {
            companion.GetRow(0)[i] = -a[i + 1] / a[0];
        }

        for (var i = 1; i < size; i++)
        {
            companion.GetRow(i)[i - 1] = 1;
        }

        return companion;
    }

    /// <summary>
    /// Fills the matrix with random values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FillRandom()
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values.Span[i] = Random.Shared.NextSingle();
        }
    }

    /// <summary>
    /// Creates a deep copy of the matrix.
    /// </summary>
    /// <returns>A deep copy of the matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix Clone()
    {
        Matrix clone = new(_rows, _columns);
        _values.CopyTo(clone._values);
        return clone;
    }

    /// <summary>
    /// Returns a string representation of the matrix.
    /// </summary>
    /// <returns>A string representation of the matrix.</returns>
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