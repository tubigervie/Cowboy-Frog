using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindDemonBeastAttack2State : AIState
{
    [SerializeField] float castTime = 5f;
    float castTimer = 0f;
    [SerializeField] GameObject lightningPrefab;
    [SerializeField] SoundEffectSO castSFX;

    Coroutine stateRoutine;

    public override int CalculateStateCost()
    {
        int stateCost = int.MaxValue;
        if (owner.health.GetHealthPercent() <= minHealthPercent && stateCooldownTimer < 0f)
        {
            float distToPlayer = Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition());
            if (minDistance <= distToPlayer && distToPlayer <= maxDistance)
            {
                stateCost = 0;
            }
        }
        return stateCost;
    }

    private void WeaponSoundEffect()
    {
        if (castSFX != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(castSFX);
        }
    }

    public override void OnEnter()
    {
        castTimer = castTime;
        stateRoutine = StartCoroutine(CastAbility());
    }

    private IEnumerator CastAbility()
    {
        owner.animator.Play(Settings.attack2);
        WeaponSoundEffect();
        while (castTimer > 0)
        {
            float random = Random.Range(0f, 1f);
            Vector3 lightningPoint;
            if (random <= .3f)
            {
                lightningPoint = GameManager.Instance.GetPlayer().transform.position;
            }
            else
            {
                lightningPoint = Random.insideUnitCircle * 7.5f;
                lightningPoint += transform.position;
            }
            WindDemonBeastAttack2AnimationEvent lightning = (WindDemonBeastAttack2AnimationEvent)PoolManager.Instance.ReuseComponent(lightningPrefab, lightningPoint, Quaternion.identity);
            lightning.gameObject.SetActive(true);
            yield return new WaitForSeconds(.2f);
        }
        yield return new WaitForSeconds(1f);
        stateCooldownTimer = Random.Range(stateCooldownMin, stateCooldownMax);
        owner.ChooseState();
    }

    public override void OnExit()
    {
        if (stateRoutine != null)
            StopCoroutine(stateRoutine);
        stateRoutine = null;
        owner.animator.Play(Settings.idle);
    }

    public override void StateUpdate(float deltaTime)
    {
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        castTimer -= Time.deltaTime;
    }
}
