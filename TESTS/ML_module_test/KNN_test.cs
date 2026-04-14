using Vorcyc.Mathematics.MachineLearning;

using Vorcyc.Mathematics.Numerics;



namespace ML_module_test;



public static class KNN_test

{

    public static void Go()

    {

        Console.WriteLine("Testing KnnClassifier...");

        var x = new double[,]

        {

            { 1.0, 2.0 },

            { 2.0, 3.0 },

            { 6.0, 5.0 },

            { 7.0, 8.0 }

        };

        var y = new int[] { 0, 0, 1, 1 };

        var classifier = new KnnClassifier<double>(k: 3);

        classifier.Fit(x, y);

        int label = classifier.Predict([2.5, 2.5]);

        Console.WriteLine($"KnnClassifier: {label} (Expected 0)");



        Console.WriteLine("Testing KNN regression...");

        var regressor = new KNN<double>();

        regressor.Add(new Point<double>(1.0, 0.0), 2.0);

        regressor.Add(new Point<double>(2.0, 0.0), 4.0);

        regressor.Add(new Point<double>(3.0, 0.0), 6.0);

        regressor.Add(new Point<double>(4.0, 0.0), 8.0);



        double value = regressor.Regress(new Point<double>(2.5, 0.0), k: 2);

        Console.WriteLine($"Regression: {value} (Expected ~5.0)");



        Console.WriteLine("Testing KNN n-dimensional regression...");

        var ndRegressor = new KNN<double>();

        ndRegressor.Add([1.0, 0.0], 10.0);

        ndRegressor.Add([0.0, 1.0], 20.0);

        ndRegressor.Add([1.0, 1.0], 30.0);

        double ndValue = ndRegressor.Regress([1.0, 0.0], k: 1);

        Console.WriteLine($"N-D regression: {ndValue} (Expected 10.0)");



        Console.WriteLine("Testing KnnRegressor batch...");

        var xReg = new double[,]

        {

            { 1.0, 0.0 },

            { 2.0, 0.0 },

            { 3.0, 0.0 },

            { 4.0, 0.0 }

        };

        var yReg = new double[] { 2.0, 4.0, 6.0, 8.0 };

        var knnReg = new KnnRegressor<double>(k: 2);

        knnReg.Fit(xReg, yReg);

        double single = (double)knnReg.Predict([2.5, 0.0])!;

        double[] batch = knnReg.PredictBatch(new double[,] { { 2.5, 0.0 }, { 3.5, 0.0 } });

        Console.WriteLine($"KnnRegressor: single={single:F1}, batch[0]={batch[0]:F1}, batch[1]={batch[1]:F1}");



        double[] legacyBatch = regressor.RegressBatch(xReg, k: 2);

        Console.WriteLine($"KNN.RegressBatch[0]={legacyBatch[0]:F1}");

    }

}


