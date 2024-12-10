using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vorcyc.Mathematics.MachineLearning;

namespace ML_module_test;

internal class PCA_test
{


    public static void go()
    {

        // 示例数据集 (每行是一个样本，每列是一个特征)
        double[,] data =
        {
                    { 2.5, 2.4 },
                    { 0.5, 0.7 },
                    { 2.2, 2.9 },
                    { 1.9, 2.2 },
                    { 3.1, 3.0 },
                    { 2.3, 2.7 },
                    { 2.0, 1.6 },
                    { 1.0, 1.1 },
            { 1.5, 1.6 },
                    { 1.1, 0.9 }
                };

        PCA<double> pca = new PCA<double>(data);

        // 获取主成分
        double[,] components = pca.Transform();

        // 输出主成分
        Console.WriteLine("主成分:");
        for (int i = 0; i < components.GetLength(0); i++)
        {
            for (int j = 0; j < components.GetLength(1); j++)
            {
                Console.Write($"{components[i, j]:F2} ");
            }
            Console.WriteLine();
        }

        // 输出解释的方差比例
        double[] explainedVarianceRatio = pca.GetExplainedVarianceRatio();
        Console.WriteLine("\n解释的方差比例:");
        foreach (var proportion in explainedVarianceRatio)
        {
            Console.WriteLine($"{proportion:F2}");
        }

    }


}
