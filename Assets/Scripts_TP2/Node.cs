using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("Grafo")]
    public List<Node> connections = new List<Node>();

    [HideInInspector] public float gCost;
    [HideInInspector] public float hCost;
    public float fCost => gCost + hCost;

    [HideInInspector] public Node parent;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.color = Color.cyan;
        foreach (var n in connections)
        {
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}
