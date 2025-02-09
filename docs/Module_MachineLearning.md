# 机器学习模块 - Machine Learning Module

> 以下所有类型都位于命名空间 : Vorcyc.Mathematics.MachineLearning

:ledger:目录  
- :bookmark: [Vorcyc.Mathematics.MachineLearning.DecisionTree 类](#vorcycmathematicsmachinelearningdecisiontree-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.KNN&lt;T> 类](#vorcycmathematicsmachinelearningknnt-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.MultipleLinearRegression&lt;T> 类](#vorcycmathematicsmachinelearningmultiplelinearregressiont-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.RandomForest 类](#vorcycmathematicsmachinelearningrandomforest-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.SimpleLinearRegression&lt;T> 类](#vorcycmathematicsmachinelearningsimplelinearregressiont-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.SupportVectorMachine&lt;TSelf> 类](#vorcycmathematicsmachinelearningsupportvectormachinetself-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.Classfication.NaiveBayes 类](#vorcycmathematicsmachinelearningclassficationnaivebayes-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.Clustering.DBSCAN&lt;T> 类](#vorcycmathematicsmachinelearningclusteringdbscant-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.Clustering.ExpectationMaximization&lt;T> 类](#vorcycmathematicsmachinelearningclusteringexpectationmaximizationt-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.Clustering.GMM&lt;T> 类](#vorcycmathematicsmachinelearningclusteringgmmt-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.Clustering.HierarchicalClustering&lt;T> 类](#vorcycmathematicsmachinelearningclusteringhierarchicalclusteringt-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.Clustering.KMeansClusterer&lt;T> 类](#vorcycmathematicsmachinelearningclusteringkmeansclusterert-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.Clustering.VectorQuantization&lt;T> 类](#vorcycmathematicsmachinelearningclusteringvectorquantizationt-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.DimensionalityReduction.FactorAnalysis&lt;T> 类](#vorcycmathematicsmachinelearningdimensionalityreductionfactoranalysist-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.DimensionalityReduction.PCA&lt;TSelf> 类](#vorcycmathematicsmachinelearningdimensionalityreductionpcatself-类)
- :bookmark: [Vorcyc.Mathematics.MachineLearning.DimensionalityReduction.TSNE&lt;T> 类](#vorcycmathematicsmachinelearningdimensionalityreductiontsnet-类)
- :bookmark: [距离度量类](#距离度量类)


---

## Vorcyc.Mathematics.MachineLearning.DecisionTree 类

Vorcyc.Mathematics.MachineLearning.DecisionTree 是一个用于构建和预测决策树模型的类。

### 属性

#### 1. Attribute
- `public string Attribute { get; set; }`
  - 获取或设置特征名称。

#### 2. Children
- `public Dictionary<string, DecisionTree> Children { get; set; }`
  - 获取或设置子节点，键为特征值，值为对应的子决策树。

#### 3. Label
- `public string Label { get; set; }`
  - 获取或设置标签值，如果当前节点是叶子节点。

#### 4. Task
- `public MachineLearningTask Task { get; }`
  - 获取决策树的任务类型，支持分类和回归。

### 方法

#### 1. CalculateEntropy
- `private static double CalculateEntropy(List<Dictionary<string, string>> data, string attribute)`
  - 计算给定数据集和特征的信息熵。
  - 参数:
    - `data`: 数据集。
    - `attribute`: 特征名称。
  - 返回值: 信息熵值。

#### 2. ChooseBestAttribute
- `private static string ChooseBestAttribute(List<Dictionary<string, string>> data, List<string> attributes)`
  - 选择最佳分裂特征。
  - 参数:
    - `data`: 数据集。
    - `attributes`: 特征列表。
  - 返回值: 最佳分裂特征名称。

#### 3. BuildTree
- `public static DecisionTree BuildTree(List<Dictionary<string, string>> data, List<string> attributes)`
  - 构建决策树。
  - 参数:
    - `data`: 数据集。
    - `attributes`: 特征列表。
  - 返回值: 构建的决策树。

#### 4. Predict
- `public string Predict(Dictionary<string, string> instance)`
  - 预测给定实例的标签。
  - 参数:
    - `instance`: 实例。
  - 返回值: 预测的标签。

### 代码示例
以下是一个使用 DecisionTree 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.MachineLearning;

public class DecisionTreeExample
{
    public static void Main()
    {
        // 定义数据集 
        List<Dictionary<string, string>> data = new List<Dictionary<string, string>> 
        { 
            new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "No" } },
            new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "No" } },
            new Dictionary<string, string> { { "Outlook", "Overcast" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Rain" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } }, 
            new Dictionary<string, string> { { "Outlook", "Rain" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "No" } },
            new Dictionary<string, string> { { "Outlook", "Overcast" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "No" } }, 
            new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } }, 
            new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Mild" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new() { { "Outlook", "Overcast" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new() { { "Outlook", "Overcast" }, { "Temperature", "Hot" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } }, 
            new() { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "No" } } 
        };

        // 定义特征列表
        List<string> attributes = new List<string> { "Outlook", "Temperature", "Humidity", "Wind" };

        // 构建决策树
        DecisionTree tree = DecisionTree.BuildTree(data, attributes);

        // 预测新实例的标签
        Dictionary<string, string> instance = new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Cool" }, { "Humidity", "High" }, { "Wind", "Strong" } };
        string prediction = tree.Predict(instance);
        Console.WriteLine("Prediction for the instance: " + prediction);
    }
}
```


## Vorcyc.Mathematics.MachineLearning.KNN&lt;T> 类

Vorcyc.Mathematics.MachineLearning.KNN&lt;T> 是一个用于分类和回归的 K 最近邻算法类。

### 属性

#### 1. Task
- `public MachineLearningTask Task { get; }`
  - 获取 KNN 的任务类型，支持分类和回归。

### 构造器

#### 1. KNN
- `public KNN()`
  - 初始化 KNN 类的新实例。

### 方法

#### 1. Add
- `public void Add(Point<T> point, string label)`
  - 添加训练数据点。
  - 参数:
    - `point`: 数据点。
    - `label`: 数据点的标签。

#### 2. Classify
- `public string Classify(Point<T> point, int k)`
  - 使用 K 近邻算法对新数据点进行分类。
  - 参数:
    - `point`: 要分类的数据点。
    - `k`: 最近邻居的数量。
  - 返回值: 预测的标签。

#### 3. Regress
- `public T Regress(Point<T> point, int k)`
  - 使用 K 近邻算法对新数据点进行回归。
  - 参数:
    - `point`: 要回归的数据点。
    - `k`: 最近邻居的数量。
  - 返回值: 预测的值。

### 代码示例
以下是一个使用 KNN&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.MachineLearning;

public class KNNExample
{
    public static void Main()
    {
        // 初始化 KNN 实例
        KNN<double> knn = new KNN<double>();
        // 添加训练数据点
        knn.Add(new Point<double>(new double[] { 1.0, 2.0 }), "A");
        knn.Add(new Point<double>(new double[] { 2.0, 3.0 }), "A");
        knn.Add(new Point<double>([3.0, 3.0]), "B");
        knn.Add(new Point<double>([6.0, 5.0]), "B");

        // 使用 KNN 进行分类
        Point<double> newPoint = new Point<double>(new double[] { 2.5, 2.5 });
        string classification = knn.Classify(newPoint, 3);
        Console.WriteLine("Classification of new point: " + classification);

        // 使用 KNN 进行回归
        Point<double> regressionPoint = new Point<double>(new double[] { 2.5, 2.5 });
        double regression = knn.Regress(regressionPoint, 3);
        Console.WriteLine("Regression of new point: " + regression);
    }
}
```


