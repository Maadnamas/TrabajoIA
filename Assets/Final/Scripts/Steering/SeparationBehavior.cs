using UnityEngine;

public class SeparationBehavior : MonoBehaviour
{
    public float separationRadius = 2f;
    public float separationStrength = 3f;

    public Vector3 Calculate(Vector3[] neighbors)
    {
        Vector3 force = Vector3.zero;
        int count = 0;

        foreach (var pos in neighbors)
        {
            float dist = Vector3.Distance(
                transform.position,
                pos);

            if (dist > 0 && dist < separationRadius)
            {
                Vector3 dir =
                    transform.position - pos;

                force += dir.normalized / dist;
                count++;
            }
        }

        if (count > 0)
        {
            force /= count;
            force *= separationStrength;
        }

        return force;
    }
}
