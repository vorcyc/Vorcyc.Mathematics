﻿using System.Runtime.CompilerServices;

namespace Vorcyc.Mathematics.MachineLearning.Distances;


/// <summary>
///   Levenshtein distance.
/// </summary>
/// 
/// <remarks>
/// <para>
///   In information theory and computer science, the Levenshtein distance is a
///   string metric for measuring the difference between two sequences. Informally,
///   the Levenshtein distance between two words is the minimum number of single-character 
///   edits (i.e. insertions, deletions or substitutions) required to change one 
///   word into the other. It is named after Vladimir Levenshtein, who considered 
///   this distance in 1965.</para>
///   
/// <para>
///   Levenshtein distance may also be referred to as edit distance, although that 
///   may also denote a larger family of distance metrics. It is closely related to
///   pairwise string alignments.</para>
/// 
/// <para>
///   References:
///   <list type="bullet">
///     <item><description><a href="https://en.wikipedia.org/wiki/Levenshtein_distance">
///       https://en.wikipedia.org/wiki/Levenshtein_distance </a></description></item>
///   </list></para>  
/// </remarks>
public class Levenshtein
{

    /// <summary>
    ///   Computes the distance <c>d(x,y)</c> between points
    ///   <paramref name="x"/> and <paramref name="y"/>.
    /// </summary>
    /// 
    /// <param name="x">The first point <c>x</c>.</param>
    /// <param name="y">The second point <c>y</c>.</param>
    /// 
    /// <returns>
    ///   A double-precision value representing the distance <c>d(x,y)</c>
    ///   between <paramref name="x"/> and <paramref name="y"/> according 
    ///   to the distance function implemented by this class.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Distance(string x, string y)
    {
        if (x == null || x.Length == 0)
        {
            if (y == null || y.Length == 0)
                return 0;
            return y.Length;
        }
        else
        {
            if (y == null || y.Length == 0)
                return x.Length;
        }

        int[,] d = new int[x.Length + 1, y.Length + 1];

        for (int i = 0; i <= x.Length; i++)
            d[i, 0] = i;

        for (int i = 0; i <= y.Length; i++)
            d[0, i] = i;

        for (int i = 0; i < x.Length; i++)
        {
            for (int j = 0; j < y.Length; j++)
            {
                int cost = x[i] == y[j] ? 0 : 1;

                int a = d[i, j + 1] + 1;
                int b = d[i + 1, j] + 1;
                int c = d[i, j] + cost;

                d[i + 1, j + 1] = Math.Min(Math.Min(a, b), c);
            }
        }

        return d[x.Length, y.Length];
    }
}
