# 线性代数模块 - Linear Algebra Module
> Vorcyc.Mathematics.LinearAlgebra 命名空间。

## Vorcyc.Mathematics.LinearAlgebra.BasisTransformation 类

Vorcyc.Mathematics.LinearAlgebra.BasisTransformation 是一个静态类，提供基变换的方法。

### 方法清单及说明

#### 1. Transform
- `public static Vector<T> Transform<T>(Vector<T> vector, Matrix<T> fromBasis, Matrix<T> toBasis) where T : struct, IFloatingPointIeee754<T>`
  - 将向量从一个基变换到另一个基。
  - 参数:
    - `vector`: 要变换的向量。
    - `fromBasis`: 原基。
    - `toBasis`: 目标基。
  - 返回值: 变换后的向量。
  - 异常:
    - `ArgumentException`: 当基的维度不匹配时抛出。

### 代码示例
以下是一个使用 BasisTransformation 类中 Transform 方法的示例，并在示例中加入了注释：

```csharp
using System; 
using Vorcyc.Mathematics.LinearAlgebra;

public class BasisTransformationExample 
{ 
    public static void Main() 
    { 
        // 定义向量和基矩阵 
        Vector<double> vector = new Vector<double>(new double[] { 1.0, 2.0, 3.0 });
        Matrix<double> fromBasis = new Matrix<double>(new double[,] { { 1.0, 0.0, 0.0 }, { 0.0, 1.0, 0.0 }, { 0.0, 0.0, 1.0 } });
        Matrix<double> toBasis = new Matrix<double>(new double[,] { { 0.0, 1.0, 0.0 }, { 1.0, 0.0, 0.0 }, { 0.0, 0.0, 1.0 } });
    
        // 进行基变换
        Vector<double> transformedVector = BasisTransformation.Transform(vector, fromBasis, toBasis);

        // 输出变换后的向量
        Console.WriteLine("Transformed Vector: " + string.Join(", ", transformedVector));
    }
}

```


## Vorcyc.Mathematics.LinearAlgebra.LinearEquationSolver 类

Vorcyc.Mathematics.LinearAlgebra.LinearEquationSolver 是一个静态类，提供求解线性方程组的方法。

### 方法清单及说明

#### 1. GaussianEliminationSolve
- `public static Vector<T> GaussianEliminationSolve<T>(Matrix<T> A, Vector<T> b) where T : struct, IFloatingPointIeee754<T>`
  - 使用高斯消元法求解线性方程组 Ax = b。
  - 参数:
    - `A`: 系数矩阵 A。
    - `b`: 常数向量 b。
  - 返回值: 解向量 x。
  - 异常:
    - `ArgumentException`: 当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。

#### 2. LUSolve
- `public static Vector<T> LUSolve<T>(Matrix<T> A, Vector<T> b) where T : struct, IFloatingPointIeee754<T>`
  - 使用 LU 分解法求解线性方程组 Ax = b。
  - 参数:
    - `A`: 系数矩阵 A。
    - `b`: 常数向量 b。
  - 返回值: 解向量 x。
  - 异常:
    - `ArgumentException`: 当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。

#### 3. JacobiSolve
- `public static Vector<T> JacobiSolve<T>(Matrix<T> A, Vector<T> b, T tolerance, int maxIterations = 1000) where T : struct, IFloatingPointIeee754<T>`
  - 使用 Jacobi 法求解线性方程组 Ax = b。
  - 参数:
    - `A`: 系数矩阵 A。
    - `b`: 常数向量 b。
    - `tolerance`: 收敛容差。
    - `maxIterations`: 最大迭代次数。
  - 返回值: 解向量 x。
  - 异常:
    - `ArgumentException`: 当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。

#### 4. GaussSeidelSolve
- `public static Vector<T> GaussSeidelSolve<T>(Matrix<T> A, Vector<T> b, T tolerance, int maxIterations = 1000) where T : struct, IFloatingPointIeee754<T>`
  - 使用 Gauss-Seidel 法求解线性方程组 Ax = b。
  - 参数:
    - `A`: 系数矩阵 A。
    - `b`: 常数向量 b。
    - `tolerance`: 收敛容差。
    - `maxIterations`: 最大迭代次数。
  - 返回值: 解向量 x。
  - 异常:
    - `ArgumentException`: 当矩阵 A 不是方阵或 A 的行数与向量 b 的维度不匹配时抛出。

### 代码示例
以下是一个使用 LinearEquationSolver 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System; using Vorcyc.Mathematics.LinearAlgebra;

