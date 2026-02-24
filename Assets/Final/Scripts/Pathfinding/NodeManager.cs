using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public static NodeManager Instance;

    public List<Nodes> allNodes = new List<Nodes>();

    public float connectionDistance = 6f;

    public LayerMask obstacleMask;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FindAllNodes();
        ConnectNodes();
    }

    void FindAllNodes()
    {
        Nodes[] nodes = FindObjectsOfType<Nodes>();

        allNodes.Clear();

        foreach (Nodes n in nodes)
        {
            allNodes.Add(n);
        }
    }

    void ConnectNodes()
    {
        foreach (Nodes a in allNodes)
        {
            a.neighbors.Clear();

            foreach (Nodes b in allNodes)
            {
                if (a == b) continue;

                float dist = Vector3.Distance(
                    a.transform.position,
                    b.transform.position
                );

                if (dist <= connectionDistance)
                {
                    if (HasLineOfSight(a, b))
                    {
                        a.neighbors.Add(b);
                    }
                }
            }
        }
    }

    bool HasLineOfSight(Nodes a, Nodes b)
    {
        Vector3 dir = b.transform.position - a.transform.position;
        float dist = dir.magnitude;

        if (Physics.Raycast(
            a.transform.position,
            dir.normalized,
            dist,
            obstacleMask))
        {
            return false;
        }

        return true;
    }

    public Nodes GetClosestNode(Vector3 pos)
    {
        Nodes closest = null;
        float minDist = Mathf.Infinity;

        foreach (Nodes n in allNodes)
        {
            float d = Vector3.Distance(pos, n.transform.position);

            if (d < minDist)
            {
                minDist = d;
                closest = n;
            }
        }

        return closest;
    }
}