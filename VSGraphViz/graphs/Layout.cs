using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Layout
{
    using Vector;

    public class Layout
    {
        public Layout(int W, int H)
        {
            this.W = W;
            this.H = H;

            comp = new Rect(0, 0, W, H);
        }

        public void getRect(ref List<Vector> X, ref Vector lt, ref Vector rb)
        {
            lt = new Vector(X[0][0], X[0][1]);
            rb = new Vector(X[0][0], X[0][1]);
            for (int i = 1; i < X.Count; i++)
            {
                Vector tmp = new Vector(X[i][0], X[i][1]);

                if (tmp[0] < lt[0]) lt[0] = tmp[0];
                if (tmp[0] > rb[0]) rb[0] = tmp[0];
                if (tmp[1] < rb[1]) rb[1] = tmp[1];
                if (tmp[1] > lt[1]) lt[1] = tmp[1];
            }

            if (rb[0] - lt[0] == 0) rb[0]++;
            if (lt[1] - rb[1] == 0) lt[1]++;

            double P = (rb[0] - lt[0]) / (lt[1] - rb[1]);
            if (P <= 1)
            {
                marginX = (comp.width() - P * comp.height()) / 2 + 40;
            }
            else
            {
                marginX = comp.width() / 10;
            }
            marginY = (marginX - (comp.width() - (comp.height() - 40) * P) / 2) / P;

        }

        public Vector getCoord(Vector p, Vector lt, Vector rb, bool rev = false)
        {
            
            double dx = Math.Abs(rb[0] - lt[0]) / (comp.width() - 2 * marginX),
                    dy = Math.Abs(rb[1] - lt[1]) / (comp.height() - 2 * marginY);

            Vector res = new Vector(2);
            res[0] = ((p[0] - lt[0]) / dx + marginX + comp.x);
            if (rev) res[1] = (double)H - (comp.height() + (rb[1] - p[1]) / dy - marginY + comp.y);
            else     res[1] = (comp.height() + (rb[1] - p[1]) / dy - marginY + comp.y);

            return res;
        }

        class Rect
        {
            public Rect(double x, double y, double width, double height)
            {
                this.x = x;
                this.y = y;
                this.h = height;
                this.w = width;
            }

            public double width() { return w; }
            public double height() { return h; }

            public double x, y, h, w;
        }

        Rect comp;

        int W, H;
        public double marginX, marginY;
    }

}
