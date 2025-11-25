

using System.Numerics;

namespace Vorcyc.Mathematics.MachineLearning.Distances;

public interface IDistance<TSelf>
    //where TSelf : INumber<TSelf>, IRootFunctions<TSelf>, ITrigonometricFunctions<TSelf>
{

#if NET7_0_OR_GREATER

    public static abstract TSelf Distance(TSelf[] x, TSelf[] y);

#endif

    
}



