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
        path.enabled = false;

        Transform enemy = fov.GetNearestEnemy();

        if (enemy == null) return;

        GetComponent<AgentMovement>()
            .SetPath(
                new System.Collections.Generic.List<Vector3>
                {
                enemy.position
                });
    }

    protected override void OnFlee()
    {
        path.enabled = false;

        AgentMovement move =
            GetComponent<AgentMovement>();

        if (move == null) return;

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
            transform.position + fleeDir * 60f;

        ThetaStar theta =
            new ThetaStar(path.obstacleMask);

        var newPath =
            theta.FindPath(
                transform.position,
                desiredPos);

        if (newPath != null && newPath.Count > 0)
        {
            move.SetPath(newPath);
        }
    }
}
