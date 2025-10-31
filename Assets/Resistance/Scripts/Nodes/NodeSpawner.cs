using UnityEngine;

public class NodeSpawner : MonoBehaviour
{
    public GameObject parallelNodePrefab;
    public GameObject resistanceNodePrefab;
    public GameObject debugPrefab;
    public Transform boardParent;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SpawnNode(resistanceNodePrefab); 
            Debug.Log("spawn resistance prefab");
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // SpawnNode(parallelNodePrefab); 
            SpawnNode(parallelNodePrefab);
            Debug.Log("spawn parallel prefab");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // SpawnNode(parallelNodePrefab); 
            SpawnNode(debugPrefab); 
            Debug.Log("spwan debug prefab");
        }
    }

    void SpawnNode(GameObject prefab)
    {
        Vector3 position = new Vector3(0f, 0f, 0f);

        Quaternion rotation = Quaternion.identity;


        GameObject nodeObj = Instantiate(prefab, position, rotation, boardParent);
        
        Nodes node = nodeObj.GetComponent<Nodes>();
        node.Initialize();
    }
}
