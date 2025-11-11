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

        UpdateLine();
    }

    public void UpdateLine()
    {
        
    }
}
