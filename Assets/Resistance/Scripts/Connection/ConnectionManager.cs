using UnityEngine;
using System.Collections.Generic;

public class ConnectionManager : MonoBehaviour
{
    public GameObject connectionPrefab, board;
    public static ConnectionManager Instance { get; private set; }
    public Dictionary<(Port, Port), Connection> connections = new();
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
    }

    public Connection CreateConnection(Port a, Port b)
    {
        var key = NormalizeKey(a, b);

        if (connections.ContainsKey(key))
            return connections[key];

        GameObject go =
            Instantiate(connectionPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity, board.transform);
        var conn = go.GetComponent<Connection>();

        conn.Initialize(a, b);
        connections[key] = conn;
        return conn;
    }

    public void DeleteConnection(Port a, Port b)
    {
        var key = NormalizeKey(a, b);

        if (connections.TryGetValue(key, out var conn))
        {
            // Debug.Log("will destroy " + conn.gameObject);
            conn.DeleteLine();
            Destroy(conn.gameObject);
            connections.Remove(key);
        }
    }

    public (Port, Port) NormalizeKey(Port a, Port b)
    {
        return (a.GetInstanceID() < b.GetInstanceID()) ? (a, b) : (b, a);
    }
}
