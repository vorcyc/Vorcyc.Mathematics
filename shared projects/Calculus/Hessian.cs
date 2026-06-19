using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.Calculus;

/// <summary>
/// Computes the Hessian matrix (second-order partial derivatives) of a scalar multivariate function f: R^n -> R.
/// </summary>
/// <typeparam name="T">The floating-point numeric type.</typeparam>
public sealed class Hessian<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly MultiVariableFunction<T> _func;
    private readonly T _defaultH;
    private readonly T _minH;

    private T[]? _basePoint;
    private T[]? _work;
    private HyperDualNumber<T>[]? _hdInputs;
    private Matrix<T>? _matrix;
    private int _dim;

    /// <summary>
    /// Initializes a new instance of the <see cref="Hessian{T}"/> class.
    /// </summary>
    public Hessian(MultiVariableFunction<T> func, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _defaultH = defaultH;
        _minH = T.CreateChecked(1e-15);
    }

    /// <summary>
    /// Computes the n x n Hessian matrix at the specified point, where H[i,j] = d^2 f / (d x_i d x_j).
    /// </summary>
    public Matrix<T> Calculate(Span<T> point, T? h = null) => Calculate(point, h, null);

    /// <summary>
    /// Computes the Hessian at the specified point; reuses the provided <paramref name="matrix"/> as storage when supplied (the dimensions must match).
    /// </summary>
    public Matrix<T> Calculate(Span<T> point, T? h, Matrix<T>? matrix)
    {
        if (point.IsEmpty) throw new ArgumentException("The point must not be empty.", nameof(point));

        T step = h ?? _defaultH;
        if (step <= _minH) throw new ArgumentException($"The step size must be greater than {_minH}.", nameof(h));

        int n = point.Length;
        EnsureBuffers(n);
        point.CopyTo(_basePoint!);

        Matrix<T> hessian = matrix is not null && matrix.Rows == n && matrix.Columns == n
            ? matrix
            : (_matrix = new Matrix<T>(n, n));

        T two = T.CreateChecked(2);
        T four = T.CreateChecked(4);
        T invH2 = T.One / (step * step);
        T invFourH2 = invH2 / four;

        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                T value = i == j
                    ? DiagonalEntry(_basePoint!, _work!, i, step, invH2, two)
                    : OffDiagonalEntry(_basePoint!, _work!, i, j, step, invFourH2);

                hessian[i, j] = value;
                if (i != j)
                    hessian[j, i] = value;
            }
        }

        return hessian;
    }

    /// <summary>
    /// Computes the Hessian using hyper-dual automatic differentiation (requires an explicit HyperDual multivariate function).
    /// </summary>
    public Matrix<T> CalculateAD(ReadOnlySpan<T> point, Func<ReadOnlySpan<HyperDualNumber<T>>, HyperDualNumber<T>> func) =>
        CalculateAD(point, func, null);

    /// <summary>
    /// Computes the hyper-dual Hessian; reuses the provided <paramref name="matrix"/> as storage when supplied.
    /// </summary>
    public Matrix<T> CalculateAD(
        ReadOnlySpan<T> point,
        Func<ReadOnlySpan<HyperDualNumber<T>>, HyperDualNumber<T>> func,
        Matrix<T>? matrix)
    {
        if (point.IsEmpty) throw new ArgumentException("The point must not be empty.", nameof(point));
        int n = point.Length;
        EnsureBuffers(n);

        Matrix<T> hessian = matrix is not null && matrix.Rows == n && matrix.Columns == n
            ? matrix
            : (_matrix = new Matrix<T>(n, n));

        var inputs = _hdInputs!;

        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    inputs[k] = k == i && i == j
                        ? new HyperDualNumber<T>(point[k], T.One, T.One, T.Zero)
                        : k == i
                            ? new HyperDualNumber<T>(point[k], T.One, T.Zero, T.Zero)
                            : k == j
                                ? new HyperDualNumber<T>(point[k], T.Zero, T.One, T.Zero)
                                : new HyperDualNumber<T>(point[k]);
                }

                T value = func(inputs).E12;
                hessian[i, j] = value;
                if (i != j)
                    hessian[j, i] = value;
            }
        }

        return hessian;
    }

    private T DiagonalEntry(T[] basePoint, T[] work, int i, T h, T invH2, T two)
    {
        basePoint.CopyTo(work, 0);
        T f0 = _func(work);

        work[i] = basePoint[i] + h;
        T fPlus = _func(work);

        work[i] = basePoint[i] - h;
        T fMinus = _func(work);

        return (fPlus - two * f0 + fMinus) * invH2;
    }

    private T OffDiagonalEntry(T[] basePoint, T[] work, int i, int j, T h, T invFourH2)
    {
        basePoint.CopyTo(work, 0);
        work[i] = basePoint[i] + h;
        work[j] = basePoint[j] + h;
        T fPlusPlus = _func(work);

        work[j] = basePoint[j] - h;
        T fPlusMinus = _func(work);

        work[i] = basePoint[i] - h;
        work[j] = basePoint[j] + h;
        T fMinusPlus = _func(work);

        work[j] = basePoint[j] - h;
        T fMinusMinus = _func(work);

        return (fPlusPlus - fPlusMinus - fMinusPlus + fMinusMinus) * invFourH2;
    }

    private void EnsureBuffers(int n)
    {
        if (n <= _dim && _basePoint is not null)
            return;

        _dim = n;
        _basePoint = new T[n];
        _work = new T[n];
        _hdInputs = new HyperDualNumber<T>[n];
    }
}