## Vorcyc.Mathematics.MachineLearning.MultipleLinearRegression&lt;T> 类

Vorcyc.Mathematics.MachineLearning.MultipleLinearRegression&lt;T> 是一个用于对数据集进行多元线性回归的类。

### 属性

#### 1. Coefficients
- `public T[]? Coefficients { get; }`
  - 获取回归模型的回归系数。

#### 2. Intercept
- `public T? Intercept { get; }`
  - 获取回归模型的截距。

#### 3. Task
- `public MachineLearningTask Task { get; }`
  - 获取多元线性回归的任务类型，支持回归。

### 方法

#### 1. Learn
- `public void Learn(T[,] x, T[] y)`
  - 学习多元线性回归模型的回归系数和截距。
  - 参数:
    - `x`: 自变量矩阵。
    - `y`: 因变量向量。

#### 2. Predict
- `public T? Predict(T[] x)`
  - 根据给定的自变量和回归系数预测因变量的值。
  - 参数:
    - `x`: 自变量。
  - 返回值: 预测的因变量值。

### 代码示例
以下是一个使用 MultipleLinearRegression&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.MachineLearning;

public class MultipleLinearRegressionExample
{
    public static void Main()
    {
        // 定义自变量矩阵
        double[,] x = new double[,] { { 1, 2 }, { 2, 3 }, { 3, 4 }, { 4, 5 }, { 5, 6 } };
        // 定义因变量向量
        double[] y = new double[] { 2, 3, 5, 7, 11 };

        // 创建多元线性回归实例
        var regression = new MultipleLinearRegression<double>();
        regression.Learn(x, y);

        // 输出回归模型的截距和回归系数
        Console.WriteLine("Intercept: " + regression.Intercept);
        Console.WriteLine("Coefficients:");
        for (int i = 0; i < regression.Coefficients.Length; i++)
        {
            Console.WriteLine("Coefficient " + (i + 1) + ": " + regression.Coefficients[i]);
        }

        // 预测新输入的因变量值
        double[] newInput = new double[] { 6, 7 };
        double prediction = regression.Predict(newInput).Value;
        Console.WriteLine("Predicted value for input " + string.Join(", ", newInput) + ": " + prediction);
    }
}
```


## Vorcyc.Mathematics.MachineLearning.RandomForest 类

Vorcyc.Mathematics.MachineLearning.RandomForest 是一个用于构建和预测随机森林模型的类。

### 属性

#### 1. Task
- `public MachineLearningTask Task { get; }`
  - 获取随机森林的任务类型，支持分类和回归。

### 构造器

#### 1. RandomForest
- `public RandomForest(int numTrees, int numAttributes)`
  - 初始化随机森林类的新实例。
  - 参数:
    - `numTrees`: 决策树的数量。
    - `numAttributes`: 每棵树随机选择的特征数量。

### 方法

#### 1. Train
- `public void Train(List<Dictionary<string, string>> data, List<string> attributes)`
  - 训练随机森林模型。
  - 参数:
    - `data`: 数据集。
    - `attributes`: 特征列表。

#### 2. Predict
- `public string Predict(Dictionary<string, string> instance)`
  - 预测给定实例的标签。
  - 参数:
    - `instance`: 实例。
  - 返回值: 预测的标签。

#### 3. BootstrapSample
- `private List<Dictionary<string, string>> BootstrapSample(List<Dictionary<string, string>> data, Random random)`
  - 生成自助法样本。
  - 参数:
    - `data`: 数据集。
    - `random`: 随机数生成器。
  - 返回值: 自助法样本。

#### 4. SelectRandomAttributes
- `private List<string> SelectRandomAttributes(List<string> attributes, Random random)`
  - 随机选择特征。
  - 参数:
    - `attributes`: 特征列表。
    - `random`: 随机数生成器。
  - 返回值: 随机选择的特征列表。

### 代码示例
以下是一个使用 RandomForest 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.MachineLearning;
public class RandomForestExample
{
    public static void Main()
    {
        // 定义数据集
        List<Dictionary<string, string>> data =
        [
            new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "No" } },
            new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "No" } },
            new Dictionary<string, string> { { "Outlook", "Overcast" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Rain" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Rain" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "No" } },
            new Dictionary<string, string> { { "Outlook", "Overcast" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "No" } },
            new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Mild" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Overcast" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Overcast" }, { "Temperature", "Hot" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new Dictionary<string, string> { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "No" } }
        ];
        // 定义特征列表
        List<string> attributes = new List<string> { "Outlook", "Temperature", "Humidity", "Wind" };

        // 创建随机森林实例
        RandomForest randomForest = new RandomForest(10, 2);

        // 训练随机森林模型
        randomForest.Train(data, attributes);

        // 预测新实例的标签
        Dictionary<string, string> instance = new Dictionary<string, string> { { "Outlook", "Sunny" }, { "Temperature", "Cool" }, { "Humidity", "High" }, { "Wind", "Strong" } };
        string prediction = randomForest.Predict(instance);
        Console.WriteLine("Prediction for the instance: " + prediction);
    }
}
```

