using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SplayTree
{
    public class Node<T> : IEnumerable<T> where T : IComparable<T>
    {
        public Node<T> p;
        public Node<T> left;
        public Node<T> right;

        public T data;

        public Node(T _d = default(T),
            Node<T> _p = null, Node<T> _l = null, Node<T> _r = null)
        {
            data = _d;
            p = _p;
            left = _l; right = _r;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (left != null)
            {
                foreach (var child in left)
                    yield return child;
            }

            yield return data;

            if (right != null)
            {
                foreach (var child in right)
                    yield return child;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ST<T> : ISet<T>, ICollection<T> where T : IComparable<T>
    {
        public ST() { root = null; cnt = 0; }

        public bool Add(T key)
        {
            Node<T> l = new Node<T>(), r = new Node<T>();
            split(key, ref l, ref r);
            root = new Node<T>(key, null, l, r);

            keep_parent(root);

            cnt++;
            return true;
        }

        void ICollection<T>.Add(T key)
        {
            this.Add(key);
        }

        public bool Remove(T key)
        {
            root = find(root, key);
            if (!root.data.Equals(key)) return false;

            set_parent(root.left, null);
            set_parent(root.right, null);

            root = merge(ref root.left, ref root.right);

            cnt--;
            return true;
        }

        public bool Contains(T key)
        {
            Node<T> s = find(root, key);
            if (s != null) return s.data.Equals(key);
            else return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in root)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get { return cnt; } }
        public bool IsReadOnly { get { return false; } }
        public void Clear() { root = null; }
        public void CopyTo(T[] array, int begin_index)
        {
            foreach (var item in root)
                array[begin_index++] = item;
        }

        /*
         * TODO: implement
        */
        public void IntersectWith(IEnumerable<T> other) { }
        public void UnionWith(IEnumerable<T> other) { }
        public void ExceptWith(IEnumerable<T> other) { }
        public void SymmetricExceptWith(IEnumerable<T> other) { }
        public bool IsSubsetOf(IEnumerable<T> other) { return false; }
        public bool IsSupersetOf(IEnumerable<T> other) { return false; }
        public bool IsProperSupersetOf(IEnumerable<T> other) { return false; }
        public bool IsProperSubsetOf(IEnumerable<T> other) { return false; }
        public bool SetEquals(IEnumerable<T> other) { return false; }
        public bool Overlaps(IEnumerable<T> other) { return false; }
        /*
         * Splay Tree methods 
        */
        private Node<T> find(Node<T> v, T key)
        {
            if (v == null) return null;

            if (key.CompareTo(v.data) == 0) return splay(v);
            else if (v.left != null && key.CompareTo(v.data) < 0) return find(v.left, key);
            else if (v.right != null && key.CompareTo(v.data) > 0) return find(v.right, key);

            return splay(v);
        }

        private void rotate(Node<T> c, Node<T> p)
        {
            Node<T> gp = p.p;
            if (gp != null)
            {
                if (p == gp.left) gp.left = c;
                else gp.right = c;
            }

            if (c == p.left) { p.left = c.right; c.right = p; }
            else { p.right = c.left; c.left = p; }

            keep_parent(p);
            keep_parent(c);

            c.p = gp;
        }

        private Node<T> splay(Node<T> v)
        {
            if (v.p == null) return v;

            Node<T> p = v.p;
            Node<T> gp = p.p;

            if (gp == null)
            {
                rotate(v, p);
                return v;
            }
            else
            {
                bool zig_zig = (p == gp.left && v == p.left);
                if (zig_zig)
                {
                    rotate(p, gp);
                    rotate(v, p);
                }
                else
                {
                    rotate(v, p);
                    rotate(v, gp);
                }

                return splay(v);
            }
        }

        private void split(T key, ref Node<T> l, ref Node<T> r)
        {
            if (root == null)
            {
                l = null; r = null;
                return;
            }

            root = find(root, key);

            if (key.CompareTo(root.data) == 0)
            {
                l = root.left; r = root.right;
                set_parent(l, null); set_parent(r, null);
            }
            else if (key.CompareTo(root.data) > 0)
            {
                l = root;
                r = root.right;
                l.right = null;
                set_parent(r, null);
            }
            else
            {
                r = root;
                l = root.left;
                r.left = null;
                set_parent(l, null);
            }
        }

        private Node<T> merge(ref Node<T> l, ref Node<T> r)
        {
            if (l == null) return r;
            if (r == null) return l;

            r = find(r, l.data);
            r.left = l;
            set_parent(l, r);

            return r;
        }

        private void set_parent(Node<T> c, Node<T> p)
        {
            if (c != null) c.p = p;
        }

        private void keep_parent(Node<T> v)
        {
            set_parent(v.left, v);
            set_parent(v.right, v);
        }

        private Node<T> root;
        private int cnt;
    }
}