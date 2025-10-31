using UnityEngine;

public class ResistanceNode : Nodes
{
    private GameObject portIn;
    private GameObject portOut;
    private Nodes portInput;
    public override void Initialize()
    {
        nodeName = "Parallel";
        portIn = portOut = null;

        InstallPort();
    }

    public void GetInput()
    {
        // portInput.GetOutput();
    }

    public override void InstallPort()
    {
        GameObject ports = null;

        foreach (Transform child in this.gameObject.transform)
        {
            if (child.name == "Ports")
            {
                ports = child.gameObject;
                continue;
            }
        }


        GameObject[] portObjes = new GameObject[ports.transform.childCount];
        for (int i = 0; i < ports.transform.childCount; i++)
        {
            portObjes[i] = ports.transform.GetChild(i).gameObject;
        }

        for(int i = 0; i < ports.transform.childCount; i++)
        {
            // port 
            Vector3 position = new Vector3(0f, 0f, 0f);
            GameObject obj =
                Instantiate(portPrefab, position, Quaternion.identity, portObjes[i].transform);
            obj.transform.localPosition = position;
            Port port = obj.GetComponent<Port>();

            port.Initialize(this);
        }
    }
}
