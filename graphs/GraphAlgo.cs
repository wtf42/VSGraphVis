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

    public class TreeAlgo<T>
    {
        public TreeAlgo(Graph<T> G)
        {
            this.G = G;
            int n = G.V;
            d = new int[n];
            p = new int[n];
            w = new int[n];

            used = new bool[n];
            for (int i = 0; i < n; i++)
            {
                used[i] = false;
                w[i] = 1;
            }
        }

        public int center()
        {
            int s = 0, t = 0;

            bfs(0);
            for (int i = 0; i < G.V; i++)
                if (d[s] < d[i]) s = i;

            bfs(s);
            for (int i = 0; i < G.V; i++)
                if (d[t] < d[i]) t = i;

            List<int> path = new List<int>();
            for (int i = t; i != -1; i = p[i])
                path.Add(i);

            return path[path.Count / 2];
        }

        private void bfs(int s)
        {
            for (int i = 0; i < G.V; i++)
            {
                used[i] = false;
                p[i] = -1;
                d[i] = 0;
            }

            Queue<int> q = new Queue<int>();
            q.Enqueue(s);
            used[s] = true;
            d[s] = 0;

            while (q.Count != 0)
            {
                int v = q.Dequeue();
                foreach (var to in G.adj_list(v))
                    if (!used[to])
                    {
                        d[to] = d[v] + 1;
                        used[to] = true;
                        p[to] = v;
                        q.Enqueue(to);
                    }
            }
        }

        public void dfs(int r, int par = -1)
        {
            if (par == -1)
            {
                for (int i = 0; i < G.V; i++) d[i] = 0;
            }
            p[r] = par;

            foreach(var to in G.adj_list(r))
                if (to != par)
                {
                    d[to] = d[r] + 1;
                    dfs(to, r);
                    w[r] += w[to];
                }
        }

        /*
            number of nodes in the subtree of vertex v
        */
        public int weight(int v)  { return w[v]; }
        
        /*
            distance from root node
        */
        public int height(int v)  { return d[v]; }

        /*
            parent of vertex v int the tree G
        */
        public int parent(int v)  { return p[v]; }

        private int[] d;
        private int[] p;
        private int[] w;
        private bool[] used;

        Graph<T> G;
    }

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