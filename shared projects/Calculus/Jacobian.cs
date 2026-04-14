using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace Vorcyc.Mathematics.Calculus;

/// <summary>
/// 计算向量值函数的雅可比矩阵。
/// </summary>
/// <typeparam name="T">浮点类型</typeparam>
public sealed class Jacobian<T> where T : struct, IFloatingPointIeee754<T>
{
    private readonly VectorFieldFunction<T> _func;
    private readonly int _outputDim;
    private readonly T _defaultH;
    private readonly T _minH;

    private T[]? _values;
    private T[]? _valuesPlus;
    private T[]? _valuesMinus;
    private T[]? _pointPlus;
    private T[]? _pointMinus;
    private Matrix<T>? _matrix;
    private int _inputDim;

    /// <summary>
    /// 初始化 <see cref="Jacobian{T}"/> 实例。
    /// </summary>
    /// <param name="func">向量场函数，将结果写入 output</param>
    /// <param name="outputDim">输出维度 m</param>
    /// <param name="defaultH">默认差分步长</param>
    public Jacobian(VectorFieldFunction<T> func, int outputDim, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        if (outputDim < 1) throw new ArgumentException("输出维度必须大于等于 1", nameof(outputDim));
        _outputDim = outputDim;
        _defaultH = defaultH;
        _minH = T.CreateChecked(1e-15);
    }

    /// <summary>
    /// 在指定点计算 m×n 雅可比矩阵，J[i,j] = ∂f_i/∂x_j。
    /// </summary>
    public Matrix<T> Calculate(Span<T> point, T? h = null, Derivative<T>.Method method = Derivative<T>.Method.Central) =>
        Calculate(point, h, method, null);

    /// <summary>
    /// 在指定点计算雅可比；若提供 <paramref name="matrix"/> 则复用存储（维数须匹配）。
    /// </summary>
    public Matrix<T> Calculate(Span<T> point, T? h, Derivative<T>.Method method, Matrix<T>? matrix)
    {
        if (point.IsEmpty) throw new ArgumentException("输入点不能为空", nameof(point));

        T step = ResolveStep(point, h);
        int n = point.Length;
        EnsureBuffers(n);
        Matrix<T> jacobian = matrix is not null && matrix.Rows == _outputDim && matrix.Columns == n
            ? matrix
            : (_matrix = new Matrix<T>(_outputDim, n));

        return method switch
        {
            Derivative<T>.Method.Forward => CalculateForward(point, step, jacobian),
            Derivative<T>.Method.Backward => CalculateBackward(point, step, jacobian),
            _ => CalculateCentral(point, step, jacobian)
        };
    }

    private T ResolveStep(ReadOnlySpan<T> point, T? h)
    {
        if (h is not null)
        {
            T step = h.GetValueOrDefault();
            if (step <= _minH)
                throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));
            return step;
        }

        T scale = T.One;
        for (int i = 0; i < point.Length; i++)
        {
            T ax = T.Abs(point[i]);
            if (ax > scale) scale = ax;
        }

        T floor = NumericalStep.OptimalMagnitude(scale, 1);
        if (floor <= _minH)
            floor = T.Max(_defaultH, floor);
        if (floor <= _minH)
            throw new ArgumentException($"步长必须大于 {_minH}", nameof(h));
        return floor;
    }

    private Matrix<T> CalculateForward(Span<T> point, T step, Matrix<T> jacobian)
    {
        T invStep = T.One / step;
        _func(point, _values!);
        point.CopyTo(_pointPlus!);
        T[] ptPlus = _pointPlus!;
        T[] vals = _values!;
        T[] valsPlus = _valuesPlus!;

        for (int j = 0; j < point.Length; j++)
        {
            ptPlus[j] += step;
            _func(ptPlus, valsPlus);
            for (int i = 0; i < _outputDim; i++)
                jacobian[i, j] = (valsPlus[i] - vals[i]) * invStep;
            ptPlus[j] -= step;
        }

        return jacobian;
    }

    private Matrix<T> CalculateBackward(Span<T> point, T step, Matrix<T> jacobian)
    {
        T invStep = T.One / step;
        _func(point, _values!);
        point.CopyTo(_pointMinus!);
        T[] ptMinus = _pointMinus!;
        T[] vals = _values!;
        T[] valsMinus = _valuesMinus!;

        for (int j = 0; j < point.Length; j++)
        {
            ptMinus[j] -= step;
            _func(ptMinus, valsMinus);
            for (int i = 0; i < _outputDim; i++)
                jacobian[i, j] = (vals[i] - valsMinus[i]) * invStep;
            ptMinus[j] += step;
        }

        return jacobian;
    }

    private Matrix<T> CalculateCentral(Span<T> point, T step, Matrix<T> jacobian)
    {
        T invTwoStep = T.One / (T.CreateChecked(2) * step);
        T twoStep = step + step;
        point.CopyTo(_pointPlus!);
        T[] ptPlus = _pointPlus!;
        T[] valsPlus = _valuesPlus!;
        T[] valsMinus = _valuesMinus!;

        for (int j = 0; j < point.Length; j++)
        {
            ptPlus[j] += step;
            _func(ptPlus, valsPlus);

            ptPlus[j] -= twoStep;
            _func(ptPlus, valsMinus);

            for (int i = 0; i < _outputDim; i++)
                jacobian[i, j] = (valsPlus[i] - valsMinus[i]) * invTwoStep;

            ptPlus[j] += step;
        }

        return jacobian;
    }

    private void EnsureBuffers(int inputDim)
    {
        if (inputDim <= _inputDim && _values is not null)
            return;

        _inputDim = inputDim;
        _values = new T[_outputDim];
        _valuesPlus = new T[_outputDim];
        _valuesMinus = new T[_outputDim];
        _pointPlus = new T[inputDim];
        _pointMinus = new T[inputDim];
    }
}
