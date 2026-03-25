当前位置 : [根目录](README.md)/[实验性模块](Module_Experimental.md)

# 实验性模块 - Experimental Module

`Vorcyc.Mathematics.Experimental.KalmanFilters` 命名空间包含多种卡尔曼滤波器类，包括 `ExtendedKalmanFilter1D<T>`、`ExtendedKalmanFilter2D<T>`、`InformationFilter1D<T>`、`ParticleFilter1D<T>`、`SquareRootKalmanFilter1D<T>`、`StandardKalmanFilter1D<T>`、`StandardKalmanFilter2D<T>` 和 `UnscentedKalmanFilter1D<T>`。这些类提供了丰富的滤波功能，适用于一维和二维的扩展卡尔曼滤波、信息滤波、粒子滤波、平方根卡尔曼滤波、标准卡尔曼滤波和无迹卡尔曼滤波。




> 以下类型都位于 Vorcyc.Mathematics.Experimental.KalmanFilters 命名空间

:ledger:目录  
- :bookmark: [ExtendedKalmanFilter1D&lt;T> 类](#extendedkalmanfilter1dt-类)
- :bookmark: [ExtendedKalmanFilter2D&lt;T> 类](#extendedkalmanfilter2dt-类)
- :bookmark: [InformationFilter1D&lt;T> 类](#informationfilter1dt-类)
- :bookmark: [ParticleFilter1D&lt;T> 类](#particlefilter1dt-类)
- :bookmark: [SquareRootKalmanFilter1D&lt;T> 类](#squarerootkalmanfilter1dt-类)
- :bookmark: [StandardKalmanFilter1D&lt;T> 类](#standardkalmanfilter1dt-类)
- :bookmark: [StandardKalmanFilter2D&lt;T> 类](#standardkalmanfilter2dt-类)
- :bookmark: [UnscentedKalmanFilter1D&lt;T> 类](#unscentedkalmanfilter1dt-类)

---

`Vorcyc.Mathematics.Experimental.Signals` 命名空间包含多种信号处理接口、类和结构体。`ITimeDomainCharacteristics` 接口定义了时域特征属性（周期、幅度、总功率、平均功率、总能量、平均能量、RMS、过零率、熵）。`ITimeDomainSignal` 接口继承自 `ITimeDomainCharacteristics`，定义时域信号的基础契约。`ISingleThreadTimeDomainSignal` 接口提供单线程场景下的采样数据访问和重采样。`IModifiableTimeDomainSignal` 接口扩展了信号修改能力（追加、插入、删除、重采样），支持异步追加。`Signal` 类实现 `ISingleThreadTimeDomainSignal`，用于表示单线程时域信号，包含信号样本和采样率，并提供频域变换、重采样等方法。`ModifiableTimeDomainSignal` 类实现 `IModifiableTimeDomainSignal`，支持运行时安全的采样修改，提供锁保护的读写视图和异步批量追加。`FrequencyDomain` 只读结构体用于表示频域信号，提供频率转换、逆变换等方法。`SignalSegment` 只读结构体用于表示信号段，提供延迟计算的时域特征和频域转换。`SignalGeneratingExtension` 类提供了生成各种波形的方法，如正弦波、余弦波、方波、锯齿波、三角波、白噪声和粉红噪声。`IFrequencyDomain` 和 `IFrequencyDomainCharacteristics` 接口定义了频域信号的属性和频谱特征。


> 以下类型都位于 Vorcyc.Mathematics.Experimental.Signals 命名空间

:ledger:目录  
- :bookmark: [ITimeDomainCharacteristics 接口](#itimedomaincharacteristics-接口)
- :bookmark: [ITimeDomainSignal 接口](#itimedomainsignal-接口)
- :bookmark: [ISingleThreadTimeDomainSignal 接口](#isinglethreadtimedomainsignal-接口)
- :bookmark: [IModifiableTimeDomainSignal 接口](#imodifiabletimedomainsignal-接口)
- :bookmark: [IFrequencyDomainCharacteristics 接口](#ifrequencydomaincharacteristics-接口)
- :bookmark: [IFrequencyDomain 接口](#ifrequencydomain-接口)
- :bookmark: [FrequencyDomain 结构体](#frequencydomain-结构体)
- :bookmark: [Signal 类](#signal-类)
- :bookmark: [ModifiableTimeDomainSignal 类](#modifiabletimedomainsignal-类)
- :bookmark: [SignalSegment 结构体](#signalsegment-结构体)
- :bookmark: [SignalGeneratingExtension 类](#signalgeneratingextension-类)

---

`Vorcyc.Mathematics.Experimental.CurveFitting` 命名空间提供了多种曲线拟合方法，通过 `CurveFitter<T>` 静态类统一访问。支持线性回归、多项式回归、指数回归、对数回归、幂回归、正弦回归、三次样条插值、局部加权回归 (LOWESS)、移动平均、非线性回归（Levenberg-Marquardt）、高斯过程回归 (GPR)、神经网络回归和贝叶斯线性回归等方法。所有拟合方法返回 `FitResult<T>` 或 `MultiColumnFitResult<T>`，包含预测函数、拟合参数和均方误差。支持 SIMD 优化（`float` 和 `double`）以及标准托管代码两种模式。

> 以下类型都位于 Vorcyc.Mathematics.Experimental.CurveFitting 命名空间

:ledger:目录  
- :bookmark: [CurveFitter&lt;T> 类](#curvefittert-类)
- :bookmark: [CurveFittingMethod 枚举](#curvefittingmethod-枚举)
- :bookmark: [FitResult&lt;T> 类](#fitresultt-类)
- :bookmark: [DataRow&lt;T> 结构体](#datarowt-结构体)
- :bookmark: [OptimizationMode 枚举](#optimizationmode-枚举)

---

> 以下类型都位于 Vorcyc.Mathematics.Experimental.KalmanFilters 命名空间



## ExtendedKalmanFilter1D&lt;T> 类

Vorcyc.Mathematics.Experimental.KalmanFilters.ExtendedKalmanFilter1D&lt;T> 是一个用于一维扩展卡尔曼滤波器的类。

### 属性

无公开属性。

### 构造器

#### 1. ExtendedKalmanFilter1D
- `public ExtendedKalmanFilter1D(T A, T B, T H, T Q, T R, T initialState, T initialP)`
  - 初始化扩展卡尔曼滤波器的实例。
  - 参数:
    - `A`: 状态转移系数。
    - `B`: 控制输入系数。
    - `H`: 观测系数。
    - `Q`: 过程噪声协方差。
    - `R`: 测量噪声协方差。
    - `initialState`: 初始状态估计。
    - `initialP`: 初始估计误差协方差。

### 方法

#### 1. Predict
- `public T Predict(T u, Func<T, T, T> stateTransitionFunc)`
  - 执行预测步骤。
  - 参数:
    - `u`: 控制输入。
    - `stateTransitionFunc`: 状态转移函数。
  - 返回值: 预测的状态估计。

#### 2. Update
- `public T Update(T z, Func<T, T> measurementFunc)`
  - 执行更新步骤。
  - 参数:
    - `z`: 测量值。
    - `measurementFunc`: 观测函数。
  - 返回值: 更新后的状态估计。

### 代码示例
以下是一个使用 ExtendedKalmanFilter1D&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Experimental.KalmanFilters;

public class ExtendedKalmanFilter1DExample
{
    public static void Main()
    {
        // 定义滤波器参数
        var A = 1.0f; var B = 0.5f; var H = 1.0f; var Q = 0.1f; var R = 0.1f; var initialState = 0.0f; var initialP = 1.0f;
        // 创建扩展卡尔曼滤波器实例
        var ekf = new ExtendedKalmanFilter1D<float>(A, B, H, Q, R, initialState, initialP);

        // 定义非线性状态转移函数
        float NonlinearStateTransitionFunction(float x, float u)
        {
            return x + u;
        }

        // 定义非线性观测函数
        float NonlinearMeasurementFunction(float x)
        {
            return x;
        }

        // 定义测量数据
        float[] audioSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        float[] filteredSamples = new float[audioSamples.Length];

        // 执行滤波
        for (int i = 0; i < audioSamples.Length; i++)
        {
            var u = 0.0f; // 控制输入为0
            var z = audioSamples[i]; // 当前测量值

            var predictedState = ekf.Predict(u, NonlinearStateTransitionFunction);
            var updatedState = ekf.Update(z, NonlinearMeasurementFunction);

            filteredSamples[i] = updatedState;
        }

        // 输出滤波结果
        Console.WriteLine("Filtered Samples:");
        foreach (var sample in filteredSamples)
        {
            Console.WriteLine(sample);
        }
    }
}
```



## ExtendedKalmanFilter2D&lt;T> 类

Vorcyc.Mathematics.Experimental.KalmanFilters.ExtendedKalmanFilter2D&lt;T> 是一个用于二维扩展卡尔曼滤波器的类。

### 属性

无公开属性。

### 构造器

#### 1. ExtendedKalmanFilter2D
- `public ExtendedKalmanFilter2D(Matrix<T> A, Matrix<T> B, Matrix<T> H, Matrix<T> Q, Matrix<T> R, Matrix<T> initialState, Matrix<T> initialP)`
  - 初始化扩展卡尔曼滤波器的实例。
  - 参数:
    - `A`: 状态转移矩阵。
    - `B`: 控制输入矩阵。
    - `H`: 观测矩阵。
    - `Q`: 过程噪声协方差。
    - `R`: 测量噪声协方差。
    - `initialState`: 初始状态估计。
    - `initialP`: 初始估计误差协方差。

### 方法

#### 1. Predict
- `public Matrix<T> Predict(Matrix<T> u, Func<Matrix<T>, Matrix<T>, Matrix<T>> stateTransitionFunc)`
  - 执行预测步骤。
  - 参数:
    - `u`: 控制输入。
    - `stateTransitionFunc`: 状态转移函数。
  - 返回值: 预测的状态估计。

#### 2. Update
- `public Matrix<T> Update(Matrix<T> z, Func<Matrix<T>, Matrix<T>> measurementFunc)`
  - 执行更新步骤。
  - 参数:
    - `z`: 测量值。
    - `measurementFunc`: 观测函数。
  - 返回值: 更新后的状态估计。

### 代码示例
以下是一个使用 ExtendedKalmanFilter2D&lt;T> 类中多个方法的示例，并在示例中加入了注释：


```csharp
using System;
using Vorcyc.Mathematics.Experimental.KalmanFilters;
using Vorcyc.Mathematics.LinearAlgebra;

public class ExtendedKalmanFilter2DExample
{
    public static void Main()
    {
        // 定义滤波器参数
        var A = new Matrix<float>(new float[,] { { 1, 0 }, { 0, 1 } }); 
        var B = new Matrix<float>(new float[,] { { 0.5f }, { 1 } }); 
        var H = new Matrix<float>(new float[,] { { 1, 0 }, { 0, 1 } }); 
        var Q = new Matrix<float>(new float[,] { { 0.1f, 0 }, { 0, 0.1f } });
        var R = new Matrix<float>(new float[,] { { 0.1f, 0 }, { 0, 0.1f } }); 
        var initialState = new Matrix<float>(new float[,] { { 0 }, { 0 } }); 
        var initialP = new Matrix<float>(new float[,] { { 1, 0 }, { 0, 1 } });
        // 创建扩展卡尔曼滤波器实例
        var ekf = new ExtendedKalmanFilter2D<float>(A, B, H, Q, R, initialState, initialP);

        // 定义非线性状态转移函数
        Matrix<float> NonlinearStateTransitionFunction(Matrix<float> x, Matrix<float> u)
        {
            return x + u;
        }

        // 定义非线性观测函数
        Matrix<float> NonlinearMeasurementFunction(Matrix<float> x)
        {
            return x;
        }

        // 定义测量数据
        var u = new Matrix<float>(new float[,] { { 1 } });
        var z = new Matrix<float>(new float[,] { { 1 }, { 1 } });

        // 执行预测步骤
        var predictedState = ekf.Predict(u, NonlinearStateTransitionFunction);

        // 执行更新步骤
        var updatedState = ekf.Update(z, NonlinearMeasurementFunction);

        // 输出滤波结果
        Console.WriteLine("Predicted State:");
        PrintMatrix(predictedState);

        Console.WriteLine("Updated State:");
        PrintMatrix(updatedState);
    }

    private static void PrintMatrix(Matrix<float> matrix)
    {
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                Console.Write($"{matrix[i, j]} ");
            }
            Console.WriteLine();
        }
    }
}
```


## InformationFilter1D&lt;T> 类

Vorcyc.Mathematics.Experimental.KalmanFilters.InformationFilter1D&lt;T> 是一个用于一维信息滤波器的类。

### 属性

无公开属性。

### 构造器

#### 1. InformationFilter1D
- `public InformationFilter1D(T A, T B, T H, T Q, T R, T initialState, T initialInformationMatrix)`
  - 初始化信息滤波器的实例。
  - 参数:
    - `A`: 状态转移系数。
    - `B`: 控制输入系数。
    - `H`: 观测系数。
    - `Q`: 过程噪声协方差。
    - `R`: 测量噪声协方差。
    - `initialState`: 初始状态估计。
    - `initialInformationMatrix`: 初始信息矩阵。

### 方法

#### 1. Predict
- `public T Predict(T u)`
  - 执行预测步骤。
  - 参数:
    - `u`: 控制输入。
  - 返回值: 预测的状态估计。

#### 2. Update
- `public T Update(T z)`
  - 执行更新步骤。
  - 参数:
    - `z`: 测量值。
  - 返回值: 更新后的状态估计。

### 代码示例
以下是一个使用 InformationFilter1D&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Experimental.KalmanFilters;

public class InformationFilter1DExample
{
    public static void Main()
    {
        // 定义滤波器参数
        var A = 1.0f;
        var B = 0.5f;
        var H = 1.0f;
        var Q = 0.1f;
        var R = 0.1f;
        var initialState = 0.0f;
        var initialInformationMatrix = 1.0f;
        // 创建信息滤波器实例
        var infoFilter = new InformationFilter1D<float>(A, B, H, Q, R, initialState, initialInformationMatrix);

        // 定义测量数据
        float[] audioSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        float[] filteredSamples = new float[audioSamples.Length];

        // 执行滤波
        for (int i = 0; i < audioSamples.Length; i++)
        {
            var u = 0.0f; // 控制输入为0
            var z = audioSamples[i]; // 当前测量值

            var predictedState = infoFilter.Predict(u);
            var updatedState = infoFilter.Update(z);

            filteredSamples[i] = updatedState;
        }

        // 输出滤波结果
        Console.WriteLine("Filtered Samples:");
        foreach (var sample in filteredSamples)
        {
            Console.WriteLine(sample);
        }
    }
}
```


## ParticleFilter1D&lt;T> 类

Vorcyc.Mathematics.Experimental.KalmanFilters.ParticleFilter1D&lt;T> 是一个用于一维粒子滤波器的类。

### 属性

无公开属性。

### 构造器

#### 1. ParticleFilter1D
- `public ParticleFilter1D(int numParticles, T initialState, T initialStateStdDev, T processNoiseStdDev, T measurementNoiseStdDev)`
  - 初始化粒子滤波器的实例。
  - 参数:
    - `numParticles`: 粒子数量。
    - `initialState`: 初始状态估计。
    - `initialStateStdDev`: 初始状态标准差。
    - `processNoiseStdDev`: 过程噪声标准差。
    - `measurementNoiseStdDev`: 测量噪声标准差。

### 方法

#### 1. Predict
- `public void Predict(T u)`
  - 执行预测步骤。
  - 参数:
    - `u`: 控制输入。

#### 2. Update
- `public void Update(T z)`
  - 执行更新步骤。
  - 参数:
    - `z`: 测量值。

#### 3. Estimate
- `public T Estimate()`
  - 估计当前状态。
  - 返回值: 当前状态的估计值。

### 代码示例
以下是一个使用 ParticleFilter1D&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Experimental.KalmanFilters;
public class ParticleFilter1DExample
{
    public static void Main()
    { // 定义滤波器参数
        var numParticles = 1000;
        var initialState = 0.0f;
        var initialStateStdDev = 1.0f;
        var processNoiseStdDev = 0.1f;
        var measurementNoiseStdDev = 0.1f;
        // 创建粒子滤波器实例
        var pf = new ParticleFilter1D<float>(numParticles, initialState, initialStateStdDev, processNoiseStdDev, measurementNoiseStdDev);

        // 定义测量数据
        float[] audioSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        float[] filteredSamples = new float[audioSamples.Length];

        // 执行滤波
        for (int i = 0; i < audioSamples.Length; i++)
        {
            var u = 0.0f; // 控制输入为0
            var z = audioSamples[i]; // 当前测量值

            pf.Predict(u);
            pf.Update(z);

            filteredSamples[i] = pf.Estimate();
        }

        // 输出滤波结果
        Console.WriteLine("Filtered Samples:");
        foreach (var sample in filteredSamples)
        {
            Console.WriteLine(sample);
        }
    }
}
```
## SquareRootKalmanFilter1D&lt;T> 类

Vorcyc.Mathematics.Experimental.KalmanFilters.SquareRootKalmanFilter1D&lt;T> 是一个用于一维平方根卡尔曼滤波器的类。

### 属性

无公开属性。

### 构造器

#### 1. SquareRootKalmanFilter1D
- `public SquareRootKalmanFilter1D(T A, T B, T H, T Q, T R, T initialState, T initialS)`
  - 初始化平方根卡尔曼滤波器的实例。
  - 参数:
    - `A`: 状态转移系数。
    - `B`: 控制输入系数。
    - `H`: 观测系数。
    - `Q`: 过程噪声协方差。
    - `R`: 测量噪声协方差。
    - `initialState`: 初始状态估计。
    - `initialS`: 初始估计误差协方差的平方根。

### 方法

#### 1. Predict
- `public T Predict(T u)`
  - 执行预测步骤。
  - 参数:
    - `u`: 控制输入。
  - 返回值: 预测的状态估计。

#### 2. Update
- `public T Update(T z)`
  - 执行更新步骤。
  - 参数:
    - `z`: 测量值。
  - 返回值: 更新后的状态估计。

### 代码示例
以下是一个使用 SquareRootKalmanFilter1D&lt;T> 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.Experimental.KalmanFilters;
public class SquareRootKalmanFilter1DExample
{
    public static void Main()
    { // 定义滤波器参数
      var A = 1.0f;
        var B = 0.5f;
        var H = 1.0f;
        var Q = 0.1f;
        var R = 0.1f;
        var initialState = 0.0f;
        var initialS = 1.0f;
      // 创建平方根卡尔曼滤波器实例
        var srkf = new SquareRootKalmanFilter1D<float>(A, B, H, Q, R, initialState, initialS);

        // 定义测量数据
        float[] audioSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        float[] filteredSamples = new float[audioSamples.Length];

        // 执行滤波
        for (int i = 0; i < audioSamples.Length; i++)
        {
            var u = 0.0f; // 控制输入为0
            var z = audioSamples[i]; // 当前测量值

            var predictedState = srkf.Predict(u);
            var updatedState = srkf.Update(z);

            filteredSamples[i] = updatedState;
        }

        // 输出滤波结果
        Console.WriteLine("Filtered Samples:");
        foreach (var sample in filteredSamples)
        {
            Console.WriteLine(sample);
        }
    }
}
```
## StandardKalmanFilter1D&lt;T> 类

Vorcyc.Mathematics.Experimental.KalmanFilters.StandardKalmanFilter1D&lt;T> 是一个用于一维标准卡尔曼滤波器的类。

### 属性

无公开属性。

### 构造器

#### 1. StandardKalmanFilter1D
- `public StandardKalmanFilter1D(T A, T B, T H, T Q, T R, T initialState, T initialP)`
  - 初始化卡尔曼滤波器的实例。
  - 参数:
    - `A`: 状态转移系数。
    - `B`: 控制输入系数。
    - `H`: 观测系数。
    - `Q`: 过程噪声协方差。
    - `R`: 测量噪声协方差。
    - `initialState`: 初始状态估计。
    - `initialP`: 初始估计误差协方差。

### 方法

#### 1. Predict
- `public T Predict(T u)`
  - 执行预测步骤。
  - 参数:
    - `u`: 控制输入。
  - 返回值: 预测的状态估计。

#### 2. Update
- `public T Update(T z)`
  - 执行更新步骤。
  - 参数:
    - `z`: 测量值。
  - 返回值: 更新后的状态估计。

### 代码示例
以下是一个使用 StandardKalmanFilter1D&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Experimental.KalmanFilters;
public class StandardKalmanFilter1DExample
{
    public static void Main()
    {
        // 定义滤波器参数
        var A = 1.0f;
        var B = 0.5f;
        var H = 1.0f;
        var Q = 0.1f;
        var R = 0.1f;
        var initialState = 0.0f;
        var initialP = 1.0f;
        // 创建卡尔曼滤波器实例
        var kf = new StandardKalmanFilter1D<float>(A, B, H, Q, R, initialState, initialP);

        // 定义测量数据
        float[] audioSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        float[] filteredSamples = new float[audioSamples.Length];

        // 执行滤波
        for (int i = 0; i < audioSamples.Length; i++)
        {
            var u = 0.0f; // 控制输入为0
            var z = audioSamples[i]; // 当前测量值

            var predictedState = kf.Predict(u);
            var updatedState = kf.Update(z);

            filteredSamples[i] = updatedState;
        }

        // 输出滤波结果
        Console.WriteLine("Filtered Samples:");
        foreach (var sample in filteredSamples)
        {
            Console.WriteLine(sample);
        }
    }
}
```
## StandardKalmanFilter2D&lt;T> 类

Vorcyc.Mathematics.Experimental.KalmanFilters.StandardKalmanFilter2D&lt;T> 是一个用于二维标准卡尔曼滤波器的类。

### 属性

无公开属性。

### 构造器

#### 1. StandardKalmanFilter2D
- `public StandardKalmanFilter2D(Matrix<T> A, Matrix<T> B, Matrix<T> H, Matrix<T> Q, Matrix<T> R, Matrix<T> initialState, Matrix<T> initialP)`
  - 初始化卡尔曼滤波器的实例。
  - 参数:
    - `A`: 状态转移矩阵。
    - `B`: 控制输入矩阵。
    - `H`: 观测矩阵。
    - `Q`: 过程噪声协方差。
    - `R`: 测量噪声协方差。
    - `initialState`: 初始状态估计。
    - `initialP`: 初始估计误差协方差。

### 方法

#### 1. Predict
- `public Matrix<T> Predict(Matrix<T> u)`
  - 执行预测步骤。
  - 参数:
    - `u`: 控制输入。
  - 返回值: 预测的状态估计。

#### 2. Update
- `public Matrix<T> Update(Matrix<T> z)`
  - 执行更新步骤。
  - 参数:
    - `z`: 测量值。
  - 返回值: 更新后的状态估计。

### 代码示例
以下是一个使用 StandardKalmanFilter2D&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.Experimental.KalmanFilters;
using Vorcyc.Mathematics.LinearAlgebra;

public class StandardKalmanFilter2DExample
{
    public static void Main()
    { 
        // 定义滤波器参数
        var A = new Matrix<float>(new float[,] { { 1, 0 }, { 0, 1 } });
        var B = new Matrix<float>(new float[,] { { 0.5f }, { 1 } });
        var H = new Matrix<float>(new float[,] { { 1, 0 }, { 0, 1 } });
        var Q = new Matrix<float>(new float[,] { { 0.1f, 0 }, { 0, 0.1f } });
        var R = new Matrix<float>(new float[,] { { 0.1f, 0 }, { 0, 0.1f } });
        var initialState = new Matrix<float>(new float[,] { { 0 }, { 0 } });
        var initialP = new Matrix<float>(new float[,] { { 1, 0 }, { 0, 1 } });
        // 创建卡尔曼滤波器实例
        var kf = new StandardKalmanFilter2D<float>(A, B, H, Q, R, initialState, initialP);

        // 定义测量数据
        var u = new Matrix<float>(new float[,] { { 1 } });
        var z = new Matrix<float>(new float[,] { { 1 }, { 1 } });

        // 执行预测步骤
        var predictedState = kf.Predict(u);

        // 执行更新步骤
        var updatedState = kf.Update(z);

        // 输出滤波结果
        Console.WriteLine("Predicted State:");
        PrintMatrix(predictedState);

        Console.WriteLine("Updated State:");
        PrintMatrix(updatedState);
    }

    private static void PrintMatrix(Matrix<float> matrix)
    {
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                Console.Write($"{matrix[i, j]} ");
            }
            Console.WriteLine();
        }
    }
}
```

## UnscentedKalmanFilter1D&lt;T> 类

Vorcyc.Mathematics.Experimental.KalmanFilters.UnscentedKalmanFilter1D&lt;T> 是一个用于一维无迹卡尔曼滤波器的类。

### 属性

无公开属性。

### 构造器

#### 1. UnscentedKalmanFilter1D
- `public UnscentedKalmanFilter1D(T A, T B, T H, T Q, T R, T initialState, T initialP)`
  - 初始化无迹卡尔曼滤波器的实例。
  - 参数:
    - `A`: 状态转移系数。
    - `B`: 控制输入系数。
    - `H`: 观测系数。
    - `Q`: 过程噪声协方差。
    - `R`: 测量噪声协方差。
    - `initialState`: 初始状态估计。
    - `initialP`: 初始估计误差协方差。

### 方法

#### 1. Predict
- `public T Predict(T u, Func<T, T, T> stateTransitionFunc)`
  - 执行预测步骤。
  - 参数:
    - `u`: 控制输入。
    - `stateTransitionFunc`: 状态转移函数。
  - 返回值: 预测的状态估计。

#### 2. Update
- `public T Update(T z, Func<T, T> measurementFunc)`
  - 执行更新步骤。
  - 参数:
    - `z`: 测量值。
    - `measurementFunc`: 观测函数。
  - 返回值: 更新后的状态估计。

### 代码示例
以下是一个使用 UnscentedKalmanFilter1D&lt;T> 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.Experimental.KalmanFilters;
public class UnscentedKalmanFilter1DExample
{
    public static void Main()
    { 
        // 定义滤波器参数
        var A = 1.0f;
        var B = 0.5f;
        var H = 1.0f;
        var Q = 0.1f;
        var R = 0.1f;
        var initialState = 0.0f;
        var initialP = 1.0f;
        // 创建无迹卡尔曼滤波器实例
        var ukf = new UnscentedKalmanFilter1D<float>(A, B, H, Q, R, initialState, initialP);

        // 定义非线性状态转移函数
        float NonlinearStateTransitionFunction(float x, float u)
        {
            return x + u;
        }

        // 定义非线性观测函数
        float NonlinearMeasurementFunction(float x)
        {
            return x;
        }

        // 定义测量数据
        float[] audioSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        float[] filteredSamples = new float[audioSamples.Length];

        // 执行滤波
        for (int i = 0; i < audioSamples.Length; i++)
        {
            var u = 0.0f; // 控制输入为0
            var z = audioSamples[i]; // 当前测量值

            var predictedState = ukf.Predict(u, NonlinearStateTransitionFunction);
            var updatedState = ukf.Update(z, NonlinearMeasurementFunction);

            filteredSamples[i] = updatedState;
        }

        // 输出滤波结果
        Console.WriteLine("Filtered Samples:");
        foreach (var sample in filteredSamples)
        {
            Console.WriteLine(sample);
        }
    }
}
```

---

> 以下类型都位于 Vorcyc.Mathematics.Experimental.Signals 命名空间

## ITimeDomainCharacteristics 接口

`Vorcyc.Mathematics.Experimental.Signals.ITimeDomainCharacteristics` 定义了时域信号的关键特征属性接口，用于分析信号在时域中的统计和结构特征。

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Period` | `float` | 信号周期 |
| `Amplitude` | `float` | 信号幅度（最大值与最小值之差） |
| `TotalPower` | `float` | 总功率（样本平方和） |
| `AveragePower` | `float` | 平均功率（样本平方和 / 样本数） |
| `TotalEnergy` | `float` | 总能量（等同于总功率） |
| `AverageEnergy` | `float` | 平均能量（样本平方均值） |
| `Rms` | `float` | 均方根值（平均能量的平方根） |
| `ZeroCrossingRate` | `float` | 过零率，值范围 [0, 1] |
| `Entropy` | `float` | 归一化 Shannon 熵，值范围 [0, 1] |

### 方法

#### 1. GetEntropy
- `float GetEntropy(int binCount = 32)`
  - 使用指定的 bin 数量计算 Shannon 熵。
  - 参数:
    - `binCount`: 用于划分数据的 bin 数量，默认为 32。
  - 返回值: 计算得到的熵值。

---

## ITimeDomainSignal 接口

`Vorcyc.Mathematics.Experimental.Signals.ITimeDomainSignal` 继承自 `ITimeDomainCharacteristics`，定义了时域信号的基础契约。

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Duration` | `TimeSpan` | 信号持续时间 |
| `SamplingRate` | `float` | 采样率 |
| `Length` | `int` | 信号长度 |

### 方法

#### 1. NotifySamplesModified
- `void NotifySamplesModified()`
  - 在修改采样值后调用此方法，通知信号对象样本已被修改。

#### 2. TransformToFrequencyDomain
- `FrequencyDomain TransformToFrequencyDomain(WindowType? window = null, FftVersion fftVersion = FftVersion.Normal)`
  - 将时域信号转换为频域信号。
  - 参数:
    - `window`: 可选的窗函数类型。
    - `fftVersion`: FFT 执行模式，默认为 `FftVersion.Normal`。
  - 返回值: `FrequencyDomain` 对象。

---

## ISingleThreadTimeDomainSignal 接口

`Vorcyc.Mathematics.Experimental.Signals.ISingleThreadTimeDomainSignal` 继承自 `ITimeDomainSignal`，提供单线程场景下的采样数据访问。

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Samples` | `Span<float>` | 信号采样数据 |

### 方法

#### 1. Resample
- `Signal Resample(int destnationSamplingRate, FirFilter? filter = null, int order = 15)`
  - 重采样信号到新的采样率。
  - 参数:
    - `destnationSamplingRate`: 目标采样率。
    - `filter`: 可选的 FIR 滤波器。
    - `order`: 滤波器阶数，默认为 15。
  - 返回值: 重采样后的 `Signal` 对象。

---

## IModifiableTimeDomainSignal 接口

`Vorcyc.Mathematics.Experimental.Signals.IModifiableTimeDomainSignal` 继承自 `ITimeDomainSignal`，支持运行时修改采样数据，包括追加、插入、删除和重采样操作。

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Samples` | `ModifiableTimeDomainSignal.LockedSamplesView` | 锁保护的采样数据视图 |

### 方法

| 方法 | 说明 |
|------|------|
| `AppendAsync(float[], CancellationToken)` | 异步追加采样数据（可从其他线程调用） |
| `FlushPendingAppends()` | 刷新待追加的采样数据，返回实际追加的数量 |
| `Insert(int, float[])` | 在指定索引处插入采样数据 |
| `Insert(TimeSpan, float[])` | 在指定时间点处插入采样数据 |
| `RemoveRange(int, int)` | 移除指定索引开始的指定数量的采样数据 |
| `RemoveRange(TimeSpan, TimeSpan)` | 移除指定时间段的采样数据 |
| `Resample(int, FirFilter?, int)` | 重采样信号，返回 `ModifiableTimeDomainSignal` |

---

## IFrequencyDomainCharacteristics 接口

`Vorcyc.Mathematics.Experimental.Signals.IFrequencyDomainCharacteristics` 定义了频域分析的常用属性。

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Magnitudes` | `float[]` | 频域信号的幅度数组 |
| `Centroid` | `float` | 频谱质心 |
| `Frequency` | `float` | 频域信号的频率 |
| `Phases` | `float[]` | 频域信号的相位数组 |
| `AngularVelocities` | `float[]` | 根据相位计算的角速度数组 |

---

## IFrequencyDomain 接口

`Vorcyc.Mathematics.Experimental.Signals.IFrequencyDomain` 继承自 `IFrequencyDomainCharacteristics`，定义了完整的频域信号接口。

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Offset` | `int` | 频域变换在时域信号中的偏移量 |
| `TransformLength` | `int` | FFT 变换长度（2 的幂次） |
| `ActualLength` | `int` | 实际有效数据长度 |
| `Resolution` | `float` | 频率分辨率 |
| `WindowApplied` | `WindowType?` | 所使用的窗函数类型 |
| `Result` | `ComplexFp32[]` | FFT 变换结果的复数数组 |
| `Signal` | `ITimeDomainSignal` | 关联的原始时域信号 |

### 方法

#### 1. Inverse
- `void Inverse()`
  - 对 FFT 结果执行逆变换，将结果写回原始时域信号的采样数据中。

---

## FrequencyDomain 结构体

`Vorcyc.Mathematics.Experimental.Signals.FrequencyDomain` 是一个**只读结构体**（`readonly struct`），实现了 `IFrequencyDomain` 接口，用于表示频域信号。该结构体封装了傅里叶变换的结果，包括原始信号、窗函数信息和计算得到的频域特征。

### 属性

#### 1. Offset
- `public int Offset { get; }`
  - 获取频域信号在原始数据中的起始量。

#### 2. ActualLength
- `public int ActualLength { get; }`
  - 获取未补0的长度，也是实际有效数据的长度。

#### 3. TransformLength
- `public int TransformLength { get; }`
  - 获取FFT的长度。该值为2的N次方，通常会比 `ActualLength` 大。

#### 4. Resolution
- `public float Resolution { get; }`
  - 获取频率分辨率。

#### 5. WindowApplied
- `public WindowType? WindowApplied { get; }`
  - 获取FFT所使用的窗口类型。

#### 6. Result
- `public ComplexFp32[] Result { get; }`
  - 获取频域信号，即FFT变换的结果。

#### 7. Signal
- `public ITimeDomainSignal Signal { get; }`
  - 获取所关联的实信号。

#### 8. Magnitudes
- `public float[] Magnitudes { get; }`
  - 使用复信号的一半来计算幅度，这样会丢弃镜像部分。

#### 9. Centroid
- `public float Centroid { get; }`
  - 使用复信号的一半来计算质心，这样会丢弃镜像部分。

#### 10. Frequency
- `public float Frequency { get; }`
  - 使用完整的复信号来计算频率。

#### 11. Phases
- `public float[] Phases { get; }`
  - 使用复信号的一半来计算相位，这样会丢弃镜像部分。

#### 12. AngularVelocities
- `public float[] AngularVelocities { get; }`
  - 根据复信号的相位计算角速度。

#### 13. PowerSpectralDensity
- `public float[] PowerSpectralDensity { get; }`
  - 获取功率谱密度。

### 方法

#### 1. IndexToFrequency
- `public static float IndexToFrequency(int index, int samplingRate, int fftLen)`
  - 将索引转换为频率。
- `public float IndexToFrequency(int index)`
  - 将索引转换为频率（实例方法）。

#### 2. FrequencyToIndex
- `public int FrequencyToIndex(float frequency)`
  - 将频率转换为索引。

#### 3. Inverse
- `public void Inverse()`
  - 对FFT结果进行逆变换，并将结果写回信号的采样数据中。

### 代码示例
```csharp
using System;
using Vorcyc.Mathematics.Experimental.Signals;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

public class FrequencyDomainExample
{
    public static void Main()
    {
        // 定义时域信号
        var signal = new Signal(1000, 44100);
        signal.GenerateWave(WaveShape.Sine, 440, Behaviour.Replace);

        // 将时域信号转换为频域信号
        var frequencyDomain = signal.TransformToFrequencyDomain(WindowType.Hamming);

        // 输出频域信号的属性
        Console.WriteLine($"ActualLength: {frequencyDomain.ActualLength}");
        Console.WriteLine($"TransformLength: {frequencyDomain.TransformLength}");
        Console.WriteLine($"Resolution: {frequencyDomain.Resolution}");
        Console.WriteLine($"Centroid: {frequencyDomain.Centroid}");
        Console.WriteLine($"Frequency: {frequencyDomain.Frequency}");
    }
}
```

---

## Signal 类

`Vorcyc.Mathematics.Experimental.Signals.Signal` 是一个用于表示单线程时域信号的类，实现了 `ISingleThreadTimeDomainSignal`、`ICloneable<Signal>`、`IDisposable` 和 `IEquatable<Signal>` 接口。内部使用 `POHBuffer<float>` 存储采样数据。

### 属性

#### 1. Samples
- `public Span<float> Samples { get; }`
  - 获取信号样本数组。

#### 2. SamplingRate
- `public float SamplingRate { get; }`
  - 获取信号的采样率。

#### 3. Duration
- `public TimeSpan Duration { get; }`
  - 获取信号的持续时间。

#### 4. Length
- `public int Length { get; }`
  - 获取信号的长度。

#### 5. Amplitude
- `public float Amplitude { get; }`
  - 获取信号的幅度（最大值与最小值之差）。

#### 6. Period
- `public float Period { get; }`
  - 获取信号的周期（采样率的倒数）。

#### 7. TotalPower
- `public float TotalPower { get; }`
  - 获取信号的总功率（样本平方和）。

#### 8. AveragePower
- `public float AveragePower { get; }`
  - 获取信号的平均功率（样本平方和 / 样本数）。

#### 9. TotalEnergy
- `public float TotalEnergy { get; }`
  - 获取信号的总能量（样本平方和）。

#### 10. AverageEnergy
- `public float AverageEnergy { get; }`
  - 获取信号的平均能量（样本平方均值）。

#### 11. Rms
- `public float Rms { get; }`
  - 获取信号的均方根值。

#### 12. ZeroCrossingRate
- `public float ZeroCrossingRate { get; }`
  - 获取信号的过零率。

#### 13. Entropy
- `public float Entropy { get; }`
  - 获取信号的归一化 Shannon 熵。

#### 14. UnderlyingBuffer
- `public POHBuffer<float> UnderlyingBuffer { get; }`
  - 获取底层的固定缓冲区。

### 构造器

#### 1. Signal(int sampleCount, float samplingRate)
- 根据采样数量和采样率初始化信号。

#### 2. Signal(TimeSpan duration, float samplingRate)
- 根据时间长度和采样率初始化信号。

### 方法

#### 1. Clone
- `public Signal Clone()`
  - 创建信号的副本。

#### 2. TransformToFrequencyDomain
- `public FrequencyDomain TransformToFrequencyDomain(WindowType? window = null, FftVersion fftVersion = FftVersion.Normal)`
  - 将时域信号转换为频域信号。

#### 3. Resample
- `public Signal Resample(int destnationSamplingRate, FirFilter? filter = null, int order = 15)`
  - 重采样信号。

#### 4. Indexer
- `public float this[int index] { get; set; }`
  - 获取或设置指定索引处的采样值。

- `public SignalSegment? this[int start, int length, bool throwException = false] { get; }`
  - 以索引获取信号段的子段。

- `public SignalSegment? this[TimeSpan startTime, TimeSpan duration, bool throwException = false] { get; }`
  - 以时间量获取信号的子段。

#### 5. Operators
- `+`, `-`, `*`, `/` 运算符支持 Signal 与 float 及 Signal 与 Signal 之间的运算。

### 代码示例
```csharp
using System;
using Vorcyc.Mathematics.Experimental.Signals;
using Vorcyc.Mathematics.SignalProcessing.Windowing;

public class SignalExample
{
    public static void Main()
    {
        // 创建信号（1000 个采样，采样率 44100）
        var signal = new Signal(1000, 44100);

        // 生成正弦波
        signal.GenerateWave(WaveShape.Sine, 440, Behaviour.Replace);

        // 输出时域信号的属性
        Console.WriteLine($"Duration: {signal.Duration}");
        Console.WriteLine($"Length: {signal.Length}");
        Console.WriteLine($"Amplitude: {signal.Amplitude}");
        Console.WriteLine($"TotalPower: {signal.TotalPower}");
        Console.WriteLine($"AveragePower: {signal.AveragePower}");
        Console.WriteLine($"Rms: {signal.Rms}");
        Console.WriteLine($"ZeroCrossingRate: {signal.ZeroCrossingRate}");
        Console.WriteLine($"Entropy: {signal.Entropy}");

        // 转换为频域
        var frequencyDomain = signal.TransformToFrequencyDomain(WindowType.Hamming);
        Console.WriteLine($"Frequency: {frequencyDomain.Frequency}");

        // 重采样
        var resampled = signal.Resample(22050);
        Console.WriteLine($"Resampled Length: {resampled.Length}");

        signal.Dispose();
    }
}
```

---

## ModifiableTimeDomainSignal 类

`Vorcyc.Mathematics.Experimental.Signals.ModifiableTimeDomainSignal` 是一个支持运行时安全修改采样数据的时域信号类，实现了 `IModifiableTimeDomainSignal` 和 `IDisposable` 接口。提供锁保护的可写和只读视图、异步追加（通过 Channel）和批量刷新。

### 构造器

#### 1. ModifiableTimeDomainSignal(int sampleCount, float samplingRate)
- 根据采样数量和采样率初始化。

#### 2. ModifiableTimeDomainSignal(TimeSpan duration, float samplingRate)
- 根据时间长度和采样率初始化。

### 属性

与 `Signal` 类类似，包含 `Samples`（返回 `LockedSamplesView`）、`SamplingRate`、`Duration`、`Length`，以及所有 `ITimeDomainCharacteristics` 属性。

### 方法

| 方法 | 说明 |
|------|------|
| `AppendAsync(float[], CancellationToken)` | 异步追加采样数据（线程安全） |
| `FlushPendingAppends()` | 刷新待追加的采样数据 |
| `Insert(int, float[])` | 在指定索引处插入 |
| `Insert(TimeSpan, float[])` | 在指定时间点处插入 |
| `RemoveRange(int, int)` | 移除指定范围 |
| `RemoveRange(TimeSpan, TimeSpan)` | 移除指定时间段 |
| `Resample(int, FirFilter?, int)` | 重采样 |
| `TransformToFrequencyDomain(WindowType?, FftVersion)` | 转换到频域 |
| `Dispose()` | 释放资源 |

---

## SignalSegment 结构体

`Vorcyc.Mathematics.Experimental.Signals.SignalSegment` 是一个**只读结构体**（`readonly struct`），实现了 `ITimeDomainSignal` 和 `ISingleThreadTimeDomainSignal` 接口，用于表示信号的只读连续子段。时域特征属性采用延迟计算（`Lazy<T>`）。

### 属性

#### 1. Signal
- `public Signal Signal { get; }`
  - 获取所属的父信号。

#### 2. Start
- `public int Start { get; }`
  - 获取信号段的起始位置。

#### 3. StartTime
- `public TimeSpan StartTime { get; }`
  - 获取当前信号段的起始时间。

#### 4. Length
- `public int Length { get; }`
  - 获取信号段的长度。

#### 5. Duration
- `public TimeSpan Duration { get; }`
  - 获取信号段的持续时间。

#### 6. Samples
- `public Span<float> Samples { get; }`
  - 获取信号段的采样数据。

#### 7. SamplingRate
- `public float SamplingRate { get; }`
  - 获取信号的采样率。

#### 8. Amplitude / Period / TotalPower / AveragePower / TotalEnergy / AverageEnergy / Rms / ZeroCrossingRate / Entropy
- 所有 `ITimeDomainCharacteristics` 属性均可用，采用延迟计算 + SIMD 优化。

### 方法

#### 1. TransformToFrequencyDomain
- `public FrequencyDomain TransformToFrequencyDomain(WindowType? window = null, FftVersion fftVersion = FftVersion.Normal)`
  - 将信号段转换为频域信号。

#### 2. Resample
- `public Signal Resample(int destnationSamplingRate, FirFilter? filter = null, int order = 15)`
  - 重采样并返回新的信号。

#### 3. Decouple
- `public Signal Decouple()`
  - 从所在信号中脱离成为单独的信号。

#### 4. Operators
- `+`, `-`, `*`, `/` 运算符支持 SignalSegment 与 float 及 SignalSegment 与 SignalSegment 之间的运算。

### 代码示例
```csharp
using System;
using Vorcyc.Mathematics.Experimental.Signals;

public class SignalSegmentExample
{
    public static void Main()
    {
        var signal = new Signal(10000, 44100);
        signal.GenerateWave(WaveShape.Sine, 440, Behaviour.Replace);

        // 获取信号段（第 1000 个采样开始，取 5000 个采样）
        var segment = signal[1000, 5000];
        if (segment is SignalSegment seg)
        {
            Console.WriteLine($"Start: {seg.Start}");
            Console.WriteLine($"Length: {seg.Length}");
            Console.WriteLine($"Amplitude: {seg.Amplitude}");
            Console.WriteLine($"TotalPower: {seg.TotalPower}");
            Console.WriteLine($"Rms: {seg.Rms}");
            Console.WriteLine($"ZeroCrossingRate: {seg.ZeroCrossingRate}");

            // 脱离为独立信号
            var decoupled = seg.Decouple();
            Console.WriteLine($"Decoupled Length: {decoupled.Length}");
            decoupled.Dispose();
        }

        signal.Dispose();
    }
}
```

---

## SignalGeneratingExtension 类

`Vorcyc.Mathematics.Experimental.Signals.SignalGeneratingExtension` 是一个用于扩展信号类功能的静态类，提供了生成各种波形的方法。

### WaveShape 枚举

| 值 | 说明 |
|----|------|
| `Sine` | 正弦波 |
| `Cosine` | 余弦波 |
| `Square` | 方波 |
| `Sawtooth` | 锯齿波 |
| `Triangle` | 三角波 |
| `WhiteNoise` | 白噪声 |
| `PinkNoise` | 粉红噪声 |

### Behaviour 枚举

| 值 | 说明 |
|----|------|
| `Replace` | 替换现有信号 |
| `ElementWiseAdd` | 与现有信号逐元素相加 |
| `ElementWiseSubtract` | 与现有信号逐元素相减 |
| `ElementWiseMultiply` | 与现有信号逐元素相乘 |
| `ElementWiseDivide` | 与现有信号逐元素相除 |

### 方法

#### 1. GenerateWave
- `public static void GenerateWave(this ISingleThreadTimeDomainSignal signal, WaveShape shape, float frequency, Behaviour behaviour = Behaviour.Replace)`
  - 生成指定波形，并根据行为对信号进行处理。
  - 参数:
    - `signal`: 实现 `ISingleThreadTimeDomainSignal` 的信号对象。
    - `shape`: 波形类型。
    - `frequency`: 波形的频率。
    - `behaviour`: 处理行为，默认为 `Behaviour.Replace`。

### 代码示例
```csharp
using System;
using Vorcyc.Mathematics.Experimental.Signals;

public class SignalGeneratingExtensionExample
{
    public static void Main()
    {
        var signal = new Signal(1000, 44100);

        // 生成正弦波并替换现有信号
        signal.GenerateWave(WaveShape.Sine, 440, Behaviour.Replace);

        // 叠加方波
        signal.GenerateWave(WaveShape.Square, 220, Behaviour.ElementWiseAdd);

        // 叠加白噪声
        signal.GenerateWave(WaveShape.WhiteNoise, 0, Behaviour.ElementWiseAdd);

        Console.WriteLine($"Amplitude: {signal.Amplitude}");
        Console.WriteLine($"Rms: {signal.Rms}");

        signal.Dispose();
    }
}
```

---

> 以下类型都位于 Vorcyc.Mathematics.Experimental.CurveFitting 命名空间

## CurveFitter&lt;T> 类

`Vorcyc.Mathematics.Experimental.CurveFitting.CurveFitter<T>` 是一个泛型静态类，提供多种曲线拟合方法的统一入口。类型参数 `T` 约束为 `unmanaged, IFloatingPointIeee754<T>`。

### 方法

| 方法 | 说明 | 模型 |
|------|------|------|
| `Linear(xData, yData, optimizationMode)` | 线性回归 | y = ax + b |
| `Polynomial(xData, yData, degree, optimizationMode)` | 多项式回归 | y = a₀ + a₁x + ... + aₙxⁿ |
| `Exponential(xData, yData, optimizationMode)` | 指数回归 | y = a·e^(bx) |
| `Logarithmic(xData, yData, optimizationMode)` | 对数回归 | y = a + b·ln(x) |
| `Power(xData, yData, optimizationMode)` | 幂回归 | y = a·x^b |
| `Sinusoidal(xData, yData, maxIterations)` | 正弦回归 | y = A·sin(Bx + C) + D |
| `CubicSpline(xData, yData)` | 三次样条插值 | 分段三次多项式 |
| `LocallyWeighted(xData, yData, bandwidth)` | 局部加权回归 (LOWESS) | 局部线性 |
| `MovingAverage(xData, yData, windowSize)` | 移动平均 | 窗口平均 |
| `Nonlinear(xData, yData, model, initialParams, ...)` | 非线性回归 (LM 算法) | 自定义 f(x, params) |
| `Nonlinear(DataRow[] xData, yData, model, initialParams, ...)` | 多变量非线性回归 | 自定义 f(xVector, params) |
| `GaussianProcess(xData, yData, ...)` | 高斯过程回归 (GPR) | 核函数 |
| `GaussianProcess(DataRow[] xData, yData, ...)` | 多变量 GPR | 核函数 |
| `NeuralNetwork(xData, yData, epochs, hiddenNodes, ...)` | 神经网络回归 | MLP |
| `NeuralNetwork(DataRow[] xData, yData, epochs, hiddenNodes, ...)` | 多变量神经网络回归 | MLP |
| `BayesianLinear(DataRow[] xData, yData, alpha, beta)` | 贝叶斯线性回归 | 带先验线性 |

> `optimizationMode` 参数默认为 `OptimizationMode.SIMD`，仅支持 `float` 和 `double`。使用其他类型时请选择 `OptimizationMode.Normal`。

### 代码示例
```csharp
using System;
using Vorcyc.Mathematics.Experimental.CurveFitting;

public class CurveFitterExample
{
    public static void Main()
    {
        float[] xData = { 1, 2, 3, 4, 5 };
        float[] yData = { 2.1f, 4.0f, 5.9f, 8.1f, 10.0f };

        // 线性回归
        var linear = CurveFitter<float>.Linear(xData, yData);
        Console.WriteLine($"Linear MSE: {linear.MeanSquaredError}");
        Console.WriteLine($"Predict(6): {linear.Predict(6)}");

        // 多项式回归
        var poly = CurveFitter<float>.Polynomial(xData, yData, degree: 2);
        Console.WriteLine($"Polynomial MSE: {poly.MeanSquaredError}");

        // 指数回归
        float[] yExp = { 1.0f, 2.7f, 7.4f, 20.1f, 54.6f };
        var exp = CurveFitter<float>.Exponential(xData, yExp);
        Console.WriteLine($"Exponential Predict(6): {exp.Predict(6)}");
    }
}
```

---

## CurveFittingMethod 枚举

`Vorcyc.Mathematics.Experimental.CurveFitting.CurveFittingMethod` 枚举定义了所有支持的曲线拟合方法。

| 值 | 说明 |
|----|------|
| `LinearRegression` | 线性回归 |
| `PolynomialRegression` | 多项式回归 |
| `ExponentialRegression` | 指数回归 |
| `LogarithmicRegression` | 对数回归 |
| `PowerRegression` | 幂回归 |
| `SinusoidalRegression` | 正弦回归 |
| `CubicSplineInterpolation` | 三次样条插值 |
| `LocallyWeightedRegression` | 局部加权回归 |
| `MovingAverage` | 移动平均 |
| `NonlinearRegression` | 非线性回归 |
| `GaussianProcessRegression` | 高斯过程回归 |
| `NeuralNetworkRegression` | 神经网络回归 |
| `SupportVectorRegression` | 支持向量回归 |
| `BayesianRegression` | 贝叶斯回归 |

---

## FitResult&lt;T> 类

`Vorcyc.Mathematics.Experimental.CurveFitting.FitResult<T>` 表示曲线拟合的结果。

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Predict` | `Func<T, T>` | 拟合后的预测函数 |
| `Parameters` | `T[]` | 拟合参数（斜率、截距或系数等） |
| `MeanSquaredError` | `T` | 均方误差 (MSE) |

> `MultiColumnFitResult<T>` 用于多变量输入场景，其 `MultiPredict` 属性类型为 `Func<DataRow<T>, T>`。

---

## DataRow&lt;T> 结构体

`Vorcyc.Mathematics.Experimental.CurveFitting.DataRow<T>` 是一个只读结构体，表示一行多列的数据，用于多变量输入的拟合方法。

### 构造器

- `public DataRow(params T[] columns)`
  - 初始化一行数据。

### 属性

- `public T this[int index]` — 获取指定列的值。
- `public int ColumnCount` — 获取列数。

---

## OptimizationMode 枚举

`Vorcyc.Mathematics.Experimental.CurveFitting.OptimizationMode` 枚举定义了拟合算法的优化模式。

| 值 | 说明 |
|----|------|
| `Normal` | 使用标准托管代码，支持所有 `IFloatingPointIeee754<T>` 类型 |
| `SIMD` | 使用 SIMD 优化，仅支持 `float` 和 `double` |

---








