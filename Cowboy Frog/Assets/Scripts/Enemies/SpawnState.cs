using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnState : AIState
{
    [SerializeField] float additionalTimeToWait = 2f;
    public override void OnEnter()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        owner.animator.Play(Settings.spawn);
        yield return new WaitForSeconds(owner.animator.GetCurrentAnimatorClipInfo(0)[0].clip.length + additionalTimeToWait);
        owner.BossEnable(true);
        owner.ChooseState();
    }

    public override void OnExit()
    {
    }

    public override void StateUpdate(float deltaTime)
    {
    }

    public override int CalculateStateCost()
    {
        int cost = int.MaxValue;
        return cost;
    }

}
