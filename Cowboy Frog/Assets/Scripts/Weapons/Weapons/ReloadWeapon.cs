using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponReloadedEvent))]
[RequireComponent(typeof(SetWeaponActiveEvent))]
[DisallowMultipleComponent]
public class ReloadWeapon : MonoBehaviour
{
    private ReloadWeaponEvent reloadWeaponEvent;
    private WeaponReloadedEvent weaponReloadedEvent;
    private SetWeaponActiveEvent setWeaponActiveEvent;
    //private MovementToPositionEvent movementToPositionEvent;
    private Coroutine reloadWeaponCoroutine;

    private void Awake()
    {
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponReloadedEvent = GetComponent<WeaponReloadedEvent>();
        setWeaponActiveEvent = GetComponent<SetWeaponActiveEvent>();
        //movementToPositionEvent = GetComponent<MovementToPositionEvent>();
    }

    private void OnEnable()
    {
        reloadWeaponEvent.OnReloadWeapon += ReloadWeaponEvent_OnReloadWeapon;
        setWeaponActiveEvent.OnSetActiveWeapon += SetActiveWeaponEvent_OnSetActiveWeapon;
        //movementToPositionEvent.OnMovementToPosition += CancelReload;
    }

    private void OnDisable()
    {
        reloadWeaponEvent.OnReloadWeapon -= ReloadWeaponEvent_OnReloadWeapon;
        setWeaponActiveEvent.OnSetActiveWeapon -= SetActiveWeaponEvent_OnSetActiveWeapon;
        //movementToPositionEvent.OnMovementToPosition -= CancelReload;
    }

    private void SetActiveWeaponEvent_OnSetActiveWeapon(SetWeaponActiveEvent setWeaponActiveEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        if(setActiveWeaponEventArgs.weapon.isWeaponReloading)
        {
            if(reloadWeaponCoroutine != null)
            {
                StopCoroutine(reloadWeaponCoroutine);
            }
            reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(setActiveWeaponEventArgs.weapon, 0));
        }
    }

    private void ReloadWeaponEvent_OnReloadWeapon(ReloadWeaponEvent reloadWeaponEvent, ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        StartReloadWeapon(reloadWeaponEventArgs);
    }

    private void StartReloadWeapon(ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        if(reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }
        reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(reloadWeaponEventArgs.weapon, reloadWeaponEventArgs.topUpAmmoPercent)); 
    }

    //private void CancelReload(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs movementToPositionArgs)
    //{
    //    if(reloadWeaponCoroutine != null)
    //    {
    //        StopCoroutine(reloadWeaponCoroutine);
    //    }
    //    reloadWeaponCoroutine = null;
    //    weapon.isWeaponReloading = false;
    //}

    private IEnumerator ReloadWeaponRoutine(Weapon weapon, int topUpAmmoPercent)
    {

        if (weapon.weaponDetails.weaponFiringSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponReloadSoundEffect);
        }

        weapon.isWeaponReloading = true;

        while(weapon.weaponReloadTimer < weapon.weaponDetails.weaponReloadTime)
        {
            weapon.weaponReloadTimer += Time.deltaTime;
            yield return null;
        }

        if(topUpAmmoPercent != 0)
        {
            int ammoIncrease = Mathf.RoundToInt((weapon.weaponDetails.weaponAmmoCapacity * topUpAmmoPercent) / 100f);

            int totalAmmo = weapon.weaponRemainingAmmo + ammoIncrease;

            if(totalAmmo > weapon.weaponDetails.weaponAmmoCapacity)
            {
                weapon.weaponRemainingAmmo = weapon.weaponDetails.weaponAmmoCapacity;
            }
            else
            {
                weapon.weaponRemainingAmmo = totalAmmo;
            }
        }

        if(weapon.weaponDetails.hasInfiniteAmmo)
        {
            weapon.weaponClipRemainingAmmo = weapon.weaponDetails.weaponClipAmmoCapacity;
        }
        else
        {
            int amountToReload = weapon.weaponDetails.weaponClipAmmoCapacity - weapon.weaponClipRemainingAmmo;
            int remainingAmmoDiff = weapon.weaponRemainingAmmo - amountToReload;
            if (weapon.weaponRemainingAmmo < amountToReload)
            {
                weapon.weaponClipRemainingAmmo += weapon.weaponRemainingAmmo;
                weapon.weaponRemainingAmmo = 0;
            }
            else
            {
                weapon.weaponClipRemainingAmmo += amountToReload;
                weapon.weaponRemainingAmmo = remainingAmmoDiff;
            }
        }
        weapon.weaponReloadTimer = 0f;
        weapon.isWeaponReloading = false;

        weaponReloadedEvent.CallWeaponReloadedEvent(weapon);
    }
}
