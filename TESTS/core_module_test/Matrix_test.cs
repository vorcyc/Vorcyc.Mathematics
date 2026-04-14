using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.Framework.Utilities;

namespace core_module_test;

internal class Matrix_test
{


    public static void go()
    {
        Tensor4D_test.Run();
        TestMatrixDecomposition();
        TestSingularValueDecomposition();
        TestVectorSpanAndMatrixMultiply();
        TestMatrixSolveWrappers();
        TestBidiagonalSvdLarge();

        var m1 = new Matrix(4, 3);
        for (int i = 0; i < m1.Rows; i++)
        {
            for (int j = 0; j < m1.Columns; j++)
            {
                m1[i, j] = i * 3 + j;
            }
        }

        Console.WriteLine(m1);


        m1.QRDecomposition(out var q1, out var r1);

        Console.WriteLine(q1);
        Console.WriteLine(r1);



        new string('-', 20).PrintLine( ConsoleColor.Red);

        var m = new Matrix<double>(4, 3);
        for (int i = 0; i < m.Rows; i++)
        {
            for (int j = 0; j < m.Columns; j++)
            {
                m[i, j] = i * 3 + j;
            }
        }

        Console.WriteLine(m);


        m.QRDecomposition(out var q, out var r);

        Console.WriteLine(q);
        Console.WriteLine(r);



    }

