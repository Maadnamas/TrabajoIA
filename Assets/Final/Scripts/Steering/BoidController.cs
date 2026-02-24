using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentMovement))]
public class BoidFlockingController : MonoBehaviour
{
    public float neighborRadius = 3f;

    public LayerMask boidMask;

    private AgentMovement movement;
    private SeparationBehavior separation;
    private LeaderFollowBehavior follow;

    void Awake()
    {
        movement = GetComponent<AgentMovement>();
        separation = GetComponent<SeparationBehavior>();
        follow = GetComponent<LeaderFollowBehavior>();
    }

    void Update()
    {
        Vector3 flockForce = CalculateFlocking();

        if (flockForce != Vector3.zero)
        {
            List<Vector3> tempPath =
                new List<Vector3>();

            tempPath.Add(
                transform.position + flockForce);

            movement.SetPath(tempPath);
        }
    }

    Vector3 CalculateFlocking()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                neighborRadius,
                boidMask);

        List<Vector3> neighbors =
            new List<Vector3>();

        foreach (var h in hits)
        {
            if (h.gameObject != gameObject)
            {
                neighbors.Add(
                    h.transform.position);
            }
        }

        Vector3 sepForce =
            separation.Calculate(
                neighbors.ToArray());

        Vector3 followForce =
            follow.Calculate();

        Vector3 final =
            sepForce + followForce;

        return final;
    }
}
