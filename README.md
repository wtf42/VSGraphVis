# VSGraphVis

Расширение для Visual Studio 2015, которое позволяет визуализировать структуры данных на указателях, например:

    struct node // с помощью данной структуры можно задать произвольный граф
    {
	    int key;
	    int value;
	    vector<node*> adj;

	    node(int key = 0, int value  = 0) :key(key), value(value) {}
    };
  
или 

    struct node  // класс представляет узел бинарного дерева
    {
      int key;
      vector<int> tmp;
      node *left, *right, *parent;
    };
    


##Основные особенности
* Визуализация графов с произвольной структурой
* Итеративное изменение графа при добавлении/удалении вершин/ребер, при переходе к следующему break-point'у
* Возможность выбора алгоритма укладки графа
* Выделение на графе добавленных вершин
* Возможноть взаимодействия пользователя с визуализированным графом (смотреть подробное содержимое узла, перетаскивать вершины)

На практике, данное расширение делает проверку создаваемых структур существенно проще, по сравнению со стандартными средствами 
VS 2015. 

##Демонстрация
