using UnityEngine;
using System.Collections.Generic;

public class GraphNode
{
    public int Id;
    public List<GraphEdge> Edge = new List<GraphEdge>();

    public GraphNode(int id)
    {
        Id = id;
    }
}

public class GraphEdge
{
    public GraphNode A;
    public GraphNode B;
    public double Res;

    public GraphEdge(GraphNode a, GraphNode b, double res)
    {
        A = a;
        B = b; 
        Res = res;
    }

    public GraphNode Other(GraphNode n)
    {
        return n == A ? A : B;
    }
}

public class Graph
{
    public List<GraphNode> nodes = new();
    public List<GraphEdge> edges = new();
    public GraphNode AddNode(int id)
    {
        var n = new GraphNode(id);
        nodes.Add(n);
        return n;
    }

    public void AddEdge(GraphNode a, GraphNode b, double res)
    {
        var e = new GraphEdge(a, b, res);
        edges.Add(e);
        a.Edge.Add(e);
        b.Edge.Add(e);
    }
}
