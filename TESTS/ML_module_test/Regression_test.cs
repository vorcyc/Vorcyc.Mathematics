using System;
using Vorcyc.Mathematics.Framework.Utilities;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.MachineLearning.Regression;
using Vorcyc.Mathematics.Numerics;

namespace ML_module_test;

public class Regression_test
{
    public static void Go()
    {
        Console.WriteLine("Starting tests...");

        TestSimpleLinearRegression();

        new string('-', 30).PrintLine(ConsoleColor.Green);

        TestMultipleLinearRegression();

        new string('-', 30).PrintLine(ConsoleColor.Green);
        TestRidgeRegression();

        new string('-', 30).PrintLine(ConsoleColor.Green);

        TestPolynomialRegression();

        new string('-', 30).PrintLine(ConsoleColor.Green);

        TestLinearEquationSolver();

        new string('-', 30).PrintLine(ConsoleColor.Green);

        TestBasisTransformation();

        Console.WriteLine("All tests completed. Check output for results.");
    }

    static void TestSimpleLinearRegression()
    {
        Console.WriteLine("Testing SimpleLinearRegression...");
        try
        {
            var data = new Point<double>[]
            {
                new Point<double>(1.0, 2.0),
                new Point<double>(2.0, 3.0),
                new Point<double>(3.0, 5.0),
                new Point<double>(4.0, 4.0),
                new Point<double>(5.0, 6.0)
            };
            var regression = new SimpleLinearRegression<double>();
            var (slope, intercept) = regression.Fit(data);
            double yPred = regression.GetY(6.0);

            Console.WriteLine($"Slope: {slope} (Expected ~0.9)");
            Console.WriteLine($"Intercept: {intercept} (Expected ~1.4)");
            Console.WriteLine($"Predicted y for x=6: {yPred} (Expected ~6.8)");
            Console.WriteLine($"R²: {regression.RSquared}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed: {ex.Message}");
        }
    }

    static void TestMultipleLinearRegression()
    {
        Console.WriteLine("Testing MultipleLinearRegression...");
        try
        {
            var x = new double[,]
            {
            { 1, 2 },
            { 2, 3 },
            { 3, 1 },
            { 4, 5 },
            { 5, 4 }
            };
            var y = new double[] { 2, 4, 3, 8, 8 };
            var regression = new MultipleLinearRegression<double>();
            regression.Fit(x, y);
            var prediction = regression.Predict(new double[] { 6, 7 });

            Console.WriteLine($"Intercept: {regression.Intercept} (Expected ~-0.5)");
            Console.WriteLine($"Coefficients: {string.Join(", ", regression.Coefficients)} (Expected ~1.0, 1.0)");
            Console.WriteLine($"Predicted value for [6, 7]: {prediction} (Expected ~12.5)");
            Console.WriteLine($"R²: {regression.RSquared}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed: {ex.Message}");
        }
    }

    static void TestRidgeRegression()
    {
        Console.WriteLine("Testing RidgeRegression...");
        try
        {
            var x = new double[] { 1, 2, 3, 4, 5 }.AsSpan();
            var y = new double[] { 2, 3, 5, 7, 11 }.AsSpan();
            var regression = new RidgeRegression<double>(0.1, degree: 1);
            regression.Fit(x, y);
            var prediction = regression.Predict(6.0);

            Console.WriteLine($"Intercept: {regression.Coefficients[0]} (Expected ~-0.8427)");
            Console.WriteLine($"Slope: {regression.Coefficients[1]} (Expected ~2.1532)");
            Console.WriteLine($"Predicted value for x=6: {prediction} (Expected ~12.076)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed: {ex.Message}");
        }
    }

    static void TestPolynomialRegression()
    {
        Console.WriteLine("Testing PolynomialRegression...");
        try
        {
            var x = new double[] { 1, 2, 3, 4, 5 }.AsSpan();
            var y = new double[] { 1, 4, 9, 16, 25 }.AsSpan();
            var regression = new PolynomialRegression<double>(2);
            regression.Fit(x, y);
            var prediction = regression.Predict(6.0);

            Console.WriteLine($"Coefficients: {string.Join(", ", regression.Coefficients)} (Expected ~0.0, 0.0, 1.0)");
            Console.WriteLine($"Predicted value for x=6: {prediction} (Expected ~36.0)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed: {ex.Message}");
        }
    }

    static void TestLinearEquationSolver()
    {
        Console.WriteLine("Testing LinearEquationSolver...");
        try
        {
            var A = new Matrix<double>(new double[,] { { 2, 1 }, { 1, 3 } });
            var b = new double[] { 5, 9 };

            Console.WriteLine($"Matrix A dimensions: {A.Rows}x{A.Columns}");
            Console.WriteLine($"Vector b length: {b.Length}");

            var x = LinearEquationSolver.LUSolve(A, b);

            Console.WriteLine($"LU Solve: {string.Join(", ", x)} (Expected ~1.0, 3.0)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed: {ex.Message}");
        }
    }

    static void TestBasisTransformation()
    {
        Console.WriteLine("Testing BasisTransformation...");
        try
        {
            var vector = new double[] { 1, 2 };
            var fromBasis = new Matrix<double>(new double[,] { { 1, 1 }, { 0, 1 } });
            var toBasis = new Matrix<double>(new double[,] { { 2, 0 }, { 1, 2 } });
            var result = BasisTransformation.Transform(vector, fromBasis, toBasis);

            Console.WriteLine($"Transform: {string.Join(", ", result)} (Expected ~0.0, 4.0)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed: {ex.Message}");
        }
    }
}