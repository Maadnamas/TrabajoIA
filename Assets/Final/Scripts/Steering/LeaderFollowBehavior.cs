using UnityEngine;

public class LeaderFollowBehavior : MonoBehaviour
{
    public Transform leader;

    public float followDistance = 2.5f;
    public float arriveStrength = 2f;

    public Vector3 Calculate()
    {
        if (leader == null)
            return Vector3.zero;

        Vector3 targetPos =
            leader.position - leader.forward * followDistance;

        Vector3 dir =
            targetPos - transform.position;

        dir.y = 0;

        float dist = dir.magnitude;

        if (dist < 0.1f)
            return Vector3.zero;

        Vector3 desired =
            dir.normalized * arriveStrength;

        return desired;
    }
}
