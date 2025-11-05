using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class HuntingState : HunterState
{
    private Transform targetBoid;
    private float shootCooldown = 1f;
    private float shootTimer = 0f;
    private float shootingDistance = 8f; // distancia a la que decide disparar

    public HuntingState(Hunter hunter, Transform target) : base(hunter)
    {
        targetBoid = target;
    }

    public override void Enter()
    {
        shootTimer = shootCooldown;
    }

    public override void Update()
    {
        if (targetBoid == null)
        {
            hunter.stateMachine.ChangeState(new PatrolState(hunter));
            return;
        }

        hunter.energy -= Time.deltaTime;

        float dist = Vector3.Distance(hunter.transform.position, targetBoid.position);

        if (dist > shootingDistance)
        {
            Vector3 predictedPos = PredictPosition(targetBoid);
            hunter.MoveTowards(predictedPos);
        }
        else
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootCooldown;
                hunter.energy -= 1f;
            }
        }

        if (hunter.energy <= 0)
        {
            hunter.stateMachine.ChangeState(new IdleState(hunter, hunter.restDuration));
        }

        if (dist > hunter.visionRange)
        {
            hunter.stateMachine.ChangeState(new PatrolState(hunter));
        }
    }

    public override void Exit() { }

    private Vector3 PredictPosition(Transform target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            return target.position + rb.velocity * 0.5f;
        }
        return target.position;
    }

    private void Shoot()
    {
        if (hunter.bulletPrefab == null) return;

        GameObject bullet = Object.Instantiate(
            hunter.bulletPrefab,
            hunter.transform.position,
            Quaternion.identity
        );

        if (targetBoid != null)
        {
            Vector3 direction = (targetBoid.position - hunter.transform.position).normalized;
            bullet.transform.forward = direction;
        }
    }
}