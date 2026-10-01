namespace Vorcyc.Mathematics.LinearAlgebra;
using System.Numerics;
using Vorcyc.Mathematics;
using System.Text;
/// <summary>
/// Represents a 2D matrix providing efficient matrix operations and decomposition methods.
/// </summary>
/// <typeparam name="T">The numeric type, must implement <see cref="INumber{T}"/> and <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
/// <remarks>
/// This class supports basic matrix operations (addition, subtraction, multiplication), matrix decompositions (LU, QR, Cholesky), and utility methods.
/// Performance is optimized using vectorized operations and memory efficiency, suitable for numerical computing and machine learning tasks.
/// </remarks>
public class Matrix<T> : ICloneable<Matrix<T>>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly T[] _values; // 存储矩阵元素的连续数组
    private readonly int _rows;   // 矩阵行数
    private readonly int _columns; // 矩阵列数
    /// <summary>
    /// Gets the number of rows in the matrix.
    /// </summary>
    public int Rows => _rows;
    /// <summary>
    /// Gets the number of columns in the matrix.
    /// </summary>
    public int Columns => _columns;
    /// <summary>
    /// Gets the underlying data array (internal use only).
    /// </summary>
    /// <returns>The underlying data as a 1D array.</returns>
    internal T[] GetInternalData() => _values;
    #region Constructors
    /// <summary>
    /// Constructs a matrix with the specified number of rows and columns.
    /// </summary>
    /// <param name="rows">The number of rows, must be a positive integer.</param>
    /// <param name="columns">The number of columns, must be a positive integer.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="rows"/> or <paramref name="columns"/> is less than or equal to 0.</exception>
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
    /// Constructs a square matrix with the specified size.
    /// </summary>
    /// <param name="size">The size of the matrix (rows and columns), must be a positive integer.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="size"/> is less than or equal to 0.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix(int size) : this(size, size) { }
    ///// <summary>
    ///// Constructs a matrix from a 2D array.
    ///// </summary>
    ///// <param name="initialValues">The initial data as a 2D array.</param>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public Matrix(T[,] initialValues)
    //{
    //    _rows = initialValues.GetLength(0);
    //    _columns = initialValues.GetLength(1);
    //    _values = new T[_rows * _columns];
    //    initialValues.CopyTo(_values, 0);
    //}
    /// <summary>
    /// Constructs a matrix from a 2D array.
    /// </summary>
    /// <param name="initialValues">The initial data as a 2D array.</param>
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
    /// Gets or sets the element at the specified position.
    /// </summary>
    /// <param name="row">The row index, zero-based.</param>
    /// <param name="column">The column index, zero-based.</param>
    /// <returns>A reference to the element at the specified position.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="row"/> or <paramref name="column"/> is out of range.</exception>
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
    #region Implicit Conversions
    /// <summary>
    /// Implicitly converts to a 2D array <see cref="T[,]"/>.
    /// </summary>
    /// <param name="matrix">The matrix to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T[,](Matrix<T> matrix)
    {
        var result = new T[matrix.Rows, matrix.Columns];
        matrix._values.CopyTo(result, 0);
        return result;
    }
    /// <summary>
    /// Implicitly converts to a jagged array <see cref="T[][]"/>.
    /// </summary>
    /// <param name="matrix">The matrix to convert.</param>
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
    /// Implicitly converts from a jagged array <see cref="T[][]"/> to a matrix.
    /// </summary>
    /// <param name="values">The initial data as a jagged array.</param>
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
    /// Implicitly converts from a 2D array <see cref="T[,]"/> to a matrix.
    /// </summary>
    /// <param name="values">The initial data as a 2D array.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Matrix<T>(T[,] values) => new Matrix<T>(values);
    #endregion
    #region operators
    /// <summary>
    /// Matrix addition operator.
    /// </summary>
    /// <param name="a">The first matrix.</param>
    /// <param name="b">The second matrix.</param>
    /// <returns>The sum of the two matrices.</returns>
    /// <exception cref="ArgumentException">Thrown when the matrix dimensions do not match.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
    {
        ValidateDimensions(a, b, "addition");
        var result = new Matrix<T>(a.Rows, a.Columns);
        VectorAdd(a._values, b._values, result._values);
        return result;
    }
    /// <summary>
    /// Matrix subtraction operator.
    /// </summary>
    /// <param name="a">The first matrix.</param>
    /// <param name="b">The second matrix.</param>
    /// <returns>The difference of the two matrices.</returns>
    /// <exception cref="ArgumentException">Thrown when the matrix dimensions do not match.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
    {
        ValidateDimensions(a, b, "subtraction");
        var result = new Matrix<T>(a.Rows, a.Columns);
        VectorSubtract(a._values, b._values, result._values);
        return result;
    }
    /// <summary>
    /// Matrix multiplication operator.
    /// </summary>
    /// <param name="a">The first matrix.</param>
    /// <param name="b">The second matrix.</param>
    /// <returns>The product of the two matrices.</returns>
    /// <exception cref="ArgumentException">Thrown when the matrix dimensions do not match.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b) => Multiply(a, b);
    /// <summary>
    /// Multiplies two matrices with an optional execution policy.
    /// </summary>
    public static Matrix<T> Multiply(Matrix<T> a, Matrix<T> b, ComputingContext? context = null)
    {
        if (a.Columns != b.Rows)
            throw new ArgumentException("Matrix dimensions do not match; cannot multiply.");
        var result = new Matrix<T>(a.Rows, b.Columns);
        MatrixMultiply.Multiply(a._values, a.Rows, a.Columns, b._values, b.Rows, b.Columns, result._values, context);
        return result;
    }
    /// <summary>
    /// Matrix-scalar multiplication operator.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The product of the matrix and the scalar.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator *(Matrix<T> matrix, T scalar)
    {
        var result = new Matrix<T>(matrix.Rows, matrix.Columns);
        VectorMultiplyScalar(matrix._values, scalar, result._values);
        return result;
    }
    /// <summary>
    /// Matrix-scalar division operator.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The quotient of the matrix and the scalar.</returns>
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
    /// Gets the elements of the specified row.
    /// </summary>
    /// <param name="rowIndex">The row index, zero-based.</param>
    /// <returns>A <see cref="Span{T}"/> of the elements in the specified row.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="rowIndex"/> is out of range.</exception>
    /// <remarks>
    /// The returned <see cref="Span{T}"/> references the underlying array directly, avoiding copies and improving performance.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetRow(int rowIndex)
    {
        ValidateRowIndex(rowIndex);
        return _values.AsSpan(rowIndex * _columns, _columns);
    }
    /// <summary>
    /// Gets the elements of the specified column.
    /// </summary>
    /// <param name="columnIndex">The column index, zero-based.</param>
    /// <returns>An array of the elements in the specified column.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="columnIndex"/> is out of range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] GetColumn(int columnIndex)
    {
        ValidateColumnIndex(columnIndex);
        var column = new T[_rows];
        for (int i = 0; i < _rows; i++)
            column[i] = this[i, columnIndex];
        return column;
    }
    /// <summary>
    /// Computes the product of the matrix and a column vector, writing the result into <paramref name="result"/>.
    /// </summary>
    /// <param name="vector">The column vector, its length must equal the number of matrix columns.</param>
    /// <param name="result">The result vector, its length must equal the number of matrix rows.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Multiply(ReadOnlySpan<T> vector, Span<T> result)
        => Multiply(vector, result, context: null);
    /// <summary>
    /// Computes the product of the matrix and a column vector, writing the result into <paramref name="result"/>.
    /// </summary>
    public void Multiply(ReadOnlySpan<T> vector, Span<T> result, ComputingContext? context)
    {
        if (vector.Length != _columns)
            throw new ArgumentException("Vector length must match the number of matrix columns.", nameof(vector));
        if (result.Length != _rows)
            throw new ArgumentException("Result vector length must match the number of matrix rows.", nameof(result));
        int problemSize = _rows * _columns;
        if (ComputingContextExecution.UseParallel(context, problemSize, ComputingContextExecution.ParallelMatrixMultiplyThreshold))
        {
            var matrix = _values;
            var vectorData = vector.ToArray();
            var buffer = GC.AllocateUninitializedArray<T>(_rows);
            ComputingContextExecution.ForEach(context, 0, _rows, i =>
            {
                int row = i * _columns;
                T sum = T.Zero;
                for (int j = 0; j < _columns; j++)
                {
                    sum += matrix[row + j] * vectorData[j];
                }
                buffer[i] = sum;
            }, _columns);
            buffer.AsSpan().CopyTo(result);
            return;
        }
        for (int i = 0; i < _rows; i++)
        {
            result[i] = VectorSpan.Dot(_values.AsSpan(i * _columns, _columns), vector, context);
        }
    }
    /// <summary>
    /// Computes the product of the matrix and a column vector.
    /// </summary>
    /// <param name="vector">The column vector, its length must equal the number of matrix columns.</param>
    /// <returns>The result vector, with length equal to the number of matrix rows.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] Multiply(ReadOnlySpan<T> vector)
    {
        var result = new T[_rows];
        Multiply(vector, result);
        return result;
    }
    #endregion
    #region Matrix Operations
    /// <summary>
    /// Transposes the matrix.
    /// </summary>
    /// <returns>The transposed matrix.</returns>
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
    /// Computes the determinant of the matrix.
    /// </summary>
    /// <returns>The determinant of the matrix.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the matrix is not square.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Determinant()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("The matrix must be square to compute the determinant.");
        return CalculateDeterminant(_values.AsSpan(), _rows);
    }
    /// <summary>
    /// Recursively computes the determinant of the matrix (internal method).
    /// </summary>
    /// <param name="values">A span of the matrix element data.</param>
    /// <param name="n">The order of the matrix.</param>
    /// <returns>The determinant value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T CalculateDeterminant(Span<T> values, int n)
    {
        if (n == 0) return T.One; // Empty-matrix determinant is the multiplicative identity; 1x1 Inverse() relies on this base case
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
    /// Computes the inverse of the matrix.
    /// </summary>
    /// <returns>The inverse matrix.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the matrix is not square or is not invertible.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix<T> Inverse()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("The matrix must be square to compute the inverse.");
        T det = Determinant();
        if (T.Abs(det) < T.CreateChecked(1e-10))
            throw new InvalidOperationException("The matrix is not invertible.");
        var result = new Matrix<T>(_rows, _columns);
        var adjoint = Adjoint(_values.AsSpan(), _rows);
        for (int i = 0; i < _rows; i++)
            for (int j = 0; j < _columns; j++)
                result[i, j] = adjoint[i * _columns + j] / det;
        return result;
    }
    /// <summary>
    /// Computes the adjugate matrix (internal method).
    /// </summary>
    /// <param name="values">A span of the matrix element data.</param>
    /// <param name="n">The order of the matrix.</param>
    /// <returns>The element array of the adjugate matrix.</returns>
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
    /// Performs LU decomposition (with partial pivoting).
    /// </summary>
    /// <param name="L">The output lower triangular matrix.</param>
    /// <param name="U">The output upper triangular matrix.</param>
    /// <param name="P">The output permutation vector representing the row swap order.</param>
    /// <exception cref="InvalidOperationException">Thrown when the matrix is not square or is not invertible.</exception>
    /// <remarks>
    /// This method uses partial pivoting to improve numerical stability; the decomposition satisfies PA = LU.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LUDecomposition(out Matrix<T> L, out Matrix<T> U, out int[] P)
    {
        if (_rows != _columns)
            throw new InvalidOperationException("The matrix must be square.");
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
            if (max < T.CreateChecked(1e-6))
                throw new InvalidOperationException("The matrix is not invertible.");
            if (pivot != k)
            {
                SwapRows(A, k, pivot);
                for (int j = 0; j < k; j++)
                    (L[k, j], L[pivot, j]) = (L[pivot, j], L[k, j]);
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
    /// Performs QR decomposition.
    /// </summary>
    /// <param name="Q">The output orthogonal matrix.</param>
    /// <param name="R">The output upper triangular matrix.</param>
    /// <remarks>
    /// Decomposes the matrix using Gram-Schmidt orthogonalization, satisfying A = QR.
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
    /// Performs Cholesky decomposition.
    /// </summary>
    /// <returns>The lower triangular matrix L, satisfying A = LLᵀ.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the matrix is not square or is not positive definite.</exception>
    /// <remarks>
    /// This method applies to symmetric positive-definite matrices.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix<T> CholeskyDecomposition()
    {
        if (_rows != _columns)
            throw new InvalidOperationException("The matrix must be square.");
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
                        throw new InvalidOperationException("The matrix is not positive definite.");
                    L[i, j] = T.Sqrt(diag);
                }
                else
                    L[i, j] = (this[i, j] - sum) / L[j, j];
            }
        }
        return L;
    }
    #endregion
    #region Linear Solving
    /// <summary>
    /// Solves the square linear system Ax = b (using LU decomposition).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] Solve(T[] b) => LinearEquationSolver.LUSolve(this, b);
    /// <summary>
    /// Solves the square linear system Ax = b, writing the result into <paramref name="x"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Solve(ReadOnlySpan<T> b, Span<T> x)
        => LinearEquationSolver.GaussianEliminationSolve(this, b, x);
    /// <summary>
    /// Solves the symmetric positive-definite system Ax = b (using Cholesky decomposition).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] SolveSymmetricPositiveDefinite(T[] b)
        => LinearEquationSolver.CholeskySolve(this, b);
    /// <summary>
    /// Solves least squares min ‖Ax - y‖² treating this matrix as the design matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] SolveLeastSquares(ReadOnlySpan<T> observations)
        => LinearEquationSolver.SolveLeastSquares(this, observations);
    /// <summary>
    /// Solves least squares using the thin-SVD pseudoinverse, suitable for rank-deficient or ill-conditioned design matrices.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] SolveLeastSquaresSvd(ReadOnlySpan<T> observations, T? tolerance = null)
        => LinearEquationSolver.SolveLeastSquaresSvd(this, observations, tolerance);
    /// <summary>
    /// Solves ridge regression min ‖Ax - y‖² + λ‖β‖².
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] SolveRidgeLeastSquares(
        ReadOnlySpan<T> observations,
        T lambda,
        bool regularizeIntercept = true)
        => LinearEquationSolver.SolveRidgeLeastSquares(this, observations, lambda, regularizeIntercept);
    #endregion
    #region Utility Methods
    /// <summary>
    /// Estimates κ₂(A) ≈ σ_max / σ_min from a thin SVD.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ConditionNumber(T? tolerance = null)
        => MatrixDiagnostics.ConditionNumber(this, tolerance);
    /// <summary>
    /// Returns true when the estimated condition number exceeds <paramref name="threshold"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsIllConditioned(T threshold, T? tolerance = null)
        => MatrixDiagnostics.IsIllConditioned(this, threshold, tolerance);
    /// <summary>
    /// Checks whether this square matrix is symmetric within <paramref name="tolerance"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSymmetric(T? tolerance = null)
        => MatrixDiagnostics.IsSymmetric(this, tolerance);
    /// <summary>
    /// Creates an identity matrix.
    /// </summary>
    /// <param name="size">The size of the matrix (rows and columns), must be a positive integer.</param>
    /// <returns>The identity matrix.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="size"/> is less than or equal to 0.</exception>
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
    /// Fills the matrix with random numbers.
    /// </summary>
    /// <remarks>
    /// Random numbers are in the range [0, 1), generated by <see cref="Random.Shared"/>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FillRandom()
    {
        var span = _values.AsSpan();
        for (int i = 0; i < span.Length; i++)
            span[i] = T.CreateTruncating(Random.Shared.NextDouble());
    }
    /// <summary>
    /// Creates a companion matrix.
    /// </summary>
    /// <param name="a">The input array representing the polynomial coefficients.</param>
    /// <returns>The companion matrix.</returns>
    /// <exception cref="ArgumentException">Thrown when the input array length is less than 2 or the first coefficient is near zero.</exception>
    /// <remarks>
    /// The companion matrix is used to represent the eigenvalue problem of a polynomial.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> Companion(T[] a)
    {
        if (a.Length < 2)
            throw new ArgumentException("Input array length must be at least 2.");
        if (T.Abs(a[0]) < T.CreateSaturating(1e-30))
            throw new ArgumentException("The first coefficient cannot be zero.");
        int size = a.Length - 1;
        var companion = new Matrix<T>(size);
        for (int i = 0; i < size; i++)
            companion[0, i] = -a[i + 1] / a[0];
        for (int i = 1; i < size; i++)
            companion[i, i - 1] = T.One;
        return companion;
    }
    /// <summary>
    /// Creates a deep copy of the matrix.
    /// </summary>
    /// <returns>A deep copy of the matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix<T> Clone()
    {
        var clone = new Matrix<T>(_rows, _columns);
        _values.CopyTo(clone._values, 0);
        return clone;
    }
    /// <summary>
    /// Returns the string representation of the matrix.
    /// </summary>
    /// <returns>The string representation of the matrix, with rows separated by newlines and elements separated by commas.</returns>
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
    #region Private Helper Methods
    /// <summary>
    /// Performs matrix addition using vectorized operations.
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
    /// Performs matrix subtraction using vectorized operations.
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
    /// Performs matrix-scalar multiplication using vectorized operations.
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
    /// Performs matrix-scalar division using vectorized operations.
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
    /// Swaps two rows of the matrix.
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
    /// Validates that the indices are within the valid range.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateIndices(int row, int column)
    {
        if ((uint)row >= (uint)_rows || (uint)column >= (uint)_columns)
            throw new IndexOutOfRangeException("Index out of range.");
    }
    /// <summary>
    /// Validates that the row index is valid.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateRowIndex(int row) => ValidateIndices(row, 0);
    /// <summary>
    /// Validates that the column index is valid.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateColumnIndex(int column) => ValidateIndices(0, column);
    /// <summary>
    /// Checks whether the value is a positive integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GuardAgainstNonPositive(int value, string name)
    {
        if (value <= 0)
            throw new ArgumentException($"{name} must be a positive integer.");
    }
    /// <summary>
    /// Validates that the dimensions of two matrices match.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateDimensions(Matrix<T> a, Matrix<T> b, string operation)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new ArgumentException($"Matrix dimensions do not match; cannot perform {operation}.");
    }
    #endregion
}
