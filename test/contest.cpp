// contest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <fstream>
#include <iostream>
#include <vector>
#include <cassert>
using namespace std;

#define INF (int)1e9

/*int n, m;
int w[200][200];
int c[1000];

int d[1000][3];
int _ans[1000][3];
int pos[1000][3][3];


inline void init(int i)
{
	d[0][i] = w[i][c[0]];
	pos[0][i][i] = c[0];
}

bool test(int i, int k, int p)
{
	bool f = true;
	for (int j = 0; j < 3; j++)
	{
		if (pos[i - 1][k][j] == c[i] && j != p) f = false;
	}
	return f;
}

void try_relax(int i, int p)
{
	d[i][p] = INF;
	for (int k = 0; k < 3; k++)
		if ((d[i][p] > d[i - 1][k] + w[pos[i - 1][k][p]][c[i]]) && (pos[i - 1][k][p] == c[i] || test(i, k, p)))
		{
			d[i][p] = d[i - 1][k] + w[pos[i - 1][k][p]][c[i]];

			_ans[i][p] = k;

			for (int j = 0; j < 3; j++)
				pos[i][p][j] = pos[i - 1][k][j];
			pos[i][p][p] = c[i];
		}
}

int dp()
{
	for (int i = 0; i < 3; ++i)
	{
		for (int j = 0; j < 3; ++j)
			pos[0][i][j] = j;

		init(i);
	}

	for (int i = 1; i < m; i++)
	{
		for (int p = 0; p < 3; p++)
			try_relax(i, p);

		int tmp;
	}

	int k = 0;
	for (int p = 0; p < 3; p++)
		if (d[m - 1][k] > d[m - 1][p]) k = p;

	return d[m - 1][k];
}

int ans[1000];

void build()
{
	int k = 0;
	for (int p = 0; p < 3; p++)
		if (d[m - 1][k] > d[m - 1][p]) k = p;

	for (int i = m - 1; i >= 0; i--)
	{
		ans[i] = k + 1;
		k = _ans[i][k];
	}
}*/


struct node
{
	int key;
	vector<node*> adj;

	node(int key = 0) :key(key) {}
};

node* get_next_graph(fstream &cin)
{
	int n, m; cin >> n >> m;

	vector<node*> v(n);
	for (int i = 0; i < n; i++)
		v[i] = new node(i + 1);

	node* r = v[0];
	for (int i = 0; i < m; i++)
	{
		int a, b; cin >> a >> b;
		a--, b--;
		v[a]->adj.push_back(v[b]);
		v[b]->adj.push_back(v[a]);
	}

	return r;
}

int main()
{
	//ios::sync_with_stdio(false);
	/*fstream cin("input.txt");
	cin >> n >> m;
	for (int i = 0; i < n; ++i)
		for (int j = 0; j < n; ++j)
			cin >> w[i][j];

	for (int i = 0; i < m; ++i)
		cin >> c[i], c[i]--;

	cout << dp() << endl;
	build();
	for (int i = 0; i < m; i++)
		cout << ans[i] << ' ';
	*/
	
	//build_tree();

	fstream cin("input.txt");
	

	node* r = get_next_graph(cin);

	node* a = new node(111);
	node* b = new node(112);

	r->adj.push_back(a);
	r->adj.push_back(b);

	node* r2 = get_next_graph(cin);

	node* r3 = get_next_graph(cin);

	return 0;
}
