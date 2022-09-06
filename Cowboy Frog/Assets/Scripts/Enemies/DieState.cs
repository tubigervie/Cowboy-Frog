using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState : AIState
{
    public override int CalculateStateCost()
    {
        return int.MaxValue;
    }

    public override void OnEnter()
    {
        owner.BossEnable(false);
        owner.animator.Play(Settings.die);
        Destroy(this.gameObject, 1f);
    }

    public override void OnExit()
    {
        
    }

    public override void StateUpdate(float deltaTime)
    {
        
    }
}
