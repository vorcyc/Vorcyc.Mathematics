using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.MachineLearning.Clustering;

namespace ML_module_test;

internal class ExpectationMaximization_test
{


    public static void go()
    {
        // 准备数据
        List<Vector<double>> data = new List<Vector<double>>
        {
            new Vector<double>(1.0, 2.0),
            new Vector<double>(1.5, 1.8),
            new Vector<double>(5.0, 8.0),
            new Vector<double>(8.0, 8.0),
            new Vector<double>(1.0, 0.6),
            new(9.0, 11.0)
        };

        // 创建期望最大化算法的实例，并指定聚类数量为2
        ExpectationMaximization<double> em = new ExpectationMaximization<double>(2);

        // 拟合数据
        em.Fit(data);

        // 输出结果
        Console.WriteLine("聚类中心:");
        foreach (var mean in em.Means)
        {
            Console.WriteLine(string.Join(", ", mean.Elements));
        }

        Console.WriteLine("协方差矩阵:");
        foreach (var covariance in em.Covariances)
        {
            Console.WriteLine(covariance);
        }

        Console.WriteLine("权重:");
        foreach (var weight in em.Weights)
        {
            Console.WriteLine(weight);
        }
    }


}
