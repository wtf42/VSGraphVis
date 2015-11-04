using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radial
{
    using Graph;
    using GraphAlgo;
    using Vector;
    using Layout;

    public class Radial
    {
        public Radial(Graph<Object> G, int W, int H)
        {
            this.G = G;
            this.W = W;
            this.H = H;

            layout = new Layout(W, H);
        }

        public List<Vector> system_config(int root = -1, int p_root = -1)
        {
            this.root = root;
            this.p_root = p_root;

            List<Vector> X = new List<Vector>();
            for (int i = 0; i < G.V; i++)
                X.Add(new Vector(0, 0));

            c   = new double[G.V];
            i   = new double[G.V];
            inc = new double[G.V];
            d = 50.0;

            GA = new TreeAlgo<Object>(G);
            if (root == -1)
                root = GA.center();

            GA.dfs(root, p_root);

            c[root] = 0;
            i[root] = 2 * Math.PI;

            compute_inc();
            foreach(var s in G.adj_list(root))
                dfs(s, root);

            for (int v = 0; v < G.V; v++)
            {
                double h = d * GA.height(v);
                X[v] = new Vector(h * Math.Cos(c[v]), h * Math.Sin(c[v]));
            }

            Layout.getRect(ref X, ref lt, ref rb);
            for (int i = 0; i < X.Count; i++)
                X[i] = layout.getCoord(X[i], lt, rb);
           
            return X;
        }


        /*
            accumulative weight (inc) of the first child is 0,
            and the inc of node v is it's the sum of weights of all
            its siblings placed before v in the order
        */
        void compute_inc()
        {
            if (G.V == 1)
            {
                inc[0] = 0;
                return;
            }

            for (int p = 0; p < G.V; p++)
            {
                int cur = -1;
                if (G.adj[p][0] == GA.parent(p))
                {
                    cur = (G.adj[p].Count > 1) ? 1 : -1;
                }
                else cur = 0;

                if (cur == -1) continue;
              
                int prev = G.adj[p][cur];
                inc[G.adj[p][cur]] = 0; cur++;
                for (; cur <  G.adj[p].Count; cur++)
                {
                    int c = G.adj[p][cur];
                    if (c == GA.parent(p)) continue;

                    inc[c] = inc[prev] + GA.weight(prev);
                    prev = c;
                }
            }
        }

        /*
            computing the angular coordinate (c) and angular interval (l)
            of vertex in the pre-order traversal of the Graph
        */
        void dfs(int v, int p)
        {
            i[v] = i[p] / (double)GA.weight(p) * (double)GA.weight(v);
            c[v] = i[p] / (double)GA.weight(p) * inc[v] + c[p];

            foreach (var to in G.adj_list(v))
                if (to != p) dfs(to, v);
        }

        Graph<Object> G;
        TreeAlgo<Object> GA;

        int root, p_root, W, H;
        double d;
        double[] c;
        double[] i;
        double[] inc;

        Layout layout;
        Vector lt, rb;
    }
}
