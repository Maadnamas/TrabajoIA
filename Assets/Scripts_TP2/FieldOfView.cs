using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius = 8f;
    [Range(0, 360)] public float viewAngle = 120f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector] public bool canSeePlayer = false;
    [HideInInspector] public Transform player;

    private void Awake()
    {
        TryFindPlayer();
    }

    private void TryFindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) TryFindPlayer();
        FindVisibleTargets();
    }

    void FindVisibleTargets()
    {
        canSeePlayer = false;
        if (player == null) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= viewRadius)
        {
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 0.8f, dirToPlayer, dist, obstacleMask))
                {
                    canSeePlayer = true;
                    return;
                }
            }
        }
        canSeePlayer = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Vector3 l = DirFromAngle(-viewAngle / 2, false) * viewRadius;
        Vector3 r = DirFromAngle(viewAngle / 2, false) * viewRadius;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + l);
        Gizmos.DrawLine(transform.position, transform.position + r);
        if (canSeePlayer && player != null) { Gizmos.color = Color.red; Gizmos.DrawLine(transform.position, player.position); }
    }

    public Vector3 DirFromAngle(float angleDeg, bool global)
    {
        if (!global) angleDeg += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleDeg * Mathf.Deg2Rad), 0, Mathf.Cos(angleDeg * Mathf.Deg2Rad));
    }
}