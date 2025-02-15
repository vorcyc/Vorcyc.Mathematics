当前位置 : [根目录](README.md)/[统计模块](Module_Statistics.md)/[IComparableExtension](Module_Statistics_IComparableExtension.md)

# 统计模块 - Statistics Module
## IComparableExtension

`Vorcyc.Mathematics.Statistics.IComparableExtension` 是一个提供扩展方法的类，用于简化 IComparable&lt;T> 和 IComparable 的比较操作，并支持并行计算最大值和最小值。

> 以下方法均位于类 ：Vorcyc.Mathematics.Statistics.IComparableExtension

---

:ledger:目录  
- :bookmark: [LessThan 方法](#1-lessthan-方法)  
- :bookmark: [LessThanOrEqual 方法](#2-lessthanorequal-方法)  
- :bookmark: [GreaterThan 方法](#3-greaterthan-方法)  
- :bookmark: [GreaterThanOrEqual 方法](#4-greaterthanorequal-方法)  
- :bookmark: [Equal 方法](#5-equal-方法)  
- :bookmark: [NotEqual 方法](#6-notequal-方法)  
- :bookmark: [CompareMax 方法](#7-comparemax-方法)  
- :bookmark: [CompareMin 方法](#8-comparemin-方法)  
- :bookmark: [CompareMax (数组) 方法](#9-comparemax-数组-方法)  
- :bookmark: [CompareMax (数组范围) 方法](#10-comparemax-数组范围-方法)  
- :bookmark: [CompareMax (Span) 方法](#11-comparemax-span-方法)  
- :bookmark: [CompareMin (数组) 方法](#12-comparemin-数组-方法)  
- :bookmark: [CompareMin (数组范围) 方法](#13-comparemin-数组范围-方法)  
- :bookmark: [CompareMin (Span) 方法](#14-comparemin-span-方法)  
- :bookmark: [CompareMaxMin (数组) 方法](#15-comparemaxmin-数组-方法)  
- :bookmark: [CompareMaxMin (数组范围) 方法](#16-comparemaxmin-数组范围-方法)  
- :bookmark: [CompareMaxMin (Span) 方法](#17-comparemaxmin-span-方法)  
- :bookmark: [LocateMax (数组) 方法](#18-locatemax-数组-方法)  
- :bookmark: [LocateMax (数组范围) 方法](#19-locatemax-数组范围-方法)  
- :bookmark: [LocateMax (Span) 方法](#20-locatemax-span-方法)  
- :bookmark: [LocateMin (数组) 方法](#21-locatemin-数组-方法)  
- :bookmark: [LocateMin (数组范围) 方法](#22-locatemin-数组范围-方法)  
- :bookmark: [LocateMin (Span) 方法](#23-locatemin-span-方法)  
- :bookmark: [CompareMaxAsync 方法](#24-comparemaxasync-方法)  
- :bookmark: [CompareMaxAsync (数组范围) 方法](#25-comparemaxasync-数组范围-方法)  
- :bookmark: [LocateMaxAsync 方法](#26-locatemaxasync-方法)  
- :bookmark: [LocateMaxAsync (数组范围) 方法](#27-locatemaxasync-数组范围-方法)  
- :bookmark: [CompareMinAsync 方法](#28-compareminasync-方法)  
- :bookmark: [CompareMinAsync (数组范围) 方法](#29-compareminasync-数组范围-方法)  
- :bookmark: [LocateMinAsync 方法](#30-locateminasync-方法)  
- :bookmark: [LocateMinAsync (数组范围) 方法](#31-locateminasync-数组范围-方法)  

---

## Vorcyc.Mathematics.Statistics.IComparableExtension 类


### 方法

#### 1. LessThan
- `public static bool LessThan<T>(this T value1, T value2) where T : IComparable, IComparable<T>`
  - 确定第一个值是否小于第二个值。
  - 参数:
    - `value1`: 第一个值。
    - `value2`: 第二个值。
  - 返回值: 如果第一个值小于第二个值，则返回 `true`；否则返回 `false`。

#### 2. LessThanOrEqual
- `public static bool LessThanOrEqual<T>(this T value1, T value2) where T : IComparable, IComparable<T>`
  - 确定第一个值是否小于或等于第二个值。
  - 参数:
    - `value1`: 第一个值。
    - `value2`: 第二个值。
  - 返回值: 如果第一个值小于或等于第二个值，则返回 `true`；否则返回 `false`。

#### 3. GreaterThan
- `public static bool GreaterThan<T>(this T value1, T value2) where T : IComparable, IComparable<T>`
  - 确定第一个值是否大于第二个值。
  - 参数:
    - `value1`: 第一个值。
    - `value2`: 第二个值。
  - 返回值: 如果第一个值大于第二个值，则返回 `true`；否则返回 `false`。

#### 4. GreaterThanOrEqual
- `public static bool GreaterThanOrEqual<T>(this T value1, T value2) where T : IComparable, IComparable<T>`
  - 确定第一个值是否大于或等于第二个值。
  - 参数:
    - `value1`: 第一个值。
    - `value2`: 第二个值。
  - 返回值: 如果第一个值大于或等于第二个值，则返回 `true`；否则返回 `false`。

#### 5. Equal
- `public static bool Equal<T>(this T value1, T value2) where T : IComparable, IComparable<T>`
  - 确定第一个值是否等于第二个值。
  - 参数:
    - `value1`: 第一个值。
    - `value2`: 第二个值。
  - 返回值: 如果第一个值等于第二个值，则返回 `true`；否则返回 `false`。

#### 6. NotEqual
- `public static bool NotEqual<T>(this T value1, T value2) where T : IComparable, IComparable<T>`
  - 确定第一个值是否不等于第二个值。
  - 参数:
    - `value1`: 第一个值。
    - `value2`: 第二个值。
  - 返回值: 如果第一个值不等于第二个值，则返回 `true`；否则返回 `false`。

#### 7. CompareMax
- `public static T CompareMax<T>(this T value1, T value2) where T : IComparable, IComparable<T>`
  - 返回两个值中的最大值。
  - 参数:
    - `value1`: 第一个值。
    - `value2`: 第二个值。
  - 返回值: 两个值中的最大值。

#### 8. CompareMin
- `public static T CompareMin<T>(this T value1, T value2) where T : IComparable, IComparable<T>`
  - 返回两个值中的最小值。
  - 参数:
    - `value1`: 第一个值。
    - `value2`: 第二个值。
  - 返回值: 两个值中的最小值。

#### 9. CompareMax (数组)
- `public static T CompareMax<T>(this T[] values) where T : IComparable, IComparable<T>`
  - 返回数组中的最大值。
  - 参数:
    - `values`: 值的数组。
  - 返回值: 数组中的最大值。

#### 10. CompareMax (数组范围)
- `public static T CompareMax<T>(this T[] values, int start, int length) where T : IComparable, IComparable<T>`
  - 返回数组指定范围内的最大值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 数组指定范围内的最大值。

#### 11. CompareMax (Span)
- `public static T CompareMax<T>(this Span<T> span) where T : IComparable, IComparable<T>`
  - 返回 Span 中的最大值。
  - 参数:
    - `span`: 值的 Span。
  - 返回值: Span 中的最大值。

#### 12. CompareMin (数组)
- `public static T CompareMin<T>(this T[] values) where T : IComparable, IComparable<T>`
  - 返回数组中的最小值。
  - 参数:
    - `values`: 值的数组。
  - 返回值: 数组中的最小值。

#### 13. CompareMin (数组范围)
- `public static T CompareMin<T>(this T[] values, int start, int length) where T : IComparable, IComparable<T>`
  - 返回数组指定范围内的最小值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 数组指定范围内的最小值。

#### 14. CompareMin (Span)
- `public static T CompareMin<T>(this Span<T> span) where T : IComparable, IComparable<T>`
  - 返回 Span 中的最小值。
  - 参数:
    - `span`: 值的 Span。
  - 返回值: Span 中的最小值。

#### 15. CompareMaxMin (数组)
- `public static (T max, T min) CompareMaxMin<T>(this T[] values) where T : IComparable, IComparable<T>`
  - 返回数组中的最大值和最小值。
  - 参数:
    - `values`: 值的数组。
  - 返回值: 包含数组中最大值和最小值的元组。

#### 16. CompareMaxMin (数组范围)
- `public static (T max, T min) CompareMaxMin<T>(this T[] values, int start, int length) where T : IComparable, IComparable<T>`
  - 返回数组指定范围内的最大值和最小值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 包含数组指定范围内最大值和最小值的元组。

#### 17. CompareMaxMin (Span)
- `public static (T max, T min) CompareMaxMin<T>(this Span<T> span) where T : IComparable, IComparable<T>`
  - 返回 Span 中的最大值和最小值。
  - 参数:
    - `span`: 值的 Span。
  - 返回值: 包含 Span 中最大值和最小值的元组。

#### 18. LocateMax (数组)
- `public static (int index, T value) LocateMax<T>(T[] values) where T : IComparable, IComparable<T>`
  - 返回数组中最大元素的索引和值。
  - 参数:
    - `values`: 值的数组。
  - 返回值: 包含数组中最大元素的索引和值的元组。

#### 19. LocateMax (数组范围)
- `public static (int index, T value) LocateMax<T>(T[] values, int start, int length) where T : IComparable, IComparable<T>`
  - 返回数组指定范围内最大元素的索引和值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 包含数组指定范围内最大元素的索引和值的元组。

#### 20. LocateMax (Span)
- `public static (int index, T value) LocateMax<T>(Span<T> span) where T : IComparable, IComparable<T>`
  - 返回 Span 中最大元素的索引和值。
  - 参数:
    - `span`: 值的 Span。
  - 返回值: 包含 Span 中最大元素的索引和值的元组。

#### 21. LocateMin (数组)
- `public static (int index, T value) LocateMin<T>(T[] values) where T : IComparable, IComparable<T>`
  - 返回数组中最小元素的索引和值。
  - 参数:
    - `values`: 值的数组。
  - 返回值: 包含数组中最小元素的索引和值的元组。

#### 22. LocateMin (数组范围)
- `public static (int index, T value) LocateMin<T>(T[] values, int start, int length) where T : IComparable, IComparable<T>`
  - 返回数组指定范围内最小元素的索引和值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
  - 返回值: 包含数组指定范围内最小元素的索引和值的元组。

#### 23. LocateMin (Span)
- `public static (int index, T value) LocateMin<T>(Span<T> span) where T : IComparable, IComparable<T>`
  - 返回 Span 中最小元素的索引和值。
  - 参数:
    - `span`: 值的 Span。
  - 返回值: 包含 Span 中最小元素的索引和值的元组。

#### 24. CompareMaxAsync
- `public static Task<TValue> CompareMaxAsync<TValue>(this TValue[] values, int? numberOfWorkers = null, bool useTPL = false) where TValue : IComparable, IComparable<TValue>`
  - 返回并行序列中的最大值。
  - 参数:
    - `values`: 值的数组。
    - `numberOfWorkers`: 工作任务的数量。如果为 null，则由环境确定工作任务的数量。
    - `useTPL`: 如果为 true，则使用任务并行库 (TPL) 进行并行计算。
  - 返回值: 表示异步操作的任务。任务结果包含最大值。

#### 25. CompareMaxAsync (数组范围)
- `public static Task<TValue> CompareMaxAsync<TValue>(this TValue[] values, int start, int length, int? numberOfWorkers = null, bool useTPL = false) where TValue : IComparable, IComparable<TValue>`
  - 返回并行序列指定范围内的最大值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
    - `numberOfWorkers`: 工作任务的数量。如果为 null，则由环境确定工作任务的数量。
    - `useTPL`: 如果为 true，则使用任务并行库 (TPL) 进行并行计算。
  - 返回值: 表示异步操作的任务。任务结果包含最大值。

#### 26. LocateMaxAsync
- `public static Task<(int, TValue)> LocateMaxAsync<TValue>(this TValue[] values, int? numberOfWorkers = null, bool useTPL = false) where TValue : IComparable, IComparable<TValue>`
  - 返回并行序列中最大元素的索引和值。
  - 参数:
    - `values`: 值的数组。
    - `numberOfWorkers`: 工作任务的数量。如果为 null，则由环境确定工作任务的数量。
    - `useTPL`: 如果为 true，则使用任务并行库 (TPL) 进行并行计算。
  - 返回值: 表示异步操作的任务。任务结果包含最大元素的索引和值。

#### 27. LocateMaxAsync (数组范围)
- `public static Task<(int, TValue)> LocateMaxAsync<TValue>(this TValue[] values, int start, int length, int? numberOfWorkers = null, bool useTPL = false) where TValue : IComparable, IComparable<TValue>`
  - 返回并行序列指定范围内最大元素的索引和值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
    - `numberOfWorkers`: 工作任务的数量。如果为 null，则由环境确定工作任务的数量。
    - `useTPL`: 如果为 true，则使用任务并行库 (TPL) 进行并行计算。
  - 返回值: 表示异步操作的任务。任务结果包含最大元素的索引和值。

#### 28. CompareMinAsync
- `public static Task<TValue> CompareMinAsync<TValue>(this TValue[] values, int? numberOfWorkers = null, bool useTPL = false) where TValue : IComparable, IComparable<TValue>`
  - 返回并行序列中的最小值。
  - 参数:
    - `values`: 值的数组。
    - `numberOfWorkers`: 工作任务的数量。如果为 null，则由环境确定工作任务的数量。
    - `useTPL`: 如果为 true，则使用任务并行库 (TPL) 进行并行计算。
  - 返回值: 表示异步操作的任务。任务结果包含最小值。

#### 29. CompareMinAsync (数组范围)
- `public static Task<TValue> CompareMinAsync<TValue>(this TValue[] values, int start, int length, int? numberOfWorkers = null, bool useTPL = false) where TValue : IComparable, IComparable<TValue>`
  - 返回并行序列指定范围内的最小值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
    - `numberOfWorkers`: 工作任务的数量。如果为 null，则由环境确定工作任务的数量。
    - `useTPL`: 如果为 true，则使用任务并行库 (TPL) 进行并行计算。
  - 返回值: 表示异步操作的任务。任务结果包含最小值。

#### 30. LocateMinAsync
- `public static Task<(int, TValue)> LocateMinAsync<TValue>(this TValue[] values, int? numberOfWorkers = null, bool useTPL = false) where TValue : IComparable, IComparable<TValue>`
  - 返回并行序列中最小元素的索引和值。
  - 参数:
    - `values`: 值的数组。
    - `numberOfWorkers`: 工作任务的数量。如果为 null，则由环境确定工作任务的数量。
    - `useTPL`: 如果为 true，则使用任务并行库 (TPL) 进行并行计算。
  - 返回值: 表示异步操作的任务。任务结果包含最小元素的索引和值。

#### 31. LocateMinAsync (数组范围)
- `public static Task<(int, TValue)> LocateMinAsync<TValue>(this TValue[] values, int start, int length, int? numberOfWorkers = null, bool useTPL = false) where TValue : IComparable, IComparable<TValue>`
  - 返回并行序列指定范围内最小元素的索引和值。
  - 参数:
    - `values`: 值的数组。
    - `start`: 范围的起始索引。
    - `length`: 范围的长度。
    - `numberOfWorkers`: 工作任务的数量。如果为 null，则由环境确定工作任务的数量。
    - `useTPL`: 如果为 true，则使用任务并行库 (TPL) 进行并行计算。
  - 返回值: 表示异步操作的任务。任务结果包含最小元素的索引和值。

### 代码示例
以下是一个使用 IComparableExtension 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using System.Threading.Tasks;
using Vorcyc.Mathematics.Statistics;

public class IComparableExtensionExample
{
    public static async Task Main()
    {
        // 定义两个值
        int value1 = 5;
        int value2 = 10;

        // 比较两个值
        Console.WriteLine($"Is {value1} less than {value2}? {value1.LessThan(value2)}");
        Console.WriteLine($"Is {value1} less than or equal to {value2}? {value1.LessThanOrEqual(value2)}");
        Console.WriteLine($"Is {value1} greater than {value2}? {value1.GreaterThan(value2)}");
        Console.WriteLine($"Is {value1} greater than or equal to {value2}? {value1.GreaterThanOrEqual(value2)}");
        Console.WriteLine($"Is {value1} equal to {value2}? {value1.Equal(value2)}");
        Console.WriteLine($"Is {value1} not equal to {value2}? {value1.NotEqual(value2)}");

        // 定义一个数组
        int[] values = { 1, 2, 3, 4, 5 };

        // 获取数组中的最大值和最小值
        int max = values.CompareMax();
        int min = values.CompareMin();
        Console.WriteLine($"Max value in array: {max}");
        Console.WriteLine($"Min value in array: {min}");

        // 获取数组中最大值和最小值的索引和值
        var (maxIndex, maxValue) = values.LocateMax();
        var (minIndex, minValue) = values.LocateMin();
        Console.WriteLine($"Max value in array: {maxValue} at index {maxIndex}");
        Console.WriteLine($"Min value in array: {minValue} at index {minIndex}");

        // 并行计算数组中的最大值和最小值
        int parallelMax = await values.CompareMaxAsync();
        int parallelMin = await values.CompareMinAsync();
        Console.WriteLine($"Parallel max value in array: {parallelMax}");
        Console.WriteLine($"Parallel min value in array: {parallelMin}");

        // 并行计算数组中最大值和最小值的索引和值
        var (parallelMaxIndex, parallelMaxValue) = await values.LocateMaxAsync();
        var (parallelMinIndex, parallelMinValue) = await values.LocateMinAsync();
        Console.WriteLine($"Parallel max value in array: {parallelMaxValue} at index {parallelMaxIndex}");
        Console.WriteLine($"Parallel min value in array: {parallelMinValue} at index {parallelMinIndex}");
    }
}
```





