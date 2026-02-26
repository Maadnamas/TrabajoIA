using UnityEngine;

public class BoidFSM : AgentFSM
{
    private LeaderFollowBehavior follow;
    private AgentMovement movement;

    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;

    private float lastAttack;

    protected override void Awake()
    {
        base.Awake();

        follow = GetComponent<LeaderFollowBehavior>();
        movement = GetComponent<AgentMovement>();
    }

    protected override void OnFollow()
    {
        follow.enabled = true;
    }

    protected override void OnAttack()
    {
        follow.enabled = false;

        Transform enemy = fov.GetNearestEnemy();

        if (enemy == null) return;

        float dist =
            Vector3.Distance(
                transform.position,
                enemy.position);

        if (dist > attackRange)
        {
            movement.SetPath(
                new System.Collections.Generic.List<Vector3>
                {
                    enemy.position
                });
        }
        else
        {
            TryAttack(enemy);
        }
    }

    protected override void OnFlee()
    {
        follow.enabled = false;

        Transform enemy = fov.GetNearestEnemy();

        // Si no ve enemigo, huye en dirección opuesta al líder
        if (enemy == null)
        {
            Vector3 randomDir =
                Random.insideUnitSphere;

            randomDir.y = 0;

            Vector3 fleePos =
                transform.position + randomDir * 5f;

            movement.SetPath(
                new System.Collections.Generic.List<Vector3>
                {
                fleePos
                });

            return;
        }

        Vector3 dir =
            (transform.position - enemy.position).normalized;

        Vector3 fleePosition =
            transform.position + dir * 6f;

        movement.SetPath(
            new System.Collections.Generic.List<Vector3>
            {
            fleePosition
            });
    }

    void TryAttack(Transform enemy)
    {
        if (Time.time - lastAttack < attackCooldown)
            return;

        lastAttack = Time.time;

        AgentFSM other =
            enemy.GetComponentInParent<AgentFSM>();

        if (other == null)
            return;

        other.TakeDamage(attackDamage);
    }
}
