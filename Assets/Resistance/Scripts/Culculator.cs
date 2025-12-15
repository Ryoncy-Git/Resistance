/*
using UnityEngine;
using System.Collections.Generic;

public class Culculator : MonoBehaviour
{
    public GameObject startObj, endObj, board;
    private Port startPort, endPort;
    private Graph graph;
    private Dictionary<Port, GraphNode> portToNode;
    public UIManager uiManager;


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

        uiManager.ChangeTextRes(R);
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


        // 2. Port→GraphNode mapping
        // Dictionary<Port, GraphNode> portToNode = new();
        portToNode = new();
        for (int i = 0; i < allPorts.Count; i++)
        {
            var node = new GraphNode(i);
            graph.nodes.Add(node);
            portToNode[allPorts[i]] = node; // 辞書の登録 これがPort -> dictionaryの対応表になる
        }


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
    }

    // 0Ωのノードを統合して簡略化したグラフを再構築

    // Dictionary<int, int> BuildReducedNodeMap(Graph g)
    // {
    //     UnionFind uf = new UnionFind(g.nodes.Count);

    //     foreach (var e in g.edges)
    //     {
    //         if (Mathf.Approximately((float)e.Res, 0f))
    //         {
    //             uf.Union(e.A.Id, e.B.Id);
    //         }
    //     }

    //     Debug.Log("end of UnionFind");

    //     Dictionary<int, int> map = new();
    //     int newId = 0;

    //     foreach (var n in g.nodes)
    //     {
    //         int root = uf.Find(n.Id);
    //         if (!map.ContainsKey(root))
    //             map[root] = newId++;
    //     }

    //     return map;
    // }
    Dictionary<int,int> BuildReducedNodeMap(Graph g)
    {
        // 1) 安全のため、oldId が連番でない場合に備えて index マップを作る
        //    indexMap: oldId -> compactIndex (0..N-1)
        int N = g.nodes.Count;
        var indexMap = new Dictionary<int,int>(N);
        for (int i = 0; i < g.nodes.Count; i++)
        {
            indexMap[g.nodes[i].Id] = i;
        }

        // 2) UnionFind は 0..N-1 のインデックスで扱う
        UnionFind uf = new UnionFind(N);

        // 3) 0Ω エッジで union (注意: e.A.Id / e.B.Id を index に変換)
        foreach (var e in g.edges)
        {
            if (Mathf.Approximately((float)e.Res, 0f))
            {
                // e.A.Id と e.B.Id が indexMap にあるかチェック（無ければスキップ or エラー）
                if (!indexMap.TryGetValue(e.A.Id, out int ia) || !indexMap.TryGetValue(e.B.Id, out int ib))
                {
                    Debug.LogWarning($"BuildReducedNodeMap: edge refers unknown node id A:{e.A.Id} B:{e.B.Id}");
                    continue;
                }
                uf.Union(ia, ib);
            }
        }

        // 4) 代表 rootIndex -> reducedId を作る（代表だけに連番を振る）
        var rootToReduced = new Dictionary<int,int>();
        int nextReducedId = 0;
        for (int i = 0; i < N; i++)
        {
            int root = uf.Find(i);
            if (!rootToReduced.ContainsKey(root))
                rootToReduced[root] = nextReducedId++;
        }

        // 5) 最終的に originalNodeId -> reducedId を返す
        var finalMap = new Dictionary<int,int>(N);
        foreach (var node in g.nodes)
        {
            int origId = node.Id;
            int idx = indexMap[origId];      // 0..N-1 の index
            int root = uf.Find(idx);        // 代表 index
            int reduced = rootToReduced[root];
            finalMap[origId] = reduced;
        }

        return finalMap;
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
        // 1. ノード縮約
        var map = BuildReducedNodeMap(g);
        int N = map.Count;

        Debug.Log("N = " + N);


        // 2. 行列生成
        double[,] G = BuildConductanceMatrix(g, map);


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


        // 4. 方程式を解く
        double[] V = SolveLinear(A, b);


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
                Debug.Log(e.Res);
            }
        }


        Debug.Log(Itotal);// 0になっちゃう

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

*/

using UnityEngine;
using System.Collections.Generic;

public class Calculator : MonoBehaviour
{
    public GameObject startObj, endObj, board;
    private Port startPort, endPort;
    private Graph graph;
    private Dictionary<Port, GraphNode> portToNode;
    public UIManager uiManager;

    void Start()
    {
        startPort = startObj.GetComponent<Port>();
        endPort = endObj.GetComponent<Port>();
    }

    public void StartCalculate()
    {
        if (!CanCalculate())
        {
            Debug.Log("cannot calculate");
            return;
        }

        CreateGraph();
        double R = ComputeEquivalentResistance(graph, startPort, endPort);
        uiManager.ChangeTextRes(R);
    }

    private bool CanCalculate()
    {
        if (startPort.connectedPorts.Count == 0 || endPort.connectedPorts.Count == 0)
            return false;

        foreach (Transform child in board.transform)
        {
            if (child.name == "StartNode" || child.name == "EndNode")
                continue;

            Transform ports = child.Find("Ports");
            foreach (Transform c in ports)
            {
                Port p = c.GetChild(0).GetComponent<Port>();
                if (p.connectedPorts.Count == 0)
                    return false;
            }
        }
        return true;
    }

