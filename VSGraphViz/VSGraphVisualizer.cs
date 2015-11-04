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
            BuildGraph(exp);
            MakeVertexCaptions();
            UpdateGraphLayout();
        }

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
                    foreach (Expression field in m.DataMembers)
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
            bool found = false;
            foreach (Expression field in root_expression.DataMembers)
            {
                if (field.Type == root_expression.Type)
                    continue;
                if (field.DataMembers.OfType<Expression>().
                        Where(e => e.Type == root_expression.Type).Any())
                    continue;
                if (graph.vertices.OfType<Vertex<object>>().
                    Select(v => v.data as ExpressionVertex).
                    Select(expv => expv.exp.DataMembers.OfType<Expression>()).
                    Select(m => m.FirstOrDefault(f => f.Name == field.Name)).
                    Select(f => f.Value).
                    Distinct().Count() == graph.vertices.Count)
                {
                    found = true;
                    foreach (var gr_vert in graph.vertices)
                    {
                        ExpressionVertex vert = gr_vert.data as ExpressionVertex;
                        Expression exp = vert.exp;
                        vert.name = exp.Value + "\n{ ERROR }";
                        foreach (Expression f in exp.DataMembers)
                        {
                            if (f.Name == field.Name)
                            {
                                vert.name = exp.Value + "\n" + "{ " + f.Name + " = " + f.Value + " }";
                                break;
                            }
                        }
                    }
                    break;
                }
            }
            if (!found)
            {
                foreach (var gr_vert in graph.vertices)
                {
                    ExpressionVertex vert = gr_vert.data as ExpressionVertex;
                    vert.name = vert.exp.Value;
                }
            }
        }

        public void UpdateGraphLayout()
        {
            VSGraphVizPackage.ToolWindowCtl.show_graph(graph, 0);
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
