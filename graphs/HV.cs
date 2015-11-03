using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HV
{
    using Vector;
    using Graph;
    using GraphAlgo;
    using Layout;

    public class RightHeavyHV
    {
        public RightHeavyHV(Graph<Object> G, int W, int H)
        {
            this.G = G;
            this.W = W;
            this.H = H;

            layout = new Layout(W, H);
        }

        public List<Vector> system_config(int root = -1, int p_root = -1)
        {
            HV hv = new HV();
            List<Vector> X = hv.system_config(G, root, p_root);
            Layout.getRect(ref X, ref lt, ref rb);

            for (int i = 0; i < X.Count; i++)
                X[i] = layout.getCoord(X[i], lt, rb);

            return X;
        }

        Layout layout;

        Graph<Object> G;
        int W, H;

        Vector lt, rb; 
    }

    
    class HV
    {
        public HV() { init(); }

        public void init(int r = -1, int p_r = -1)
        {
            sx = INF; sy = INF;
            root = r;
            p_root = p_r;
            bottom = INF;
        }

        public List<Vector> system_config(Graph<Object> G, int root = -1, int p_root = -1)
        {
            init(root, p_root); // for safety!!!
            this.G = G;

            List<Vector> X = new List<Vector>();
            for (int i = 0; i < G.V; i++)
                X.Add(new Vector(0,0));

            TreeAlgo<Object> GA = new TreeAlgo<Object>(G);
            if (root == -1)
                root = GA.center();

            GA.dfs(root, p_root);

            right = new int[G.V];
            for (int i = 0; i < G.V; i++)
                right[i] = 0;

            X[root] = new Vector(sx == INF ? 0 : sx, sy == INF ? 0 : sy);
            setPos(root, p_root, ref X, ref GA);

            return X;
        }

        public void setStartPoint(int x, int y)
        {
            sx = x; sy = y;
        }

        private void setPos(int v, int p, ref List<Vector> X, ref TreeAlgo<Object> GA)
        {
            bottom = Math.Min(bottom, (int)X[v][1]);

            int max_w = G.adj[v][0];
            int prev = -1;

            foreach(var to in G.adj_list(v))
            {
                if (max_w == p && to != p) max_w = to;

                if (to != p && GA.weight(max_w) < GA.weight(to))
                    max_w = to;
            }

            foreach(var to in G.adj_list(v))
            {
                if (to != p && to != max_w)
                {
                    X[to][0] = (prev == -1) ? X[v][0] : (right[prev] + 1);
                    X[to][1] = X[v][1] - 1;

                    prev = to;

                    setPos(to, v, ref X, ref GA);

                    right[v] = Math.Max(right[v], right[to]);
                }
            }

            if (max_w != p)
            {
                X[max_w][0] = (prev == -1) ? (X[v][0] + 1) :
                                            (right[prev] + 1);
                X[max_w][1] = X[v][1];

                setPos(max_w, v, ref X, ref GA);
            }
            else right[v] = (int)X[v][0];

            if (p != -1) right[p] = Math.Max(right[p], right[v]);
        }

        Graph<Object> G;
        int[] right;
        int bottom;
        int sx, sy;
        int root, p_root;

        const int INF = (int)1e9;
    }
}
