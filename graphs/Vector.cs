using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vector
{
    public class Vector
    {
        private double[] v;
        private int _dim;

        public Vector(int size) { _dim = size; v = new double[size]; }

        public Vector(params int[] val)
        {
            _dim = val.Length; v = new double[_dim];
            for (int i = 0; i < _dim; i++)
                v[i] = val[i];
        }

        public Vector(params double[] val)
        {
            _dim = val.Length; v = new double[_dim];
            for (int i = 0; i < _dim; i++)
                v[i] = val[i];
        }

        public void assign(int val) { for (int i = 0; i < dim; i++) v[i] = val; }

        public int dim { get { return _dim; } }

        public double this[int i]
        {
            get { return v[i]; }
            set { v[i] = value; }
        }

        /*
            Required: dimensions of vectors should be equal 
        */
        public static Vector sub(Vector a, Vector b)
        {
            Vector res = new Vector(a.dim);
            for (int i = 0; i < a.dim; i++)
                res[i] = a[i] - b[i];
            return res;
        }

        public static Vector add(Vector a, Vector b)
        {
            Vector res = new Vector(a.dim);
            for (int i = 0; i < a.dim; i++)
                res[i] = a[i] + b[i];
            return res;
        }

        public static double length(Vector a)
        {
            double res = 0;
            for (int i = 0; i < a.dim; i++)
                res += a[i] * a[i];
            return Math.Sqrt(res);
        }

        /*
            mult, div by single number 
        */
        public static Vector mult(Vector v, double num)
        {
            Vector res = new Vector(v.dim);
            for (int i = 0; i < v.dim; i++)
                res[i] = v[i] * num;
            return res;
        }

        public static Vector div(Vector v, double num)
        {
            Vector res = new Vector(v.dim);
            for (int i = 0; i < v.dim; i++)
                res[i] = (num != 0) ? (v[i] / num) : 0;
            return res;
        }
    }
}
