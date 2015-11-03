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
