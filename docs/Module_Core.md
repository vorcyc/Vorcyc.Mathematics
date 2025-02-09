# 核心模块 - Core Module

> Vorcyc. Mathematics 命名空间

## Vorcyc.Mathematics.ArrayExtension 类

方法清单及说明  
### 通用方法
#### 1.	Copy
- `public static T[] Copy<T>(this T[] source)`  
复制整个数组。
- `public static T[] Copy<T>(this T[] source, int length)`  
复制数组的指定长度。
#### 2.	InitializeArray
- `public static T[]? InitializeArray<T>(int length, T initialValue = default)`  
初始化一个指定长度的数组，并用初始值填充。
#### 3.	Fill
- `public static void Fill<T>(this T[] array, T value)`  
用指定值填充整个数组。
- `public static void Fill<T>(this T[] array, int start, int end, T value)`  
用指定值填充数组的指定范围。
- `public static void Fill<T>(this T[] array, Range range, T value)`  
用指定值填充数组的指定范围。
- `public static void Fill<T>(this Span<T> values, T value)`  
用指定值填充Span&lt;T&gt;。
#### 4.	FillWithRandomNumber
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
#### 5. GetInner
- `public static IEnumerable<T> GetInner<T>(this T[] array, int start, int length)`
获取数组的内部片段，返回一个可枚举对象。
#### 6. GetInnerArray
- `public static T[] GetInnerArray<T>(this T[] array, int start, int length)`
获取数组的内部片段，返回一个新的数组。
#### 7. RemoveSegment
- `public static T[] RemoveSegment<T>(this T[] array, int start, int length)`
移除数组中的一部分，并返回移除后的新数组。
#### 8. Merge
- `public static T[] Merge<T>(this T[] leading, T[] following)`
合并两个数组，返回合并后的新数组。
#### 9. ToString
- `public static string ToString<T>(this IEnumerable<T> collection)`
将集合转换为字符串表示。
#### 10. FastCopyFragment
- `public static T[] FastCopyFragment<T>(this T[] source, int size, int sourceOffset = 0, int destinationOffset = 0) where T : unmanaged`  
快速复制数组的片段到一个新数组。
#### 11.	FastCopyTo
- `public static void FastCopyTo<T>(this T[] source, T[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
快速复制数组的元素到另一个数组。
#### 12.	Repeat
- `public static T[] Repeat<T>(this T[] source, int n)`  
创建一个包含源数组重复指定次数的新数组。


### 单精度方法
#### 13.	FastCopy
- `public static float[] FastCopy(this float[] source)`
创建一个单精度浮点数组的快速副本。
#### 14.	FastCopyTo
- `public static void FastCopyTo(this float[] source, float[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
快速复制单精度浮点数组的元素到另一个数组。
#### 15.	FastCopyFragment
- `public static float[] FastCopyFragment(this float[] source, int size, int sourceOffset = 0, int destinationOffset = 0)`  
快速复制单精度浮点数组的片段到一个新数组。
#### 16.	Merge
- `public static float[] Merge(this float[] source, float[] another)`  
合并两个单精度浮点数组，返回合并后的新数组。
#### 17.	Repeat
`public static float[] Repeat(this float[] source, int n)`
创建一个包含源单精度浮点数组重复指定次数的新数组。
#### 18.	PadZeros
- `public static float[] PadZeros(this float[] source, int size)`  
在原数组的基础上将数组补0至目标长度，以创建一个指定大小的零填充单精度浮点数组。
- `public static T[] PadZeros<T>(this T[] source, int size)`  
在原数组的基础上将数组补0至目标长度，以创建一个指定大小的零填充数组。

### 双精度方法
#### 19.	FastCopy
- `public static double[] FastCopy(this double[] source)`  
创建一个双精度浮点数组的快速副本。
#### 20.	FastCopyTo
- `public static void FastCopyTo(this double[] source, double[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
快速复制双精度浮点数组的元素到另一个数组。
#### 21.	FastCopyFragment
- `public static double[] FastCopyFragment(this double[] source, int size, int sourceOffset = 0, int destinationOffset = 0)`  
快速复制双精度浮点数组的片段到一个新数组。
#### 22.	Merge
- `public static double[] Merge(this double[] source, double[] another)`  
合并两个双精度浮点数组，返回合并后的新数组。
#### 23.	Repeat
- `public static double[] Repeat(this double[] source, int n)`  
创建一个包含源双精度浮点数组重复指定次数的新数组。
#### 24.	PadZeros
- `public static double[] PadZeros(this double[] source, int size)`  
创建一个指定大小的零填充双精度浮点数组。

### 其他方法
#### 25.	ToFloats
- `public static float[] ToFloats(this IEnumerable<double> values)`  
将双精度值的可枚举对象转换为单精度数组。
#### 26.	ToDoubles
- `public static double[] ToDoubles(this IEnumerable<float> values)`  
将单精度值的可枚举对象转换为双精度数组。
#### 27.	Last
- `public static T Last<T>(this T[] array)`  
获取数组的最后一个元素。
#### 28.	First
- `public static T First<T>(this T[] array)`  
获取数组的第一个元素。

### 代码示例
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
#### 1.	ToBaseString
- `public static string ToBaseString<TSelf>(this TSelf integer, TSelf baseNumber) where TSelf : IBinaryInteger<TSelf>`
将任意实现了 IBinaryInteger<TSelf> 接口的整数类型转换为指定进制的字符串表示。进制的范围在2到94之间。  
- 参数:
	- integer: 要转换的整数。
	- baseNumber: 进制数。要求大于或等于2，小于或等于94。
-	返回值: 字符串形式的进制数。
-	异常:
	1.	ArgumentOutOfRangeException: 当整数为负数或进制数不在2到94之间时抛出。

#### 2. FromBaseString
- `public static TSelf FromBaseString<TSelf>(this string value, TSelf baseNumber) where TSelf : IBinaryInteger<TSelf>`
- 将指定进制的字符串表示转换为任意实现了 IBinaryInteger<TSelf> 接口的整数类型。进制的范围在2到94之间。
- 参数:
	1.	value: 要转换的字符串。
	2.	baseNumber: 进制数。要求大于或等于2，小于或等于94。
- 返回值: 转换后的整数。
- 异常:
	1. ArgumentOutOfRangeException: 当进制数不在2到94之间或字符串包含无效字符时抛出。
		
### 代码示例
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
### 方法清单及说明
#### 1.	IsPowerOf2
- public static bool IsPowerOf2(this uint x)
- public static bool IsPowerOf2(this ulong x)
- public static bool IsPowerOf2(this int x)
- public static bool IsPowerOf2(this long x)
- 验证一个数是否是2的幂。
#### 2.	NextPowerOf2
- public static int NextPowerOf2(this int x)
- public static ulong NextPowerOf2(this ulong value)
- public static uint NextPowerOf2(this uint value)
- 获取下一个2的幂。
#### 3.	PreviousPowerOf2
- public static int PreviousPowerOf2(this int x)
- public static ulong PreviousPowerOf2(this ulong value)
- public static uint PreviousPowerOf2(this uint value)
- 获取上一个2的幂。
#### 4.	CountBitsSet
- public static int CountBitsSet(this uint value)
- public static int CountBitsSet(this ulong value)
- 计算设置的位数。
#### 5.	CountBitsCleared
- public static int CountBitsCleared(this uint value)
- public static int CountBitsCleared(this ulong value)
- 计算未设置的位数。
#### 6.	CreateBitMask
- public static ulong CreateBitMask(this int bitCount)
- 创建一个具有给定位数的位掩码。
#### 7.	CountTrailingZeros
- public static int CountTrailingZeros(this uint value)
- public static int CountTrailingZeros(this ulong value)
- 计算从最低位开始的连续0的个数。
#### 8.	CountLeadingZeros
- public static int CountLeadingZeros(this uint value)
- public static int CountLeadingZeros(this ulong value)
- 计算从最高位开始的连续0的个数。
#### 9.	CountTrailingOnes
- public static int CountTrailingOnes(this uint value)
- public static int CountTrailingOnes(this ulong value)
- 计算从最低位开始的连续1的个数。
#### 10.	CountLeadingOnes
- public static int CountLeadingOnes(this uint value)
- public static int CountLeadingOnes(this ulong value)
- 计算从最高位开始的连续1的个数。
#### 11.	GetSetBitPositions
- public static IEnumerable<int> GetSetBitPositions(this ulong value)
- public static IEnumerable<int> GetSetBitPositions(this uint value)
- 返回设置位的位置。
#### 12.	GetClearedBitPositions
- public static IEnumerable<int> GetClearedBitPositions(this uint value)
- public static IEnumerable<int> GetClearedBitPositions(this ulong value)
- 返回未设置位的位置。
#### 13.	IsOdd
- public static bool IsOdd(this long value)
- public static bool IsOdd(this ulong value)
- public static bool IsOdd(this int value)
- public static bool IsOdd(this uint value)
- 判断是否是奇数。
#### 14.	IsEven
- public static bool IsEven(this long value)
- public static bool IsEven(this ulong value)
- public static bool IsEven(this int value)
- public static bool IsEven(this uint value)
- 判断是否是偶数。

### 代码示例
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



## Vorcyc.Mathematics.Combinatorics 类

Vorcyc.Mathematics.Combinatorics 是一个静态类，提供了多种组合数学函数。该类包含以下主要功能：
### 方法清单及说明
#### 1.	TruthTable
- public static int[][] TruthTable(int length)
- public static int[][] TruthTable(int symbols, int length)
- public static int[][] TruthTable(this int[] symbols)
- 生成所有可能的有序排列，允许重复（真值表）。
#### 2.	Sequences
- public static IEnumerable<int[]> Sequences(int length, bool inPlace = false)
- public static IEnumerable<int[]> Sequences(int symbols, int length, bool inPlace = false)
- public static IEnumerable<int[]> Sequences(this int[] symbols, bool inPlace = false)
- 提供一种方法来枚举所有可能的有序排列，允许重复（真值表），而不使用大量内存分配。
#### 3.	Combinations
- public static IEnumerable<T[]> Combinations<T>(this T[] values, bool inPlace = false)
- public static IEnumerable<T[]> Combinations<T>(this T[] values, int k, bool inPlace = false)
- 枚举给定数组的所有可能值组合。
#### 4.	Subsets
- public static IEnumerable<SortedSet<T>> Subsets<T>(this ISet<T> set, bool inPlace = false)
- public static IEnumerable<SortedSet<T>> Subsets<T>(this ISet<T> set, int k, bool inPlace = false)
- 生成给定集合的所有可能子集。

### 代码示例
以下是一个使用 Combinatorics 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics;

public class CombinatoricsExample
{
	public static void Main()
	{
		// 生成真值表
		int length = 3;
		int[][] truthTable = Combinatorics.TruthTable(length);
		Console.WriteLine("Truth Table:");
		foreach (var row in truthTable)
		{
			Console.WriteLine(string.Join(", ", row));
		}

		// 枚举所有可能的有序排列
		Console.WriteLine("\nSequences:");
		foreach (var sequence in Combinatorics.Sequences(length))
		{
			Console.WriteLine(string.Join(", ", sequence));
		}

		// 枚举给定数组的所有可能值组合
		int[] values = { 1, 2, 3 };
		Console.WriteLine("\nCombinations:");
		foreach (var combination in Combinatorics.Combinations(values))
		{
			Console.WriteLine(string.Join(", ", combination));
		}

		// 生成给定集合的所有可能子集
		ISet<int> set = new HashSet<int> { 1, 2, 3 };
		Console.WriteLine("\nSubsets:");
		foreach (var subset in Combinatorics.Subsets(set))
		{
			Console.WriteLine(string.Join(", ", subset));
		}
	}
}

```

## Vorcyc.Mathematics.ConstantsFp32 类

Vorcyc.Mathematics.ConstantsFp32 是一个静态类，定义了一组常用的单精度浮点数常量。这些常量在数学计算中非常有用。以下是每个常量的详细说明：
常量清单及说明
#### 1.	E
- public const float E = 2.71828182845904523536f;
- 自然对数的底数 e。
#### 2.	LOG2E
- public const float LOG2E = 1.44269504088896340736f;
- 以 2 为底的 e 的对数。
#### 3.	LOG10E
- public const float LOG10E = 0.434294481903251827651f;
- 以 10 为底的 e 的对数。
#### 4.	LN2
- public const float LN2 = 0.693147180559945309417f;
- 2 的自然对数。
#### 5.	LN10
- public const float LN10 = 2.30258509299404568402f;
- 10 的自然对数。
#### 6.	PI
- public const float PI = 3.1415926535897932384626433832795f;
- 圆周率 π。
#### 7.	PI_2
- public const float PI_2 = 1.57079632679489661923f;
- π 的一半。
#### 8.	PI_4
- public const float PI_4 = 0.785398163397448309616f;
- π 的四分之一。
#### 9.	_1_PI
- public const float _1_PI = 0.318309886183790671538f;
- 1 除以 π。
#### 10.	_2_PI
- public const float _2_PI = 0.636619772367581343076f;
- 2 除以 π。
#### 11.	_2_SQRT_PI
- public const float _2_SQRT_PI = 1.12837916709551257390f;
- 2 除以 π 的平方根。
#### 12.	_SQRT2
- public const float _SQRT2 = 1.41421356237309504880f;
- 2 的平方根。
#### 13.	_SQRT1_2
- public const float _SQRT1_2 = 0.707106781186547524401f;
- 1 除以 2 的平方根。
#### 14.	TWO_PI
- public const float TWO_PI = 6.28318530717958647692f;
- 2 倍的 π。
#### 15.	TOLERANCE
- public const float TOLERANCE = 1E-09f;
- 公差，用于比较浮点数的精度。
#### 16.	RADIANS_PER_DEGREE
- public const float RADIANS_PER_DEGREE = 0.01745329f;
- 每度的弧度数。
#### 17.	DEGREES_PER_RADIAN
- public const float DEGREES_PER_RADIAN = 57.29578f;
- 每弧度的度数。
#### 18.	EPSILON
- public const float EPSILON = 0.001f;
- 用于浮点数比较的极小值。
### 代码示例
以下是一个使用 ConstantsFp32 类中常量的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics;

public class ConstantsFp32Example
{
	public static void Main()
	{
		// 使用自然对数的底数 e
		float e = ConstantsFp32.E;
		Console.WriteLine($"e: {e}");

		// 使用圆周率 π
		float pi = ConstantsFp32.PI;
		Console.WriteLine($"π: {pi}");

		// 使用每度的弧度数
		float radiansPerDegree = ConstantsFp32.RADIANS_PER_DEGREE;
		Console.WriteLine($"Radians per degree: {radiansPerDegree}");

		// 使用每弧度的度数
		float degreesPerRadian = ConstantsFp32.DEGREES_PER_RADIAN;
		Console.WriteLine($"Degrees per radian: {degreesPerRadian}");

		// 使用公差
		float tolerance = ConstantsFp32.TOLERANCE;
		Console.WriteLine($"Tolerance: {tolerance}");

		// 使用 epsilon
		float epsilon = ConstantsFp32.EPSILON;
		Console.WriteLine($"Epsilon: {epsilon}");
	}
}
```



## Vorcyc.Mathematics.NumberMapper 类
Vorcyc.Mathematics.NumberMapper 是一个静态类，提供了将一个值从一个范围映射到另一个范围的方法。该类包含以下主要功能：
方法清单及说明
#### 1.	Map
- 将输入值从一个范围映射到另一个范围。
- `public static TNumber Map<TNumber>(this TNumber number, TNumber inMin, TNumber inMax, TNumber outMin, TNumber outMax, InputValueOutOfRangeHandleBehavior handleBehavior = InputValueOutOfRangeHandleBehavior.Saturating) where TNumber : unmanaged, INumber<TNumber>`
	-   参数:
		1.	number: 要映射的输入值。
		2.	inMin: 输入范围的最小值。
		3.	inMax: 输入范围的最大值。
		4.	outMin: 输出范围的最小值。
		5.	outMax: 输出范围的最大值。
		6.	handleBehavior: 指定当输入值超出范围时的处理行为。默认值为 InputValueOutOfRangeHandleBehavior.Saturating。
	-	返回值: 映射到输出范围的值。
	-	异常:
		1.	ArgumentOutOfRangeException: 当 inMin 大于或等于 inMax，或 outMin 大于或等于 outMax 时抛出。
		2.	ArgumentException: 当 number 超出输入范围且 handleBehavior 为 InputValueOutOfRangeHandleBehavior.ThrowException 时抛出。
#### 2.	Map
- 将浮点数从一个范围映射到另一个范围，并返回一个布尔值指示是否成功。
- `public static bool Map<TFloatingNumber>(TFloatingNumber input, TFloatingNumber inputMin, TFloatingNumber inputMax, TFloatingNumber outputMin, TFloatingNumber outputMax, out TFloatingNumber result) where TFloatingNumber : unmanaged, IFloatingPointIeee754<TFloatingNumber>`
	-	参数:
		1.	input: 要映射的浮点数。
		2.	inputMin: 输入范围的最小值。
		3.	inputMax: 输入范围的最大值。
		4.	outputMin: 输出范围的最小值。
		5.	outputMax: 输出范围的最大值。
		6.	result: 如果映射成功，则为映射后的浮点数；否则为 NaN。
	-	返回值: 如果映射成功，则为 true；否则为 false。
### 3. 枚举
	- InputValueOutOfRangeHandleBehavior
	- 定义当输入值超出指定范围时的处理行为。	值:
		1.	Saturating: 将输入值夹紧到最近的边界值。
		2.	ThrowException: 当输入值超出范围时抛出异常。
			
### 代码示例
以下是一个使用 NumberMapper 类中方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics;

public class NumberMapperExample
{
	public static void Main()
	{
		// 将整数从一个范围映射到另一个范围
		int number = 5;
		int inMin = 0;
		int inMax = 10;
		int outMin = 0;
		int outMax = 100;
		int mappedValue = number.Map(inMin, inMax, outMin, outMax);
		Console.WriteLine($"Mapped value: {mappedValue}");

		// 将浮点数从一个范围映射到另一个范围，并返回一个布尔值指示是否成功
		float input = 0.5f;
		float inputMin = 0.0f;
		float inputMax = 1.0f;
		float outputMin = 0.0f;
		float outputMax = 100.0f;
		bool success = NumberMapper.Map(input, inputMin, inputMax, outputMin, outputMax, out float result);
		Console.WriteLine($"Mapping successful: {success}, Mapped value: {result}");
	}
}

```


## Vorcyc.Mathematics.PinnableArray 类

Vorcyc.Mathematics.PinnableArray 是一个泛型类，表示一个可固定内存的一维数组，用以提供高性能内存访问机制和数组的操作。该类提供了多种方法和属性来操作数组，并且可以将数组固定在内存中，以便与非托管代码进行交互。该类包含以下主要功能：
### 方法清单及说明
#### 1.	构造函数
- `public PinnableArray(ArraySegment<T> segment, bool pin = false)`
	使用 ArraySegment&lt;T> 初始化 PinnableArray 实例。
- `public PinnableArray(Span<T> span, bool pin = false)`
	使用 Span&lt;T> 初始化 PinnableArray 实例。
- `public PinnableArray(T[] array, bool pin = false)`
	使用数组初始化 PinnableArray 实例。
- `public PinnableArray(T[] array, int offset, int count, bool pin = false)`
	使用数组的部分内容初始化 PinnableArray 实例。
- `public PinnableArray(int count, bool pin = false)`
	使用指定长度初始化 PinnableArray 实例。
#### 2.	Pin
-	`public void Pin()`
	将 PinnableArray 固定在内存中，以便与非托管代码进行交互。
#### 3.	Unpin
-	`public void Unpin()`
	取消固定 PinnableArray，使其可以被垃圾回收。
#### 4.	AsSpan
-	`public Span<T> AsSpan()`
	返回表示当前数组的 Span&lt;T>。
-	`public Span<T> AsSpan(int start, int length)`
	返回表示当前数组部分内容的 Span&lt;T>。
-	`public Span<T> AsSpan(int start)`
	返回表示当前数组从指定位置开始的 Span&lt;T>。
-	`public Span<T> AsSpan(System.Index startIndex)`
	返回表示当前数组从指定索引开始的 Span&lt;T>。
-	`public Span<T> AsSpan(Range range)`
	返回表示当前数组指定范围的 Span&lt;T>。
#### 5.	Fill
-	`public void Fill(T value)`
	用指定值填充整个数组。
-	`public void Fill(T startValue, T step)`
	用指定的起始值和步长填充数组。
-   `public void FillWith<TNumber>(TNumber number) where TNumber : unmanaged`
	用另一个非托管类型的值填充数组。
-   `public void FillWithRandomNumber()`
	用随机数填充数组。
#### 6.	Each
-   `public void Each(Func<int, T, T> func)`
	使用指定的函数遍历数组的每个元素。
-   `public void Each(Func<int, T?, T, T?, T> func, Direction direction = Direction.Forward)`
	使用指定的函数遍历数组的每个元素，并指定遍历方向。
#### 7.	Max
-   `public T Max()`
	返回数组中的最大值。
-   `public T Max(int start, int length)`
	返回数组指定范围内的最大值。
-   `public async Task<T> MaxAsync(int? numberOfWorkers = null, bool useTPL = false)`
	异步返回数组中的最大值。
-   `public async Task<T> MaxAsync(int start, int length, int? numberOfWorkers = null, bool useTPL = false)`
	异步返回数组指定范围内的最大值。
#### 8.	Min
-   `public T Min()`
	返回数组中的最小值。
-   `public T Min(int start, int length)`
	返回数组指定范围内的最小值。
- 	`public async Task<T> MinAsync(int? numberOfWorkers = null, bool useTPL = false)`
	异步返回数组中的最小值。该方法通过并行执行，针对大规模规矩效果更佳。
-   `public async Task<T> MinAsync(int start, int length, int? numberOfWorkers = null, bool useTPL = false)`
	异步返回数组指定范围内的最小值。该方法通过并行执行，针对大规模规矩效果更佳。
#### 9.	Map
-    `public void MapIn(T fromMin, T fromMax, T toMin, T toMax)`
	将数组中的值从一个范围映射到另一个范围。
-   `public void MapIn(T toMin, T toMax)`
	将数组中的值从当前范围映射到指定范围。
-    `public PinnableArray<T> Map(T fromMin, T fromMax, T toMin, T toMax)`
	返回一个新的 PinnableArray，其中的值从一个范围映射到另一个范围。
-    `public PinnableArray<T> Map(T toMin, T toMax)`
	返回一个新的 PinnableArray，其中的值从当前范围映射到指定范围。
#### 10.	Dot
-    `public T Dot(PinnableArray<T> another)`
	计算当前数组与另一个 PinnableArray 的点积。
#### 11.	Dispose
-    `public void Dispose()`
	释放 PinnableArray 占用的资源。
### 枚举
#### 12.	`Direction`
-    定义遍历数组的方向。
-    值:
	1.	`Forward`: 从前向后遍历。
	2.	`Inverse`: 从后向前遍历。
	
### 属性  
#### 13.	IsPinned
-    `public bool IsPinned { get; }`
	获取当前数组是否已固定在内存中。
#### 14.	Length
-    `public int Length { get; }`
	获取当前数组的长度。
#### 15.	Values
-    `public T[] Values { get; }`
	获取当前数组的普通数组形式。
### 操作符重载
#### 16.	 +
-    `public static PinnableArray<T> operator +(PinnableArray<T> left, PinnableArray<T> right)`
-    `public static PinnableArray<T> operator +(PinnableArray<T> left, T right)`
-    `public static PinnableArray<T> operator +(T left, PinnableArray<T> right)`
#### 17.	-
-    `public static PinnableArray<T> operator -(PinnableArray<T> left, PinnableArray<T> right)`
-    `public static PinnableArray<T> operator -(PinnableArray<T> left, T right)`
-    `public static PinnableArray<T> operator -(T left, PinnableArray<T> right)`
#### 18.	*
-    `public static PinnableArray<T> operator *(PinnableArray<T> left, PinnableArray<T> right)`
-    `public static PinnableArray<T> operator *(PinnableArray<T> left, T right)`
-    `public static PinnableArray<T> operator *(T left, PinnableArray<T> right)`
#### 19.	/
-    `public static PinnableArray<T> operator /(PinnableArray<T> left, PinnableArray<T> right)`
-    `public static PinnableArray<T> operator /(PinnableArray<T> left, T right)`
-    `public static PinnableArray<T> operator /(T left, PinnableArray<T> right)`

#### 代码示例
以下是一个使用 PinnableArray 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using System.Threading.Tasks;
using Vorcyc.Mathematics;

public class PinnableArrayExample
{
	public static async Task Main()
	{
		// 使用数组初始化 PinnableArray 实例
		int[] array = { 1, 2, 3, 4, 5 };
		PinnableArray<int> pinnableArray = new PinnableArray<int>(array);

		// 将数组固定在内存中
		pinnableArray.Pin();
		Console.WriteLine($"IsPinned: {pinnableArray.IsPinned}");

		// 获取数组的长度
		int length = pinnableArray.Length;
		Console.WriteLine($"Length: {length}");

		// 获取数组的普通数组形式
		int[] values = pinnableArray;
		Console.WriteLine($"Values: {string.Join(", ", values)}");

		// 用指定值填充整个数组
		pinnableArray.Fill(10);
		Console.WriteLine($"Filled Values: {string.Join(", ", pinnableArray.Values)}");

		// 用指定的起始值和步长填充数组
		pinnableArray.Fill(1, 2);
		Console.WriteLine($"Filled with step Values: {string.Join(", ", pinnableArray.Values)}");

		// 用随机数填充数组
		pinnableArray.FillWithRandomNumber();
		Console.WriteLine($"Filled with random numbers: {string.Join(", ", pinnableArray.Values)}");

		// 遍历数组的每个元素并进行操作
		pinnableArray.Each((index, value) => value * 2);
		Console.WriteLine($"Each element multiplied by 2: {string.Join(", ", pinnableArray.Values)}");

		// 获取数组中的最大值
		int maxValue = pinnableArray.Max();
		Console.WriteLine($"Max value: {maxValue}");

		// 获取数组中的最小值
		int minValue = pinnableArray.Min();
		Console.WriteLine($"Min value: {minValue}");

		// 异步获取数组中的最大值
		int maxAsyncValue = await pinnableArray.MaxAsync();
		Console.WriteLine($"Max async value: {maxAsyncValue}");

		// 异步获取数组中的最小值
		int minAsyncValue = await pinnableArray.MinAsync();
		Console.WriteLine($"Min async value: {minAsyncValue}");

		// 将数组中的值从一个范围映射到另一个范围
		pinnableArray.MapIn(0, 1, 0, 100);
		Console.WriteLine($"Mapped values: {string.Join(", ", pinnableArray.Values)}");

		// 计算当前数组与另一个 PinnableArray 的点积
		PinnableArray<int> anotherArray = new PinnableArray<int>(new int[] { 1, 2, 3, 4, 5 });
		int dotProduct = pinnableArray.Dot(anotherArray);
		Console.WriteLine($"Dot product: {dotProduct}");

		// 使用索引器访问和修改数组元素
		pinnableArray[0] = 42;
		Console.WriteLine($"First element after modification: {pinnableArray[0]}");

		// 获取数组的 Span 表示
		Span<int> span = pinnableArray.AsSpan();
		Console.WriteLine($"Span values: {string.Join(", ", span.ToArray())}");

		// 取消固定数组
		pinnableArray.Unpin();
		Console.WriteLine($"IsPinned: {pinnableArray.IsPinned}");

		// 释放资源
		pinnableArray.Dispose();
	}
}
```


## SimpleRNG_Fp32 和 SimpleRNG_Fp64 类

Vorcyc.Mathematics.SimpleRNG_Fp32 和 Vorcyc.Mathematics.SimpleRNG_Fp64 是两个静态类，分别用于生成 32 位和 64 位浮点数的随机数。它们基于 George Marsaglia 的 MWC（乘法与进位）算法，能够生成通过 Marsaglia 的 DIEHARD 随机数生成器测试的随机数。
### 方法清单及说明
#### 1. SetSeed
- `public static void SetSeed(uint u, uint v)`
  - 设置随机数生成器的种子。
  - 参数:
	- `u`: 第一个种子值。
	- `v`: 第二个种子值。

- `public static void SetSeed(uint u)`
  - 设置随机数生成器的种子。
  - 参数:
	- `u`: 种子值。

- `public static void SetSeedFromSystemTime()`
  - 使用系统时间设置随机数生成器的种子。

#### 2. GetUniform
- `public static double GetUniform()`
  - 生成一个 (0, 1) 区间内的均匀分布随机数。

#### 3. GetNormal
- `public static double GetNormal()`
  - 生成一个均值为 0，标准差为 1 的正态分布随机数。

- `public static double GetNormal(double mean, double standardDeviation)`
  - 生成一个指定均值和标准差的正态分布随机数。
  - 参数:
	- `mean`: 均值。
	- `standardDeviation`: 标准差。

#### 4. GetExponential
- `public static double GetExponential()`
  - 生成一个均值为 1 的指数分布随机数。

- `public static double GetExponential(double mean)`
  - 生成一个指定均值的指数分布随机数。
  - 参数:
	- `mean`: 均值。

#### 5. GetGamma
- `public static double GetGamma(double shape, double scale)`
  - 生成一个指定形状和尺度参数的伽马分布随机数。
  - 参数:
	- `shape`: 形状参数。
	- `scale`: 尺度参数。

#### 6. GetChiSquare
- `public static double GetChiSquare(double degreesOfFreedom)`
  - 生成一个指定自由度的卡方分布随机数。
  - 参数:
	- `degreesOfFreedom`: 自由度。

#### 7. GetInverseGamma
- `public static double GetInverseGamma(double shape, double scale)`
  - 生成一个指定形状和尺度参数的逆伽马分布随机数。
  - 参数:
	- `shape`: 形状参数。
	- `scale`: 尺度参数。

#### 8. GetWeibull
- `public static double GetWeibull(double shape, double scale)`
  - 生成一个指定形状和尺度参数的威布尔分布随机数。
  - 参数:
	- `shape`: 形状参数。
	- `scale`: 尺度参数。

#### 9. GetCauchy
- `public static double GetCauchy(double median, double scale)`
  - 生成一个指定中位数和尺度参数的柯西分布随机数。
  - 参数:
	- `median`: 中位数。
	- `scale`: 尺度参数。

#### 10. GetStudentT
- `public static double GetStudentT(double degreesOfFreedom)`
  - 生成一个指定自由度的 t 分布随机数。
  - 参数:
	- `degreesOfFreedom`: 自由度。

#### 11. GetLaplace
- `public static double GetLaplace(double mean, double scale)`
  - 生成一个指定均值和尺度参数的拉普拉斯分布（双指数分布）随机数。
  - 参数:
	- `mean`: 均值。
	- `scale`: 尺度参数。

#### 12. GetLogNormal
- `public static double GetLogNormal(double mu, double sigma)`
  - 生成一个对数正态分布随机数。
  - 参数:
	- `mu`: 对数正态分布的均值。
	- `sigma`: 对数正态分布的标准差。

#### 13. GetBeta
- `public static double GetBeta(double a, double b)`
  - 生成一个指定参数的贝塔分布随机数。
  - 参数:
	- `a`: 贝塔分布的第一个参数。
	- `b`: 贝塔分布的第二个参数。
### 代码示例
以下是一个使用 SimpleRNG_Fp32 和 SimpleRNG_Fp64 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics;

public class SimpleRNGExample
{
	public static void Main()
	{
		// 使用 SimpleRNG_Fp64 生成随机数
		SimpleRNG_Fp64.SetSeedFromSystemTime();
		double uniform64 = SimpleRNG_Fp64.GetUniform();
		double normal64 = SimpleRNG_Fp64.GetNormal();
		double exponential64 = SimpleRNG_Fp64.GetExponential();
		double gamma64 = SimpleRNG_Fp64.GetGamma(2.0, 2.0);
		double chiSquare64 = SimpleRNG_Fp64.GetChiSquare(2.0);
		double inverseGamma64 = SimpleRNG_Fp64.GetInverseGamma(2.0, 2.0);
		double weibull64 = SimpleRNG_Fp64.GetWeibull(2.0, 2.0);
		double cauchy64 = SimpleRNG_Fp64.GetCauchy(0.0, 1.0);
		double studentT64 = SimpleRNG_Fp64.GetStudentT(2.0);
		double laplace64 = SimpleRNG_Fp64.GetLaplace(0.0, 1.0);
		double logNormal64 = SimpleRNG_Fp64.GetLogNormal(0.0, 1.0);
		double beta64 = SimpleRNG_Fp64.GetBeta(2.0, 2.0);

		Console.WriteLine("SimpleRNG_Fp64:");
		Console.WriteLine($"Uniform: {uniform64}");
		Console.WriteLine($"Normal: {normal64}");
		Console.WriteLine($"Exponential: {exponential64}");
		Console.WriteLine($"Gamma: {gamma64}");
		Console.WriteLine($"ChiSquare: {chiSquare64}");
		Console.WriteLine($"InverseGamma: {inverseGamma64}");
		Console.WriteLine($"Weibull: {weibull64}");
		Console.WriteLine($"Cauchy: {cauchy64}");
		Console.WriteLine($"StudentT: {studentT64}");
		Console.WriteLine($"Laplace: {laplace64}");
		Console.WriteLine($"LogNormal: {logNormal64}");
		Console.WriteLine($"Beta: {beta64}");

		// 使用 SimpleRNG_Fp32 生成随机数
		SimpleRNG_Fp32.SetSeedFromSystemTime();
		float uniform32 = SimpleRNG_Fp32.GetUniform();
		float normal32 = SimpleRNG_Fp32.GetNormal();
		float exponential32 = SimpleRNG_Fp32.GetExponential();
		float gamma32 = SimpleRNG_Fp32.GetGamma(2.0f, 2.0f);
		float chiSquare32 = SimpleRNG_Fp32.GetChiSquare(2.0f);
		float inverseGamma32 = SimpleRNG_Fp32.GetInverseGamma(2.0f, 2.0f);
		float weibull32 = SimpleRNG_Fp32.GetWeibull(2.0f, 2.0f);
		float cauchy32 = SimpleRNG_Fp32.GetCauchy(0.0f, 1.0f);
		float studentT32 = SimpleRNG_Fp32.GetStudentT(2.0f);
		float laplace32 = SimpleRNG_Fp32.GetLaplace(0.0f, 1.0f);
		float logNormal32 = SimpleRNG_Fp32.GetLogNormal(0.0f, 1.0f);
		float beta32 = SimpleRNG_Fp32.GetBeta(2.0f, 2.0f);

		Console.WriteLine("SimpleRNG_Fp32:");
		Console.WriteLine($"Uniform: {uniform32}");
		Console.WriteLine($"Normal: {normal32}");
		Console.WriteLine($"Exponential: {exponential32}");
		Console.WriteLine($"Gamma: {gamma32}");
		Console.WriteLine($"ChiSquare: {chiSquare32}");
		Console.WriteLine($"InverseGamma: {inverseGamma32}");
		Console.WriteLine($"Weibull: {weibull32}");
		Console.WriteLine($"Cauchy: {cauchy32}");
		Console.WriteLine($"StudentT: {studentT32}");
		Console.WriteLine($"Laplace: {laplace32}");
		Console.WriteLine($"LogNormal: {logNormal32}");
		Console.WriteLine($"Beta: {beta32}");
	}
}
```


## Vorcyc.Mathematics.TrigonometryHelper 类
Vorcyc.Mathematics.TrigonometryHelper 是一个静态类，提供了多种用于三角运算的辅助方法。该类包含以下主要功能：
### 方法清单及说明
#### 1.	RadiansToDegrees
`public static double RadiansToDegrees(double a)`  
`public static float RadiansToDegrees(float radians)`  
将角度从弧度转换为度。
#### 2.	DegreesToRadians
`public static double DegreesToRadians(double a)`  
`public static float DegreesToRadians(float degrees)`  
将角度从度转换为弧度。
#### 3.	GetAngleDifference
`public static float GetAngleDifference(float radianAngle1, float radianAngle2)`  
获取两个角度之间的差值，单位为弧度。
#### 4.	RadianMin
`public static float RadianMin(float radianAngle)`
`public static double RadianMin(double radianAngle)`
将角度减少到其等效的 -π 和 π 之间。
#### 5.	Angle
`public static float Angle(float x, float y)`  
`public static double Angle(double x, double y)`  
获取由向量 [x,y] 形成的角度，单位为弧度。
#### 6.	Acosh
`public static double Acosh(double x)`  
`public static float Acosh(float x)`  
返回指定值的双曲余弦。
#### 7.	Asinh
`public static double Asinh(double d)`  
`public static float Asinh(float d)`  
`public static T Asinh<T>(T x) where T : struct, IFloatingPointIeee754<T>`  
返回指定值的双曲正弦。
#### 8.	Atanh
`public static double Atanh(double d)`  
`public static float Atanh(float d)`  
返回指定值的双曲正切。
#### 9.	Sinc
`public static double Sinc(double x)`  
`public static float Sinc(float x)`  
`public static T Sinc<T>(T x) where T : IFloatingPointIeee754<T>`  
计算 Sinc 函数。
### 代码示例
以下是一个使用 TrigonometryHelper 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics;

public class TrigonometryHelperExample
{
	public static void Main()
	{
		// 将角度从弧度转换为度
		double radians = Math.PI;
		double degrees = TrigonometryHelper.RadiansToDegrees(radians);
		Console.WriteLine($"Radians: {radians}, Degrees: {degrees}");

		// 将角度从度转换为弧度
		degrees = 180.0;
		radians = TrigonometryHelper.DegreesToRadians(degrees);
		Console.WriteLine($"Degrees: {degrees}, Radians: {radians}");

		// 获取两个角度之间的差值，单位为弧度
		float angle1 = 1.0f;
		float angle2 = 2.0f;
		float angleDifference = TrigonometryHelper.GetAngleDifference(angle1, angle2);
		Console.WriteLine($"Angle1: {angle1}, Angle2: {angle2}, Difference: {angleDifference}");

		// 将角度减少到其等效的 -π 和 π 之间
		float radianAngle = 4.0f;
		float reducedAngle = TrigonometryHelper.RadianMin(radianAngle);
		Console.WriteLine($"Radian Angle: {radianAngle}, Reduced Angle: {reducedAngle}");

		// 获取由向量 [x,y] 形成的角度，单位为弧度
		float x = 1.0f;
		float y = 1.0f;
		float vectorAngle = TrigonometryHelper.Angle(x, y);
		Console.WriteLine($"Vector [x,y]: [{x},{y}], Angle: {vectorAngle}");

		// 返回指定值的双曲余弦
		double value = 2.0;
		double acoshValue = TrigonometryHelper.Acosh(value);
		Console.WriteLine($"Value: {value}, Acosh: {acoshValue}");

		// 返回指定值的双曲正弦
		value = 1.0;
		double asinhValue = TrigonometryHelper.Asinh(value);
		Console.WriteLine($"Value: {value}, Asinh: {asinhValue}");

		// 返回指定值的双曲正切
		value = 0.5;
		double atanhValue = TrigonometryHelper.Atanh(value);
		Console.WriteLine($"Value: {value}, Atanh: {atanhValue}");

		// 计算 Sinc 函数
		double sincValue = TrigonometryHelper.Sinc(value);
		Console.WriteLine($"Sinc({value}) = {sincValue}");
	}
}

```


## VMath 类
Vorcyc.Mathematics.VMath 是一个静态类，提供了多种数学函数和操作。该类包含以下主要功能：
### 方法清单及说明
#### 1.	BinomialCoefficient
`public static float BinomialCoefficient(int k, int n)`  
计算二项式系数。
#### 2.	Diff
`public static void Diff(float[] samples, float[] diff)`  
计算离散差分。
#### 3.	InterpolateLinear
`public static void InterpolateLinear(float[] x, float[] y, float[] arg, float[] interp)`  
进行线性插值。
#### 4.	BilinearTransform
`public static void BilinearTransform(double[] re, double[] im)`  
`public static void BilinearTransform(float[] re, float[] im)`  
进行双线性变换。
#### 5.	Unwrap
`public static double[] Unwrap(double[] phase, double tolerance = Math.PI)`  
`public static float[] Unwrap(float[] phase, float tolerance = ConstantsFp32.PI)`  
`public static T[] Unwrap<T>(T[] phase, T? tolerance = null) where T : unmanaged, IFloatingPointIeee754<T>`  
进行相位展开。
#### 6.	Wrap
`public static double[] Wrap(double[] phase, double tolerance = Math.PI)`  
`public static float[] Wrap(float[] phase, float tolerance = ConstantsFp32.PI)`  
进行相位包裹。
#### 7.	FindNth
`public static float FindNth(float[] a, int n, int start, int end)`  
`public static T FindNth<T>(Span<T> values, int n) where T : INumber<T>`  
查找数组中的第 n 个顺序统计量。
#### 8.	I0
`public static double I0(double x)`  
`public static float I0(float x)`  
`public static T I0<T>(T x) where T : IFloatingPointIeee754<T>`  
`public static Complex<T> I0<T>(Complex<T> x) where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>`  
计算第一类修正贝塞尔函数 I0。
#### 9.	PolynomialRoots
`public static Complex[]? PolynomialRoots(double[] a, int maxIterations = PolyRootsIterations)`  
`public static ComplexFp32[]? PolynomialRoots(float[] a, int maxIterations = PolyRootsIterations)`  
使用 Durand-Kerner 算法计算多项式的复数根。
#### 10.	EvaluatePolynomial
`public static Complex EvaluatePolynomial(double[] a, Complex x)`  
`public static ComplexFp32 EvaluatePolynomial(float[] a, ComplexFp32 x)`  
使用 Horner 方案计算多项式。
#### 11.	MultiplyPolynomials
`public static Complex[] MultiplyPolynomials(Complex[] poly1, Complex[] poly2)`  
`public static ComplexFp32[] MultiplyPolynomials(ComplexFp32[] poly1, ComplexFp32[] poly2)`
乘法多项式。
#### 12.	DividePolynomial
`public static Complex[][] DividePolynomial(Complex[] dividend, Complex[] divisor)`  
`public static ComplexFp32[][] DividePolynomial(ComplexFp32[] dividend, ComplexFp32[] divisor)`  
除法多项式。
#### 13.	Gcd
`public static int Gcd(int n, int m)`  
`public static T Gcd<T>(this T a, T b) where T : IBinaryInteger<T>`  
使用欧几里得算法计算两个整数的最大公约数（GCD）。
#### 14.	Hcf
`public static int Hcf(int a, int b)`  
计算两个整数的最大公约数（HCF），使用递归方法。
#### 15.	Lcm
`public static int Lcm(int a, int b)`  
计算两个整数的最小公倍数（LCM）。
#### 16.	SimplestIntegerRatioOfFraction
`public static (int numerator, int denominator) SimplestIntegerRatioOfFraction(int numerator, int denominator)`  
取分数的最简整数比。
#### 17.	Hypotenuse
`public static double Hypotenuse(double a, double b)`  
`public static float Hypotenuse(float a, float b)`  
计算三角形斜边。
#### 18.	Mod
`public static int Mod(int x, int m)`  
`public static double Mod(double x, double m)`  
`public static float Mod(float x, float m)`  
获取整数或实数的模。
#### 19.	FactorialPower
`public static int FactorialPower(int value, int degree)`  
返回指定值的阶乘幂。
#### 20.	TruncatedPower
`public static double TruncatedPower(double value, double degree)`  
截断幂函数。
#### 21.	InvSqrt
`public static float InvSqrt(float f)`  
快速逆浮点数平方根。
#### 22.	Max
`public static float Max(float a, float b, float c)`  
`public static double Max(double a, double b, double c)`  
获取三个值中的最大值。
#### 23.	Min
`public static float Min(float a, float b, float c)`  
`public static double Min(double a, double b, double c)`  
获取三个值中的最小值。
#### 24.	Pow2
`public static int Pow2(int power)`  
计算 2 的幂。
#### 25.	Log2
`public static int Log2(int x)`  
`public static int Log2_2(int x)`  
获取二进制对数的基数。
#### 26.	Factorial
`public static int Factorial(int n)`  
计算阶乘。
#### 27.	Sqrt
`public static decimal Sqrt(decimal x, decimal epsilon = 0.0M)`  
返回指定 decimal 数的平方根。
#### 28.	Ulp
`public static double Ulp(double value)`  
`ublic static float Ulp(float value)`  
`public static Half Ulp(Half value)`  
计算输入值的最后一位单位（ULP）。
### 代码示例
以下是一个使用 VMath 类中多个方法的示例，并在示例中加入了注释：
```csharp
using System;
using Vorcyc.Mathematics;

public class VMathExample
{
	public static void Main()
	{
		// 计算二项式系数
		int k = 3;
		int n = 5;
		float binomialCoefficient = VMath.BinomialCoefficient(k, n);
		Console.WriteLine($"BinomialCoefficient({k}, {n}) = {binomialCoefficient}");

		// 计算离散差分
		float[] samples = { 1.0f, 2.0f, 4.0f, 7.0f };
		float[] diff = new float[samples.Length];
		VMath.Diff(samples, diff);
		Console.WriteLine($"Diff: {string.Join(", ", diff)}");

		// 进行线性插值
		float[] xValues = { 0.0f, 1.0f, 2.0f };
		float[] yValues = { 0.0f, 1.0f, 4.0f };
		float[] arg = { 0.5f, 1.5f };
		float[] interp = new float[arg.Length];
		VMath.InterpolateLinear(xValues, yValues, arg, interp);
		Console.WriteLine($"InterpolateLinear: {string.Join(", ", interp)}");

		// 进行双线性变换
		double[] re = { 0.5, 0.6 };
		double[] im = { 0.5, 0.6 };
		VMath.BilinearTransform(re, im);
		Console.WriteLine($"BilinearTransform: re = {string.Join(", ", re)}, im = {string.Join(", ", im)}");

		// 进行相位展开
		double[] phase = { 0.0, Math.PI, 2 * Math.PI, 3 * Math.PI };
		double[] unwrappedPhase = VMath.Unwrap(phase);
		Console.WriteLine($"Unwrap: {string.Join(", ", unwrappedPhase)}");

		// 查找数组中的第 n 个顺序统计量
		float[] array = { 3.0f, 1.0f, 4.0f, 1.5f };
		float nthValue = VMath.FindNth(array, 2, 0, array.Length - 1);
		Console.WriteLine($"FindNth: {nthValue}");

		// 计算第一类修正贝塞尔函数 I0
		double x = 1.0;
		double i0Value = VMath.I0(x);
		Console.WriteLine($"I0({x}) = {i0Value}");

		// 使用 Durand-Kerner 算法计算多项式的复数根
		double[] coefficients = { 1.0, -6.0, 11.0, -6.0 };
		var roots = VMath.PolynomialRoots(coefficients);
		Console.WriteLine($"PolynomialRoots: {string.Join(", ", roots)}");

		// 计算多项式
		var complexX = new Complex(1.0, 1.0);
		var polynomialValue = VMath.EvaluatePolynomial(coefficients, complexX);
		Console.WriteLine($"EvaluatePolynomial: {polynomialValue}");

		// 乘法多项式
		var poly1 = new Complex[] { new Complex(1.0, 0.0), new Complex(2.0, 0.0) };
		var poly2 = new Complex[] { new Complex(3.0, 0.0), new Complex(4.0, 0.0) };
		var multipliedPoly = VMath.MultiplyPolynomials(poly1, poly2);
		Console.WriteLine($"MultiplyPolynomials: {string.Join(", ", multipliedPoly)}");

		// 除法多项式
		var dividend = new Complex[] { new Complex(1.0, 0.0), new Complex(2.0, 0.0), new Complex(1.0, 0.0) };
		var divisor = new Complex[] { new Complex(1.0, 0.0), new Complex(1.0, 0.0) };
		var dividedPoly = VMath.DividePolynomial(dividend, divisor);
		Console.WriteLine($"DividePolynomial: quotient = {string.Join(", ", dividedPoly[0])}, remainder = {string.Join(", ", dividedPoly[1])}");

		// 计算最大公约数
		int gcdValue = VMath.Gcd(48, 18);
		Console.WriteLine($"Gcd(48, 18) = {gcdValue}");

		// 计算最小公倍数
		int lcmValue = VMath.Lcm(4, 5);
		Console.WriteLine($"Lcm(4, 5) = {lcmValue}");

		// 计算三角形斜边
		double hypotenuseValue = VMath.Hypotenuse(3.0, 4.0);
		Console.WriteLine($"Hypotenuse(3.0, 4.0) = {hypotenuseValue}");

		// 获取整数的模
		int modValue = VMath.Mod(10, 3);
		Console.WriteLine($"Mod(10, 3) = {modValue}");

		// 计算阶乘
		int factorialValue = VMath.Factorial(5);
		Console.WriteLine($"Factorial(5) = {factorialValue}");

		// 计算平方根
		decimal sqrtValue = VMath.Sqrt(16.0M);
		Console.WriteLine($"Sqrt(16.0) = {sqrtValue}");

		// 计算输入值的最后一位单位（ULP）
		double ulpValue = VMath.Ulp(1.0);
		Console.WriteLine($"Ulp(1.0) = {ulpValue}");
	}
}

```