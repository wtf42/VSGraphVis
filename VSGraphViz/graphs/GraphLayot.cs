using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLayout
{
    using Graph;
    using Vector;

    public interface GraphLayout
    {
        List<List<Vector>> system_config(int W, int H, 
                                         Graph<Object> G, 
                                         int root = -1, int p_root = -1, 
                                         List<Vector> initial_config = null);
    }
}
