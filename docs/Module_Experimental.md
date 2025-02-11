当前位置 : [根目录](README.md)/[实验性模块](Module_Experimental.md)

# 实验性模块 - Experimental Module


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

> 以下类型都位于 Vorcyc.Mathematics.Experimental.Signals 命名空间

:ledger:目录  
- :bookmark: [FrequencyDomain 类](#frequencydomain-类)
- :bookmark: [Signal 类](#signal-类)
- :bookmark: [SignalExtension 类](#signalextension-类)
- :bookmark: [SignalSegment 类](#signalsegment-类)

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

## FrequencyDomain 类

Vorcyc.Mathematics.Experimental.Signals.FrequencyDomain 是一个用于表示频域信号的类。

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
  - 参数:
    - `index`: 索引。
    - `samplingRate`: 采样率。
    - `fftLen`: FFT长度。
  - 返回值: 对应的频率。

- `public float IndexToFrequency(int index)`
  - 将索引转换为频率。
  - 参数:
    - `index`: 索引。
  - 返回值: 对应的频率。

#### 2. FrequencyToIndex
- `public int FrequencyToIndex(float frequency)`
  - 将频率转换为索引。
  - 参数:
    - `frequency`: 频率。
  - 返回值: 对应的索引。

#### 3. Inverse
- `public void Inverse()`
  - 对FFT结果进行逆变换，并将结果写回信号的采样数据中。

### 代码示例
以下是一个使用 FrequencyDomain 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.Experimental.Signals;
using Vorcyc.Mathematics.SignalProcessing.Fourier;
using Vorcyc.Mathematics.SignalProcessing.Windowing;
public class FrequencyDomainExample
{
    public static void Main()
    { 
        // 定义时域信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var samplingRate = 1000;
        var signal = new Signal(samples, samplingRate);
        // 将时域信号转换为频域信号
        var frequencyDomain = signal.TransformToFrequencyDomain(WindowType.Hamming);

        // 输出频域信号的属性
        Console.WriteLine($"Offset: {frequencyDomain.Offset}");
        Console.WriteLine($"ActualLength: {frequencyDomain.ActualLength}");
        Console.WriteLine($"TransformLength: {frequencyDomain.TransformLength}");
        Console.WriteLine($"Resolution: {frequencyDomain.Resolution}");
        Console.WriteLine($"WindowApplied: {frequencyDomain.WindowApplied}");

        // 输出频域信号的结果
        Console.WriteLine("FFT Result:");
        foreach (var value in frequencyDomain.Result)
        {
            Console.WriteLine(value);
        }

        // 输出幅度
        Console.WriteLine("Magnitudes:");
        foreach (var magnitude in frequencyDomain.Magnitudes)
        {
            Console.WriteLine(magnitude);
        }

        // 输出质心
        Console.WriteLine($"Centroid: {frequencyDomain.Centroid}");

        // 输出频率
        Console.WriteLine($"Frequency: {frequencyDomain.Frequency}");

        // 输出相位
        Console.WriteLine("Phases:");
        foreach (var phase in frequencyDomain.Phases)
        {
            Console.WriteLine(phase);
        }

        // 输出角速度
        Console.WriteLine("AngularVelocities:");
        foreach (var angularVelocity in frequencyDomain.AngularVelocities)
        {
            Console.WriteLine(angularVelocity);
        }

        // 输出功率谱密度
        Console.WriteLine("PowerSpectralDensity:");
        foreach (var psd in frequencyDomain.PowerSpectralDensity)
        {
            Console.WriteLine(psd);
        }

        // 执行逆变换
        frequencyDomain.Inverse();
        Console.WriteLine("Inverse FFT Result:");
        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```



## Signal 类

Vorcyc.Mathematics.Experimental.Signals.Signal 是一个用于表示信号的类，包含信号样本和采样率，并提供计算信号属性的方法。

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

#### 7. Power
- `public float Power { get; }`
  - 获取信号的功率（样本平方和的平均值）。

#### 8. Energy
- `public float Energy { get; }`
  - 获取信号的能量（样本平方和）。

### 方法

#### 1. Clone
- `public Signal Clone()`
  - 创建信号的副本。
  - 返回值: 信号的副本。

#### 2. TransformToFrequencyDomain
- `public FrequencyDomain TransformToFrequencyDomain(WindowType? window = null)`
  - 将时域信号转换为频域信号。
  - 参数:
    - `window`: 加窗类型（可选）。
  - 返回值: 频域信号的 `FrequencyDomain` 对象。

#### 3. Resample
- `public Signal Resample(int destnationSamplingRate, FirFilter? filter = null, int order = 15)`
  - 重采样信号。
  - 参数:
    - `destnationSamplingRate`: 目标采样率。
    - `filter`: 可选的 FIR 滤波器。
    - `order`: 滤波器阶数，默认为 15。
  - 返回值: 重采样后的 `Signal` 对象。

#### 4. Indexer
- `public float this[int index] { get; set; }`
  - 获取或设置指定索引处的采样值。
  - 参数:
    - `index`: 采样值的索引。
  - 返回值: 指定索引处的采样值。

- `public SignalSegment? this[int start, int length, bool throwException = false] { get; }`
  - 以索引获取信号段的子段。
  - 参数:
    - `start`: 子段的起始索引。
    - `length`: 子段的长度。
    - `throwException`: 是否允许抛出异常。
  - 返回值: 指定起始位置和长度的信号子段。若索引超出边界则返回 null。

- `public SignalSegment? this[TimeSpan startTime, TimeSpan duration, bool throwException = false] { get; }`
  - 以时间量获取信号的子段。
  - 参数:
    - `startTime`: 子段的起始时间。
    - `duration`: 子段的时长。
    - `throwException`: 是否允许抛出异常。
  - 返回值: 指定起始时间和时长的信号子段。若时间超出边界则返回 null。

#### 5. Operators
- `public static Signal operator +(Signal left, float right)`
  - 将信号与浮点数相加。
  - 参数:
    - `left`: 信号。
    - `right`: 浮点数。
  - 返回值: 相加后的信号。

- `public static Signal? operator +(Signal left, Signal right)`
  - 将两个信号相加。
  - 参数:
    - `left`: 左侧信号。
    - `right`: 右侧信号。
  - 返回值: 相加后的信号。若信号长度或采样率不匹配则返回 null。

- `public static Signal operator -(Signal left, float right)`
  - 将信号与浮点数相减。
  - 参数:
    - `left`: 信号。
    - `right`: 浮点数。
  - 返回值: 相减后的信号。

- `public static Signal? operator -(Signal left, Signal right)`
  - 将两个信号相减。
  - 参数:
    - `left`: 左侧信号。
    - `right`: 右侧信号。
  - 返回值: 相减后的信号。若信号长度或采样率不匹配则返回 null。

- `public static Signal operator *(Signal left, float right)`
  - 将信号与浮点数相乘。
  - 参数:
    - `left`: 信号。
    - `right`: 浮点数。
  - 返回值: 相乘后的信号。

- `public static Signal? operator *(Signal left, Signal right)`
  - 将两个信号相乘。
  - 参数:
    - `left`: 左侧信号。
    - `right`: 右侧信号。
  - 返回值: 相乘后的信号。若信号长度或采样率不匹配则返回 null。

- `public static Signal operator /(Signal left, float right)`
  - 将信号与浮点数相除。
  - 参数:
    - `left`: 信号。
    - `right`: 浮点数。
  - 返回值: 相除后的信号。

### 代码示例
以下是一个使用 Signal 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.Experimental.Signals;
using Vorcyc.Mathematics.SignalProcessing.Filters.Fda;
using Vorcyc.Mathematics.SignalProcessing.Windowing;
public class SignalExample
{
    public static void Main()
    {
        // 定义时域信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
        var samplingRate = 1000; 
        var signal = new Signal(samples, samplingRate);

      // 输出时域信号的属性
        Console.WriteLine($"Duration: {signal.Duration}");
        Console.WriteLine($"SamplingRate: {signal.SamplingRate}");
        Console.WriteLine($"Length: {signal.Length}");
        Console.WriteLine($"Amplitude: {signal.Amplitude}");
        Console.WriteLine($"Period: {signal.Period}");
        Console.WriteLine($"Power: {signal.Power}");
        Console.WriteLine($"Energy: {signal.Energy}");

        // 将时域信号转换为频域信号
        var frequencyDomain = signal.TransformToFrequencyDomain(WindowType.Hamming);

        // 输出频域信号的属性
        Console.WriteLine($"Offset: {frequencyDomain.Offset}");
        Console.WriteLine($"ActualLength: {frequencyDomain.ActualLength}");
        Console.WriteLine($"TransformLength: {frequencyDomain.TransformLength}");
        Console.WriteLine($"Resolution: {frequencyDomain.Resolution}");
        Console.WriteLine($"WindowApplied: {frequencyDomain.WindowApplied}");

        // 重采样信号
        var resampledSignal = signal.Resample(2000);

        // 输出重采样后的信号属性
        Console.WriteLine($"Resampled Duration: {resampledSignal.Duration}");
        Console.WriteLine($"Resampled SamplingRate: {resampledSignal.SamplingRate}");
        Console.WriteLine($"Resampled Length: {resampledSignal.Length}");
    }
}
```
## SignalExtension 类

Vorcyc.Mathematics.Experimental.Signals.SignalExtension 是一个用于扩展信号类功能的静态类，提供了生成各种波形的方法。

### 方法

#### 1. GenerateWave
- `public static void GenerateWave(this ITimeDomainSignal signal, WaveShape shape, float frequency, Behaviour behaviour = Behaviour.Replace)`
  - 生成指定波形，并根据行为对信号进行处理。
  - 参数:
    - `signal`: 表示信号的对象。
    - `shape`: 波形类型。
    - `frequency`: 波形的频率。
    - `behaviour`: 处理行为，默认为 `Behaviour.Replace`。

#### 2. GenerateSineWave
- `internal static void GenerateSineWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)`
  - 生成正弦波，并对每个生成的值执行指定操作。
  - 参数:
    - `signal`: 表示信号的对象。
    - `frequency`: 正弦波的频率。
    - `action`: 对每个生成的值执行的操作。

#### 3. GenerateCosineWave
- `internal static void GenerateCosineWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)`
  - 生成余弦波，并对每个生成的值执行指定操作。
  - 参数:
    - `signal`: 表示信号的对象。
    - `frequency`: 余弦波的频率。
    - `action`: 对每个生成的值执行的操作。

#### 4. GenerateSquareWave
- `internal static void GenerateSquareWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)`
  - 生成方波，并对每个生成的值执行指定操作。
  - 参数:
    - `signal`: 表示信号的对象。
    - `frequency`: 方波的频率。
    - `action`: 对每个生成的值执行的操作。

#### 5. GenerateSawtoothWave
- `internal static void GenerateSawtoothWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)`
  - 生成锯齿波，并对每个生成的值执行指定操作。
  - 参数:
    - `signal`: 表示信号的对象。
    - `frequency`: 锯齿波的频率。
    - `action`: 对每个生成的值执行的操作。

#### 6. GenerateTriangleWave
- `internal static void GenerateTriangleWave(ITimeDomainSignal signal, float frequency, Action<int, float> action)`
  - 生成三角波，并对每个生成的值执行指定操作。
  - 参数:
    - `signal`: 表示信号的对象。
    - `frequency`: 三角波的频率。
    - `action`: 对每个生成的值执行的操作。

#### 7. GenerateWhiteNoise
- `internal static void GenerateWhiteNoise(ITimeDomainSignal signal, Action<int, float> action)`
  - 生成白噪声，并对每个生成的值执行指定操作。
  - 参数:
    - `signal`: 表示信号的对象。
    - `action`: 对每个生成的值执行的操作。

#### 8. GeneratePinkNoise
- `internal static void GeneratePinkNoise(ITimeDomainSignal signal, Action<int, float> action)`
  - 生成粉红噪声，并对每个生成的值执行指定操作。
  - 参数:
    - `signal`: 表示信号的对象。
    - `action`: 对每个生成的值执行的操作。

### 代码示例
以下是一个使用 SignalExtension 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.Experimental.Signals;
public class SignalExtensionExample
{
    public static void Main()
    {
        // 定义时域信号
        float[] samples = new float[1000];
        var samplingRate = 1000;
        var signal = new Signal(samples.Length, samplingRate);

        // 生成正弦波并替换现有信号
        signal.GenerateWave(WaveShape.Sine, 50, Behaviour.Replace);

        // 输出生成的信号样本
        Console.WriteLine("Generated Sine Wave:");
        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }

        // 生成方波并与现有信号逐元素相加
        signal.GenerateWave(WaveShape.Square, 50, Behaviour.ElementWiseAdd);

        // 输出生成的信号样本
        Console.WriteLine("Generated Square Wave (ElementWiseAdd):");
        foreach (var sample in signal.Samples)
        {
            Console.WriteLine(sample);
        }
    }
}
```
## SignalSegment 类

Vorcyc.Mathematics.Experimental.Signals.SignalSegment 是一个用于表示信号段的类，实现了 ITimeDomainSignal 接口。

### 属性

#### 1. Start
- `public int Start { get; }`
  - 获取信号段的起始位置。

#### 2. StartTime
- `public TimeSpan StartTime { get; }`
  - 获取当前信号段的起始时间。

#### 3. Length
- `public int Length { get; }`
  - 获取信号段的长度。

#### 4. Duration
- `public TimeSpan Duration { get; }`
  - 获取信号段的持续时间。

#### 5. Samples
- `public Span<float> Samples { get; }`
  - 获取信号段的采样数据。

#### 6. SamplingRate
- `public float SamplingRate { get; }`
  - 获取信号的采样率。

#### 7. Amplitude
- `public float Amplitude { get; }`
  - 获取信号段的振幅。

#### 8. Period
- `public float Period { get; }`
  - 获取信号的周期。

#### 9. Power
- `public float Power { get; }`
  - 获取信号段的功率。

#### 10. Energy
- `public float Energy { get; }`
  - 获取信号段的能量。

### 方法

#### 1. TransformToFrequencyDomain
- `public FrequencyDomain TransformToFrequencyDomain(WindowType? window = null)`
  - 将信号段转换为频域信号。
  - 参数:
    - `window`: 窗口类型，可选参数。
  - 返回值: 频域信号对象。

#### 2. Resample
- `public Signal Resample(int destnationSamplingRate, FirFilter? filter = null, int order = 15)`
  - 重采样并返回新的信号。
  - 参数:
    - `destnationSamplingRate`: 目标采样率。
    - `filter`: 可选的 FIR 滤波器。
    - `order`: 滤波器阶数，默认为 15。
  - 返回值: 重采样后的 `Signal` 对象。

#### 3. Decouple
- `public Signal Decouple()`
  - 从所在信号中脱离成为单独的信号。
  - 返回值: 脱离后的 `Signal` 对象。

### 运算符

#### 1. 加法运算符
- `public static Signal operator +(SignalSegment left, float right)`
  - 将信号段与一个浮点数相加。
  - 参数:
    - `left`: 信号段。
    - `right`: 浮点数。
  - 返回值: 相加后的新信号。

- `public static Signal? operator +(SignalSegment left, SignalSegment right)`
  - 将两个信号段相加。
  - 参数:
    - `left`: 左侧信号段。
    - `right`: 右侧信号段。
  - 返回值: 相加后的新信号，如果长度或采样率不匹配则返回 null。

#### 2. 减法运算符
- `public static Signal operator -(SignalSegment left, float right)`
  - 将信号段与一个浮点数相减。
  - 参数:
    - `left`: 信号段。
    - `right`: 浮点数。
  - 返回值: 相减后的新信号。

- `public static Signal? operator -(SignalSegment left, SignalSegment right)`
  - 将两个信号段相减。
  - 参数:
    - `left`: 左侧信号段。
    - `right`: 右侧信号段。
  - 返回值: 相减后的新信号，如果长度或采样率不匹配则返回 null。

#### 3. 乘法运算符
- `public static Signal operator *(SignalSegment left, float right)`
  - 将信号段与一个浮点数相乘。
  - 参数:
    - `left`: 信号段。
    - `right`: 浮点数。
  - 返回值: 相乘后的新信号。

- `public static Signal? operator *(SignalSegment left, SignalSegment right)`
  - 将两个信号段相乘。
  - 参数:
    - `left`: 左侧信号段。
    - `right`: 右侧信号段。
  - 返回值: 相乘后的新信号，如果长度或采样率不匹配则返回 null。

#### 4. 除法运算符
- `public static Signal operator /(SignalSegment left, float right)`
  - 将信号段与一个浮点数相除。
  - 参数:
    - `left`: 信号段。
    - `right`: 浮点数。
  - 返回值: 相除后的新信号。

### 代码示例
以下是一个使用 SignalSegment 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.Experimental.Signals;
public class SignalSegmentExample
{
    public static void Main()
    {
        // 定义时域信号
        float[] samples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f, 0.85f, 1.0f };
        var samplingRate = 1000;
        var signal = new Signal(samples.Length, samplingRate);

        // 创建信号段
        var segment = new SignalSegment(signal, 2, 5);

        // 输出信号段的属性
        Console.WriteLine($"Start: {segment.Start}");
        Console.WriteLine($"StartTime: {segment.StartTime}");
        Console.WriteLine($"Length: {segment.Length}");
        Console.WriteLine($"Duration: {segment.Duration}");
        Console.WriteLine($"Amplitude: {segment.Amplitude}");
        Console.WriteLine($"Period: {segment.Period}");
        Console.WriteLine($"Power: {segment.Power}");
        Console.WriteLine($"Energy: {segment.Energy}");

        // 将信号段转换为频域信号
        var frequencyDomain = segment.TransformToFrequencyDomain();

        // 输出频域信号的属性
        Console.WriteLine($"Offset: {frequencyDomain.Offset}");
        Console.WriteLine($"ActualLength: {frequencyDomain.ActualLength}");
        Console.WriteLine($"TransformLength: {frequencyDomain.TransformLength}");
        Console.WriteLine($"Resolution: {frequencyDomain.Resolution}");
        Console.WriteLine($"WindowApplied: {frequencyDomain.WindowApplied}");

        // 重采样信号段
        var resampledSignal = segment.Resample(2000);

        // 输出重采样后的信号属性
        Console.WriteLine($"Resampled Duration: {resampledSignal.Duration}");
        Console.WriteLine($"Resampled SamplingRate: {resampledSignal.SamplingRate}");
        Console.WriteLine($"Resampled Length: {resampledSignal.Length}");
    }
}
```













