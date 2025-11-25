using System.Numerics;

namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

/// <summary>
/// 使用牛顿-拉夫逊法求解非线性方程 f(x) = 0 的实例类，支持泛型浮点类型。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public class NewtonRaphson<T> where T : struct, IFloatingPointIeee754<T>
{

    private readonly SingleVariableFunction<T> _func; // 目标函数 f(x)
    private readonly Derivative<T> _derivative;    // 用于计算导数 f'(x)
    private readonly T _defaultH;                  // 导数默认步长

    /// <summary>
    /// 初始化 <see cref="NewtonRaphson{T}"/> 实例。
    /// </summary>
    /// <param name="func">要解的方程 f(x) = 0 中的函数</param>
    /// <param name="defaultH">导数计算的默认步长</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="func"/> 为 null 时抛出</exception>
    public NewtonRaphson(SingleVariableFunction<T> func, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _defaultH = defaultH;
        _derivative = new Derivative<T>(func, defaultH);
    }

    /// <summary>
    /// 使用牛顿-拉夫逊法求解方程 f(x) = 0 的根。
    /// </summary>
    /// <param name="initialGuess">初始猜测值</param>
    /// <param name="maxIterations">最大迭代次数，默认为 100</param>
    /// <param name="tolerance">收敛容差，默认为 1e-10</param>
    /// <returns>方程的近似根</returns>
    /// <exception cref="ArgumentException">当 <paramref name="maxIterations"/> 小于 1 时抛出</exception>
    /// <exception cref="InvalidOperationException">当导数为 0 或迭代未收敛时抛出</exception>
    public T Solve(T initialGuess, int maxIterations = 100, T? tolerance = null)
    {
        if (maxIterations < 1) throw new ArgumentException("迭代次数必须大于等于 1", nameof(maxIterations));
        T tol = tolerance ?? T.CreateChecked(1e-10); // 默认收敛容差

        T x = initialGuess;
        for (int i = 0; i < maxIterations; i++)
        {
            T fx = _func(x);               // f(x)
            T dfx = _derivative.Calculate(x); // f'(x)

            // 检查导数是否为 0
            if (T.Abs(dfx) < T.CreateChecked(1e-15))
                throw new InvalidOperationException("导数接近 0，牛顿法无法继续");

            T dx = fx / dfx; // 牛顿迭代步长
            T xNext = x - dx;

            // 检查收敛
            if (T.Abs(dx) < tol)
                return xNext;

            x = xNext;
        }

        throw new InvalidOperationException("牛顿法未在指定迭代次数内收敛");
    }
}