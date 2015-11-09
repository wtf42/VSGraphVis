// contest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <fstream>
#include <iostream>
#include <vector>
#include <cassert>
using namespace std;

#define INF (int)1e9

struct node
{
	int key;
	int value;
	vector<node*> adj;

	node(int key = 0, int value  = 0) :key(key), value(value) {}
};

node* get_next_graph(fstream &cin)
{
	int n, m; cin >> n >> m;

	vector<node*> v(n);
	for (int i = 0; i < n; i++)
		v[i] = new node(i + 1, i + 1);

	node* r = v[0];
	for (int i = 0; i < m; i++)
	{
		int a, b; cin >> a >> b;
		a--, b--;
		v[a]->adj.push_back(v[b]);
		v[b]->adj.push_back(v[a]);
		if (i % 3 == 0)
		{
			a = 0;
		}
	}

	return r;
}

int main()
{
	fstream cin("input.txt");

	node* r = get_next_graph(cin);

	node* a = new node(111, 111);
	node* b = new node(112, 112);

	r->adj.push_back(a);
	r->adj.push_back(b);

	node* r2 = get_next_graph(cin);

	node* r3 = get_next_graph(cin);

	node* full = get_next_graph(cin);


	return 0;
}
