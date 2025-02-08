# 核心模块 - Core Module

> Vorcyc. Mathematics 命名空间

## Vorcyc.Mathematics.ArrayExtension 类

方法清单及说明  
## 通用方法
### 1.	Copy
- `public static T[] Copy<T>(this T[] source)`  
复制整个数组。
- `public static T[] Copy<T>(this T[] source, int length)`  
复制数组的指定长度。
### 2.	InitializeArray
- `public static T[]? InitializeArray<T>(int length, T initialValue = default)`  
初始化一个指定长度的数组，并用初始值填充。
### 3.	Fill
- `public static void Fill<T>(this T[] array, T value)`  
用指定值填充整个数组。
- `public static void Fill<T>(this T[] array, int start, int end, T value)`  
用指定值填充数组的指定范围。
- `public static void Fill<T>(this T[] array, Range range, T value)`  
用指定值填充数组的指定范围。
- `public static void Fill<T>(this Span<T> values, T value)`  
用指定值填充Span&lt;T&gt;。
### 4.	FillWithRandomNumber
- `public static void FillWithRandomNumber(this Span<float> span)`  
用随机浮点数填充数组。
- `public static void FillWithRandomNumber<T>(this T[] array) where T : IFloatingPointIeee754<T>`  
用随机数填充数组。
- `public static void FillWithRandomNumber<T>(this T[] array, Range range) where T : IFloatingPointIeee754<T>`  
用随机数填充数组的指定范围。
- `public static void FillWithRandomNumber(this int[] array, (int max, int min)? limit = null)`  
用随机数填充数组。
- `public static void FillWithRandomNumber(this int[] array, Range range)`  
用随机数填充数组的指定范围。
- `public static void FillWithRandomNumber(this long[] array)`  
用随机数填充数组。
- `public static void FillWithRandomNumber(this long[] array, Range range)`  
用随机数填充数组的指定范围。
### 5. GetInner
- `public static IEnumerable<T> GetInner<T>(this T[] array, int start, int length)`
获取数组的内部片段，返回一个可枚举对象。
### 6. GetInnerArray
- `public static T[] GetInnerArray<T>(this T[] array, int start, int length)`
获取数组的内部片段，返回一个新的数组。
### 7. RemoveSegment
- `public static T[] RemoveSegment<T>(this T[] array, int start, int length)`
移除数组中的一部分，并返回移除后的新数组。
### 8. Merge
- `public static T[] Merge<T>(this T[] leading, T[] following)`
合并两个数组，返回合并后的新数组。
### 9. ToString
- `public static string ToString<T>(this IEnumerable<T> collection)`
将集合转换为字符串表示。
### 10. FastCopyFragment
- `public static T[] FastCopyFragment<T>(this T[] source, int size, int sourceOffset = 0, int destinationOffset = 0) where T : unmanaged`  
快速复制数组的片段到一个新数组。
### 11.	FastCopyTo
- `public static void FastCopyTo<T>(this T[] source, T[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
快速复制数组的元素到另一个数组。
### 12.	Repeat
- `public static T[] Repeat<T>(this T[] source, int n)`  
创建一个包含源数组重复指定次数的新数组。


## 单精度方法
### 13.	FastCopy
- `public static float[] FastCopy(this float[] source)`
创建一个单精度浮点数组的快速副本。
### 14.	FastCopyTo
- `public static void FastCopyTo(this float[] source, float[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
快速复制单精度浮点数组的元素到另一个数组。
### 15.	FastCopyFragment
- `public static float[] FastCopyFragment(this float[] source, int size, int sourceOffset = 0, int destinationOffset = 0)`  
快速复制单精度浮点数组的片段到一个新数组。
### 16.	Merge
- `public static float[] Merge(this float[] source, float[] another)`  
合并两个单精度浮点数组，返回合并后的新数组。
### 17.	Repeat
`public static float[] Repeat(this float[] source, int n)`
创建一个包含源单精度浮点数组重复指定次数的新数组。
### 18.	PadZeros
- `public static float[] PadZeros(this float[] source, int size)`  
在原数组的基础上将数组补0至目标长度，以创建一个指定大小的零填充单精度浮点数组。
- `public static T[] PadZeros<T>(this T[] source, int size)`  
在原数组的基础上将数组补0至目标长度，以创建一个指定大小的零填充数组。

## 双精度方法
### 19.	FastCopy
- `public static double[] FastCopy(this double[] source)`  
创建一个双精度浮点数组的快速副本。
### 20.	FastCopyTo
- `public static void FastCopyTo(this double[] source, double[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
快速复制双精度浮点数组的元素到另一个数组。
### 21.	FastCopyFragment
- `public static double[] FastCopyFragment(this double[] source, int size, int sourceOffset = 0, int destinationOffset = 0)`  
快速复制双精度浮点数组的片段到一个新数组。
### 22.	Merge
- `public static double[] Merge(this double[] source, double[] another)`  
合并两个双精度浮点数组，返回合并后的新数组。
### 23.	Repeat
- `public static double[] Repeat(this double[] source, int n)`  
创建一个包含源双精度浮点数组重复指定次数的新数组。
### 24.	PadZeros
- `public static double[] PadZeros(this double[] source, int size)`  
创建一个指定大小的零填充双精度浮点数组。

## 其他方法
### 25.	ToFloats
- `public static float[] ToFloats(this IEnumerable<double> values)`  
将双精度值的可枚举对象转换为单精度数组。
### 26.	ToDoubles
- `public static double[] ToDoubles(this IEnumerable<float> values)`  
将单精度值的可枚举对象转换为双精度数组。
### 27.	Last
- `public static T Last<T>(this T[] array)`  
获取数组的最后一个元素。
### 28.	First
- `public static T First<T>(this T[] array)`  
获取数组的第一个元素。

代码示例
```csharp
using System;
using System.Linq;
using Vorcyc.Mathematics;

public class ArrayExtensionExample
{
	public static void Main()
	{
		// 创建一个整数数组
		int[] array = { 1, 2, 3, 4, 5 };

		// 复制数组
		int[] copiedArray = array.Copy();
		Console.WriteLine("Copied Array: " + string.Join(", ", copiedArray));

		// 初始化一个长度为5，初始值为10的数组
		int[] initializedArray = ArrayExtension.InitializeArray(5, 10);
		Console.WriteLine("Initialized Array: " + string.Join(", ", initializedArray));

		// 用指定值填充数组
		array.Fill(9);
		Console.WriteLine("Filled Array: " + string.join(", ", array));

		// 用随机数填充数组
		array.FillWithRandomNumber((max: 100, min: 1));
		Console.WriteLine("Array Filled with Random Numbers: " + string.Join(", ", array));

		// 获取数组的内部片段
		int[] innerArray = array.GetInnerArray(1, 3);
		Console.WriteLine("Inner Array: " + string.Join(", ", innerArray));

		// 移除数组中的一部分
		int[] removedSegmentArray = array.RemoveSegment(1, 2);
		Console.WriteLine("Array after Removing Segment: " + string.Join(", ", removedSegmentArray));

		// 合并两个数组
		int[] mergedArray = array.Merge(new int[] { 6, 7, 8 });
		Console.WriteLine("Merged Array: " + string.Join(", ", mergedArray));

		// 将数组转换为字符串
		string arrayString = array.ToString();
		Console.WriteLine("Array as String: " + arrayString);

		// 快速复制数组的片段
		int[] fastCopiedFragment = array.FastCopyFragment(3, 1);
		Console.WriteLine("Fast Copied Fragment: " + string.Join(", ", fastCopiedFragment));

		// 创建一个包含源数组重复3次的新数组
		int[] repeatedArray = array.Repeat(3);
		Console.WriteLine("Repeated Array: " + string.Join(", ", repeatedArray));

		// 将数组分割成指定长度的段
		var segments = array.Split(2);
		Console.WriteLine("Array Segments:");
		foreach (var segment in segments)
		{
			Console.WriteLine(string.Join(", ", segment));
		}

		// 压缩数组并生成数组片段序列
		var zippedSegments = array.Zip(3);
		Console.WriteLine("Zipped Array Segments:");
		foreach (var segment in zippedSegments)
		{
			Console.WriteLine(string.Join(", ", segment));
		}

		// 通过平均原数组的片段，将单精度浮点数组转换为指定长度的新数组
		float[] floatArray = { 1.1f, 2.2f, 3.3f, 4.4f, 5.5f };
		float[] transformedArray = floatArray.TransformToArray(3);
		Console.WriteLine("Transformed Array: " + string.Join(", ", transformedArray));
	}
}

```