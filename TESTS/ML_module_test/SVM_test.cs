using Vorcyc.Mathematics.MachineLearning;

namespace ML_module_test;

internal class SVM_test
{

    public static void go()
    {
        double[][] inputs =
        {
            [0, 0],
            [1, 0],
            [0, 1],
            [1, 1]
        };

        int[] outputs =
        {
            -1, // Class 1
            -1, // Class 1
            -1, // Class 1
             1  // Class 2
        };

        // 创建并训练支持向量机
        var svm = new SupportVectorMachine<double>(featureCount: 2);
        svm.Train(inputs, outputs);

        // 预测
        double[] newInput = { 0.8, 0.8 };
        int prediction = svm.Predict(newInput);

        Console.WriteLine($"预测结果: {prediction}");

    }

}
