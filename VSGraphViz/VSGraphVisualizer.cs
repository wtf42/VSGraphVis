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
        public VSGraphVisualizer()
        {
            root_expression = null;
        }
        public void UpdateGraph(EnvDTE.Expression exp)
        {
            //VSGraphVizPackage.ToolWindowCtl.setText(log_write(exp.DataMembers));
            BuildGraph(exp);
            MakeVertexCaptions();
        }
        Expression root_expression;
        Graph<ExpressionVertex> graph;

        void BuildGraph(Expression root_exp)
        {
            root_expression = root_exp;
            graph = new Graph<ExpressionVertex>();
            int root = graph.add(new ExpressionVertex(root_exp));
            BuildGraphRec(root_exp, root, 0);
        }
        void BuildGraphRec(Expression exp, int v, int rec_level)
        {
            if (rec_level == VSGraphVizSettings.max_rec_depth)
                return;
            foreach (Expression e in exp.DataMembers)
            {
                if (e.Type != root_expression.Type)
                    continue;
                int to = graph.add(new ExpressionVertex(e));
                BuildGraphRec(e, to, rec_level + 1);
            }
        }
        void MakeVertexCaptions()
        {
            foreach(var v in graph.vertices)
            {
                v.data.name = "(" + v.data.exp.Type + ") " + v.data.exp.Name + " = " + v.data.exp.Value;
            }
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
