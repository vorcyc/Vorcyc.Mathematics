# Vorcyc.Mathematics

## A high performance math library.

![VMath logo](logos/logo1.png "logo")

Vorcyc® Mathematics is a math library for .NET, designed to fully leverage the latest features of .NET and provide high-performance and accurate mathematical functions and operations. It is written in C# and can be used in any .NET application.

Vorcyc® Mathematics 是一套面向.NET的数学库，旨在充分利用.NET的最新特性并提供高性能和准确度的数学函数和运算。它是用C#编写的，可以在任何.NET应用程序中使用。

主要意义有：
1. 充分利用.NET最新特性以获得最佳性能。
2. 补充.NET内建常用数学函数的不足。
3. 提供额外的数学算法和运算。

---
  
>***面向.NET版本：.NET 9.0及以上。***

---


## Features
- SIMD加速的CPU串行计算
- 并行计算
- 泛型数学
- GPU计算（未完成）



## 依赖项
- ILGPU: 版本 1.5.1  
- ILGPU.Algorithms: 版本 1.5.1  
- System.Numerics.Tensors: 版本 9.0.0  


***This package targets a minimum of .NET 9.***

---




**核心模块**：包含基本的数学运算和常用函数。  
**Core Module**: Contains basic mathematical operations and common functions.


***深度学习模块***：提供深度学习相关的函数和运算。   
***Deep Learning Module***: Provides functions and operations related to deep learning.


***实验性模块***：包含一些实验性的功能，可能会在未来版本中更改或删除。
***Experimental Module***: Contains some experimental features that may change or be removed in future versions.


**线性代数模块**：提供矩阵和向量运算、矩阵分解等功能。   
***Linear Algebra Module***: Provides matrix and vector operations, matrix decomposition, etc.


***机器学习模块***: 提供机器学习相关的函数和运算。  
***Machine Learning Module***: Provides functions and operations related to machine learning.

**信号处理模块**：用于滤波、傅里叶变换等。  
**Signal Processing Module**: Used for filtering, Fourier transform, etc.


***数值模块***：提供多个常见的数值类型 
***Numerics Module***: Provides several common numeric types


**统计模块**：包括概率分布、统计分析等。  
**Statistics Module**: Includes probability distributions, statistical analysis, etc.





---

***数值模块 - Numerics Module***
本模块提供了多个与数值计算相关的类型。这些类型包括整数、浮点数、复数、有理数、分数等。这些类型提供了高精度的数值计算功能，并且可以与其他数值类型进行转换。
命名空间  ：Vorcyc.Mathematics.Numerics


