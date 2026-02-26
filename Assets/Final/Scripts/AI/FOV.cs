using System.Collections.Generic;
using UnityEngine;

public class FOV : MonoBehaviour
{
    public float viewRadius = 8f;
    [Range(0, 360)]
    public float viewAngle = 120f;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    void Update()
    {
        FindVisibleTargets();
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();

        Collider[] targets =
            Physics.OverlapSphere(
                transform.position,
                viewRadius,
                targetMask);

        foreach (Collider c in targets)
        {
            Transform target = c.transform;

            Vector3 dir =
                (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dir) < viewAngle / 2)
            {
                float dist =
                    Vector3.Distance(
                        transform.position,
                        target.position);

                if (!Physics.Raycast(
                    transform.position,
                    dir,
                    dist,
                    obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    public bool CanSeeEnemy()
    {
        return visibleTargets.Count > 0;
    }

    public Transform GetNearestEnemy()
    {
        Transform best = null;
        float minDist = Mathf.Infinity;

        foreach (Transform t in visibleTargets)
        {
            float d =
                Vector3.Distance(
                    transform.position,
                    t.position);

            if (d < minDist)
            {
                minDist = d;
                best = t;
            }
        }

        return best;
    }
}
