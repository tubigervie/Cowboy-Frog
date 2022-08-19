using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="WeaponDetails_", menuName = "Scriptable Objects/Weapons/Weapon Details")]
public class WeaponDetailsSO : ScriptableObject
{
    #region Header WEAPON BASE DETAILS
    [Space(10)]
    [Header("WEAPON BASE DETAILS")]
    #endregion Header WEAPON BASE DETAILS
    #region Toolip
    [Tooltip("Weapon name")]
    #endregion
    public string weaponName;
    #region Toolip
    [Tooltip("The sprite for the weapon - the sprite should have the 'generate physics shape' option selected")]
    #endregion
    public Sprite weaponSprite;

    #region Header WEAPON CONFIGURATION
    [Space(10)]
    [Header("WEAPON CONFIGURATION")]
    #endregion Header WEAPON CONFIGURATION
    #region Toolip
    [Tooltip("Weapon Shoot Position - the offset position for the end of the weapon from the sprite pivot point")]
    #endregion
    public Vector3 weaponShootPosition; //for figuring out where to place ammo
    #region Toolip
    [Tooltip("Weapon current ammo")]
    #endregion
    public AmmoDetailsSO weaponCurrentAmmo;

    #region Header WEAPON OPERATING VALUES
    [Space(10)]
    [Header("WEAPON OPERATING VALUES")]
    #endregion Header WEAPON OPERATING VALUES
    #region Toolip
    [Tooltip("Select if the weapon has infinite ammo")]
    #endregion
    public bool hasInfiniteAmmo = false;
    #region Toolip
    [Tooltip("Select if the weapon has infinite clip capacity")]
    #endregion
    public bool hasInfiniteClipCapacity = false;
    #region Toolip
    [Tooltip("The weapon capacity - shots before a reload")]
    #endregion
    public int weaponClipAmmoCapacity = 6;

    #region Toolip
    [Tooltip("Weapon ammo capacity - the maximum number of rounds that can be held for this weapon")]
    #endregion
    public int weaponAmmoCapacity = 100;
    #region Toolip
    [Tooltip("Weapon Fire Rate - 0.2 means 5 shots a second")]
    #endregion
    public float weaponFireRate = 0.2f;
    #region Toolip
    [Tooltip("Weapon Precharge Time - time in seconds to hold fire button down before firing")]
    #endregion
    public float weaponPrechargeTime = 0f;
    #region Toolip
    [Tooltip("Weapon Reload Time - in seconds")]
    #endregion
    public float weaponReloadTime = 0f;

    public bool isAutomatic = true;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(weaponName), weaponName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponCurrentAmmo), weaponCurrentAmmo);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponFireRate), weaponFireRate, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponPrechargeTime), weaponPrechargeTime, true);

        if(!hasInfiniteAmmo)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponAmmoCapacity), weaponAmmoCapacity, false);
        }

        if(!hasInfiniteClipCapacity)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponClipAmmoCapacity), weaponClipAmmoCapacity, false);
        }
    }
#endif
    #endregion
}
