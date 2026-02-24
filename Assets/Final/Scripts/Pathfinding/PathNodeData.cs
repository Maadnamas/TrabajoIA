using UnityEngine;

public class PathNodeData
{
    public Nodes node;

    public float gCost;
    public float hCost;

    public float fCost => gCost + hCost;

    public PathNodeData parent;

    public PathNodeData(Nodes n)
    {
        node = n;
    }
}
