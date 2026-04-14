using System.Numerics;

using Vorcyc.Mathematics.LinearAlgebra;



namespace Vorcyc.Mathematics.Calculus;



/// <summary>

/// 计算标量多元函数 f: Rⁿ→R 的 Hessian 矩阵（二阶偏导数）。

/// </summary>

/// <typeparam name="T">浮点类型</typeparam>

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

    /// 初始化 <see cref="Hessian{T}"/> 实例。

    /// </summary>

    public Hessian(MultiVariableFunction<T> func, T defaultH)

    {

        _func = func ?? throw new ArgumentNullException(nameof(func));

        _defaultH = defaultH;

        _minH = T.CreateChecked(1e-15);

    }



    /// <summary>

    /// 在指定点计算 n×n Hessian 矩阵，H[i,j] = ∂²f/∂xᵢ∂xⱼ。

    /// </summary>

    public Matrix<T> Calculate(Span<T> point, T? h = null) => Calculate(point, h, null);



    /// <summary>

    /// 在指定点计算 Hessian；若提供 <paramref name="matrix"/> 则复用存储（维数须匹配）。

    /// </summary>

    public Matrix<T> Calculate(Span<T> point, T? h, Matrix<T>? matrix)

    {

        if (point.IsEmpty) throw new ArgumentException("输入点不能为空", nameof(point));



        T step = h ?? _defaultH;

        if (step <= _minH) throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));



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

    /// 使用超对偶数自动微分计算 Hessian（需显式 HyperDual 多元函数）。

    /// </summary>

    public Matrix<T> CalculateAD(ReadOnlySpan<T> point, Func<ReadOnlySpan<HyperDualNumber<T>>, HyperDualNumber<T>> func) =>

        CalculateAD(point, func, null);



    /// <summary>

    /// 超对偶 Hessian；若提供 <paramref name="matrix"/> 则复用存储。

    /// </summary>

    public Matrix<T> CalculateAD(

        ReadOnlySpan<T> point,

        Func<ReadOnlySpan<HyperDualNumber<T>>, HyperDualNumber<T>> func,

        Matrix<T>? matrix)

    {

        if (point.IsEmpty) throw new ArgumentException("输入点不能为空", nameof(point));

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


