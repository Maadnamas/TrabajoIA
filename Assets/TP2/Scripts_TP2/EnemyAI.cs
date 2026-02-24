using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FieldOfView))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Alert, Return }

    [Header("Movimiento")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 0.5f;
    public float rotationSpeed = 8f;

    [Header("Patrulla")]
    public List<Node> patrolNodes = new List<Node>();
    private int patrolIndex = 0;

    [Header("Búsqueda")]
    public float searchDuration = 4f;

    private FieldOfView fov;
    private Graph graph;
    private AStarPathfinder pathfinder;

    private List<Node> path = null;
    private int pathIndex = 0;

    public State currentState = State.Patrol;
    private Vector3 lastKnownPlayerPos;
    private float searchTimer = 0f;
    private float repathTimer = 0f;
    private const float repathInterval = 0.6f;

    private float alertBroadcastTimer = 0f;
    private const float alertBroadcastInterval = 1.0f;

    private void Awake()
    {
        fov = GetComponent<FieldOfView>();
        graph = FindObjectOfType<Graph>();
        pathfinder = FindObjectOfType<AStarPathfinder>();

        if (AlertManager.Instance != null)
        {
            AlertManager.Instance.Register(this);
            Debug.Log($"[EnemyAI] {name} registrado en AlertManager en Awake.");
        }
    }

    private void Start()
    {
        if (AlertManager.Instance != null) AlertManager.Instance.Register(this);
        MoveToNextPatrolNode();
    }

    private void OnDestroy()
    {
        if (AlertManager.Instance != null) AlertManager.Instance.Unregister(this);
    }

    private void Update()
    {
        if (fov.canSeePlayer && fov.player != null)
        {
            lastKnownPlayerPos = fov.player.position;

            if (currentState != State.Chase)
            {
                currentState = State.Chase;
                Debug.Log($"[EnemyAI] {name} vio al player → CHASE");
            }

            alertBroadcastTimer += Time.deltaTime;
            if (alertBroadcastTimer >= alertBroadcastInterval)
            {
                alertBroadcastTimer = 0f;
                if (AlertManager.Instance != null)
                    AlertManager.Instance.BroadcastAlert(lastKnownPlayerPos, searchDuration);
            }
        }

        switch (currentState)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase: UpdateChase(); break;
            case State.Alert: UpdateAlert(); break;
            case State.Return: UpdateReturn(); break;
        }
    }

    void UpdatePatrol()
    {
        if (!HasPath()) { MoveToNextPatrolNode(); return; }
        FollowPath();
        if (pathIndex >= path.Count) MoveToNextPatrolNode();
    }

    void UpdateChase()
    {
        if (fov.canSeePlayer && fov.player != null)
        {
            Vector3 dir = (fov.player.position - transform.position);
            dir.y = 0;
            Vector3 moveDir = dir.normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            if (moveDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), rotationSpeed * Time.deltaTime);
            }
            lastKnownPlayerPos = fov.player.position;
            return;
        }

        currentState = State.Alert;
        AttemptMoveToVisibleNode(lastKnownPlayerPos);
        searchTimer = 0f;
        Debug.Log($"[EnemyAI] {name} perdió al player → ALERT");
    }

    void UpdateAlert()
    {
        if (!HasPath())
        {
            searchTimer += Time.deltaTime;
            transform.Rotate(Vector3.up * rotationSpeed * 50f * Time.deltaTime);

            if (searchTimer >= searchDuration)
            {
                currentState = State.Return;
                MoveToClosestPatrolNode();
                Debug.Log($"[EnemyAI] {name} terminó de buscar → RETURN");
            }
            return;
        }

        FollowPath();

        if (pathIndex >= path.Count)
        {
            path = null;
            pathIndex = 0;
            searchTimer = 0f;
        }
    }

    void UpdateReturn()
    {
        if (!HasPath())
        {
            MoveToNextPatrolNode();
            currentState = State.Patrol;
            return;
        }
        FollowPath();
        if (pathIndex >= path.Count)
        {
            currentState = State.Patrol;
            MoveToNextPatrolNode();
        }
    }

    bool HasPath() => path != null && path.Count > 0 && pathIndex < path.Count;

    void FollowPath()
    {
        if (!HasPath()) return;

        Vector3 target = path[pathIndex].transform.position;
        Vector3 dir = (target - transform.position);
        dir.y = 0;
        Vector3 moveDir = dir.normalized;

        transform.position += moveDir * moveSpeed * Time.deltaTime;
        if (moveDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target) <= stoppingDistance)
            pathIndex++;
    }

    bool IsPositionVisibleFrom(Vector3 worldPos, Vector3 fromPosition)
    {
        Vector3 dir = (worldPos - fromPosition);
        float dist = dir.magnitude;
        if (dist < 0.001f) return true;
        dir.Normalize();
        Vector3 origin = fromPosition + Vector3.up * 0.8f;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, fov.obstacleMask))
        {
            return false;
        }
        return true;
    }

    Node FindNodeWithLineOfSightTo(Vector3 worldPos)
    {
        if (graph == null || graph.nodes == null || graph.nodes.Count == 0) return null;

        Node best = null;
        float bestScore = Mathf.Infinity;

        foreach (Node n in graph.nodes)
        {
            if (n == null) continue;

            if (!IsPositionVisibleFrom(worldPos, n.transform.position)) continue;

            float d = Vector3.Distance(transform.position, n.transform.position);

            if (d < bestScore)
            {
                best = n;
                bestScore = d;
            }
        }

        return best;
    }

    void AttemptMoveToVisibleNode(Vector3 worldPos)
    {
        if (IsPositionVisibleFrom(worldPos, transform.position))
        {
            Node closestToTarget = graph.GetClosestNode(worldPos);
            if (closestToTarget != null)
            {
                path = pathfinder.FindPath(graph.GetClosestNode(transform.position), closestToTarget);
                pathIndex = 0;
                return;
            }
        }

        Node visibleNode = FindNodeWithLineOfSightTo(worldPos);

        if (visibleNode != null)
        {
            Node start = graph.GetClosestNode(transform.position);
            List<Node> p = pathfinder.FindPath(start, visibleNode);
            if (p != null && p.Count > 0)
            {
                path = p;
                pathIndex = 0;
                return;
            }
        }

        Node fallback = graph.GetClosestNode(worldPos);
        if (fallback != null)
        {
            List<Node> p2 = pathfinder.FindPath(graph.GetClosestNode(transform.position), fallback);
            if (p2 != null && p2.Count > 0)
            {
                path = p2;
                pathIndex = 0;
                return;
            }
        }

        path = null;
        pathIndex = 0;
    }

    void AttemptMoveTo(Vector3 worldPos)
    {
        AttemptMoveToVisibleNode(worldPos);
    }

    void MoveToNextPatrolNode()
    {
        if (patrolNodes == null || patrolNodes.Count == 0) { path = null; return; }

        Node nextNode = patrolNodes[patrolIndex];
        patrolIndex = (patrolIndex + 1) % patrolNodes.Count;

        if (IsPositionVisibleFrom(nextNode.transform.position, transform.position))
        {
            Node start = graph.GetClosestNode(transform.position);
            List<Node> direct = pathfinder.FindPath(start, nextNode);
            if (direct != null && direct.Count > 0)
            {
                path = direct;
                pathIndex = 0;
                return;
            }
            else
            {
                path = null;
                pathIndex = 0;
                return;
            }
        }
        else
        {
            Node visibleNode = FindNodeWithLineOfSightTo(nextNode.transform.position);
            if (visibleNode != null)
            {
                Node start = graph.GetClosestNode(transform.position);
                List<Node> p = pathfinder.FindPath(start, visibleNode);
                if (p != null && p.Count > 0)
                {
                    path = p;
                    pathIndex = 0;
                    return;
                }
            }

            Node start2 = graph.GetClosestNode(transform.position);
            List<Node> fallback = pathfinder.FindPath(start2, nextNode);
            if (fallback != null && fallback.Count > 0)
            {
                path = fallback;
                pathIndex = 0;
                return;
            }

            path = null;
            pathIndex = 0;
        }
    }

    void MoveToClosestPatrolNode()
    {
        if (patrolNodes == null || patrolNodes.Count == 0) { path = null; return; }
        Node closest = patrolNodes[0];
        float min = Vector3.Distance(transform.position, closest.transform.position);
        for (int i = 1; i < patrolNodes.Count; i++)
        {
            float d = Vector3.Distance(transform.position, patrolNodes[i].transform.position);
            if (d < min) { min = d; closest = patrolNodes[i]; }
        }
        List<Node> p = pathfinder.FindPath(graph.GetClosestNode(transform.position), closest);
        if (p == null || p.Count == 0) { path = null; pathIndex = 0; } else { path = p; pathIndex = 0; }
    }

    public void OnAlert(Vector3 pos, float duration)
    {
        if (currentState == State.Chase) return;

        Debug.Log($"[Alert] {name} recibió alerta → va a {pos}");
        lastKnownPlayerPos = pos;
        currentState = State.Alert;
        AttemptMoveToVisibleNode(pos);
        searchTimer = 0f;
    }
}