using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HunterState
{
    protected Hunter hunter;

    public HunterState(Hunter hunter)
    {
        this.hunter = hunter;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}