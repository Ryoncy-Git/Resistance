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
        for(int i = 0; i < n; i++)
        {
            double buff;
            double pivot = A[i, i];
            if(Mathf.Abs((float)pivot) < 1e-12f)
            {
                // ピボットが0なので行入れ替え
                // まず、0じゃないところを探す
                int newPivotRow = -1;
                for(int k = i + 1; k < n; k++)
                {
                    if(Mathf.Abs((float)A[k, i]) < 1e-12f)
                    {
                        if(k == n - 1)
                        {
                            Debug.LogWarning("行列サイズ != rank");
                        }
                        continue;
                    }
                    else
                    {
                        newPivotRow = k;
                        break;
                    }
                }
                

                if(newPivotRow == -1)
                {
                    Debug.LogError("new pivot row = -1");
                }

                // 良い感じに行入れ替え
                
                for(int j = 0; j < n; j++)
                {
                    buff = A[i, j];
                    A[i, j] = A[newPivotRow, j];
                    A[newPivotRow, j] = buff;
                }
                buff = b[i];
                b[i] = b[newPivotRow];
                b[newPivotRow] = buff;
            }
            pivot = A[i, i];

            for(int j = 0; j < n; j++)
            {
                // pivotを1にするためにその行をpivorで割る
                A[i, j] /= pivot;
            }
            b[i] /= pivot;

            for(int ii = i + 1; ii < n; ii++)
            {
                // 斜め成分をとる
                double head = A[ii, i];
                for(int j = i; j < n; j++)
                {
                    A[ii, j] -= A[i, j] * head;
                }

                b[ii] -= b[i] * head;
            }
        }


        for (int i = n - 1; i >= 0; i--)
        {
            for (int j = i + 1; j < n; j++)
            {
                b[i] -= A[i, j] * b[j];
            }
            // A[i,i] は pivot 正規化済みなので 1 のはず
            // もし 1 でない可能性があるなら b[i] /= A[i,i] を入れる
        }

        return b;
    }



    double ComputeEquivalentResistance(Graph g, Port startPort, Port endPort)
    {
        // foreach (var e in g.edges)
        // {
        //     Debug.Log($"Edge {e.A.Id} - {e.B.Id}, R={e.Res}");
        // }

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

        // 特別処理: 未知ノードがゼロなら直接抵抗を返す
        if (M == 0)
        {
            foreach (var e in g.edges)
            {
                if (e.Res > 0 &&
                    ((map[e.A.Id] == s && map[e.B.Id] == t) ||
                    (map[e.A.Id] == t && map[e.B.Id] == s)))
                {
                    return e.Res;
                }
            }
            return double.PositiveInfinity; // 抵抗が見つからない場合は開放
        }

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
        double[] Vunknown = SolveLinear(A, b);

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
                    Vo = (idx >= 0) ? Vunknown[idx] : 0.0;
                }

                Itotal += (Vs - Vo) / e.Res;
            }
        }

        Debug.Log("Itotal = " + Itotal);

        if (double.IsNaN(Itotal) || Mathf.Approximately((float)Itotal, 0f))
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

