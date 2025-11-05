using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("Configuración del FOV")]
    public float viewRadius = 8f;
    [Range(0, 360)]
    public float viewAngle = 120f;

    [Header("Referencias")]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector] public bool canSeePlayer = false;
    [HideInInspector] public Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        FindVisibleTargets();
    }

    void FindVisibleTargets()
    {
        canSeePlayer = false;

        if (player == null)
            return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer < viewRadius)
        {
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 1f, dirToPlayer, distToPlayer, obstacleMask))
                {
                    canSeePlayer = true;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

        if (canSeePlayer && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
