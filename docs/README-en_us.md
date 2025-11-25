# Vorcyc.Mathematics

## A high performance math library.

![VMath logo](logos/logo1.png "logo")
0.8.0

Vorcyc® Mathematics is a math library mainly for .NET, designed to fully leverage the latest features of .NET and provide high-performance and accurate mathematical functions and operations. It is written in C# and can be used in any .NET application.

[Goto Chinese Edition](README.md)

The main significance is:
1. Fully leverage the latest features of .NET for optimal performance, such as:
   - Using SIMD-accelerated CPU serial computation, parallel computation, and generic mathematics.
   - Using Span&lt;T> instead of arrays as parameters and Memory&lt;T> as fields.
   - Using managed pointers and memory pools to improve performance.
2. Supplement the deficiencies of .NET's built-in common mathematical functions.
3. Provide additional mathematical algorithms and operations.


---

> ***Targeting .NET version: .NET 10.0 and above.***  
> ***This package targets a minimum of .NET 10.***

---

## Features
- SIMD accelerated CPU serial computation
- Parallel computation
- Generic mathematics
- GPU computation (unfinished)

## Dependencies
- ILGPU: Version 1.5.1  
- ILGPU.Algorithms: Version 1.5.1  
- System.Numerics.Tensors: Version 9.0.0  

---

**Core Module**: This module provides basic mathematical operations, common functions, and extension methods, covering array operations, base conversion, bit operations, combinatorial mathematics, constant definitions, numerical mapping, fixed arrays, random number generation, trigonometric operations, and various mathematical functions.   
:blue_book:[Manual](Module_Core.md)

---

***Deep Learning Module***: Currently only provides the definition of neural network layers.   
:blue_book:[Manual](Module_DeepLearning.md)

---

***Experimental Module***: Contains some experimental features that may be changed or removed in future versions.  
:blue_book:[Manual](Module_Experimental.md)

---

**Linear Algebra Module**: Contains various linear algebra classes and structures, providing rich linear algebra operations and methods such as basis transformation, solving linear equations, matrix operations, quaternion operations, tensor operations, and vector operations, suitable for various mathematical and scientific computing needs.   
:blue_book:[Manual](Module_LinearAlgebra.md)

---

***Machine Learning Module***: Contains various machine learning algorithms and utility classes, including decision trees, K-nearest neighbors, multiple linear regression, random forests, simple linear regression, support vector machines, naive Bayes classifiers, DBSCAN clustering, expectation-maximization, Gaussian mixture models, hierarchical clustering, K-means clustering, vector quantization, factor analysis, principal component analysis (PCA), t-SNE dimensionality reduction algorithms, and more. Additionally, it includes various distance metric classes such as Angular, ArgMax, BrayCurtis, Canberra, Chebyshev, Cosine, Dice, Euclidean, Hamming, Hellinger, Jaccard, Kulczynski, Levenshtein, Manhattan, Matching, Minkowski, PearsonCorrelation, RogersTanimoto, RusselRao, SokalMichener, SokalSneath, WeightedEuclidean, WeightedSquareEuclidean, and Yule. These classes provide rich machine learning and data analysis functions, suitable for various classification, regression, clustering, and dimensionality reduction tasks.    
:blue_book:[Manual](Module_MachineLearning.md)

---

**Signal Processing Module**: This module provides rich signal processing functions, covering audio processing, feature extraction, filtering, Fourier transform, common operations, signal definition and operations, transform operations, and window functions, suitable for various signal processing needs.  
:blue_book:[Manual](Module_SignalProcessing.md)

---

***Numerics Module***: This module provides multiple types related to numerical computation. These types include integers, floating-point numbers, complex numbers, rational numbers, fractions, etc. These types provide high-precision numerical computation functions and can be converted with other numerical types.  
:blue_book:[Manual](Module_Numerics.md)

---

**Statistics Module**: Used for statistical analysis of numerical data, supporting various data structures and numerical types. It includes methods for finding extrema, calculating sums, averages, variances, and identifying the maximum and minimum values and their indices. This class utilizes hardware acceleration where possible to optimize performance.   
:blue_book:[Manual](Module_Statistics.md)

---


**Calculus Module**: Provides comprehensive computational tools, including limit calculation, integration, derivative calculation, Taylor series expansion, Fourier series expansion, Runge-Kutta method for solving differential equations, and Newton-Raphson method for solving nonlinear equations.   
:blue_book:[Manual](Module_Calculus.md)

---


