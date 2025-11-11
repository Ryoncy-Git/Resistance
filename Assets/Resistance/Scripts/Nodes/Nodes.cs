using UnityEngine;
using System.Collections.Generic;
public abstract class Nodes : MonoBehaviour 
{
    public List<Port> Ports = new();
    public GameObject portPrefab;
    public string nodeName;

    public Port CreatePort()
    {
        var portGO = Instantiate(portPrefab, transform); // gameobject
        var port = portGO.GetComponent<Port>();
        port.Initialize(this);
        Ports.Add(port);
        return port;
    }

    public abstract void Initialize();
    public abstract void InstallPort();
}