public class LinearEquationSolverExample 
{
    public static void Main() 
    { 
        // 定义系数矩阵 A 和常数向量 b Matrix<double> A = new Matrix<double>(new double[,] { { 2.0, -1.0, 0.0 }, { -1.0, 2.0, -1.0 }, { 0.0, -1.0, 2.0 } }); 
        Vector<double> b = new Vector<double>(new double[] { 1.0, 0.0, 1.0 });

        // 使用高斯消元法求解
        Vector<double> x1 = LinearEquationSolver.GaussianEliminationSolve(A, b);
        Console.WriteLine("Gaussian Elimination Solution: " + string.Join(", ", x1));

        // 使用 LU 分解法求解
        Vector<double> x2 = LinearEquationSolver.LUSolve(A, b);
        Console.WriteLine("LU Decomposition Solution: " + string.Join(", ", x2));

        // 使用 Jacobi 法求解
        Vector<double> x3 = LinearEquationSolver.JacobiSolve(A, b, 1e-10);
        Console.WriteLine("Jacobi Method Solution: " + string.Join(", ", x3));

        // 使用 Gauss-Seidel 法求解
        Vector<double> x4 = LinearEquationSolver.GaussSeidelSolve(A, b, 1e-10);
        Console.WriteLine("Gauss-Seidel Method Solution: " + string.Join(", ", x4));
    }

}
```


## Vorcyc.Mathematics.LinearAlgebra.Matrix 类

Vorcyc.Mathematics.LinearAlgebra.Matrix 是一个表示二维矩阵的类，提供了矩阵的基本操作和运算方法。

### 属性

#### 1. Rows
- `public int Rows { get; }`
  - 获取矩阵的行数。

#### 2. Columns
- `public int Columns { get; }`
  - 获取矩阵的列数。

### 构造器

#### 1. Matrix(int rows, int columns)
- `public Matrix(int rows, int columns)`
  - 使用指定的行数和列数构造一个矩阵。
  - 参数:
    - `rows`: 行数。
    - `columns`: 列数。

#### 2. Matrix(int size)
- `public Matrix(int size)`
  - 使用指定的大小构造一个方阵。
  - 参数:
    - `size`: 矩阵的大小（行数和列数）。

#### 3. Matrix(float[,] initialValues)
- `public Matrix(float[,] initialValues)`
  - 使用二维数组构造一个矩阵。
  - 参数:
    - `initialValues`: 二维数组形式的初始值。

### 索引器

#### 1. this[int row, int column]
- `public ref float this[int row, int column]`
  - 获取或设置指定位置的元素。
  - 参数:
    - `row`: 行索引。
    - `column`: 列索引。
  - 返回值: 指定位置的元素。

### 隐式转换

#### 1. 隐式转换为二维数组
- `public static implicit operator float[,](Matrix matrix)`
  - 将矩阵隐式转换为二维数组。
  - 参数:
    - `matrix`: 要转换的矩阵。

#### 2. 隐式转换为锯齿数组
- `public static implicit operator float[][](Matrix matrix)`
  - 将矩阵隐式转换为锯齿数组。
  - 参数:
    - `matrix`: 要转换的矩阵。

#### 3. 隐式转换为矩阵
- `public static implicit operator Matrix(float[][] values)`
  - 将锯齿数组隐式转换为矩阵。
  - 参数:
    - `values`: 要转换的锯齿数组。

- `public static implicit operator Matrix(float[,] values)`
  - 将二维数组隐式转换为矩阵。
  - 参数:
    - `values`: 要转换的二维数组。

### 运算符

#### 1. 矩阵加法
- `public static Matrix operator +(Matrix a, Matrix b)`
  - 矩阵加法运算符。
  - 参数:
    - `a`: 第一个矩阵。
    - `b`: 第二个矩阵。
  - 返回值: 两个矩阵的和。

#### 2. 矩阵减法
- `public static Matrix operator -(Matrix a, Matrix b)`
  - 矩阵减法运算符。
  - 参数:
    - `a`: 第一个矩阵。
    - `b`: 第二个矩阵。
  - 返回值: 两个矩阵的差。

#### 3. 矩阵乘法
- `public static Matrix operator *(Matrix a, Matrix b)`
  - 矩阵乘法运算符。
  - 参数:
    - `a`: 第一个矩阵。
    - `b`: 第二个矩阵。
  - 返回值: 两个矩阵的积。

#### 4. 矩阵与标量乘法
- `public static Matrix operator *(Matrix matrix, float scalar)`
  - 矩阵与标量乘法运算符。
  - 参数:
    - `matrix`: 矩阵。
    - `scalar`: 标量。
  - 返回值: 矩阵与标量的积。

#### 5. 矩阵与标量除法
- `public static Matrix operator /(Matrix matrix, float scalar)`
  - 矩阵与标量除法运算符。
  - 参数:
    - `matrix`: 矩阵。
    - `scalar`: 标量。
  - 返回值: 矩阵与标量的商。

### 方法

#### 1. GetRow
- `public Span<float> GetRow(int rowIndex)`
  - 获取指定行的元素。
  - 参数:
    - `rowIndex`: 行索引。
  - 返回值: 指定行的元素的 Span。

#### 2. GetColumn
- `public ReadOnlySpan<float> GetColumn(int columnIndex)`
  - 获取指定列的元素。
  - 参数:
    - `columnIndex`: 列索引。
  - 返回值: 指定列的元素的 ReadOnlySpan。

#### 3. Transpose
- `public Matrix Transpose()`
  - 矩阵转置。
  - 返回值: 转置后的矩阵。

#### 4. Determinant
- `public float Determinant()`
  - 计算矩阵的行列式。
  - 返回值: 矩阵的行列式。

#### 5. Inverse
- `public Matrix Inverse()`
  - 计算矩阵的逆矩阵。
  - 返回值: 矩阵的逆矩阵。

#### 6. LUDecomposition
- `public void LUDecomposition(out Matrix L, out Matrix U)`
  - LU 分解。
  - 参数:
    - `L`: 输出的下三角矩阵。
    - `U`: 输出的上三角矩阵。

#### 7. QRDecomposition
- `public void QRDecomposition(out Matrix Q, out Matrix R)`
  - QR 分解。
  - 参数:
    - `Q`: 输出的正交矩阵。
    - `R`: 输出的上三角矩阵。

#### 8. CholeskyDecomposition
- `public Matrix CholeskyDecomposition()`
  - Cholesky 分解。
  - 返回值: 下三角矩阵。

#### 9. Eye
- `public static Matrix Eye(int size)`
  - 创建一个单位矩阵。
  - 参数:
    - `size`: 矩阵的大小（行数和列数）。
  - 返回值: 单位矩阵。

#### 10. Companion
- `public static Matrix Companion(float[] a)`
  - 创建一个伴随矩阵。
  - 参数:
    - `a`: 输入数组，表示多项式的系数。
  - 返回值: 伴随矩阵。
  - 异常:
    - `ArgumentException`: 当输入数组的长度小于 2 或第一个系数为零时抛出。

#### 11. FillRandom
- `public void FillRandom()`
  - 用随机数填充矩阵。

#### 12. Clone
- `public Matrix Clone()`
  - 创建矩阵的深拷贝。
  - 返回值: 矩阵的深拷贝。

#### 13. ToString
- `public override string ToString()`
  - 返回矩阵的字符串表示形式。
  - 返回值: 矩阵的字符串表示形式。

### 代码示例
以下是一个使用 Matrix 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System; 
using Vorcyc.Mathematics.LinearAlgebra;

public class MatrixExample 
{ 
    public static void Main() 
    { 
        // 使用二维数组构造矩阵 
        Matrix matrixA = new Matrix(new float[,] { { 1.0f, 2.0f, 3.0f }, { 4.0f, 5.0f, 6.0f }, { 7.0f, 8.0f, 9.0f } });

        // 使用行数和列数构造矩阵
        Matrix matrixB = new Matrix(3, 3);
        matrixB.FillRandom();

        // 矩阵加法
        Matrix sum = matrixA + matrixB;
        Console.WriteLine("Sum of matrices:\n" + sum);

        // 矩阵减法
        Matrix difference = matrixA - matrixB;
        Console.WriteLine("Difference of matrices:\n" + difference);

        // 矩阵乘法
        Matrix product = matrixA * matrixB;
        Console.WriteLine("Product of matrices:\n" + product);

        // 矩阵转置
        Matrix transpose = matrixA.Transpose();
        Console.WriteLine("Transpose of matrixA:\n" + transpose);

        // 计算行列式
        float determinant = matrixA.Determinant();
        Console.WriteLine("Determinant of matrixA: " + determinant);

        // 计算逆矩阵
        Matrix inverse = matrixA.Inverse();
        Console.WriteLine("Inverse of matrixA:\n" + inverse);

        // LU 分解
        matrixA.LUDecomposition(out Matrix L, out Matrix U);
        Console.WriteLine("L matrix from LU decomposition:\n" + L);
        Console.WriteLine("U matrix from LU decomposition:\n" + U);

        // QR 分解
        matrixA.QRDecomposition(out Matrix Q, out Matrix R);
        Console.WriteLine("Q matrix from QR decomposition:\n" + Q);
        Console.WriteLine("R matrix from QR decomposition:\n" + R);

        // Cholesky 分解
        Matrix cholesky = matrixA.CholeskyDecomposition();
        Console.WriteLine("Cholesky decomposition of matrixA:\n" + cholesky);

        // 创建单位矩阵
        Matrix identity = Matrix.Eye(3);
        Console.WriteLine("Identity matrix:\n" + identity);

        // 创建伴随矩阵
        Matrix companion = Matrix.Companion(new float[] { 1.0f, -6.0f, 11.0f, -6.0f });
        Console.WriteLine("Companion matrix:\n" + companion);

        // 矩阵深拷贝
        Matrix clone = matrixA.Clone();
        Console.WriteLine("Clone of matrixA:\n" + clone);
    }
}
```

## Vorcyc.Mathematics.LinearAlgebra.Matrix<T> 类

Vorcyc.Mathematics.LinearAlgebra.Matrix<T> 是一个表示二维矩阵的泛型类，提供了矩阵的基本操作和运算方法。

### 属性

#### 1. Rows
- `public int Rows { get; }`
  - 获取矩阵的行数。

#### 2. Columns
- `public int Columns { get; }`
  - 获取矩阵的列数。

### 构造器

#### 1. Matrix(int rows, int columns)
- `public Matrix(int rows, int columns)`
  - 使用指定的行数和列数构造一个矩阵。
  - 参数:
    - `rows`: 行数。
    - `columns`: 列数。

#### 2. Matrix(int size)
- `public Matrix(int size)`
  - 使用指定的大小构造一个方阵。
  - 参数:
    - `size`: 矩阵的大小（行数和列数）。

#### 3. Matrix(T[,] initialValues)
- `public Matrix(T[,] initialValues)`
  - 使用二维数组构造一个矩阵。
  - 参数:
    - `initialValues`: 二维数组形式的初始值。

### 索引器

#### 1. this[int row, int column]
- `public ref T this[int row, int column]`
  - 获取或设置指定位置的元素。
  - 参数:
    - `row`: 行索引。
    - `column`: 列索引。
  - 返回值: 指定位置的元素。

### 隐式转换

#### 1. 隐式转换为二维数组
- `public static implicit operator T[,](Matrix<T> matrix)`
  - 将矩阵隐式转换为二维数组。
  - 参数:
    - `matrix`: 要转换的矩阵。

#### 2. 隐式转换为锯齿数组
- `public static implicit operator T[][](Matrix<T> matrix)`
  - 将矩阵隐式转换为锯齿数组。
  - 参数:
    - `matrix`: 要转换的矩阵。

#### 3. 隐式转换为矩阵
- `public static implicit operator Matrix<T>(T[][] values)`
  - 将锯齿数组隐式转换为矩阵。
  - 参数:
    - `values`: 要转换的锯齿数组。

- `public static implicit operator Matrix<T>(T[,] values)`
  - 将二维数组隐式转换为矩阵。
  - 参数:
    - `values`: 要转换的二维数组。

### 运算符

#### 1. 矩阵加法
- `public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)`
  - 矩阵加法运算符。
  - 参数:
    - `a`: 第一个矩阵。
    - `b`: 第二个矩阵。
  - 返回值: 两个矩阵的和。

#### 2. 矩阵减法
- `public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)`
  - 矩阵减法运算符。
  - 参数:
    - `a`: 第一个矩阵。
    - `b`: 第二个矩阵。
  - 返回值: 两个矩阵的差。

#### 3. 矩阵乘法
- `public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)`
  - 矩阵乘法运算符。
  - 参数:
    - `a`: 第一个矩阵。
    - `b`: 第二个矩阵。
  - 返回值: 两个矩阵的积。

#### 4. 矩阵与标量乘法
- `public static Matrix<T> operator *(Matrix<T> matrix, T scalar)`
  - 矩阵与标量乘法运算符。
  - 参数:
    - `matrix`: 矩阵。
    - `scalar`: 标量。
  - 返回值: 矩阵与标量的积。

