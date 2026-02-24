using System.Collections.Generic;
using UnityEngine;

public class PathTester : MonoBehaviour
{
    public Transform target;

    private ThetaStar pathfinder;

    public LayerMask obstacleMask;

    private List<Vector3> path;

    void Start()
    {
        pathfinder = new ThetaStar(obstacleMask);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CalculatePath();
        }
    }

    void CalculatePath()
    {
        path = pathfinder.FindPath(
            transform.position,
            target.position);
    }

    private void OnDrawGizmos()
    {
        if (path == null) return;

        Gizmos.color = Color.green;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
        }
    }
}