using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [Header("Nodos del grafo")]
    public List<Node> nodes = new List<Node>();

    private void Awake()
    {
        nodes.Clear();
        foreach (Transform child in transform.Find("Nodes"))
        {
            Node n = child.GetComponent<Node>();
            if (n != null)
                nodes.Add(n);
        }
    }

    public Node GetClosestNode(Vector3 position)
    {
        Node closest = null;
        float minDist = Mathf.Infinity;

        foreach (Node n in nodes)
        {
            float dist = Vector3.Distance(position, n.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = n;
            }
        }

        return closest;
    }
}