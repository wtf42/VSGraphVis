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
        public Expression root_expression;
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

            usedVertices = new SortedDictionary<string, int>();
            int root = graph.add(new ExpressionVertex(root_exp));
            BuildGraphRec(root_exp, root, 0);
        }
        
        SortedDictionary<string, int> usedVertices;
        void BuildGraphRec(Expression exp, int v, int rec_level)
        {
            usedVertices.Add(exp.Value, v);
            if (rec_level == VSGraphVizSettings.max_rec_depth)
                return;
            foreach (Expression m in exp.DataMembers)
            {
                if (m.Type == root_expression.Type)
                {
                    addVertexRec(v, m, rec_level);
                }
                else
                {
                    foreach(Expression field in m.DataMembers)
                    {
                        if (field.Type == root_expression.Type)
                            addVertexRec(v, field, rec_level);
                    }
                }
            }
        }
        void addVertexRec(int par, Expression exp, int rec_level)
        {
            if (!(isValidVertex(exp)))
                return;
            if (!usedVertices.ContainsKey(exp.Value))
            {
                int to = graph.add(new ExpressionVertex(exp));
                BuildGraphRec(exp, to, rec_level + 1);
            }
            graph.add(par, usedVertices[exp.Value]);
        }

        bool isValidVertex(Expression exp)
        {
            foreach (Expression e in exp.DataMembers)
            {
                if (!e.IsValidValue)
                    return false;
            }
            return true;
        }

        void MakeVertexCaptions()
        {
            for (int i = 0; i < graph.V; i++)
            {
                ExpressionVertex v = graph.vertices[i].data as ExpressionVertex;
                v.name = /*"(" + v.exp.Type + ") " + v.exp.Name + " = " + */v.exp.Value;
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
