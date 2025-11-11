using UnityEngine;
using System.Collections.Generic;

public class ConnectionManager : MonoBehaviour
{
    public GameObject linePrefab, board;
    public static ConnectionManager Instance { get; private set; }
    private Dictionary<(Port, Port), Connection> connections = new();
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
    }

    public Connection CreateConnection(Port a, Port b)
    {
        var key = (a, b);

        if (connections.ContainsKey(key))
            return connections[key];

        GameObject go =
            Instantiate(linePrefab, new Vector3(0f, 0f, 0f), Quaternion.identity, board.transform);
        var conn = go.GetComponent<Connection>();

        conn.Initialize(a, b);
        connections[key] = conn;
        return conn;
    }

    public void DeleteConnection(Port a, Port b)
    {

    }
}
