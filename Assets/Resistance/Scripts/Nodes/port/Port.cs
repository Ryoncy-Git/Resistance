using UnityEngine;

public class Port : MonoBehaviour
{
    public string portType;
    public Nodes parentNode;
    public Port connectedPort;
    private GameManager gameManager;
    private static Port draggingPort = null;

    public int resolution = 30;
    private LineRenderer lineRenderer;
    private Transform[] portBezier;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = resolution + 1;

        portBezier = new Transform[2];
        int i = 0;
        foreach(Transform child in transform)
        {
            if(child.name.Contains("Bezier"))
            {
                portBezier[i] = child;
                i++;
            }
        }
    }

    public void Initialize(Nodes parent)
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        portType = "port";
        connectedPort = null;
        parentNode = parent;
    }

    public bool CanConnect(Port other)
    {
        return this.parentNode != other.parentNode;
    }

    void OnMouseDown()
    {
        // 接続はじめ
        if (draggingPort == null)
        {
            draggingPort = this;
            Debug.Log("Start dragging from port: " + this.name);
        }
        else
        {
            Port port = this;
            if (port != draggingPort)
            {
                if (port.CanConnect(draggingPort))
                {
                    port.connectedPort = draggingPort;
                    draggingPort.connectedPort = port;
                    Debug.Log($"Connected {draggingPort.parentNode.name} ↔ {port.parentNode.name}");

                    ConnectBezier();
                }
                else
                {
                    Debug.Log("Cannot connect: same parent node");
                }
            }

            draggingPort = null;
        }
    }

    public void ConnectBezier()
    {
        if (this.connectedPort == null)
            return;

        Transform p0 = portBezier[0], p1 = portBezier[1],
                  p2 = connectedPort.portBezier[0], p3 = connectedPort.portBezier[1];

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 point = CalculateCubicBezierPoint(t, p0.position, p1.position, p2.position, p3.position);
            lineRenderer.SetPosition(i, point);
        }
        Debug.Log("connected by Bezier");
    }
    
    Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0; // (1 - t)^3 * P0
        point += 3 * uu * t * p1; // 3(1 - t)^2 * t * P1
        point += 3 * u * tt * p2; // 3(1 - t) * t^2 * P2
        point += ttt * p3;        // t^3 * P3

        return point;
    }
}
