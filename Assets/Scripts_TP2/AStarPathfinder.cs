using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    private Graph graph;

    void Awake()
    {
        graph = FindObjectOfType<Graph>();
    }

    public List<Node> FindPath(Node startNode, Node targetNode)
    {
        if (startNode == null || targetNode == null)
            return null;

        foreach (Node n in graph.nodes)
        {
            n.gCost = Mathf.Infinity;
            n.hCost = 0;
            n.parent = null;
        }

        startNode.gCost = 0;
        startNode.hCost = Vector3.Distance(startNode.transform.position, targetNode.transform.position);

        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in currentNode.connections)
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float newCostToNeighbor = currentNode.gCost + Vector3.Distance(currentNode.transform.position, neighbor.transform.position);
                if (newCostToNeighbor < neighbor.gCost)
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = Vector3.Distance(neighbor.transform.position, targetNode.transform.position);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
}
