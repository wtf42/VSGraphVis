﻿using System;
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
    using GraphAlgo;
    using FruchtermanReingold;
    using Radial;
    using GraphLayout;
    using HV;
    using SplayTree;
    using Vector;
    using System.Windows.Media.Animation;
    using System.Collections;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Shell;

    public partial class Control : UserControl
    {
        public Control()
        {
            InitializeComponent();

            selectionContainer = new Microsoft.VisualStudio.Shell.SelectionContainer();

            nextGraph = null;
            hasNextGraph = false;
            VSGraphVizPackage.expressionGraph.graphUpdated += graphUpdatedHandler;

            bc = new BrushConverter();

            showCompleted = true;
            animationCounter = 0;
            animationLock = new object();

            cur_alg = 1;

            grap_layout_algo = new List<GraphLayout>();
            grap_layout_algo.Add(new FRLayout());
            grap_layout_algo.Add(new RadialLayout());
            grap_layout_algo.Add(new RightHeavyHVLayout());

            graph_layout_algo_name = new List<string>();
            graph_layout_algo_name.Add("Fruchterman-Reingold");
            graph_layout_algo_name.Add("Radial");
            graph_layout_algo_name.Add("Right-Heavy HV");

            gen_menu();
        }
        
        void gen_menu()
        {
            foreach(var name in graph_layout_algo_name)
            {
                MenuItem it = new MenuItem();
                it.Header = name;
                it.Click += selectAlgorithm;

                algo_menu.Items.Add(it);
            }

            if (algo_menu.Items.Count > 0)
            {
                MenuItem it = algo_menu.Items[0] as MenuItem;
                it.IsChecked = true;
            }
        }

        Graph<object> nextGraph;
        bool hasNextGraph;
        private void graphUpdatedHandler(Graph<object> graph)
        {
            if (hold)
                return;

            nextGraph = graph;
            hasNextGraph = true;
            ShowNextGraph();
        }
        void ShowNextGraph()
        {
            if (animationCounter != 0 || !showCompleted)
                return;
            if (!hasNextGraph)
                return;
            Graph<object> gr = nextGraph;
            nextGraph = null;
            hasNextGraph = false;
            show_graph(gr, 0);
        }

        Microsoft.VisualStudio.Shell.SelectionContainer selectionContainer;
        ITrackSelection trackSelection;
        internal ITrackSelection TrackSelection
        {
            get
            {
                return trackSelection;
            }
            set
            {
                trackSelection = value;
                selectionContainer.SelectableObjects = null;
                selectionContainer.SelectedObjects = null;
                trackSelection.OnSelectChange(selectionContainer);
            }
        }
        public void UpdateSelection()
        {
            ITrackSelection track = TrackSelection;
            if (track != null)
                track.OnSelectChange(selectionContainer);
        }
        public void SelectList(ArrayList list)
        {
            selectionContainer = new Microsoft.VisualStudio.Shell.SelectionContainer(true, false);
            selectionContainer.SelectableObjects = list;
            selectionContainer.SelectedObjects = list;
            UpdateSelection();
        }

        public void OnSizeHandler()
        {
            graphUpdatedHandler(G);
            //VSGraphVizPackage.VSOutputLog("newW=" + this.ActualWidth);
            //VSGraphVizPackage.VSOutputLog("newH=" + this.ActualHeight);
        }

        private void chb_true(object sender, RoutedEventArgs e)
        {
            hold = true;
            menu_update(sender);
        }
        private void chb_false(object sender, RoutedEventArgs e)
        {
            hold = false;
            menu_update(sender);
        }

        private void menu_update(object sender)
        {
            MenuItem obj = sender as MenuItem;
            MenuItem p = obj.Parent as MenuItem;

            foreach (var ch in p.Items)
            {
                MenuItem sub = ch as MenuItem;
                sub.IsChecked = false;
            }

            obj.IsChecked = true;
        }

        private void selectAlgorithm(object sender, RoutedEventArgs e)
        {
            MenuItem obj = sender as MenuItem;
            MenuItem p = obj.Parent as MenuItem;

            int i = 0;
            int id = 0;
            foreach (var ch in p.Items)
            {
                MenuItem sub = ch as MenuItem;
                if (sub.Equals(obj)) id = i;
                i++;
            }
            id++;

            if (G == null)
                return;

            menu_update(sender);

            cur_alg = id;
            show_graph(G, 0);
        }

        private void AddEdge(int v, int u)
        {
            for (int i = edge.Count; i <= Math.Max(v, u) + 1; i++)
                edge.Add(new List<KeyValuePair<int, Line>>());

            Line ln = new Line();
            ln.StrokeThickness = VSGraphVizSettings.line_thickness;
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
            ve.StrokeThickness = VSGraphVizSettings.line_thickness;
            ve.Fill = (Brush)bc.ConvertFrom("#FFB7B7B7");

            vert[v].Cursor = Cursors.Hand;
            vert[v].Children.Add(ve);

            TextBlock tb = new TextBlock();
            tb.MaxWidth = VSGraphVizSettings.node_width;
            tb.MaxHeight = VSGraphVizSettings.node_height;
            tb.Text = G.vertices[v].data.ToString();
            tb.FontSize = VSGraphVizSettings.node_font_size;
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;

            vert[v].Children.Add(tb);

            ve.Height = VSGraphVizSettings.node_height;
            ve.Width = VSGraphVizSettings.node_width;
            vert_x = (int)ve.Width / 2;
            vert_y = (int)ve.Height / 2;

            ToolTip tt = new System.Windows.Controls.ToolTip();
            tt.Content = vert_info[v];
            tt.StaysOpen = true;
            vert[v].ToolTip = tt;
            ToolTipService.SetShowDuration(vert[v], Int32.MaxValue);

            vert[v].MouseRightButtonUp += Control_MouseRightButtonUp;
            vert[v].Tag = G.vertices[v].data;

            vert[v].MouseEnter += (o, e) => {
                Rectangle r = vert[v].Children[0] as Rectangle;
                r.StrokeThickness = VSGraphVizSettings.line_thickness + 1;
            };
            vert[v].MouseLeave += (o, e) => {
                Rectangle r = vert[v].Children[0] as Rectangle;
                r.StrokeThickness = VSGraphVizSettings.line_thickness;
            };

            vert[v].MouseLeftButtonDown += VertexMouseDown;
            vert[v].MouseLeftButtonUp += VertexMouseUp;

            front_canvas.Children.Add(vert[v]);
        }

        private void Control_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ExpressionVertex expv = (ExpressionVertex)(sender as Grid).Tag;
            ArrayList list = new ArrayList(expv.exp.DataMembers.OfType<EnvDTE.Expression>().ToList());
            SelectList(list);
        }

        private void InitVis(Graph<Object> G, Canvas front_canvas, int root = -1)
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
                if (i == root)
                {
                    e.Fill = (Brush)bc.ConvertFrom("#007ACC"); //
                    TextBlock t = vert[i].Children[1] as TextBlock;
                    t.Foreground = (Brush)bc.ConvertFrom("#FFFFFF");
                }
                else
                    e.Fill = (Brush)bc.ConvertFrom("#FFFFFF"); // #EF5555
            }

            if (new_vertices != null)
            {
                foreach (var i in new_vertices)
                {
                    Rectangle e = vert[i].Children[0] as Rectangle;
                    TextBlock t = vert[i].Children[1] as TextBlock;
                    if (i != root)
                    {
                        e.Fill = (Brush)bc.ConvertFrom("#3CB371");
                        t.Foreground = (Brush)bc.ConvertFrom("#FFFFFF");
                    }
                }
            }
            

            front_canvas.MouseMove += VertexMouseMove;
        }

        // Vertices moving 
        private void VertexMouseDown(object sender, MouseEventArgs e)
        {
            moving_obj = sender;
            UIElement src = (UIElement)sender;
            Grid g = src as Grid;
            Mouse.Capture(g.Children[1]);

            //Rectangle

            X_shape = Canvas.GetLeft(src);
            Y_shape = Canvas.GetTop(src);

            X = e.GetPosition(front_canvas).X;
            Y = e.GetPosition(front_canvas).Y;

            Canvas.SetZIndex(src, 1);
        }

        private void VertexMouseUp(object sender, MouseEventArgs e)
        {
            moving_obj = null;
            Mouse.Capture(null);
        }

        private void VertexMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && moving_obj != null
                && animationCounter == 0)
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

                if (initial_config != null && initial_config.Count > cur_vertex)
                {
                    initial_config[cur_vertex][0] = X;
                    initial_config[cur_vertex][1] = Y;
                }

                for (int i = 0; edge.Count > 0 && i < edge[cur_vertex].Count; i++)
                {
                    AnimateEdge(edge[cur_vertex][i].Value,
                                (int)new_X,
                                (int)new_Y,
                                0,
                                cur_vertex < edge[cur_vertex][i].Key);
                }
            }
        }

        // 
        private void ComputeXY(Graph<Object> G, int cur_alg)
        {
            AcyclicTest<Object> AT = new AcyclicTest<Object>(G);
            bool acyclic = AT.isAcyclic();

            if (cur_alg >= 2 && !acyclic)
            {
                // tree algorithm was chosen for inappropriate graph
                cur_alg = 1;
                ComputeXY(G, cur_alg);
                return;
            }

            List<Vector> config = new List<Vector>();

            this.xy = grap_layout_algo[cur_alg - 1].system_config(canvas_width(),
                                                        canvas_height(),
                                                        G,
                                                        out config,
                                                        -1, -1,
                                                        initial_config);


            if (config.Count > 0 /*&& xy.Count > 0*/)
            {
                initial_config = new List<Vector>();
                /*foreach (Vector v in xy[xy.Count - 1])
                    initial_config.Add(new Vector(v[0], v[1]));*/
                foreach (Vector v in config)
                    initial_config.Add(new Vector(v[0], v[1]));
            }
           
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
                AnimationCompletedHandler(s, e);
            };
            y_anim.Completed += AnimationCompletedHandler;
            lock (animationLock)
            {
                animationCounter += 2;
            }

            // incident edges animation
            for (int i = 0; edge.Count > 0 && i < edge[v_id].Count; i++)
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

        private void AnimationCompletedHandler(object sender, EventArgs e)
        {
            lock (animationLock)
            {
                --animationCounter;
            }
            ShowNextGraph();
        }

        private Graph<Object> InitGraph(Graph<Object> graph = null)
        {
            new_vertices = null;

            Random rnd = new Random();
            vert_info = new List<string>();
      
            for (int i = 0; i < graph.V; i++)
            {
                //vert_info.Add(graph.vertices[i].data.ToString());
                vert_info.Add((graph.vertices[i].data as ExpressionVertex).tooltip);
            }

            if (G == null)
            {
                initial_config = null;
                return graph;
            }

            int sim = 0;
            List<Vector> tmp_config = new List<Vector>();
            for (int i = 0; i < graph.V; i++)
            {
                int x = rnd.Next(1, canvas_width()), y = rnd.Next(1, canvas_height());  
                tmp_config.Add(new Vector(x, y));
            }

            List<bool> used = new List<bool>(graph.V);
            for (int i = 0; i < graph.V; i++)
                used.Add(false);

            for (int i = 0; i < G.V; i++)
            {
                for (int j = 0; j < graph.V; j++)
                {
                    if (G.vertices[i].data.ToString().Equals(graph.vertices[j].data.ToString()))
                    {
                        sim++;
                        tmp_config[j][0] = initial_config[i][0];
                        tmp_config[j][1] = initial_config[i][1];

                        used[j] = true;
                    }
                }
            }

            if (sim > 0)
            {
                new_vertices = new List<int>();
                for (int i = 0; i < graph.V; i++)
                    if (!used[i]) new_vertices.Add(i);

                if (new_vertices.Count == 0) new_vertices = null;
                    

                initial_config = tmp_config;
            }
            else
            {
                initial_config = null;
            }

            return graph;
        }

        public void show_graph(Graph<Object> graph, int root = -1)
        {
            if (graph == null)
            {
                G = null;
                front_canvas.Children.Clear();
                return;
            }

            showCompleted = false;

            G = InitGraph(graph);

            xy = new List<List<Vector>>();
            ComputeXY(G, cur_alg);
            InitVis(G, front_canvas, root);

            current = new int[G.V];

            for (int v = 0; v < G.V; v++)
            {
                if (edge.Count == 0) break;

                for (int j = 0, to; edge.Count > 0 && xy.Count > 0 && j < edge[v].Count; j++)
                {
                    to = edge[v][j].Key;
                    if (v > to) continue;

                    edge[v][j].Value.X1 = xy[0][v][0] + vert_x;
                    edge[v][j].Value.Y1 = xy[0][v][1] + vert_y;
                    edge[v][j].Value.X2 = xy[0][to][0] + vert_x;
                    edge[v][j].Value.Y2 = xy[0][to][1] + vert_y;
                }
            }


            for (int i = 0; xy.Count > 0 && i < G.V; i++)
            {
                Canvas.SetLeft(vert[i], 0);
                Canvas.SetTop(vert[i], 0);

                AnimateVertex(i, (int)xy[0][i][0], (int)xy[0][i][1]);
            }

            showCompleted = true;
            ShowNextGraph();
        }

        private int canvas_width()
        {
            return Math.Max((int)front_canvas.ActualWidth - VSGraphVizSettings.node_width, 0);
        }

        private int canvas_height()
        {
            return Math.Max((int)front_canvas.ActualHeight - VSGraphVizSettings.node_height, 0);
        }

        List<GraphLayout> grap_layout_algo;
        List<String> graph_layout_algo_name;

        Graph<Object> G;

        BrushConverter bc;

        int vert_x;
        int vert_y;
        private List<List<KeyValuePair<int, Line>>> edge;
        private List<Grid> vert;
        private List<List<Vector>> xy;
        private int[] current;
        private List<String> vert_info;

        private List<Vector> initial_config;
        private List<int> new_vertices;

        private int cur_alg;

        bool showCompleted;
        int animationCounter;
        object animationLock;
        bool hold;
        double X, Y;
        double X_shape, Y_shape;
        object moving_obj;
        int cur_vertex;
    }
}