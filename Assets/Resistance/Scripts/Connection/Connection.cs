using UnityEngine;
using System.Collections.Generic;

public class Connection : MonoBehaviour
{
    public Port portA;
    public Port portB;
    private LineRenderer line;

    public void Initialize(Port a, Port b)
    {
        portA = a;
        portB = b;
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;

        UpdateLine();
    }

    public void UpdateLine()
    {
        line.SetPosition(0, portA.transform.position);
        line.SetPosition(1, portB.transform.position);
    }

    public void DeleteLine()
    {
        line.positionCount = 0;
    }
}
