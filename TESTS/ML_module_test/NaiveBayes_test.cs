using Vorcyc.Mathematics.MachineLearning.Classfication;

namespace ML_module_test;

internal class NaiveBayes_test
{

    public static void go()
    {
        var nb = new NaiveBayes();

        // 训练数据
        nb.Train(["sunny", "hot", "high", "false"], "no");
        nb.Train(new string[] { "sunny", "hot", "high", "true" }, "no");
        nb.Train(new string[] { "overcast", "hot", "high", "false" }, "yes");
        nb.Train(new string[] { "rainy", "mild", "high", "false" }, "yes");
        nb.Train(new string[] { "rainy", "cool", "normal", "false" }, "yes");
        nb.Train(new string[] { "rainy", "cool", "normal", "true" }, "no");
        nb.Train(new string[] { "overcast", "cool", "normal", "true" }, "yes");
        nb.Train(new string[] { "sunny", "mild", "high", "false" }, "no");
        nb.Train(new string[] { "sunny", "cool", "normal", "false" }, "yes");
        nb.Train(new string[] { "rainy", "mild", "normal", "false" }, "yes");
        nb.Train(new string[] { "sunny", "mild", "normal", "true" }, "yes");
        nb.Train(new string[] { "overcast", "mild", "high", "true" }, "yes");
        nb.Train(new string[] { "overcast", "hot", "normal", "false" }, "yes");
        nb.Train(new string[] { "rainy", "mild", "high", "true" }, "no");

        // 预测
        string[] newFeatures = new string[] { "sunny", "cool", "high", "true" };
        string prediction = nb.Predict(newFeatures);

        Console.WriteLine($"预测结果: {prediction}");

    }

}
