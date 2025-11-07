using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FieldOfView))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Alert, Return }

    [Header("Movimiento")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 8f;
    public float stoppingDistance = 0.5f;

    [Header("Ruta de patrulla (en bucle)")]
    public List<Node> patrolNodes = new List<Node>();
    private int patrolIndex = 0;

    [Header("Busqueda")]
    public float searchDuration = 4f; // segundos que se queda buscando

    private Rigidbody rb;
    private FieldOfView fov;
    private Graph graph;
    private AStarPathfinder pathfinder;

    private List<Node> path = new List<Node>();
    private int pathIndex = 0;

    private Vector3 lastKnownPlayerPos;
    private float searchTimer = 0f;
    private float repathTimer = 0f;

    private const float repathInterval = 0.8f; // recalcula ruta cada 0.8 s

    public State currentState = State.Patrol;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        fov = GetComponent<FieldOfView>();
        graph = FindObjectOfType<Graph>();
        pathfinder = FindObjectOfType<AStarPathfinder>();

        if (AlertManager.Instance != null)
            AlertManager.Instance.Register(this);
    }

    private void Start()
    {
        MoveToNextPatrolNode();
    }

    private void OnDestroy()
    {
        if (AlertManager.Instance != null)
            AlertManager.Instance.Unregister(this);
    }

    private void Update()
    {
        // --------- DETECCIÓN DEL PLAYER ---------
        if (fov.canSeePlayer && fov.player != null)
        {
            lastKnownPlayerPos = fov.player.position;
            if (currentState != State.Chase)
            {
                currentState = State.Chase;
                if (AlertManager.Instance != null)
                    AlertManager.Instance.BroadcastAlert(lastKnownPlayerPos, 6f);
            }
        }

        // --------- FSM ---------
        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Alert: Alert(); break;
            case State.Return: Return(); break;
        }
    }

    // ---------------- ESTADOS ----------------

    private void Patrol()
    {
        if (path == null || path.Count == 0)
        {
            MoveToNextPatrolNode();
            return;
        }

        MoveAlongPath();

        // cuando llega al destino, pasa al siguiente nodo
        if (pathIndex >= path.Count)
        {
            MoveToNextPatrolNode();
        }
    }

    private void Chase()
    {
        if (fov.player == null)
        {
            // si perdió referencia, pasa a alerta
            currentState = State.Alert;
            MoveToPosition(lastKnownPlayerPos);
            searchTimer = 0f;
            return;
        }

        // recalcular ruta cada cierto tiempo para optimizar
        repathTimer += Time.deltaTime;
        if (repathTimer >= repathInterval)
        {
            repathTimer = 0f;
            MoveToPosition(fov.player.position);
        }

        if (path == null || path.Count == 0) return;

        MoveAlongPath();

        // Si lo pierde de vista, pasa a alerta
        if (!fov.canSeePlayer)
        {
            currentState = State.Alert;
            MoveToPosition(lastKnownPlayerPos);
            searchTimer = 0f;
        }
    }

    private void Alert()
    {
        // llegó a la última posición conocida
        if (path == null || path.Count == 0)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= searchDuration)
            {
                currentState = State.Return;
                MoveToClosestPatrolNode();
            }
            return;
        }

        MoveAlongPath();

        if (pathIndex >= path.Count)
        {
            path = null;
            pathIndex = 0;
        }
    }

    private void Return()
    {
        if (path == null || path.Count == 0)
        {
            currentState = State.Patrol;
            MoveToNextPatrolNode();
            return;
        }

        MoveAlongPath();

        if (pathIndex >= path.Count)
        {
            currentState = State.Patrol;
            MoveToNextPatrolNode();
        }
    }

    // ---------------- MOVIMIENTO ----------------

    private void MoveAlongPath()
    {
        if (path == null || pathIndex >= path.Count) return;

        Vector3 target = path[pathIndex].transform.position;
        Vector3 dir = (target - transform.position);
        dir.y = 0;
        Vector3 moveDir = dir.normalized;

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target) <= stoppingDistance)
        {
            pathIndex++;
        }
    }

    // ---------------- RUTAS ----------------

    private void MoveToNextPatrolNode()
    {
        if (patrolNodes.Count == 0) return;

        Node start = graph.GetClosestNode(transform.position);
        Node target = patrolNodes[patrolIndex];
        patrolIndex = (patrolIndex + 1) % patrolNodes.Count;

        path = pathfinder.FindPath(start, target);
        pathIndex = 0;
    }

    private void MoveToClosestPatrolNode()
    {
        if (patrolNodes.Count == 0) return;

        Node closest = patrolNodes[0];
        float min = Vector3.Distance(transform.position, closest.transform.position);
        for (int i = 1; i < patrolNodes.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, patrolNodes[i].transform.position);
            if (dist < min) { min = dist; closest = patrolNodes[i]; }
        }

        Node start = graph.GetClosestNode(transform.position);
        path = pathfinder.FindPath(start, closest);
        pathIndex = 0;
    }

    private void MoveToPosition(Vector3 worldPos)
    {
        Node start = graph.GetClosestNode(transform.position);
        Node target = graph.GetClosestNode(worldPos);
        path = pathfinder.FindPath(start, target);
        pathIndex = 0;
    }

    // ---------------- ALERTA GLOBAL ----------------

    public void OnAlert(Vector3 pos, float duration)
    {
        if (currentState == State.Chase) return; // el que ve al player no se mueve a la alerta
        lastKnownPlayerPos = pos;
        currentState = State.Alert;
        MoveToPosition(pos);
        searchTimer = 0f;
    }
}
