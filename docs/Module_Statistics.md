# 统计模块
> Vorcyc.Mathematics.Statistics 类。

Vorcyc.Mathematics.Statistics类是一个全面的工具类，用于对数值数据进行统计分析，支持多种数据结构和数值类型。它包括查找极值、计算总和、平均值、方差，以及识别最大值和最小值及其索引的方法。该类在可能的情况下利用硬件加速以优化性能。

## 1. FindExtremeValue 方法
在数组、数组段和跨度中查找最大值和最小值。
### 重载：
- `(float[] array, int start, int length)`
- `(T[] array, int start, int length) where T : INumber<T>`
- `(float[] array)`
- `(T[] array) where T : INumber<T>`
- `(ArraySegment<float> arraySegment)`
- `(ArraySegment<T> arraySegment) where T : INumber<T>`
- `(Span<T> span) where T : unmanaged, INumber<T>`
### 示例：
```csharp
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   var (max, min) = data.FindExtremeValue();
   Console.WriteLine($"Max: {max}, Min: {min}");
```
   

## 2. Sum 方法
计算数组、数组段和跨度中元素的总和。
### 重载：
- `(ArraySegment<float> arraySegment)`
- `(ArraySegment<double> arraySegment)`
- `(Span<float> values)`
- `(float[] array, int start, int length)`
- `(float[] values)`
- `(Span<double> values)`
- `(double[] values)`
- `(double[] values, int start, int length)`
- `(Span<T> span) where T : INumber<T>`
### 示例：
```csharp
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   float sum = data.Sum();
   Console.WriteLine($"Sum: {sum}");
```

## 3. Average 方法
计算数组、数组段和跨度中元素的平均值。
### 重载：
- `(float[] array)`
- `(float[] array, int start, int length)`
- `(byte[] array)`
- `(Span<float> values)`
- `(Span<double> values)`
- `(ArraySegment<float> arraySegment)`
- `(IEnumerable<ArraySegment<float>> arraySegments)`
- `(Span<T> span) where T : INumber<T>`
### 示例：
```csharp
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   float average = data.Average();
   Console.WriteLine($"Average: {average}");
```
   

## 4. Variance 方法
计算数组和跨度中元素的方差。
### 重载：
- `(float[] array)`
- `(float[] array, int start, int length)`
- `(Span<float> values)`
- `(Span<double> values)`
- `(Span<T> array) where T : INumber<T>`
### 示例：
```csharp
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   var (average, variance) = data.Variance();
   Console.WriteLine($"Average: {average}, Variance: {variance}");
```
   

## 5. GetMaximumMinimumMedian 方法
查找浮点数组中的最大值、最小值和中位数。
### 重载：
- `(float[] array)`
### 示例：
```csharp
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   var (max, min, median) = data.GetMaximumMinimumMedian();
   Console.WriteLine($"Max: {max}, Min: {min}, Median: {median}");
```
   

## 6. Max 方法
查找数组和跨度中元素的最大值。
### 重载：
- `(Span<T> span) where T : struct, INumber<T>`
- `(T[] values) where T : struct, INumber<T>`
- `(T[] values, int start, int length) where T : struct, INumber<T>`
### 示例：
```csharp
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   float max = data.Max();
   Console.WriteLine($"Max: {max}");
```   

## 7. LocateMax 方法
查找数组和跨度中元素的最大值及其索引。
### 重载：
- `(Span<T> span) where T : INumber<T>`
- `(T[] values) where T : INumber<T>`
- `(T[] values, int start, int length) where T : INumber<T>`：
### 示例：
```csharp
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   var (index, max) = data.LocateMax();
   Console.WriteLine($"Index: {index}, Max: {max}");
```

## 8. Min 方法
查找数组和跨度中元素的最小值。
### 重载：
- `(Span<T> span) where T : struct, INumber<T>`
- `(T[] values) where T : struct, INumber<T>`
- `(T[] values, int start, int length) where T : struct, INumber<T>`
### 示例：
```
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   float min = data.Min();
   Console.WriteLine($"Min: {min}");
```

## 9. LocateMin 方法
查找数组和跨度中元素的最小值及其索引。
### 重载：
- `(Span<T> span) where T : INumber<T>`
- `(T[] values) where T : INumber<T>`
- `(T[] values, int start, int length) where T : INumber<T>`
### 示例：
```
   float[] data = { 1.2f, 3.4f, 5.6f, 7.8f };
   var (index, min) = data.LocateMin();
   Console.WriteLine($"Index: {index}, Min: {min}");
```
   

