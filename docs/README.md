# Vorcyc.Mathematics

## A high performance math library.

![VMath logo](logos/logo1.png "logo")
0.8.0

Vorcyc® Mathematics 是一套主要面向.NET的数学库，旨在充分利用.NET的最新特性并提供高性能和准确度的数学函数和运算。它是用C#编写的，可以在任何.NET应用程序中使用。

[Goto English Edition](README-en_us.md)

主要意义有：
1. 充分利用.NET最新特性以获得最佳性能。如：
	- 使用SIMD加速的CPU串行计算、并行计算、泛型数学。
	- 使用 Span&lt;T> 替代 数组作为参数；使用Memory&lt;T> 作为字段。
	- 使用托管指针和内存池以提高性能。
2. 补充.NET内建常用数学函数的不足。
3. 提供额外的数学算法和运算。

---
  
> ***面向.NET版本：.NET 10.0及以上。***  
> ***This package targets a minimum of .NET 10.***

---


## 特性
- SIMD加速的CPU串行计算
- 并行计算
- 泛型数学
- GPU计算（未完成）



## 依赖项
- ILGPU: 版本 1.5.1  
- ILGPU.Algorithms: 版本 1.5.1  
- System.Numerics.Tensors: 版本 9.0.0  



---

**核心模块**：本模块提供了基本的数学运算、常用函数和扩展方法，涵盖数组操作、进制转换、位运算、组合数学、常量定义、数值映射、可固定数组、随机数生成、三角运算和多种数学函数。   
:blue_book:[手册](Module_Core.md)

---

***深度学习模块***：目前只提供神经网络层的定义。   
:blue_book:[手册](Module_DeepLearning.md)

---

***实验性模块***：包含一些实验性的功能，可能会在未来版本中更改或删除。  
:blue_book:[手册](Module_Experimental.md)

---

**线性代数模块**：包含多种线性代数类和结构，这些类和结构提供了丰富的线性代数操作和运算方法，如基变换、线性方程组求解、矩阵运算、四元数运算、张量运算和向量运算等，适用于各种数学和科学计算需求。  
:blue_book:[手册](Module_LinearAlgebra.md)

---

***机器学习模块***: 包含多种机器学习算法和工具类，包括决策树、K 最近邻、多元线性回归、随机森林、简单线性回归、支持向量机、朴素贝叶斯分类器、DBSCAN 聚类、期望最大化、高斯混合模型、层次聚类、K 均值聚类、矢量量化、因子分析、主成分分析 (PCA)、t-SNE 降维算法等。此外，还包括多种距离度量类，如 Angular、ArgMax、BrayCurtis、Canberra、Chebyshev、Cosine、Dice、Euclidean、Hamming、Hellinger、Jaccard、Kulczynski、Levenshtein、Manhattan、Matching、Minkowski、PearsonCorrelation、RogersTanimoto、RusselRao、SokalMichener、SokalSneath、WeightedEuclidean、WeightedSquareEuclidean 和 Yule。这些类提供了丰富的机器学习和数据分析功能，适用于各种分类、回归、聚类和降维任务。  
:blue_book:[手册](Module_MachineLearning.md)

---

**信号处理模块**：该模块提供了丰富的信号处理功能，涵盖了音效处理、特征提取、滤波、傅里叶变换、常用操作、信号定义与操作、变换操作和窗函数等多个方面，适用于各种信号处理需求。  
:blue_book:[手册](Module_SignalProcessing.md)

---

***数值模块***：本模块提供了多个与数值计算相关的类型。这些类型包括整数、浮点数、复数、有理数、分数等。这些类型提供了高精度的数值计算功能，并且可以与其他数值类型进行转换。  
:blue_book:[手册](Module_Numerics.md)

---

**统计模块**：用于对数值数据进行统计分析，支持多种数据结构和数值类型。它包括查找极值、计算总和、平均值、方差，以及识别最大值和最小值及其索引的方法。该类在可能的情况下利用硬件加速以优化性能。  
:blue_book:[手册](Module_Statistics.md)

---

**微积分模块**：提供了全面的计算工具，包括极限计算、积分计算、导数计算、泰勒级数展开、傅里叶级数展开、龙格-库塔法求解微分方程和牛顿-拉夫逊法求解非线性方程。   
:blue_book:[手册](Module_Calculus.md)

---


