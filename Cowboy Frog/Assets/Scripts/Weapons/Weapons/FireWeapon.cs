using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
[RequireComponent(typeof(ReloadWeaponEvent))]
[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    private float firePreChargeTimer = 0f;
    private float firerateCoolDownTimer = 0f;
    private ActiveWeapon activeWeapon;
    private WeaponFiredEvent weaponFiredEvent;
    private FireWeaponEvent fireWeaponEvent;
    private ReloadWeaponEvent reloadWeaponEvent;

    private void Awake()
    {
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
    }

    private void Update()
    {
        firerateCoolDownTimer -= Time.deltaTime;
    }

    private void OnEnable()
    {
        fireWeaponEvent.OnFireWeapon += FireWeaponEvent_OnFireWeapon;
    }

    private void OnDisable()
    {
        fireWeaponEvent.OnFireWeapon -= FireWeaponEvent_OnFireWeapon;
    }

    private void FireWeaponEvent_OnFireWeapon(FireWeaponEvent fireWeaponEvent, FireWeaponEventArgs fireWeaponEventArgs)
    {
        WeaponFire(fireWeaponEventArgs);
    }

    private void WeaponFire(FireWeaponEventArgs fireWeaponEventArgs)
    {
        WeaponPreCharge(fireWeaponEventArgs);
        if (fireWeaponEventArgs.fire)
        {
            if (IsWeaponReadyToFire())
            {
                FireAmmo(fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector);
                ResetCooldownTimer();
            }
        }
    }

    private void ResetPrechargeTimer()
    {
        firePreChargeTimer = activeWeapon.GetCurrentWEapon().weaponDetails.weaponPrechargeTime;
    }

    private void WeaponPreCharge(FireWeaponEventArgs fireWeaponEventArgs)
    {
        if (fireWeaponEventArgs.firePreviousFrame)
        {
            firePreChargeTimer -= Time.deltaTime;
        }
        else
        {
            ResetPrechargeTimer();
        }
    }

    private void ResetCooldownTimer()
    {
        firerateCoolDownTimer = activeWeapon.GetCurrentWEapon().weaponDetails.weaponFireRate;
    }

    private void FireAmmo(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        AmmoDetailsSO currentAmmo = activeWeapon.GetCurrentAmmo();
        if (currentAmmo != null)
        {
            StartCoroutine(FireAmmoRoutine(currentAmmo, aimAngle, weaponAimAngle, weaponAimDirectionVector));
        }
    }

    private IEnumerator FireAmmoRoutine(AmmoDetailsSO currentAmmo, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        int ammoCounter = 0;
        while(ammoCounter < currentAmmo.ammoSpawnAmount)
        {
            ammoCounter++;
            GameObject ammoPrefab = currentAmmo.ammoPrefabArray[UnityEngine.Random.Range(0, currentAmmo.ammoPrefabArray.Length)];
            float ammoSpeed = currentAmmo.ammoSpeed;
            IFireable ammo = (IFireable)PoolManager.Instance.ReuseComponent(ammoPrefab, activeWeapon.GetShootPosition(), Quaternion.identity);
            ammo.InitializeAmmo(currentAmmo, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector);
            yield return new WaitForSeconds(currentAmmo.ammoSpawnInterval);
        }
        if (!activeWeapon.GetCurrentWEapon().weaponDetails.hasInfiniteClipCapacity)
        {
            activeWeapon.GetCurrentWEapon().weaponClipRemainingAmmo--;
        }

        weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentWEapon());

        WeaponShootEffect(aimAngle);
        WeaponSoundEffect();
    }

    private void WeaponShootEffect(float aimAngle)
    {
        if (activeWeapon.GetCurrentWEapon().weaponDetails.weaponShootEffect != null && activeWeapon.GetCurrentWEapon().weaponDetails.weaponShootEffect.weaponShootEffectPrefab != null)
        {
            WeaponShootEffect weaponShootEffect = (WeaponShootEffect)PoolManager.Instance.ReuseComponent(activeWeapon.GetCurrentWEapon().weaponDetails.weaponShootEffect.weaponShootEffectPrefab,
                activeWeapon.GetShootEffectPosition(), Quaternion.identity);

            weaponShootEffect.SetShootEffect(activeWeapon.GetCurrentWEapon().weaponDetails.weaponShootEffect, aimAngle);

            weaponShootEffect.gameObject.SetActive(true);
        }
    }

    private void WeaponSoundEffect()
    {
        if(activeWeapon.GetCurrentWEapon().weaponDetails.weaponFiringSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentWEapon().weaponDetails.weaponFiringSoundEffect);
        }
    }

    private bool IsWeaponReadyToFire()
    {
        if (activeWeapon.GetCurrentWEapon().isWeaponReloading)
            return false;
        if (firePreChargeTimer > 0f || firerateCoolDownTimer > 0f)
            return false;
        if (!activeWeapon.GetCurrentWEapon().weaponDetails.hasInfiniteClipCapacity && activeWeapon.GetCurrentWEapon().weaponClipRemainingAmmo <= 0)
        {
            if(activeWeapon.GetCurrentWEapon().weaponRemainingAmmo > 0 || activeWeapon.GetCurrentWEapon().weaponDetails.hasInfiniteAmmo)
                reloadWeaponEvent.CallReloadWeaponEvent(activeWeapon.GetCurrentWEapon(), 0);
            return false;
        }
        return true;
    }

}
