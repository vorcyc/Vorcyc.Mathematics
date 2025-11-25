using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

#if NET7_0_OR_GREATER

public interface ISimilarity<TSelf>
//where TSelf : INumber<TSelf>, IRootFunctions<TSelf>, ITrigonometricFunctions<TSelf>
{
    public static abstract TSelf Similarity(TSelf[] x, TSelf[] y);
}


#endif