    static void TestMatrixDecomposition()
    {
        var symmetric = new Matrix<double>(new double[,]
        {
            { 2.0, 1.0 },
            { 1.0, 2.0 }
        });

        var eig = MatrixDecomposition.SymmetricEigendecomposition(symmetric);
        if (Math.Abs(eig.Eigenvalues[0] - 3.0) > 1e-8 || Math.Abs(eig.Eigenvalues[1] - 1.0) > 1e-8)
            throw new InvalidOperationException("SymmetricEigendecomposition eigenvalues failed.");

        var reconstructed = eig.Eigenvectors * new Matrix<double>(new double[,]
        {
            { eig.Eigenvalues[0], 0.0 },
            { 0.0, eig.Eigenvalues[1] }
        }) * eig.Eigenvectors.Transpose();

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                if (Math.Abs(reconstructed[i, j] - symmetric[i, j]) > 1e-6)
                    throw new InvalidOperationException("SymmetricEigendecomposition reconstruction failed.");
            }
        }
    }

    static void TestSingularValueDecomposition()
    {
        var diagonal = new Matrix<double>(new double[,]
        {
            { 3.0, 0.0 },
            { 0.0, 2.0 }
        });

        var svd = MatrixDecomposition.SingularValueDecomposition(diagonal);
        var reconstructed = MatrixDecomposition.Reconstruct(svd);
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                if (Math.Abs(reconstructed[i, j] - diagonal[i, j]) > 1e-6)
                    throw new InvalidOperationException("SVD reconstruction failed for diagonal matrix.");
            }
        }

        if (Math.Abs(svd.SingularValues[0] - 3.0) > 1e-6 || Math.Abs(svd.SingularValues[1] - 2.0) > 1e-6)
            throw new InvalidOperationException("SVD singular values failed.");

        var tall = new Matrix<double>(new double[,]
        {
            { 1.0, 0.0 },
            { 0.0, 1.0 },
            { 0.0, 0.0 },
            { 0.0, 0.0 }
        });
        var tallSvd = MatrixDecomposition.SingularValueDecomposition(tall);
        var tallReconstructed = MatrixDecomposition.Reconstruct(tallSvd);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                if (Math.Abs(tallReconstructed[i, j] - tall[i, j]) > 1e-6)
                    throw new InvalidOperationException("Tall-matrix SVD reconstruction failed.");
            }
        }

        var wide = new Matrix<double>(new double[,]
        {
            { 2.0, 0.0, 0.0 },
            { 0.0, 3.0, 0.0 }
        });
        var wideSvd = MatrixDecomposition.SingularValueDecomposition(wide);
        if (Math.Abs(wideSvd.SingularValues[0] - 3.0) > 1e-6 || Math.Abs(wideSvd.SingularValues[1] - 2.0) > 1e-6)
            throw new InvalidOperationException("Wide-matrix SVD singular values failed.");

        var design = new Matrix<double>(new double[,]
        {
            { 1.0, 1.0 },
            { 1.0, 2.0 },
            { 1.0, 3.0 }
        });
        double[] y = { 6.0, 8.0, 10.0 };
        var x = LinearEquationSolver.SolveLeastSquaresSvd(design, y);
        if (Math.Abs(x[0] - 4.0) > 1e-4 || Math.Abs(x[1] - 2.0) > 1e-4)
            throw new InvalidOperationException("SolveLeastSquaresSvd failed.");

        var rankDeficient = new Matrix<double>(new double[,]
        {
            { 1.0, 2.0, 3.0 },
            { 2.0, 4.0, 6.0 },
            { 1.0, 1.0, 1.0 }
        });
        double[] rhs = { 6.0, 12.0, 3.0 };
        var robust = LinearEquationSolver.SolveLeastSquares(rankDeficient, rhs);
        if (robust.Length != 3)
            throw new InvalidOperationException("Robust least squares failed.");

        var illConditioned = new Matrix<double>(new double[,]
        {
            { 1.0, 1.0000001 },
            { 1.0, 1.0000002 },
            { 1.0, 1.0000003 }
        });
        double[] illRhs = { 2.0, 2.0000001, 2.0000002 };
        var illSolution = LinearEquationSolver.SolveLeastSquares(illConditioned, illRhs);
        if (illSolution.Length != 2 || double.IsNaN(illSolution[0]) || double.IsNaN(illSolution[1]))
            throw new InvalidOperationException("Ill-conditioned least squares fallback failed.");

        if (MatrixDiagnostics.IsIllConditioned(illConditioned, 1e6))
            Console.WriteLine("Condition number diagnostics passed.");

        if (!diagonal.IsSymmetric())
            throw new InvalidOperationException("IsSymmetric failed.");

        var nearlyDependent = new Matrix<double>(new double[,]
        {
            { 1.0, 1.0 },
            { 1.0, 1.0000001 },
            { 1.0, 1.0000002 },
            { 1.0, 1.0000003 },
            { 1.0, 1.0000004 }
        });
        var ndSvd = MatrixDecomposition.SingularValueDecomposition(nearlyDependent);
        if (ndSvd.SingularValues[0] / ndSvd.SingularValues[^1] < 1e3)
            throw new InvalidOperationException("Jacobi SVD should expose large condition number for nearly dependent columns.");

        Console.WriteLine("SVD tests passed.");
    }

    static void TestVectorSpanAndMatrixMultiply()
    {
        double[] a = { 1.0, 2.0, 3.0 };
        double[] b = { 4.0, 5.0, 6.0 };

        if (Math.Abs(VectorSpan.Dot(a, b) - 32.0) > 1e-10)
            throw new InvalidOperationException("VectorSpan.Dot failed.");

        if (Math.Abs(VectorSpan.Sum(a) - 6.0) > 1e-10)
            throw new InvalidOperationException("VectorSpan.Sum failed.");

        double[] y = { 1.0, 1.0, 1.0 };
        VectorSpan.Axpy(2.0, a, y);
        if (Math.Abs(y[0] - 3.0) > 1e-10 || Math.Abs(y[2] - 7.0) > 1e-10)
            throw new InvalidOperationException("VectorSpan.Axpy failed.");

        var matrix = new Matrix<double>(new double[,]
        {
            { 1.0, 2.0, 3.0 },
            { 4.0, 5.0, 6.0 }
        });

        double[] product = matrix.Multiply(a);
        if (Math.Abs(product[0] - 14.0) > 1e-10 || Math.Abs(product[1] - 32.0) > 1e-10)
            throw new InvalidOperationException("Matrix<double>.Multiply failed.");

        var left = new Matrix<double>(new double[,] { { 1, 2 }, { 3, 4 } });
        var right = new Matrix<double>(new double[,] { { 5, 6 }, { 7, 8 } });
        var matProduct = left * right;
        if (Math.Abs(matProduct[0, 0] - 19) > 1e-10 || Math.Abs(matProduct[1, 1] - 50) > 1e-10)
            throw new InvalidOperationException("Matrix SIMD multiply failed.");

        Console.WriteLine("VectorSpan and Matrix.Multiply tests passed.");
    }

    static void TestMatrixSolveWrappers()
    {
        var square = new Matrix<double>(new double[,] { { 2, 1 }, { 1, 3 } });
        double[] rhs = { 5, 9 };
        var lu = square.Solve(rhs);
        var residual = square.Multiply(lu);
        if (Math.Abs(residual[0] - rhs[0]) > 1e-8 || Math.Abs(residual[1] - rhs[1]) > 1e-8)
            throw new InvalidOperationException("Matrix.Solve failed.");

        Span<double> workspace = stackalloc double[2];
        square.Solve(rhs, workspace);
        residual = square.Multiply(workspace);
        if (Math.Abs(residual[0] - rhs[0]) > 1e-8 || Math.Abs(residual[1] - rhs[1]) > 1e-8)
            throw new InvalidOperationException("Matrix.Solve(span) failed.");

        var design = new Matrix<double>(new double[,]
        {
            { 1, 1 },
            { 1, 2 },
            { 1, 3 }
        });
        double[] y = { 6, 8, 10 };
        var ls = design.SolveLeastSquares(y);
        if (Math.Abs(ls[0] - 4.0) > 1e-4 || Math.Abs(ls[1] - 2.0) > 1e-4)
            throw new InvalidOperationException("Matrix.SolveLeastSquares failed.");

        var ridge = design.SolveRidgeLeastSquares(y, 0.01);
        if (ridge.Length != 2 || double.IsNaN(ridge[0]))
            throw new InvalidOperationException("Matrix.SolveRidgeLeastSquares failed.");

        Console.WriteLine("Matrix solve wrapper tests passed.");
    }

    static void TestBidiagonalSvdLarge()
    {
        int rows = 60;
        int cols = 8;
        var random = new Random(42);
        var matrix = new Matrix<double>(rows, cols);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                matrix[i, j] = random.NextDouble();
        }

        var svd = MatrixDecomposition.SingularValueDecomposition(matrix);
        var reconstructed = MatrixDecomposition.Reconstruct(svd);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (Math.Abs(reconstructed[i, j] - matrix[i, j]) > 1e-5)
                    throw new InvalidOperationException("Bidiagonal SVD reconstruction failed for large matrix.");
            }
        }

        var legacy = new Matrix(rows, cols);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                legacy[i, j] = (float)matrix[i, j];
        }

        double[] y = new double[rows];
        for (int i = 0; i < rows; i++)
            y[i] = random.NextDouble();

        var genericSolution = matrix.SolveLeastSquares(y);
        var floatY = new float[rows];
        for (int i = 0; i < rows; i++)
            floatY[i] = (float)y[i];
        var legacySolution = legacy.SolveLeastSquares(floatY);
        if (legacySolution.Length != genericSolution.Length)
            throw new InvalidOperationException("Matrix<float> SolveLeastSquares length mismatch.");

        Console.WriteLine("Bidiagonal SVD large-matrix tests passed.");
    }
}


// 类 、 结构体 、 接口 、 枚举 、 委托
//值类型 ：结构体、枚举
//引用类型：类、接口、委托
