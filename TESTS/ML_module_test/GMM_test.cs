using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.MachineLearning.Clustering;

namespace ML_module_test;

internal class GMM_test
{
    public static void Go()
    {
        Console.WriteLine("Testing GMM...");

        // 准备数据（使用 double[] 替代 Vector<double>）
        List<double[]> data = new List<double[]>
        {
            new double[] { 1.0, 2.0 },
            new double[] { 1.5, 1.8 },
            new double[] { 5.0, 8.0 },
            new double[] { 8.0, 8.0 },
            new double[] { 1.0, 0.6 },
            new double[] { 9.0, 11.0 },
            new double[] { 8.0, 2.0 },
            new double[] { 10.0, 2.0 },
            new double[] { 9.0, 3.0 }
        };

        // 创建 GMM 实例
        var gmm = new GMM<double>(numComponents: 2, maxIterations: 100, tolerance: 1e-6);

        try
        {
            // 拟合数据
            gmm.Fit(data);

            // 获取结果
            var means = gmm.Means;
            var covariances = gmm.Covariances;
            var weights = gmm.Weights;

            // 输出结果
            Console.WriteLine("\n聚类中心 (Means):");
            foreach (var mean in means)
            {
                Console.WriteLine($"[{string.Join(", ", mean)}]");
            }

            Console.WriteLine("\n协方差矩阵 (Covariances):");
            foreach (var covariance in covariances)
            {
                Console.WriteLine(covariance.ToString());
            }

            Console.WriteLine("\n权重 (Weights):");
            Console.WriteLine(string.Join(", ", weights));

            // 测试预测
            Console.WriteLine("\n预测聚类标签:");
            var testPoint = new double[] { 2.0, 3.0 };
            int cluster = gmm.Predict(testPoint);
            Console.WriteLine($"输入点 [{string.Join(", ", testPoint)}] -> Cluster {cluster}");

            // 输出对数似然值（验证模型质量）
            double logLikelihood = ComputeLogLikelihood(gmm, data);
            Console.WriteLine($"\nLog Likelihood (model quality): {logLikelihood:F6}");

            // 异常测试
            Console.WriteLine("\n测试异常情况:");
            try
            {
                gmm.Fit(null!);
                Console.WriteLine("Error: Null data should throw exception");
            }
            catch (ArgumentNullException) { Console.WriteLine("Passed: Null data exception caught"); }

            try
            {
                gmm.Fit(new List<double[]>());
                Console.WriteLine("Error: Empty data should throw exception");
            }
            catch (ArgumentException) { Console.WriteLine("Passed: Empty data exception caught"); }

            try
            {
                gmm.Predict(new double[] { 1.0 }); // 维度不匹配
                Console.WriteLine("Error: Dimension mismatch should throw exception");
            }
            catch (ArgumentException) { Console.WriteLine("Passed: Dimension mismatch exception caught"); }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed: {ex.Message}");
        }

        Console.WriteLine("\nGMM test completed.");
    }

    // 简单实现 ComputeLogLikelihood，用于验证模型质量
    private static double ComputeLogLikelihood(GMM<double> gmm, List<double[]> data)
    {
        double logLikelihood = 0;
        foreach (var point in data)
        {
            double sum = 0;
            for (int j = 0; j < gmm.Means.Count; j++)
            {
                sum += (double)gmm.Weights[j] * gmm.MultivariateGaussian(point.AsSpan(), gmm.Means[j].AsSpan(), gmm.Covariances[j]);
            }
            logLikelihood += Math.Log(sum > 0 ? sum : 1e-10); // 避免 log(0)
        }
        return logLikelihood;
    }
}