# 数值模块 - Numerics Module

本模块提供了多个与数值计算相关的类型。这些类型包括整数、浮点数、复数、有理数、分数等。这些类型提供了高精度的数值计算功能，并且可以与其他数值类型进行转换。

>命名空间：`Vorcyc.Mathematics.Numerics`



## 1. BigFloat
一个表示高精度浮点数的结构体。

### 功能：
- 表示高精度浮点数
- 支持加、减、乘、除等运算
- 支持与其他数值类型的转换
- 支持平方根、幂运算等高级数学运算
- **字段**: `DataBits`, `Scale`, `Size`, `Exponent`, `IsZero`, `OutOfPrecision`, `GetPrecision`, `GetAccuracy`, `IsPositive`, `IsNegative`, `Sign`, `Int`
- **方法**: `Add`, `Subtract`, `Multiply`, `Divide`, `Abs`, `Round`, `Floor`, `Ceiling`, `Sqrt`, `Pow`, `Log`, `Exp`, `Parse`, `TryParse`
- **接口实现**: `IComparable`, `IComparable<BigFloat>`, `IEquatable<BigFloat>`

### 使用示例：
```csharp
BigFloat a = new BigFloat(123.456);
BigFloat b = new BigFloat("789.012");
BigFloat resultAdd = a + b;
BigFloat resultSub = a - b;
BigFloat resultMul = a * b;
BigFloat resultDiv = a / b;
BigFloat resultSqrt = BigFloat.Sqrt(a);
BigFloat resultPow = BigFloat.Pow(a, 2);

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Multiply: {resultMul}");
Console.WriteLine($"Divide: {resultDiv}");
Console.WriteLine($"Sqrt: {resultSqrt}");
Console.WriteLine($"Pow: {resultPow}");
```



## 2. Complex&lt;T&gt;
一个表示复数的结构体，具有泛型数值类型。

### 功能：
- 表示复数
- 支持加、减、乘、除等运算
- 支持极坐标转换
- 支持求模、求共轭等操作
- **字段**: `Real`, `Imaginary`
- **属性**: `Magnitude`, `Phase`
- **方法**: `Add`, `Subtract`, `Multiply`, `Divide`, `Negate`, `Conjugate`, `Reciprocal`, `Abs`, `Sqrt`, `Pow`, `Log`, `Exp`, `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`, `Sinh`, `Cosh`, `Tanh`
- **运算符重载**: `+`, `-`, `*`, `/`, `==`, `!=`
- **接口实现**: `IEquatable<Complex<T>>`, `IFormattable`, `INumberBase<Complex<T>>`, `ISignedNumber<Complex<T>>`

### 使用示例：
```csharp
Complex<double> c1 = new Complex<double>(1.0, 2.0);
Complex<double> c2 = new Complex<double>(3.0, 4.0);
Complex<double> resultAdd = c1 + c2;
Complex<double> resultSub = c1 - c2;
Complex<double> resultMul = c1 * c2;
Complex<double> resultDiv = c1 / c2;
double magnitude = Complex<double>.Abs(c1);
Complex<double> conjugate = Complex<double>.Conjugate(c1);

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Multiply: {resultMul}");
Console.WriteLine($"Divide: {resultDiv}");
Console.WriteLine($"Magnitude: {magnitude}");
Console.WriteLine($"Conjugate: {conjugate}");
```



## 3. ComplexFp32
一个表示复数的结构体，具有单精度浮点数分量。

### 功能：
- 表示复数
- 支持加、减、乘、除等运算
- 支持极坐标转换
- 支持求模、求共轭等操作
- **字段**: `Real`, `Imaginary`
- **属性**: `Magnitude`, `Phase`
- **方法**: `Add`, `Subtract`, `Multiply`, `Divide`, `Negate`, `Conjugate`, `Reciprocal`, `Abs`, `Sqrt`, `Pow`, `Log`, `Exp`, `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`, `Sinh`, `Cosh`, `Tanh`
- **运算符重载**: `+`, `-`, `*`, `/`, `==`, `!=`
- **接口实现**: `IEquatable<ComplexFp32>`, `IFormattable`, `INumberBase<ComplexFp32>`, `ISignedNumber<ComplexFp32>`

