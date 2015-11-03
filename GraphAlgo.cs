using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GraphAlgo
{
    using Graph;

    public class Traversal<T>
    {
        public Traversal(Graph<T> G)
        {
            this.G = G;
            timer = 0;
            order = new int[G.V];
            used = new bool[G.V];

            for (int i = 0; i < G.V; i++)
                if (!used[i]) dfs(i);
        }

        public int traversal_order(int v)
        {
            return order[v];
        }

        private void dfs(int v)
        {
            used[v] = true;
            order[v] = timer++;
            foreach (int to in G.adj_list(v))
                if (!used[to]) dfs(to);
        }

        private Graph<T> G;

        private int timer;
        private int[] order;
        private bool[] used;
    }
}