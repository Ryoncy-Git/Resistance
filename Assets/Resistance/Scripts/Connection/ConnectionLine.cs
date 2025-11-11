using UnityEngine;
using System.Collections.Generic;

public class ConnectionLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Port parentPort;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        parentPort = this.gameObject.GetComponent<Port>();
    }

    public void ConnectLine()
    {
        if (parentPort.connectedPorts.Count == 0)
            return;

        Vector3 thisPosition = this.gameObject.transform.position;
        // Vector3 otherPosition = parentPort.connectedPort.gameObject.transform.position;

        List<Port> buffs = parentPort.connectedPorts;

        foreach(Port buff in buffs)
        {
            Vector3 otherPosition = buff.transform.position;

            lineRenderer.SetPosition(0, thisPosition);
            lineRenderer.SetPosition(1, otherPosition);
        }

        // lineRenderer.SetPosition(0, thisPosition);
        // lineRenderer.SetPosition(1, otherPosition);

        // Debug.Log("connected by Line");
    }
    

    public void DeleteLine()
    {
        Vector3 buff = new Vector3(0, 0, 0);

        lineRenderer.SetPosition(0, buff);
        lineRenderer.SetPosition(1, buff);
    }

    
}

// public struct ConnectionPort 
// {
//     private port1 = {get; private set};
//     private port2 = {get; private set};

// }
