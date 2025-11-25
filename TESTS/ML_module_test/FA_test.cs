using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.MachineLearning;
using Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;

namespace ML_module_test;

internal class FA_test
{

    public static void go()
    {

        // 创建示例数据
        var data = new double[,]
        {
                { 4.0, 2.0, 0.6 },
                { 4.2, 2.1, 0.59 },
                { 3.9, 2.0, 0.58 },
                { 4.3, 2.1, 0.62 },
                { 4.1, 2.2, 0.63 }
        };

        var matrix =new Matrix<double>(data);   

        // 执行因子分析
        var factorAnalysis = new FactorAnalysis<double>();
        factorAnalysis.Analyze(matrix, 2);

        // 输出结果
        Console.WriteLine("因子载荷矩阵:");
        PrintMatrix(factorAnalysis.Loadings);

        Console.WriteLine("共同性:");
        PrintArray(factorAnalysis.Communalities);

        Console.WriteLine("特异性方差:");
        PrintArray(factorAnalysis.SpecificVariances);

    }

    private static void PrintMatrix(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write($"{matrix[i, j]:F4} ");
            }
            Console.WriteLine();
        }
    }

    private static void PrintArray(double[] array)
    {
        foreach (var value in array)
        {
            Console.WriteLine($"{value:F4}");
        }
    }
}
