using Vorcyc.Mathematics;

namespace core_module_test;

internal class ExtremeValueFinder_test
{

    public static void go()
    {
        var n = new int[] { 1000, 100000, 500000, 100000, 500000, 1000000, 5000000, 10000000 };
        foreach (var count in n)
        {

            var array = new float[count];
            array.FillWithRandomNumber();

            var r = Vorcyc.Mathematics.Statistics.FindExtremeValue_Normal<float>(array);
            Console.WriteLine(r);


            var r2 = Vorcyc.Mathematics.Statistics.FindExtremeValue_Vector128(array);
            Console.WriteLine(r2);


            var r3 = Vorcyc.Mathematics.Statistics.FindExtremeValue_Vector256(array);
            Console.WriteLine(r3);


            var rx1 = Vorcyc.Mathematics.Statistics.FindExtremeValue_Vector128(array);
            Console.WriteLine(rx1);

            var rx2 = Vorcyc.Mathematics.Statistics.FindExtremeValue_Vector256(array);
            Console.WriteLine(rx2);

            //var r4 = Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Vector512(array);
            //Console.WriteLine(r4);

        }
    }

}