#### 5. 矩阵与标量除法
- `public static Matrix<T> operator /(Matrix<T> matrix, T scalar)`
  - 矩阵与标量除法运算符。
  - 参数:
    - `matrix`: 矩阵。
    - `scalar`: 标量。
  - 返回值: 矩阵与标量的商。

### 方法

#### 1. GetRow
- `public Span<T> GetRow(int rowIndex)`
  - 获取指定行的元素。
  - 参数:
    - `rowIndex`: 行索引。
  - 返回值: 指定行的元素的 Span。

#### 2. GetColumn
- `public ReadOnlySpan<T> GetColumn(int columnIndex)`
  - 获取指定列的元素。
  - 参数:
    - `columnIndex`: 列索引。
  - 返回值: 指定列的元素的 ReadOnlySpan。

#### 3. Transpose
- `public Matrix<T> Transpose()`
  - 矩阵转置。
  - 返回值: 转置后的矩阵。

#### 4. Determinant
- `public T Determinant()`
  - 计算矩阵的行列式。
  - 返回值: 矩阵的行列式。

#### 5. Inverse
- `public Matrix<T> Inverse()`
  - 计算矩阵的逆矩阵。
  - 返回值: 矩阵的逆矩阵。

#### 6. LUDecomposition
- `public void LUDecomposition(out Matrix<T> L, out Matrix<T> U)`
  - LU 分解。
  - 参数:
    - `L`: 输出的下三角矩阵。
    - `U`: 输出的上三角矩阵。

#### 7. QRDecomposition
- `public void QRDecomposition(out Matrix<T> Q, out Matrix<T> R)`
  - QR 分解。
  - 参数:
    - `Q`: 输出的正交矩阵。
    - `R`: 输出的上三角矩阵。

#### 8. CholeskyDecomposition
- `public Matrix<T> CholeskyDecomposition()`
  - Cholesky 分解。
  - 返回值: 下三角矩阵。

#### 9. Eye
- `public static Matrix<T> Eye(int size)`
  - 创建一个单位矩阵。
  - 参数:
    - `size`: 矩阵的大小（行数和列数）。
  - 返回值: 单位矩阵。

#### 10. Companion
- `public static Matrix<T> Companion(T[] a)`
  - 创建一个伴随矩阵。
  - 参数:
    - `a`: 输入数组，表示多项式的系数。
  - 返回值: 伴随矩阵。
  - 异常:
    - `ArgumentException`: 当输入数组的长度小于 2 或第一个系数为零时抛出。

#### 11. FillRandom
- `public void FillRandom()`
  - 用随机数填充矩阵。

#### 12. Clone
- `public Matrix<T> Clone()`
  - 创建矩阵的深拷贝。
  - 返回值: 矩阵的深拷贝。

#### 13. ToString
- `public override string ToString()`
  - 返回矩阵的字符串表示形式。
  - 返回值: 矩阵的字符串表示形式。

### 代码示例
以下是一个使用 Matrix<T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System; 
using Vorcyc.Mathematics.LinearAlgebra;

