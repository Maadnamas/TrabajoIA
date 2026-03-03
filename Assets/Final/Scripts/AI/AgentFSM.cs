using UnityEngine;

[RequireComponent(typeof(FOV))]
public class AgentFSM : MonoBehaviour
{
    public AgentState currentState = AgentState.Idle;

    public float maxHP = 100f;
    public float currentHP;

    [Header("Flee Healing")]
    public float healAmount = 5f;
    public float healInterval = 1.5f;

    protected float lastHealTime;

    protected FOV fov;

    protected bool isDead = false;

    protected virtual void Awake()
    {
        fov = GetComponent<FOV>();
        currentHP = maxHP;
    }

    protected virtual void Update()
    {
        if (isDead) return;

        UpdateState();
        HandleState();
    }

    protected virtual void UpdateState()
    {
        if (currentHP <= 0)
        {
            currentState = AgentState.Dead;
            return;
        }

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
                HealWhileFleeing();
                break;

            case AgentState.Dead:
                Die();
                break;
        }
    }

    protected virtual void OnIdle() { }
    protected virtual void OnFollow() { }
    protected virtual void OnAttack() { }
    protected virtual void OnFlee() { }

    void HealWhileFleeing()
    {
        if (Time.time - lastHealTime < healInterval)
            return;

        lastHealTime = Time.time;

        currentHP += healAmount;
        currentHP = Mathf.Min(currentHP, maxHP);
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHP -= dmg;
        currentHP = Mathf.Max(currentHP, 0);
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        currentState = AgentState.Dead;

        AgentMovement move =
            GetComponent<AgentMovement>();

        if (move != null)
            move.enabled = false;

        LeaderFollowBehavior follow =
            GetComponent<LeaderFollowBehavior>();

        if (follow != null)
            follow.enabled = false;

        this.enabled = false;

        Destroy(gameObject, 1.5f);
    }
}