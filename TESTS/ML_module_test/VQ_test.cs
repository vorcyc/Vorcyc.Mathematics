using System;
using Vorcyc.Mathematics.MachineLearning.Clustering;

namespace ML_module_test;

internal class VQ_test
{
    public static void Go()
    {
        Console.WriteLine("Testing VectorQuantization...");

        // 定义矢量的维度和码书大小
        int dimensions = 3;
        int codebookSize = 5;

        // 创建 VectorQuantization 实例
        var vq = new VectorQuantization<double>(codebookSize, dimensions);

        try
        {
            // 创建训练数据（使用 double[] 替代 Vector<double>）
            var data = new List<double[]>
            {
                new double[] { 1.0, 2.0, 3.0 },
                new double[] { 4.0, 5.0, 6.0 },
                new double[] { 7.0, 8.0, 9.0 },
                new double[] { 1.1, 2.1, 3.1 },
                new double[] { 4.1, 5.1, 6.1 }
            };

            // 训练矢量量化模型
            var errors = vq.Train(data, maxIterations: 100);

            // 输出训练后的码书
            Console.WriteLine("\n训练后的码书 (Codebook):");
            foreach (var vector in vq.Codebook)
            {
                Console.WriteLine($"[{string.Join(", ", vector)}]");
            }

            // 输出训练误差
            Console.WriteLine("\n训练误差 (Errors):");
            Console.WriteLine(string.Join(", ", errors));

            // 测试预测
            Console.WriteLine("\n预测最近码矢量:");
            var testPoint = new double[] { 2.0, 3.0, 4.0 };
            var nearestVector = vq.Predict(testPoint);
            Console.WriteLine($"输入点 [{string.Join(", ", testPoint)}] -> 最近码矢量 [{string.Join(", ", nearestVector)}]");

            // 保存和加载码书
            Console.WriteLine("\n测试保存和加载码书:");
            vq.SaveCodebook("codebook.json");
            var vqLoaded = new VectorQuantization<double>(codebookSize, dimensions);
            vqLoaded.LoadCodebook("codebook.json");
            Console.WriteLine("加载后的码书:");
            foreach (var vector in vqLoaded.Codebook)
            {
                Console.WriteLine($"[{string.Join(", ", vector)}]");
            }

            // 异常测试
            Console.WriteLine("\n测试异常情况:");
            try
            {
                vq.Train(null!);
                Console.WriteLine("Error: Null data should throw exception");
            }
            catch (ArgumentNullException) { Console.WriteLine("Passed: Null data exception caught"); }

            try
            {
                vq.Train(new List<double[]>());
                Console.WriteLine("Error: Empty data should throw exception");
            }
            catch (ArgumentException) { Console.WriteLine("Passed: Empty data exception caught"); }

            try
            {
                vq.Predict(new double[] { 1.0 }); // 维度不匹配
                Console.WriteLine("Error: Dimension mismatch should throw exception");
            }
            catch (ArgumentException) { Console.WriteLine("Passed: Dimension mismatch exception caught"); }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed: {ex.Message}");
        }

        Console.WriteLine("\nVectorQuantization test completed.");
    }
}