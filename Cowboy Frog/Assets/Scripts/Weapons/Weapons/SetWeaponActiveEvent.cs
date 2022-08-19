using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SetWeaponActiveEvent : MonoBehaviour
{
    public event Action<SetWeaponActiveEvent, SetActiveWeaponEventArgs> OnSetActiveWeapon;

    public void CallSetActiveWeaponEvent(Weapon weapon)
    {
        OnSetActiveWeapon?.Invoke(this, new SetActiveWeaponEventArgs() { weapon = weapon });
    }
}

public class SetActiveWeaponEventArgs : EventArgs
{
    public Weapon weapon;
}
