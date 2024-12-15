namespace Vorcyc.Mathematics.MachineLearning;

/// <summary>
/// 决策树类，用于构建和预测决策树模型。
/// </summary>
public class DecisionTree : IMachineLearning
{
    /// <summary>
    /// 特征名称。
    /// </summary>
    public string Attribute { get; set; }

    /// <summary>
    /// 子节点，键为特征值，值为对应的子决策树。
    /// </summary>
    public Dictionary<string, DecisionTree> Children { get; set; } = [];

    /// <summary>
    /// 标签值，如果当前节点是叶子节点。
    /// </summary>
    public string Label { get; set; }


    public MachineLearningTask Task => MachineLearningTask.Classification | MachineLearningTask.Regression;

    /// <summary>
    /// 计算给定数据集和特征的信息熵。
    /// </summary>
    /// <param name="data">数据集。</param>
    /// <param name="attribute">特征名称。</param>
    /// <returns>信息熵值。</returns>
    private static double CalculateEntropy(List<Dictionary<string, string>> data, string attribute)
    {
        Dictionary<string, int> labelCounts = new Dictionary<string, int>();
        foreach (var row in data)
        {
            if (!labelCounts.ContainsKey(row[attribute]))
                labelCounts[row[attribute]] = 0;
            labelCounts[row[attribute]]++;
        }

        double entropy = 0.0;
        foreach (var count in labelCounts.Values)
        {
            double probability = (double)count / data.Count;
            entropy -= probability * Math.Log2(probability);
        }
        return entropy;
    }

    /// <summary>
    /// 选择最佳分裂特征。
    /// </summary>
    /// <param name="data">数据集。</param>
    /// <param name="attributes">特征列表。</param>
    /// <returns>最佳分裂特征名称。</returns>
    private static string ChooseBestAttribute(List<Dictionary<string, string>> data, List<string> attributes)
    {
        double baseEntropy = CalculateEntropy(data, "Label");
        double bestInfoGain = 0.0;
        string bestAttribute = null;

        foreach (var attribute in attributes)
        {
            double attributeEntropy = 0.0;
            Dictionary<string, List<Dictionary<string, string>>> attributeValues = new Dictionary<string, List<Dictionary<string, string>>>();

            foreach (var row in data)
            {
                if (!attributeValues.ContainsKey(row[attribute]))
                    attributeValues[row[attribute]] = new List<Dictionary<string, string>>();
                attributeValues[row[attribute]].Add(row);
            }

            foreach (var subset in attributeValues.Values)
            {
                double subsetProbability = (double)subset.Count / data.Count;
                attributeEntropy += subsetProbability * CalculateEntropy(subset, "Label");
            }

            double infoGain = baseEntropy - attributeEntropy;
            if (infoGain > bestInfoGain)
            {
                bestInfoGain = infoGain;
                bestAttribute = attribute;
            }
        }
        return bestAttribute;
    }

    /// <summary>
    /// 构建决策树。
    /// </summary>
    /// <param name="data">数据集。</param>
    /// <param name="attributes">特征列表。</param>
    /// <returns>构建的决策树。</returns>
    public static DecisionTree BuildTree(List<Dictionary<string, string>> data, List<string> attributes)
    {
        DecisionTree tree = new DecisionTree();
        Dictionary<string, int> labelCounts = new Dictionary<string, int>();

        foreach (var row in data)
        {
            if (!labelCounts.ContainsKey(row["Label"]))
                labelCounts[row["Label"]] = 0;
            labelCounts[row["Label"]]++;
        }

        if (labelCounts.Count == 1)
        {
            tree.Label = labelCounts.Keys.First();
            return tree;
        }

        if (attributes.Count == 0)
        {
            tree.Label = labelCounts.OrderByDescending(kv => kv.Value).First().Key;
            return tree;
        }

        string bestAttribute = ChooseBestAttribute(data, attributes);
        tree.Attribute = bestAttribute;

        Dictionary<string, List<Dictionary<string, string>>> subsets = new Dictionary<string, List<Dictionary<string, string>>>();
        foreach (var row in data)
        {
            if (!subsets.ContainsKey(row[bestAttribute]))
                subsets[row[bestAttribute]] = new List<Dictionary<string, string>>();
            subsets[row[bestAttribute]].Add(row);
        }

        List<string> remainingAttributes = new List<string>(attributes);
        remainingAttributes.Remove(bestAttribute);

        foreach (var subset in subsets)
        {
            tree.Children[subset.Key] = BuildTree(subset.Value, remainingAttributes);
        }

        return tree;
    }

    /// <summary>
    /// 预测给定实例的标签。
    /// </summary>
    /// <param name="instance">实例。</param>
    /// <returns>预测的标签。</returns>
    public string Predict(Dictionary<string, string> instance)
    {
        if (Label != null)
            return Label;

        string attributeValue = instance[Attribute];
        if (Children.ContainsKey(attributeValue))
            return Children[attributeValue].Predict(instance);
        else
            return null;
    }
}
