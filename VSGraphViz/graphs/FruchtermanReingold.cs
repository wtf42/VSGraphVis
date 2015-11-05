using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace FruchtermanReingold
{
    using Graph;
    using GraphAlgo;
    using SplayTree;
    using Vector;
    using GraphLayout;

    public class FRLayout : GraphLayout
    {
        public List<List<Vector>> system_config(int W, int H, Graph<Object> G,
                                                int root = -1, int p_root = -1,
                                                List<Vector> initial_config = null)
        {
            List<List<Vector>> X = new List<List<Vector>>();

            FR_grid fr_layout = new FR_grid(G, square_distance_attractive_force.f,
                                      square_distance_repulsive_force.f,
                                      W, H,
                                      initial_config);

            bool equilibria = false;
            List<Vector> xy = new List<Vector>();
            int iter = 0;

            if (initial_config != null)
                X.Add(initial_config);

            while (!equilibria)
            {
                xy = fr_layout.system_config(out equilibria);
                if (iter % 100 == 0 || equilibria)
                {
                    X.Add(xy);
                }
                iter++;
            }

            return X;
        }
    }

    public class linear_cooling
    {
        public linear_cooling(double temp, int iterations)
        {
            _step = temp / iterations;
            _temp = temp;
        }

        public double cool()
        {
            double old_temp = _temp;
            _temp -= _step;
            if (_temp < 0) _temp = 0;
            return old_temp;
        }

        public double t { get { return _temp; } }

        private double _step;
        private double _temp;
    }

    public delegate double Force(double d, double k);

    public class square_distance_repulsive_force
    {
        public static double f(double d, double k) { return k * k / d; }
    }

    public class square_distance_attractive_force
    {
        public static double f(double d, double k) { return d * d / k; }
    }

    public class FR
    {
        public FR(Graph<Object> _G, Force fattractive, Force frepulsive,
                    int _W, int _H, List<Vector> initial_config = null)
        {
            W = _W; H = _H;
            G = new Graph<Config>(_G.V);

            init_config(initial_config);

            foreach (var e in _G)
                G.add(e.u.v, e.v.v);

            k = Math.Sqrt((double)W * H / G.V);
            cool_sch = new linear_cooling(W / 10, W);

            fa = fattractive;
            fr = frepulsive;

            no_rep_force = new ST<int>[G.V];
            for (int i = 0; i < G.V; i++)
                no_rep_force[i] = new ST<int>();
        }

        protected void init_config(List<Vector> initial_config = null)
        {
            /*Random rnd = new Random();
            foreach (var v in G.vertices)
                v.data = new Config(rnd.Next(W), rnd.Next(H));*/

            if (initial_config != null)
            {
                for (int i = 0; i < G.V; i++)
                {
                    double x = Math.Min(W - 1, Math.Max(1, initial_config[i][0]));
                    double y = Math.Min(H - 1, Math.Max(1,initial_config[i][1]));
                    G.vertices[i].data = new Config(x, y);
                }
            }
            else
            {
                Traversal<Config> tr = new Traversal<Config>(G);
                double R = Math.Min(H / 2, W / 2);
                foreach (var v in G.vertices)
                {
                    double angle = Math.PI * (4 * tr.traversal_order(v.v) + G.V) / (2 * G.V);
                    double x = R * Math.Cos(angle) + W / 2,
                           y = R * Math.Sin(angle) + H / 2;

                    v.data = new Config(x == W ? x - 1 : x, y == H ? y - 1 : y);
                }
            }
        }

        protected ST<int>[] no_rep_force;

        public void no_repulsive_force(KeyValuePair<int, int> vert_pair)
        {
            no_rep_force[vert_pair.Key].Add(vert_pair.Value);
            no_rep_force[vert_pair.Value].Add(vert_pair.Key);
        }

        /*
            returns Configuration of the n-body system
            equilibria - признак статического равновесия системы
        */
        public List<Vector> system_config(out bool equilibria)
        {
            if (!this.equilibria)
                iteration();

            equilibria = this.equilibria;

            List<Vector> cfg = new List<Vector>();
            for (int i = 0; i < G.V; i++)
            {
                cfg.Add(new Vector(2));
                cfg[i][0] = G.vertices[i].data.x;
                cfg[i][1] = G.vertices[i].data.y;
            }

            return cfg;
        }

        protected virtual void calculate_fr()
        {
            foreach (var v in G.vertices)
            {
                v.data.disp.assign(0);
                foreach (var u in G.vertices)
                    if (u.v != v.v)
                    {
                        if (no_rep_force[v.v].Contains(u.v)) continue;

                        Vector d = Vector.sub(v.data.pos, u.data.pos);
                        double d_len = Vector.length(d);

                        v.data.disp = Vector.add(v.data.disp,
                                                Vector.mult(Vector.div(d, d_len),
                                                            fr(d_len, k)
                                                            )
                                                 );
                    }
            }
        }

        protected void calculate_fa()
        {
            foreach (var e in G)
            {
                Vector d = Vector.sub(e.v.data.pos, e.u.data.pos);
                double d_len = Vector.length(d);

                Vector tmp = Vector.mult(Vector.div(d, d_len),
                                         fa(d_len, k)
                                         );

                e.v.data.disp = Vector.sub(e.v.data.disp, tmp);
                e.u.data.disp = Vector.add(e.u.data.disp, tmp);
            }
        }

        /*
            limit the maximum displacement to the temperature t
            and then prevent from being displaced outside frame 
        */
        protected virtual void limit_disp()
        {
            foreach (var v in G.vertices)
            {
                double disp_len = Vector.length(v.data.disp);

                v.data.pos = Vector.add(v.data.pos,
                                        Vector.mult(
                                                    Vector.div(v.data.disp, disp_len),
                                                    Math.Min(disp_len, cool_sch.t))
                                        );
                v.data.x = Math.Min(W - 1, Math.Max(0, v.data.x));
                v.data.y = Math.Min(H - 1, Math.Max(0, v.data.y));
            }

            cool_sch.cool();
        }

        protected void iteration()
        {
            // calculate repulsive forces 
            calculate_fr();

            // calculate attractive forces
            calculate_fa();

            // limit the total displacement by temperature
            limit_disp();

            equilibria = (cool_sch.t == 0);
        }

        protected int W, H;
        protected double k;

        /*
            Attractive/Repulsive force 
        */
        protected Force fa, fr;

        // cooling schedule
        protected linear_cooling cool_sch;

        protected class Config
        {
            public Vector pos;
            public Vector disp;

            public Config(double x, double y)
            {
                pos = new Vector(2);
                pos[0] = x; pos[1] = y;
                disp = new Vector(2);
                disp[0] = disp[1] = 0;
            }

            public double x
            {
                get { return pos[0]; }
                set { pos[0] = value; }
            }

            public double y
            {
                get { return pos[1]; }
                set { pos[1] = value; }
            }
        }

        protected Graph<Config> G;

        protected bool equilibria;
    }

    /*
        Cell - содержит множество элементов типа int 
    */
    public class Grid<Cell> where Cell : ISet<int>, new()
    {
        public Grid(int Rows, int Cols, int H, int W)
        {
            G = new List<List<Cell>>(Rows);
            for (int i = 0; i < Rows; i++)
            {
                G.Add(new List<Cell>(Cols));
                for (int j = 0; j < Cols; j++)
                    G[i].Add(new Cell());
            }

            this.Rows = Rows;
            this.Cols = Cols;

            cell_height = (double)H / Rows;
            cell_width = (double)W / Cols;
        }

        public int Row_cnt { get { return Rows; } }
        public int Col_cnt { get { return Cols; } }

        public List<Cell> this[int i]
        {
            get { return G[i]; }
        }

        public Cell get_cell(double x, double y)
        {
            int i, j;
            return get_cell(x, y, out i, out j);
        }

        public Cell get_cell(double x, double y, out int i, out int j)
        {
            j = (int)Math.Floor(x / cell_width);
            i = Rows - 1 - (int)Math.Floor(y / cell_height);

            return G[i][j];
        }

        public bool Remove(int key, double x, double y)
        {
            return get_cell(x, y).Remove(key);
        }

        public void Add(int key, double x, double y)
        {
            get_cell(x, y).Add(key);
        }


        private List<List<Cell>> G;
        private int Rows, Cols;
        private double cell_width, cell_height;
    }

    public class FR_grid : FR
    {
        public FR_grid(Graph<Object> _G, Force fattractive, Force frepulsive,
                    int _W, int _H, List<Vector> initial_config = null)
            : base(_G, fattractive, frepulsive, _W, _H, initial_config)
        {
            grid = new Grid<ST<int>>((int)Math.Ceiling(H / (k * 2.0)),
                                (int)Math.Ceiling(W / (k * 2.0)), H, W);
            neighbour = new int[8, 2] { { 1, 0 }, 
                                        { -1, 0 }, 
                                        { 0, 1 }, 
                                        { 0, -1 }, 
                                        { 1, 1 }, 
                                        { -1, -1 }, 
                                        { -1, 1 }, 
                                        { 1, -1 } };

            foreach (var v in G.vertices)
                grid.Add(v.v, v.data.x, v.data.y);
        }

        protected override void calculate_fr()
        {
            //base.calculate_fr();
            foreach (var v in G.vertices)
            {
                v.data.disp.assign(0);

                foreach (var u in neighbours(v))
                    if (u.v != v.v)
                    {
                        if (no_rep_force[v.v].Contains(u.v)) continue;

                        Vector d = Vector.sub(v.data.pos, u.data.pos);
                        double d_len = Vector.length(d);

                        if (2 * k - d_len < 0)
                        {
                            continue;
                        }

                        v.data.disp = Vector.add(v.data.disp,
                                                Vector.mult(Vector.div(d, d_len),
                                                            fr(d_len, k)
                                                            )
                                                 );
                    }
            }
        }

        protected override void limit_disp()
        {
            //base.limit_disp();
            foreach (var v in G.vertices)
            {
                double disp_len = Vector.length(v.data.disp);

                Vector old_pos = v.data.pos;

                v.data.pos = Vector.add(v.data.pos,
                                        Vector.mult(
                                                    Vector.div(v.data.disp, disp_len),
                                                    Math.Min(disp_len, cool_sch.t))
                                        );
                v.data.x = Math.Min(W - 1, Math.Max(0, v.data.x));
                v.data.y = Math.Min(H - 1, Math.Max(0, v.data.y));

                if (Vector.length(Vector.sub(v.data.pos, old_pos)) != 0)
                {
                    grid.Remove(v.v, old_pos[0], old_pos[1]);
                    grid.Add(v.v, v.data.x, v.data.y);
                }
            }

            cool_sch.cool();
        }

        private List<Vertex<Config>> neighbours(Vertex<Config> v)
        {
            List<Vertex<Config>> ns = new List<Vertex<Config>>();
            List<ST<int>> vs = new List<ST<int>>();
            int i, j, size = 0;

            vs.Add(grid.get_cell(v.data.x, v.data.y, out i, out j));
            size += vs[0].Count;

            for (int dir = 0; dir < 8; dir++)
            {
                int ni = i + neighbour[dir, 0],
                    nj = j + neighbour[dir, 1];

                if (ni >= 0 && ni < grid.Row_cnt && nj >= 0 && nj < grid.Col_cnt
                            && grid[ni][nj].Count > 0)
                {
                    vs.Add(grid[ni][nj]);
                    size += grid[ni][nj].Count;
                }
            }

            int[] v_ids = new int[size];
            int cpy_pos = 0;
            foreach (var cell in vs)
            {
                cell.CopyTo(v_ids, cpy_pos);
                cpy_pos += cell.Count;
            }

            foreach (var v_id in v_ids)
                ns.Add(G.vertices[v_id]);

            return ns;
        }

        private Grid<ST<int>> grid;
        private int[,] neighbour;
    }
}
