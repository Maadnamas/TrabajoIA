using UnityEngine;

[RequireComponent(typeof(FOV))]
public class AgentFSM : MonoBehaviour
{
    public AgentState currentState = AgentState.Idle;

    public float maxHP = 100f;
    public float currentHP;

    protected FOV fov;

    protected virtual void Awake()
    {
        fov = GetComponent<FOV>();
        currentHP = maxHP;
    }

    protected virtual void Update()
    {
        UpdateState();
        HandleState();
    }

    protected virtual void UpdateState()
    {
        if (currentHP <= maxHP * 0.3f)
        {
            currentState = AgentState.Flee;
            return;
        }

        if (fov.CanSeeEnemy())
        {
            currentState = AgentState.Attack;
            return;
        }

        currentState = AgentState.Follow;
    }

    protected virtual void HandleState()
    {
        switch (currentState)
        {
            case AgentState.Idle:
                OnIdle();
                break;

            case AgentState.Follow:
                OnFollow();
                break;

            case AgentState.Attack:
                OnAttack();
                break;

            case AgentState.Flee:
                OnFlee();
                break;
        }
    }

    protected virtual void OnIdle() { }
    protected virtual void OnFollow() { }
    protected virtual void OnAttack() { }
    protected virtual void OnFlee() { }

    public void TakeDamage(float dmg)
    {
        if (currentState == AgentState.Dead) return;

        currentHP -= dmg;
        currentHP = Mathf.Max(currentHP, 0);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        currentState = AgentState.Dead;

        AgentMovement move = GetComponent<AgentMovement>();

        if (move != null)
        {
            move.enabled = false;
        }

        this.enabled = false;
    }
}
