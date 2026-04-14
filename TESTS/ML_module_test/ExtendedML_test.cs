using Vorcyc.Mathematics.MachineLearning;
using Vorcyc.Mathematics.MachineLearning.DimensionalityReduction;
using Vorcyc.Mathematics.MachineLearning.Regression;

namespace ML_module_test;

public static class ExtendedML_test
{
    public static void Go()
    {
        TestDataSplit();
        TestMultivariateRidgeRegression();
        TestLinearDiscriminantAnalysis();
        TestSupportVectorMachineLinear();
        TestSupportVectorMachineRbf();
    }

    static void TestDataSplit()
    {
        Console.WriteLine("Testing DataSplit...");
        var x = new double[,]
        {
            { 1, 2 },
            { 2, 3 },
            { 3, 4 },
            { 4, 5 },
            { 5, 6 },
            { 6, 7 }
        };
        var y = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };

        var (xTrain, yTrain, xTest, yTest) = DataSplit.TrainTestSplit(x, y, testRatio: 0.33, seed: 42);
        Console.WriteLine($"Train rows: {xTrain.GetLength(0)}, Test rows: {xTest.GetLength(0)}");
        Console.WriteLine($"Train+Test = {xTrain.GetLength(0) + xTest.GetLength(0)} (Expected 6)");
    }

    static void TestMultivariateRidgeRegression()
    {
        Console.WriteLine("Testing MultivariateRidgeRegression...");
        var x = new double[,]
        {
            { 1, 2 },
            { 2, 3 },
            { 3, 1 },
            { 4, 5 },
            { 5, 4 }
        };
        var y = new[] { 2.0, 4.0, 3.0, 8.0, 8.0 };

        var model = new MultivariateRidgeRegression<double>(0.1);
        model.Fit(x, y);
        var prediction = model.Predict([6.0, 7.0]);

        Console.WriteLine($"Intercept: {model.Intercept:F4}");
        Console.WriteLine($"Coefficients: {string.Join(", ", model.Coefficients.Select(c => c.ToString("F4")))}");
        Console.WriteLine($"Predict [6,7]: {prediction:F4}");
        Console.WriteLine($"R²: {model.RSquared:F4}");
    }

    static void TestLinearDiscriminantAnalysis()
    {
        Console.WriteLine("Testing LinearDiscriminantAnalysis...");
        var x = new double[,]
        {
            { 2.0, 2.0 },
            { 2.2, 2.3 },
            { 2.5, 2.1 },
            { 8.0, 8.2 },
            { 8.3, 7.9 },
            { 7.8, 8.1 }
        };
        int[] labels = [0, 0, 0, 1, 1, 1];

        var lda = new LinearDiscriminantAnalysis<double>(numComponents: 1);
        lda.Fit(x, labels);

        int pLow = lda.Predict([2.3, 2.2]);
        int pHigh = lda.Predict([8.1, 8.0]);
        var projected = lda.Transform(x);

        Console.WriteLine($"Predict low cluster: {pLow} (Expected 0)");
        Console.WriteLine($"Predict high cluster: {pHigh} (Expected 1)");
        Console.WriteLine($"Projected shape: {projected.GetLength(0)}x{projected.GetLength(1)}");
    }

    static void TestSupportVectorMachineLinear()
    {
        Console.WriteLine("Testing SVM (linear)...");
        double[][] inputs =
        [
            [0, 0],
            [1, 0],
            [0, 1],
            [1, 1]
        ];
        int[] outputs = [-1, -1, -1, 1];

        var svm = new SupportVectorMachine<double>(featureCount: 2, epochs: 2000, learningRate: 0.1);
        svm.Train(inputs, outputs);

        int prediction = svm.Predict([0.8, 0.8]);
        Console.WriteLine($"Linear SVM predict [0.8,0.8]: {prediction} (Expected 1)");
    }

    static void TestSupportVectorMachineRbf()
    {
        Console.WriteLine("Testing SVM (RBF)...");
        double[][] inputs =
        [
            [0, 0],
            [1, 0],
            [0, 1],
            [1, 1]
        ];
        int[] outputs = [-1, -1, -1, 1];

        var svm = new SupportVectorMachine<double>(
            featureCount: 2,
            epochs: 500,
            learningRate: 0.1,
            kernelType: SupportVectorMachineKernelType.RBF,
            gamma: 2.0);

        svm.Train(inputs, outputs);
        int prediction = svm.Predict([0.8, 0.8]);
        Console.WriteLine($"RBF SVM predict [0.8,0.8]: {prediction} (Expected 1)");
    }
}
