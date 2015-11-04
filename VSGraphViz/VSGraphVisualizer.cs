using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Graph;

namespace VSGraphViz
{
    public class VSGraphVisualizer
    {
        Expression root_expression;
        Graph<Object> graph;

        public VSGraphVisualizer()
        {
            root_expression = null;
            graph = null;
        }

        public void UpdateGraph(EnvDTE.Expression exp)
        {
            //VSGraphVizPackage.ToolWindowCtl.setText(log_write(exp.DataMembers));
            BuildGraph(exp);
            MakeVertexCaptions();
            UpdateGraphLayout();
            //TODO: update toolbox?
        }
        /*
        string log_write(Expressions e)
        {
            string ret = "";
            foreach (EnvDTE.Expression m in e)
            {
                ret += "(" + m.Type + ")" + m.Name + " = " + m.Value + "\n";
            }
            return ret;
        }*/

        void BuildGraph(Expression root_exp)
        {
            root_expression = root_exp;
            graph = new Graph<Object>();
            usedVertices = new SortedSet<string>();
            int root = graph.add(new ExpressionVertex(root_exp));
            BuildGraphRec(root_exp, root, 0);
        }

        SortedSet<string> usedVertices;
        void BuildGraphRec(Expression exp, int v, int rec_level)
        {
            usedVertices.Add(exp.Value);
            if (rec_level == VSGraphVizSettings.max_rec_depth)
                return;
            foreach (Expression e in exp.DataMembers)
            {
                if (e.Type == root_expression.Type)
                {
                    if (usedVertices.Contains(e.Value))
                        continue;
                    int to = graph.add(new ExpressionVertex(e));
                    graph.add(v, to);
                    BuildGraphRec(e, to, rec_level + 1);
                }
                else
                {
                    foreach(Expression field in e.DataMembers)
                    {
                        if (field.Type != root_expression.Type)
                            continue;
                        if (usedVertices.Contains(field.Value))
                            continue;
                        int to = graph.add(new ExpressionVertex(field));
                        graph.add(v, to);
                        BuildGraphRec(field, to, rec_level + 1);
                    }
                }
            }
        }

        void MakeVertexCaptions()
        {
            for (int i = 0; i < graph.V; i++)
            {
                ExpressionVertex v = graph.vertices[i].data as ExpressionVertex;
                v.name = "(" + v.exp.Type + ") " + v.exp.Name + " = " + v.exp.Value;
            }

            /*foreach (var v in graph.vertices)
            {
                v.data.name = "(" + v.data.exp.Type + ") " + v.data.exp.Name + " = " + v.data.exp.Value;
            }*/
        }

        public void UpdateGraphLayout()
        {
            //
            VSGraphVizPackage.ToolWindowCtl.show_graph(graph);
        }
    }
    public class ExpressionVertex
    {
        public Expression exp;
        public string name;
        public ExpressionVertex(Expression exp)
        {
            this.exp = exp;
        }
        public override string ToString()
        {
            return name;
        }
    }
}
