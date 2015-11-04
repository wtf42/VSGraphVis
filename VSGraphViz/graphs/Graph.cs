using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Graph
{
    public class Vertex<T>
    {
        public Vertex(int id, T data = default(T))
        {
            _v = id;
            _data = data;
        }

        public int v { get { return _v; } }

        public T data
        {
            get { return _data; }
            set { _data = value; }
        }

        private int _v;
        private T _data;
    }
    
    public class Edge<T>
    {
        public Edge(Vertex<T> from, Vertex<T> to, int weight = 1)
        {
            v = from;
            u = to;
            w = weight;
        }

        public Vertex<T> v, u;
        public int w;
    }
    
    public class Graph<T> : IEnumerable<Edge<T>>
    {
        public Graph(int V = 0, bool oriented = false)
        {
            Vcnt = 0;
            vertices = new List<Vertex<T>>();
            adj = new List<List<int>>();
            weight = new List<List<int>>();

            for (int i = 0; i < V; i++)
                add();

            Ecnt = 0;
            directed = oriented;
        }

        public int V { get { return Vcnt; } }

        public int E { get { return Ecnt; } }

        public bool Directed { get { return directed; } }

        public int add(T info = default(T))
        {
            int v = Vcnt;

            vertices.Add(new Vertex<T>(v, info));
            adj.Add(new List<int>());
            weight.Add(new List<int>());

            Vcnt++;
            return v;
        }

        public void add(Edge<T> e)
        {
            int from = e.v.v, to = e.u.v;

            bool cont = false;
            if (!adj[from].Contains(to))
            {
                adj[from].Add(to);
                weight[from].Add(e.w);
            }
            else cont = true;

            if (!directed && !cont)
            {
                adj[to].Add(from);
                weight[to].Add(e.w);
            }

            Ecnt++;
        }

        public void add(int u, int v, int w = 1)
        {
            add(new Edge<T>(vertices[u], vertices[v], w));
        }

        public bool remove(int u, int v, bool directed = false)
        {
            for (int i = 0; i < adj[u].Count; i++)
                if (adj[u][i] == v)
                {
                    adj[u].RemoveAt(i);
                    weight[u].RemoveAt(i);
                    if (!directed && !Directed)
                    {
                        Ecnt--;
                        remove(v, u, true);
                    }

                    if (Directed) Ecnt--;

                    return true;
                }

            return false;
        }

        public IEnumerable<int> adj_list(int v)
        {
            for (int u = 0; u < adj[v].Count; u++)
                yield return adj[v][u];
        }

        public IEnumerable<KeyValuePair<int, int>> adj_list_w(int v)
        {
            for (int u = 0; u < adj[v].Count; u++)
                yield return new KeyValuePair<int, int>(adj[v][u], weight[v][u]);
        }

        public IEnumerator<Edge<T>> GetEnumerator()
        {
            for (int v = 0; v < V; v++)
                for (int u = 0; u < adj[v].Count; u++)
                {
                    if (v > adj[v][u] && !Directed) continue;

                    yield return new Edge<T>(vertices[v], vertices[adj[v][u]],
                                             weight[v][u]);
                }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void set_data(int v, T data) { vertices[v].data = data; }
        public T get_data(int v) { return vertices[v].data; }

        public void contract(List<int> super_node)
        {
            bool[] outside = new bool[V];
            int[] w = new int[V];
            int p = super_node[0];

            foreach (var sub_node in super_node)
                outside[sub_node] = true;

            foreach (var sub_node in super_node)
            {
                List<int> rem = new List<int>();
                for (int i = 0; i < adj[sub_node].Count; i++)
                {
                    int u = adj[sub_node][i];
                    if (!outside[u])
                        w[u] += weight[sub_node][i];

                    rem.Add(u);
                }

                foreach (var u in rem)
                    remove(sub_node, u);
            }

            for (int i = 0; i < V; i++)
            {
                if (w[i] > 0)
                    add(i, p, w[i]);
            }
        }

        public static Graph<T> DeepClone(Graph<T> obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (Graph<T>)formatter.Deserialize(ms);
            }
        }

        private bool directed;
        private int Vcnt, Ecnt;
        public List<List<int>> adj;
        private List<List<int>> weight;

        public List<Vertex<T>> vertices;
    }
}
