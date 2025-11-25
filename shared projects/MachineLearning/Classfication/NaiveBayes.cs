using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Classfication
{
    //var nb = new NaiveBayes();

    //// 训练数据
    //nb.Train(new string[] { "sunny", "hot", "high", "false" }, "no");
    //nb.Train(new string[] { "sunny", "hot", "high", "true" }, "no");
    //nb.Train(new string[] { "overcast", "hot", "high", "false" }, "yes");
    //nb.Train(new string[] { "rainy", "mild", "high", "false" }, "yes");
    //nb.Train(new string[] { "rainy", "cool", "normal", "false" }, "yes");
    //nb.Train(new string[] { "rainy", "cool", "normal", "true" }, "no");
    //nb.Train(new string[] { "overcast", "cool", "normal", "true" }, "yes");
    //nb.Train(new string[] { "sunny", "mild", "high", "false" }, "no");
    //nb.Train(new string[] { "sunny", "cool", "normal", "false" }, "yes");
    //nb.Train(new string[] { "rainy", "mild", "normal", "false" }, "yes");
    //nb.Train(new string[] { "sunny", "mild", "normal", "true" }, "yes");
    //nb.Train(new string[] { "overcast", "mild", "high", "true" }, "yes");
    //nb.Train(new string[] { "overcast", "hot", "normal", "false" }, "yes");
    //nb.Train(new string[] { "rainy", "mild", "high", "true" }, "no");

    //// 预测
    //string[] newFeatures = new string[] { "sunny", "cool", "high", "true" };
    //string prediction = nb.Predict(newFeatures);

    //Console.WriteLine($"预测结果: {prediction}");


    /// <summary>
    /// 朴素贝叶斯分类器类
    /// </summary>
    public class NaiveBayes
    {
        private readonly Dictionary<string, Dictionary<string, int>> _featureCountPerClass; // 每个类别的特征计数
        private readonly Dictionary<string, int> _classCount; // 每个类别的样本计数
        private int _totalSamples; // 总样本数

        /// <summary>
        /// 初始化 NaiveBayes 类的新实例
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NaiveBayes()
        {
            _featureCountPerClass = [];
            _classCount = [];
            _totalSamples = 0;
        }

        /// <summary>
        /// 训练模型
        /// </summary>
        /// <param name="features">特征数组</param>
        /// <param name="label">类别标签</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Train(string[] features, string label)
        {
            // 如果类别标签不存在，则初始化
            if (!_classCount.ContainsKey(label))
            {
                _classCount[label] = 0;
                _featureCountPerClass[label] = [];
            }

            // 增加该类别的样本计数和总样本数
            _classCount[label]++;
            _totalSamples++;

            // 增加该类别下每个特征的计数
            foreach (var feature in features)
            {
                if (!_featureCountPerClass[label].TryGetValue(feature, out int count))
                {
                    _featureCountPerClass[label][feature] = 0;
                }
                _featureCountPerClass[label][feature]++;
            }
        }

        /// <summary>
        /// 预测类别
        /// </summary>
        /// <param name="features">特征数组</param>
        /// <returns>预测的类别标签</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Predict(string[] features)
        {
            double maxProbability = double.NegativeInfinity; // 最大概率
            string bestClass = null; // 最佳类别

            // 遍历每个类别，计算其概率
            foreach (var classLabel in _classCount.Keys)
            {
                // 计算类别概率
                double classProbability = Math.Log((double)_classCount[classLabel] / _totalSamples);
                double featureProbabilitySum = CalculateFeatureProbabilitySum(classLabel, features);

                // 计算总概率
                double totalProbability = classProbability + featureProbabilitySum;

                // 更新最大概率和最佳类别
                if (totalProbability > maxProbability)
                {
                    maxProbability = totalProbability;
                    bestClass = classLabel;
                }
            }

            return bestClass;

            // 局部函数：计算特征的条件概率和
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            double CalculateFeatureProbabilitySum(string classLabel, string[] features)
            {
                double featureProbabilitySum = 0.0; // 特征概率和
                int classFeatureCount = _featureCountPerClass[classLabel].Count;

                foreach (var feature in features)
                {
                    if (_featureCountPerClass[classLabel].TryGetValue(feature, out int featureCount))
                    {
                        featureProbabilitySum += Math.Log((double)featureCount / _classCount[classLabel]);
                    }
                    else
                    {
                        // 如果特征在该类别中不存在，使用拉普拉斯平滑
                        featureProbabilitySum += Math.Log(1.0 / (_classCount[classLabel] + classFeatureCount));
                    }
                }

                return featureProbabilitySum;
            }
        }
    }
}
