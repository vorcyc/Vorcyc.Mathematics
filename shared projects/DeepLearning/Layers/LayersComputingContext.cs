namespace Vorcyc.Mathematics.DeepLearning.Layers;

using Vorcyc.Mathematics;

public static partial class Layers
{
    private static void ForEachDepth(int depth, long workPerDepth, Action<int> body)
        => ComputingContextExecution.ForEach(null, 0, depth, body, workPerDepth);
}
