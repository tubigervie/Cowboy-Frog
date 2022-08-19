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
        if(fireWeaponEventArgs.fire)
        {
            if(IsWeaponReadyToFire())
            {
                FireAmmo(fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector);
                ResetCooldownTimer();
            }
        }
    }

    private void ResetCooldownTimer()
    {
        firerateCoolDownTimer = activeWeapon.GetCurrentWEapon().weaponDetails.weaponFireRate;
    }

    private void FireAmmo(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        AmmoDetailsSO currentAmmo = activeWeapon.GetCurrentAmmo();
        if(currentAmmo != null)
        {
            GameObject ammoPrefab = currentAmmo.ammoPrefabArray[UnityEngine.Random.Range(0, currentAmmo.ammoPrefabArray.Length)];
            float ammoSpeed = currentAmmo.ammoSpeed;
            IFireable ammo = (IFireable)PoolManager.Instance.ReuseComponent(ammoPrefab, activeWeapon.GetShootPosition(), Quaternion.identity);
            ammo.InitializeAmmo(currentAmmo, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector);

            if(!activeWeapon.GetCurrentWEapon().weaponDetails.hasInfiniteClipCapacity)
            {
                activeWeapon.GetCurrentWEapon().weaponClipRemainingAmmo--;
            }

            weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentWEapon());
        }
    }

    private bool IsWeaponReadyToFire()
    {
        if (activeWeapon.GetCurrentWEapon().isWeaponReloading)
            return false;
        if (firerateCoolDownTimer > 0f)
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
