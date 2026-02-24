using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class PatrolState : HunterState
{
    private int currentWaypoint = 0;

    public PatrolState(Hunter hunter) : base(hunter) { }
    public override void Enter() { }

    public override void Update()
    {
        if (hunter.waypoints.Length == 0) return;

        Vector3 target = hunter.waypoints[currentWaypoint].position;
        hunter.MoveTowards(target);

        if (Vector3.Distance(hunter.transform.position, target) < 0.3f)
        {
            currentWaypoint = (currentWaypoint + 1) % hunter.waypoints.Length;
        }

        Transform boid = hunter.GetClosestBoid();
        if (boid != null)
            hunter.stateMachine.ChangeState(new HuntingState(hunter, boid));
    }

    public override void Exit() { }
}
