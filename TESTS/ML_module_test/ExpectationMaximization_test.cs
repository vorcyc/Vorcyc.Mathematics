using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.MachineLearning.Clustering;

namespace ML_module_test;

internal class ExpectationMaximization_test
{


    public static void Go()
    {
        Console.WriteLine("Testing ExpectationMaximization...");

        // 准备数据（使用 T[] 替代 Vector<T>）
        List<double[]> data = new List<double[]>
        {
            new double[] { 1.0, 2.0 },
            new double[] { 1.5, 1.8 },
            new double[] { 5.0, 8.0 },
            new double[] { 8.0, 8.0 },
            new double[] { 1.0, 0.6 },
            new double[] { 9.0, 11.0 }
        };

        // 创建期望最大化算法实例，指定聚类数量为 2
        var em = new ExpectationMaximization<double>(2);

        try
        {
            // 拟合数据
            em.Fit(data);

            // 输出结果
            Console.WriteLine("聚类中心 (Means):");
            foreach (var mean in em.Means)
            {
                Console.WriteLine($"[{string.Join(", ", mean)}]");
            }

            Console.WriteLine("\n协方差矩阵 (Covariances):");
            foreach (var covariance in em.Covariances)
            {
                Console.WriteLine(covariance.ToString());
            }

            Console.WriteLine("\n权重 (Weights):");
            Console.WriteLine(string.Join(", ", em.Weights));

            // 验证聚类
            Console.WriteLine("\n预测聚类标签:");
            foreach (var point in data)
            {
                int cluster = em.Predict(point);
                Console.WriteLine($"Point [{string.Join(", ", point)}] -> Cluster {cluster}");
            }

            // 验证 R² 或模型质量（可选，因 EM 未直接提供 R²，此处仅示意）
            Console.WriteLine($"\nLog Likelihood (approximated quality): {ComputeLogLikelihood(em, data):F6}");

            // 异常测试
            Console.WriteLine("\nTesting invalid inputs...");
            try
            {
                em.Fit(null!);
                Console.WriteLine("Error: Null data should throw exception");
            }
            catch (ArgumentNullException) { Console.WriteLine("Passed: Null data exception caught"); }

            try
            {
                em.Fit(new List<double[]> { new double[1] });
                Console.WriteLine("Error: Too few points should throw exception");
            }
            catch (ArgumentException) { Console.WriteLine("Passed: Too few points exception caught"); }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed: {ex.Message}");
        }

        Console.WriteLine("ExpectationMaximization test completed.");
    }

    // 简单实现 ComputeLogLikelihood，用于验证模型质量
    private static double ComputeLogLikelihood(ExpectationMaximization<double> em, List<double[]> data)
    {
        double logLikelihood = 0;
        foreach (var point in data)
        {
            double sum = 0;
            for (int j = 0; j < em.Means.Count; j++)
            {
                sum += (double)em.Weights[j] * em.MultivariateGaussian(point.AsSpan(), em.Means[j].AsSpan(), em.Covariances[j]);
            }
            logLikelihood += Math.Log(sum > 0 ? sum : 1e-10); // 避免 log(0)
        }
        return logLikelihood;
    }


}
