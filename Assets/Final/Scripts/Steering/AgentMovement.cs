using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AgentMovement : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float rotationSpeed = 10f;
    public float arriveRadius = 0.5f;

    private CharacterController controller;

    private List<Vector3> path;
    private int currentIndex = 0;

    private Vector3 velocity;
    private Vector3 externalForce;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (path != null && path.Count > 0)
        {
            MoveAlongPath();
        }

        ApplyExternalForce();
    }

    public void SetPath(List<Vector3> newPath)
    {
        path = newPath;
        currentIndex = 0;
    }

    void MoveAlongPath()
    {
        if (currentIndex >= path.Count)
            return;

        Vector3 target = path[currentIndex];
        Vector3 dir = target - transform.position;
        dir.y = 0;

        float dist = dir.magnitude;

        if (dist < arriveRadius)
        {
            currentIndex++;
            return;
        }

        Vector3 desiredVelocity =
            dir.normalized * maxSpeed;

        Vector3 steering =
            desiredVelocity - velocity;

        velocity += steering * Time.deltaTime;

        velocity = Vector3.ClampMagnitude(
            velocity,
            maxSpeed);

        controller.Move(
            velocity * Time.deltaTime);

        RotateToMovement();
    }

    void RotateToMovement()
    {
        if (velocity.magnitude < 0.05f)
            return;

        Vector3 flatVel = velocity;
        flatVel.y = 0;

        if (flatVel == Vector3.zero)
            return;

        Quaternion targetRot =
            Quaternion.LookRotation(flatVel.normalized);

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * 200f * Time.deltaTime);
    }
    public void AddExternalForce(Vector3 force)
    {
        externalForce += force;
    }

    void ApplyExternalForce()
    {
        if (externalForce == Vector3.zero)
            return;

        velocity += externalForce * Time.deltaTime;

        externalForce = Vector3.zero;
    }
}
