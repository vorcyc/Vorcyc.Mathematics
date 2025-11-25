namespace Vorcyc.Mathematics.Calculus.NumericalMethods;

using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// 使用龙格-库塔法（RK4）求解常微分方程 dy/dx = f(x,y) 的实例类，支持泛型浮点类型。
/// </summary>
/// <typeparam name="T">浮点类型，必须实现 <see cref="IFloatingPointIeee754{T}"/></typeparam>
public class RungeKutta<T> where T : struct, IFloatingPointIeee754<T>
{


    private readonly DifferentialFunction<T> _func; // 微分方程 f(x,y)
    private readonly T _defaultH;                // 默认步长

    // 缓存函数值，避免重复计算
    private readonly Dictionary<(T x, T y), T> _cache;

    /// <summary>
    /// 初始化 <see cref="RungeKutta{T}"/> 实例。
    /// </summary>
    /// <param name="func">微分方程 dy/dx = f(x,y)</param>
    /// <param name="defaultH">默认步长</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="func"/> 为 null 时抛出</exception>
    public RungeKutta(DifferentialFunction<T> func, T defaultH)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _defaultH = defaultH;
        _cache = new Dictionary<(T, T), T>();
    }

    /// <summary>
    /// 使用四阶龙格-库塔法（RK4）求解微分方程。
    /// </summary>
    /// <param name="x0">初始 x 值</param>
    /// <param name="y0">初始 y 值</param>
    /// <param name="xEnd">目标 x 值</param>
    /// <param name="steps">步数，决定步长 h = (xEnd - x0) / steps</param>
    /// <param name="h">步长，可选，优先级高于默认步长</param>
    /// <returns>在 <paramref name="xEnd"/> 处的 y 值</returns>
    /// <exception cref="ArgumentException">当 <paramref name="steps"/> 小于 1 时抛出</exception>
    public T Solve(T x0, T y0, T xEnd, int steps = 100, T? h = null)
    {
        if (steps < 1) throw new ArgumentException("步数必须大于等于 1", nameof(steps));

        T step = h ?? (xEnd - x0) / T.CreateChecked(steps); // 使用指定步长或计算步长
        T x = x0;
        T y = y0;

        // 迭代直到 x 达到 xEnd
        while (x < xEnd)
        {
            T k1 = GetValue(x, y);
            T k2 = GetValue(x + step / T.CreateChecked(2), y + k1 * step / T.CreateChecked(2));
            T k3 = GetValue(x + step / T.CreateChecked(2), y + k2 * step / T.CreateChecked(2));
            T k4 = GetValue(x + step, y + k3 * step);

            // RK4 更新公式：y_{n+1} = y_n + (h/6)(k1 + 2k2 + 2k3 + k4)
            T dy = step * (k1 + T.CreateChecked(2) * k2 + T.CreateChecked(2) * k3 + k4) / T.CreateChecked(6);
            y += dy;
            x += step;

            // 如果超过 xEnd，调整最后一步
            if (x > xEnd)
            {
                step = xEnd - (x - step);
                x = xEnd;
                y -= dy; // 回退一步，重新计算
                k1 = GetValue(x - step, y);
                k2 = GetValue(x - step / T.CreateChecked(2), y + k1 * step / T.CreateChecked(2));
                k3 = GetValue(x - step / T.CreateChecked(2), y + k2 * step / T.CreateChecked(2));
                k4 = GetValue(x, y + k3 * step);
                y += step * (k1 + T.CreateChecked(2) * k2 + T.CreateChecked(2) * k3 + k4) / T.CreateChecked(6);
            }
        }
        return y;
    }

    // 获取缓存中的函数值
    private T GetValue(T x, T y)
    {
        var key = (x, y);
        if (!_cache.TryGetValue(key, out T value))
        {
            value = _func(x, y);
            _cache[key] = value;
        }
        return value;
    }

    /// <summary>
    /// 清空函数值缓存。
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }
}