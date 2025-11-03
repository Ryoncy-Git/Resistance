using UnityEngine;

public class BezierConnection : MonoBehaviour
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

    public void ConnectBezier()
    {
        if (parentPort.connectedPort == null)
            return;

        Vector3 thisPosition = this.gameObject.transform.position;
        Vector3 otherPosition = parentPort.connectedPort.gameObject.transform.position;

        lineRenderer.SetPosition(0, thisPosition);
        lineRenderer.SetPosition(1, otherPosition);

        // Debug.Log("connected by Bezier");
    }
    

    public void DeleteBezier()
    {
        Vector3 buff = new Vector3(0, 0, 0);

        lineRenderer.SetPosition(0, buff);
        lineRenderer.SetPosition(1, buff);
    }
}
