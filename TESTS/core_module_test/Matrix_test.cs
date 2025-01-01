using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.Utilities;

namespace core_module_test;

internal class Matrix_test
{


    public static void go()
    {


        var m1 = new Matrix(4, 3);
        for (int i = 0; i < m1.Rows; i++)
        {
            for (int j = 0; j < m1.Columns; j++)
            {
                m1[i, j] = i * 3 + j;
            }
        }

        Console.WriteLine(m1);


        m1.QRDecomposition(out var q1, out var r1);

        Console.WriteLine(q1);
        Console.WriteLine(r1);



        new string('-', 20).PrintLine( ConsoleColor.Red);

        var m = new Matrix<double>(4, 3);
        for (int i = 0; i < m.Rows; i++)
        {
            for (int j = 0; j < m.Columns; j++)
            {
                m[i, j] = i * 3 + j;
            }
        }

        Console.WriteLine(m);


        m.QRDecomposition(out var q, out var r);

        Console.WriteLine(q);
        Console.WriteLine(r);



    }




}


// 类 、 结构体 、 接口 、 枚举 、 委托
//值类型 ：结构体、枚举
//引用类型：类、接口、委托