## Vorcyc.Mathematics.MachineLearning.SimpleLinearRegression&lt;T> 类

Vorcyc.Mathematics.MachineLearning.SimpleLinearRegression&lt;T> 是一个用于一元线性回归的类。

### 属性

#### 1. Slope
- `public T? Slope { get; }`
  - 获取线性回归模型的斜率。

#### 2. Intercept
- `public T? Intercept { get; }`
  - 获取线性回归模型的截距。

#### 3. Task
- `public MachineLearningTask Task { get; }`
  - 获取一元线性回归的任务类型，支持回归。

### 方法

#### 1. Learn
- `public (T slope, T intercept) Learn(Point<T>[] data)`
  - 计算线性回归模型的斜率和截距。
  - 参数:
    - `data`: 包含数据点的数组。
  - 返回值: 返回一个元组，包含斜率和截距。

- `public (T slope, T intercept) Learn(Span<T> x, Span<T> y)`
  - 计算线性回归模型的斜率和截距。
  - 参数:
    - `x`: 自变量的数组。
    - `y`: 因变量的数组。
  - 返回值: 返回一个元组，包含斜率和截距。

#### 2. GetX
- `public T GetX(T y)`
  - 根据给定的 y 值、斜率和截距计算 x 值。
  - 参数:
    - `y`: 因变量的值。
  - 返回值: 返回计算得到的 x 值。

#### 3. GetY
- `public T GetY(T x)`
  - 根据给定的 x 值、斜率和截距计算 y 值。
  - 参数:
    - `x`: 自变量的值。
  - 返回值: 返回计算得到的 y 值。

### 代码示例
以下是一个使用 SimpleLinearRegression&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.MachineLearning;
using static System.Runtime.InteropServices.JavaScript.JSType;
public class SimpleLinearRegressionExample
{
    public static void Main()
    {
        // 定义数据点
        var data = new Point<double>[] { new Point<double>(1.0, 2.0), new Point<double>(2.0, 3.0), new Point<double>(3.0, 5.0), new Point<double>(4.0, 4.0), new Point<double>(5.0, 6.0) };
        // 创建一元线性回归实例
        var regression = new SimpleLinearRegression<double>();
        var (slope, intercept) = regression.Learn(data);

        // 输出回归模型的斜率和截距
        Console.WriteLine($"Slope: {slope}, Intercept: {intercept}");

        // 预测新输入的 y 值
        var x = 6.0;
        var y = regression.GetY(x);
        Console.WriteLine($"For x = {x}, predicted y = {y}");
    }
}
```

## Vorcyc.Mathematics.MachineLearning.SupportVectorMachine&lt;TSelf> 类

Vorcyc.Mathematics.MachineLearning.SupportVectorMachine&lt;TSelf> 是一个简单的线性支持向量机（SVM）实现，支持选择核函数。

### 属性

#### 1. Task
- `public MachineLearningTask Task { get; }`
  - 获取支持向量机的任务类型，支持分类和回归。

### 构造器

#### 1. SupportVectorMachine
- `public SupportVectorMachine(int featureCount, TSelf? learningRate = null, int epochs = 1000, SupportVectorMachineKernelType kernelType = SupportVectorMachineKernelType.Linear)`
  - 初始化 SupportVectorMachine 类的新实例。
  - 参数:
    - `featureCount`: 输入数据的特征数量。
    - `learningRate`: 训练算法的学习率。为 null 时默认为 0.01。
    - `epochs`: 训练的轮数。
    - `kernelType`: 核函数类型。默认为线性核函数。

### 方法

#### 1. Train
- `public void Train(TSelf[][] inputs, int[] outputs)`
  - 使用提供的训练数据训练 SVM。
  - 参数:
    - `inputs`: 输入数据，每个元素是一个特征值数组。
    - `outputs`: 与输入数据对应的输出标签。

#### 2. Predict
- `public int Predict(Span<TSelf> input)`
  - 预测给定输入数据的类别标签。
  - 参数:
    - `input`: 输入数据，一个特征值数组。
  - 返回值: 预测的类别标签（1 或 -1）。

#### 3. PredictRaw
- `private TSelf PredictRaw(TSelf[] input)`
  - 计算给定输入数据的原始预测值。
  - 参数:
    - `input`: 输入数据，一个特征值数组。
  - 返回值: 原始预测值。

#### 4. GetKernelFunction
- `private Func<TSelf[], TSelf[], TSelf> GetKernelFunction(SupportVectorMachineKernelType kernelType)`
  - 获取指定类型的核函数。
  - 参数:
    - `kernelType`: 核函数类型。
  - 返回值: 核函数的委托。

#### 5. LinearKernel
- `private static TSelf LinearKernel(TSelf[] x, TSelf[] y)`
  - 线性核函数。
  - 参数:
    - `x`: 第一个输入向量。
    - `y`: 第二个输入向量。
  - 返回值: 核函数的计算结果。

#### 6. PolynomialKernel
- `private static TSelf PolynomialKernel(TSelf[] x, TSelf[] y)`
  - 多项式核函数。
  - 参数:
    - `x`: 第一个输入向量。
    - `y`: 第二个输入向量。
  - 返回值: 核函数的计算结果。

#### 7. GaussianKernel
- `private static TSelf GaussianKernel(TSelf[] x, TSelf[] y)`
  - 高斯核函数（RBF）。
  - 参数:
    - `x`: 第一个输入向量。
    - `y`: 第二个输入向量。
  - 返回值: 核函数的计算结果。

#### 8. SigmoidKernel
- `private static TSelf SigmoidKernel(TSelf[] x, TSelf[] y)`
  - Sigmoid 核函数。
  - 参数:
    - `x`: 第一个输入向量。
    - `y`: 第二个输入向量。
  - 返回值: 核函数的计算结果。

### 代码示例
以下是一个使用 SupportVectorMachine&lt;TSelf> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.MachineLearning;
public class SupportVectorMachineExample
{
    public static void Main()
    {
        // 定义训练数据
        double[][] inputs = new double[][] { new double[] { 1.0, 2.0 }, new double[] { 2.0, 3.0 }, new double[] { 3.0, 3.0 }, new double[] { 6.0, 5.0 }, new double[] { 7.0, 8.0 } };
        int[] outputs = new int[] { 1, 1, -1, -1, 1 };
        // 创建支持向量机实例
        var svm = new SupportVectorMachine<double>(2);

        // 训练支持向量机
        svm.Train(inputs, outputs);

        // 预测新输入的类别标签
        double[] newInput = new double[] { 4.0, 4.0 };
        int prediction = svm.Predict(newInput);
        Console.WriteLine($"Prediction for input {string.Join(", ", newInput)}: {prediction}");
    }
}
```


