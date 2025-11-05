using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FieldOfView))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 0.3f;

    [Header("Ruta de patrulla")]
    public List<Node> patrolNodes = new List<Node>();
    private int currentPatrolIndex = 0;

    [Header("Estados")]
    public bool isChasing = false;
    public bool isAlerted = false;

    private Rigidbody rb;
    private FieldOfView fov;
    private AStarPathfinder pathfinder;
    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;
    private Graph graph;
    private Vector3 targetPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fov = GetComponent<FieldOfView>();
        pathfinder = FindObjectOfType<AStarPathfinder>();
        graph = FindObjectOfType<Graph>();

        GoToNextPatrolNode();
    }

    void Update()
    {
        if (fov.canSeePlayer)
        {
            if (!isChasing)
            {
                isChasing = true;
                isAlerted = true;
                AlertAllies(fov.player.position);
            }
            ChasePlayer();
        }
        else if (isChasing)
        {
            if (Vector3.Distance(transform.position, fov.player.position) > fov.viewRadius * 1.5f)
            {
                isChasing = false;
                GoToNextPatrolNode();
            }
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (currentPath.Count == 0) return;

        MoveAlongPath();

        float dist = Vector3.Distance(transform.position, currentPath[currentPathIndex].transform.position);
        if (dist <= stoppingDistance)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPath.Count)
            {
                GoToNextPatrolNode();
            }
        }
    }

    void ChasePlayer()
    {
        Node startNode = graph.GetClosestNode(transform.position);
        Node targetNode = graph.GetClosestNode(fov.player.position);
        currentPath = pathfinder.FindPath(startNode, targetNode);
        currentPathIndex = 0;

        if (currentPath != null && currentPath.Count > 0)
        {
            MoveAlongPath();
        }
    }

    void MoveAlongPath()
    {
        if (currentPath == null || currentPathIndex >= currentPath.Count) return;

        Vector3 target = currentPath[currentPathIndex].transform.position;
        Vector3 direction = (target - transform.position).normalized;
        rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
    }

    void GoToNextPatrolNode()
    {
        if (patrolNodes.Count == 0) return;

        Node startNode = graph.GetClosestNode(transform.position);
        Node nextNode = patrolNodes[currentPatrolIndex];
        currentPath = pathfinder.FindPath(startNode, nextNode);
        currentPathIndex = 0;

        currentPatrolIndex++;
        if (currentPatrolIndex >= patrolNodes.Count)
            currentPatrolIndex = 0;
    }

    void AlertAllies(Vector3 playerPosition)
    {
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in allEnemies)
        {
            if (enemy != this)
            {
                enemy.ReceiveAlert(playerPosition);
            }
        }
    }

    public void ReceiveAlert(Vector3 playerPosition)
    {
        if (!isAlerted)
        {
            isAlerted = true;
            Node startNode = graph.GetClosestNode(transform.position);
            Node targetNode = graph.GetClosestNode(playerPosition);
            currentPath = pathfinder.FindPath(startNode, targetNode);
            currentPathIndex = 0;
        }
    }
}
