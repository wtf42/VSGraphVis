using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace VSGraphViz
{
    using Graph;
    using FruchtermanReingold;
    using Radial;
    using HV;
    using SplayTree;
    using Vector;
    using System.Windows.Media.Animation;
    using System.IO;

    public partial class Control : UserControl
    {
        public Control()
        {
            InitializeComponent();

            bc = new BrushConverter();
            // = 13;

            //MainWindow.Background = (Brush)bc.ConvertFrom("#FFF1F1F1");

            show = false;
            animation_complete = false;
        }
        public void setText(string text)
        {
            tb1.Text = text;
        }
        public double getW()
        {
            return Width;
        }
        public double getH()
        {
            return Height;
        }


        private void AddEdge(int v, int u)
        {
            for (int i = edge.Count; i <= Math.Max(v, u) + 1; i++)
                edge.Add(new List<KeyValuePair<int, Line>>());

            Line ln = new Line();
            ln.StrokeThickness = 2;
            ln.Stroke = (Brush)bc.ConvertFrom("#FF636363");

            edge[v].Add(new KeyValuePair<int, Line>(u, ln));
            edge[u].Add(new KeyValuePair<int, Line>(v, ln));

            front_canvas.Children.Add(ln);
        }

        private void AddVertex(int v)
        {
            for (int i = vert.Count; i <= v + 1; i++)
                vert.Add(new Grid());

            Rectangle ve = new Rectangle();
            ve.Stroke = (Brush)bc.ConvertFrom("#FF636363");
            ve.StrokeThickness = 2;
            ve.Fill = (Brush)bc.ConvertFrom("#FFB7B7B7");
            
            ve.Cursor = Cursors.Hand;
            vert[v].Children.Add(ve);

            TextBlock tb = new TextBlock();
            tb.Text = G.vertices[v].data.ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;

            vert[v].Children.Add(tb);

            ve.Height = 25;
            ve.Width = 70;
            vert_x = (int)ve.Width / 2;
            vert_y = (int)ve.Height / 2;

            ToolTip tt = new System.Windows.Controls.ToolTip();
            tt.Content = vert_info[v];
            ve.ToolTip = tt;

            //ve.MouseEnter += (o, e) => { ve.StrokeThickness = 3; };
            //ve.MouseLeave += (o, e) => { ve.StrokeThickness = 2; };

            //vert[v].MouseLeftButtonDown += VertexMouseDown;
            //vert[v].MouseLeftButtonUp += VertexMouseUp;

            front_canvas.Children.Add(vert[v]);
        }

        private void InitVis(Graph<Object> G, Canvas front_canvas)
        {
            front_canvas.Children.Clear();
            edge = new List<List<KeyValuePair<int, Line>>>();

            vert = new List<Grid>(G.V);

            foreach (var e in G)
                AddEdge(e.v.v, e.u.v);

            
            for (int i = 0; i < G.V; i++)
            {
                AddVertex(i);
                Rectangle e = vert[i].Children[0] as Rectangle;
                e.Fill = (Brush)bc.ConvertFrom("#FFFFFF"); // #EF5555
            }

            front_canvas.MouseMove += VertexMouseMove;
        }

        // Vertices moving 
        private void VertexMouseDown(object sender, MouseEventArgs e)
        {
            moving_obj = sender;

            Grid tmp = sender as Grid;
            Mouse.Capture(tmp);
            X_shape = Canvas.GetLeft(tmp);
            Y_shape = Canvas.GetTop(tmp);

            X = e.GetPosition((UIElement)tmp.Parent).X;
            Y = e.GetPosition((UIElement)tmp.Parent).Y;

            Canvas.SetZIndex(tmp, 1);
        }

        private void VertexMouseUp(object sender, MouseEventArgs e)
        {
            moving_obj = null;
            Mouse.Capture(null);
        }

        private void VertexMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && moving_obj != null
                && animation_complete)
            {
                Grid tmp = moving_obj as Grid;

                double new_X = e.GetPosition((UIElement)tmp.Parent).X;
                X_shape += new_X - X;
                Canvas.SetLeft(tmp, X_shape);
                double new_Y = e.GetPosition((UIElement)tmp.Parent).Y;
                Y_shape += new_Y - Y;
                Canvas.SetTop(tmp, Y_shape);

                X = new_X;
                Y = new_Y;

                // GRID ?
                for (int i = 0; i < vert.Count; i++)
                    if (vert[i] == tmp) cur_vertex = i;

                for (int i = 0; i < edge[cur_vertex].Count; i++)
                {
                    AnimateEdge(edge[cur_vertex][i].Value,
                                (int)new_X,
                                (int)new_Y,
                                0,
                                cur_vertex < edge[cur_vertex][i].Key);
                }
            }
        }

        // Fruchterman Reingold Algorithm
        private void ComputeXY(Graph<Object> G)
        {


            List<int> dv = new List<int>();



            int dummy = G.V;


            FR_grid fr_layout = new FR_grid(G, square_distance_attractive_force.f,
                                     square_distance_repulsive_force.f,
                                     (int)front_canvas.ActualWidth, (int)front_canvas.ActualHeight);


            bool equilibria = false;
            List<Vector> xy = new List<Vector>();
            int iter = 0;
            while (!equilibria)
            {
                xy = fr_layout.system_config(out equilibria);

                if (iter % 100 == 0)
                {
                    this.xy.Add(xy);
                }

                iter++;
            }

            /*RightHeavyHV r = new RightHeavyHV(G, (int)front_canvas.ActualWidth, (int)front_canvas.ActualHeight);
            List<Vector> xy = r.system_config();
            this.xy.Add(xy);*/
        }

        // Vertices and Edges Animation
        private void AnimateVertex(int v_id, int x, int y)
        {
            // x
            DoubleAnimation x_anim = new DoubleAnimation();
            x_anim.From = current[v_id] == 0 ? xy[current[v_id]][v_id][0] : xy[current[v_id] - 1][v_id][0];
            x_anim.To = x;

            // y
            DoubleAnimation y_anim = new DoubleAnimation();
            y_anim.From = current[v_id] == 0 ? xy[current[v_id]][v_id][1] : xy[current[v_id] - 1][v_id][1];
            y_anim.To = y;

            double duration = Math.Sqrt(Math.Pow((double)x_anim.From - x, 2) +
                                        Math.Pow((double)y_anim.From - y, 2)) + 250;

            x_anim.Duration = new Duration(TimeSpan.FromMilliseconds(duration));
            y_anim.Duration = new Duration(TimeSpan.FromMilliseconds(duration));

            TranslateTransform tt = new TranslateTransform();
            vert[v_id].RenderTransform = tt;

            x_anim.Completed += (s, e) =>
            {
                int step = ++current[v_id];
                if (step < xy.Count)
                {
                    AnimateVertex(v_id, (int)xy[step][v_id][0], (int)xy[step][v_id][1]);
                }
                else
                {
                    animation_complete = true;
                }
            };

            // incident edges animation
            for (int i = 0; i < edge[v_id].Count; i++)
            {

                AnimateEdge(edge[v_id][i].Value, (int)x_anim.To + vert_x,
                                                 (int)y_anim.To + vert_y,
                                                 (int)duration,
                                                 v_id < edge[v_id][i].Key);
            }

            tt.BeginAnimation(TranslateTransform.XProperty, x_anim);
            tt.BeginAnimation(TranslateTransform.YProperty, y_anim);
        }

        private void AnimateEdge(Line e, int to_x, int to_y, int duration, bool x1y1)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation da, da1;

            if (x1y1)
            {
                da = new DoubleAnimation(e.Y1, to_y, new Duration(new TimeSpan(0, 0, 0, 0, (int)duration)));

                da1 = new DoubleAnimation(e.X1, to_x, new Duration(new TimeSpan(0, 0, 0, 0, (int)duration)));

                Storyboard.SetTargetProperty(da, new PropertyPath("(Line.Y1)"));
                Storyboard.SetTargetProperty(da1, new PropertyPath("(Line.X1)"));
            }
            else
            {
                da = new DoubleAnimation(e.Y2, to_y, new Duration(new TimeSpan(0, 0, 0, 0, (int)duration)));

                da1 = new DoubleAnimation(e.X2, to_x, new Duration(new TimeSpan(0, 0, 0, 0, (int)duration)));

                Storyboard.SetTargetProperty(da, new PropertyPath("(Line.Y2)"));
                Storyboard.SetTargetProperty(da1, new PropertyPath("(Line.X2)"));
            }

            sb.Children.Add(da);
            sb.Children.Add(da1);

            e.BeginStoryboard(sb);
        }

        

        private Graph<Object> InitGraph(Graph<Object> graph = null)
        {
            Graph<Object> G;
            Random rnd = new Random();
            vert_info = new List<string>();

            int v_idx = 0;

            /*if (graph == null)
            {

                using (TextReader reader = File.OpenText("input.txt"))
                {
                    int n, m;
                    string text = reader.ReadLine();
                    string[] bits = text.Split(' ');
                    n = int.Parse(bits[0]);
                    m = int.Parse(bits[1]);
                    v_idx = n;

                    G = new Graph<Object>(n);
                    for (int i = 0; i < n; i++)
                    {
                        G.vertices[i].data = i + 1;
                        vert_info.Add(G.vertices[i].data.ToString());
                    }

                    for (int i = 0; i < m; i++)
                    {
                        text = reader.ReadLine();
                        bits = text.Split(' ');
                        int a = int.Parse(bits[0]);
                        int b = int.Parse(bits[1]);
                        a--; b--;

                        G.add(a, b);
                    }
                }
            }
            else
            {*/
                G = graph;
                v_idx = G.V;
                for (int i = 0; i < G.V; i++)
                {
                    vert_info.Add(G.vertices[i].data.ToString());
                }
            //}

            return G;
        }

        public void show_graph(Graph<Object> graph)
        {
            if (!show)
            {
                G = InitGraph(graph);

                xy = new List<List<Vector>>();

                ComputeXY(G);
                InitVis(G, front_canvas);

                current = new int[G.V];

                for (int v = 0; v < G.V; v++)
                {
                    for (int j = 0, to; j < edge[v].Count; j++)
                    {
                        to = edge[v][j].Key;
                        if (v > to) continue;

                        edge[v][j].Value.X1 = xy[0][v][0] + vert_x;
                        edge[v][j].Value.Y1 = xy[0][v][1] + vert_y;
                        edge[v][j].Value.X2 = xy[0][to][0] + vert_x;
                        edge[v][j].Value.Y2 = xy[0][to][1] + vert_y;
                    }
                }


                for (int i = 0; i < G.V; i++)
                {
                    Canvas.SetLeft(vert[i], 0);
                    Canvas.SetTop(vert[i], 0);

                    AnimateVertex(i, (int)xy[0][i][0], (int)xy[0][i][1]);
                }
            }
            //show = true;
        }


        Graph<Object> G;

        BrushConverter bc;

        int vert_x;
        int vert_y;
        bool show;
        private List<List<KeyValuePair<int, Line>>> edge;
        private List<Grid> vert;
        private List<List<Vector>> xy;
        private int[] current;
        private List<String> vert_info;

        bool animation_complete;

        double X, Y;
        double X_shape, Y_shape;
        object moving_obj;
        int cur_vertex;
    }
}