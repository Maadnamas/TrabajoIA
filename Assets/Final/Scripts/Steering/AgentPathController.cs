using UnityEngine;

public class AgentPathController : MonoBehaviour
{
    public Transform target;

    public LayerMask obstacleMask;

    private ThetaStar pathfinder;
    private AgentMovement movement;

    void Start()
    {
        movement = GetComponent<AgentMovement>();
        pathfinder = new ThetaStar(obstacleMask);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            MoveToClickPoint();
        }
    }

    void MoveToClickPoint()
    {
        Ray ray =
            Camera.main.ScreenPointToRay(
                Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var path =
                pathfinder.FindPath(
                    transform.position,
                    hit.point);

            if (path != null)
            {
                movement.SetPath(path);
            }
        }
    }
}
