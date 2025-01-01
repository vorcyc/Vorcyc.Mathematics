using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;

namespace core_module_test;

internal class Matrix_test
{


    public static void go()
    {


     


        var nm = new NewMatrix<double>(4,3);
        nm.FillRandom();

        Console.WriteLine(  nm);


        nm[0,0] = 1;

        Console.WriteLine(nm);

        Console.WriteLine(nm[0,0]);


        //m.QRDecomposition(out var q, out var r);

        //Console.WriteLine(q);
        //Console.WriteLine(r);
    }


    public static double Add(double a, double b)
    {
        var x = 1;
        var y = new float[100];

        return a + b;
    }


}


// 类 、 结构体 、 接口 、 枚举 、 委托
//值类型 ：结构体、枚举
//引用类型：类、接口、委托
