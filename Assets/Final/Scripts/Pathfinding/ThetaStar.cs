using System.Collections.Generic;
using UnityEngine;

public class ThetaStar
{
    private List<PathNodeData> openList;
    private HashSet<PathNodeData> closedList;

    private LayerMask obstacleMask;

    public ThetaStar(LayerMask mask)
    {
        obstacleMask = mask;
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Nodes startNode = NodeManager.Instance.GetClosestNode(startPos);
        Nodes targetNode = NodeManager.Instance.GetClosestNode(targetPos);

        if (startNode == null || targetNode == null)
            return null;

        Dictionary<Nodes, PathNodeData> nodeData =
            new Dictionary<Nodes, PathNodeData>();

        openList = new List<PathNodeData>();
        closedList = new HashSet<PathNodeData>();

        PathNodeData startData = new PathNodeData(startNode);

        startData.gCost = 0;
        startData.hCost = Heuristic(startNode, targetNode);

        openList.Add(startData);
        nodeData[startNode] = startData;

        while (openList.Count > 0)
        {
            PathNodeData current = GetLowestFCost();

            if (current.node == targetNode)
            {
                return BuildPath(current);
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (Nodes neighbor in current.node.neighbors)
            {
                if (!nodeData.ContainsKey(neighbor))
                {
                    nodeData[neighbor] = new PathNodeData(neighbor);
                }

                PathNodeData neighborData = nodeData[neighbor];

                if (closedList.Contains(neighborData))
                    continue;

                if (!openList.Contains(neighborData))
                {
                    openList.Add(neighborData);
                }

                UpdateVertex(current, neighborData, targetNode);
            }
        }

        return null;
    }

    void UpdateVertex(
        PathNodeData current,
        PathNodeData neighbor,
        Nodes target)
    {
        PathNodeData parent = current.parent;

        if (parent != null && HasLineOfSight(parent.node, neighbor.node))
        {
            float newG =
                parent.gCost +
                Distance(parent.node, neighbor.node);

            if (newG < neighbor.gCost || neighbor.parent == null)
            {
                neighbor.gCost = newG;
                neighbor.parent = parent;
            }
        }
        else
        {
            float newG =
                current.gCost +
                Distance(current.node, neighbor.node);

            if (newG < neighbor.gCost || neighbor.parent == null)
            {
                neighbor.gCost = newG;
                neighbor.parent = current;
            }
        }

        neighbor.hCost = Heuristic(neighbor.node, target);
    }

    float Heuristic(Nodes a, Nodes b)
    {
        return Vector3.Distance(
            a.transform.position,
            b.transform.position);
    }

    float Distance(Nodes a, Nodes b)
    {
        return Vector3.Distance(
            a.transform.position,
            b.transform.position);
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

    PathNodeData GetLowestFCost()
    {
        PathNodeData best = openList[0];

        foreach (PathNodeData n in openList)
        {
            if (n.fCost < best.fCost)
            {
                best = n;
            }
        }

        return best;
    }

    List<Vector3> BuildPath(PathNodeData endNode)
    {
        List<Vector3> path = new List<Vector3>();

        PathNodeData current = endNode;

        while (current != null)
        {
            path.Add(current.node.transform.position);
            current = current.parent;
        }

        path.Reverse();

        return path;
    }
}