public class MatrixExample 
{ 
    public static void Main() 
    { 
        // 使用二维数组构造矩阵 
        Matrix<double> matrixA = new Matrix<double>(new double[,] { { 1.0, 2.0, 3.0 }, { 4.0, 5.0, 6.0 }, { 7.0, 8.0, 9.0 } });

        // 使用行数和列数构造矩阵
        Matrix<double> matrixB = new Matrix<double>(3, 3);
        matrixB.FillRandom();

        // 矩阵加法
        Matrix<double> sum = matrixA + matrixB;
        Console.WriteLine("Sum of matrices:\n" + sum);

        // 矩阵减法
        Matrix<double> difference = matrixA - matrixB;
        Console.WriteLine("Difference of matrices:\n" + difference);

        // 矩阵乘法
        Matrix<double> product = matrixA * matrixB;
        Console.WriteLine("Product of matrices:\n" + product);

        // 矩阵转置
        Matrix<double> transpose = matrixA.Transpose();
        Console.WriteLine("Transpose of matrixA:\n" + transpose);

        // 计算行列式
        double determinant = matrixA.Determinant();
        Console.WriteLine("Determinant of matrixA: " + determinant);

        // 计算逆矩阵
        Matrix<double> inverse = matrixA.Inverse();
        Console.WriteLine("Inverse of matrixA:\n" + inverse);

        // LU 分解
        matrixA.LUDecomposition(out Matrix<double> L, out Matrix<double> U);
        Console.WriteLine("L matrix from LU decomposition:\n" + L);
        Console.WriteLine("U matrix from LU decomposition:\n" + U);

        // QR 分解
        matrixA.QRDecomposition(out Matrix<double> Q, out Matrix<double> R);
        Console.WriteLine("Q matrix from QR decomposition:\n" + Q);
        Console.WriteLine("R matrix from QR decomposition:\n" + R);

        // Cholesky 分解
        Matrix<double> cholesky = matrixA.CholeskyDecomposition();
        Console.WriteLine("Cholesky decomposition of matrixA:\n" + cholesky);

        // 创建单位矩阵
        Matrix<double> identity = Matrix<double>.Eye(3);
        Console.WriteLine("Identity matrix:\n" + identity);

        // 创建伴随矩阵
        Matrix<double> companion = Matrix<double>.Companion(new double[] { 1.0, -6.0, 11.0, -6.0 });
        Console.WriteLine("Companion matrix:\n" + companion);

        // 矩阵深拷贝
        Matrix<double> clone = matrixA.Clone();
        Console.WriteLine("Clone of matrixA:\n" + clone);
    }
}
```

## Vorcyc.Mathematics.LinearAlgebra.MatrixHelper 类

Vorcyc.Mathematics.LinearAlgebra.MatrixHelper 是一个静态类，提供了多种矩阵和数组操作的辅助方法。

### 方法清单及说明

#### 1. GetLength
- `internal static int GetLength<T>(T[][] values, int dimension)`
  - 获取锯齿数组指定维度的长度。
  - 参数:
    - `values`: 锯齿数组。
    - `dimension`: 维度（1 或 2）。
  - 返回值: 指定维度的长度。

- `internal static int GetLength<T>(T[,] values, int dimension)`
  - 获取二维数组指定维度的长度。
  - 参数:
    - `values`: 二维数组。
    - `dimension`: 维度（1 或 2）。
  - 返回值: 指定维度的长度。

#### 2. GetTotalLength
- `public static int GetTotalLength(this Array array, bool deep = true, bool rectangular = true)`
  - 获取数组所有维度的总长度。
  - 参数:
    - `array`: 数组。
    - `deep`: 是否递归获取所有维度的长度。
    - `rectangular`: 是否假设数组是矩形的。
  - 返回值: 数组所有维度的总长度。

#### 3. GetLength
- `public static int[] GetLength(this Array array, bool deep = true, bool max = false)`
  - 获取数组每个维度的长度。
  - 参数:
    - `array`: 数组。
    - `deep`: 是否递归获取所有维度的长度。
    - `max`: 是否获取每个维度的最大长度。
  - 返回值: 数组每个维度的长度。

#### 4. IsJagged
- `public static bool IsJagged(this Array array)`
  - 判断数组是否为锯齿数组。
  - 参数:
    - `array`: 数组。
  - 返回值: 如果是锯齿数组则返回 true，否则返回 false。

#### 5. IsMatrix
- `public static bool IsMatrix(this Array array)`
  - 判断数组是否为多维数组。
  - 参数:
    - `array`: 数组。
  - 返回值: 如果是多维数组则返回 true，否则返回 false。

#### 6. IsVector
- `public static bool IsVector(this Array array)`
  - 判断数组是否为向量。
  - 参数:
    - `array`: 数组。
  - 返回值: 如果是向量则返回 true，否则返回 false。

#### 7. Concatenate
- `public static T[] Concatenate<T>(this T[] a, params T[] b)`
  - 水平合并两个向量。
  - 参数:
    - `a`: 第一个向量。
    - `b`: 第二个向量。
  - 返回值: 合并后的向量。

- `public static T[] Concatenate<T>(this T[] vector, T element)`
  - 水平合并向量和一个元素。
  - 参数:
    - `vector`: 向量。
    - `element`: 元素。
  - 返回值: 合并后的向量。

- `public static T[] Concatenate<T>(this T element, T[] vector)`
  - 水平合并一个元素和向量。
  - 参数:
    - `element`: 元素。
    - `vector`: 向量。
  - 返回值: 合并后的向量。

- `public static T[,] Concatenate<T>(this T[,] a, T[,] b)`
  - 水平合并两个二维数组。
  - 参数:
    - `a`: 第一个二维数组。
    - `b`: 第二个二维数组。
  - 返回值: 合并后的二维数组。

- `public static T[][] Concatenate<T>(this T[][] a, T[][] b)`
  - 水平合并两个锯齿数组。
  - 参数:
    - `a`: 第一个锯齿数组。
    - `b`: 第二个锯齿数组。
  - 返回值: 合并后的锯齿数组。

- `public static T[,] Concatenate<T>(params T[][,] matrices)`
  - 水平合并多个二维数组。
  - 参数:
    - `matrices`: 多个二维数组。
  - 返回值: 合并后的二维数组。

- `public static T[][] Concatenate<T>(params T[][][] matrices)`
  - 水平合并多个锯齿数组。
  - 参数:
    - `matrices`: 多个锯齿数组。
  - 返回值: 合并后的锯齿数组。

- `public static T[] Concatenate<T>(this T[][] vectors)`
  - 水平合并多个向量。
  - 参数:
    - `vectors`: 多个向量。
  - 返回值: 合并后的向量。

#### 8. Stack
- `public static T[][] Stack<T>(this T[][] a, T[][] b)`
  - 垂直合并两个锯齿数组。
  - 参数:
    - `a`: 第一个锯齿数组。
    - `b`: 第二个锯齿数组。
  - 返回值: 合并后的锯齿数组。

- `public static T[,] Stack<T>(params T[] elements)`
  - 垂直合并多个向量。
  - 参数:
    - `elements`: 多个向量。
  - 返回值: 合并后的二维数组。

- `public static T[,] Stack<T>(this T[] vector, T element)`
  - 垂直合并向量和一个元素。
  - 参数:
    - `vector`: 向量。
    - `element`: 元素。
  - 返回值: 合并后的二维数组。

- `public static T[,] Stack<T>(params T[][,] matrices)`
  - 垂直合并多个二维数组。
  - 参数:
    - `matrices`: 多个二维数组。
  - 返回值: 合并后的二维数组。

- `public static T[,] Stack<T>(this T[,] matrix, T[] vector)`
  - 垂直合并二维数组和向量。
  - 参数:
    - `matrix`: 二维数组。
    - `vector`: 向量。
  - 返回值: 合并后的二维数组。

- `public static T[][] Stack<T>(params T[][][] matrices)`
  - 垂直合并多个锯齿数组。
  - 参数:
    - `matrices`: 多个锯齿数组。
  - 返回值: 合并后的锯齿数组。

#### 9. Transpose
- `public static T[,] Transpose<T>(this T[,] matrix)`
  - 获取矩阵的转置。
  - 参数:
    - `matrix`: 矩阵。
  - 返回值: 转置后的矩阵。

- `public static T[,] Transpose<T>(this T[,] matrix, bool inPlace)`
  - 获取矩阵的转置。
  - 参数:
    - `matrix`: 矩阵。
    - `inPlace`: 是否在原矩阵上进行转置。
  - 返回值: 转置后的矩阵。

- `public static T[,] Transpose<T>(this T[] rowVector)`
  - 获取行向量的转置。
  - 参数:
    - `rowVector`: 行向量。
  - 返回值: 转置后的矩阵。

- `public static T[,] Transpose<T>(this T[] rowVector, T[,] result)`
  - 获取行向量的转置。
  - 参数:
    - `rowVector`: 行向量。
    - `result`: 存储转置结果的矩阵。
  - 返回值: 转置后的矩阵。

- `public static Array Transpose(this Array array, int[] order)`
  - 获取张量的广义转置。
  - 参数:
    - `array`: 张量。
    - `order`: 新的维度顺序。
  - 返回值: 转置后的张量。

- `public static T Transpose<T>(this T array, int[] order) where T : class, IList`
  - 获取张量的广义转置。
  - 参数:
    - `array`: 张量。
    - `order`: 新的维度顺序。
  - 返回值: 转置后的张量。

#### 10. Get
- `public static T[,] Get<T>(this T[,] source, int startRow, int endRow, int startColumn, int endColumn)`
  - 返回从当前矩阵中提取的子矩阵。
  - 参数:
    - `source`: 源矩阵。
    - `startRow`: 起始行索引。
    - `endRow`: 结束行索引。
    - `startColumn`: 起始列索引。
    - `endColumn`: 结束列索引。
  - 返回值: 提取的子矩阵。

- `public static T[,] Get<T>(this T[,] source, T[,] destination, int startRow, int endRow, int startColumn, int endColumn)`
  - 返回从当前矩阵中提取的子矩阵。
  - 参数:
    - `source`: 源矩阵。
    - `destination`: 存储结果的矩阵。
    - `startRow`: 起始行索引。
    - `endRow`: 结束行索引。
    - `startColumn`: 起始列索引。
    - `endColumn`: 结束列索引。
  - 返回值: 提取的子矩阵。

- `public static T[,] Get<T>(this T[,] source, int[] rowIndexes, int[] columnIndexes, T[,] result = null)`
  - 返回从当前矩阵中提取的子矩阵。
  - 参数:
    - `source`: 源矩阵。
    - `rowIndexes`: 行索引数组。
    - `columnIndexes`: 列索引数组。
    - `result`: 存储结果的矩阵。
  - 返回值: 提取的子矩阵。

- `public static T[,] Get<T>(this T[,] source, bool[] rowMask, bool[] columnMask, T[,] result = null)`
  - 返回从当前矩阵中提取的子矩阵。
  - 参数:
    - `source`: 源矩阵。
    - `rowMask`: 行掩码数组。
    - `columnMask`: 列掩码数组。
    - `result`: 存储结果的矩阵。
  - 返回值: 提取的子矩阵。

- `public static T[,] Get<T>(this T[,] source, int[] rowIndexes)`
  - 返回从当前矩阵中提取的子矩阵。
  - 参数:
    - `source`: 源矩阵。
    - `rowIndexes`: 行索引数组。
  - 返回值: 提取的子矩阵。

- `public static T[,] Get<T>(this T[,] source, int startRow, int endRow, int[] columnIndexes)`
  - 返回从当前矩阵中提取的子矩阵。
  - 参数:
    - `source`: 源矩阵。
    - `startRow`: 起始行索引。
    - `endRow`: 结束行索引。
    - `columnIndexes`: 列索引数组。
  - 返回值: 提取的子矩阵。

- `public static T[,] Get<T>(this T[,] source, int[] rowIndexes, int startColumn, int endColumn)`
  - 返回从当前矩阵中提取的子矩阵。
  - 参数:
    - `source`: 源矩阵。
    - `rowIndexes`: 行索引数组。
    - `startColumn`: 起始列索引。
    - `endColumn`: 结束列索引。
  - 返回值: 提取的子矩阵。

- `public static T[][] Get<T>(this T[][] source, int startRow, int endRow, int startColumn, int endColumn)`
  - 返回从当前锯齿数组中提取的子数组。
  - 参数:
    - `source`: 源锯齿数组。
    - `startRow`: 起始行索引。
    - `endRow`: 结束行索引。
    - `startColumn`: 起始列索引。
    - `endColumn`: 结束列索引。
  - 返回值: 提取的子数组。

- `public static T[][] Set<T>(this T[][] destination, int startRow, int endRow, int startColumn, int endColumn, T[][] values)`
  - 设置矩阵的子矩阵。
  - 参数:
    - `destination`: 目标矩阵。
    - `startRow`: 起始行索引。
    - `endRow`: 结束行索引。
    - `startColumn`: 起始列索引。
    - `endColumn`: 结束列索引。
    - `values`: 子矩阵的值。
  - 返回值: 设置后的矩阵。

- `public static T[][] Set<T>(this T[][] values, Func<T, bool> match, T value)`
  - 设置矩阵中满足条件的元素的值。
  - 参数:
    - `values`: 矩阵。
    - `match`: 用于确定是否更改元素的函数。
    - `value`: 要设置的值。
  - 返回值: 设置后的矩阵。

- `public static T[][] Get<T>(this T[][] source, int[] rowIndexes, int[] columnIndexes, bool reuseMemory = false, T[][] result = null)`
  - 返回从当前锯齿数组中提取的子数组。
  - 参数:
    - `source`: 源锯齿数组。
    - `rowIndexes`: 行索引数组。
    - `columnIndexes`: 列索引数组。
    - `reuseMemory`: 是否重用内存。
    - `result`: 存储结果的数组。
  - 返回值: 提取的子数组。

- `public static T[][] Get<T>(this T[][] source, bool[] rowMask, bool[] columnMask, bool reuseMemory = false, T[][] result = null)`
  - 返回从当前锯齿数组中提取的子数组。
  - 参数:
    - `source`: 源锯齿数组。
    - `rowMask`: 行掩码数组。
    - `columnMask`: 列掩码数组。
    - `reuseMemory`: 是否重用内存。
    - `result`: 存储结果的数组。
  - 返回值: 提取的子数组。

- `public static T[][] Get<T>(this T[][] source, int[] indexes, bool transpose = false)`
  - 返回从当前锯齿数组中提取的子数组。
  - 参数:
    - `source`: 源锯齿数组。
    - `indexes`: 索引数组。
    - `transpose`: 是否转置结果。
  - 返回值: 提取的子数组。

- `public static T[][] Get<T>(this T[][] source, int startRow, int endRow, int[] columnIndexes)`
  - 返回从当前锯齿数组中提取的子数组。
  - 参数:
    - `source`: 源锯齿数组。
    - `startRow`: 起始行索引。
    - `endRow`: 结束行索引。
    - `columnIndexes`: 列索引数组。
  - 返回值: 提取的子数组。

- `public static T[] Get<T>(this T[] source, int[] indexes, bool inPlace = false)`
  - 返回从当前向量中提取的子向量。
  - 参数:
    - `source`: 源向量。
    - `indexes`: 索引数组。
    - `inPlace`: 是否在原向量上进行操作。
  - 返回值: 提取的子向量。

- `public static T[] Get<T>(this T[] source, IList<int> indexes)`
  - 返回从当前向量中提取的子向量。
  - 参数:
    - `source`: 源向量。
    - `indexes`: 索引列表。
  - 返回值: 提取的子向量。

- `public static T[] Get<T>(this T[] source, int startRow, int endRow)`
  - 返回从当前向量中提取的子向量。
  - 参数:
    - `source`: 源向量。
    - `startRow`: 起始行索引。
    - `endRow`: 结束行索引。
  - 返回值: 提取的子向量。

- `public static T Get<T>(this T[] source, int index)`
  - 返回从当前向量中提取的值。
  - 参数:
    - `source`: 源向量。
    - `index`: 索引。
  - 返回值: 提取的值。

- `public static List<T> Get<T>(this List<T> source, int[] indexes)`
  - 返回从当前列表中提取的子列表。
  - 参数:
    - `source`: 源列表。
    - `indexes`: 索引数组。
  - 返回值: 提取的子列表。

#### 11. Rows
- `public static int Rows<T>(this T[] vector)`
  - 获取向量的行数。
  - 参数:
    - `vector`: 向量。
  - 返回值: 向量的行数。

- `public static int Rows<T>(this T[,] matrix)`
  - 获取矩阵的行数。
  - 参数:
    - `matrix`: 矩阵。
  - 返回值: 矩阵的行数。

#### 12. Columns
- `public static int Columns<T>(this T[,] matrix)`
  - 获取矩阵的列数。
  - 参数:
    - `matrix`: 矩阵。
  - 返回值: 矩阵的列数。

#### 13. Count
- `public static int Count<T>(this T[] data, Func<T, bool> func)`
  - 获取满足条件的元素数量。
  - 参数:
    - `data`: 数组。
    - `func`: 条件函数。
  - 返回值: 满足条件的元素数量。

#### 14. First
- `public static int First<T>(this T[] data, Func<T, bool> func)`
  - 获取第一个满足条件的元素的索引。
  - 参数:
    - `data`: 数组。
    - `func`: 条件函数。
  - 返回值: 第一个满足条件的元素的索引。

#### 15. FirstOrNull
- `public static int? FirstOrNull<T>(this T[] data, Func<T, bool> func)`
  - 获取第一个满足条件的元素的索引，如果没有找到则返回 null。
  - 参数:
    - `data`: 数组。
    - `func`: 条件函数。
  - 返回值: 第一个满足条件的元素的索引，如果没有找到则返回 null。

#### 16. IndexOf
- `public static int IndexOf<T>(this T[] data, T value)`
  - 搜索指定的值并返回数组中第一次出现的索引。
  - 参数:
    - `data`: 数组。
    - `value`: 要搜索的值。
  - 返回值: 搜索到的值在数组中的索引，如果未找到则返回 -1。

#### 17. Find
- `public static int[] Find<T>(this T[] data, Func<T, bool> func)`
  - 获取所有满足条件的元素的索引。
  - 参数:
    - `data`: 数组。
    - `func`: 条件函数。
  - 返回值: 满足条件的元素的索引数组。

- `public static int[] Find<T>(this T[] data, Func<T, bool> func, bool firstOnly)`
  - 获取所有满足条件的元素的索引。
  - 参数:
    - `data`: 数组。
    - `func`: 条件函数。
    - `firstOnly`: 是否只获取第一个满足条件的元素的索引。
  - 返回值: 满足条件的元素的索引数组。

- `public static int[][] Find<T>(this T[,] data, Func<T, bool> func)`
  - 获取所有满足条件的元素的索引。
  - 参数:
    - `data`: 二维数组。
    - `func`: 条件函数。
  - 返回值: 满足条件的元素的索引数组。

- `public static int[][] Find<T>(this T[,] data, Func<T, bool> func, bool firstOnly)`
  - 获取所有满足条件的元素的索引。
  - 参数:
    - `data`: 二维数组。
    - `func`: 条件函数。
    - `firstOnly`: 是否只获取第一个满足条件的元素的索引。
  - 返回值: 满足条件的元素的索引数组。

### 代码示例
以下是一个使用 MatrixHelper 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System; 
using Vorcyc.Mathematics.LinearAlgebra;

public class MatrixHelperExample 
{ 
    public static void Main() 
    { 
        // 定义二维数组 
        int[,] matrix = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };

        // 获取矩阵的转置
        int[,] transpose = matrix.Transpose();
        Console.WriteLine("Transpose of matrix:");
        PrintMatrix(transpose);

        // 获取子矩阵
        int[,] subMatrix = matrix.Get(0, 2, 0, 2);
        Console.WriteLine("SubMatrix of matrix:");
        PrintMatrix(subMatrix);

        // 合并两个矩阵
        int[,] concatenatedMatrix = matrix.Concatenate(matrix);
        Console.WriteLine("Concatenated matrix:");
        PrintMatrix(concatenatedMatrix);

        // 获取满足条件的元素的索引
        int[][] indices = matrix.Find(x => x > 5);
        Console.WriteLine("Indices of elements greater than 5:");
        foreach (var index in indices)
        {
            Console.WriteLine($"({index[0]}, {index[1]})");
        }
    }

    private static void PrintMatrix(int[,] matrix)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Console.Write(matrix[i, j] + " ");
            }
            Console.WriteLine();
        }
    }
}
```

