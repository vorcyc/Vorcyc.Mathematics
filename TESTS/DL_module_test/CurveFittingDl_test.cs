using Vorcyc.Mathematics.Experimental.CurveFitting;

namespace DL_module_test;

internal static class CurveFittingDl_test
{
    public static bool Run()
    {
        Span<float> x = [0.1f, 0.3f, 0.5f, 0.7f, 0.9f];
        Span<float> y = [0.01f, 0.09f, 0.25f, 0.49f, 0.81f];

        var result = CurveFitter<float>.NeuralNetwork(x, y, epochs: 2000, hiddenNodes: 8, learningRate: 0.2f);
        float mse = float.CreateTruncating(result.MeanSquaredError);
        return mse < 0.05f;
    }
}
