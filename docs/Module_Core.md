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