## Vorcyc.Mathematics.MachineLearning.Classfication.NaiveBayes 类

Vorcyc.Mathematics.MachineLearning.Classfication.NaiveBayes 是一个用于分类的朴素贝叶斯分类器类。

### 属性

无公开属性。

### 构造器

#### 1. NaiveBayes
- `public NaiveBayes()`
  - 初始化 NaiveBayes 类的新实例。

### 方法

#### 1. Train
- `public void Train(string[] features, string label)`
  - 训练模型。
  - 参数:
    - `features`: 特征数组。
    - `label`: 类别标签。

#### 2. Predict
- `public string Predict(string[] features)`
  - 预测类别。
  - 参数:
    - `features`: 特征数组。
  - 返回值: 预测的类别标签。

### 代码示例
以下是一个使用 NaiveBayes 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.MachineLearning.Classfication;
public class NaiveBayesExample
{
    public static void Main()
    {
        // 创建朴素贝叶斯分类器实例
        var nb = new NaiveBayes();
        // 训练数据
        nb.Train(new string[] { "sunny", "hot", "high", "false" }, "no");
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
```


## Vorcyc.Mathematics.MachineLearning.Clustering.DBSCAN&lt;T> 类

Vorcyc.Mathematics.MachineLearning.Clustering.DBSCAN&lt;T> 是一个用于二维平面点的 DBSCAN 聚类算法类。

### 属性

#### 1. Task
- `public MachineLearningTask Task { get; }`
  - 获取 DBSCAN 的任务类型，支持聚类。

### 构造器

#### 1. DBSCAN
- `public DBSCAN(Point<T>[] points, T eps, int minPts)`
  - 使用指定的点、邻域半径和最小点数初始化 DBSCAN 类的新实例。
  - 参数:
    - `points`: 要聚类的点。
    - `eps`: 邻域半径。
    - `minPts`: 形成一个聚类的最小点数。

### 方法

#### 1. Cluster
- `public List<List<Point<T>>> Cluster()`
  - 执行 DBSCAN 聚类。
  - 返回值: 聚类的列表，每个聚类是一个点的列表。

#### 2. ExpandCluster
- `private void ExpandCluster(Point<T> point, List<Point<T>> neighbors, List<Point<T>> cluster)`
  - 扩展聚类。
  - 参数:
    - `point`: 当前点。
    - `neighbors`: 当前点的邻居。
    - `cluster`: 当前聚类。

#### 3. GetNeighbors
- `private List<Point<T>> GetNeighbors(Point<T> point)`
  - 获取点的邻居。
  - 参数:
    - `point`: 当前点。
  - 返回值: 邻居的列表。

### 代码示例
以下是一个使用 DBSCAN&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.MachineLearning.Clustering;
public class DBSCANExample
{
    public static void Main()
    { 
        
        // 定义点
        var points = new Point<double>[] { new Point<double>(1.0, 2.0), new Point<double>(2.0, 2.0), new Point<double>(2.0, 3.0), new Point<double>(8.0, 7.0), new Point<double>(8.0, 8.0), new Point<double>(25.0, 80.0) };
        // 创建 DBSCAN 实例
        var dbscan = new DBSCAN<double>(points, 3.0, 2);

        // 执行聚类
        var clusters = dbscan.Cluster();

        // 输出聚类结果
        for (int i = 0; i < clusters.Count; i++)
        {
            Console.WriteLine($"Cluster {i + 1}:");
            foreach (var point in clusters[i])
            {
                Console.WriteLine($"({point.X}, {point.Y})");
            }
        }
    }
}
```


## Vorcyc.Mathematics.MachineLearning.Clustering.ExpectationMaximization&lt;T> 类

Vorcyc.Mathematics.MachineLearning.Clustering.ExpectationMaximization&lt;T> 是一个用于实现期望最大化（EM）算法的类。

### 属性

#### 1. Means
- `public IReadOnlyList<Vector<T>> Means { get; }`
  - 获取聚类中心（均值）。

#### 2. Covariances
- `public IReadOnlyList<Matrix<T>> Covariances { get; }`
  - 获取协方差矩阵。

#### 3. Weights
- `public IReadOnlyList<T> Weights { get; }`
  - 获取权重。

#### 4. Task
- `public MachineLearningTask Task { get; }`
  - 获取期望最大化算法的任务类型，支持聚类。

### 构造器

#### 1. ExpectationMaximization
- `public ExpectationMaximization(int numClusters)`
  - 初始化 ExpectationMaximization 类的新实例。
  - 参数:
    - `numClusters`: 聚类的数量。

### 方法

#### 1. Fit
- `public void Fit(List<Vector<T>> data)`
  - 使用期望最大化算法拟合数据。
  - 参数:
    - `data`: 要拟合的数据，表示为向量的列表。

### 代码示例
以下是一个使用 ExpectationMaximization&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.MachineLearning.Clustering;
public class ExpectationMaximizationExample
{
    public static void Main()
    { 
        // 定义数据点
        var data = new List<Vector<double>>
        { 
            new Vector<double>(new double[] { 1.0, 2.0 }), 
            new Vector<double>(new double[] { 2.0, 2.0 }), 
            new Vector<double>(new double[] { 2.0, 3.0 }),
            new Vector<double>(new double[] { 8.0, 7.0 }), 
            new Vector<double>(new double[] { 8.0, 8.0 }),
            new Vector<double>(new double[] { 25.0, 80.0 }) 
        };
        // 创建期望最大化实例
        var em = new ExpectationMaximization<double>(2);

        // 拟合数据
        em.Fit(data);

        // 输出聚类中心
        Console.WriteLine("Cluster Centers (Means):");
        foreach (var mean in em.Means)
        {
            Console.WriteLine($"({string.Join(", ", mean.Elements)})");
        }

        // 输出协方差矩阵
        Console.WriteLine("Covariance Matrices:");
        foreach (var covariance in em.Covariances)
        {
            for (int i = 0; i < covariance.Rows; i++)
            {
                for (int j = 0; j < covariance.Columns; j++)
                {
                    Console.Write($"{covariance[i, j]} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        // 输出权重
        Console.WriteLine("Weights:");
        foreach (var weight in em.Weights)
        {
            Console.WriteLine(weight);
        }
    }
}
```

## Vorcyc.Mathematics.MachineLearning.Clustering.GMM&lt;T> 类

Vorcyc.Mathematics.MachineLearning.Clustering.GMM&lt;T> 是一个用于实现高斯混合模型（GMM）的类。

### 属性

#### 1. Means
- `public IReadOnlyList<Vector<T>> Means { get; }`
  - 获取聚类中心（均值）。

#### 2. Covariances
- `public IReadOnlyList<Matrix<T>> Covariances { get; }`
  - 获取协方差矩阵。

#### 3. Weights
- `public IReadOnlyList<T> Weights { get; }`
  - 获取权重。

#### 4. Task
- `public MachineLearningTask Task { get; }`
  - 获取高斯混合模型的任务类型，支持聚类。

### 构造器

#### 1. GMM
- `public GMM(int numComponents, int maxIterations = 100, double tolerance = 1e-6)`
  - 初始化 GMM 类的新实例。
  - 参数:
    - `numComponents`: 高斯分布的数量。
    - `maxIterations`: 最大迭代次数。
    - `tolerance`: 对数似然函数的收敛容差。

### 方法

#### 1. Fit
- `public void Fit(List<Vector<T>> data)`
  - 使用高斯混合模型拟合数据。
  - 参数:
    - `data`: 要拟合的数据，表示为向量的列表。

### 代码示例
以下是一个使用 GMM&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.MachineLearning.Clustering;
public class GMMExample
{
    public static void Main()
    {
        // 定义数据点
        var data = new List<Vector<double>>
        {
            new Vector<double>(new double[] { 1.0, 2.0 }),
            new Vector<double>(new double[] { 2.0, 2.0 }),
            new Vector<double>(new double[] { 2.0, 3.0 }),
            new Vector<double>(new double[] { 8.0, 7.0 }),
            new Vector<double>(new double[] { 8.0, 8.0 }),
            new Vector<double>(new double[] { 25.0, 80.0 })
        };
        // 创建高斯混合模型实例
        var gmm = new GMM<double>(2);

        // 拟合数据
        gmm.Fit(data);

        // 输出聚类中心
        Console.WriteLine("Cluster Centers (Means):");
        foreach (var mean in gmm.Means)
        {
            Console.WriteLine($"({string.Join(", ", mean.Elements)})");
        }

        // 输出协方差矩阵
        Console.WriteLine("Covariance Matrices:");
        foreach (var covariance in gmm.Covariances)
        {
            for (int i = 0; i < covariance.Rows; i++)
            {
                for (int j = 0; j < covariance.Columns; j++)
                {
                    Console.Write($"{covariance[i, j]} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        // 输出权重
        Console.WriteLine("Weights:");
        foreach (var weight in gmm.Weights)
        {
            Console.WriteLine(weight);
        }
    }
}
```


## Vorcyc.Mathematics.MachineLearning.Clustering.HierarchicalClustering&lt;T> 类

Vorcyc.Mathematics.MachineLearning.Clustering.HierarchicalClustering&lt;T> 是一个用于二维平面点的层次聚类算法类。

### 属性

#### 1. Task
- `public MachineLearningTask Task { get; }`
  - 获取层次聚类的任务类型，支持聚类。

### 构造器

#### 1. HierarchicalClustering
- `public HierarchicalClustering(Point<T>[] points)`
  - 使用指定的点初始化 HierarchicalClustering 类的新实例。
  - 参数:
    - `points`: 要聚类的点。

### 方法

#### 1. Cluster
- `public List<List<Point<T>>> Cluster(int k)`
  - 对点执行层次聚类，直到达到指定数量的聚类。
  - 参数:
    - `k`: 所需的聚类数量。
  - 返回值: 聚类的列表，每个聚类是一个点的列表。

#### 2. FindClosestClusters
- `private (int cluster1, int cluster2, T minDistance) FindClosestClusters()`
  - 查找最接近的两个聚类。
  - 返回值: 最接近的两个聚类的索引和它们之间的距离。

#### 3. AverageLinkage
- `private T AverageLinkage(List<Point<T>> cluster1, List<Point<T>> cluster2)`
  - 计算两个聚类之间的平均连接距离。
  - 参数:
    - `cluster1`: 第一个聚类。
    - `cluster2`: 第二个聚类。
  - 返回值: 两个聚类之间的平均连接距离。

#### 4. Distance
- `private T Distance(Point<T> a, Point<T> b)`
  - 计算两个点之间的欧几里得距离。
  - 参数:
    - `a`: 第一个点。
    - `b`: 第二个点。
  - 返回值: 两个点之间的欧几里得距离。

### 代码示例
以下是一个使用 HierarchicalClustering&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.MachineLearning.Clustering;
public class HierarchicalClusteringExample
{
    public static void Main()
    {
        // 定义点
        var points = new Point<double>[]
        {
            new Point<double>(1.0, 2.0),
            new Point<double>(2.0, 2.0),
            new Point<double>(2.0, 3.0),
            new Point<double>(8.0, 7.0),
            new Point<double>(8.0, 8.0),
            new Point<double>(25.0, 80.0)
        };
        // 创建层次聚类实例
        var hierarchicalClustering = new HierarchicalClustering<double>(points);

        // 执行聚类
        var clusters = hierarchicalClustering.Cluster(2);

        // 输出聚类结果
        for (int i = 0; i < clusters.Count; i++)
        {
            Console.WriteLine($"Cluster {i + 1}:");
            foreach (var point in clusters[i])
            {
                Console.WriteLine($"({point.X}, {point.Y})");
            }
        }
    }
}
```


## Vorcyc.Mathematics.MachineLearning.Clustering.KMeansClusterer&lt;T> 类

Vorcyc.Mathematics.MachineLearning.Clustering.KMeansClusterer&lt;T> 是一个用于实现 K 均值聚类算法的类。

### 属性

#### 1. Task
- `public MachineLearningTask Task { get; }`
  - 获取 K 均值聚类的任务类型，支持聚类。

### 构造器

#### 1. KMeansClusterer
- `public KMeansClusterer(T[][] data, int numClusters, int maxIterations, T tolerance)`
  - 使用指定的数据、聚类数量、最大迭代次数和收敛容忍度初始化 KMeansClusterer 类的新实例。
  - 参数:
    - `data`: 要聚类的数据。
    - `numClusters`: 要创建的聚类数量。
    - `maxIterations`: 最大迭代次数。
    - `tolerance`: 收敛容忍度。

### 方法

#### 1. Cluster
- `public int[] Cluster()`
  - 执行 K 均值聚类算法。
  - 返回值: 每个数据点的聚类分配。

#### 2. InitClustering
- `private int[] InitClustering(int numData, int numClusters)`
  - 初始化聚类。
  - 参数:
    - `numData`: 数据的数量。
    - `numClusters`: 聚类的数量。
  - 返回值: 初始化的聚类分配。

#### 3. Allocate
- `private T[][] Allocate(int numRows, int numCols)`
  - 分配二维数组的空间。
  - 参数:
    - `numRows`: 行数。
    - `numCols`: 列数。
  - 返回值: 分配的二维数组。

#### 4. UpdateCentroids
- `private bool UpdateCentroids()`
  - 更新质心（聚类中心）。
  - 返回值: 是否成功更新质心。

#### 5. UpdateClustering
- `private bool UpdateClustering()`
  - 更新聚类分配。
  - 返回值: 是否成功更新聚类分配。

#### 6. Distance
- `private T Distance(T[] vector1, T[] vector2)`
  - 计算两个向量之间的欧几里得距离。
  - 参数:
    - `vector1`: 第一个向量。
    - `vector2`: 第二个向量。
  - 返回值: 两个向量之间的欧几里得距离。

#### 7. MinIndex
- `private int MinIndex(T[] distances)`
  - 找到数组中最小值的索引。
  - 参数:
    - `distances`: 表示距离的数组。
  - 返回值: 最小值的索引。

### 代码示例
以下是一个使用 KMeansClusterer&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.MachineLearning.Clustering;
public class KMeansClustererExample
{
    public static void Main()
    {
        // 定义数据点
        var data = new double[][]
        {
            new double[] { 1.0, 2.0 },
            new double[] { 2.0, 2.0 },
            new double[] { 2.0, 3.0 },
            new double[] { 8.0, 7.0 },
            new double[] { 8.0, 8.0 },
            new double[] { 25.0, 80.0 }
        };
        // 创建 KMeansClusterer 实例
        var kMeans = new KMeansClusterer<double>(data, 2, 100, 1e-6);

        // 执行聚类
        var clustering = kMeans.Cluster();

        // 输出聚类结果
        for (int i = 0; i < clustering.Length; i++)
        {
            Console.WriteLine($"Data point {i + 1} is in cluster {clustering[i] + 1}");
        }
    }
}
```


## Vorcyc.Mathematics.MachineLearning.Clustering.VectorQuantization&lt;T> 类

Vorcyc.Mathematics.MachineLearning.Clustering.VectorQuantization&lt;T> 是一个用于实现矢量量化算法的类。

### 属性

#### 1. Codebook
- `public IReadOnlyList<Vorcyc.Mathematics.LinearAlgebra.Vector<T>> Codebook { get; }`
  - 获取码书。

#### 2. Task
- `public MachineLearningTask Task { get; }`
  - 获取矢量量化的任务类型，支持聚类。

### 构造器

#### 1. VectorQuantization
- `public VectorQuantization(int codebookSize, int dimensions)`
  - 初始化 VectorQuantization 类的新实例。
  - 参数:
    - `codebookSize`: 码书的大小。
    - `dimensions`: 矢量的维度。

### 方法

#### 1. Train
- `public List<T> Train(IEnumerable<Vorcyc.Mathematics.LinearAlgebra.Vector<T>> data, int maxIterations = 100)`
  - 训练矢量量化模型。
  - 参数:
    - `data`: 训练数据，包含多个矢量。
    - `maxIterations`: 最大迭代次数。
  - 返回值: 每次迭代的误差列表。

#### 2. Predict
- `public Vorcyc.Mathematics.LinearAlgebra.Vector<T> Predict(Vorcyc.Mathematics.LinearAlgebra.Vector<T> vector)`
  - 根据输入矢量找到最近的码矢量。
  - 参数:
    - `vector`: 输入矢量。
  - 返回值: 最近的码矢量。

#### 3. SaveCodebook
- `public void SaveCodebook(string filePath)`
  - 将码书保存到文件中。
  - 参数:
    - `filePath`: 文件路径。

#### 4. LoadCodebook
- `public void LoadCodebook(string filePath)`
  - 从文件中加载码书。
  - 参数:
    - `filePath`: 文件路径。

### 代码示例
以下是一个使用 VectorQuantization&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using System.Collections.Generic;
using Vorcyc.Mathematics.MachineLearning.Clustering;
using Vorcyc.Mathematics.LinearAlgebra;
using System.Numerics;
public class VectorQuantizationExample
{
    public static void Main()
    {
        // 定义数据点
        var data = new List<Vector<double>> 
        { 
            new Vector<double>(new double[] { 1.0, 2.0 }), 
            new Vector<double>(new double[] { 2.0, 2.0 }),
            new Vector<double>(new double[] { 2.0, 3.0 }),
            new Vector<double>(new double[] { 8.0, 7.0 }),
            new Vector<double>(new double[] { 8.0, 8.0 }), 
            new Vector<double>(new double[] { 25.0, 80.0 }) 
        };
        // 创建矢量量化实例
        var vq = new VectorQuantization<double>(2, 2);

        // 训练模型
        var errors = vq.Train(data);

        // 输出训练误差
        Console.WriteLine("Training Errors:");
        foreach (var error in errors)
        {
            Console.WriteLine(error);
        }

        // 预测新矢量
        var newVector = new Vector<double>(new double[] { 3.0, 3.0 });
        var nearestCodeVector = vq.Predict(newVector);
        Console.WriteLine($"Nearest code vector to ({string.Join(", ", newVector.Elements)}): ({string.Join(", ", nearestCodeVector.Elements)})");

        // 保存码书
        vq.SaveCodebook("codebook.json");

        // 加载码书
        vq.LoadCodebook("codebook.json");
    }
}
```


## Vorcyc.Mathematics.MachineLearning.DimensionalityReduction.FactorAnalysis&lt;T> 类

Vorcyc.Mathematics.MachineLearning.DimensionalityReduction.FactorAnalysis&lt;T> 是一个用于执行因子分析的类。

### 属性

#### 1. Loadings
- `public Matrix<T> Loadings { get; private set; }`
  - 获取因子载荷矩阵。

#### 2. Communalities
- `public T[] Communalities { get; private set; }`
  - 获取共同性数组。

#### 3. SpecificVariances
- `public T[] SpecificVariances { get; private set; }`
  - 获取特异性方差数组。

#### 4. Task
- `public MachineLearningTask Task { get; }`
  - 获取因子分析的任务类型，支持降维。

### 构造器

#### 1. FactorAnalysis
- `public FactorAnalysis()`
  - 初始化 FactorAnalysis 类的新实例。

### 方法

#### 1. Analyze
- `public void Analyze(Matrix<T> data, int numFactors)`
  - 执行因子分析。
  - 参数:
    - `data`: 数据矩阵，每行代表一个样本，每列代表一个变量。
    - `numFactors`: 因子数量。

### 代码示例
以下是一个使用 FactorAnalysis&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;
using Vorcyc.Mathematics.LinearAlgebra;
public class FactorAnalysisExample
{
    public static void Main()
    {
        // 定义数据矩阵
        var data = new Matrix<double>(
            new double[,] 
            {
                { 1.0, 2.0, 3.0 }, 
                { 2.0, 3.0, 4.0 }, 
                { 3.0, 4.0, 5.0 }, 
                { 4.0, 5.0, 6.0 }, 
                { 5.0, 6.0, 7.0 } 
            });
        // 创建因子分析实例
        var factorAnalysis = new FactorAnalysis<double>();

        // 执行因子分析
        factorAnalysis.Analyze(data, 2);

        // 输出因子载荷矩阵
        Console.WriteLine("Factor Loadings:");
        for (int i = 0; i < factorAnalysis.Loadings.Rows; i++)
        {
            for (int j = 0; j < factorAnalysis.Loadings.Columns; j++)
            {
                Console.Write($"{factorAnalysis.Loadings[i, j]} ");
            }
            Console.WriteLine();
        }

        // 输出共同性数组
        Console.WriteLine("Communalities:");
        foreach (var communality in factorAnalysis.Communalities)
        {
            Console.WriteLine(communality);
        }

        // 输出特异性方差数组
        Console.WriteLine("Specific Variances:");
        foreach (var specificVariance in factorAnalysis.SpecificVariances)
        {
            Console.WriteLine(specificVariance);
        }
    }
}
```


## Vorcyc.Mathematics.MachineLearning.DimensionalityReduction.PCA&lt;TSelf> 类

Vorcyc.Mathematics.MachineLearning.DimensionalityReduction.PCA&lt;TSelf> 是一个用于降维和特征提取的主成分分析 (PCA) 类。

### 属性

#### 1. Task
- `public MachineLearningTask Task { get; }`
  - 获取 PCA 的任务类型，支持降维。

### 构造器

#### 1. PCA
- `public PCA(TSelf[,] data)`
  - 初始化 PCA 类的新实例。
  - 参数:
    - `data`: 输入数据集，每行是一个样本，每列是一个特征。

### 方法

#### 1. Transform
- `public TSelf[,] Transform()`
  - 将原始数据转换为主成分。
  - 返回值: 转换后的主成分数据。

#### 2. GetExplainedVarianceRatio
- `public TSelf[] GetExplainedVarianceRatio()`
  - 获取解释的方差比例。
  - 返回值: 解释的方差比例数组。

### 代码示例
以下是一个使用 PCA&lt;TSelf> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;
public class PCAExample
{
    public static void Main()
    {
        // 定义数据矩阵
        var data = new double[,]
        {
          { 1.0, 2.0, 3.0 },
          { 2.0, 3.0, 4.0 },
          { 3.0, 4.0, 5.0 },
          { 4.0, 5.0, 6.0 },
          { 5.0, 6.0, 7.0 }
        };
        // 创建 PCA 实例
        var pca = new PCA<double>(data);

        // 转换数据
        var transformedData = pca.Transform();

        // 输出转换后的数据
        Console.WriteLine("Transformed Data:");
        for (int i = 0; i < transformedData.GetLength(0); i++)
        {
            for (int j = 0; j < transformedData.GetLength(1); j++)
            {
                Console.Write($"{transformedData[i, j]} ");
            }
            Console.WriteLine();
        }

        // 输出解释的方差比例
        var explainedVarianceRatio = pca.GetExplainedVarianceRatio();
        Console.WriteLine("Explained Variance Ratio:");
        foreach (var ratio in explainedVarianceRatio)
        {
            Console.WriteLine(ratio);
        }
    }
}
```


## Vorcyc.Mathematics.MachineLearning.DimensionalityReduction.TSNE&lt;T> 类

Vorcyc.Mathematics.MachineLearning.DimensionalityReduction.TSNE&lt;T> 是一个用于降维和数据可视化的 t-SNE 算法类。

### 属性

无公开属性。

### 构造器

#### 1. TSNE
- `public TSNE(int perplexity = 30, int maxIter = 1000, T learningRate = default)`
  - 初始化 TSNE 类的新实例。
  - 参数:
    - `perplexity`: 困惑度参数。
    - `maxIter`: 最大迭代次数。
    - `learningRate`: 学习率。

### 方法

#### 1. FitTransform
- `public Matrix<T> FitTransform(Matrix<T> data)`
  - 执行 t-SNE 算法并返回降维后的矩阵。
  - 参数:
    - `data`: 输入的高维数据矩阵。
  - 返回值: 降维后的矩阵。

### 代码示例
以下是一个使用 TSNE&lt;T> 类中多个方法的示例，并在示例中加入了注释：

```csharp
using System;
using Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;
using Vorcyc.Mathematics.LinearAlgebra;
public class TSNEExample
{
    public static void Main()
    {
        // 定义数据矩阵
        var data = new Matrix<double>(
            new double[,]
            { 
                { 1.0, 2.0, 3.0 }, 
                { 2.0, 3.0, 4.0 }, 
                { 3.0, 4.0, 5.0 }, 
                { 4.0, 5.0, 6.0 }, 
                { 5.0, 6.0, 7.0 } 
            });
        // 创建 t-SNE 实例
        var tsne = new TSNE<double>();

        // 执行 t-SNE 算法
        var transformedData = tsne.FitTransform(data);

        // 输出降维后的数据
        Console.WriteLine("Transformed Data:");
        for (int i = 0; i < transformedData.Rows; i++)
        {
            for (int j = 0; j < transformedData.Columns; j++)
            {
                Console.Write($"{transformedData[i, j]} ");
            }
            Console.WriteLine();
        }
    }
}
```

# 距离度量类

## Vorcyc.Mathematics.MachineLearning.Distances.Angular&lt;TSelf> 类

### 描述
Angular 距离，或 Cosine 距离的正确度量版本。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

- `public static TSelf Similarity(TSelf[] x, TSelf[] y)`
  - 获取两个点之间的相似度。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间相似度的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.ArgMax&lt;TSelf> 类

### 描述
ArgMax 距离（L0 距离）。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.BrayCurtis&lt;TSelf> 类

### 描述
Bray-Curtis 距离。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Canberra&lt;TSelf> 类

### 描述
Canberra 距离。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Chebyshev&lt;TSelf> 类

### 描述
Chebyshev 距离。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Cosine&lt;TSelf> 类

### 描述
Cosine 距离。对于正确的距离度量，请参见 Angular 距离。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

- `public static TSelf Similarity(TSelf[] x, TSelf[] y)`
  - 获取两个点之间的相似度。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间相似度的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Dice&lt;TSelf> 类

### 描述
Dice 不相似度。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Euclidean&lt;TSelf> 类

### 描述
欧几里得距离度量。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

- `public static TSelf Similarity(TSelf[] x, TSelf[] y)`
  - 获取两个点之间的相似度。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间相似度的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Hamming&lt;TSelf> 类

### 描述
汉明距离。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Hellinger&lt;TSelf> 类

### 描述
Hellinger 距离。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Jaccard&lt;TSelf> 类

### 描述
Jaccard（指数）距离。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

- `public static TSelf Similarity(TSelf[] x, TSelf[] y)`
  - 获取两个点之间的相似度。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间相似度的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Kulczynski&lt;TSelf> 类

### 描述
Kulczynski 不相似度。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Levenshtein 类

### 描述
Levenshtein 距离。

### 方法
- `public double Distance(string x, string y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Manhattan&lt;TSelf> 类

### 描述
曼哈顿（也称为 Taxicab 或 L1）距离。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Matching&lt;TSelf> 类

### 描述
Matching 不相似度。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Minkowski&lt;TNumber> 类

### 描述
Minkowski 距离是一个在范数向量空间中的度量，可以看作是欧几里得距离和曼哈顿距离的推广。

### 属性
- `public static TNumber Order { get; set; }`
  - 获取或设置此 Minkowski 距离的阶数 `p`。

### 方法
- `public static TNumber Distance(TNumber[] x, TNumber[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.PearsonCorrelation&lt;TSelf> 类

### 描述
Pearson 相关性相似度。

### 方法
- `public static TSelf Similarity(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的相似度。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间相似度的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.RogersTanimoto&lt;TSelf> 类

### 描述
Rogers-Tanimoto 不相似度。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.RusselRao&lt;TSelf> 类

### 描述
Russel-Rao 不相似度。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.SokalMichener&lt;TSelf> 类

### 描述
Sokal-Michener 不相似度。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.SokalSneath&lt;TSelf> 类

### 描述
Sokal-Sneath 不相似度。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.WeightedEuclidean&lt;TSelf> 类

### 描述
加权平方欧几里得距离和相似度。请注意，此距离不是度量，因为它不遵守三角不等式。

### 属性
- `public static TSelf[] Weights { get; set; }`
  - 获取或设置每个维度的权重。默认值是一个全为 1 的向量。

### 方法
- `public static void SetDimensions(int dimensions)`
  - 将 Weights 设置为指定维度的全 1 向量。
  - 参数:
    - `dimensions`: 数据集中的维度（列）数。

- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

- `public static TSelf Similarity(TSelf[] x, TSelf[] y)`
  - 获取两个点之间的相似度。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间相似度的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.WeightedSquareEuclidean&lt;TSelf> 类

### 描述
加权平方欧几里得距离和相似度。请注意，此距离不是度量，因为它不遵守三角不等式。

### 属性
- `public TSelf[] Weights { get; set; }`
  - 获取或设置每个维度的权重。默认值是一个全为 1 的向量。

### 方法
- `public static void SetDimensions(int dimensions)`
  - 将 Weights 设置为指定维度的全 1 向量。
  - 参数:
    - `dimensions`: 数据集中的维度（列）数。

- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

- `public static TSelf Similarity(TSelf[] x, TSelf[] y)`
  - 获取两个点之间的相似度。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间相似度的值。

---

## Vorcyc.Mathematics.MachineLearning.Distances.Yule&lt;TSelf> 类

### 描述
Yule 不相似度。

### 方法
- `public static TSelf Distance(TSelf[] x, TSelf[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的值。

- `public static double Distance(int[] x, int[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的双精度值。

- `public static float Distance(float[] x, float[] y)`
  - 计算点 `x` 和 `y` 之间的距离。
  - 参数:
    - `x`: 第一个点。
    - `y`: 第二个点。
  - 返回值: 表示 `x` 和 `y` 之间距离的单精度值。


