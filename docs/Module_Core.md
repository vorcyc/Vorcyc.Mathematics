# ����ģ�� - Core Module

> Vorcyc. Mathematics �����ռ�

## Vorcyc.Mathematics.ArrayExtension ��

�����嵥��˵��  
### ͨ�÷���
#### 1.	Copy
- `public static T[] Copy<T>(this T[] source)`  
�����������顣
- `public static T[] Copy<T>(this T[] source, int length)`  
���������ָ�����ȡ�
#### 2.	InitializeArray
- `public static T[]? InitializeArray<T>(int length, T initialValue = default)`  
��ʼ��һ��ָ�����ȵ����飬���ó�ʼֵ��䡣
#### 3.	Fill
- `public static void Fill<T>(this T[] array, T value)`  
��ָ��ֵ����������顣
- `public static void Fill<T>(this T[] array, int start, int end, T value)`  
��ָ��ֵ��������ָ����Χ��
- `public static void Fill<T>(this T[] array, Range range, T value)`  
��ָ��ֵ��������ָ����Χ��
- `public static void Fill<T>(this Span<T> values, T value)`  
��ָ��ֵ���Span&lt;T&gt;��
#### 4.	FillWithRandomNumber
- `public static void FillWithRandomNumber(this Span<float> span)`  
�����������������顣
- `public static void FillWithRandomNumber<T>(this T[] array) where T : IFloatingPointIeee754<T>`  
�������������顣
- `public static void FillWithRandomNumber<T>(this T[] array, Range range) where T : IFloatingPointIeee754<T>`  
���������������ָ����Χ��
- `public static void FillWithRandomNumber(this int[] array, (int max, int min)? limit = null)`  
�������������顣
- `public static void FillWithRandomNumber(this int[] array, Range range)`  
���������������ָ����Χ��
- `public static void FillWithRandomNumber(this long[] array)`  
�������������顣
- `public static void FillWithRandomNumber(this long[] array, Range range)`  
���������������ָ����Χ��
#### 5. GetInner
- `public static IEnumerable<T> GetInner<T>(this T[] array, int start, int length)`
��ȡ������ڲ�Ƭ�Σ�����һ����ö�ٶ���
#### 6. GetInnerArray
- `public static T[] GetInnerArray<T>(this T[] array, int start, int length)`
��ȡ������ڲ�Ƭ�Σ�����һ���µ����顣
#### 7. RemoveSegment
- `public static T[] RemoveSegment<T>(this T[] array, int start, int length)`
�Ƴ������е�һ���֣��������Ƴ���������顣
#### 8. Merge
- `public static T[] Merge<T>(this T[] leading, T[] following)`
�ϲ��������飬���غϲ���������顣
#### 9. ToString
- `public static string ToString<T>(this IEnumerable<T> collection)`
������ת��Ϊ�ַ�����ʾ��
#### 10. FastCopyFragment
- `public static T[] FastCopyFragment<T>(this T[] source, int size, int sourceOffset = 0, int destinationOffset = 0) where T : unmanaged`  
���ٸ��������Ƭ�ε�һ�������顣
#### 11.	FastCopyTo
- `public static void FastCopyTo<T>(this T[] source, T[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
���ٸ��������Ԫ�ص���һ�����顣
#### 12.	Repeat
- `public static T[] Repeat<T>(this T[] source, int n)`  
����һ������Դ�����ظ�ָ�������������顣


### �����ȷ���
#### 13.	FastCopy
- `public static float[] FastCopy(this float[] source)`
����һ�������ȸ�������Ŀ��ٸ�����
#### 14.	FastCopyTo
- `public static void FastCopyTo(this float[] source, float[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
���ٸ��Ƶ����ȸ��������Ԫ�ص���һ�����顣
#### 15.	FastCopyFragment
- `public static float[] FastCopyFragment(this float[] source, int size, int sourceOffset = 0, int destinationOffset = 0)`  
���ٸ��Ƶ����ȸ��������Ƭ�ε�һ�������顣
#### 16.	Merge
- `public static float[] Merge(this float[] source, float[] another)`  
�ϲ����������ȸ������飬���غϲ���������顣
#### 17.	Repeat
`public static float[] Repeat(this float[] source, int n)`
����һ������Դ�����ȸ��������ظ�ָ�������������顣
#### 18.	PadZeros
- `public static float[] PadZeros(this float[] source, int size)`  
��ԭ����Ļ����Ͻ����鲹0��Ŀ�곤�ȣ��Դ���һ��ָ����С������䵥���ȸ������顣
- `public static T[] PadZeros<T>(this T[] source, int size)`  
��ԭ����Ļ����Ͻ����鲹0��Ŀ�곤�ȣ��Դ���һ��ָ����С����������顣

### ˫���ȷ���
#### 19.	FastCopy
- `public static double[] FastCopy(this double[] source)`  
����һ��˫���ȸ�������Ŀ��ٸ�����
#### 20.	FastCopyTo
- `public static void FastCopyTo(this double[] source, double[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
���ٸ���˫���ȸ��������Ԫ�ص���һ�����顣
#### 21.	FastCopyFragment
- `public static double[] FastCopyFragment(this double[] source, int size, int sourceOffset = 0, int destinationOffset = 0)`  
���ٸ���˫���ȸ��������Ƭ�ε�һ�������顣
#### 22.	Merge
- `public static double[] Merge(this double[] source, double[] another)`  
�ϲ�����˫���ȸ������飬���غϲ���������顣
#### 23.	Repeat
- `public static double[] Repeat(this double[] source, int n)`  
����һ������Դ˫���ȸ��������ظ�ָ�������������顣
#### 24.	PadZeros
- `public static double[] PadZeros(this double[] source, int size)`  
����һ��ָ����С�������˫���ȸ������顣

### ��������
#### 25.	ToFloats
- `public static float[] ToFloats(this IEnumerable<double> values)`  
��˫����ֵ�Ŀ�ö�ٶ���ת��Ϊ���������顣
#### 26.	ToDoubles
- `public static double[] ToDoubles(this IEnumerable<float> values)`  
��������ֵ�Ŀ�ö�ٶ���ת��Ϊ˫�������顣
#### 27.	Last
- `public static T Last<T>(this T[] array)`  
��ȡ��������һ��Ԫ�ء�
#### 28.	First
- `public static T First<T>(this T[] array)`  
��ȡ����ĵ�һ��Ԫ�ء�

### ����ʾ��
```csharp
using System;
using System.Linq;
using Vorcyc.Mathematics;

public class ArrayExtensionExample
{
	public static void Main()
	{
		// ����һ����������
		int[] array = { 1, 2, 3, 4, 5 };

		// ��������
		int[] copiedArray = array.Copy();
		Console.WriteLine("Copied Array: " + string.Join(", ", copiedArray));

		// ��ʼ��һ������Ϊ5����ʼֵΪ10������
		int[] initializedArray = ArrayExtension.InitializeArray(5, 10);
		Console.WriteLine("Initialized Array: " + string.Join(", ", initializedArray));

		// ��ָ��ֵ�������
		array.Fill(9);
		Console.WriteLine("Filled Array: " + string.join(", ", array));

		// ��������������
		array.FillWithRandomNumber((max: 100, min: 1));
		Console.WriteLine("Array Filled with Random Numbers: " + string.Join(", ", array));

		// ��ȡ������ڲ�Ƭ��
		int[] innerArray = array.GetInnerArray(1, 3);
		Console.WriteLine("Inner Array: " + string.Join(", ", innerArray));

		// �Ƴ������е�һ����
		int[] removedSegmentArray = array.RemoveSegment(1, 2);
		Console.WriteLine("Array after Removing Segment: " + string.Join(", ", removedSegmentArray));

		// �ϲ���������
		int[] mergedArray = array.Merge(new int[] { 6, 7, 8 });
		Console.WriteLine("Merged Array: " + string.Join(", ", mergedArray));

		// ������ת��Ϊ�ַ���
		string arrayString = array.ToString();
		Console.WriteLine("Array as String: " + arrayString);

		// ���ٸ��������Ƭ��
		int[] fastCopiedFragment = array.FastCopyFragment(3, 1);
		Console.WriteLine("Fast Copied Fragment: " + string.Join(", ", fastCopiedFragment));

		// ����һ������Դ�����ظ�3�ε�������
		int[] repeatedArray = array.Repeat(3);
		Console.WriteLine("Repeated Array: " + string.Join(", ", repeatedArray));

		// ������ָ��ָ�����ȵĶ�
		var segments = array.Split(2);
		Console.WriteLine("Array Segments:");
		foreach (var segment in segments)
		{
			Console.WriteLine(string.Join(", ", segment));
		}

		// ѹ�����鲢��������Ƭ������
		var zippedSegments = array.Zip(3);
		Console.WriteLine("Zipped Array Segments:");
		foreach (var segment in zippedSegments)
		{
			Console.WriteLine(string.Join(", ", segment));
		}

		// ͨ��ƽ��ԭ�����Ƭ�Σ��������ȸ�������ת��Ϊָ�����ȵ�������
		float[] floatArray = { 1.1f, 2.2f, 3.3f, 4.4f, 5.5f };
		float[] transformedArray = floatArray.TransformToArray(3);
		Console.WriteLine("Transformed Array: " + string.Join(", ", transformedArray));
	}
}

```


## Vorcyc.Mathematics.BaseConverter ��

Vorcyc.Mathematics.BaseConverter ��һ����̬�࣬�ṩ�˽�����ʵ���� IBinaryInteger<TSelf> �ӿڵ���������ת��Ϊָ�����Ƶ��ַ�����ʾ�ʹ�ָ�����Ƶ��ַ�����ʾת��Ϊ�����ķ������������������Ҫ���ܣ�
�����嵥��˵��
#### 1.	ToBaseString
- `public static string ToBaseString<TSelf>(this TSelf integer, TSelf baseNumber) where TSelf : IBinaryInteger<TSelf>`
������ʵ���� IBinaryInteger<TSelf> �ӿڵ���������ת��Ϊָ�����Ƶ��ַ�����ʾ�����Ƶķ�Χ��2��94֮�䡣  
- ����:
	- integer: Ҫת����������
	- baseNumber: ��������Ҫ����ڻ����2��С�ڻ����94��
-	����ֵ: �ַ�����ʽ�Ľ�������
-	�쳣:
	1.	ArgumentOutOfRangeException: ������Ϊ���������������2��94֮��ʱ�׳���

#### 2. FromBaseString
- `public static TSelf FromBaseString<TSelf>(this string value, TSelf baseNumber) where TSelf : IBinaryInteger<TSelf>`
- ��ָ�����Ƶ��ַ�����ʾת��Ϊ����ʵ���� IBinaryInteger<TSelf> �ӿڵ��������͡����Ƶķ�Χ��2��94֮�䡣
- ����:
	1.	value: Ҫת�����ַ�����
	2.	baseNumber: ��������Ҫ����ڻ����2��С�ڻ����94��
- ����ֵ: ת�����������
- �쳣:
	1. ArgumentOutOfRangeException: ������������2��94֮����ַ���������Ч�ַ�ʱ�׳���
		
### ����ʾ��
```csharp
using System;
using Vorcyc.Mathematics;

public class BaseConverterExample
{
	public static void Main()
	{
		// ������ת��Ϊָ�����Ƶ��ַ�����ʾ
		int number = 100;
		int baseNumber = 16;
		// ʹ�� ToBaseString ���������� 100 ת��Ϊ 16 ���Ƶ��ַ�����ʾ
		string baseString = number.ToBaseString(baseNumber);
		Console.WriteLine($"Number {number} in base {baseNumber} is: {baseString}");

		// ��ָ�����Ƶ��ַ�����ʾת��Ϊ����
		string value = "64";
		// ʹ�� FromBaseString ������ 16 ���Ƶ��ַ��� "64" ת��Ϊ����
		int convertedNumber = value.FromBaseString(baseNumber);
		Console.WriteLine($"String {value} in base {baseNumber} is: {convertedNumber}");
	}
}
```



## Vorcyc.Mathematics.BitMathExtension ��

Vorcyc.Mathematics.BitMathExtension ��һ����̬�࣬�ṩ�˶�������λ�������չ�������������������Ҫ���ܣ�
### �����嵥��˵��
#### 1.	IsPowerOf2
- public static bool IsPowerOf2(this uint x)
- public static bool IsPowerOf2(this ulong x)
- public static bool IsPowerOf2(this int x)
- public static bool IsPowerOf2(this long x)
- ��֤һ�����Ƿ���2���ݡ�
#### 2.	NextPowerOf2
- public static int NextPowerOf2(this int x)
- public static ulong NextPowerOf2(this ulong value)
- public static uint NextPowerOf2(this uint value)
- ��ȡ��һ��2���ݡ�
#### 3.	PreviousPowerOf2
- public static int PreviousPowerOf2(this int x)
- public static ulong PreviousPowerOf2(this ulong value)
- public static uint PreviousPowerOf2(this uint value)
- ��ȡ��һ��2���ݡ�
#### 4.	CountBitsSet
- public static int CountBitsSet(this uint value)
- public static int CountBitsSet(this ulong value)
- �������õ�λ����
#### 5.	CountBitsCleared
- public static int CountBitsCleared(this uint value)
- public static int CountBitsCleared(this ulong value)
- ����δ���õ�λ����
#### 6.	CreateBitMask
- public static ulong CreateBitMask(this int bitCount)
- ����һ�����и���λ����λ���롣
#### 7.	CountTrailingZeros
- public static int CountTrailingZeros(this uint value)
- public static int CountTrailingZeros(this ulong value)
- ��������λ��ʼ������0�ĸ�����
#### 8.	CountLeadingZeros
- public static int CountLeadingZeros(this uint value)
- public static int CountLeadingZeros(this ulong value)
- ��������λ��ʼ������0�ĸ�����
#### 9.	CountTrailingOnes
- public static int CountTrailingOnes(this uint value)
- public static int CountTrailingOnes(this ulong value)
- ��������λ��ʼ������1�ĸ�����
#### 10.	CountLeadingOnes
- public static int CountLeadingOnes(this uint value)
- public static int CountLeadingOnes(this ulong value)
- ��������λ��ʼ������1�ĸ�����
#### 11.	GetSetBitPositions
- public static IEnumerable<int> GetSetBitPositions(this ulong value)
- public static IEnumerable<int> GetSetBitPositions(this uint value)
- ��������λ��λ�á�
#### 12.	GetClearedBitPositions
- public static IEnumerable<int> GetClearedBitPositions(this uint value)
- public static IEnumerable<int> GetClearedBitPositions(this ulong value)
- ����δ����λ��λ�á�
#### 13.	IsOdd
- public static bool IsOdd(this long value)
- public static bool IsOdd(this ulong value)
- public static bool IsOdd(this int value)
- public static bool IsOdd(this uint value)
- �ж��Ƿ���������
#### 14.	IsEven
- public static bool IsEven(this long value)
- public static bool IsEven(this ulong value)
- public static bool IsEven(this int value)
- public static bool IsEven(this uint value)
- �ж��Ƿ���ż����

### ����ʾ��
������һ��ʹ�� BitMathExtension ���ж��������ʾ��������ʾ���м�����ע�ͣ�

```csharp
using System;
using Vorcyc.Mathematics;

public class BitMathExtensionExample
{
	public static void Main()
	{
		// ��֤һ�����Ƿ���2����
		int number = 16;
		bool isPowerOf2 = number.IsPowerOf2();
		Console.WriteLine($"{number} is power of 2: {isPowerOf2}");

		// ��ȡ��һ��2����
		int nextPowerOf2 = number.NextPowerOf2();
		Console.WriteLine($"Next power of 2 after {number} is: {nextPowerOf2}");

		// ��ȡ��һ��2����
		int previousPowerOf2 = number.PreviousPowerOf2();
		Console.WriteLine($"Previous power of 2 before {number} is: {previousPowerOf2}");

		// �������õ�λ��
		uint value = 29;
		int bitsSet = value.CountBitsSet();
		Console.WriteLine($"Number of bits set in {value} is: {bitsSet}");

		// ����δ���õ�λ��
		int bitsCleared = value.CountBitsCleared();
		Console.WriteLine($"Number of bits cleared in {value} is: {bitsCleared}");

		// ����һ�����и���λ����λ����
		int bitCount = 5;
		ulong bitMask = bitCount.CreateBitMask();
		Console.WriteLine($"Bit mask with {bitCount} bits is: {bitMask}");

		// ��������λ��ʼ������0�ĸ���
		int trailingZeros = value.CountTrailingZeros();
		Console.WriteLine($"Number of trailing zeros in {value} is: {trailingZeros}");

		// ��������λ��ʼ������0�ĸ���
		int leadingZeros = value.CountLeadingZeros();
		Console.WriteLine($"Number of leading zeros in {value} is: {leadingZeros}");

		// �ж��Ƿ�������
		bool isOdd = number.IsOdd();
		Console.WriteLine($"{number} is odd: {isOdd}");

		// �ж��Ƿ���ż��
		bool isEven = number.IsEven();
		Console.WriteLine($"{number} is even: {isEven}");
	}
}

```



## Vorcyc.Mathematics.Combinatorics ��

Vorcyc.Mathematics.Combinatorics ��һ����̬�࣬�ṩ�˶��������ѧ�������������������Ҫ���ܣ�
### �����嵥��˵��
#### 1.	TruthTable
- public static int[][] TruthTable(int length)
- public static int[][] TruthTable(int symbols, int length)
- public static int[][] TruthTable(this int[] symbols)
- �������п��ܵ��������У������ظ�����ֵ����
#### 2.	Sequences
- public static IEnumerable<int[]> Sequences(int length, bool inPlace = false)
- public static IEnumerable<int[]> Sequences(int symbols, int length, bool inPlace = false)
- public static IEnumerable<int[]> Sequences(this int[] symbols, bool inPlace = false)
- �ṩһ�ַ�����ö�����п��ܵ��������У������ظ�����ֵ��������ʹ�ô����ڴ���䡣
#### 3.	Combinations
- public static IEnumerable<T[]> Combinations<T>(this T[] values, bool inPlace = false)
- public static IEnumerable<T[]> Combinations<T>(this T[] values, int k, bool inPlace = false)
- ö�ٸ�����������п���ֵ��ϡ�
#### 4.	Subsets
- public static IEnumerable<SortedSet<T>> Subsets<T>(this ISet<T> set, bool inPlace = false)
- public static IEnumerable<SortedSet<T>> Subsets<T>(this ISet<T> set, int k, bool inPlace = false)
- ���ɸ������ϵ����п����Ӽ���

### ����ʾ��
������һ��ʹ�� Combinatorics ���ж��������ʾ��������ʾ���м�����ע�ͣ�
```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics;

public class CombinatoricsExample
{
	public static void Main()
	{
		// ������ֵ��
		int length = 3;
		int[][] truthTable = Combinatorics.TruthTable(length);
		Console.WriteLine("Truth Table:");
		foreach (var row in truthTable)
		{
			Console.WriteLine(string.Join(", ", row));
		}

		// ö�����п��ܵ���������
		Console.WriteLine("\nSequences:");
		foreach (var sequence in Combinatorics.Sequences(length))
		{
			Console.WriteLine(string.Join(", ", sequence));
		}

		// ö�ٸ�����������п���ֵ���
		int[] values = { 1, 2, 3 };
		Console.WriteLine("\nCombinations:");
		foreach (var combination in Combinatorics.Combinations(values))
		{
			Console.WriteLine(string.Join(", ", combination));
		}

		// ���ɸ������ϵ����п����Ӽ�
		ISet<int> set = new HashSet<int> { 1, 2, 3 };
		Console.WriteLine("\nSubsets:");
		foreach (var subset in Combinatorics.Subsets(set))
		{
			Console.WriteLine(string.Join(", ", subset));
		}
	}
}

```

## Vorcyc.Mathematics.ConstantsFp32 ��

Vorcyc.Mathematics.ConstantsFp32 ��һ����̬�࣬������һ�鳣�õĵ����ȸ�������������Щ��������ѧ�����зǳ����á�������ÿ����������ϸ˵����
�����嵥��˵��
#### 1.	E
- public const float E = 2.71828182845904523536f;
- ��Ȼ�����ĵ��� e��
#### 2.	LOG2E
- public const float LOG2E = 1.44269504088896340736f;
- �� 2 Ϊ�׵� e �Ķ�����
#### 3.	LOG10E
- public const float LOG10E = 0.434294481903251827651f;
- �� 10 Ϊ�׵� e �Ķ�����
#### 4.	LN2
- public const float LN2 = 0.693147180559945309417f;
- 2 ����Ȼ������
#### 5.	LN10
- public const float LN10 = 2.30258509299404568402f;
- 10 ����Ȼ������
#### 6.	PI
- public const float PI = 3.1415926535897932384626433832795f;
- Բ���� �С�
#### 7.	PI_2
- public const float PI_2 = 1.57079632679489661923f;
- �� ��һ�롣
#### 8.	PI_4
- public const float PI_4 = 0.785398163397448309616f;
- �� ���ķ�֮һ��
#### 9.	_1_PI
- public const float _1_PI = 0.318309886183790671538f;
- 1 ���� �С�
#### 10.	_2_PI
- public const float _2_PI = 0.636619772367581343076f;
- 2 ���� �С�
#### 11.	_2_SQRT_PI
- public const float _2_SQRT_PI = 1.12837916709551257390f;
- 2 ���� �� ��ƽ������
#### 12.	_SQRT2
- public const float _SQRT2 = 1.41421356237309504880f;
- 2 ��ƽ������
#### 13.	_SQRT1_2
- public const float _SQRT1_2 = 0.707106781186547524401f;
- 1 ���� 2 ��ƽ������
#### 14.	TWO_PI
- public const float TWO_PI = 6.28318530717958647692f;
- 2 ���� �С�
#### 15.	TOLERANCE
- public const float TOLERANCE = 1E-09f;
- ������ڱȽϸ������ľ��ȡ�
#### 16.	RADIANS_PER_DEGREE
- public const float RADIANS_PER_DEGREE = 0.01745329f;
- ÿ�ȵĻ�������
#### 17.	DEGREES_PER_RADIAN
- public const float DEGREES_PER_RADIAN = 57.29578f;
- ÿ���ȵĶ�����
#### 18.	EPSILON
- public const float EPSILON = 0.001f;
- ���ڸ������Ƚϵļ�Сֵ��
### ����ʾ��
������һ��ʹ�� ConstantsFp32 ���г�����ʾ��������ʾ���м�����ע�ͣ�
```csharp
using System;
using Vorcyc.Mathematics;

public class ConstantsFp32Example
{
	public static void Main()
	{
		// ʹ����Ȼ�����ĵ��� e
		float e = ConstantsFp32.E;
		Console.WriteLine($"e: {e}");

		// ʹ��Բ���� ��
		float pi = ConstantsFp32.PI;
		Console.WriteLine($"��: {pi}");

		// ʹ��ÿ�ȵĻ�����
		float radiansPerDegree = ConstantsFp32.RADIANS_PER_DEGREE;
		Console.WriteLine($"Radians per degree: {radiansPerDegree}");

		// ʹ��ÿ���ȵĶ���
		float degreesPerRadian = ConstantsFp32.DEGREES_PER_RADIAN;
		Console.WriteLine($"Degrees per radian: {degreesPerRadian}");

		// ʹ�ù���
		float tolerance = ConstantsFp32.TOLERANCE;
		Console.WriteLine($"Tolerance: {tolerance}");

		// ʹ�� epsilon
		float epsilon = ConstantsFp32.EPSILON;
		Console.WriteLine($"Epsilon: {epsilon}");
	}
}
```



## Vorcyc.Mathematics.NumberMapper ��
Vorcyc.Mathematics.NumberMapper ��һ����̬�࣬�ṩ�˽�һ��ֵ��һ����Χӳ�䵽��һ����Χ�ķ������������������Ҫ���ܣ�
�����嵥��˵��
#### 1.	Map
- ������ֵ��һ����Χӳ�䵽��һ����Χ��
- `public static TNumber Map<TNumber>(this TNumber number, TNumber inMin, TNumber inMax, TNumber outMin, TNumber outMax, InputValueOutOfRangeHandleBehavior handleBehavior = InputValueOutOfRangeHandleBehavior.Saturating) where TNumber : unmanaged, INumber<TNumber>`
	-   ����:
		1.	number: Ҫӳ�������ֵ��
		2.	inMin: ���뷶Χ����Сֵ��
		3.	inMax: ���뷶Χ�����ֵ��
		4.	outMin: �����Χ����Сֵ��
		5.	outMax: �����Χ�����ֵ��
		6.	handleBehavior: ָ��������ֵ������Χʱ�Ĵ�����Ϊ��Ĭ��ֵΪ InputValueOutOfRangeHandleBehavior.Saturating��
	-	����ֵ: ӳ�䵽�����Χ��ֵ��
	-	�쳣:
		1.	ArgumentOutOfRangeException: �� inMin ���ڻ���� inMax���� outMin ���ڻ���� outMax ʱ�׳���
		2.	ArgumentException: �� number �������뷶Χ�� handleBehavior Ϊ InputValueOutOfRangeHandleBehavior.ThrowException ʱ�׳���
#### 2.	Map
- ����������һ����Χӳ�䵽��һ����Χ��������һ������ֵָʾ�Ƿ�ɹ���
- `public static bool Map<TFloatingNumber>(TFloatingNumber input, TFloatingNumber inputMin, TFloatingNumber inputMax, TFloatingNumber outputMin, TFloatingNumber outputMax, out TFloatingNumber result) where TFloatingNumber : unmanaged, IFloatingPointIeee754<TFloatingNumber>`
	-	����:
		1.	input: Ҫӳ��ĸ�������
		2.	inputMin: ���뷶Χ����Сֵ��
		3.	inputMax: ���뷶Χ�����ֵ��
		4.	outputMin: �����Χ����Сֵ��
		5.	outputMax: �����Χ�����ֵ��
		6.	result: ���ӳ��ɹ�����Ϊӳ���ĸ�����������Ϊ NaN��
	-	����ֵ: ���ӳ��ɹ�����Ϊ true������Ϊ false��
### 3. ö��
	- InputValueOutOfRangeHandleBehavior
	- ���嵱����ֵ����ָ����Χʱ�Ĵ�����Ϊ��	ֵ:
		1.	Saturating: ������ֵ�н�������ı߽�ֵ��
		2.	ThrowException: ������ֵ������Χʱ�׳��쳣��
			
### ����ʾ��
������һ��ʹ�� NumberMapper ���з�����ʾ��������ʾ���м�����ע�ͣ�

```csharp
using System;
using Vorcyc.Mathematics;

public class NumberMapperExample
{
	public static void Main()
	{
		// ��������һ����Χӳ�䵽��һ����Χ
		int number = 5;
		int inMin = 0;
		int inMax = 10;
		int outMin = 0;
		int outMax = 100;
		int mappedValue = number.Map(inMin, inMax, outMin, outMax);
		Console.WriteLine($"Mapped value: {mappedValue}");

		// ����������һ����Χӳ�䵽��һ����Χ��������һ������ֵָʾ�Ƿ�ɹ�
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


## Vorcyc.Mathematics.PinnableArray ��

Vorcyc.Mathematics.PinnableArray ��һ�������࣬��ʾһ���ɹ̶��ڴ��һά���飬�����ṩ�������ڴ���ʻ��ƺ�����Ĳ����������ṩ�˶��ַ������������������飬���ҿ��Խ�����̶����ڴ��У��Ա�����йܴ�����н������������������Ҫ���ܣ�
### �����嵥��˵��
#### 1.	���캯��
- `public PinnableArray(ArraySegment<T> segment, bool pin = false)`
	ʹ�� ArraySegment&lt;T> ��ʼ�� PinnableArray ʵ����
- `public PinnableArray(Span<T> span, bool pin = false)`
	ʹ�� Span&lt;T> ��ʼ�� PinnableArray ʵ����
- `public PinnableArray(T[] array, bool pin = false)`
	ʹ�������ʼ�� PinnableArray ʵ����
- `public PinnableArray(T[] array, int offset, int count, bool pin = false)`
	ʹ������Ĳ������ݳ�ʼ�� PinnableArray ʵ����
- `public PinnableArray(int count, bool pin = false)`
	ʹ��ָ�����ȳ�ʼ�� PinnableArray ʵ����
#### 2.	Pin
-	`public void Pin()`
	�� PinnableArray �̶����ڴ��У��Ա�����йܴ�����н�����
#### 3.	Unpin
-	`public void Unpin()`
	ȡ���̶� PinnableArray��ʹ����Ա��������ա�
#### 4.	AsSpan
-	`public Span<T> AsSpan()`
	���ر�ʾ��ǰ����� Span&lt;T>��
-	`public Span<T> AsSpan(int start, int length)`
	���ر�ʾ��ǰ���鲿�����ݵ� Span&lt;T>��
-	`public Span<T> AsSpan(int start)`
	���ر�ʾ��ǰ�����ָ��λ�ÿ�ʼ�� Span&lt;T>��
-	`public Span<T> AsSpan(System.Index startIndex)`
	���ر�ʾ��ǰ�����ָ��������ʼ�� Span&lt;T>��
-	`public Span<T> AsSpan(Range range)`
	���ر�ʾ��ǰ����ָ����Χ�� Span&lt;T>��
#### 5.	Fill
-	`public void Fill(T value)`
	��ָ��ֵ����������顣
-	`public void Fill(T startValue, T step)`
	��ָ������ʼֵ�Ͳ���������顣
-   `public void FillWith<TNumber>(TNumber number) where TNumber : unmanaged`
	����һ�����й����͵�ֵ������顣
-   `public void FillWithRandomNumber()`
	�������������顣
#### 6.	Each
-   `public void Each(Func<int, T, T> func)`
	ʹ��ָ���ĺ������������ÿ��Ԫ�ء�
-   `public void Each(Func<int, T?, T, T?, T> func, Direction direction = Direction.Forward)`
	ʹ��ָ���ĺ������������ÿ��Ԫ�أ���ָ����������
#### 7.	Max
-   `public T Max()`
	���������е����ֵ��
-   `public T Max(int start, int length)`
	��������ָ����Χ�ڵ����ֵ��
-   `public async Task<T> MaxAsync(int? numberOfWorkers = null, bool useTPL = false)`
	�첽���������е����ֵ��
-   `public async Task<T> MaxAsync(int start, int length, int? numberOfWorkers = null, bool useTPL = false)`
	�첽��������ָ����Χ�ڵ����ֵ��
#### 8.	Min
-   `public T Min()`
	���������е���Сֵ��
-   `public T Min(int start, int length)`
	��������ָ����Χ�ڵ���Сֵ��
- 	`public async Task<T> MinAsync(int? numberOfWorkers = null, bool useTPL = false)`
	�첽���������е���Сֵ���÷���ͨ������ִ�У���Դ��ģ���Ч�����ѡ�
-   `public async Task<T> MinAsync(int start, int length, int? numberOfWorkers = null, bool useTPL = false)`
	�첽��������ָ����Χ�ڵ���Сֵ���÷���ͨ������ִ�У���Դ��ģ���Ч�����ѡ�
#### 9.	Map
-    `public void MapIn(T fromMin, T fromMax, T toMin, T toMax)`
	�������е�ֵ��һ����Χӳ�䵽��һ����Χ��
-   `public void MapIn(T toMin, T toMax)`
	�������е�ֵ�ӵ�ǰ��Χӳ�䵽ָ����Χ��
-    `public PinnableArray<T> Map(T fromMin, T fromMax, T toMin, T toMax)`
	����һ���µ� PinnableArray�����е�ֵ��һ����Χӳ�䵽��һ����Χ��
-    `public PinnableArray<T> Map(T toMin, T toMax)`
	����һ���µ� PinnableArray�����е�ֵ�ӵ�ǰ��Χӳ�䵽ָ����Χ��
#### 10.	Dot
-    `public T Dot(PinnableArray<T> another)`
	���㵱ǰ��������һ�� PinnableArray �ĵ����
#### 11.	Dispose
-    `public void Dispose()`
	�ͷ� PinnableArray ռ�õ���Դ��
### ö��
#### 12.	`Direction`
-    �����������ķ���
-    ֵ:
	1.	`Forward`: ��ǰ��������
	2.	`Inverse`: �Ӻ���ǰ������
	
### ����  
#### 13.	IsPinned
-    `public bool IsPinned { get; }`
	��ȡ��ǰ�����Ƿ��ѹ̶����ڴ��С�
#### 14.	Length
-    `public int Length { get; }`
	��ȡ��ǰ����ĳ��ȡ�
#### 15.	Values
-    `public T[] Values { get; }`
	��ȡ��ǰ�������ͨ������ʽ��
### ����������
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

#### ����ʾ��
������һ��ʹ�� PinnableArray ���ж��������ʾ��������ʾ���м�����ע�ͣ�
```csharp
using System;
using System.Threading.Tasks;
using Vorcyc.Mathematics;

public class PinnableArrayExample
{
	public static async Task Main()
	{
		// ʹ�������ʼ�� PinnableArray ʵ��
		int[] array = { 1, 2, 3, 4, 5 };
		PinnableArray<int> pinnableArray = new PinnableArray<int>(array);

		// ������̶����ڴ���
		pinnableArray.Pin();
		Console.WriteLine($"IsPinned: {pinnableArray.IsPinned}");

		// ��ȡ����ĳ���
		int length = pinnableArray.Length;
		Console.WriteLine($"Length: {length}");

		// ��ȡ�������ͨ������ʽ
		int[] values = pinnableArray;
		Console.WriteLine($"Values: {string.Join(", ", values)}");

		// ��ָ��ֵ�����������
		pinnableArray.Fill(10);
		Console.WriteLine($"Filled Values: {string.Join(", ", pinnableArray.Values)}");

		// ��ָ������ʼֵ�Ͳ����������
		pinnableArray.Fill(1, 2);
		Console.WriteLine($"Filled with step Values: {string.Join(", ", pinnableArray.Values)}");

		// ��������������
		pinnableArray.FillWithRandomNumber();
		Console.WriteLine($"Filled with random numbers: {string.Join(", ", pinnableArray.Values)}");

		// ���������ÿ��Ԫ�ز����в���
		pinnableArray.Each((index, value) => value * 2);
		Console.WriteLine($"Each element multiplied by 2: {string.Join(", ", pinnableArray.Values)}");

		// ��ȡ�����е����ֵ
		int maxValue = pinnableArray.Max();
		Console.WriteLine($"Max value: {maxValue}");

		// ��ȡ�����е���Сֵ
		int minValue = pinnableArray.Min();
		Console.WriteLine($"Min value: {minValue}");

		// �첽��ȡ�����е����ֵ
		int maxAsyncValue = await pinnableArray.MaxAsync();
		Console.WriteLine($"Max async value: {maxAsyncValue}");

		// �첽��ȡ�����е���Сֵ
		int minAsyncValue = await pinnableArray.MinAsync();
		Console.WriteLine($"Min async value: {minAsyncValue}");

		// �������е�ֵ��һ����Χӳ�䵽��һ����Χ
		pinnableArray.MapIn(0, 1, 0, 100);
		Console.WriteLine($"Mapped values: {string.Join(", ", pinnableArray.Values)}");

		// ���㵱ǰ��������һ�� PinnableArray �ĵ��
		PinnableArray<int> anotherArray = new PinnableArray<int>(new int[] { 1, 2, 3, 4, 5 });
		int dotProduct = pinnableArray.Dot(anotherArray);
		Console.WriteLine($"Dot product: {dotProduct}");

		// ʹ�����������ʺ��޸�����Ԫ��
		pinnableArray[0] = 42;
		Console.WriteLine($"First element after modification: {pinnableArray[0]}");

		// ��ȡ����� Span ��ʾ
		Span<int> span = pinnableArray.AsSpan();
		Console.WriteLine($"Span values: {string.Join(", ", span.ToArray())}");

		// ȡ���̶�����
		pinnableArray.Unpin();
		Console.WriteLine($"IsPinned: {pinnableArray.IsPinned}");

		// �ͷ���Դ
		pinnableArray.Dispose();
	}
}
```


## SimpleRNG_Fp32 �� SimpleRNG_Fp64 ��

Vorcyc.Mathematics.SimpleRNG_Fp32 �� Vorcyc.Mathematics.SimpleRNG_Fp64 ��������̬�࣬�ֱ��������� 32 λ�� 64 λ������������������ǻ��� George Marsaglia �� MWC���˷����λ���㷨���ܹ�����ͨ�� Marsaglia �� DIEHARD ��������������Ե��������
### �����嵥��˵��
#### 1. SetSeed
- `public static void SetSeed(uint u, uint v)`
  - ��������������������ӡ�
  - ����:
	- `u`: ��һ������ֵ��
	- `v`: �ڶ�������ֵ��

- `public static void SetSeed(uint u)`
  - ��������������������ӡ�
  - ����:
	- `u`: ����ֵ��

- `public static void SetSeedFromSystemTime()`
  - ʹ��ϵͳʱ����������������������ӡ�

#### 2. GetUniform
- `public static double GetUniform()`
  - ����һ�� (0, 1) �����ڵľ��ȷֲ��������

#### 3. GetNormal
- `public static double GetNormal()`
  - ����һ����ֵΪ 0����׼��Ϊ 1 ����̬�ֲ��������

- `public static double GetNormal(double mean, double standardDeviation)`
  - ����һ��ָ����ֵ�ͱ�׼�����̬�ֲ��������
  - ����:
	- `mean`: ��ֵ��
	- `standardDeviation`: ��׼�

#### 4. GetExponential
- `public static double GetExponential()`
  - ����һ����ֵΪ 1 ��ָ���ֲ��������

- `public static double GetExponential(double mean)`
  - ����һ��ָ����ֵ��ָ���ֲ��������
  - ����:
	- `mean`: ��ֵ��

#### 5. GetGamma
- `public static double GetGamma(double shape, double scale)`
  - ����һ��ָ����״�ͳ߶Ȳ�����٤��ֲ��������
  - ����:
	- `shape`: ��״������
	- `scale`: �߶Ȳ�����

#### 6. GetChiSquare
- `public static double GetChiSquare(double degreesOfFreedom)`
  - ����һ��ָ�����ɶȵĿ����ֲ��������
  - ����:
	- `degreesOfFreedom`: ���ɶȡ�

#### 7. GetInverseGamma
- `public static double GetInverseGamma(double shape, double scale)`
  - ����һ��ָ����״�ͳ߶Ȳ�������٤��ֲ��������
  - ����:
	- `shape`: ��״������
	- `scale`: �߶Ȳ�����

#### 8. GetWeibull
- `public static double GetWeibull(double shape, double scale)`
  - ����һ��ָ����״�ͳ߶Ȳ������������ֲ��������
  - ����:
	- `shape`: ��״������
	- `scale`: �߶Ȳ�����

#### 9. GetCauchy
- `public static double GetCauchy(double median, double scale)`
  - ����һ��ָ����λ���ͳ߶Ȳ����Ŀ����ֲ��������
  - ����:
	- `median`: ��λ����
	- `scale`: �߶Ȳ�����

#### 10. GetStudentT
- `public static double GetStudentT(double degreesOfFreedom)`
  - ����һ��ָ�����ɶȵ� t �ֲ��������
  - ����:
	- `degreesOfFreedom`: ���ɶȡ�

#### 11. GetLaplace
- `public static double GetLaplace(double mean, double scale)`
  - ����һ��ָ����ֵ�ͳ߶Ȳ�����������˹�ֲ���˫ָ���ֲ����������
  - ����:
	- `mean`: ��ֵ��
	- `scale`: �߶Ȳ�����

#### 12. GetLogNormal
- `public static double GetLogNormal(double mu, double sigma)`
  - ����һ��������̬�ֲ��������
  - ����:
	- `mu`: ������̬�ֲ��ľ�ֵ��
	- `sigma`: ������̬�ֲ��ı�׼�

#### 13. GetBeta
- `public static double GetBeta(double a, double b)`
  - ����һ��ָ�������ı����ֲ��������
  - ����:
	- `a`: �����ֲ��ĵ�һ��������
	- `b`: �����ֲ��ĵڶ���������
### ����ʾ��
������һ��ʹ�� SimpleRNG_Fp32 �� SimpleRNG_Fp64 ���ж��������ʾ��������ʾ���м�����ע�ͣ�
```csharp
using System;
using Vorcyc.Mathematics;

public class SimpleRNGExample
{
	public static void Main()
	{
		// ʹ�� SimpleRNG_Fp64 ���������
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

		// ʹ�� SimpleRNG_Fp32 ���������
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


## Vorcyc.Mathematics.TrigonometryHelper ��
Vorcyc.Mathematics.TrigonometryHelper ��һ����̬�࣬�ṩ�˶���������������ĸ����������������������Ҫ���ܣ�
### �����嵥��˵��
#### 1.	RadiansToDegrees
`public static double RadiansToDegrees(double a)`  
`public static float RadiansToDegrees(float radians)`  
���Ƕȴӻ���ת��Ϊ�ȡ�
#### 2.	DegreesToRadians
`public static double DegreesToRadians(double a)`  
`public static float DegreesToRadians(float degrees)`  
���ǶȴӶ�ת��Ϊ���ȡ�
#### 3.	GetAngleDifference
`public static float GetAngleDifference(float radianAngle1, float radianAngle2)`  
��ȡ�����Ƕ�֮��Ĳ�ֵ����λΪ���ȡ�
#### 4.	RadianMin
`public static float RadianMin(float radianAngle)`
`public static double RadianMin(double radianAngle)`
���Ƕȼ��ٵ����Ч�� -�� �� �� ֮�䡣
#### 5.	Angle
`public static float Angle(float x, float y)`  
`public static double Angle(double x, double y)`  
��ȡ������ [x,y] �γɵĽǶȣ���λΪ���ȡ�
#### 6.	Acosh
`public static double Acosh(double x)`  
`public static float Acosh(float x)`  
����ָ��ֵ��˫�����ҡ�
#### 7.	Asinh
`public static double Asinh(double d)`  
`public static float Asinh(float d)`  
`public static T Asinh<T>(T x) where T : struct, IFloatingPointIeee754<T>`  
����ָ��ֵ��˫�����ҡ�
#### 8.	Atanh
`public static double Atanh(double d)`  
`public static float Atanh(float d)`  
����ָ��ֵ��˫�����С�
#### 9.	Sinc
`public static double Sinc(double x)`  
`public static float Sinc(float x)`  
`public static T Sinc<T>(T x) where T : IFloatingPointIeee754<T>`  
���� Sinc ������
### ����ʾ��
������һ��ʹ�� TrigonometryHelper ���ж��������ʾ��������ʾ���м�����ע�ͣ�
```csharp
using System;
using Vorcyc.Mathematics;

public class TrigonometryHelperExample
{
	public static void Main()
	{
		// ���Ƕȴӻ���ת��Ϊ��
		double radians = Math.PI;
		double degrees = TrigonometryHelper.RadiansToDegrees(radians);
		Console.WriteLine($"Radians: {radians}, Degrees: {degrees}");

		// ���ǶȴӶ�ת��Ϊ����
		degrees = 180.0;
		radians = TrigonometryHelper.DegreesToRadians(degrees);
		Console.WriteLine($"Degrees: {degrees}, Radians: {radians}");

		// ��ȡ�����Ƕ�֮��Ĳ�ֵ����λΪ����
		float angle1 = 1.0f;
		float angle2 = 2.0f;
		float angleDifference = TrigonometryHelper.GetAngleDifference(angle1, angle2);
		Console.WriteLine($"Angle1: {angle1}, Angle2: {angle2}, Difference: {angleDifference}");

		// ���Ƕȼ��ٵ����Ч�� -�� �� �� ֮��
		float radianAngle = 4.0f;
		float reducedAngle = TrigonometryHelper.RadianMin(radianAngle);
		Console.WriteLine($"Radian Angle: {radianAngle}, Reduced Angle: {reducedAngle}");

		// ��ȡ������ [x,y] �γɵĽǶȣ���λΪ����
		float x = 1.0f;
		float y = 1.0f;
		float vectorAngle = TrigonometryHelper.Angle(x, y);
		Console.WriteLine($"Vector [x,y]: [{x},{y}], Angle: {vectorAngle}");

		// ����ָ��ֵ��˫������
		double value = 2.0;
		double acoshValue = TrigonometryHelper.Acosh(value);
		Console.WriteLine($"Value: {value}, Acosh: {acoshValue}");

		// ����ָ��ֵ��˫������
		value = 1.0;
		double asinhValue = TrigonometryHelper.Asinh(value);
		Console.WriteLine($"Value: {value}, Asinh: {asinhValue}");

		// ����ָ��ֵ��˫������
		value = 0.5;
		double atanhValue = TrigonometryHelper.Atanh(value);
		Console.WriteLine($"Value: {value}, Atanh: {atanhValue}");

		// ���� Sinc ����
		double sincValue = TrigonometryHelper.Sinc(value);
		Console.WriteLine($"Sinc({value}) = {sincValue}");
	}
}

```


## VMath ��
Vorcyc.Mathematics.VMath ��һ����̬�࣬�ṩ�˶�����ѧ�����Ͳ������������������Ҫ���ܣ�
### �����嵥��˵��
#### 1.	BinomialCoefficient
`public static float BinomialCoefficient(int k, int n)`  
�������ʽϵ����
#### 2.	Diff
`public static void Diff(float[] samples, float[] diff)`  
������ɢ��֡�
#### 3.	InterpolateLinear
`public static void InterpolateLinear(float[] x, float[] y, float[] arg, float[] interp)`  
�������Բ�ֵ��
#### 4.	BilinearTransform
`public static void BilinearTransform(double[] re, double[] im)`  
`public static void BilinearTransform(float[] re, float[] im)`  
����˫���Ա任��
#### 5.	Unwrap
`public static double[] Unwrap(double[] phase, double tolerance = Math.PI)`  
`public static float[] Unwrap(float[] phase, float tolerance = ConstantsFp32.PI)`  
`public static T[] Unwrap<T>(T[] phase, T? tolerance = null) where T : unmanaged, IFloatingPointIeee754<T>`  
������λչ����
#### 6.	Wrap
`public static double[] Wrap(double[] phase, double tolerance = Math.PI)`  
`public static float[] Wrap(float[] phase, float tolerance = ConstantsFp32.PI)`  
������λ������
#### 7.	FindNth
`public static float FindNth(float[] a, int n, int start, int end)`  
`public static T FindNth<T>(Span<T> values, int n) where T : INumber<T>`  
���������еĵ� n ��˳��ͳ������
#### 8.	I0
`public static double I0(double x)`  
`public static float I0(float x)`  
`public static T I0<T>(T x) where T : IFloatingPointIeee754<T>`  
`public static Complex<T> I0<T>(Complex<T> x) where T : struct, INumberBase<T>, IFloatingPointIeee754<T>, IMinMaxValue<T>`  
�����һ���������������� I0��
#### 9.	PolynomialRoots
`public static Complex[]? PolynomialRoots(double[] a, int maxIterations = PolyRootsIterations)`  
`public static ComplexFp32[]? PolynomialRoots(float[] a, int maxIterations = PolyRootsIterations)`  
ʹ�� Durand-Kerner �㷨�������ʽ�ĸ�������
#### 10.	EvaluatePolynomial
`public static Complex EvaluatePolynomial(double[] a, Complex x)`  
`public static ComplexFp32 EvaluatePolynomial(float[] a, ComplexFp32 x)`  
ʹ�� Horner �����������ʽ��
#### 11.	MultiplyPolynomials
`public static Complex[] MultiplyPolynomials(Complex[] poly1, Complex[] poly2)`  
`public static ComplexFp32[] MultiplyPolynomials(ComplexFp32[] poly1, ComplexFp32[] poly2)`
�˷�����ʽ��
#### 12.	DividePolynomial
`public static Complex[][] DividePolynomial(Complex[] dividend, Complex[] divisor)`  
`public static ComplexFp32[][] DividePolynomial(ComplexFp32[] dividend, ComplexFp32[] divisor)`  
��������ʽ��
#### 13.	Gcd
`public static int Gcd(int n, int m)`  
`public static T Gcd<T>(this T a, T b) where T : IBinaryInteger<T>`  
ʹ��ŷ������㷨�����������������Լ����GCD����
#### 14.	Hcf
`public static int Hcf(int a, int b)`  
�����������������Լ����HCF����ʹ�õݹ鷽����
#### 15.	Lcm
`public static int Lcm(int a, int b)`  
����������������С��������LCM����
#### 16.	SimplestIntegerRatioOfFraction
`public static (int numerator, int denominator) SimplestIntegerRatioOfFraction(int numerator, int denominator)`  
ȡ��������������ȡ�
#### 17.	Hypotenuse
`public static double Hypotenuse(double a, double b)`  
`public static float Hypotenuse(float a, float b)`  
����������б�ߡ�
#### 18.	Mod
`public static int Mod(int x, int m)`  
`public static double Mod(double x, double m)`  
`public static float Mod(float x, float m)`  
��ȡ������ʵ����ģ��
#### 19.	FactorialPower
`public static int FactorialPower(int value, int degree)`  
����ָ��ֵ�Ľ׳��ݡ�
#### 20.	TruncatedPower
`public static double TruncatedPower(double value, double degree)`  
�ض��ݺ�����
#### 21.	InvSqrt
`public static float InvSqrt(float f)`  
�����渡����ƽ������
#### 22.	Max
`public static float Max(float a, float b, float c)`  
`public static double Max(double a, double b, double c)`  
��ȡ����ֵ�е����ֵ��
#### 23.	Min
`public static float Min(float a, float b, float c)`  
`public static double Min(double a, double b, double c)`  
��ȡ����ֵ�е���Сֵ��
#### 24.	Pow2
`public static int Pow2(int power)`  
���� 2 ���ݡ�
#### 25.	Log2
`public static int Log2(int x)`  
`public static int Log2_2(int x)`  
��ȡ�����ƶ����Ļ�����
#### 26.	Factorial
`public static int Factorial(int n)`  
����׳ˡ�
#### 27.	Sqrt
`public static decimal Sqrt(decimal x, decimal epsilon = 0.0M)`  
����ָ�� decimal ����ƽ������
#### 28.	Ulp
`public static double Ulp(double value)`  
`ublic static float Ulp(float value)`  
`public static Half Ulp(Half value)`  
��������ֵ�����һλ��λ��ULP����
### ����ʾ��
������һ��ʹ�� VMath ���ж��������ʾ��������ʾ���м�����ע�ͣ�
```csharp
using System;
using Vorcyc.Mathematics;

public class VMathExample
{
	public static void Main()
	{
		// �������ʽϵ��
		int k = 3;
		int n = 5;
		float binomialCoefficient = VMath.BinomialCoefficient(k, n);
		Console.WriteLine($"BinomialCoefficient({k}, {n}) = {binomialCoefficient}");

		// ������ɢ���
		float[] samples = { 1.0f, 2.0f, 4.0f, 7.0f };
		float[] diff = new float[samples.Length];
		VMath.Diff(samples, diff);
		Console.WriteLine($"Diff: {string.Join(", ", diff)}");

		// �������Բ�ֵ
		float[] xValues = { 0.0f, 1.0f, 2.0f };
		float[] yValues = { 0.0f, 1.0f, 4.0f };
		float[] arg = { 0.5f, 1.5f };
		float[] interp = new float[arg.Length];
		VMath.InterpolateLinear(xValues, yValues, arg, interp);
		Console.WriteLine($"InterpolateLinear: {string.Join(", ", interp)}");

		// ����˫���Ա任
		double[] re = { 0.5, 0.6 };
		double[] im = { 0.5, 0.6 };
		VMath.BilinearTransform(re, im);
		Console.WriteLine($"BilinearTransform: re = {string.Join(", ", re)}, im = {string.Join(", ", im)}");

		// ������λչ��
		double[] phase = { 0.0, Math.PI, 2 * Math.PI, 3 * Math.PI };
		double[] unwrappedPhase = VMath.Unwrap(phase);
		Console.WriteLine($"Unwrap: {string.Join(", ", unwrappedPhase)}");

		// ���������еĵ� n ��˳��ͳ����
		float[] array = { 3.0f, 1.0f, 4.0f, 1.5f };
		float nthValue = VMath.FindNth(array, 2, 0, array.Length - 1);
		Console.WriteLine($"FindNth: {nthValue}");

		// �����һ���������������� I0
		double x = 1.0;
		double i0Value = VMath.I0(x);
		Console.WriteLine($"I0({x}) = {i0Value}");

		// ʹ�� Durand-Kerner �㷨�������ʽ�ĸ�����
		double[] coefficients = { 1.0, -6.0, 11.0, -6.0 };
		var roots = VMath.PolynomialRoots(coefficients);
		Console.WriteLine($"PolynomialRoots: {string.Join(", ", roots)}");

		// �������ʽ
		var complexX = new Complex(1.0, 1.0);
		var polynomialValue = VMath.EvaluatePolynomial(coefficients, complexX);
		Console.WriteLine($"EvaluatePolynomial: {polynomialValue}");

		// �˷�����ʽ
		var poly1 = new Complex[] { new Complex(1.0, 0.0), new Complex(2.0, 0.0) };
		var poly2 = new Complex[] { new Complex(3.0, 0.0), new Complex(4.0, 0.0) };
		var multipliedPoly = VMath.MultiplyPolynomials(poly1, poly2);
		Console.WriteLine($"MultiplyPolynomials: {string.Join(", ", multipliedPoly)}");

		// ��������ʽ
		var dividend = new Complex[] { new Complex(1.0, 0.0), new Complex(2.0, 0.0), new Complex(1.0, 0.0) };
		var divisor = new Complex[] { new Complex(1.0, 0.0), new Complex(1.0, 0.0) };
		var dividedPoly = VMath.DividePolynomial(dividend, divisor);
		Console.WriteLine($"DividePolynomial: quotient = {string.Join(", ", dividedPoly[0])}, remainder = {string.Join(", ", dividedPoly[1])}");

		// �������Լ��
		int gcdValue = VMath.Gcd(48, 18);
		Console.WriteLine($"Gcd(48, 18) = {gcdValue}");

		// ������С������
		int lcmValue = VMath.Lcm(4, 5);
		Console.WriteLine($"Lcm(4, 5) = {lcmValue}");

		// ����������б��
		double hypotenuseValue = VMath.Hypotenuse(3.0, 4.0);
		Console.WriteLine($"Hypotenuse(3.0, 4.0) = {hypotenuseValue}");

		// ��ȡ������ģ
		int modValue = VMath.Mod(10, 3);
		Console.WriteLine($"Mod(10, 3) = {modValue}");

		// ����׳�
		int factorialValue = VMath.Factorial(5);
		Console.WriteLine($"Factorial(5) = {factorialValue}");

		// ����ƽ����
		decimal sqrtValue = VMath.Sqrt(16.0M);
		Console.WriteLine($"Sqrt(16.0) = {sqrtValue}");

		// ��������ֵ�����һλ��λ��ULP��
		double ulpValue = VMath.Ulp(1.0);
		Console.WriteLine($"Ulp(1.0) = {ulpValue}");
	}
}

```