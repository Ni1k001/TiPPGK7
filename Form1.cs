using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace TiPPGK7
{
    public partial class Form1 : Form
    {
        private readonly Graphics graphics;
        private readonly Bitmap bmp;
     
        private int mapWidth = 10;
        private int mapHeight = 10;

        private int tileSize = 70;

        private int[,] map;

        private Timer timer;

        Random r;

        List<Creature> creatures;
        List<Node> food;

        List<Label> labels;

        public Form1()
        {
            InitializeComponent();

            bmp = new Bitmap(702, 702);
            graphics = Graphics.FromImage(bmp);
            pictureBox1.Image = bmp;

            map = new int[mapWidth, mapHeight];

            timer = new Timer();
            timer.Interval = 250;
            timer.Tick += OnTick;
            timer.Start();

            r = new Random();

            creatures = new List<Creature>();
            for (int i = 0; i < 4; i++)
                creatures.Add(new Creature(r.Next(0, 10), r.Next(0, 10), r.Next(0, 500), Color.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256))));

            for (int i = 0; i < creatures.Count(); i++)
                for (int j = 0; j < creatures.Count(); j++)
                    if (i != j)
                        while (creatures[i].GetCoords().Item1 == creatures[j].GetCoords().Item1 && creatures[i].GetCoords().Item2 == creatures[j].GetCoords().Item2)
                            creatures[i].SetCords(r.Next(0, 10), r.Next(0, 10));

            food = new List<Node>();
            labels = new List<Label>();
            for (int i = 0; i < 5; i++)
            {
                food.Add(new Node(r.Next(0, 1000)));
                food[food.Count - 1].SetCoords(r.Next(0, 10), r.Next(0, 10));
                labels.Add(new Label());
                labels[labels.Count() - 1].Parent = pictureBox1;
                labels[labels.Count() - 1].TextAlign = ContentAlignment.MiddleCenter;
                labels[labels.Count() - 1].BackColor = Color.Transparent;
                labels[labels.Count() - 1].Location = new Point(food[food.Count() - 1].GetCoords().Item1 * tileSize + tileSize / 5, food[food.Count() - 1].GetCoords().Item2 * tileSize + tileSize / 3);
                labels[labels.Count() - 1].Width = 40;
                labels[labels.Count() - 1].Text = food[food.Count() - 1].GetValue().ToString();
            }

            for (int i = 0; i < food.Count(); i++)
                for (int j = 0; j < food.Count(); j++)
                    if (i != j)
                        while (food[i].GetCoords().Item1 == food[j].GetCoords().Item1 && food[i].GetCoords().Item2 == food[j].GetCoords().Item2)
                        {
                            food[i].SetCoords(r.Next(0, 10), r.Next(0, 10));
                            labels[i].Location = new Point(food[i].GetCoords().Item1 * tileSize + tileSize / 5, food[i].GetCoords().Item2 * tileSize + tileSize / 3);
                        }

            Invalidate();
        }

        private void OnTick(object sender, EventArgs e)
        {
            graphics.Clear(Color.White);

            DrawMap(graphics);

            foreach (Node n in food)
            {
                if (n.GetValue() > 0)
                    graphics.FillRectangle(Brushes.Red, n.GetCoords().Item1 * tileSize + tileSize / 4, n.GetCoords().Item2 * tileSize + tileSize / 4, tileSize / 2, tileSize / 2);
                else
                    graphics.DrawRectangle(Pens.Red, n.GetCoords().Item1 * tileSize + tileSize / 4, n.GetCoords().Item2 * tileSize + tileSize / 4, tileSize / 2, tileSize / 2);
            }

            foreach (Creature c in creatures)
            {
                c.Draw(graphics);

                int r1, r2, rnd;

                rnd = r.Next(0, 41);
                if (rnd < 10) { r1 = -1; r2 = 0; }
                else if (rnd >= 10 && rnd < 20) { r1 = 1; r2 = 0; }
                else if (rnd >= 20 && rnd < 30) { r1 = 0; r2 = -1; }
                else { r1 = 0; r2 = 1; }

                if (food.Find(x => (x.GetCoords().Item1 == c.GetCoords().Item1 && x.GetCoords().Item2 == c.GetCoords().Item2)) != null)
                    c.UpdateState(Tuple.Create<int, int>(r1, r2), food.FindAll(x => (x.GetCoords().Item1 == c.GetCoords().Item1 && x.GetCoords().Item2 == c.GetCoords().Item2)).ToArray());
                else if (c.GetState() != State.talk && creatures.Find(x => x != c && (x.GetCoords().Item1 == c.GetCoords().Item1 && x.GetCoords().Item2 == c.GetCoords().Item2) && (c.GetInfo().Count > 0 || x.GetInfo().Count > 0)) != null)
                {
                    List<Creature> s = creatures.FindAll(x => x != c && (x.GetCoords().Item1 == c.GetCoords().Item1 && x.GetCoords().Item2 == c.GetCoords().Item2));
                    foreach (Creature cr in s)
                        c.UpdateState(Tuple.Create<int, int>(0, 0), cr.GetInfo().ToArray());
                }
                else
                    c.UpdateState(Tuple.Create<int, int>(r1, r2));
            }

            foreach (Node node in food)
            {
                float fv = node.GetValue();

                List<Creature> c = creatures.FindAll(x => x.GetInfo().Find(y => y.GetCoords().Item1 == node.GetCoords().Item1 && y.GetCoords().Item2 == node.GetCoords().Item2) != null);

                foreach (Creature cx in c)
                {
                    foreach (Node cxn in cx.GetInfo())
                    {
                        if ((cxn.GetCoords().Item1 == node.GetCoords().Item1 && cxn.GetCoords().Item2 == node.GetCoords().Item2) && cxn.GetValue() < fv)
                            fv = cxn.GetValue();
                    }
                }

                node.SetValue(fv);

                Label l = labels.Find(x => (x.Location.X - tileSize / 5) / tileSize == node.GetCoords().Item1 && (x.Location.Y - tileSize / 3) / tileSize == node.GetCoords().Item2);
                if (l != null)
                {
                    l.Text = fv.ToString();
                }
            }

            Refresh();
            Invalidate();
        }

        void DrawMap(Graphics g)
        {
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                    g.DrawRectangle(Pens.Black, x * tileSize, y * tileSize, tileSize, tileSize);
        }
    }

    class Node
    {
        float value;
        int x = -1, y = -1;
        Node parent;

        public Node(float value = 0, Node parent = null)
        {
            this.value = value;
            this.parent = parent;
        }

        public void SetValue(float value) { this.value = value; }
        public float GetValue() { return value; }
        public void SetParent(Node parent) { this.parent = parent; }
        public Node GetParent() { return parent; }
        public void SetCoords(int x, int y) { this.x = x; this.y = y; }
        public Tuple<int, int> GetCoords() { return Tuple.Create(x, y); }
    }

    class Heap
    {
        private List<Node> nodes;

        public Heap()
        {
            nodes = new List<Node>();
        }

        ~Heap()
        {
            nodes.Clear(); ;
        }

        public void Insert(Node node)
        {
            nodes.Add(node);
            Update(nodes.IndexOf(nodes.Last()));
        }

        public Node GetMinNode()
        {
            if (!Empty())
                return nodes.ElementAt(0);
            return null;
            //throw new System.ArgumentOutOfRangeException("Empty");
        }

        public void ExtractMin()
        {
            if (Empty())
                return;

            nodes[0] = nodes.Last();
            nodes.Remove(nodes.Last());

            if (!Empty())
                Update(0);
        }

        void Update(int index)
        {
            if (index < nodes.Count() && index >= 0)
            {
                bool l = false, r = false;

                if (index * 2 + 1 < nodes.Count())
                    if (nodes[index].GetValue() > nodes[index * 2 + 1].GetValue())
                        l = true;

                if (index * 2 + 2 < nodes.Count())
                    if (nodes[index].GetValue() > nodes[index * 2 + 2].GetValue())
                        r = true;

                if (l && r)
                {
                    if (nodes[index * 2 + 1].GetValue() < nodes[index * 2 + 2].GetValue())
                    {
                        Swap(nodes, index, index * 2 + 1);
                        Update(index * 2 + 1);
                    }
                    else
                    {
                        Swap(nodes, index, index * 2 + 2);
                        Update(index * 2 + 2);
                    }
                }
                else if (l)
                {
                    Swap(nodes, index, index * 2 + 1);
                    Update(index * 2 + 1);
                }
                else if (r)
                {
                    Swap(nodes, index, index * 2 + 2);
                    Update(index * 2 + 2);
                }

                if ((index - 1) / 2 >= 0 && index != 0)
                {
                    if (nodes[index].GetValue() < nodes[(index - 1) / 2].GetValue())
                    {
                        Swap(nodes, index, (index - 1) / 2);
                        Update((index - 1) / 2);
                    }
                }
            }
            else
                Debug.WriteLine(index + " Out of Range");
        }

        public bool Empty()
        {
            if (nodes.Count == 0)
                return true;
            return false;
        }

        public void Print()
        {
            foreach (Node node in nodes)
            {
                Debug.WriteLine(node.GetValue());
            }
        }
        static void Swap(IList<Node> list, int indexA, int indexB)
        {
            Node tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        public void Clear()
        {
            nodes.Clear();
        }
    }

    enum State
    {
        patrol,
        talk,
        search,
        eat,
        death
    }

    class Creature
    {
        private int mapWidth = 10;
        private int mapHeight = 10;

        private int tileSize = 70;

        int x, y;
        int fullness;

        Brush brush;

        List<Node> infos;

        Heap heap;

        List<Node> tree;

        State current = State.patrol;

        public Creature(int x, int y, int fullness, Color c)
        {
            this.x = x;
            this.y = y;

            this.fullness = fullness;

            infos = new List<Node>();
            tree = new List<Node>();
            heap = new Heap();

            brush = new SolidBrush(c);
        }

        public State GetState() { return current; }
        public Tuple<int, int> GetCoords() { return Tuple.Create<int, int>(x, y); }

        public void SetCords(int x, int y) { this.x = x; this.y = y; }

        public List<Node> GetInfo() { return infos; }

        public void Draw(Graphics g)
        {
            g.FillEllipse(brush, x * tileSize + tileSize / 4, y * tileSize + tileSize / 4, tileSize / 2, tileSize / 2);
        }

        public void UpdateInfo(Node info)
        {
            Node n = infos.Find(i => i.GetCoords().Item1 == info.GetCoords().Item1 && i.GetCoords().Item2 == info.GetCoords().Item2);

            if (n != null)
            {
                int index = infos.IndexOf(n);

                if (n.GetValue() > 0 && infos[index].GetValue() != 0)
                {
                    if (infos[index].GetValue() > info.GetValue())
                        infos[index].SetValue(info.GetValue());
                }
            }
            else
            {
                infos.Add(new Node(info.GetValue()));
                infos[infos.Count - 1].SetCoords(info.GetCoords().Item1, info.GetCoords().Item2);
            }
        }

        float GetCurrentPathValue(Node node)
        {
            float steps = 0;

            while (node != null)
            {
                steps++;
                node = node.GetParent();
            }

            return steps;
        }

        float GetFuturePathValue(int x1, int y1, int x2, int y2)
        {
            double value = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            return (float)value;
        }

        int GetClosestPoint()
        {
            if (infos.Count > 0)
            {
                float min = float.MaxValue;

                foreach (Node node in infos)
                {
                    if (node.GetValue() > 0 && GetFuturePathValue(node.GetCoords().Item1, node.GetCoords().Item2, x, y) < min)
                    {
                        min = GetFuturePathValue(node.GetCoords().Item1, node.GetCoords().Item2, x, y);
                    }
                }

                if (min != float.MaxValue)
                    return infos.FindIndex(i => GetFuturePathValue(i.GetCoords().Item1, i.GetCoords().Item2, x, y) == min);
            }

            return -1;
        }

        void FindPath(int index = -1)
        {
            bool[,] visited = new bool[mapWidth, mapHeight];
            tree = new List<Node>();

            Node root = new Node();
            root.SetCoords(x, y);

            heap.Insert(root);

            while (!heap.Empty())
            {
                tree.Add(heap.GetMinNode());
                heap.ExtractMin();
                visited[tree[tree.Count() - 1].GetCoords().Item1, tree[tree.Count() - 1].GetCoords().Item2] = true;

                if (tree[tree.Count() - 1].GetCoords().Item1 == infos[index].GetCoords().Item1 && tree[tree.Count() - 1].GetCoords().Item2 == infos[index].GetCoords().Item2)
                    break;

                Node up = new Node();
                Node right = new Node();
                Node down = new Node();
                Node left = new Node();

                if (tree[tree.Count() - 1].GetCoords().Item1 - 1 >= 0 && !visited[tree[tree.Count() - 1].GetCoords().Item1 - 1, tree[tree.Count() - 1].GetCoords().Item2])
                {
                    left = new Node(GetCurrentPathValue(tree[tree.Count() - 1]) + GetFuturePathValue(infos[index].GetCoords().Item1, infos[index].GetCoords().Item2, tree[tree.Count() - 1].GetCoords().Item1 - 1, tree[tree.Count() - 1].GetCoords().Item2), tree[tree.Count() - 1]);
                    left.SetCoords(tree[tree.Count() - 1].GetCoords().Item1 - 1, tree[tree.Count() - 1].GetCoords().Item2);
                    heap.Insert(left);
                    visited[tree[tree.Count() - 1].GetCoords().Item1 - 1, tree[tree.Count() - 1].GetCoords().Item2] = true;
                }

                if (left.GetValue() == 0)
                    left.SetValue(int.MaxValue);

                tree.Add(left);

                if (tree[tree.Count() - 2].GetCoords().Item1 + 1 < mapWidth && !visited[tree[tree.Count() - 2].GetCoords().Item1 + 1, tree[tree.Count() - 2].GetCoords().Item2])
                {
                    right = new Node(1 + GetCurrentPathValue(tree[tree.Count() - 2]) + GetFuturePathValue(infos[index].GetCoords().Item1, infos[index].GetCoords().Item2, tree[tree.Count() - 2].GetCoords().Item1 + 1, tree[tree.Count() - 2].GetCoords().Item2), tree[tree.Count() - 2]);
                    right.SetCoords(tree[tree.Count() - 2].GetCoords().Item1 + 1, tree[tree.Count() - 2].GetCoords().Item2);
                    heap.Insert(right);
                    visited[tree[tree.Count() - 2].GetCoords().Item1 + 1, tree[tree.Count() - 2].GetCoords().Item2] = true;
                }

                if (right.GetValue() == 0)
                    right.SetValue(int.MaxValue);

                tree.Add(right);

                if (tree[tree.Count() - 3].GetCoords().Item2 - 1 >= 0 && !visited[tree[tree.Count() - 3].GetCoords().Item1, tree[tree.Count() - 3].GetCoords().Item2 - 1])
                {
                    up = new Node(1 + GetCurrentPathValue(tree[tree.Count() - 3]) + GetFuturePathValue(infos[index].GetCoords().Item1, infos[index].GetCoords().Item2, tree[tree.Count() - 3].GetCoords().Item1, tree[tree.Count() - 3].GetCoords().Item2 - 1), tree[tree.Count() - 3]);
                    up.SetCoords(tree[tree.Count() - 3].GetCoords().Item1, tree[tree.Count() - 3].GetCoords().Item2 - 1);
                    heap.Insert(up);
                    visited[tree[tree.Count() - 3].GetCoords().Item1, tree[tree.Count() - 3].GetCoords().Item2 - 1] = true;
                }

                if (up.GetValue() == 0)
                    up.SetValue(int.MaxValue);

                tree.Add(up);

                if (tree[tree.Count() - 4].GetCoords().Item2 + 1 < mapHeight && !visited[tree[tree.Count() - 4].GetCoords().Item1, tree[tree.Count() - 4].GetCoords().Item2 + 1])
                {
                    down = new Node(1 + GetCurrentPathValue(tree[tree.Count() - 4]) + GetFuturePathValue(infos[index].GetCoords().Item1, infos[index].GetCoords().Item2, tree[tree.Count() - 4].GetCoords().Item1, tree[tree.Count() - 4].GetCoords().Item2 + 1), tree[tree.Count() - 4]);
                    down.SetCoords(tree[tree.Count() - 4].GetCoords().Item1, tree[tree.Count() - 4].GetCoords().Item2 + 1);
                    heap.Insert(down);
                    visited[tree[tree.Count() - 4].GetCoords().Item1, tree[tree.Count() - 4].GetCoords().Item2 + 1] = true;
                }

                if (down.GetValue() == 0)
                    down.SetValue(int.MaxValue);

                tree.Add(down);
            }
        }

        public void UpdateState(Tuple<int, int> random, params Node[] n)
        {
            //Debug.WriteLine("fullness: " + fullness);
            //Debug.WriteLine("state: " + current);
            //Debug.WriteLine("color: " + new Pen(brush).Color.Name);

            if (current != State.death)
            {
                if (n.Length > 0)
                {
                    foreach (Node node in n)
                    {
                        UpdateInfo(node);
                    }

                    if (random.Item1 == 0 && random.Item2 == 0)
                    {
                        current = State.talk;
                        fullness--;
                        return;
                    }
                }

                if (fullness == 0)
                {
                    current = State.death;
                    return;
                }

                else if (fullness > 0 && fullness < 500)
                {
                    fullness--;

                    if (infos.Count > 0)
                    {
                        int index = GetClosestPoint();

                        if (index != -1)
                        {
                            if (infos[index].GetCoords().Item1 == x && infos[index].GetCoords().Item2 == y)
                            {
                                current = State.eat;

                                if (fullness + infos[index].GetValue() > 1000)
                                {
                                    infos[index].SetValue(infos[index].GetValue() - (1000 - fullness));
                                    fullness = 1000;
                                }
                                else
                                {
                                    fullness += (int)infos[index].GetValue();
                                    infos[index].SetValue(0);
                                }

                                return;
                            }
                            else
                            {
                                current = State.search;
                                FindPath(index);
                                MoveTo();
                                return;
                            }
                        }
                        else
                        {
                            current = State.search;
                            MoveRandom(random);
                            return;
                        }
                    }
                    else
                    {
                        current = State.search;
                        MoveRandom(random);
                        return;
                    }
                }

                else if (fullness >= 500 && fullness < 1000)
                {
                    current = State.patrol;
                    fullness--;

                    MoveRandom(random);

                    return;
                }

                else if (fullness == 1000)
                {
                    current = State.patrol;
                    fullness--;

                    MoveRandom(random);

                    return;
                }
            }
        }

        void MoveRandom(Tuple<int, int> random)
        {
            if (x + random.Item1 >= 0 && x + random.Item1 < mapWidth)
                x += random.Item1;

            if (y + random.Item2 >= 0 && y + random.Item2 < mapHeight)
                y += random.Item2;
        }

        void MoveTo()
        {
            Node current = tree[tree.Count() - 1];

            while (current.GetParent() != null)
            {
                x = current.GetCoords().Item1;
                y = current.GetCoords().Item2;

                current = current.GetParent();
            }

            
        }
    }
}
