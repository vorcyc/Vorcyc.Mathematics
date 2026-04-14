using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.Calculus.Series;

/// <summary>
/// Padé 构造工作区，复用线性求解缓冲。
/// </summary>
public sealed class PadeWorkspace<T> where T : struct, IFloatingPointIeee754<T>
{
    private Matrix<T>? _matrix;
    private T[]? _rhs;
    private T[]? _solution;
    private T[,]? _augmented;
    private T[]? _taylor;
    private int _capacity;

    internal void EnsureTaylor(int taylorLength)
    {
        if (_taylor is null || _taylor.Length < taylorLength)
            _taylor = new T[taylorLength];
    }

    internal void EnsureSolve(int n)
    {
        if (n <= 0)
            return;

        if (_matrix is not null && _capacity >= n)
            return;

        _capacity = n;
        _matrix = new Matrix<T>(n, n);
        _rhs = new T[n];
        _solution = new T[n];
        _augmented = new T[n, n + 1];
    }

    internal Matrix<T> Matrix(int n) => _matrix!;
    internal Span<T> Rhs(int n) => _rhs!.AsSpan(0, n);
    internal Span<T> Solution(int n) => _solution!.AsSpan(0, n);
    internal T[,] Augmented(int n) => _augmented!;
    internal Span<T> Taylor(int length) => _taylor!.AsSpan(0, length);
}

/// <summary>
/// [m/n] Padé 有理逼近，由泰勒系数构造。
/// </summary>
public sealed class PadeApproximant<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly T[] _pCoeffs;
    private readonly T[] _qCoeffs;

    /// <summary>
    /// 由泰勒系数 c₀,c₁,…,c_{m+n} 构造 [m/n] Padé 逼近（Q(0)=1）。
    /// </summary>
    public PadeApproximant(ReadOnlySpan<T> taylorCoefficients, int m, int n) :
        this(taylorCoefficients, m, n, null)
    {
    }

    /// <summary>
    /// 由泰勒系数构造；可选 <paramref name="workspace"/> 复用求解缓冲。
    /// </summary>
    public PadeApproximant(ReadOnlySpan<T> taylorCoefficients, int m, int n, PadeWorkspace<T>? workspace)
    {
        if (m < 0 || n < 0) throw new ArgumentException("m、n 必须非负");
        if (taylorCoefficients.Length < m + n + 1)
            throw new ArgumentException("泰勒系数数量不足");

        _pCoeffs = new T[m + 1];
        _qCoeffs = new T[n + 1];
        _qCoeffs[0] = T.One;

        int taylorLen = m + n + 1;
        T[]? rentedTaylor = null;
        Span<T> c = workspace is not null
            ? PrepareTaylor(workspace, taylorLen, taylorCoefficients)
            : (rentedTaylor = new T[taylorLen]);

        if (rentedTaylor is not null)
            taylorCoefficients[..taylorLen].CopyTo(c);

        if (n == 0)
        {
            for (int i = 0; i <= m; i++)
                _pCoeffs[i] = c[i];
            return;
        }

        workspace?.EnsureSolve(n);
        Matrix<T> matrix = workspace is not null ? workspace.Matrix(n) : new Matrix<T>(n, n);
        Span<T> rhs = workspace is not null ? workspace.Rhs(n) : new T[n];
        Span<T> qTail = workspace is not null ? workspace.Solution(n) : new T[n];

        for (int i = 0; i < n; i++)
        {
            rhs[i] = -c[m + 1 + i];
            for (int j = 0; j < n; j++)
            {
                int cIndex = m + i - j;
                matrix[i, j] = cIndex >= 0 ? c[cIndex] : T.Zero;
            }
        }

        if (workspace is not null)
            LinearEquationSolver.GaussianEliminationSolve(matrix, rhs, qTail, workspace.Augmented(n));
        else
            LinearEquationSolver.GaussianEliminationSolve(matrix, rhs, qTail);

        for (int j = 0; j < n; j++)
            _qCoeffs[j + 1] = qTail[j];

        for (int i = 0; i <= m; i++)
        {
            T sum = T.Zero;
            for (int j = 0; j <= i && j <= n; j++)
                sum += _qCoeffs[j] * c[i - j];
            _pCoeffs[i] = sum;
        }
    }

    private static Span<T> PrepareTaylor(PadeWorkspace<T> workspace, int taylorLen, ReadOnlySpan<T> taylorCoefficients)
    {
        workspace.EnsureTaylor(taylorLen);
        Span<T> c = workspace.Taylor(taylorLen);
        taylorCoefficients[..taylorLen].CopyTo(c);
        return c;
    }

    /// <summary>在 x 处求逼近值 P(x)/Q(x)。</summary>
    public T Evaluate(T x) => Horner(_pCoeffs, x) / Horner(_qCoeffs, x);

    private static T Horner(ReadOnlySpan<T> coeffs, T x)
    {
        T sum = coeffs[^1];
        for (int i = coeffs.Length - 2; i >= 0; i--)
            sum = sum * x + coeffs[i];
        return sum;
    }
}
