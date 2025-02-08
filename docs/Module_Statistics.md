# 统计模块
> Vorcyc.Mathematics.Statistics类。

Vorcyc.Mathematics.Statistics类是一个全面的工具类，用于对数值数据进行统计分析，支持多种数据结构和数值类型。它包括查找极值、计算总和、平均值、方差，以及识别最大值和最小值及其索引的方法。该类在可能的情况下利用硬件加速以优化性能。

## 1. FindExtremeValue
### 重载：
- `(float[] array, int start, int length)`
- `(T[] array, int start, int length) where T : INumber<T>`
- `(float[] array)`
- `(T[] array) where T : INumber<T>`
- `(ArraySegment<float> arraySegment)`
- `(ArraySegment<T> arraySegment) where T : INumber<T>`
- `(Span<T> span) where T : unmanaged, INumber<T>`
### 描述：在数组、数组段和跨度中查找最大值和最小值。
### 示例：
```csharp
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   var (max, min) = data.FindExtremeValue();
   Console.WriteLine($"Max: {max}, Min: {min}");
```
   

Sum
1.	重载：
·	(ArraySegment<float> arraySegment)
·	(ArraySegment<double> arraySegment)
·	(Span<float> values)
·	(float[] array, int start, int length)
·	(float[] values)
·	(Span<double> values)
·	(double[] values)
·	(double[] values, int start, int length)
·	(Span<T> span) where T : INumber<T>
2.	描述：计算数组、数组段和跨度中元素的总和。
3.	示例：
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   float sum = data.Sum();
   Console.WriteLine($"Sum: {sum}");
   

Average
1.	重载：
·	(float[] array)
·	(float[] array, int start, int length)
·	(byte[] array)
·	(Span<float> values)
·	(Span<double> values)
·	(ArraySegment<float> arraySegment)
·	(IEnumerable<ArraySegment<float>> arraySegments)
·	(Span<T> span) where T : INumber<T>
2.	描述：计算数组、数组段和跨度中元素的平均值。
3.	示例：
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   float average = data.Average();
   Console.WriteLine($"Average: {average}");
   

Variance
1.	重载：
·	(float[] array)
·	(float[] array, int start, int length)
·	(Span<float> values)
·	(Span<double> values)
·	(Span<T> array) where T : INumber<T>
2.	描述：计算数组和跨度中元素的方差。
3.	示例：
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   var (average, variance) = data.Variance();
   Console.WriteLine($"Average: {average}, Variance: {variance}");
   

GetMaximumMinimumMedian
1.	重载：
·	(float[] array)
2.	描述：查找浮点数组中的最大值、最小值和中位数。
3.	示例：
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   var (max, min, median) = data.GetMaximumMinimumMedian();
   Console.WriteLine($"Max: {max}, Min: {min}, Median: {median}");
   

Max
1.	重载：
·	(Span<T> span) where T : struct, INumber<T>
·	(T[] values) where T : struct, INumber<T>
·	(T[] values, int start, int length) where T : struct, INumber<T>
2.	描述：查找数组和跨度中元素的最大值。
3.	示例：
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   float max = data.Max();
   Console.WriteLine($"Max: {max}");
   

LocateMax
1.	重载：
·	(Span<T> span) where T : INumber<T>
·	(T[] values) where T : INumber<T>
·	(T[] values, int start, int length) where T : INumber<T>
2.	描述：查找数组和跨度中元素的最大值及其索引。
3.	示例：
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   var (index, max) = data.LocateMax();
   Console.WriteLine($"Index: {index}, Max: {max}");
   

Min
1.	重载：
·	(Span<T> span) where T : struct, INumber<T>
·	(T[] values) where T : struct, INumber<T>
·	(T[] values, int start, int length) where T : struct, INumber<T>
2.	描述：查找数组和跨度中元素的最小值。
3.	示例：
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   float min = data.Min();
   Console.WriteLine($"Min: {min}");
   

LocateMin
1.	重载：
·	(Span<T> span) where T : INumber<T>
·	(T[] values) where T : INumber<T>
·	(T[] values, int start, int length) where T : INumber<T>
2.	描述：查找数组和跨度中元素的最小值及其索引。
3.	示例：
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   var (index, min) = data.LocateMin();
   Console.WriteLine($"Index: {index}, Min: {min}");
   

