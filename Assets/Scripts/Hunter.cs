using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HunterState { Idle, Patrol, Hunting }

public class Hunter : MonoBehaviour
{
    [Header("FSM")]
    public HunterState currentState = HunterState.Patrol;
    public float energy = 100f;
    public float maxEnergy = 100f;
    public float idleRecoverRate = 20f;
    public float patrolDrainRate = 5f;
    public float huntingDrainRate = 12f;
    public float restTimeSeconds = 3f;
    bool resting = false;

    [Header("Patrol")]
    public Transform[] waypoints;
    public bool loopWaypoints = true;
    int currentWaypoint = 0;
    public float patrolSpeed = 3.5f;
    public float waypointArriveDistance = 0.5f;
    public bool reverseOnEnd = false;
    int waypointDirection = 1;

    [Header("Perception & Combat")]
    public float visionRadius = 10f;
    public LayerMask boidLayer;
    public float shootingRange = 8f;
    public GameObject bulletPrefab;
    public Transform shootOrigin;
    public float fireRate = 1f;
    float fireCooldown = 0f;
    public float pursuitPredictionTime = 0.8f;

    [Header("Movement")]
    public float maxSpeed = 5f;
    Vector3 velocity;

    void Update()
    {
        switch (currentState)
        {
            case HunterState.Idle: IdleBehavior(); break;
            case HunterState.Patrol: PatrolBehavior(); break;
            case HunterState.Hunting: HuntingBehavior(); break;
        }

        if (energy <= 0f && currentState != HunterState.Idle && !resting)
        {
            StartCoroutine(GoRest());
        }

        fireCooldown = Mathf.Max(0f, fireCooldown - Time.deltaTime);
    }

    IEnumerator GoRest()
    {
        resting = true;
        currentState = HunterState.Idle;
        float t = 0f;
        while (t < restTimeSeconds)
        {
            t += Time.deltaTime;
            energy += idleRecoverRate * Time.deltaTime;
            yield return null;
        }
        energy = Mathf.Min(energy, maxEnergy);
        resting = false;
        currentState = HunterState.Patrol;
    }

    void IdleBehavior()
    {
        velocity = Vector3.zero;
        energy = Mathf.Min(maxEnergy, energy + idleRecoverRate * Time.deltaTime);
    }

    void PatrolBehavior()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];
        MoveTowards(target.position, patrolSpeed);

        if (Vector3.Distance(transform.position, target.position) < waypointArriveDistance)
        {
            if (reverseOnEnd)
            {
                if (currentWaypoint == waypoints.Length - 1) waypointDirection = -1;
                if (currentWaypoint == 0) waypointDirection = 1;
                currentWaypoint += waypointDirection;
            }
            else
            {
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            }
        }

        Collider[] seen = Physics.OverlapSphere(transform.position, visionRadius, boidLayer);
        if (seen.Length > 0)
        {
            currentState = HunterState.Hunting;
        }

        energy -= patrolDrainRate * Time.deltaTime;
    }

    void HuntingBehavior()
    {
        Collider[] seen = Physics.OverlapSphere(transform.position, visionRadius, boidLayer);
        if (seen.Length == 0)
        {
            currentState = HunterState.Patrol;
            return;
        }

        Transform best = null;
        float bestD = Mathf.Infinity;
        foreach (var c in seen)
        {
            float d = Vector3.SqrMagnitude(c.transform.position - transform.position);
            if (d < bestD)
            {
                best = c.transform;
                bestD = d;
            }
        }

        if (best == null) { currentState = HunterState.Patrol; return; }

        float dist = Vector3.Distance(transform.position, best.position);
        if (dist <= shootingRange && bulletPrefab != null && shootOrigin != null)
        {
            ShootAt(best);
        }
        else
        {
            Pursuit(best);
        }

        energy -= huntingDrainRate * Time.deltaTime;
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 desired = (target - transform.position).normalized * speed;
        velocity = Vector3.Lerp(velocity, desired, Time.deltaTime * 3f);
        transform.position += velocity * Time.deltaTime;
        if (velocity.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(velocity);
    }

    void Pursuit(Transform targetBoid)
    {
        Boid b = targetBoid.GetComponent<Boid>();
        Vector3 targetVel = Vector3.zero;
        if (b != null) targetVel = b.GetVelocity();
        Vector3 predictedPos = targetBoid.position + targetVel * pursuitPredictionTime;
        MoveTowards(predictedPos, maxSpeed);
    }

    void ShootAt(Transform targetBoid)
    {
        if (fireCooldown > 0f) return;
        fireCooldown = 1f / fireRate;

        Boid b = targetBoid.GetComponent<Boid>();
        Vector3 targetVel = Vector3.zero;
        if (b != null) targetVel = b.GetVelocity();

        float bulletSpeed = 15f;
        Vector3 toTarget = targetBoid.position - shootOrigin.position;
        float distance = toTarget.magnitude;
        float timeToHit = distance / bulletSpeed;
        Vector3 aimPoint = targetBoid.position + targetVel * timeToHit;

        Vector3 dir = (aimPoint - shootOrigin.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, shootOrigin.position, Quaternion.LookRotation(dir));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb) rb.velocity = dir * bulletSpeed;

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}
