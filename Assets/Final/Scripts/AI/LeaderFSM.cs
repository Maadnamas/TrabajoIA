using System.Collections.Generic;
using UnityEngine;

public class LeaderFSM : AgentFSM
{
    private AgentPathController path;

    protected override void Awake()
    {
        base.Awake();
        path = GetComponent<AgentPathController>();
    }

    protected override void OnFollow()
    {
        if (!path.enabled)
            path.enabled = true;
    }

    protected override void OnAttack()
    {
        Transform enemy = fov.GetNearestEnemy();

        if (enemy == null) return;

        path.enabled = false;
    }

    protected override void OnFlee()
    {
        path.enabled = false;

        AgentMovement move =
            GetComponent<AgentMovement>();

        move.enabled = true;

        Transform enemy =
            fov.GetNearestEnemy();

        Vector3 fleeDir;

        if (enemy != null)
        {
            fleeDir =
                (transform.position - enemy.position).normalized;
        }
        else
        {
            fleeDir =
                Random.insideUnitSphere.normalized;
            fleeDir.y = 0;
        }

        Vector3 desiredPos =
            transform.position + fleeDir * 50f;

        ThetaStar theta =
            new ThetaStar(path.obstacleMask);

        var newPath =
            theta.FindPath(
                transform.position,
                desiredPos);

        // Si hay camino → usarlo
        if (newPath != null && newPath.Count > 0)
        {
            move.SetPath(newPath);
        }
        else
        {
            // Fallback: ir directo
            List<Vector3> fallback =
                new List<Vector3>();

            fallback.Add(desiredPos);

            move.SetPath(fallback);
        }
    }
}
