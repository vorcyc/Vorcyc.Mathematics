当前位置 : [根目录](README.md)/[信号处理模块](Module_SignalProcessing.md)/[变换操作](Module_SignalProcessing_Transforms.md)

# 信号处理模块 - Signal Processing Module
## 变换操作 - Transforms

`Vorcyc.Mathematics.SignalProcessing.Transforms` 命名空间包含多种变换操作类，包括 `Dct1`、`Dct2`、`Dct3`、`Dct4`、`FastDct2`、`FastDct3`、`FastDct4`、`FastMdct` 和 `Mdct`。这些类提供了离散余弦变换（DCT）和修正离散余弦变换（MDCT）的实现，支持不同类型的 DCT 变换（I、II、III、IV）以及基于 FFT 的快速实现。每个类都提供了直接变换、归一化变换、逆变换和归一化逆变换的方法，适用于各种信号处理需求。


> 以下类型均位于命名空间 ：Vorcyc.Mathematics.SignalProcessing.Transforms

---

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Transforms.Dct1 类](#vorcycmathematicssignalprocessingtransformsdct1-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Transforms.Dct2 类](#vorcycmathematicssignalprocessingtransformsdct2-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Transforms.Dct3 类](#vorcycmathematicssignalprocessingtransformsdct3-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Transforms.Dct4 类](#vorcycmathematicssignalprocessingtransformsdct4-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Transforms.FastDct2 类](#vorcycmathematicssignalprocessingtransformsfastdct2-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Transforms.FastDct3 类](#vorcycmathematicssignalprocessingtransformsfastdct3-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Transforms.FastDct4 类](#vorcycmathematicssignalprocessingtransformsfastdct4-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Transforms.FastMdct 类](#vorcycmathematicssignalprocessingtransformsfastmdct-类)  
- :bookmark: [Vorcyc.Mathematics.SignalProcessing.Transforms.Mdct 类](#vorcycmathematicssignalprocessingtransformsmdct-类)  

---


> 以下类型均位于 Vorcyc.Mathematics.SignalProcessing.Transforms 命名空间。


  ## Vorcyc.Mathematics.SignalProcessing.Transforms.Dct1 类

`Vorcyc.Mathematics.SignalProcessing.Transforms.Dct1` 表示离散余弦变换的类型-I (DCT-I)。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取 DCT-I 的大小。

### 方法

#### 1. Dct1 构造函数
- `public Dct1(int dctSize)`
  - 构造给定 `dctSize` 的 `Dct1` 并预计算 DCT 矩阵。
  - 参数:
    - `dctSize`: DCT-I 的大小。

#### 2. Direct
- `public void Direct(float[] input, float[] output)`
  - 执行 DCT-I。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 3. DirectNorm
- `public void DirectNorm(float[] input, float[] output)`
  - 执行归一化的 DCT-I。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 4. Inverse
- `public void Inverse(float[] input, float[] output)`
  - 执行逆 DCT-I。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 5. InverseNorm
- `public void InverseNorm(float[] input, float[] output)`
  - 执行归一化的逆 DCT-I。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

### 代码示例
以下是一个使用 Dct1 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

public class Dct1Example
{
    public static void Main()
    {
        // 定义 DCT 大小
        int dctSize = 8;

        // 创建 Dct1 实例
        var dct = new Dct1(dctSize);

        // 定义输入数据
        float[] input = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };
        float[] output = new float[dctSize];

        // 执行 DCT-I
        dct.Direct(input, output);
        Console.WriteLine("DCT-I Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的 DCT-I
        dct.DirectNorm(input, output);
        Console.WriteLine("Normalized DCT-I Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行逆 DCT-I
        dct.Inverse(output, input);
        Console.WriteLine("Inverse DCT-I Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的逆 DCT-I
        dct.InverseNorm(output, input);
        Console.WriteLine("Normalized Inverse DCT-I Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Transforms.Dct2 类

`Vorcyc.Mathematics.SignalProcessing.Transforms.Dct2` 表示离散余弦变换的类型-II (DCT-II)。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取 DCT-II 的大小。

### 方法

#### 1. Dct2 构造函数
- `public Dct2(int dctSize)`
  - 构造给定 `dctSize` 的 `Dct2` 并预计算 DCT 矩阵。
  - 参数:
    - `dctSize`: DCT-II 的大小。

#### 2. Direct
- `public void Direct(float[] input, float[] output)`
  - 执行 DCT-II。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 3. DirectNorm
- `public void DirectNorm(float[] input, float[] output)`
  - 执行归一化的 DCT-II。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 4. Inverse
- `public void Inverse(float[] input, float[] output)`
  - 执行逆 DCT-II。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 5. InverseNorm
- `public void InverseNorm(float[] input, float[] output)`
  - 执行归一化的逆 DCT-II。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

### 代码示例
以下是一个使用 Dct2 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

public class Dct2Example
{
    public static void Main()
    {
        // 定义 DCT 大小
        int dctSize = 8;

        // 创建 Dct2 实例
        var dct = new Dct2(dctSize);

        // 定义输入数据
        float[] input = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };
        float[] output = new float[dctSize];

        // 执行 DCT-II
        dct.Direct(input, output);
        Console.WriteLine("DCT-II Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的 DCT-II
        dct.DirectNorm(input, output);
        Console.WriteLine("Normalized DCT-II Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行逆 DCT-II
        dct.Inverse(output, input);
        Console.WriteLine("Inverse DCT-II Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的逆 DCT-II
        dct.InverseNorm(output, input);
        Console.WriteLine("Normalized Inverse DCT-II Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Transforms.Dct3 类

`Vorcyc.Mathematics.SignalProcessing.Transforms.Dct3` 表示离散余弦变换的类型-III (DCT-III)。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取 DCT-III 的大小。

### 方法

#### 1. Dct3 构造函数
- `public Dct3(int dctSize)`
  - 构造给定 `dctSize` 的 `Dct3` 并预计算 DCT 矩阵。
  - 参数:
    - `dctSize`: DCT-III 的大小。

#### 2. Direct
- `public void Direct(float[] input, float[] output)`
  - 执行 DCT-III。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 3. DirectNorm
- `public void DirectNorm(float[] input, float[] output)`
  - 执行归一化的 DCT-III。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 4. Inverse
- `public void Inverse(float[] input, float[] output)`
  - 执行逆 DCT-III。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 5. InverseNorm
- `public void InverseNorm(float[] input, float[] output)`
  - 执行归一化的逆 DCT-III。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

### 代码示例
以下是一个使用 Dct3 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

public class Dct3Example
{
    public static void Main()
    {
        // 定义 DCT 大小
        int dctSize = 8;

        // 创建 Dct3 实例
        var dct = new Dct3(dctSize);

        // 定义输入数据
        float[] input = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };
        float[] output = new float[dctSize];

        // 执行 DCT-III
        dct.Direct(input, output);
        Console.WriteLine("DCT-III Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的 DCT-III
        dct.DirectNorm(input, output);
        Console.WriteLine("Normalized DCT-III Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行逆 DCT-III
        dct.Inverse(output, input);
        Console.WriteLine("Inverse DCT-III Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的逆 DCT-III
        dct.InverseNorm(output, input);
        Console.WriteLine("Normalized Inverse DCT-III Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Transforms.Dct4 类

`Vorcyc.Mathematics.SignalProcessing.Transforms.Dct4` 表示离散余弦变换的类型-IV (DCT-IV)。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取 DCT-IV 的大小。

### 方法

#### 1. Dct4 构造函数
- `public Dct4(int dctSize)`
  - 构造给定 `dctSize` 的 `Dct4` 并预计算 DCT 矩阵。
  - 参数:
    - `dctSize`: DCT-IV 的大小。

#### 2. Direct
- `public void Direct(float[] input, float[] output)`
  - 执行 DCT-IV。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 3. DirectNorm
- `public void DirectNorm(float[] input, float[] output)`
  - 执行归一化的 DCT-IV。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 4. Inverse
- `public void Inverse(float[] input, float[] output)`
  - 执行逆 DCT-IV。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 5. InverseNorm
- `public void InverseNorm(float[] input, float[] output)`
  - 执行归一化的逆 DCT-IV。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

### 代码示例
以下是一个使用 Dct4 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

public class Dct4Example
{
    public static void Main()
    {
        // 定义 DCT 大小
        int dctSize = 8;

        // 创建 Dct4 实例
        var dct = new Dct4(dctSize);

        // 定义输入数据
        float[] input = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };
        float[] output = new float[dctSize];

        // 执行 DCT-IV
        dct.Direct(input, output);
        Console.WriteLine("DCT-IV Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的 DCT-IV
        dct.DirectNorm(input, output);
        Console.WriteLine("Normalized DCT-IV Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行逆 DCT-IV
        dct.Inverse(output, input);
        Console.WriteLine("Inverse DCT-IV Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的逆 DCT-IV
        dct.InverseNorm(output, input);
        Console.WriteLine("Normalized Inverse DCT-IV Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Transforms.FastDct2 类

`Vorcyc.Mathematics.SignalProcessing.Transforms.FastDct2` 表示离散余弦变换的类型-II (DCT-II)。这种基于 FFT 的 DCT-II 实现对于较大的 DCT 尺寸更快。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取 DCT-II 的大小。

### 方法

#### 1. FastDct2 构造函数
- `public FastDct2(int dctSize)`
  - 构造给定 `dctSize` 的 `FastDct2`。
  - 参数:
    - `dctSize`: DCT-II 的大小。

#### 2. Direct
- `public void Direct(float[] input, float[] output)`
  - 执行 DCT-II。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 3. DirectNorm
- `public void DirectNorm(float[] input, float[] output)`
  - 执行归一化的 DCT-II。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 4. Inverse
- `public void Inverse(float[] input, float[] output)`
  - 执行逆 DCT-II。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 5. InverseNorm
- `public void InverseNorm(float[] input, float[] output)`
  - 执行归一化的逆 DCT-II。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

### 代码示例
以下是一个使用 FastDct2 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

public class FastDct2Example
{
    public static void Main()
    {
        // 定义 DCT 大小
        int dctSize = 8;

        // 创建 FastDct2 实例
        var dct = new FastDct2(dctSize);

        // 定义输入数据
        float[] input = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };
        float[] output = new float[dctSize];

        // 执行 DCT-II
        dct.Direct(input, output);
        Console.WriteLine("DCT-II Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的 DCT-II
        dct.DirectNorm(input, output);
        Console.WriteLine("Normalized DCT-II Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行逆 DCT-II
        dct.Inverse(output, input);
        Console.WriteLine("Inverse DCT-II Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的逆 DCT-II
        dct.InverseNorm(output, input);
        Console.WriteLine("Normalized Inverse DCT-II Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }
    }
}

```


## Vorcyc.Mathematics.SignalProcessing.Transforms.FastDct3 类

`Vorcyc.Mathematics.SignalProcessing.Transforms.FastDct3` 表示离散余弦变换的类型-III (DCT-III)。这种基于 FFT 的 DCT-III 实现对于较大的 DCT 尺寸更快。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取 DCT-III 的大小。

### 方法

#### 1. FastDct3 构造函数
- `public FastDct3(int dctSize)`
  - 构造给定 `dctSize` 的 `FastDct3`。
  - 参数:
    - `dctSize`: DCT-III 的大小。

#### 2. Direct
- `public void Direct(float[] input, float[] output)`
  - 执行 DCT-III。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 3. DirectNorm
- `public void DirectNorm(float[] input, float[] output)`
  - 执行归一化的 DCT-III。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 4. Inverse
- `public void Inverse(float[] input, float[] output)`
  - 执行逆 DCT-III。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 5. InverseNorm
- `public void InverseNorm(float[] input, float[] output)`
  - 执行归一化的逆 DCT-III。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

### 代码示例
以下是一个使用 FastDct3 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

public class FastDct3Example
{
    public static void Main()
    {
        // 定义 DCT 大小
        int dctSize = 8;

        // 创建 FastDct3 实例
        var dct = new FastDct3(dctSize);

        // 定义输入数据
        float[] input = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };
        float[] output = new float[dctSize];

        // 执行 DCT-III
        dct.Direct(input, output);
        Console.WriteLine("DCT-III Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的 DCT-III
        dct.DirectNorm(input, output);
        Console.WriteLine("Normalized DCT-III Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行逆 DCT-III
        dct.Inverse(output, input);
        Console.WriteLine("Inverse DCT-III Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的逆 DCT-III
        dct.InverseNorm(output, input);
        Console.WriteLine("Normalized Inverse DCT-III Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Transforms.FastDct4 类

`Vorcyc.Mathematics.SignalProcessing.Transforms.FastDct4` 表示离散余弦变换的类型-IV (DCT-IV)。这种基于 FFT 的 DCT-IV 实现对于较大的 DCT 尺寸更快。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取 DCT-IV 的大小。

### 方法

#### 1. FastDct4 构造函数
- `public FastDct4(int dctSize)`
  - 构造给定 `dctSize` 的 `FastDct4`。
  - 参数:
    - `dctSize`: DCT-IV 的大小。

#### 2. Direct
- `public void Direct(float[] input, float[] output)`
  - 执行 DCT-IV。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 3. DirectNorm
- `public void DirectNorm(float[] input, float[] output)`
  - 执行归一化的 DCT-IV。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 4. Inverse
- `public void Inverse(float[] input, float[] output)`
  - 执行逆 DCT-IV。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 5. InverseNorm
- `public void InverseNorm(float[] input, float[] output)`
  - 执行归一化的逆 DCT-IV。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

### 代码示例
以下是一个使用 FastDct4 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

public class FastDct4Example
{
    public static void Main()
    {
        // 定义 DCT 大小
        int dctSize = 8;

        // 创建 FastDct4 实例
        var dct = new FastDct4(dctSize);

        // 定义输入数据
        float[] input = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };
        float[] output = new float[dctSize];

        // 执行 DCT-IV
        dct.Direct(input, output);
        Console.WriteLine("DCT-IV Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的 DCT-IV
        dct.DirectNorm(input, output);
        Console.WriteLine("Normalized DCT-IV Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行逆 DCT-IV
        dct.Inverse(output, input);
        Console.WriteLine("Inverse DCT-IV Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的逆 DCT-IV
        dct.InverseNorm(output, input);
        Console.WriteLine("Normalized Inverse DCT-IV Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }
    }
}
```


## Vorcyc.Mathematics.SignalProcessing.Transforms.FastMdct 类

`Vorcyc.Mathematics.SignalProcessing.Transforms.FastMdct` 表示修正离散余弦变换 (MDCT)。这种基于 FFT 的 MDCT 实现对于较大的 DCT 尺寸更快。

### 方法

#### 1. FastMdct 构造函数
- `public FastMdct(int dctSize)`
  - 构造给定 `dctSize` 的 `FastMdct`。
  - 参数:
    - `dctSize`: MDCT 的大小。

### 代码示例
以下是一个使用 FastMdct 类的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Transforms;

public class FastMdctExample
{
    public static void Main()
    {
        // 定义 MDCT 大小
        int dctSize = 8;

        // 创建 FastMdct 实例
        var mdct = new FastMdct(dctSize);

        // 定义输入数据
        float[] input = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };
        float[] output = new float[dctSize];

        // 执行 MDCT
        mdct.Direct(input, output);
        Console.WriteLine("MDCT Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行逆 MDCT
        mdct.Inverse(output, input);
        Console.WriteLine("Inverse MDCT Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }
    }
}
```

## Vorcyc.Mathematics.SignalProcessing.Transforms.Mdct 类

`Vorcyc.Mathematics.SignalProcessing.Transforms.Mdct` 表示修正离散余弦变换 (MDCT)。

### 属性

#### 1. Size
- `public int Size { get; }`
  - 获取 MDCT 的大小。

### 方法

#### 1. Mdct 构造函数
- `public Mdct(int dctSize, IDct dct = null)`
  - 构造给定 `dctSize` 的 `Mdct`。
  - 参数:
    - `dctSize`: MDCT 的大小。
    - `dct`: 内部 DCT 变换器（默认使用 `Dct4`）。

#### 2. Direct
- `public void Direct(float[] input, float[] output)`
  - 执行 MDCT。`input` 的长度必须等于 2 * `Mdct.Size`，`output` 的长度必须等于 `Mdct.Size`。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 3. DirectNorm
- `public void DirectNorm(float[] input, float[] output)`
  - 执行归一化的 MDCT。`input` 的长度必须等于 2 * `Mdct.Size`，`output` 的长度必须等于 `Mdct.Size`。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 4. Inverse
- `public void Inverse(float[] input, float[] output)`
  - 执行逆 MDCT。`input` 的长度必须等于 `Mdct.Size`，`output` 的长度必须等于 2 * `Mdct.Size`。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

#### 5. InverseNorm
- `public void InverseNorm(float[] input, float[] output)`
  - 执行归一化的逆 MDCT。`input` 的长度必须等于 `Mdct.Size`，`output` 的长度必须等于 2 * `Mdct.Size`。
  - 参数:
    - `input`: 输入数据。
    - `output`: 输出数据。

### 代码示例
以下是一个使用 Mdct 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.SignalProcessing.Transforms;
public class MdctExample
{
    public static void Main()
    {
        // 定义 MDCT 大小
        int dctSize = 8;

        // 创建 Mdct 实例
        var mdct = new Mdct(dctSize);

        // 定义输入数据
        float[] input = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f, 11.0f, 12.0f, 13.0f, 14.0f, 15.0f, 16.0f };
        float[] output = new float[dctSize];

        // 执行 MDCT
        mdct.Direct(input, output);
        Console.WriteLine("MDCT Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的 MDCT
        mdct.DirectNorm(input, output);
        Console.WriteLine("Normalized MDCT Output:");

        foreach (var value in output)
        {
            Console.WriteLine(value);
        }

        // 执行逆 MDCT
        mdct.Inverse(output, input);
        Console.WriteLine("Inverse MDCT Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }

        // 执行归一化的逆 MDCT
        mdct.InverseNorm(output, input);
        Console.WriteLine("Normalized Inverse MDCT Output:");

        foreach (var value in input)
        {
            Console.WriteLine(value);
        }
    }
}
```