## Vorcyc.Mathematics.LinearAlgebra.MatrixOperations 类

Vorcyc.Mathematics.LinearAlgebra.MatrixOperations 是一个静态类，提供了执行矩阵运算的静态方法。

### 方法清单及说明

#### 1. Multiply
- `public static T[,] Multiply<T>(T[,] a, T[,] b) where T : INumber<T>`
  - 乘法运算符，用于两个矩阵相乘。
  - 参数:
    - `a`: 第一个矩阵。
    - `b`: 第二个矩阵。
  - 返回值: 两个矩阵的乘积。
  - 异常:
    - `ArgumentException`: 当矩阵的维度不兼容时抛出。

- `public static float[][] Multiply(float[][] a, float[][] b)`
  - 乘法运算符，用于两个锯齿数组矩阵相乘。
  - 参数:
    - `a`: 第一个锯齿数组矩阵。
    - `b`: 第二个锯齿数组矩阵。
  - 返回值: 两个锯齿数组矩阵的乘积。
  - 异常:
    - `ArgumentException`: 当矩阵的维度不兼容时抛出。

### 代码示例
以下是一个使用 MatrixOperations 类中 Multiply 方法的示例，并在示例中加入了注释：
```csharp
using System; 
using Vorcyc.Mathematics.LinearAlgebra;

public class MatrixOperationsExample 
{ 
    public static void Main() 
    { 
        // 定义两个二维数组矩阵 
        int[,] matrixA = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
        int[,] matrixB = new int[,]
        {
            { 9, 8, 7 },
            { 6, 5, 4 },
            { 3, 2, 1 }
        };

        // 矩阵乘法
        int[,] product = MatrixOperations.Multiply(matrixA, matrixB);
        Console.WriteLine("Product of matrixA and matrixB:");
        PrintMatrix(product);

        // 定义两个锯齿数组矩阵
        float[][] jaggedMatrixA = new float[][]
        {
            new float[] { 1.0f, 2.0f, 3.0f },
            new float[] { 4.0f, 5.0f, 6.0f },
            new float[] { 7.0f, 8.0f, 9.0f }
        };

        float[][] jaggedMatrixB = new float[][]
        {
            new float[] { 9.0f, 8.0f, 7.0f },
            new float[] { 6.0f, 5.0f, 4.0f },
            new float[] { 3.0f, 2.0f, 1.0f }
        };

        // 锯齿数组矩阵乘法
        float[][] jaggedProduct = MatrixOperations.Multiply(jaggedMatrixA, jaggedMatrixB);
        Console.WriteLine("Product of jaggedMatrixA and jaggedMatrixB:");
        PrintJaggedMatrix(jaggedProduct);
    }

    private static void PrintMatrix(int[,] matrix)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Console.Write(matrix[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    private static void PrintJaggedMatrix(float[][] matrix)
    {
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[i].Length; j++)
            {
                Console.Write(matrix[i][j] + " ");
            }
            Console.WriteLine();
        }
    }
}

```

