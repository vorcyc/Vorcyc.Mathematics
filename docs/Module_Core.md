# ����ģ�� - Core Module

> Vorcyc. Mathematics �����ռ�

## Vorcyc.Mathematics.ArrayExtension ��

�����嵥��˵��  
## ͨ�÷���
### 1.	Copy
- `public static T[] Copy<T>(this T[] source)`  
�����������顣
- `public static T[] Copy<T>(this T[] source, int length)`  
���������ָ�����ȡ�
### 2.	InitializeArray
- `public static T[]? InitializeArray<T>(int length, T initialValue = default)`  
��ʼ��һ��ָ�����ȵ����飬���ó�ʼֵ��䡣
### 3.	Fill
- `public static void Fill<T>(this T[] array, T value)`  
��ָ��ֵ����������顣
- `public static void Fill<T>(this T[] array, int start, int end, T value)`  
��ָ��ֵ��������ָ����Χ��
- `public static void Fill<T>(this T[] array, Range range, T value)`  
��ָ��ֵ��������ָ����Χ��
- `public static void Fill<T>(this Span<T> values, T value)`  
��ָ��ֵ���Span&lt;T&gt;��
### 4.	FillWithRandomNumber
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
### 5. GetInner
- `public static IEnumerable<T> GetInner<T>(this T[] array, int start, int length)`
��ȡ������ڲ�Ƭ�Σ�����һ����ö�ٶ���
### 6. GetInnerArray
- `public static T[] GetInnerArray<T>(this T[] array, int start, int length)`
��ȡ������ڲ�Ƭ�Σ�����һ���µ����顣
### 7. RemoveSegment
- `public static T[] RemoveSegment<T>(this T[] array, int start, int length)`
�Ƴ������е�һ���֣��������Ƴ���������顣
### 8. Merge
- `public static T[] Merge<T>(this T[] leading, T[] following)`
�ϲ��������飬���غϲ���������顣
### 9. ToString
- `public static string ToString<T>(this IEnumerable<T> collection)`
������ת��Ϊ�ַ�����ʾ��
### 10. FastCopyFragment
- `public static T[] FastCopyFragment<T>(this T[] source, int size, int sourceOffset = 0, int destinationOffset = 0) where T : unmanaged`  
���ٸ��������Ƭ�ε�һ�������顣
### 11.	FastCopyTo
- `public static void FastCopyTo<T>(this T[] source, T[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
���ٸ��������Ԫ�ص���һ�����顣
### 12.	Repeat
- `public static T[] Repeat<T>(this T[] source, int n)`  
����һ������Դ�����ظ�ָ�������������顣


## �����ȷ���
### 13.	FastCopy
- `public static float[] FastCopy(this float[] source)`
����һ�������ȸ�������Ŀ��ٸ�����
### 14.	FastCopyTo
- `public static void FastCopyTo(this float[] source, float[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
���ٸ��Ƶ����ȸ��������Ԫ�ص���һ�����顣
### 15.	FastCopyFragment
- `public static float[] FastCopyFragment(this float[] source, int size, int sourceOffset = 0, int destinationOffset = 0)`  
���ٸ��Ƶ����ȸ��������Ƭ�ε�һ�������顣
### 16.	Merge
- `public static float[] Merge(this float[] source, float[] another)`  
�ϲ����������ȸ������飬���غϲ���������顣
### 17.	Repeat
`public static float[] Repeat(this float[] source, int n)`
����һ������Դ�����ȸ��������ظ�ָ�������������顣
### 18.	PadZeros
- `public static float[] PadZeros(this float[] source, int size)`  
��ԭ����Ļ����Ͻ����鲹0��Ŀ�곤�ȣ��Դ���һ��ָ����С������䵥���ȸ������顣
- `public static T[] PadZeros<T>(this T[] source, int size)`  
��ԭ����Ļ����Ͻ����鲹0��Ŀ�곤�ȣ��Դ���һ��ָ����С����������顣

## ˫���ȷ���
### 19.	FastCopy
- `public static double[] FastCopy(this double[] source)`  
����һ��˫���ȸ�������Ŀ��ٸ�����
### 20.	FastCopyTo
- `public static void FastCopyTo(this double[] source, double[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)`  
���ٸ���˫���ȸ��������Ԫ�ص���һ�����顣
### 21.	FastCopyFragment
- `public static double[] FastCopyFragment(this double[] source, int size, int sourceOffset = 0, int destinationOffset = 0)`  
���ٸ���˫���ȸ��������Ƭ�ε�һ�������顣
### 22.	Merge
- `public static double[] Merge(this double[] source, double[] another)`  
�ϲ�����˫���ȸ������飬���غϲ���������顣
### 23.	Repeat
- `public static double[] Repeat(this double[] source, int n)`  
����һ������Դ˫���ȸ��������ظ�ָ�������������顣
### 24.	PadZeros
- `public static double[] PadZeros(this double[] source, int size)`  
����һ��ָ����С�������˫���ȸ������顣

## ��������
### 25.	ToFloats
- `public static float[] ToFloats(this IEnumerable<double> values)`  
��˫����ֵ�Ŀ�ö�ٶ���ת��Ϊ���������顣
### 26.	ToDoubles
- `public static double[] ToDoubles(this IEnumerable<float> values)`  
��������ֵ�Ŀ�ö�ٶ���ת��Ϊ˫�������顣
### 27.	Last
- `public static T Last<T>(this T[] array)`  
��ȡ��������һ��Ԫ�ء�
### 28.	First
- `public static T First<T>(this T[] array)`  
��ȡ����ĵ�һ��Ԫ�ء�

����ʾ��
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
1.	`ToBaseString`
	- public static string ToBaseString<TSelf>(this TSelf integer, TSelf baseNumber) where TSelf : IBinaryInteger<TSelf>  
	������ʵ���� IBinaryInteger<TSelf> �ӿڵ���������ת��Ϊָ�����Ƶ��ַ�����ʾ�����Ƶķ�Χ��2��94֮�䡣  
	- ����:
		- integer: Ҫת����������
		- baseNumber: ��������Ҫ����ڻ����2��С�ڻ����94��
	-	����ֵ: �ַ�����ʽ�Ľ�������
	-	�쳣:
		1.	ArgumentOutOfRangeException: ������Ϊ���������������2��94֮��ʱ�׳���

2. `FromBaseString`
	- public static TSelf FromBaseString<TSelf>(this string value, TSelf baseNumber) where TSelf : IBinaryInteger<TSelf>
	- ��ָ�����Ƶ��ַ�����ʾת��Ϊ����ʵ���� IBinaryInteger<TSelf> �ӿڵ��������͡����Ƶķ�Χ��2��94֮�䡣
	- ����:
		1.	value: Ҫת�����ַ�����
		2.	baseNumber: ��������Ҫ����ڻ����2��С�ڻ����94��
	- ����ֵ: ת�����������
	- �쳣:
		1. ArgumentOutOfRangeException: ������������2��94֮����ַ���������Ч�ַ�ʱ�׳���
		
- ����ʾ��
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
�����嵥��˵��
### 1.	IsPowerOf2
- public static bool IsPowerOf2(this uint x)
- public static bool IsPowerOf2(this ulong x)
- public static bool IsPowerOf2(this int x)
- public static bool IsPowerOf2(this long x)
- ��֤һ�����Ƿ���2���ݡ�
### 2.	NextPowerOf2
- public static int NextPowerOf2(this int x)
- public static ulong NextPowerOf2(this ulong value)
- public static uint NextPowerOf2(this uint value)
- ��ȡ��һ��2���ݡ�
### 3.	PreviousPowerOf2
- public static int PreviousPowerOf2(this int x)
- public static ulong PreviousPowerOf2(this ulong value)
- public static uint PreviousPowerOf2(this uint value)
- ��ȡ��һ��2���ݡ�
### 4.	CountBitsSet
- public static int CountBitsSet(this uint value)
- public static int CountBitsSet(this ulong value)
- �������õ�λ����
### 5.	CountBitsCleared
- public static int CountBitsCleared(this uint value)
- public static int CountBitsCleared(this ulong value)
- ����δ���õ�λ����
### 6.	CreateBitMask
- public static ulong CreateBitMask(this int bitCount)
- ����һ�����и���λ����λ���롣
### 7.	CountTrailingZeros
- public static int CountTrailingZeros(this uint value)
- public static int CountTrailingZeros(this ulong value)
- ��������λ��ʼ������0�ĸ�����
### 8.	CountLeadingZeros
- public static int CountLeadingZeros(this uint value)
- public static int CountLeadingZeros(this ulong value)
- ��������λ��ʼ������0�ĸ�����
### 9.	CountTrailingOnes
- public static int CountTrailingOnes(this uint value)
- public static int CountTrailingOnes(this ulong value)
- ��������λ��ʼ������1�ĸ�����
### 10.	CountLeadingOnes
- public static int CountLeadingOnes(this uint value)
- public static int CountLeadingOnes(this ulong value)
- ��������λ��ʼ������1�ĸ�����
### 11.	GetSetBitPositions
- public static IEnumerable<int> GetSetBitPositions(this ulong value)
- public static IEnumerable<int> GetSetBitPositions(this uint value)
- ��������λ��λ�á�
### 12.	GetClearedBitPositions
- public static IEnumerable<int> GetClearedBitPositions(this uint value)
- public static IEnumerable<int> GetClearedBitPositions(this ulong value)
- ����δ����λ��λ�á�
### 13.	IsOdd
- public static bool IsOdd(this long value)
- public static bool IsOdd(this ulong value)
- public static bool IsOdd(this int value)
- public static bool IsOdd(this uint value)
- �ж��Ƿ���������
### 14.	IsEven
- public static bool IsEven(this long value)
- public static bool IsEven(this ulong value)
- public static bool IsEven(this int value)
- public static bool IsEven(this uint value)
- �ж��Ƿ���ż����
����ʾ��
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