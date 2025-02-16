当前位置 : [根目录](README.md)/[统计模块](Module_Statistics.md)/[极值查找器](Module_Statistics_ExtremeValueFinder.md)

# 统计模块 - Statistics Module
## 极值查找器 - ExtremeValueFinder

`Vorcyc.Mathematics.Statistics.ExtremeValueFinder` 是一个提供查找序列中最大值和最小值的方法的类。该类提供了优化的算法以同时获得序列中的最大值和最小值。

> 以下方法均位于类 ：Vorcyc.Mathematics.Statistics.ExtremeValueFinder

---

:ledger:目录  
- :bookmark: [FindExtremeValue(float[], int, int) 方法](#1-findextremevaluefloat-int-int-方法)  
- :bookmark: [FindExtremeValue&lt;T> (T[], int, int) 方法](#2-findextremevaluet-tint-int-方法)  
- :bookmark: [FindExtremeValue(float[]) 方法](#3-findextremevaluefloat-方法)  
- :bookmark: [FindExtremeValue&lt;T>T[]) 方法](#4-findextremevaluett-方法)  
- :bookmark: [FindExtremeValue(ArraySegment&lt;float>) 方法](#5-findextremevaluearraysegmentfloat-方法)  
- :bookmark: [FindExtremeValue&lt;T> (ArraySegment&lt;T>) 方法](#6-findextremevaluetarraysegmentt-方法)  
- :bookmark: [FindExtremeValue(Span&lt;float>) 方法](#7-findextremevaluespanfloat-方法)    


---

## Vorcyc.Mathematics.Statistics.ExtremeValueFinder 类

Vorcyc.Mathematics.Statistics.ExtremeValueFinder 是一个提供查找序列中最大值和最小值的方法的类。

### 方法

#### 1. FindExtremeValue(float[], int, int) 方法
- `public static (float max, float min) FindExtremeValue(this float[] array, int start, int length)`
  - 查找浮点数组指定范围内的最大值和最小值。
  - 参数:
    - `array`: 浮点数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 包含指定范围内最大值和最小值的元组。

#### 2. FindExtremeValue&lt;T>(T[], int, int) 方法
- `public static (T max, T min) FindExtremeValue<T>(this T[] array, int start, int length) where T : INumber<T>`
  - 查找数组指定范围内的最大值和最小值。
  - 参数:
    - `array`: 数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 包含指定范围内最大值和最小值的元组。

#### 3. FindExtremeValue(float[]) 方法
- `public static (float max, float min) FindExtremeValue(this float[] array)`
  - 查找浮点数组中的最大值和最小值。
  - 参数:
    - `array`: 浮点数组。
  - 返回值: 包含数组中最大值和最小值的元组。

#### 4. FindExtremeValue&lt;T>(T[]) 方法
- `public static (T max, T min) FindExtremeValue<T>(this T[] array) where T : INumber<T>`
  - 查找数组中的最大值和最小值。
  - 参数:
    - `array`: 数组。
  - 返回值: 包含数组中最大值和最小值的元组。

#### 5. FindExtremeValue(ArraySegment&lt;float>) 方法
- `public static (float max, float min) FindExtremeValue(this ArraySegment<float> arraySegment)`
  - 查找浮点数组段中的最大值和最小值。
  - 参数:
    - `arraySegment`: 浮点数组段。
  - 返回值: 包含数组段中最大值和最小值的元组。

#### 6. FindExtremeValue&lt;T>(ArraySegment&lt;T>) 方法
- `public static (T max, T min) FindExtremeValue<T>(this ArraySegment<T> arraySegment) where T : INumber<T>`
  - 查找数组段中的最大值和最小值。
  - 参数:
    - `arraySegment`: 数组段。
  - 返回值: 包含数组段中最大值和最小值的元组。

#### 7. FindExtremeValue(Span&lt;float>) 方法
- `public static (float max, float min) FindExtremeValue(this Span<float> span)`
  - 查找浮点 Span 中的最大值和最小值。
  - 参数:
    - `span`: 浮点 Span。
  - 返回值: 包含 Span 中最大值和最小值的元组。


### 代码示例
以下是一个使用 ExtremeValueFinder 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics.Statistics;

public class ExtremeValueFinderExample
{
    public static void Main()
    {
        // 定义一个浮点数组
        float[] floatArray = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 查找数组中的最大值和最小值
        var (maxFloat, minFloat) = floatArray.FindExtremeValue();
        Console.WriteLine($"Max value in float array: {maxFloat}, Min value in float array: {minFloat}");

        // 查找数组指定范围内的最大值和最小值
        var (maxFloatRange, minFloatRange) = floatArray.FindExtremeValue(1, 3);
        Console.WriteLine($"Max value in float array range: {maxFloatRange}, Min value in float array range: {minFloatRange}");

        // 定义一个整数数组
        int[] intArray = { 1, 2, 3, 4, 5 };

        // 查找数组中的最大值和最小值
        var (maxInt, minInt) = intArray.FindExtremeValue();
        Console.WriteLine($"Max value in int array: {maxInt}, Min value in int array: {minInt}");

        // 查找数组指定范围内的最大值和最小值
        var (maxIntRange, minIntRange) = intArray.FindExtremeValue(1, 3);
        Console.WriteLine($"Max value in int array range: {maxIntRange}, Min value in int array range: {minIntRange}");

        // 定义一个浮点数组段
        ArraySegment<float> floatSegment = new ArraySegment<float>(floatArray, 1, 3);

        // 查找数组段中的最大值和最小值
        var (maxFloatSegment, minFloatSegment) = floatSegment.FindExtremeValue();
        Console.WriteLine($"Max value in float array segment: {maxFloatSegment}, Min value in float array segment: {minFloatSegment}");

        // 定义一个整数数组段
        ArraySegment<int> intSegment = new ArraySegment<int>(intArray, 1, 3);

        // 查找数组段中的最大值和最小值
        var (maxIntSegment, minIntSegment) = intSegment.FindExtremeValue();
        Console.WriteLine($"Max value in int array segment: {maxIntSegment}, Min value in int array segment: {minIntSegment}");

        // 定义一个浮点 Span
        Span<float> floatSpan = new Span<float>(floatArray);

        // 查找 Span 中的最大值和最小值
        var (maxFloatSpan, minFloatSpan) = floatSpan.FindExtremeValue();
        Console.WriteLine($"Max value in float span: {maxFloatSpan}, Min value in float span: {minFloatSpan}");
    }
}
```




