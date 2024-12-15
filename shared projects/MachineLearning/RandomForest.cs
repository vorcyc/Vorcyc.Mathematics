namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 随机森林类，用于构建和预测随机森林模型。
/// </summary>
public class RandomForest : IMachineLearning
{
    private List<DecisionTree> _trees;
    private int _numTrees;
    private int _numAttributes;

    public MachineLearningTask Task => MachineLearningTask.Classification | MachineLearningTask.Regression;

    /// <summary>
    /// 初始化随机森林类的新实例。
    /// </summary>
    /// <param name="numTrees">决策树的数量。</param>
    /// <param name="numAttributes">每棵树随机选择的特征数量。</param>
    public RandomForest(int numTrees, int numAttributes)
    {
        this._numTrees = numTrees;
        this._numAttributes = numAttributes;
        this._trees = new List<DecisionTree>();
    }

    /// <summary>
    /// 训练随机森林模型。
    /// </summary>
    /// <param name="data">数据集。</param>
    /// <param name="attributes">特征列表。</param>
    public void Train(List<Dictionary<string, string>> data, List<string> attributes)
    {
        Random random = new Random();
        for (int i = 0; i < _numTrees; i++)
        {
            var bootstrapSample = BootstrapSample(data, random);
            var selectedAttributes = SelectRandomAttributes(attributes, random);
            var tree = DecisionTree.BuildTree(bootstrapSample, selectedAttributes);
            _trees.Add(tree);
        }
    }

    /// <summary>
    /// 预测给定实例的标签。
    /// </summary>
    /// <param name="instance">实例。</param>
    /// <returns>预测的标签。</returns>
    public string Predict(Dictionary<string, string> instance)
    {
        Dictionary<string, int> votes = new Dictionary<string, int>();
        foreach (var tree in _trees)
        {
            var prediction = tree.Predict(instance);
            if (!votes.ContainsKey(prediction))
                votes[prediction] = 0;
            votes[prediction]++;
        }
        return votes.OrderByDescending(v => v.Value).First().Key;
    }

    /// <summary>
    /// 生成自助法样本。
    /// </summary>
    /// <param name="data">数据集。</param>
    /// <param name="random">随机数生成器。</param>
    /// <returns>自助法样本。</returns>
    private List<Dictionary<string, string>> BootstrapSample(List<Dictionary<string, string>> data, Random random)
    {
        var sample = new List<Dictionary<string, string>>();
        for (int i = 0; i < data.Count; i++)
        {
            int index = random.Next(data.Count);
            sample.Add(data[index]);
        }
        return sample;
    }

    /// <summary>
    /// 随机选择特征。
    /// </summary>
    /// <param name="attributes">特征列表。</param>
    /// <param name="random">随机数生成器。</param>
    /// <returns>随机选择的特征列表。</returns>
    private List<string> SelectRandomAttributes(List<string> attributes, Random random)
    {
        var selectedAttributes = new List<string>();
        var attributesCopy = new List<string>(attributes);
        for (int i = 0; i < _numAttributes; i++)
        {
            int index = random.Next(attributesCopy.Count);
            selectedAttributes.Add(attributesCopy[index]);
            attributesCopy.RemoveAt(index);
        }
        return selectedAttributes;
    }
}