### 使用示例：
```csharp
ComplexFp32 c1 = new ComplexFp32(1.0f, 2.0f);
ComplexFp32 c2 = new ComplexFp32(3.0f, 4.0f);
ComplexFp32 resultAdd = c1 + c2;
ComplexFp32 resultSub = c1 - c2;
ComplexFp32 resultMul = c1 * c2;
ComplexFp32 resultDiv = c1 / c2;
float magnitude = ComplexFp32.Abs(c1);
ComplexFp32 conjugate = ComplexFp32.Conjugate(c1);

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Multiply: {resultMul}");
Console.WriteLine($"Divide: {resultDiv}");
Console.WriteLine($"Magnitude: {magnitude}");
Console.WriteLine($"Conjugate: {conjugate}");
```



## 4. Int24
一个表示24位有符号整数的结构体。

### 功能：
- 表示24位有符号整数
- 支持加、减、乘、除等运算
- 支持与其他数值类型的转换
- **字段**: `_upper`, `_middle`, `_low`
- **方法**: `Add`, `Subtract`, `Multiply`, `Divide`
- **运算符重载**: `+`, `-`, `*`, `/`
- **接口实现**: `IMinMaxValue<Int24>`, `IAdditionOperators<Int24, Int24, Int24>`, `ISubtractionOperators<Int24, Int24, Int24>`, `IMultiplyOperators<Int24, Int24, Int24>`, `IDivisionOperators<Int24, Int24, Int24>`, `IAdditiveIdentity<Int24, Int24>`, `IComparable`, `IComparable<Int24>`, `IFormattable`

### 使用示例：
```csharp
Int24 a = new Int24(123456);
Int24 b = new Int24(654321);
Int24 resultAdd = a + b;
Int24 resultSub = a - b;
Int24 resultMul = a * b;
Int24 resultDiv = a / b;

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Multiply: {resultMul}");
Console.WriteLine($"Divide: {resultDiv}");
```


## 5. IntRange
一个表示整数范围的结构体。

### 功能：
- 表示整数范围
- 检查值是否在范围内
- 计算范围的交集
- **字段**: `min`, `max`
- **属性**: `Min`, `Max`, `Length`
- **方法**: `IsInside`, `Intersection`, `IsOverlapping`, `ToString`
- **运算符重载**: `==`, `!=`
- **接口实现**: `IEquatable<IntRange>`, `IEnumerable<int>`

### 使用示例：
```csharp
IntRange range = new IntRange(1, 10);
bool isInside = range.IsInside(5);
IntRange intersection = range.Intersection(new IntRange(5, 15));

Console.WriteLine($"IsInside: {isInside}");
Console.WriteLine($"Intersection: [{intersection.Min}, {intersection.Max}]");
```



## 6. Number
一个表示数字的类，作为数字列表，具有任意精度。

### 功能：
- 表示任意精度的数字
- 支持加、减、乘、除等运算
- 支持与其他数值类型的转换
- **字段**: `digits`, `floatingDigitsCount`, `isNegative`
- **属性**: `DigitsCount`, `IntegerDigitsCount`, `FloatingDigitsCount`, `FirstDigit`, `LastDigit`
- **方法**: `Add`, `Subtract`, `Multiply`, `Divide`, `Abs`, `Round`, `Floor`, `Ceiling`, `GetFloatingPart`, `AppendFirstDigit`, `RemoveFirstDigit`, `AppendLastDigit`, `RemoveLastDigit`
- **接口实现**: `IComparable<Number>`
### 使用示例：
```csharp
Number n1 = new Number("12345678901234567890");
Number n2 = new Number("98765432109876543210");
Number resultAdd = n1 + n2;
Number resultSub = n1 - n2;
Number resultMul = n1 * n2;
Number resultDiv = n1 / n2;

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Multiply: {resultMul}");
Console.WriteLine($"Divide: {resultDiv}");
```

