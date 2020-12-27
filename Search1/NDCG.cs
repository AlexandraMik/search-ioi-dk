using System;
using System.Collections.Generic;

namespace Search1.src
{
    class NDCG
    {
        private static double Zk = 0.23;
        public static double getNDCG(List<List<int>> queries)
        {
            double value = 0;
            foreach (List<int> query_results in queries)
            {
                value += Zk * getNDCGForRequest(query_results);
            }
            value /= queries.Count;
            return value;
        }
        private static double getNDCGForRequest(List<int> docs)
        {
            double value = 0;
            for (int i = 0; i < docs.Count; i++)
            {
                value += (Math.Pow(2, docs[i]) - 1) / (2 + i);
            }
            return value;
        }
    }
}
