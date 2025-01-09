using Vorcyc.Mathematics.Numerics;

namespace core_module_test;

internal class Rational_test
{


    public static void go()
    {

        Rational<int> r = new(100, 200);

        Console.WriteLine(  r);

        Console.WriteLine(r.Reciprocal());


        Console.WriteLine(  r.ToFloatingNumber<float>());

    }
}
