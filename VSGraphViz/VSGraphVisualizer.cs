using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace VSGraphViz
{
    public class VSGraphVisualizer
    {
        public VSGraphVisualizer()
        {
            //
        }
        public void UpdateGraph(EnvDTE.Expression exp)
        {
            VSGraphVizPackage.ToolWindowCtl.setText(log_write(exp.DataMembers));
            //
        }
        string log_write(Expressions e)
        {
            string ret = "";
            foreach (EnvDTE.Expression m in e)
            {
                ret += "(" + m.Type + ")" + m.Name + " = " + m.Value + "\n";
            }
            return ret;
        }
    }
}