## Vorcyc.Mathematics.LinearAlgebra.Quaternion 结构

Vorcyc.Mathematics.LinearAlgebra.Quaternion 是一个表示四元数的泛型结构，提供了四元数的基本操作和运算方法。

### 属性

#### 1. W
- `public T W { get; }`
  - 获取四元数的 W 分量。

#### 2. X
- `public T X { get; }`
  - 获取四元数的 X 分量。

#### 3. Y
- `public T Y { get; }`
  - 获取四元数的 Y 分量。

#### 4. Z
- `public T Z { get; }`
  - 获取四元数的 Z 分量。

### 构造器

#### 1. Quaternion(T w, T x, T y, T z)
- `public Quaternion(T w, T x, T y, T z)`
  - 使用指定的分量构造一个四元数。
  - 参数:
    - `w`: W 分量。
    - `x`: X 分量。
    - `y`: Y 分量。
    - `z`: Z 分量。

### 方法

#### 1. Conjugate
- `public Quaternion<T> Conjugate()`
  - 返回四元数的共轭。
  - 返回值: 四元数的共轭。

#### 2. Norm
- `public T Norm()`
  - 返回四元数的范数（模）。
  - 返回值: 四元数的范数。

#### 3. Normalize
- `public Quaternion<T> Normalize()`
  - 返回归一化的四元数。
  - 返回值: 归一化的四元数。

#### 4. Inverse
- `public Quaternion<T> Inverse()`
  - 返回四元数的逆。
  - 返回值: 四元数的逆。

### 运算符

#### 1. 加法运算符
- `public static Quaternion<T> operator +(Quaternion<T> a, Quaternion<T> b)`
  - 加法运算符，用于两个四元数相加。
  - 参数:
    - `a`: 第一个四元数。
    - `b`: 第二个四元数。
  - 返回值: 两个四元数的和。

#### 2. 减法运算符
- `public static Quaternion<T> operator -(Quaternion<T> a, Quaternion<T> b)`
  - 减法运算符，用于一个四元数减去另一个四元数。
  - 参数:
    - `a`: 第一个四元数。
    - `b`: 第二个四元数。
  - 返回值: 两个四元数的差。

#### 3. 乘法运算符
- `public static Quaternion<T> operator *(Quaternion<T> a, Quaternion<T> b)`
  - 乘法运算符，用于两个四元数相乘。
  - 参数:
    - `a`: 第一个四元数。
    - `b`: 第二个四元数。
  - 返回值: 两个四元数的积。

#### 4. 除法运算符
- `public static Quaternion<T> operator /(Quaternion<T> a, Quaternion<T> b)`
  - 除法运算符，用于一个四元数除以另一个四元数。
  - 参数:
    - `a`: 第一个四元数。
    - `b`: 第二个四元数。
  - 返回值: 两个四元数的商。

#### 5. 相等运算符
- `public static bool operator ==(Quaternion<T> left, Quaternion<T> right)`
  - 相等运算符，用于判断两个四元数是否相等。
  - 参数:
    - `left`: 第一个四元数。
    - `right`: 第二个四元数。
  - 返回值: 如果两个四元数相等，则返回 true；否则返回 false。

#### 6. 不相等运算符
- `public static bool operator !=(Quaternion<T> left, Quaternion<T> right)`
  - 不相等运算符，用于判断两个四元数是否不相等。
  - 参数:
    - `left`: 第一个四元数。
    - `right`: 第二个四元数。
  - 返回值: 如果两个四元数不相等，则返回 true；否则返回 false。

### 重写方法

#### 1. ToString
- `public override string ToString()`
  - 返回四元数的字符串表示形式。
  - 返回值: 四元数的字符串表示形式。

#### 2. Equals
- `public override bool Equals(object? obj)`
  - 判断指定对象是否与当前四元数相等。
  - 参数:
    - `obj`: 要与当前四元数进行比较的对象。
  - 返回值: 如果指定对象与当前四元数相等，则返回 true；否则返回 false。

#### 3. GetHashCode
- `public override int GetHashCode()`
  - 返回当前四元数的哈希代码。
  - 返回值: 当前四元数的哈希代码。

### 代码示例
以下是一个使用 Quaternion 结构中多个方法的示例，并在示例中加入了注释：

```csharp
using System; 
using Vorcyc.Mathematics.LinearAlgebra;

public class QuaternionExample 
{ 
    public static void Main() 
    { 
        // 定义四元数 
        Quaternion<double> quaternionA = new Quaternion<double>(1.0, 2.0, 3.0, 4.0); 
        Quaternion<double> quaternionB = new Quaternion<double>(5.0, 6.0, 7.0, 8.0);
        // 四元数加法
        Quaternion<double> sum = quaternionA + quaternionB;
        Console.WriteLine("Sum of quaternionA and quaternionB: " + sum);

        // 四元数减法
        Quaternion<double> difference = quaternionA - quaternionB;
        Console.WriteLine("Difference of quaternionA and quaternionB: " + difference);

        // 四元数乘法
        Quaternion<double> product = quaternionA * quaternionB;
        Console.WriteLine("Product of quaternionA and quaternionB: " + product);

        // 四元数除法
        Quaternion<double> quotient = quaternionA / quaternionB;
        Console.WriteLine("Quotient of quaternionA and quaternionB: " + quotient);

        // 四元数共轭
        Quaternion<double> conjugate = quaternionA.Conjugate();
        Console.WriteLine("Conjugate of quaternionA: " + conjugate);

        // 四元数范数
        double norm = quaternionA.Norm();
        Console.WriteLine("Norm of quaternionA: " + norm);

        // 四元数归一化
        Quaternion<double> normalized = quaternionA.Normalize();
        Console.WriteLine("Normalized quaternionA: " + normalized);

        // 四元数逆
        Quaternion<double> inverse = quaternionA.Inverse();
        Console.WriteLine("Inverse of quaternionA: " + inverse);
    }
}
```


## Vorcyc.Mathematics.LinearAlgebra.Tensor 类

Vorcyc.Mathematics.LinearAlgebra.Tensor 是一个表示三维张量的类，提供了张量的基本操作和运算方法。

### 属性

#### 1. Values
- `public float[] Values { get; }`
  - 获取张量的值。

#### 2. Width
- `public int Width { get; }`
  - 获取张量的宽度。

#### 3. Height
- `public int Height { get; }`
  - 获取张量的高度。

#### 4. Depth
- `public int Depth { get; }`
  - 获取张量的深度。

### 构造器

#### 1. Tensor(int w, int h, int d)
- `public Tensor(int w, int h, int d)`
  - 使用指定的大小初始化张量。
  - 参数:
    - `w`: 宽度。
    - `h`: 高度。
    - `d`: 深度。

#### 2. Tensor(int w, int h, int d, float initialValue)
- `public Tensor(int w, int h, int d, float initialValue)`
  - 使用指定的大小和初始值初始化张量。
  - 参数:
    - `w`: 宽度。
    - `h`: 高度。
    - `d`: 深度。
    - `initialValue`: 所有元素的初始值。

#### 3. Tensor(float[,,] array)
- `public Tensor(float[,,] array)`
  - 从三维数组初始化张量。
  - 参数:
    - `array`: 用于初始化张量的三维数组。

### 索引器

#### 1. this[int x, int y, int z]
- `public ref float this[int x, int y, int z]`
  - 获取或设置指定坐标的值。
  - 参数:
    - `x`: X 坐标（宽度）。
    - `y`: Y 坐标（高度）。
    - `z`: Z 坐标（深度）。
  - 返回值: 指定坐标的值。

### 方法

#### 1. Fill
- `public void Fill(float value)`
  - 用指定的值填充张量。
  - 参数:
    - `value`: 用于填充张量的值。

#### 2. Add
- `public void Add(Tensor other)`
  - 将另一个张量加到此张量。
  - 参数:
    - `other`: 要加的张量。

#### 3. Subtract
- `public void Subtract(Tensor other)`
  - 从此张量中减去另一个张量。
  - 参数:
    - `other`: 要减去的张量。

#### 4. Multiply
- `public void Multiply(float scalar)`
  - 将此张量乘以一个标量值。
  - 参数:
    - `scalar`: 要乘以的标量值。

#### 5. Transpose
- `public Tensor Transpose(int axis1, int axis2)`
  - 沿指定轴转置张量。
  - 参数:
    - `axis1`: 第一个轴。
    - `axis2`: 第二个轴。
  - 返回值: 转置后的新张量。

#### 6. Sum
- `public float Sum()`
  - 计算张量中所有元素的和。
  - 返回值: 张量中所有元素的和。

#### 7. Dot
- `public float Dot(Tensor other)`
  - 计算两个张量的点积。
  - 参数:
    - `other`: 另一个张量。
  - 返回值: 两个张量的点积。

#### 8. Normalize
- `public void Normalize()`
  - 归一化张量。

