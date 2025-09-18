using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class IdleState : HunterState
{
    private float timer = 0f;
    private float restTime;

    public IdleState(Hunter hunter, float restDuration) : base(hunter)
    {
        restTime = restDuration;
    }

    public override void Enter()
    {
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= restTime)
        {
            hunter.energy = hunter.maxEnergy;
            hunter.stateMachine.ChangeState(new PatrolState(hunter));
        }
    }

    public override void Exit() { }
}
