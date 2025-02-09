# Vorcyc.Mathematics

## A high performance math library.

![VMath logo](logos/logo1.png "logo")

Vorcyc® Mathematics is a math library for .NET, designed to fully leverage the latest features of .NET and provide high-performance and accurate mathematical functions and operations. It is written in C# and can be used in any .NET application.

Vorcyc® Mathematics 是一套主要面向.NET的数学库，旨在充分利用.NET的最新特性并提供高性能和准确度的数学函数和运算。它是用C#编写的，可以在任何.NET应用程序中使用。

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
:blue_book:[手册](Module_Core.md)

***深度学习模块***：提供深度学习相关的函数和运算。   
***Deep Learning Module***: Provides functions and operations related to deep learning.


***实验性模块***：包含一些实验性的功能，可能会在未来版本中更改或删除。  
***Experimental Module***: Contains some experimental features that may change or be removed in future versions.


**线性代数模块**：提供矩阵和向量运算、矩阵分解等功能。   
***Linear Algebra Module***: Provides matrix and vector operations, matrix decomposition, etc.  
:blue_book:[手册](Module_LinearAlgebra.md)


***机器学习模块***: 提供机器学习相关的函数和运算。  
***Machine Learning Module***: Provides functions and operations related to machine learning.  
:blue_book:[手册](Module_MachineLearning.md)

**信号处理模块**：用于滤波、傅里叶变换等。  
**Signal Processing Module**: Used for filtering, Fourier transform, etc.


***数值模块***：提供多个常见的数值类型   
***Numerics Module***: Provides several common numeric types  
:blue_book:[手册](Module_Numerics.md)


**统计模块**：包括概率分布、统计分析等。  
**Statistics Module**: Includes probability distributions, statistical analysis, etc.  
:blue_book:[手册](Module_Statistics.md)


---