    private void CreateGraph()
    {
        graph = new Graph();
        List<Port> allPorts = new();

        foreach (Transform node in board.transform)
        {
            if (node.name == "StartNode" || node.name == "EndNode")
                continue;

            Transform ports = node.Find("Ports");
            foreach (Transform child in ports)
            {
                allPorts.Add(child.GetChild(0).GetComponent<Port>());
            }
        }

        allPorts.Add(startPort);
        allPorts.Add(endPort);

        portToNode = new();
        for (int i = 0; i < allPorts.Count; i++)
        {
            var node = new GraphNode(i);
            graph.nodes.Add(node);
            portToNode[allPorts[i]] = node;
        }

        HashSet<(int, int)> added = new();

        foreach (var p in allPorts)
        {
            foreach (var cp in p.connectedPorts)
            {
                int idA = portToNode[p].Id;
                int idB = portToNode[cp].Id;
                if (idA == idB) continue;

                var key = idA < idB ? (idA, idB) : (idB, idA);
                if (added.Contains(key)) continue;
                added.Add(key);

                graph.AddEdge(portToNode[p], portToNode[cp], 0);
            }
        }


        // 4. 抵抗ノード内部のエッジ（2ポート間に抵抗値）
        foreach (Transform node in board.transform)
        {
            var n = node.GetComponent<Nodes>();
            if (n != null && n.nodeType == "Resistance")
            {
                var ports = node.Find("Ports");
                if (ports.childCount == 2)
                {
                    var p0 = ports.GetChild(0).GetChild(0).GetComponent<Port>();
                    var p1 = ports.GetChild(1).GetChild(0).GetComponent<Port>();
                    graph.AddEdge(portToNode[p0], portToNode[p1], n.resistance);
                    Debug.Log($"抵抗ノード {n.name}: {portToNode[p0].Id} - {portToNode[p1].Id}, R={n.resistance}");
                }
                else
                {
                    Debug.LogWarning($"抵抗ノード {n.name} が2ポートでない");
                }
            }
        }
    }

    Dictionary<int, int> BuildReducedNodeMap(Graph g)
    {
        int N = g.nodes.Count;
        var indexMap = new Dictionary<int, int>(N);
        for (int i = 0; i < g.nodes.Count; i++)
            indexMap[g.nodes[i].Id] = i;

        UnionFind uf = new UnionFind(N);
        foreach (var e in g.edges)
        {
            if (Mathf.Approximately((float)e.Res, 0f))
            {
                if (indexMap.TryGetValue(e.A.Id, out int ia) &&
                    indexMap.TryGetValue(e.B.Id, out int ib))
                {
                    uf.Union(ia, ib);
                }
            }
        }

        var rootToReduced = new Dictionary<int, int>();
        int nextReducedId = 0;
        for (int i = 0; i < N; i++)
        {
            int root = uf.Find(i);
            if (!rootToReduced.ContainsKey(root))
                rootToReduced[root] = nextReducedId++;
        }

        var finalMap = new Dictionary<int, int>(N);
        foreach (var node in g.nodes)
        {
            int idx = indexMap[node.Id];
            int root = uf.Find(idx);
            finalMap[node.Id] = rootToReduced[root];
        }

        return finalMap;
    }

    double[,] BuildConductanceMatrix(Graph g, Dictionary<int, int> nodeMap)
    {
        int N = nodeMap.Count;
        double[,] G = new double[N, N];

        foreach (var e in g.edges)
        {
            if (e.Res <= 0) continue;

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

    double ComputeEquivalentResistance(Graph g, Port startPort, Port endPort)
    {
        // 計算が何やらおかしいことになっている
        // 回路の検出は上手くいっている
        // 回路の縮約以降は動作をまだ追えていないので要注意すること
        // 具体的には、直列回路があるとNANΩになってしまう
        // 詳細はcopilot君に
        foreach (var e in g.edges)
        {
            Debug.Log($"Edge {e.A.Id} - {e.B.Id}, R={e.Res}");
        }

        var map = BuildReducedNodeMap(g);
        int N = map.Count;

        double[,] G = BuildConductanceMatrix(g, map);

        int s = map[portToNode[startPort].Id]; // startノード
        int t = map[portToNode[endPort].Id];   // endノード

        // --- Dirichlet境界条件: s=1V, t=0V を既知として残りのノードだけ未知数にする ---
        List<int> unknownNodes = new();
        for (int i = 0; i < N; i++)
        {
            if (i != s && i != t) unknownNodes.Add(i);
        }

        int M = unknownNodes.Count;
        double[,] A = new double[M, M];
        double[] b = new double[M];

        // 行列構築
        for (int ui = 0; ui < M; ui++)
        {
            int i = unknownNodes[ui];
            for (int uj = 0; uj < M; uj++)
            {
                int j = unknownNodes[uj];
                A[ui, uj] = G[i, j];
            }

            // 既知電位の寄与を b に加える
            double rhs = 0;
            rhs -= G[i, s] * 1.0; // Vs=1V
            rhs -= G[i, t] * 0.0; // Vt=0V
            b[ui] = -rhs;
        }

        // 解く
        double[] Vunknown = (M > 0) ? SolveLinear(A, b) : new double[0];

        // --- 電流計算 ---
        double Vs = 1.0;
        double Itotal = 0;

        foreach (var e in g.edges)
        {
            if (e.Res <= 0) continue;

            if (map[e.A.Id] == s || map[e.B.Id] == s)
            {
                int other = (map[e.A.Id] == s ? map[e.B.Id] : map[e.A.Id]);
                double Vo;

                if (other == t) Vo = 0.0;
                else if (other == s) Vo = 1.0;
                else
                {
                    int idx = unknownNodes.IndexOf(other);
                    Vo = Vunknown[idx];
                }

                Itotal += (Vs - Vo) / e.Res;
            }
        }

        Debug.Log("Itotal = " + Itotal);

        if (Mathf.Approximately((float)Itotal, 0f))
            return double.PositiveInfinity; // 開放回路扱い

        return 1.0 / Itotal; // R = V/I
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

