using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vorcyc.Mathematics.MachineLearning;
using Vorcyc.Mathematics.MachineLearning.Clustering;
using Vorcyc.Mathematics.Numerics;

namespace ML_module_test;

internal class HierarchicalClustering_test
{


    public static void go()
    {
        Point<float>[] points =
        [
            new(1, 2),
            new(2, 3),
            new(3, 4),
            new(4, 5),
            new(5, 6)
        ];

        int k = 2;

        var hc = new HierarchicalClustering<float>(points);
        var clusters = hc.Cluster(k);


        for (int i = 0; i < clusters.Count; i++)
        {
            Console.WriteLine($"Cluster {i + 1}:");
            foreach (var point in clusters[i])
            {
                Console.WriteLine($"({point.X}, {point.Y})");
            }
        }

    }
}
