#include <iostream>
#include <vector>
#include <list>

//using namespace std;
using std::cin;
using std::cout;
using std::vector;

int counter = 0;

struct tree
{
	int key;
	tree *l;
	tree *r;
};

tree *full_tree(int height)
{
	if (height == 0)
		return NULL;
	return new tree{++counter,
					full_tree(height - 1),
					full_tree(height - 1) };
}

struct list
{
	int key;
	tree *t;
	list *prev;
	list *next;
};

void build_list1()
{
	list *l = new list{ ++counter, NULL, NULL };
	for (int i = 0; i < 10; ++i)
	{
		list *nxt = new list{ ++counter, NULL, l };
		l = nxt;
	}
	l = l;
}

void build_list2()
{
	std::list<tree*> lst;
	for (int i = 0; i < 10; ++i) {
	tree *t = full_tree(rand() % 7);
	lst.insert(lst.begin(), t);
	}
	lst = lst;
}

struct xtree
{
	int key;
	xtree *par;
	vector<xtree*> childs;
};

void build_list3()
{
	vector<xtree*> all;
	xtree *root = new xtree{ ++counter, NULL };
	all.push_back(root);
	for (int i = 1; i < 25; ++i)
	{
		int par = rand() % i;
		xtree *cur = new xtree{ ++counter, all[par] };
		all.push_back(cur);
		all[par]->childs.push_back(cur);
		if (i % 3 == 0) {
			cur = cur;
		}
	}
	all = all;
}

struct node
{
	int key;
	vector<node*> adj;

	node(int key = 0) : key(key) { }
};

int main()
{
	build_list1();
	build_list2();
	build_list3();

	freopen("input.txt", "r", stdin);
	int n, m;
	cin >> n >> m;

	vector<node*> v(n);
	for (int i = 0; i < n; i++)
		v[i] = new node(i + 1);
	for (int i = 0; i < m; i++)
	{
		int a, b; cin >> a >> b;
		a--, b--;
		v[a]->adj.push_back(v[b]);
		v[b]->adj.push_back(v[a]);
	}

	node*r = v[0];

	node* a = new node(n + 1);
	node* b = new node(n + 2);

	r->adj.push_back(a);
	r->adj.push_back(b);

	r = v[0];

	return 0;
}
