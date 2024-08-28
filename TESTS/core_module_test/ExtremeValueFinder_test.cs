using Vorcyc.Mathematics;

namespace core_module_test;

internal class ExtremeValueFinder_test
{

    public static void go()
    {
        var array = new float[1000000];
        //for (int i = 0; i < array.Length; i++)
        //    array[i] = i;
        array.FillWithRandomNumber();

        var r = Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Normal<float>(array);
        Console.WriteLine(r);


        var r2 = Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Vector128(array);
        Console.WriteLine(r2);


        var r3 = Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Vector256(array);
        Console.WriteLine(r3);


        var rx1 = Vorcyc.Mathematics.Statistics.old_extremeValueFinder.FindExtremeValue_Vector128(array);
        Console.WriteLine(rx1);

        var rx2 = Vorcyc.Mathematics.Statistics.old_extremeValueFinder.FindExtremeValue_Vector256(array);
        Console.WriteLine(rx2);

        var r4 = Vorcyc.Mathematics.Statistics.ExtremeValueFinder.FindExtremeValue_Vector512(array);
        Console.WriteLine(r4);
    }

}
