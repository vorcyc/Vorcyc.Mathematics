﻿using Vorcyc.Mathematics;
using Vorcyc.Mathematics.Framework.Utilities;

namespace core_module_test;

internal class SimpleLinearRegression_test
{

    public static void go()
    {
        //var n = new int[] { 1000, 100000, 500000, 500000, 100000, 500000, 1000000, 5000000, 10000000, 20000000, 30000000, 40000000, 50000000 };
        var n = new int[10];
        n.FillWithRandomNumber((500000, 1000));
        foreach (var count in n)
        {

            count.PrintLine(ConsoleColor.Green);
            var x = new PinnableArray<float>(count, false);
            var y = new PinnableArray<float>(count, false);
            x.FillWithRandomNumber();
            y.FillWithRandomNumber();


            var r = new Vorcyc.Mathematics.MachineLearning.SimpleLinearRegression<float>();
            var c= r.Learn(x, y);

            c.PrintLine(ConsoleColor.Red);


            Console.WriteLine("-------------");

            x.Dispose();
            y.Dispose();
        }

    }

}
