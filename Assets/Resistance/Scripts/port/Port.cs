using UnityEngine;
using System.Collections.Generic;

public class Port : MonoBehaviour
{
    public string portType;
    public Nodes parentNode;
    public Port connectedPort;
    private static Port draggingPort = null;
    private bool isPushAlt = false;
    public ConnectionLine connectionLine;

    public List<Port> connectedPorts = new List<Port>();

    void Start()
    {
        connectionLine = this.gameObject.GetComponent<ConnectionLine>();
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
        // altキー押してるかだけ判断
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
            // コネクションの切断
            // 接続先が無かったらearly return
            if (connectedPorts.Count == 0)
                return;


            // 接続をクリア
            foreach (Port port in connectedPorts)
            {
                // 相手方のconnectedPortから自分を削除
                port.connectedPorts.Remove(this);
            }
            // 自分のconnectedPortをすべてクリア
            connectedPorts.Clear();


            // 接続線の描画をクリア
            connectionLine.DeleteLine();
            foreach (Port port in connectedPorts)
            {
                port.connectionLine.DeleteLine();
            }
            // connectedPort.connectedPort = null;
            

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
                        // 接続
                        // connectedPortに接続先を追加
                        port.connectedPorts.Add(draggingPort);
                        draggingPort.connectedPorts.Add(port);
                        Debug.Log($"Connected {draggingPort.parentNode.name} ↔ {port.parentNode.name}");

                        // 接続されていることを表す線を描画
                        connectionLine.ConnectLine();
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
