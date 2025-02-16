当前位置 : [根目录](README.md)/[统计模块](Module_Statistics.md)/[INumberExtension](Module_Statistics_INumberExtension.md)

# 统计模块 - Statistics Module
## INumberExtension

`Vorcyc.Mathematics.Statistics.INumberExtension` 提供数值操作的扩展方法，支持SIMD优化，包括在跨度中查找最大值和最小值。

> 以下方法均位于类 ：Vorcyc.Mathematics.Statistics.INumberExtension

---

:ledger:目录  
- :bookmark: [Max (Span&lt;T>) 方法](#1-max-spant-方法)  
- :bookmark: [Max (T[]) 方法](#2-max-t-方法)  
- :bookmark: [Max (T[], int, int) 方法](#3-max-t-int-int-方法)  
- :bookmark: [LocateMax (Span&lt;T>) 方法](#4-locatemax-spant-方法)  
- :bookmark: [LocateMax (T[]) 方法](#5-locatemax-t-方法)  
- :bookmark: [LocateMax (T[], int, int) 方法](#6-locatemax-t-int-int-方法)  
- :bookmark: [Min (Span&lt;T>) 方法](#7-min-spant-方法)  
- :bookmark: [Min (T[]) 方法](#8-min-t-方法)  
- :bookmark: [Min (T[], int, int) 方法](#9-min-t-int-int-方法)  
- :bookmark: [LocateMin (Span&lt;T>) 方法](#10-locatemin-spant-方法)  
- :bookmark: [LocateMin (T[]) 方法](#11-locatemin-t-方法)  
- :bookmark: [LocateMin (T[], int, int) 方法](#12-locatemin-t-int-int-方法)  

---

## Vorcyc.Mathematics.Statistics.INumberExtension 类


### 方法

#### 1. Max (Span&lt;T>) 方法
- `public static T Max<T>(this Span<T> span) where T : struct, INumber<T>`
  - 获取 <see cref="Span{T}"/> 中的最大值。
  - 参数:
    - `span`: 值的 Span。
  - 返回值: Span 中的最大值。

#### 2. Max (T[]) 方法
- `public static T Max<T>(this T[] values) where T : struct, INumber<T>`
  - 查找数组中的最大值。
  - 参数:
    - `values`: 值的数组。
  - 返回值: 数组中的最大值。

#### 3. Max (T[], int, int) 方法
- `public static T Max<T>(this T[] values, int start, int length) where T : struct, INumber<T>`
  - 查找数组指定范围内的最大值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 数组指定范围内的最大值。

#### 4. LocateMax (Span&lt;T>) 方法
- `public static (int index, T value) LocateMax<T>(this Span<T> span) where T : INumber<T>`
  - 获取 <see cref="Span{T}"/> 中最大值及其索引。
  - 参数:
    - `span`: 值的 Span。
  - 返回值: 包含最大值及其索引的元组。

#### 5. LocateMax (T[]) 方法
- `public static (int index, T value) LocateMax<T>(this T[] values) where T : INumber<T>`
  - 获取数组中最大值及其索引。
  - 参数:
    - `values`: 值的数组。
  - 返回值: 包含最大值及其索引的元组。

#### 6. LocateMax (T[], int, int) 方法
- `public static (int index, T value) LocateMax<T>(this T[] values, int start, int length) where T : INumber<T>`
  - 获取数组指定范围内最大值及其索引。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 包含最大值及其索引的元组。

#### 7. Min (Span&lt;T>) 方法
- `public static T Min<T>(this Span<T> span) where T : struct, INumber<T>`
  - 获取 <see cref="Span{T}"/> 中的最小值。
  - 参数:
    - `span`: 值的 Span。
  - 返回值: Span 中的最小值。

#### 8. Min (T[]) 方法
- `public static T Min<T>(this T[] values) where T : struct, INumber<T>`
  - 查找数组中的最小值。
  - 参数:
    - `values`: 值的数组。
  - 返回值: 数组中的最小值。

#### 9. Min (T[], int, int) 方法
- `public static T Min<T>(this T[] values, int start, int length) where T : struct, INumber<T>`
  - 查找数组指定范围内的最小值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 数组指定范围内的最小值。

#### 10. LocateMin (Span&lt;T>) 方法
- `public static (int index, T value) LocateMin<T>(this Span<T> span) where T : INumber<T>`
  - 获取 <see cref="Span{T}"/> 中最小值及其索引。
  - 参数:
    - `span`: 值的 Span。
  - 返回值: 包含最小值及其索引的元组。

#### 11. LocateMin (T[]) 方法
- `public static (int index, T value) LocateMin<T>(this T[] values) where T : INumber<T>`
  - 获取数组中最小值及其索引。
  - 参数:
    - `values`: 值的数组。
  - 返回值: 包含最小值及其索引的元组。

#### 12. LocateMin (T[], int, int) 方法
- `public static (int index, T value) LocateMin<T>(this T[] values, int start, int length) where T : INumber<T>`
  - 获取数组指定范围内最小值及其索引。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 包含最小值及其索引的元组。

### 代码示例
以下是一个使用 INumberExtension 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using System.Linq;
using Vorcyc.Mathematics.Statistics;

public class INumberExtensionExample
{
    public static void Main()
    {
        // 定义一个浮点数组
        float[] floatArray = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // 获取数组中的最大值和最小值
        float maxFloat = floatArray.Max();
        float minFloat = floatArray.Min();
        Console.WriteLine($"Max value in float array: {maxFloat}");
        Console.WriteLine($"Min value in float array: {minFloat}");

        // 获取数组中最大值和最小值的索引和值
        var (maxFloatIndex, maxFloatValue) = floatArray.LocateMax();
        var (minFloatIndex, minFloatValue) = floatArray.LocateMin();
        Console.WriteLine($"Max value in float array: {maxFloatValue} at index {maxFloatIndex}");
        Console.WriteLine($"Min value in float array: {minFloatValue} at index {minFloatIndex}");

        // 获取数组指定范围内的最大值和最小值
        float maxFloatRange = floatArray.Max(1, 3);
        float minFloatRange = floatArray.Min(1, 3);
        Console.WriteLine($"Max value in float array range: {maxFloatRange}");
        Console.WriteLine($"Min value in float array range: {minFloatRange}");

        // 获取数组指定范围内最大值和最小值的索引和值
        var (maxFloatRangeIndex, maxFloatRangeValue) = floatArray.LocateMax(1, 3);
        var (minFloatRangeIndex, minFloatRangeValue) = floatArray.LocateMin(1, 3);
        Console.WriteLine($"Max value in float array range: {maxFloatRangeValue} at index {maxFloatRangeIndex}");
        Console.WriteLine($"Min value in float array range: {minFloatRangeValue} at index {minFloatRangeIndex}");

        // 定义一个整数数组
        int[] intArray = { 1, 2, 3, 4, 5 };

        // 获取数组中的最大值和最小值
        int maxInt = intArray.Max();
        int minInt = intArray.Min();
        Console.WriteLine($"Max value in int array: {maxInt}");
        Console.WriteLine($"Min value in int array: {minInt}");

        // 获取数组中最大值和最小值的索引和值
        var (maxIntIndex, maxIntValue) = intArray.LocateMax();
        var (minIntIndex, minIntValue) = intArray.LocateMin();
        Console.WriteLine($"Max value in int array: {maxIntValue} at index {maxIntIndex}");
        Console.WriteLine($"Min value in int array: {minIntValue} at index {minIntIndex}");

        // 获取数组指定范围内的最大值和最小值
        int maxIntRange = intArray.Max(1, 3);
        int minIntRange = intArray.Min(1, 3);
        Console.WriteLine($"Max value in int array range: {maxIntRange}");
        Console.WriteLine($"Min value in int array range: {minIntRange}");

        // 获取数组指定范围内最大值和最小值的索引和值
        var (maxIntRangeIndex, maxIntRangeValue) = intArray.LocateMax(1, 3);
        var (minIntRangeIndex, minIntRangeValue) = intArray.LocateMin(1, 3);
        Console.WriteLine($"Max value in int array range: {maxIntRangeValue} at index {maxIntRangeIndex}");
        Console.WriteLine($"Min value in int array range: {minIntRangeValue} at index {minIntRangeIndex}");
    }
}
```





