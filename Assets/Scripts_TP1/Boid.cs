using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 4f;
    public float maxForce = 3f;
    Vector3 velocity;

    [Header("Perception")]
    public float neighborRadius = 3f;
    public float separationRadius = 1f;

    [Header("References")]
    public BoidManager manager;
    public GameBounds gameBounds;

    [Header("Decision thresholds")]
    public float foodDetectRadius = 6f;
    public float hunterDetectRadius = 6f;

    Transform targetFood;
    Transform hunterTransform;

    void Start()
    {
        velocity = transform.forward * (maxSpeed * 0.5f);

        if (gameBounds == null && manager != null)
            gameBounds = manager.gameBounds;
    }

    void Update()
    {
        if (manager != null)
        {
            targetFood = FindClosestInLayer(manager.GetLayerObjects(1 << LayerMask.NameToLayer("Food")), foodDetectRadius);
            hunterTransform = FindClosestInLayer(manager.GetLayerObjects(1 << LayerMask.NameToLayer("Hunter")), hunterDetectRadius);
        }

        Vector3 acceleration = Vector3.zero;

        if (hunterTransform != null)
        {
            acceleration += Evade(hunterTransform.position);
            acceleration += Flocking() * 0.2f;
        }
        else if (targetFood != null)
        {
            acceleration += Arrive(targetFood.position);
            acceleration += Flocking() * 0.3f;
        }
        else if (HasNeighbors())
        {
            acceleration += Flocking();
        }
        else
        {
            acceleration += Wander();
        }

        velocity += acceleration * Time.deltaTime;
        velocity = Utils.Limit(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        // Fijar altura
        Vector3 pos = transform.position;
        pos.y = 0f;

        if (gameBounds != null)
            pos = gameBounds.ClampPosition(pos);

        transform.position = pos;

        if (velocity.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(velocity);
    }

    Transform FindClosestInLayer(List<GameObject> objects, float radius)
    {
        Transform best = null;
        float bestDistSqr = radius * radius;

        foreach (var obj in objects)
        {
            if (obj == null) continue;
            float distSqr = (obj.transform.position - transform.position).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                best = obj.transform;
                bestDistSqr = distSqr;
            }
        }

        return best;
    }

    bool HasNeighbors()
    {
        float rSqr = neighborRadius * neighborRadius;
        foreach (var other in manager.boids)
        {
            if (other == null || other == gameObject) continue;
            if ((other.transform.position - transform.position).sqrMagnitude <= rSqr)
                return true;
        }
        return false;
    }

    Vector3 Flocking()
    {
        Vector3 sep = Separation() * 3.0f;
        Vector3 ali = Alignment() * 1.0f;
        Vector3 coh = Cohesion() * 1.0f;

        Vector3 steer = sep + ali + coh;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    Vector3 Separation()
    {
        Vector3 steer = Vector3.zero;
        int count = 0;
        float rSqr = separationRadius * separationRadius;

        foreach (var other in manager.boids)
        {
            if (other == null || other == gameObject) continue;
            float distSqr = (other.transform.position - transform.position).sqrMagnitude;
            if (distSqr <= rSqr)
            {
                Vector3 diff = transform.position - other.transform.position;
                steer += diff.normalized / Mathf.Sqrt(distSqr);
                count++;
            }
        }

        if (count > 0)
        {
            steer /= count;
            steer = steer.normalized * maxSpeed - velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
        }

        return steer;
    }

    Vector3 Alignment()
    {
        Vector3 sumVel = Vector3.zero;
        int count = 0;
        float rSqr = neighborRadius * neighborRadius;

        foreach (var other in manager.boids)
        {
            if (other == null || other == gameObject) continue;
            if ((other.transform.position - transform.position).sqrMagnitude <= rSqr)
            {
                Boid b = other.GetComponent<Boid>();
                if (b == null) continue;
                sumVel += b.GetVelocity();
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        Vector3 avg = sumVel / count;
        Vector3 desired = avg.normalized * maxSpeed;
        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    Vector3 Cohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;
        float rSqr = neighborRadius * neighborRadius;

        foreach (var other in manager.boids)
        {
            if (other == null || other == gameObject) continue;
            if ((other.transform.position - transform.position).sqrMagnitude <= rSqr)
            {
                center += other.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;
        center /= count;
        return Seek(center);
    }

    Vector3 Seek(Vector3 target)
    {
        Vector3 desired = (target - transform.position).normalized * maxSpeed;
        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    Vector3 Arrive(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        float dist = toTarget.magnitude;
        if (dist < 0.001f) return Vector3.zero;
        float slowingDistance = 1.5f;
        float ramped = maxSpeed * (dist / slowingDistance);
        float clamped = Mathf.Min(ramped, maxSpeed);
        Vector3 desired = toTarget * (clamped / dist);
        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    Vector3 Evade(Vector3 predatorPos)
    {
        Vector3 desired = transform.position - predatorPos;
        desired = desired.normalized * maxSpeed;
        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce * 1.2f);
    }

    Vector3 Wander()
    {
        Vector3 jitter = new Vector3(
            Mathf.PerlinNoise(Time.time, transform.position.y) - 0.5f,
            0,
            Mathf.PerlinNoise(transform.position.x, Time.time) - 0.5f
        ) * 0.5f;
        return Vector3.ClampMagnitude(jitter, maxForce * 0.5f);
    }

    public Vector3 GetVelocity() => velocity;
}