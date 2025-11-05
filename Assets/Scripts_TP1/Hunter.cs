using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HunterStates { Idle, Patrol, Hunting }

public class Hunter : MonoBehaviour
{
    public GameObject bulletPrefab;

    public float moveSpeed = 3f;
    public float visionRange = 8f;
    public float energy = 10f;
    public float maxEnergy = 10f;
    public float restDuration = 3f;

    public Transform[] waypoints;

    [HideInInspector] public StateMachine stateMachine;

    public GameBounds gameBounds;

    public BoidManager manager; // referencia al manager

    private void Start()
    {
        // Iniciar state machine
        stateMachine = new StateMachine();
        stateMachine.ChangeState(new PatrolState(this));

        // Registrar en el manager
        if (manager == null)
            manager = FindObjectOfType<BoidManager>();

        if (manager != null)
            manager.AddHunter(this.gameObject);
    }

    private void Update()
    {
        stateMachine.Update();

        Vector3 pos = transform.position;
        pos.y = 0f;
        transform.position = pos;
    }

    public void MoveTowards(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );
    }

    public Transform GetClosestBoid()
    {
        GameObject[] boids = GameObject.FindGameObjectsWithTag("Boid");
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var boid in boids)
        {
            float dist = Vector3.Distance(transform.position, boid.transform.position);
            if (dist < minDist && dist <= visionRange)
            {
                minDist = dist;
                closest = boid.transform;
            }
        }
        return closest;
    }
}