#### 9. Norm
- `public float Norm()`
  - 计算张量的范数。
  - 返回值: 张量的范数。

#### 10. Slice
- `public Tensor Slice(int axis, int index)`
  - 沿指定轴切片张量。
  - 参数:
    - `axis`: 要切片的轴。
    - `index`: 切片的索引。
  - 返回值: 切片后的新张量。

#### 11. Clone
- `public Tensor Clone()`
  - 克隆张量。
  - 返回值: 一个新的张量，它是当前张量的副本。

### 运算符

#### 1. 加法运算符
- `public static Tensor operator +(Tensor a, Tensor b)`
  - 加法运算符，用于两个张量相加。
  - 参数:
    - `a`: 第一个张量。
    - `b`: 第二个张量。
  - 返回值: 两个张量的和。

#### 2. 减法运算符
- `public static Tensor operator -(Tensor a, Tensor b)`
  - 减法运算符，用于一个张量减去另一个张量。
  - 参数:
    - `a`: 第一个张量。
    - `b`: 第二个张量。
  - 返回值: 两个张量的差。

#### 3. 乘法运算符
- `public static Tensor operator *(Tensor tensor, float scalar)`
  - 乘法运算符，用于张量乘以一个标量值。
  - 参数:
    - `tensor`: 张量。
    - `scalar`: 标量值。
  - 返回值: 张量乘以标量值的结果。

- `public static Tensor operator *(float scalar, Tensor tensor)`
  - 乘法运算符，用于张量乘以一个标量值。
  - 参数:
    - `scalar`: 标量值。
    - `tensor`: 张量。
  - 返回值: 张量乘以标量值的结果。

#### 4. 相等运算符
- `public static bool operator ==(Tensor a, Tensor b)`
  - 相等运算符，用于判断两个张量是否相等。
  - 参数:
    - `a`: 第一个张量。
    - `b`: 第二个张量。
  - 返回值: 如果两个张量相等，则返回 true；否则返回 false。

#### 5. 不相等运算符
- `public static bool operator !=(Tensor a, Tensor b)`
  - 不相等运算符，用于判断两个张量是否不相等。
  - 参数:
    - `a`: 第一个张量。
    - `b`: 第二个张量。
  - 返回值: 如果两个张量不相等，则返回 true；否则返回 false。

### 重写方法

#### 1. ToString
- `public override string ToString()`
  - 返回张量的字符串表示形式。
  - 返回值: 张量的字符串表示形式。

#### 2. Equals
- `public override bool Equals(object? obj)`
  - 判断指定对象是否与当前张量相等。
  - 参数:
    - `obj`: 要与当前张量进行比较的对象。
  - 返回值: 如果指定对象与当前张量相等，则返回 true；否则返回 false。

#### 3. GetHashCode
- `public override int GetHashCode()`
  - 返回当前张量的哈希代码。
  - 返回值: 当前张量的哈希代码。

### 代码示例
以下是一个使用 Tensor 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System; 
using Vorcyc.Mathematics.LinearAlgebra;

public class TensorExample 
{
    public static void Main()
    { 
        // 定义张量 
        Tensor tensorA = new Tensor(2, 2, 2, 1.0f); 
        Tensor tensorB = new Tensor(new float[,,] { { { 1.0f, 2.0f }, { 3.0f, 4.0f } }, { { 5.0f, 6.0f }, { 7.0f, 8.0f } } });

        // 张量加法
        Tensor sum = tensorA + tensorB;
        Console.WriteLine("Sum of tensorA and tensorB: " + sum);

        // 张量减法
        Tensor difference = tensorA - tensorB;
        Console.WriteLine("Difference of tensorA and tensorB: " + difference);

        // 张量乘法
        Tensor product = tensorA * 2.0f;
        Console.WriteLine("Product of tensorA and scalar: " + product);

        // 张量转置
        Tensor transposed = tensorB.Transpose(0, 1);
        Console.WriteLine("Transposed tensorB: " + transposed);

        // 张量和
        float sumOfElements = tensorB.Sum();
        Console.WriteLine("Sum of elements in tensorB: " + sumOfElements);

        // 张量点积
        float dotProduct = tensorA.Dot(tensorB);
        Console.WriteLine("Dot product of tensorA and tensorB: " + dotProduct);

        // 张量归一化
        tensorB.Normalize();
        Console.WriteLine("Normalized tensorB: " + tensorB);

        // 张量范数
        float norm = tensorB.Norm();
        Console.WriteLine("Norm of tensorB: " + norm);

        // 张量切片
        Tensor slice = tensorB.Slice(0, 1);
        Console.WriteLine("Slice of tensorB along axis 0 at index 1: " + slice);

        // 张量克隆
        Tensor clone = tensorB.Clone();
        Console.WriteLine("Clone of tensorB: " + clone);
    }
}
```


## Vorcyc.Mathematics.LinearAlgebra.Tensor&lt;T> 类

Vorcyc.Mathematics.LinearAlgebra.Tensor&lt;T> 是一个表示三维张量的泛型类，提供了张量的基本操作和运算方法。

### 属性

#### 1. Values
- `public Span<T> Values { get; }`
  - 获取张量的值。

#### 2. Width
- `public int Width { get; }`
  - 获取张量的宽度。

#### 3. Height
- `public int Height { get; }`
  - 获取张量的高度。

#### 4. Depth
- `public int Depth { get; }`
  - 获取张量的深度。

### 构造器

#### 1. Tensor(int w, int h, int d)
- `public Tensor(int w, int h, int d)`
  - 使用指定的大小初始化张量。
  - 参数:
    - `w`: 宽度。
    - `h`: 高度。
    - `d`: 深度。

#### 2. Tensor(int w, int h, int d, T initialValue)
- `public Tensor(int w, int h, int d, T initialValue)`
  - 使用指定的大小和初始值初始化张量。
  - 参数:
    - `w`: 宽度。
    - `h`: 高度。
    - `d`: 深度。
    - `initialValue`: 所有元素的初始值。

#### 3. Tensor(T[,,] array)
- `public Tensor(T[,,] array)`
  - 从三维数组初始化张量。
  - 参数:
    - `array`: 用于初始化张量的三维数组。

### 索引器

#### 1. this[int x, int y, int z]
- `public ref T this[int x, int y, int z]`
  - 获取或设置指定坐标的值。
  - 参数:
    - `x`: X 坐标（宽度）。
    - `y`: Y 坐标（高度）。
    - `z`: Z 坐标（深度）。
  - 返回值: 指定坐标的值。

### 方法

#### 1. Fill
- `public void Fill(T value)`
  - 用指定的值填充张量。
  - 参数:
    - `value`: 用于填充张量的值。

#### 2. Add
- `public void Add(Tensor<T> other)`
  - 将另一个张量加到此张量。
  - 参数:
    - `other`: 要加的张量。

#### 3. Subtract
- `public void Subtract(Tensor<T> other)`
  - 从此张量中减去另一个张量。
  - 参数:
    - `other`: 要减去的张量。

#### 4. Multiply
- `public void Multiply(T scalar)`
  - 将此张量乘以一个标量值。
  - 参数:
    - `scalar`: 要乘以的标量值。

#### 5. Transpose
- `public Tensor<T> Transpose(int axis1, int axis2)`
  - 沿指定轴转置张量。
  - 参数:
    - `axis1`: 第一个轴。
    - `axis2`: 第二个轴。
  - 返回值: 转置后的新张量。

#### 6. Sum
- `public T Sum()`
  - 计算张量中所有元素的和。
  - 返回值: 张量中所有元素的和。

#### 7. Dot
- `public T Dot(Tensor<T> other)`
  - 计算两个张量的点积。
  - 参数:
    - `other`: 另一个张量。
  - 返回值: 两个张量的点积。

#### 8. Normalize
- `public void Normalize()`
  - 归一化张量。

#### 9. Norm
- `public T Norm()`
  - 计算张量的范数。
  - 返回值: 张量的范数。

#### 10. Slice
- `public Tensor<T> Slice(int axis, int index)`
  - 沿指定轴切片张量。
  - 参数:
    - `axis`: 要切片的轴。
    - `index`: 切片的索引。
  - 返回值: 切片后的新张量。

#### 11. Clone
- `public Tensor<T> Clone()`
  - 克隆张量。
  - 返回值: 一个新的张量，它是当前张量的副本。

### 运算符

#### 1. 加法运算符
- `public static Tensor<T> operator +(Tensor<T> a, Tensor<T> b)`
  - 加法运算符，用于两个张量相加。
  - 参数:
    - `a`: 第一个张量。
    - `b`: 第二个张量。
  - 返回值: 两个张量的和。

#### 2. 减法运算符
- `public static Tensor<T> operator -(Tensor<T> a, Tensor<T> b)`
  - 减法运算符，用于一个张量减去另一个张量。
  - 参数:
    - `a`: 第一个张量。
    - `b`: 第二个张量。
  - 返回值: 两个张量的差。

#### 3. 乘法运算符
- `public static Tensor<T> operator *(Tensor<T> tensor, T scalar)`
  - 乘法运算符，用于张量乘以一个标量值。
  - 参数:
    - `tensor`: 张量。
    - `scalar`: 标量值。
  - 返回值: 张量乘以标量值的结果。

- `public static Tensor<T> operator *(T scalar, Tensor<T> tensor)`
  - 乘法运算符，用于张量乘以一个标量值。
  - 参数:
    - `scalar`: 标量值。
    - `tensor`: 张量。
  - 返回值: 张量乘以标量值的结果。

#### 4. 相等运算符
- `public static bool operator ==(Tensor<T> a, Tensor<T> b)`
  - 相等运算符，用于判断两个张量是否相等。
  - 参数:
    - `a`: 第一个张量。
    - `b`: 第二个张量。
  - 返回值: 如果两个张量相等，则返回 true；否则返回 false。

#### 5. 不相等运算符
- `public static bool operator !=(Tensor<T> a, Tensor<T> b)`
  - 不相等运算符，用于判断两个张量是否不相等。
  - 参数:
    - `a`: 第一个张量。
    - `b`: 第二个张量。
  - 返回值: 如果两个张量不相等，则返回 true；否则返回 false。

### 重写方法

#### 1. ToString
- `public override string ToString()`
  - 返回张量的字符串表示形式。
  - 返回值: 张量的字符串表示形式。

#### 2. Equals
- `public override bool Equals(object? obj)`
  - 判断指定对象是否与当前张量相等。
  - 参数:
    - `obj`: 要与当前张量进行比较的对象。
  - 返回值: 如果指定对象与当前张量相等，则返回 true；否则返回 false。

#### 3. GetHashCode
- `public override int GetHashCode()`
  - 返回当前张量的哈希代码。
  - 返回值: 当前张量的哈希代码。

### 代码示例
以下是一个使用 Tensor&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System; 
using Vorcyc.Mathematics.LinearAlgebra;

public class TensorExample 
{ 
    public static void Main() 
    { 
        // 定义张量 
        Tensor<double> tensorA = new Tensor<double>(2, 2, 2, 1.0); 
        Tensor<double> tensorB = new Tensor<double>(new double[,,] { { { 1.0, 2.0 }, { 3.0, 4.0 } }, { { 5.0, 6.0 }, { 7.0, 8.0 } } });

        // 张量加法
        Tensor<double> sum = tensorA + tensorB;
        Console.WriteLine("Sum of tensorA and tensorB: " + sum);

        // 张量减法
        Tensor<double> difference = tensorA - tensorB;
        Console.WriteLine("Difference of tensorA and tensorB: " + difference);

        // 张量乘法
        Tensor<double> product = tensorA * 2.0;
        Console.WriteLine("Product of tensorA and scalar: " + product);

        // 张量转置
        Tensor<double> transposed = tensorB.Transpose(0, 1);
        Console.WriteLine("Transposed tensorB: " + transposed);

        // 张量和
        double sumOfElements = tensorB.Sum();
        Console.WriteLine("Sum of elements in tensorB: " + sumOfElements);

        // 张量点积
        double dotProduct = tensorA.Dot(tensorB);
        Console.WriteLine("Dot product of tensorA and tensorB: " + dotProduct);

        // 张量归一化
        tensorB.Normalize();
        Console.WriteLine("Normalized tensorB: " + tensorB);

        // 张量范数
        double norm = tensorB.Norm();
        Console.WriteLine("Norm of tensorB: " + norm);

        // 张量切片
        Tensor<double> slice = tensorB.Slice(0, 1);
        Console.WriteLine("Slice of tensorB along axis 0 at index 1: " + slice);

        // 张量克隆
        Tensor<double> clone = tensorB.Clone();
        Console.WriteLine("Clone of tensorB: " + clone);
    }
}
```


