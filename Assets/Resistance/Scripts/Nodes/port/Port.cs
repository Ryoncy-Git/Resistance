using System.Runtime.CompilerServices;
using UnityEngine;

public class Port : MonoBehaviour
{
    // public virtual void OnMouseDown();

    public string portType;
    public Nodes parentNode;
    public Port connectedPort;
    private GameManager gameManager;
    private static Port draggingPort = null;

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
