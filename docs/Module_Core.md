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


## Vorcyc.Mathematics.BaseConverter 类

Vorcyc.Mathematics.BaseConverter 是一个静态类，提供了将任意实现了 IBinaryInteger<TSelf> 接口的整数类型转换为指定进制的字符串表示和从指定进制的字符串表示转换为整数的方法。该类包含以下主要功能：
方法清单及说明
1.	`ToBaseString`
	- public static string ToBaseString<TSelf>(this TSelf integer, TSelf baseNumber) where TSelf : IBinaryInteger<TSelf>  
	将任意实现了 IBinaryInteger<TSelf> 接口的整数类型转换为指定进制的字符串表示。进制的范围在2到94之间。  
	- 参数:
		- integer: 要转换的整数。
		- baseNumber: 进制数。要求大于或等于2，小于或等于94。
	-	返回值: 字符串形式的进制数。
	-	异常:
		1.	ArgumentOutOfRangeException: 当整数为负数或进制数不在2到94之间时抛出。

2. `FromBaseString`
	- public static TSelf FromBaseString<TSelf>(this string value, TSelf baseNumber) where TSelf : IBinaryInteger<TSelf>
	- 将指定进制的字符串表示转换为任意实现了 IBinaryInteger<TSelf> 接口的整数类型。进制的范围在2到94之间。
	- 参数:
		1.	value: 要转换的字符串。
		2.	baseNumber: 进制数。要求大于或等于2，小于或等于94。
	- 返回值: 转换后的整数。
	- 异常:
		1. ArgumentOutOfRangeException: 当进制数不在2到94之间或字符串包含无效字符时抛出。
		
- 代码示例
```csharp
using System;
using Vorcyc.Mathematics;

public class BaseConverterExample
{
	public static void Main()
	{
		// 将整数转换为指定进制的字符串表示
		int number = 100;
		int baseNumber = 16;
		// 使用 ToBaseString 方法将整数 100 转换为 16 进制的字符串表示
		string baseString = number.ToBaseString(baseNumber);
		Console.WriteLine($"Number {number} in base {baseNumber} is: {baseString}");

		// 将指定进制的字符串表示转换为整数
		string value = "64";
		// 使用 FromBaseString 方法将 16 进制的字符串 "64" 转换为整数
		int convertedNumber = value.FromBaseString(baseNumber);
		Console.WriteLine($"String {value} in base {baseNumber} is: {convertedNumber}");
	}
}
```



## Vorcyc.Mathematics.BitMathExtension 类

Vorcyc.Mathematics.BitMathExtension 是一个静态类，提供了多种用于位运算的扩展方法。该类包含以下主要功能：
方法清单及说明
### 1.	IsPowerOf2
- public static bool IsPowerOf2(this uint x)
- public static bool IsPowerOf2(this ulong x)
- public static bool IsPowerOf2(this int x)
- public static bool IsPowerOf2(this long x)
- 验证一个数是否是2的幂。
### 2.	NextPowerOf2
- public static int NextPowerOf2(this int x)
- public static ulong NextPowerOf2(this ulong value)
- public static uint NextPowerOf2(this uint value)
- 获取下一个2的幂。
### 3.	PreviousPowerOf2
- public static int PreviousPowerOf2(this int x)
- public static ulong PreviousPowerOf2(this ulong value)
- public static uint PreviousPowerOf2(this uint value)
- 获取上一个2的幂。
### 4.	CountBitsSet
- public static int CountBitsSet(this uint value)
- public static int CountBitsSet(this ulong value)
- 计算设置的位数。
### 5.	CountBitsCleared
- public static int CountBitsCleared(this uint value)
- public static int CountBitsCleared(this ulong value)
- 计算未设置的位数。
### 6.	CreateBitMask
- public static ulong CreateBitMask(this int bitCount)
- 创建一个具有给定位数的位掩码。
### 7.	CountTrailingZeros
- public static int CountTrailingZeros(this uint value)
- public static int CountTrailingZeros(this ulong value)
- 计算从最低位开始的连续0的个数。
### 8.	CountLeadingZeros
- public static int CountLeadingZeros(this uint value)
- public static int CountLeadingZeros(this ulong value)
- 计算从最高位开始的连续0的个数。
### 9.	CountTrailingOnes
- public static int CountTrailingOnes(this uint value)
- public static int CountTrailingOnes(this ulong value)
- 计算从最低位开始的连续1的个数。
### 10.	CountLeadingOnes
- public static int CountLeadingOnes(this uint value)
- public static int CountLeadingOnes(this ulong value)
- 计算从最高位开始的连续1的个数。
### 11.	GetSetBitPositions
- public static IEnumerable<int> GetSetBitPositions(this ulong value)
- public static IEnumerable<int> GetSetBitPositions(this uint value)
- 返回设置位的位置。
### 12.	GetClearedBitPositions
- public static IEnumerable<int> GetClearedBitPositions(this uint value)
- public static IEnumerable<int> GetClearedBitPositions(this ulong value)
- 返回未设置位的位置。
### 13.	IsOdd
- public static bool IsOdd(this long value)
- public static bool IsOdd(this ulong value)
- public static bool IsOdd(this int value)
- public static bool IsOdd(this uint value)
- 判断是否是奇数。
### 14.	IsEven
- public static bool IsEven(this long value)
- public static bool IsEven(this ulong value)
- public static bool IsEven(this int value)
- public static bool IsEven(this uint value)
- 判断是否是偶数。
代码示例
以下是一个使用 BitMathExtension 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics;

public class BitMathExtensionExample
{
	public static void Main()
	{
		// 验证一个数是否是2的幂
		int number = 16;
		bool isPowerOf2 = number.IsPowerOf2();
		Console.WriteLine($"{number} is power of 2: {isPowerOf2}");

		// 获取下一个2的幂
		int nextPowerOf2 = number.NextPowerOf2();
		Console.WriteLine($"Next power of 2 after {number} is: {nextPowerOf2}");

		// 获取上一个2的幂
		int previousPowerOf2 = number.PreviousPowerOf2();
		Console.WriteLine($"Previous power of 2 before {number} is: {previousPowerOf2}");

		// 计算设置的位数
		uint value = 29;
		int bitsSet = value.CountBitsSet();
		Console.WriteLine($"Number of bits set in {value} is: {bitsSet}");

		// 计算未设置的位数
		int bitsCleared = value.CountBitsCleared();
		Console.WriteLine($"Number of bits cleared in {value} is: {bitsCleared}");

		// 创建一个具有给定位数的位掩码
		int bitCount = 5;
		ulong bitMask = bitCount.CreateBitMask();
		Console.WriteLine($"Bit mask with {bitCount} bits is: {bitMask}");

		// 计算从最低位开始的连续0的个数
		int trailingZeros = value.CountTrailingZeros();
		Console.WriteLine($"Number of trailing zeros in {value} is: {trailingZeros}");

		// 计算从最高位开始的连续0的个数
		int leadingZeros = value.CountLeadingZeros();
		Console.WriteLine($"Number of leading zeros in {value} is: {leadingZeros}");

		// 判断是否是奇数
		bool isOdd = number.IsOdd();
		Console.WriteLine($"{number} is odd: {isOdd}");

		// 判断是否是偶数
		bool isEven = number.IsEven();
		Console.WriteLine($"{number} is even: {isEven}");
	}
}

```