## Vorcyc.Mathematics.LinearAlgebra.Vector<T> 类

Vorcyc.Mathematics.LinearAlgebra.Vector<T> 是一个表示数学向量的泛型类，提供了向量的基本操作和运算方法。

### 属性

#### 1. Elements
- `public T[] Elements { get; }`
  - 获取向量的元素。

#### 2. Dimension
- `public int Dimension { get; }`
  - 获取向量的维度。

### 构造器

#### 1. Vector(params T[] elements)
- `public Vector(params T[] elements)`
  - 使用指定的元素初始化向量。
  - 参数:
    - `elements`: 向量的元素。

### 方法

#### 1. Add
- `public static Vector<T> Add(Vector<T> v1, Vector<T> v2)`
  - 加法运算符，用于两个向量相加。
  - 参数:
    - `v1`: 第一个向量。
    - `v2`: 第二个向量。
  - 返回值: 两个向量的和。
  - 异常:
    - `ArgumentException`: 当向量的维度不匹配时抛出。

#### 2. Subtract
- `public static Vector<T> Subtract(Vector<T> v1, Vector<T> v2)`
  - 减法运算符，用于一个向量减去另一个向量。
  - 参数:
    - `v1`: 第一个向量。
    - `v2`: 第二个向量。
  - 返回值: 两个向量的差。
  - 异常:
    - `ArgumentException`: 当向量的维度不匹配时抛出。

#### 3. DotProduct
- `public static T DotProduct(Vector<T> v1, Vector<T> v2)`
  - 计算两个向量的点积。
  - 参数:
    - `v1`: 第一个向量。
    - `v2`: 第二个向量。
  - 返回值: 两个向量的点积。
  - 异常:
    - `ArgumentException`: 当向量的维度不匹配时抛出。

#### 4. Magnitude
- `public T Magnitude()`
  - 计算向量的模（长度）。
  - 返回值: 向量的模。

#### 5. Normalize
- `public Vector<T> Normalize()`
  - 返回归一化的向量。
  - 返回值: 归一化的向量。

### 运算符

#### 1. 加法运算符
- `public static Vector<T> operator +(Vector<T> v1, Vector<T> v2)`
  - 加法运算符，用于两个向量相加。
  - 参数:
    - `v1`: 第一个向量。
    - `v2`: 第二个向量。
  - 返回值: 两个向量的和。

#### 2. 减法运算符
- `public static Vector<T> operator -(Vector<T> v1, Vector<T> v2)`
  - 减法运算符，用于一个向量减去另一个向量。
  - 参数:
    - `v1`: 第一个向量。
    - `v2`: 第二个向量。
  - 返回值: 两个向量的差。

#### 3. 点积运算符
- `public static T operator *(Vector<T> v1, Vector<T> v2)`
  - 点积运算符，用于计算两个向量的点积。
  - 参数:
    - `v1`: 第一个向量。
    - `v2`: 第二个向量。
  - 返回值: 两个向量的点积。

#### 4. 矩阵与向量乘法运算符
- `public static Vector<T> operator *(Matrix<T> matrix, Vector<T> vector)`
  - 矩阵与向量乘法运算符。
  - 参数:
    - `matrix`: 矩阵。
    - `vector`: 向量。
  - 返回值: 矩阵与向量的乘积。
  - 异常:
    - `ArgumentException`: 当矩阵的列数与向量的维度不匹配时抛出。

### 代码示例
以下是一个使用 Vector<T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System; 
using Vorcyc.Mathematics.LinearAlgebra;

public class VectorExample 
{ 
    public static void Main()
    { 
        // 定义两个向量 
        Vector<double> vectorA = new Vector<double>(1.0, 2.0, 3.0); 
        Vector<double> vectorB = new Vector<double>(4.0, 5.0, 6.0);

        // 向量加法
        Vector<double> sum = vectorA + vectorB;
        Console.WriteLine("Sum of vectorA and vectorB: " + string.Join(", ", sum.Elements));

        // 向量减法
        Vector<double> difference = vectorA - vectorB;
        Console.WriteLine("Difference of vectorA and vectorB: " + string.Join(", ", difference.Elements));

        // 向量点积
        double dotProduct = vectorA * vectorB;
        Console.WriteLine("Dot product of vectorA and vectorB: " + dotProduct);

        // 向量模
        double magnitude = vectorA.Magnitude();
        Console.WriteLine("Magnitude of vectorA: " + magnitude);

        // 向量归一化
        Vector<double> normalized = vectorA.Normalize();
        Console.WriteLine("Normalized vectorA: " + string.Join(", ", normalized.Elements));

        // 矩阵与向量乘法
        Matrix<double> matrix = new Matrix<double>(new double[,] 
        {
            { 1.0, 2.0, 3.0 },
            { 4.0, 5.0, 6.0 },
            { 7.0, 8.0, 9.0 }
        });
        Vector<double> matrixVectorProduct = matrix * vectorA;
        Console.WriteLine("Matrix and vector product: " + string.Join(", ", matrixVectorProduct.Elements));
    }
}
```