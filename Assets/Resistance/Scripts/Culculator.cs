using UnityEngine;
using System.Collections.Generic;

public class Culculator : MonoBehaviour
{
    public GameObject startObj, endObj, board;
    private Port startPort, endPort;
    private Graph graph;
    private Dictionary<Port, GraphNode> portToNode;


    void Start()
    {
        startPort = startObj.GetComponent<Port>();
        endPort = endObj.GetComponent<Port>();
    }
    public void StartCulculate()
    {
        if(!canCalculate())
        {
            Debug.Log("connot culclate");
            return;
        }

        CreateGraph();
        // 計算
        double R = ComputeEquivalentResistance(graph, startPort, endPort);
    }
    
    // お￥設計中～
    private bool canCalculate()
    {
        if(startPort.connectedPorts.Count == 0 || endPort.connectedPorts.Count == 0)
            return false;

        // 全ポートが接続先を持っているかチェック
        foreach (Transform child in board.transform)
        {
            if(child.name == "StartNode" || child.name == "EndNode")
                continue;

            Transform ports = child.Find("Ports");// portの親を取得
            foreach(Transform c in ports)
            {
                Port p = c.GetChild(0).gameObject.GetComponent<Port>();
                if (p.connectedPorts.Count == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void CreateGraph()
    {
        graph = new Graph();

        List<Port> allPorts = new();

        //1. 全ポートの収集
        foreach (Transform node in board.transform)
        {
            if(node.name == "StartNode" || node.name == "EndNode")
                continue;

            Transform ports = node.Find("Ports");// portの親を取得
            foreach(Transform child in ports)
            {
                allPorts.Add(child.GetChild(0).GetComponent<Port>());
            }
        }

        allPorts.Add(startPort);
        allPorts.Add(endPort);

        Debug.Log("end of collect port");

        // 2. Port→GraphNode mapping
        // Dictionary<Port, GraphNode> portToNode = new();
        portToNode = new();
        Debug.Log("end of new");
        for (int i = 0; i < allPorts.Count; i++)
        {
            var node = new GraphNode(i);
            graph.nodes.Add(node);
            Debug.Log("end of add");
            portToNode[allPorts[i]] = node; // 辞書の登録 これがPort -> dictionaryの対応表になる
        }

        Debug.Log("end of mapping port");

        // 3. Edgeを張る
        HashSet<(int,int)> added = new(); // <Port ID, Port ID>で各エッジを定義

        // 全ポートの接続先を確認
        foreach (var p in allPorts)
        {
            foreach (var cp in p.connectedPorts)
            {
                // 接続が確認できたポート同士の
                int idA = portToNode[p].Id; // 片方のポートのIDをIdAとして
                int idB = portToNode[cp].Id;// もう片方をIdBとして

                if (idA == idB) continue;

                // 重複エッジを避ける
                var key = idA < idB ? (idA, idB) : (idB, idA);
                if (added.Contains(key)) continue;

                added.Add(key); // 確認済みリストを更新

                double R = 0; //　いったん0で初期化
                if(p.parentNode == cp.parentNode) // board内で同じノードのポートだった時（親ノードが同じとき）
                {
                    if(p.parentNode.nodeType == "Resistance")// 抵抗ノードの時はその抵抗値を代入
                    {
                        R = p.parentNode.resistance;
                    }
                    else // 抵抗ノードでないときは抵抗値0
                    {
                        R = 0;
                    }
                }

                graph.AddEdge(portToNode[p], portToNode[cp], R);
            }
        }
        Debug.Log("end of create edge");
    }

    // 0Ωのノードを統合して簡略化したグラフを再構築

    Dictionary<int, int> BuildReducedNodeMap(Graph g)
    {
        UnionFind uf = new UnionFind(g.nodes.Count);

        foreach (var e in g.edges)
        {
            if (Mathf.Approximately((float)e.Res, 0f))
            {
                uf.Union(e.A.Id, e.B.Id);
            }
        }

        Dictionary<int, int> map = new();
        int newId = 0;

        foreach (var n in g.nodes)
        {
            int root = uf.Find(n.Id);
            if (!map.ContainsKey(root))
                map[root] = newId++;
        }

        return map;
    }

    // コンダクタンス行列、N * N行列を作る
    double[,] BuildConductanceMatrix(Graph g, Dictionary<int,int> nodeMap)
    {
        int N = nodeMap.Count;
        double[,] G = new double[N, N];

        foreach (var e in g.edges)
        {
            if (e.Res <= 0) continue; // 0Ω は統合済みで無視

            int a = nodeMap[e.A.Id];
            int b = nodeMap[e.B.Id];

            double gval = 1.0 / e.Res;

            G[a, a] += gval;
            G[b, b] += gval;
            G[a, b] -= gval;
            G[b, a] -= gval;
        }

        return G;
    }

    // ガウス消去法によって行列計算
    double[] SolveLinear(double[,] A, double[] b)
    {
        int n = b.Length;

        for (int i = 0; i < n; i++)
        {
            double pivot = A[i, i];
            for (int j = i; j < n; j++)
                A[i, j] /= pivot;
            b[i] /= pivot;

            for (int k = 0; k < n; k++)
            {
                if (k == i) continue;

                double factor = A[k, i];
                for (int j = i; j < n; j++)
                    A[k, j] -= factor * A[i, j];

                b[k] -= factor * b[i];
            }
        }

        return b;
    }

    // 抵抗値を求める本体部分
    double ComputeEquivalentResistance(Graph g, Port startPort, Port endPort)
    {
        Debug.Log("start of compute");
        // 1. ノード縮約
        var map = BuildReducedNodeMap(g);
        int N = map.Count;

        Debug.Log("end of reduce map");

        // 2. 行列生成
        double[,] G = BuildConductanceMatrix(g, map);

        Debug.Log("end of build matrix");

        // 3. Known voltage boundary conditions
        int s = map[portToNode[startPort].Id]; // startPort の GraphNode ID 
        int t = map[portToNode[endPort].Id];   // endPort の GraphNode ID (ground)

        int M = N - 1; // ground 行・列を除去

        double[,] A = new double[M, M];
        double[] b = new double[M];

        System.Func<int,int> idx = (x) => x < t ? x : x - 1;

        for (int i = 0; i < N; i++)
        {
            if (i == t) continue;
            int ii = idx(i);

            for (int j = 0; j < N; j++)
            {
                if (j == t) continue;
                int jj = idx(j);
                A[ii, jj] = G[i, j];
            }
        }

        // startPort の電位を +1V とする
        b[idx(s)] = 1.0;

        Debug.Log("end of ????");

        // 4. 方程式を解く
        double[] V = SolveLinear(A, b);

        Debug.Log("end of solve");

        // startPort の電位
        double Vs = 1.0;

        // 5. startPort から出る電流を計算
        double Itotal = 0;

        foreach (var e in g.edges)
        {
            if (e.A.Id == s || e.B.Id == s)
            {
                int other = (e.A.Id == s ? map[e.B.Id] : map[e.A.Id]);
                double Vo = (other == t ? 0 : V[idx(other)]);

                Itotal += (Vs - Vo) / e.Res;
            }
        }

        Debug.Log("end of culclation");

        return 1.0 / Itotal; // R = V / I , V=1
    }
}

class UnionFind
{
    int[] parent;

    public UnionFind(int n)
    {
        parent = new int[n];
        for (int i = 0; i < n; i++) parent[i] = i;
    }

    public int Find(int x)
    {
        if (parent[x] != x) parent[x] = Find(parent[x]);
        return parent[x];
    }

    public void Union(int a, int b)
    {
        int ra = Find(a);
        int rb = Find(b);
        if (ra != rb) parent[rb] = ra;
    }
}
