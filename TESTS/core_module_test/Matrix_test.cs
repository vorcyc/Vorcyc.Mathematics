using Vorcyc.Mathematics.LinearAlgebra;

namespace core_module_test;

internal class Matrix_test
{


    public static void go()
    {


        Matrix<int> m = new Matrix<int>(3, 3);
        for (int x = 0; x < m.Rows; x++)
        {
            for (int y = 0; y < m.Columns; y++)
            {
                m[x, y] = x + y;
            }
        }


        m.QRDecomposition(out var q, out var r);

        Console.WriteLine(q);
        Console.WriteLine(r);
    }

}
