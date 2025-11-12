using UnityEngine;

public class Culculator : MonoBehaviour
{
    public GameObject startObj, endObj, board;
    private Port startPort, endPort;

    void Start()
    {
        startPort = startObj.GetComponent<Port>();
        endPort = endObj.GetComponent<Port>();
    }
    public void StartCulculate()
    {

    }
    
    // お￥設計中～
    private bool canCalculate()
    {
        // 全ポートが接続先を持っているかチェック
        foreach (Children child in board)
        {
            Port p = child.GetComponent<Port>();
            if (p.connectedPorts.Count == 0)
            {
                return false;
            }
        }

        return true;
    }
}
