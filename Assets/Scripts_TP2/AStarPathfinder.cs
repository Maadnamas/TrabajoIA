using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    private Graph graph;

    [Header("Debug")]
    public bool drawDebugPaths = true;
    public Color debugColor = Color.magenta;

    private List<Node> lastPath = new List<Node>();

    private void Awake()
    {
        graph = FindObjectOfType<Graph>();
    }

    public List<Node> FindPath(Node startNode, Node targetNode)
    {
        if (startNode == null || targetNode == null)
        {
            Debug.LogWarning("❌ A* Error: Nodo inicial o destino nulo.");
            return new List<Node>();
        }

        ResetNodeCosts();

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.gCost = 0;
        startNode.hCost = Vector3.Distance(startNode.transform.position, targetNode.transform.position);
        openSet.Add(startNode);

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
            {
                List<Node> finalPath = RetracePath(startNode, targetNode);
                lastPath = finalPath;

                if (drawDebugPaths) DrawDebugPath(finalPath);
                return finalPath;
            }

            foreach (Node neighbor in currentNode.connections)
            {
                if (neighbor == null || closedSet.Contains(neighbor))
                    continue;

                float tentativeGCost = currentNode.gCost + Vector3.Distance(currentNode.transform.position, neighbor.transform.position);

                if (tentativeGCost < neighbor.gCost)
                {
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = Vector3.Distance(neighbor.transform.position, targetNode.transform.position);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        Debug.LogWarning("⚠️ A*: no se encontró camino entre " + startNode.name + " y " + targetNode.name);
        return new List<Node>();
    }

    private void ResetNodeCosts()
    {
        foreach (Node n in graph.nodes)
        {
            n.gCost = Mathf.Infinity;
            n.hCost = Mathf.Infinity;
            n.parent = null;
        }
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != null && currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Add(startNode);
        path.Reverse();
        return path;
    }

    private void DrawDebugPath(List<Node> path)
    {
        if (path == null || path.Count < 2) return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(
                path[i].transform.position + Vector3.up * 0.2f,
                path[i + 1].transform.position + Vector3.up * 0.2f,
                debugColor, 1.0f
            );
        }
    }
}
