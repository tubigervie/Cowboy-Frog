using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : AIState
{

    public override void OnEnter()
    {
        stateCooldownTimer = Random.Range(stateCooldownMin, stateCooldownMax);
    }

    public override void OnExit()
    {
    }

    public override void StateUpdate(float deltaTime)
    {
        if(stateCooldownTimer <= 0f)
            owner.ChooseState();
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override int CalculateStateCost()
    {
        int cost = int.MaxValue - 1;
        return cost;
    }
}
