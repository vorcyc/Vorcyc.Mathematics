using Vorcyc.Mathematics.Numerics;

namespace core_module_test;

internal class Rational_test
{


    public static void go()
    {

        Rational<int> r = new(100, 200);

        Console.WriteLine(  r);

        Console.WriteLine(r.Reciprocal());


        Console.WriteLine(  r.ToFloatingPointNumber<float>());



        Rational<int> r1 = new Rational<int>(1, 2);
        Rational<int> r2 = new Rational<int>(3, 4);
        Rational<int> resultAdd = r1 + r2;
        Rational<int> resultSub = r1 - r2;
        Rational<int> resultMul = r1 * r2;
        Rational<int> resultDiv = r1 / r2;

        Console.WriteLine($"Add: {resultAdd}");
        Console.WriteLine($"Subtract: {resultSub}");
        Console.WriteLine($"Multiply: {resultMul}");
        Console.WriteLine($"Divide: {resultDiv}");
    }
}
