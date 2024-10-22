using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vorcyc.Mathematics;

namespace core_module_test;

internal class PinnableArray_test
{

    public static void go()
    {
        {
            PinnableArray<float>.Option.UseLeasingMode = true;


            PinnableArray<float> a = new(1000);
            a.FillWithRandomNumber();

            Console.WriteLine(a.ToString());

            Console.WriteLine(a.Values.Length);
            Console.WriteLine(a.Length);
        }



    }


}