## 7. Point&lt;T&gt;
一个表示二维平面上点的结构体，具有泛型数值类型。

### 功能：
- 表示二维平面上的点
- 支持加、减运算
- 支持距离计算
- **字段**: `_x`, `_y`
- **属性**: `X`, `Y`, `IsEmpty`
- **方法**: `Add`, `Subtract`, `ToString`
- **运算符重载**: `+`, `-`
- **接口实现**: `IAdditionOperators<Point<T>, Size<T>, Point<T>>`, `ISubtractionOperators<Point<T>, Size<T>, Point<T>>`, `IEquatable<Point<T>>`

### 使用示例：
```csharp
Point<int> p1 = new Point<int>(1, 2);
Point<int> p2 = new Point<int>(3, 4);
Point<int> resultAdd = p1 + p2;
Point<int> resultSub = p1 - p2;
double distance = Point<int>.Distance(p1, p2);

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Distance: {distance}");
```


## 8. PointFp32
一个表示二维平面上点的结构体，具有单精度浮点数坐标。

### 功能：
- 表示二维平面上的点
- 支持加、减运算
- 支持距离计算
- **字段**: `x`, `y`
- **属性**: `X`, `Y`, `IsEmpty`
- **方法**: `Add`, `Subtract`, `ToString`
- **运算符重载**: `+`, `-`, `==`, `!=`

### 使用示例：
```csharp
PointFp32 p1 = new PointFp32(1.0f, 2.0f);
PointFp32 p2 = new PointFp32(3.0f, 4.0f);
PointFp32 resultAdd = p1 + p2;
PointFp32 resultSub = p1 - p2;
float distance = PointFp32.Distance(p1, p2);

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Distance: {distance}");
```


## 9. Range&lt;T&gt;
一个表示值范围的类，具有泛型数值类型。

### 功能：
- 表示值范围
- 检查值是否在范围内
- 计算范围的交集和并集
- **字段**: `_value`
- **属性**: `Value`, `Minimum`, `Maximum`
- **方法**: `IsInside`, `Intersection`, `Union`, `IsOverlapping`, `ToString`
- **接口实现**: `IEquatable<Range<T>>`

### 使用示例：
```csharp
Range<int> range = new Range<int>(1, 10);
bool isInside = range.IsInside(5);
Range<int> intersection = range.Intersection(new Range<int>(5, 15));
Range<int> union = range.Union(new Range<int>(5, 15));

Console.WriteLine($"IsInside: {isInside}");
Console.WriteLine($"Intersection: [{intersection.Minimum}, {intersection.Maximum}]");
Console.WriteLine($"Union: [{union.Minimum}, {union.Maximum}]");
```



## 10. Rational&lt;T&gt;
一个表示有理数的结构体，具有泛型整数类型。

### 功能：
- 表示有理数
- 支持加、减、乘、除等运算
- 支持与其他数值类型的转换
- **字段**: `Numerator`, `Denominator`
- **方法**: `Add`, `Subtract`, `Multiply`, `Divide`, `Negate`, `Reciprocal`, `Abs`, `ToDouble`, `ToFloatingNumber`
- **接口实现**: `IComparable<Rational<T>>`, `IEquatable<Rational<T>>`

### 使用示例：
```csharp
Rational<int> r1 = new Rational<int>(1, 2);
Rational<int> r2 = new Rational<int>(3, 4);
Rational<int> resultAdd = r1 + r2;
Rational<int> resultSub = r1 - r2;
Rational<int> resultMul = r1 * r2;
Rational<int> resultDiv = r1 / r2;

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Multiply: {resultMul}");
Console.WriteLine($"Divide: {resultDiv}");
```



## 11. RectangleFP32
一个表示具有浮点数坐标和尺寸的矩形结构体。

