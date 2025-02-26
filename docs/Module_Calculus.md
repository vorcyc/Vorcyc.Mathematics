当前位置 : [根目录](README.md)/[微积分模块](Module_Calculus.md)

---

## 微积分模块 - Calculus Module

Vorcyc.Mathematics.Calculus 命名空间提供了全面的计算工具，包括极限计算、积分计算、导数计算、泰勒级数展开、傅里叶级数展开、龙格-库塔法求解微分方程和牛顿-拉夫逊法求解非线性方程。

> 本模块所有类型的根命名空间为 ：Vorcyc.Mathematics.Calculus

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.Calculus.Limits 类](#vorcycmathematicscalculuslimits-类)
- :bookmark: [Vorcyc.Mathematics.Calculus.Integration 类](#vorcycmathematicscalculusintegration-类)
- :bookmark: [Vorcyc.Mathematics.Calculus.Derivative 类](#vorcycmathematicscalculusderivative-类)
- :bookmark: [Vorcyc.Mathematics.Calculus.Series.TaylorSeries 类](#vorcycmathematicscalculusseriestaylorseries-类)
- :bookmark: [Vorcyc.Mathematics.Calculus.Series.FourierSeries 类](#vorcycmathematicscalculusseriesfourierseries-类)
- :bookmark: [Vorcyc.Mathematics.Calculus.NumericalMethods.RungeKutta 类](#vorcycmathematicscalculusnumericalmethodsrungekutta-类)
- :bookmark: [Vorcyc.Mathematics.Calculus.NumericalMethods.NewtonRaphson 类](#vorcycmathematicscalculusnumericalmethodsnewtonraphson-类)

---


## Vorcyc.Mathematics.Calculus.Limits 类

`Vorcyc.Mathematics.Calculus.Limits` 是一个提供数值极限计算的类，支持泛型浮点类型。支持从左侧、右侧或双侧趋近目标点计算极限。

### 方法

#### 1. CalculateLimit 方法
- `public T CalculateLimit(T a, Direction direction = Direction.Both, int maxSteps = 100, T? tolerance = null, T? h = null)`
  - 计算函数在指定点的极限。
  - 参数:
    - `a`: 目标点，极限趋近于此点。
    - `direction`: 趋近方向，默认为双侧趋近。
    - `maxSteps`: 最大迭代步数，默认值为 100。
    - `tolerance`: 收敛容差，默认值为 1e-10。
    - `h`: 初始步长，可选，优先级高于默认步长。
  - 返回值: 极限值。
  - 异常:
    - `ArgumentException`: 当参数无效或极限不存在时抛出。

#### 2. ClearCache 方法
- `public void ClearCache()`
  - 清空实例的函数值缓存。

### 代码示例
以下是一个使用 `Limits` 类中 `CalculateLimit` 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Calculus;

public class LimitsExample
{
    public static void Main()
    {
        // 定义函数
        var limits = new Limits<double>(x => x * x, 1e-5);

        // 计算极限
        double resultLeft = limits.CalculateLimit(2.0, Limits<double>.Direction.Left);
        double resultRight = limits.CalculateLimit(2.0, Limits<double>.Direction.Right);
        double resultBoth = limits.CalculateLimit(2.0, Limits<double>.Direction.Both);

        // 输出结果
        Console.WriteLine($"Limits Test: Left = {resultLeft}, Right = {resultRight}, Both = {resultBoth}");
    }
}

```

---

## Vorcyc.Mathematics.Calculus.Integration 类

`Vorcyc.Mathematics.Calculus.Integration` 是一个提供数值积分计算的类，支持泛型浮点类型。

### 方法

#### 1. Integrate 方法
- `public T Integrate(T a, T b, int n, SingleVariableFunction<T> func, T? h = null, Method method = Method.Trapezoidal)`
  - 计算定积分，从 `a` 到 `b`。
  - 参数:
    - `a`: 积分下限。
    - `b`: 积分上限。
    - `n`: 分段数，默认值为 1000。
    - `func`: 被积函数。
    - `h`: 步长，可选，优先级高于默认步长。
    - `method`: 积分方法，默认为梯形法则。
  - 返回值: 定积分值。
  - 异常:
    - `ArgumentNullException`: 当 `func` 为 null 时抛出。
    - `ArgumentException`: 当 `n` 小于 1、`h` 过小或方法不支持时抛出。

#### 2. ClearCache 方法
- `public void ClearCache()`
  - 清空实例的函数值缓存。

### 代码示例
以下是一个使用 `Integration` 类中 `Integrate` 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Calculus;

public class IntegrationExample
{
    public static void Main()
    {
        // 定义被积函数
        var integration = new Integration<double>(1e-5);

        // 计算定积分
        double resultTrapezoidal = integration.Integrate(0.0, 1.0, 1000, x => x * x);
        double resultSimpson = integration.Integrate(0.0, 1.0, 1000, x => x * x, method: Integration<double>.Method.Simpson);

        // 输出结果
        Console.WriteLine($"Integration Test: Trapezoidal = {resultTrapezoidal}, Simpson = {resultSimpson}");
    }
}

```

---

## Vorcyc.Mathematics.Calculus.Derivative 类

`Vorcyc.Mathematics.Calculus.Derivative` 是一个提供导数和偏导数计算的类，支持数值方法和自动微分。

### 方法

#### 1. Calculate 方法
- `public T Calculate(T x, int order = 1, T? h = null, Method method = Method.Central)`
  - 计算单变量函数的导数（数值方法）。
  - 参数:
    - `x`: 计算导数的点。
    - `order`: 导数的阶数，默认为 1。
    - `h`: 步长，默认为 null。
    - `method`: 数值方法，默认为中央差分法。
  - 返回值: 导数值。
  - 异常:
    - `InvalidOperationException`: 当实例不支持数值单变量导数计算时抛出。
    - `ArgumentException`: 当 `order` 小于 1 或步长过小时抛出。

#### 2. CalculateAD 方法
- `public T CalculateAD(T x)`
  - 使用自动微分计算单变量导数。
  - 参数:
    - `x`: 计算导数的点。
  - 返回值: 导数值。
  - 异常:
    - `InvalidOperationException`: 当实例不支持单变量自动微分时抛出。

#### 3. ClearCache 方法
- `public void ClearCache()`
  - 清空实例的缓存。

### 代码示例
以下是一个使用 `Derivative` 类中 `Calculate` 和 `CalculateAD` 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Calculus;

public class DerivativeExample
{
    public static void Main()
    {
        // 定义函数
        var derivative = new Derivative<double>(x => x * x, 1e-5);

        // 计算导数
        double resultNumeric = derivative.Calculate(2.0);
        var derivativeAD = new Derivative<double>(x => new DualNumber<double>(x * x, 2 * x));
        double resultAD = derivativeAD.CalculateAD(2.0);

        // 输出结果
        Console.WriteLine($"Derivative Test: Numeric = {resultNumeric}, AD = {resultAD}");
    }
}

```

---

## Vorcyc.Mathematics.Calculus.Series.TaylorSeries 类

`Vorcyc.Mathematics.Calculus.Series.TaylorSeries` 是一个提供泰勒级数展开计算的类，支持泛型浮点类型。

### 方法

#### 1. Calculate 方法
- `public T Calculate(T x, int order = 5)`
  - 计算泰勒级数在指定点的值，截断到指定阶数。
  - 参数:
    - `x`: 计算点。
    - `order`: 最高阶数，默认为 5。
  - 返回值: 泰勒级数近似值。
  - 异常:
    - `ArgumentException`: 当 `order` 小于 0 时抛出。

#### 2. GetTaylorCoefficient 方法
- `public T GetTaylorCoefficient(int order)`
  - 获取泰勒级数的系数（导数值除以阶乘）。
  - 参数:
    - `order`: 阶数。
  - 返回值: 第 `order` 阶泰勒系数。

#### 3. ClearCache 方法
- `public void ClearCache()`
  - 清空导数缓存。

### 代码示例
以下是一个使用 `TaylorSeries` 类中 `Calculate` 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Calculus.Series;

public class TaylorSeriesExample
{
    public static void Main()
    {
        // 定义函数
        var taylorSeries = new TaylorSeries<double>(x => Math.Exp(x), 0.0, 1e-5);

        // 计算泰勒级数
        double result = taylorSeries.Calculate(1.0, 5);

        // 输出结果
        Console.WriteLine($"Taylor Series Test: Result = {result}");
    }
}

```

---

## Vorcyc.Mathematics.Calculus.Series.FourierSeries 类

`Vorcyc.Mathematics.Calculus.Series.FourierSeries` 是一个提供傅里叶级数展开计算的类，支持泛型浮点类型。

### 方法

#### 1. Calculate 方法
- `public T Calculate(T x, int order = 5, int segments = 1000)`
  - 计算傅里叶级数在指定点的值，截断到指定阶数。
  - 参数:
    - `x`: 计算点。
    - `order`: 最高阶数，默认为 5。
    - `segments`: 积分分段数，默认值为 1000。
  - 返回值: 傅里叶级数近似值。
  - 异常:
    - `ArgumentException`: 当 `order` 小于 0 或 `segments` 小于 1 时抛出。

#### 2. GetFourierCoefficient 方法
- `public T GetFourierCoefficient(bool isCosine, int n, int segments = 1000)`
  - 获取傅里叶级数的系数（aₙ 或 bₙ）。
  - 参数:
    - `isCosine`: true 表示余弦系数 aₙ，false 表示正弦系数 bₙ。
    - `n`: 谐波阶数。
    - `segments`: 积分分段数，默认值为 1000。
  - 返回值: 第 `n` 阶傅里叶系数。
  - 异常:
    - `ArgumentException`: 当 `n` 小于 0 或 `segments` 小于 1 时抛出。
    - `InvalidOperationException`: 当积分结果无效时抛出。

#### 3. ClearCache 方法
- `public void ClearCache()`
  - 清空实例的函数值缓存。

### 代码示例
以下是一个使用 `FourierSeries` 类中 `Calculate` 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Calculus.Series;

public class FourierSeriesExample
{
    public static void Main()
    {
        // 定义函数
        var fourierSeries = new FourierSeries<double>(x => Math.Sin(x), 2 * Math.PI, 1e-5);

        // 计算傅里叶级数
        double result = fourierSeries.Calculate(Math.PI / 2, 5);

        // 输出结果
        Console.WriteLine($"Fourier Series Test: Result = {result}");
    }
}

```

---

## Vorcyc.Mathematics.Calculus.NumericalMethods.RungeKutta 类

`Vorcyc.Mathematics.Calculus.NumericalMethods.RungeKutta` 是一个使用龙格-库塔法（RK4）求解常微分方程的类，支持泛型浮点类型。

### 方法

#### 1. Solve 方法
- `public T Solve(T x0, T y0, T xEnd, int steps = 100, T? h = null)`
  - 使用四阶龙格-库塔法（RK4）求解微分方程。
  - 参数:
    - `x0`: 初始 x 值。
    - `y0`: 初始 y 值。
    - `xEnd`: 目标 x 值。
    - `steps`: 步数，决定步长 h = (xEnd - x0) / steps。
    - `h`: 步长，可选，优先级高于默认步长。
  - 返回值: 在 `xEnd` 处的 y 值。
  - 异常:
    - `ArgumentException`: 当 `steps` 小于 1 时抛出。

#### 2. ClearCache 方法
- `public void ClearCache()`
  - 清空函数值缓存。

### 代码示例
以下是一个使用 `RungeKutta` 类中 `Solve` 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Calculus.NumericalMethods;

public class RungeKuttaExample
{
    public static void Main()
    {
        // 定义微分方程
        var rungeKutta = new RungeKutta<double>((x, y) => x + y, 1e-5);

        // 求解微分方程
        double result = rungeKutta.Solve(0.0, 1.0, 1.0, 100);

        // 输出结果
        Console.WriteLine($"Runge-Kutta Test: Result = {result}");
    }
}

```

---

## Vorcyc.Mathematics.Calculus.NumericalMethods.NewtonRaphson 类

`Vorcyc.Mathematics.Calculus.NumericalMethods.NewtonRaphson` 是一个使用牛顿-拉夫逊法求解非线性方程 f(x) = 0 的类，支持泛型浮点类型。

### 方法

#### 1. Solve 方法
- `public T Solve(T initialGuess, int maxIterations = 100, T? tolerance = null)`
  - 使用牛顿-拉夫逊法求解方程 f(x) = 0 的根。
  - 参数:
    - `initialGuess`: 初始猜测值。
    - `maxIterations`: 最大迭代次数，默认为 100。
    - `tolerance`: 收敛容差，默认为 1e-10。
  - 返回值: 方程的近似根。
  - 异常:
    - `ArgumentException`: 当 `maxIterations` 小于 1 时抛出。
    - `InvalidOperationException`: 当导数为 0 或迭代未收敛时抛出。

### 代码示例
以下是一个使用 `NewtonRaphson` 类中 `Solve` 方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Calculus.NumericalMethods;

public class NewtonRaphsonExample
{
    public static void Main()
    {
        // 定义函数
        var newtonRaphson = new NewtonRaphson<double>(x => x * x - 2.0, 1e-5);

        // 求解方程
        double result = newtonRaphson.Solve(1.0);

        // 输出结果
        Console.WriteLine($"Newton-Raphson Test: Result = {result}");
    }
}

```
