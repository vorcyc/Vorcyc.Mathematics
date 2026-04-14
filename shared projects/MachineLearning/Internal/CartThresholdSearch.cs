using System.Numerics;



namespace Vorcyc.Mathematics.MachineLearning.Internal;



/// <summary>

/// 数值 CART 共用的特征阈值搜索与样本索引划分。

/// </summary>

internal static class CartThresholdSearch

{

    public static bool TryFindBestSplit<T>(

        T[,] x,

        int[] indices,

        HashSet<int>? allowedFeatures,

        Func<int, T, ReadOnlySpan<int>, ReadOnlySpan<int>, T> evaluateSplit,

        out int bestFeature,

        out T bestThreshold,

        out int[] bestLeft,

        out int[] bestRight)

        where T : struct, IFloatingPointIeee754<T>

    {

        bestFeature = -1;

        bestThreshold = T.Zero;

        bestLeft = [];

        bestRight = [];

        T bestScore = T.CreateChecked(double.MaxValue);

        int cols = x.GetLength(1);

        int n = indices.Length;



        var leftScratch = new int[n];

        var rightScratch = new int[n];

        var values = new T[n];



        for (int feature = 0; feature < cols; feature++)

        {

            if (allowedFeatures != null && !allowedFeatures.Contains(feature))

                continue;



            for (int i = 0; i < n; i++)

                values[i] = x[indices[i], feature];

            Array.Sort(values);



            for (int t = 0; t < n - 1; t++)

            {

                if (values[t] == values[t + 1])

                    continue;



                T threshold = (values[t] + values[t + 1]) / T.CreateChecked(2.0);

                SplitIndices(x, indices, feature, threshold, leftScratch, rightScratch, out int leftLen, out int rightLen);

                if (leftLen == 0 || rightLen == 0)

                    continue;



                T score = evaluateSplit(

                    feature,

                    threshold,

                    leftScratch.AsSpan(0, leftLen),

                    rightScratch.AsSpan(0, rightLen));

                if (score < bestScore)

                {

                    bestScore = score;

                    bestFeature = feature;

                    bestThreshold = threshold;

                    bestLeft = leftScratch.AsSpan(0, leftLen).ToArray();

                    bestRight = rightScratch.AsSpan(0, rightLen).ToArray();

                }

            }

        }



        return bestFeature >= 0;

    }



    public static void SplitIndices<T>(

        T[,] x,

        int[] indices,

        int feature,

        T threshold,

        int[] leftBuffer,

        int[] rightBuffer,

        out int leftLength,

        out int rightLength)

        where T : struct, IFloatingPointIeee754<T>

    {

        leftLength = 0;

        rightLength = 0;

        foreach (int i in indices)

        {

            if (x[i, feature] <= threshold)

                leftBuffer[leftLength++] = i;

            else

                rightBuffer[rightLength++] = i;

        }

    }



    public static void SplitIndices<T>(

        T[,] x,

        int[] indices,

        int feature,

        T threshold,

        out int[] left,

        out int[] right)

        where T : struct, IFloatingPointIeee754<T>

    {

        int n = indices.Length;

        var leftBuf = new int[n];

        var rightBuf = new int[n];

        SplitIndices(x, indices, feature, threshold, leftBuf, rightBuf, out int leftLen, out int rightLen);

        left = leftBuf.AsSpan(0, leftLen).ToArray();

        right = rightBuf.AsSpan(0, rightLen).ToArray();

    }

}


