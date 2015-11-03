# VSGraphVis
HOW TO USE FR Algorithm:

    FR_grid fr_layout = new FR_grid(G, square_distance_attractive_force.f, 
                                     square_distance_repulsive_force.f, 
                                     W, H);
                                     
where:

  G - input graph

  W - width of layout

  H - height of layout

RUN ALGORITHM:

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


HOW TO USE RightHeavyHV:

	RightHeavyHV hv = new RightHeavyHV(G, W, H);
        List<Vector> xy = hv.system_config();		// you can specify root node

HOW TO USE Radial:

	Radial r = new Radial(G, 100, 100);
        List<Vector> xy = r.system_config();		// you can specify root node
        
        
DRAWING EXAMPLE:

	Graph<Object> G = new Graph<Object>(N);
	// ...
	RightHeavyHV hv = new RightHeavyHV(G, W, H);
        List<Vector> xy = hv.system_config()
        for (int from = 0; from < G.V; from++) 
        {
        	foreach(var to : G.adj_list(from))
        		draw_line(xy[from], xy[to]); // or draw_line(xy[from][0], xy[from][1], xy[to][0], xy[to][1]);
	}
