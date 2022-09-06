using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindDemonBeastAttack1State : AIState
{
    [SerializeField] Transform shootPosition;
    [SerializeField] AmmoDetailsSO ammoDetails;
    [SerializeField] SoundEffectSO shootSFX;

    Coroutine stateRoutine;

    public override int CalculateStateCost()
    {
        int stateCost = int.MaxValue;
        if (stateCooldownTimer < 0f)
        {
            float distToPlayer = Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition());
            if (minDistance <= distToPlayer && distToPlayer <= maxDistance)
            {
                stateCost = 0;
            }
        }
        return stateCost;
    }

    public void FireWeapon()
    {
        if (ammoDetails == null) return;

        StartCoroutine(FireAmmoRoutine(ammoDetails));
    }

    public override void OnEnter()
    {
        stateRoutine = StartCoroutine(AnimationRoutine()); 
    }

    private IEnumerator AnimationRoutine()
    {
        int spawnAmount = 1;
        int ammoCounter = 0;
        if (owner.health.GetHealthPercent() <= .5f) spawnAmount = 2;
        while(ammoCounter < spawnAmount)
        {
            owner.animator.Play(Settings.attack1);
            ammoCounter++;
            yield return new WaitForSeconds(1.5f);
        }
        stateCooldownTimer = Random.Range(stateCooldownMin, stateCooldownMax);
        owner.ChooseState();
    }

    private void WeaponSoundEffect()
    {
        if (shootSFX != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(shootSFX);
        }
    }

    private IEnumerator FireAmmoRoutine(AmmoDetailsSO currentAmmo)
    {
        int ammoCounter = 0;
        int spawnAmount = 1;
        while (ammoCounter < spawnAmount)
        {
            ammoCounter++;
            GameObject ammoPrefab = currentAmmo.ammoPrefabArray[UnityEngine.Random.Range(0, currentAmmo.ammoPrefabArray.Length)];
            IFireable ammo = (IFireable)PoolManager.Instance.ReuseComponent(ammoPrefab, shootPosition.position, Quaternion.identity);
            ammo.InitializeAmmo(currentAmmo, GameManager.Instance.GetPlayer().transform);
            WeaponSoundEffect();
            yield return new WaitForSeconds(currentAmmo.ammoSpawnInterval);
        }
    }

    public override void OnExit()
    {
        if (stateRoutine != null)
            StopCoroutine(stateRoutine);
        stateRoutine = null;
    }

    public override void StateUpdate(float deltaTime)
    {
    }

}
