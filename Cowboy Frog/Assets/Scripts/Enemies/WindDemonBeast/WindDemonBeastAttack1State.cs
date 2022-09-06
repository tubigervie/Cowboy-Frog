using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindDemonBeastAttack1State : AIState
{
    [SerializeField] Transform shootPosition;
    [SerializeField] float minDistance = 0f;
    [SerializeField] float maxDistance = 99f;
    [SerializeField] AmmoDetailsSO ammoDetails;

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

        stateRoutine = StartCoroutine(FireAmmoRoutine(ammoDetails));
    }

    public override void OnEnter()
    {
        owner.animator.Play(Settings.attack1);
    }

    private IEnumerator FireAmmoRoutine(AmmoDetailsSO currentAmmo)
    {
        int ammoCounter = 0;
        while (ammoCounter < currentAmmo.ammoSpawnAmount)
        {
            ammoCounter++;
            GameObject ammoPrefab = currentAmmo.ammoPrefabArray[UnityEngine.Random.Range(0, currentAmmo.ammoPrefabArray.Length)];
            IFireable ammo = (IFireable)PoolManager.Instance.ReuseComponent(ammoPrefab, shootPosition.position, Quaternion.identity);
            ammo.InitializeAmmo(currentAmmo, GameManager.Instance.GetPlayer().transform);
            yield return new WaitForSeconds(currentAmmo.ammoSpawnInterval);
        }
        yield return new WaitForSeconds(1f);
        stateCooldownTimer = Random.Range(stateCooldownMin, stateCooldownMax);
        owner.ChooseState();
        //WeaponShootEffect(aimAngle);
        //WeaponSoundEffect();
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