### 功能：
- 表示矩形
- 检查点是否在矩形内
- 计算矩形的交集和并集
- **字段**: `x`, `y`, `width`, `height`
- **属性**: `X`, `Y`, `Width`, `Height`, `Location`, `Size`, `IsEmpty`, `Left`, `Right`, `Top`, `Bottom`
- **方法**: `Contains`, `Inflate`, `Intersect`, `IntersectsWith`, `Offset`, `ToString`
- **运算符重载**: `==`, `!=`

### 使用示例：
```csharp
RectangleFP32 rect = new RectangleFP32(0.0f, 0.0f, 10.0f, 20.0f);
bool contains = rect.Contains(5.0f, 5.0f);
RectangleFP32 intersection = RectangleFP32.Intersect(rect, new RectangleFP32(5.0f, 5.0f, 10.0f, 20.0f));
RectangleFP32 union = RectangleFP32.Union(rect, new RectangleFP32(5.0f, 5.0f, 10.0f, 20.0f));

Console.WriteLine($"Contains: {contains}");
Console.WriteLine($"Intersection: {intersection}");
Console.WriteLine($"Union: {union}");
```



## 12. Size&lt;T&gt;
一个表示二维尺寸的结构体，具有泛型数值类型。
### 功能：
- 表示二维尺寸
- 支持加、减、乘、除等运算
- **字段**: `_width`, `_height`
- **属性**: `Width`, `Height`
- **方法**: `Add`, `Subtract`, `ToString`
- **运算符重载**: `+`, `-`, `*`, `/`

使用示例：
```csharp
Size<int> size1 = new Size<int>(10, 20);
Size<int> size2 = new Size<int>(5, 5);
Size<int> resultAdd = size1 + size2;
Size<int> resultSub = size1 - size2;
Size<int> resultMul = size1 * 2;
Size<int> resultDiv = size1 / 2;

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Multiply: {resultMul}");
Console.WriteLine($"Divide: {resultDiv}");
```

## 13. SizeFp32
一个表示二维尺寸的结构体，具有单精度浮点数尺寸。
### 功能：
- 表示二维尺寸
- 支持加、减、乘、除等运算
- **字段**: `width`, `height`
- **属性**: `Width`, `Height`, `IsEmpty`
- **方法**: `Add`, `Subtract`, `ToString`
- **运算符重载**: `+`, `-`, `==`, `!=`
- 
使用示例：
```csharp
SizeFp32 size1 = new SizeFp32(10.0f, 20.0f);
SizeFp32 size2 = new SizeFp32(5.0f, 5.0f);
SizeFp32 resultAdd = size1 + size2;
SizeFp32 resultSub = size1 - size2;
SizeFp32 resultMul = size1 * 2.0f;
SizeFp32 resultDiv = size1 / 2.0f;

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Multiply: {resultMul}");
Console.WriteLine($"Divide: {resultDiv}");
```




## 14. UInt24
一个表示24位无符号整数的结构体。
## 功能：
- 表示24位无符号整数
- 支持加、减、乘、除等运算
- 支持与其他数值类型的转换
- **字段**: `_upper`, `_middle`, `_low`
- **方法**: `Add`, `Subtract`, `Multiply`, `Divide`
- **运算符重载**: `+`, `-`, `*`, `/`
- **接口实现**: `IMinMaxValue<UInt24>`, `IAdditionOperators<UInt24, UInt24, UInt24>`, `ISubtractionOperators<UInt24, UInt24, UInt24>`, `IMultiplyOperators<UInt24, UInt24, UInt24>`, `IDivisionOperators<UInt24, UInt24, UInt24>`, `IAdditiveIdentity<UInt24, UInt24>`

使用示例：
```csharp
UInt24 a = new UInt24(123456);
UInt24 b = new UInt24(654321);
UInt24 resultAdd = a + b;
UInt24 resultSub = a - b;
UInt24 resultMul = a * b;
UInt24 resultDiv = a / b;

Console.WriteLine($"Add: {resultAdd}");
Console.WriteLine($"Subtract: {resultSub}");
Console.WriteLine($"Multiply: {resultMul}");
Console.WriteLine($"Divide: {resultDiv}");
```