1.	ComplexFp32
•	描述: 表示单精度浮点复数，提供了复数的基本运算和高级数学函数。
•	字段: Real, Imaginary
•	属性: Magnitude, Phase
•	方法: Add, Subtract, Multiply, Divide, Negate, Conjugate, Reciprocal, Abs, Sqrt, Pow, Log, Exp, Sin, Cos, Tan, Asin, Acos, Atan, Sinh, Cosh, Tanh
•	运算符重载: +, -, *, /, ==, !=
•	接口实现: IEquatable<ComplexFp32>, IFormattable, INumberBase<ComplexFp32>, ISignedNumber<ComplexFp32>
2.	Rational<T>
•	描述: 表示有理数的结构体，可以表示为两个整数之比。
•	字段: Numerator, Denominator
•	方法: Add, Subtract, Multiply, Divide, Negate, Reciprocal, Abs, ToDouble, ToFloatingNumber
•	接口实现: IComparable<Rational<T>>, IEquatable<Rational<T>>
3.	Number
•	描述: 表示一个数值的类，支持所有数值类型的操作。
•	字段: digits, floatingDigitsCount, isNegative
•	属性: DigitsCount, IntegerDigitsCount, FloatingDigitsCount, FirstDigit, LastDigit
•	方法: Add, Subtract, Multiply, Divide, Abs, Round, Floor, Ceiling, GetFloatingPart, AppendFirstDigit, RemoveFirstDigit, AppendLastDigit, RemoveLastDigit
•	接口实现: IComparable<Number>
4.	Complex<T>
•	描述: 表示泛型复数，提供了复数的基本运算和高级数学函数。
•	字段: Real, Imaginary
•	属性: Magnitude, Phase
•	方法: Add, Subtract, Multiply, Divide, Negate, Conjugate, Reciprocal, Abs, Sqrt, Pow, Log, Exp, Sin, Cos, Tan, Asin, Acos, Atan, Sinh, Cosh, Tanh
•	运算符重载: +, -, *, /, ==, !=
•	接口实现: IEquatable<Complex<T>>, IFormattable, INumberBase<Complex<T>>, ISignedNumber<Complex<T>>
5.	BigFloat
•	描述: 表示高精度浮点数，提供了高精度的数学运算功能。
•	字段: DataBits, Scale, Size, Exponent, IsZero, OutOfPrecision, GetPrecision, GetAccuracy, IsPositive, IsNegative, Sign, Int
•	方法: Add, Subtract, Multiply, Divide, Abs, Round, Floor, Ceiling, Sqrt, Pow, Log, Exp, Parse, TryParse
•	接口实现: IComparable, IComparable<BigFloat>, IEquatable<BigFloat>
6.	UInt24
•	描述: 表示24位无符号整数，提供了基本的数值运算功能。
•	字段: _upper, _middle, _low
•	方法: Add, Subtract, Multiply, Divide
•	运算符重载: +, -, *, /
•	接口实现: IMinMaxValue<UInt24>, IAdditionOperators<UInt24, UInt24, UInt24>, ISubtractionOperators<UInt24, UInt24, UInt24>, IMultiplyOperators<UInt24, UInt24, UInt24>, IDivisionOperators<UInt24, UInt24, UInt24>, IAdditiveIdentity<UInt24, UInt24>
7.	SizeFP32
•	描述: 表示单精度浮点数的尺寸，提供了基本的尺寸运算功能。
•	字段: width, height
•	属性: Width, Height, IsEmpty
•	方法: Add, Subtract, ToString
•	运算符重载: +, -, ==, !=
8.	Size<T>
•	描述: 表示泛型尺寸，提供了基本的尺寸运算功能。
•	字段: _width, _height
•	属性: Width, Height
•	方法: Add, Subtract, ToString
•	运算符重载: +, -, *, /
9.	RectangleFP32
•	描述: 表示单精度浮点数的矩形，提供了基本的矩形运算功能。
•	字段: x, y, width, height
•	属性: X, Y, Width, Height, Location, Size, IsEmpty, Left, Right, Top, Bottom
•	方法: Contains, Inflate, Intersect, IntersectsWith, Offset, ToString
•	运算符重载: ==, !=
10.	Range<T>
•	描述: 表示一个范围的类，提供了范围的基本运算功能。
•	字段: _value
•	属性: Value, Minimum, Maximum
•	方法: IsInside, Intersection, Union, IsOverlapping, ToString
•	接口实现: IEquatable<Range<T>>
11.	PointFP32
•	描述: 表示单精度浮点数的点，提供了基本的点运算功能。
•	字段: x, y
•	属性: X, Y, IsEmpty
•	方法: Add, Subtract, ToString
•	运算符重载: +, -, ==, !=
12.	Point<T>
•	描述: 表示泛型点，提供了基本的点运算功能。
•	字段: _x, _y
•	属性: X, Y, IsEmpty
•	方法: Add, Subtract, ToString
•	运算符重载: +, -
•	接口实现: IAdditionOperators<Point<T>, Size<T>, Point<T>>, ISubtractionOperators<Point<T>, Size<T>, Point<T>>, IEquatable<Point<T>>
13.	IntRange
•	描述: 表示一个整数范围的结构体，提供了范围的基本运算功能。
•	字段: min, max
•	属性: Min, Max, Length
•	方法: IsInside, Intersection, IsOverlapping, ToString
•	运算符重载: ==, !=
•	接口实现: IEquatable<IntRange>, IEnumerable<int>
14.	Int24
•	描述: 表示24位有符号整数，提供了基本的数值运算功能。
•	字段: _upper, _middle, _low
•	方法: Add, Subtract, Multiply, Divide
•	运算符重载: +, -, *, /
•	接口实现: IMinMaxValue<Int24>, IAdditionOperators<Int24, Int24, Int24>, ISubtractionOperators<Int24, Int24, Int24>, IMultiplyOperators<Int24, Int24, Int24>, IDivisionOperators<Int24, Int24, Int24>, IAdditiveIdentity<Int24, Int24>, IComparable, IComparable<Int24>, IFormattable