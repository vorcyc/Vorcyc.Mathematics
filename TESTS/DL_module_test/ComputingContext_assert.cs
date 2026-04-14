namespace DL_module_test;

internal static class ComputingContext_assert
{
    public static bool TensorsClose(Vorcyc.Mathematics.LinearAlgebra.Tensor<float> a, Vorcyc.Mathematics.LinearAlgebra.Tensor<float> b, float tol, out float maxDiff)
    {
        maxDiff = 0f;
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            return false;
        }

        for (int i = 0; i < a.Values.Length; i++)
        {
            float d = MathF.Abs(a.Values[i] - b.Values[i]);
            if (d > maxDiff)
            {
                maxDiff = d;
            }
        }

        return maxDiff <= tol;
    }

    public static bool TensorsClose(Vorcyc.Mathematics.LinearAlgebra.Tensor a, Vorcyc.Mathematics.LinearAlgebra.Tensor b, float tol, out float maxDiff)
    {
        maxDiff = 0f;
        if (a.Width != b.Width || a.Height != b.Height || a.Depth != b.Depth)
        {
            return false;
        }

        for (int i = 0; i < a.Values.Length; i++)
        {
            float d = MathF.Abs(a.Values[i] - b.Values[i]);
            if (d > maxDiff)
            {
                maxDiff = d;
            }
        }

        return maxDiff <= tol;
    }

    public static bool BatchTensorsClose(Vorcyc.Mathematics.DeepLearning.BatchTensor<float> a, Vorcyc.Mathematics.DeepLearning.BatchTensor<float> b, float tol, out float maxDiff)
    {
        maxDiff = 0f;
        if (a.Batch != b.Batch || a.Height != b.Height || a.Width != b.Width || a.Channels != b.Channels)
        {
            return false;
        }

        for (int i = 0; i < a.Values.Length; i++)
        {
            float d = MathF.Abs(a.Values[i] - b.Values[i]);
            if (d > maxDiff)
            {
                maxDiff = d;
            }
        }

        return maxDiff <= tol;
    }

    public static bool GradientsClose(IReadOnlyList<Vorcyc.Mathematics.DeepLearning.Parameter<float>> a, IReadOnlyList<Vorcyc.Mathematics.DeepLearning.Parameter<float>> b, float tol, out float maxDiff)
    {
        maxDiff = 0f;
        if (a.Count != b.Count)
        {
            return false;
        }

        for (int p = 0; p < a.Count; p++)
        {
            var ga = a[p].Gradient.Values;
            var gb = b[p].Gradient.Values;
            if (ga.Length != gb.Length)
            {
                return false;
            }

            for (int i = 0; i < ga.Length; i++)
            {
                float d = MathF.Abs(ga[i] - gb[i]);
                if (d > maxDiff)
                {
                    maxDiff = d;
                }
            }
        }

        return maxDiff <= tol;
    }

    public static bool FeatureListsClose(List<float[]> a, List<float[]> b, float tol, out float maxDiff)
    {
        maxDiff = 0f;
        if (a.Count != b.Count)
        {
            return false;
        }

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].Length != b[i].Length)
            {
                return false;
            }

            for (int j = 0; j < a[i].Length; j++)
            {
                float d = MathF.Abs(a[i][j] - b[i][j]);
                if (d > maxDiff)
                {
                    maxDiff = d;
                }
            }
        }

        return maxDiff <= tol;
    }
}
