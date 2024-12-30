using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vorcyc.Mathematics.LinearAlgebra;
using Vorcyc.Mathematics.MachineLearning.Clustering;

namespace ML_module_test;

internal class GMM_test
{


    public static void go()
    {
        // 准备数据
        List<Vector<double>> data = new List<Vector<double>>
        {
            new Vector<double>(new double[] { 1.0, 2.0 }),
            new Vector<double>(new double[] { 1.5, 1.8 }),
            new Vector<double>(new double[] { 5.0, 8.0 }),
            new Vector<double>(new double[] { 8.0, 8.0 }),
            new Vector<double>(new double[] { 1.0, 0.6 }),
            new Vector<double>(new double[] { 9.0, 11.0 }),
            new Vector<double>(new double[] { 8.0, 2.0 }),
            new Vector<double>(new double[] { 10.0, 2.0 }),
            new Vector<double>(new double[] { 9.0, 3.0 }),
        };

        // 创建 GMM 实例
        GMM<double> gmm = new GMM<double>(numComponents: 2, maxIterations: 100, tolerance: 1e-6);

        // 拟合数据
        gmm.Fit(data);

        // 获取结果
        var means = gmm.Means;
        var covariances = gmm.Covariances;
        var weights = gmm.Weights;

        // 输出结果
        Console.WriteLine("Means:");
        foreach (var mean in means)
        {
            Console.WriteLine(string.Join(", ", mean.Elements));
        }

        Console.WriteLine("\nCovariances:");
        foreach (var covariance in covariances)
        {
            Console.WriteLine(covariance);
        }

        Console.WriteLine("\nWeights:");
        foreach (var weight in weights)
        {
            Console.WriteLine(weight);
        }
    }



}
