using Vorcyc.Mathematics;
using Vorcyc.Mathematics.MachineLearning;
using Vorcyc.Mathematics.MachineLearning.Classfication;
using Vorcyc.Mathematics.MachineLearning.Preprocessing;
using Vorcyc.Mathematics.MachineLearning.Regression;

namespace MachineLearning_example;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            string command = args.Length == 0 ? "overview" : args[0].ToLowerInvariant();
            return command switch
            {
                "overview" or "all" => RunOverview(),
                "classify" or "classification" => RunClassificationDemo(),
                "regression" => RunRegressionDemo(),
                "computing" or "computingcontext" or "parallel" => RunComputingContextDemo(),
                "help" or "-h" or "--help" => PrintHelp(),
                _ => UnknownCommand(command),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    static int RunOverview()
    {
        Console.WriteLine("=== Vorcyc.Mathematics.MachineLearning — 示例概览 ===\n");
        RunClassificationDemo();
        Console.WriteLine();
        RunRegressionDemo();
        return 0;
    }

    static int RunClassificationDemo()
    {
        Console.WriteLine("=== Vorcyc.Mathematics.MachineLearning — 分类示例 (0.9) ===\n");

        var (x, y) = BlobDataset.CreateThreeBlob(pointsPerClass: 20, seed: 42);
        var (xTrain, yTrain, xTest, yTest) = DataSplit.TrainTestSplit(x, y, testRatio: 0.25, seed: 7);

        Evaluate("NumericDecisionTree", new NumericDecisionTree<double>(maxDepth: 8), xTrain, yTrain, xTest, yTest);
        Evaluate("NumericRandomForest", new NumericRandomForest<double>(numTrees: 60, maxFeatures: 2, maxDepth: 10, seed: 11), xTrain, yTrain, xTest, yTest);
        Evaluate("GaussianNaiveBayes", new GaussianNaiveBayes<double>(), xTrain, yTrain, xTest, yTest);
        Evaluate("KnnClassifier (k=3)", new KnnClassifier<double>(k: 3), xTrain, yTrain, xTest, yTest);
        Evaluate("SoftmaxRegression", new SoftmaxRegression<double>(learningRate: 0.1, epochs: 2500), xTrain, yTrain, xTest, yTest);
        Evaluate("AdaBoostClassifier", new AdaBoostClassifier<double>(nEstimators: 50), xTrain, yTrain, xTest, yTest);
        Evaluate("GradientBoostingClassifier", new GradientBoostingClassifier<double>(nEstimators: 40, learningRate: 0.15, maxDepth: 3), xTrain, yTrain, xTest, yTest);

        Console.WriteLine("\n--- ClassificationPipeline (StandardScaler + Softmax) ---");
        var pipeline = new ClassificationPipeline<double>()
            .AddPreprocessor(new StandardScaler<double>())
            .SetClassifier(new SoftmaxRegression<double>(learningRate: 0.1, epochs: 2000));
        pipeline.Fit(xTrain, yTrain);
        var pipePreds = BlobDataset.PredictAll(pipeline, xTest);
        PrintMetrics(yTest, pipePreds);

        Console.WriteLine("\n--- GridSearch (Softmax 超参) ---");
        var candidates = new[]
        {
            new { Epochs = 1000, Lr = 0.05 },
            new { Epochs = 2000, Lr = 0.10 }
        };
        var search = GridSearch.SearchClassifier(
            x, y,
            candidates,
            opt => new SoftmaxRegression<double>(learningRate: opt.Lr, epochs: opt.Epochs),
            folds: 3,
            seed: 5);
        Console.WriteLine($"Best: epochs={search.BestOptions.Epochs}, lr={search.BestOptions.Lr}");
        Console.WriteLine($"CV macro-F1: {search.BestScore.MeanMacroF1:F3}");

        Console.WriteLine("\n--- CrossValidation (GaussianNaiveBayes, 5-fold) ---");
        var cv = CrossValidation.Validate(
            x, y,
            folds: 5,
            seed: 21,
            buildPredictor: (xTr, yTr) =>
            {
                var model = new GaussianNaiveBayes<double>();
                model.Fit(xTr, yTr);
                return sample => model.Predict(sample);
            });
        Console.WriteLine($"Accuracy: {cv.MeanAccuracy:P1} ± {cv.StdAccuracy:P1}");
        Console.WriteLine($"Macro-F1: {cv.MeanMacroF1:F3} ± {cv.StdMacroF1:F3}");

        return 0;
    }

    static int RunRegressionDemo()
    {
        Console.WriteLine("=== Vorcyc.Mathematics.MachineLearning — 回归示例 (0.9) ===\n");

        var (xSynth, ySynth) = RegressionDataset.CreateLinearSynthetic(rows: 80, cols: 3, seed: 11);
        var (xTrain, yTrain, xTest, yTest) = DataSplit.TrainTestSplit(xSynth, ySynth, testRatio: 0.25, seed: 3);

        Console.WriteLine("--- KnnRegressor (k=5) ---");
        var knnReg = new KnnRegressor<double>(k: 5);
        knnReg.Fit(xTrain, yTrain);
        double knnPred = knnReg.Predict(BlobDataset.GetRow(xTest, 0));
        double[] knnBatch = knnReg.PredictBatch(xTest);
        double mse = EvaluationMetrics.MeanSquaredError(yTest, knnBatch);
        Console.WriteLine($"Sample predict = {knnPred:F4}, batch MSE on test = {mse:F4}");

        Console.WriteLine("\n--- RegressionPipeline (StandardScaler + MLR) ---");
        var pipeline = new RegressionPipeline<double>()
            .AddPreprocessor(new StandardScaler<double>())
            .SetRegressor(new MultipleLinearRegression<double>());
        pipeline.Fit(xTrain, yTrain);
        double[] pipeBatch = pipeline.PredictBatch(xTest);
        Console.WriteLine($"Pipeline batch predictions (first 3): {pipeBatch[0]:F2}, {pipeBatch[1]:F2}, {pipeBatch[2]:F2}");

        var xSmall = new double[,]
        {
            { 1, 2 }, { 2, 3 }, { 3, 1 }, { 4, 5 }, { 5, 4 }, { 6, 6 }
        };
        var ySmall = new double[] { 2, 4, 3, 8, 8, 11 };

        var ridge = new MultivariateRidgeRegression<double>(lambda: 0.01);
        ridge.Fit(xSmall, ySmall);
        double ridgePred = ridge.Predict([6.0, 7.0]);
        Console.WriteLine($"\nRidge (small set) predict [6,7] = {ridgePred:F4}, R² = {ridge.RSquared:F4}");

        var cv = CrossValidation.ValidateRegressor(
            xSynth, ySynth,
            folds: 3,
            seed: 9,
            buildPredictor: (xTr, yTr) =>
            {
                var m = new KnnRegressor<double>(k: 7);
                m.Fit(xTr, yTr);
                return sample => m.Predict(sample);
            });
        Console.WriteLine($"KnnRegressor CV RMSE = {cv.MeanRMSE:F4}, R² = {cv.MeanR2:F4}");

        return 0;
    }

    static int RunComputingContextDemo()
    {
        Console.WriteLine("=== Vorcyc.Mathematics.MachineLearning — ComputingContext 示例 (0.9) ===\n");

        // 较大的数据集，使并行/SIMD 的差异更易观察
        var (x, y) = BlobDataset.CreateThreeBlob(pointsPerClass: 1500, seed: 42);
        var (xTrain, yTrain, xTest, yTest) = DataSplit.TrainTestSplit(x, y, testRatio: 0.25, seed: 7);

        // 1) 按模型设置 Context：随机森林的训练与批量预测均按所选策略执行
        Console.WriteLine("--- 按模型 Context（NumericRandomForest）---");
        foreach (var ctx in new[] { ComputingContext.Normal, ComputingContext.Parallel })
        {
            var forest = new NumericRandomForest<double>(numTrees: 60, maxFeatures: 2, maxDepth: 12, seed: 11)
            {
                Context = ctx
            };
            (int[] preds, double ms) = Timed(() =>
            {
                forest.Fit(xTrain, yTrain);
                return forest.PredictBatch(xTest);
            });
            double acc = EvaluationMetrics.Accuracy(yTest, preds);
            Console.WriteLine($"{ctx.CpuMode,-8} | fit+predict {ms,7:F1} ms | accuracy {acc:P1}");
        }

        // 2) 按作用域：Context 未设置时，模型与指标都遵循环境作用域
        Console.WriteLine("\n--- 按作用域 ComputingScope（KnnClassifier + 指标）---");
        var knn = new KnnClassifier<double>(k: 5);
        knn.Fit(xTrain, yTrain);
        using (ComputingScope.Enter(ComputingContext.Parallel))
        {
            int[] preds = knn.PredictBatch(xTest);                        // 遵循作用域并行
            var confusion = ClassificationMetrics.ConfusionMatrix(yTest, preds); // 同样遵循作用域
            double acc = EvaluationMetrics.Accuracy(yTest, preds);
            double macroF1 = ClassificationMetrics.MacroF1(confusion);
            Console.WriteLine($"Scope=Parallel | accuracy {acc:P1}, Macro-F1 {macroF1:F3}");
        }

        // 3) 单次调用覆盖：ModelBatchExtensions.PredictBatch 接受显式 context（用于泛型回退）
        // AdaBoostClassifier 未实现 IBatchClassifier，因此走遵循 context 参数的泛型批量预测路径。
        Console.WriteLine("\n--- 单次调用覆盖（ModelBatchExtensions.PredictBatch context）---");
        var ada = new AdaBoostClassifier<double>(nEstimators: 50);
        ada.Fit(xTrain, yTrain);
        int[] simdPreds = ModelBatchExtensions.PredictBatch(ada, xTest, ComputingContext.Parallel);
        Console.WriteLine($"context=Parallel | accuracy {EvaluationMetrics.Accuracy(yTest, simdPreds):P1}");

        Console.WriteLine("\n提示: 并行/串行结果在数值上完全一致，差异仅在于执行策略与速度。");
        return 0;
    }

    static (TResult result, double elapsedMs) Timed<TResult>(Func<TResult> action)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = action();
        sw.Stop();
        return (result, sw.Elapsed.TotalMilliseconds);
    }

    static void Evaluate(string name, IClassifier<double> model, double[,] xTrain, int[] yTrain, double[,] xTest, int[] yTest)
    {
        Console.WriteLine($"--- {name} ---");
        model.Fit(xTrain, yTrain);
        var predictions = BlobDataset.PredictAll(model, xTest);
        PrintMetrics(yTest, predictions);
    }

    static void PrintMetrics(int[] actual, int[] predicted)
    {
        double accuracy = EvaluationMetrics.Accuracy(actual, predicted);
        var cm = ClassificationMetrics.ConfusionMatrix(actual, predicted);
        double macroF1 = ClassificationMetrics.MacroF1(cm);
        Console.WriteLine($"Accuracy: {accuracy:P1}, Macro-F1: {macroF1:F3}");
    }

    static int PrintHelp()
    {
        Console.WriteLine("""
            Vorcyc.Mathematics — Machine Learning 示例 (0.9)

            用法:
              dotnet run --project Examples/MachineLearning_example
              dotnet run --project Examples/MachineLearning_example -- <command>

            命令:
              overview    分类 + 回归全部演示（默认）
              classify    三簇合成数据上的分类器、流水线、网格搜索与交叉验证
              regression  KnnRegressor、回归流水线批量预测与交叉验证
              computing   ComputingContext / ComputingScope 并行执行演示
              help        显示本帮助
            """);
        return 0;
    }

    static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"未知命令: {command}");
        PrintHelp();
        return 1;
    }
}
