using System.Collections.Generic;
using UnityEngine;

public class Nodes : MonoBehaviour
{
    public List<Nodes> neighbors = new List<Nodes>();

    [HideInInspector]
    public float gCost;

    [HideInInspector]
    public float hCost;

    [HideInInspector]
    public float fCost;

    [HideInInspector]
    public Nodes parent;

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    private void OnDrawGizmos()
    {
        // Dibuja el nodo
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);

        // Dibuja conexiones
        Gizmos.color = Color.white;

        foreach (Nodes n in neighbors)
        {
            if (n != null)
            {
                Gizmos.DrawLine(transform.position, n.transform.position);
            }
        }
    }
}
