using UnityEngine;

public class Port : MonoBehaviour
{
    public string portType;
    public Nodes parentNode;
    public Port connectedPort;
    private static Port draggingPort = null;
    private bool isPushAlt = false;
    public BezierConnection bezierConnection;

    void Start()
    {
        bezierConnection = this.gameObject.GetComponent<BezierConnection>();
    }

    public void Initialize(Nodes parent)
    {
        portType = "port";
        connectedPort = null;
        parentNode = parent;
    }

    public bool CanConnect(Port other)
    {
        return this.parentNode != other.parentNode;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            isPushAlt = true;
        }

        if(Input.GetKeyUp(KeyCode.LeftAlt))
        {
            isPushAlt = false;
        }
    }

    void OnMouseDown()
    {
        if (isPushAlt)
        {
            if (connectedPort == null)
                return;

            bezierConnection.DeleteBezier();
            connectedPort.bezierConnection.DeleteBezier();

            connectedPort.connectedPort = null;
            connectedPort = null;

            Debug.Log("disconnected");
        }
        else
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

                        bezierConnection.ConnectBezier();
                    }
                    else
                    {
                        Debug.Log("Cannot connect: same parent node");
                    }
                }

                draggingPort = null;
            }
        }
    }
}
