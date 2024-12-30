using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.MachineLearning.Clustering;

namespace ML_module_test;

internal class VQ_test
{


    public static void go()
    {
        // 定义矢量的维度和码书的大小
        int dimensions = 3;
        int codebookSize = 5;

        // 创建 VectorQuantization 实例
        var vq = new VectorQuantization<double>(codebookSize, dimensions);

        // 创建训练数据
        var data = new List<Vector<double>>
        {
            new Vector<double>(1.0, 2.0, 3.0),
            new Vector<double>(4.0, 5.0, 6.0),
            new Vector<double>(7.0, 8.0, 9.0),
            new Vector<double>(1.1, 2.1, 3.1),
            new Vector<double>(4.1, 5.1, 6.1)
        };

        // 训练矢量量化模型
        vq.Train(data, maxIterations: 100);

        // 输出训练后的码书
        Console.WriteLine("训练后的码书：");
        foreach (var vector in vq.Codebook)
        {
            Console.WriteLine(string.Join(", ", vector.Elements));
        }
    }
}
