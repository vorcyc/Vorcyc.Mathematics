using System.Text.RegularExpressions;
using Vorcyc.Mathematics.LanguageExtension;
using Vorcyc.Mathematics.MachineLearning;

namespace ML_module_test;

internal class SVM_test
{
    public static void simple_go()
    {
        double[][] inputs =
        {
            new double[] { 0, 0 },
            new double[] { 1, 0 },
            new double[] { 0, 1 },
            new double[] { 1, 1 }
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

    public static void text_go()
    {
        // 示例文本数据
        string[] texts = {
            "I love programming",
            "This is a great day",
            "I hate bugs",
            "Debugging is fun",
        };

        // 标签：1 表示正面情感，-1 表示负面情感
        int[] labels = { 1, 1, -1, 1 };

        // 文本预处理和特征提取
        var (inputs, featureCount) = PreprocessTexts(texts, topN: 10);

        Console.WriteLine(  featureCount);

        // 创建并训练支持向量机
        var svm = new SupportVectorMachine<double>(featureCount, epochs: 1000);
        svm.Train(inputs, labels);

        // 预测新文本
        string newText = "I hate programming";
        var newInput = ExtractFeatures(newText, featureCount);
        int prediction = svm.Predict(newInput);

        //Console.WriteLine($"预测结果: {(prediction == 1 ? "正面" : "负面")}");
        Console.WriteLine(  prediction);
    }

    private static (double[][] inputs, int featureCount) PreprocessTexts(string[] texts, int topN)
    {
        var wordSet = new HashSet<string>();
        var tokenizedTexts = new List<string[]>();

        foreach (var text in texts)
        {
            var tokens = Tokenize(text);
            tokenizedTexts.Add(tokens);
            foreach (var token in tokens)
            {
                wordSet.Add(token);
            }
        }

        var wordList = wordSet.ToList();
        var idf = CalculateIDF(tokenizedTexts, wordList);

        // 选择最重要的 topN 个特征
        var topNWords = wordList
            .Select((word, index) => new { Word = word, IDF = idf[index] })
            .OrderByDescending(x => x.IDF)
            .Take(topN)
            .Select(x => x.Word)
            .ToList();

        int featureCount = topNWords.Count;
        var inputs = new double[texts.Length][];

        for (int i = 0; i < texts.Length; i++)
        {
            inputs[i] = new double[featureCount];
            var tokens = tokenizedTexts[i];
            var tf = CalculateTF(tokens, topNWords);
            for (int j = 0; j < featureCount; j++)
            {
                inputs[i][j] = tf[j] * idf[wordList.IndexOf(topNWords[j])];
            }
        }

        return (inputs, featureCount);
    }

    private static double[] CalculateTF(string[] tokens, List<string> wordList)
    {
        var tf = new double[wordList.Count];
        foreach (var token in tokens)
        {
            int index = wordList.IndexOf(token);
            if (index >= 0)
            {
                tf[index]++;
            }
        }
        for (int i = 0; i < tf.Length; i++)
        {
            tf[i] /= tokens.Length;
        }
        return tf;
    }

    private static double[] CalculateIDF(List<string[]> tokenizedTexts, List<string> wordList)
    {
        var idf = new double[wordList.Count];
        for (int i = 0; i < wordList.Count; i++)
        {
            int docCount = tokenizedTexts.Count(doc => doc.Contains(wordList[i]));
            idf[i] = Math.Log((double)tokenizedTexts.Count / (1 + docCount));
        }
        return idf;
    }

    private static string[] Tokenize(string text)
    {
        return Regex.Split(text.ToLower(), @"\W+").Where(t => t.Length > 0).ToArray();
    }

    private static double[] ExtractFeatures(string text, int featureCount)
    {
        var tokens = Tokenize(text);
        var features = new double[featureCount];
        foreach (var token in tokens)
        {
            int index = Array.IndexOf(tokens, token);
            if (index >= 0 && index < featureCount)
            {
                features[index] = 1;
            }
        }
        return features;
    }
}
