using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AmmoDetails_", menuName = "Scriptable Objects/Weapons/Ammo Details")]
public class AmmoDetailsSO : ScriptableObject
{
    #region Header BASIC AMMO DETAILS
    [Space(10)]
    [Header("BASIC AMMO DETAILS")]
    #endregion Header BASIC AMMO DETAILS
    #region Toolip
    [Tooltip("Name for the ammo")]
    #endregion
    public string ammoName;
    public bool isPlayerAmmo;

    #region Header AMMO SPRITE, PREFAB & MATERIALS
    [Space(10)]
    [Header("AMMO SPRITE, PREFAB & MATERIALS")]
    #endregion Header AMMO SPRITE, PREFAB & MATERIALS
    #region Toolip
    [Tooltip("Sprite to be used for the ammo")]
    #endregion
    public Sprite ammoSprite;
    #region Toolip
    [Tooltip("Populate with the prefab to be used for the ammo. If multiple prefabs are specified then a random prefab from the array" +
        "will be selected. The prefab can be an ammo pattern - as long as it conforms to the IFireable interface.")]
    #endregion
    public GameObject[] ammoPrefabArray;

    #region Toolip
    [Tooltip("The material to be used for the ammo")]
    #endregion
    public Material ammoMaterial;

    #region Toolip
    [Tooltip("If the ammo should 'charge' briefly before moving then set the time in seconds that the ammo is held charging after firing" +
        "before release")]
    #endregion
    public float ammoChargeTime = 0.1f;
    public Material ammoChargeMaterial;

    #region Header AMMO HIT EFFECT
    [Space(10)]
    [Header("AMMO HIT EFFECT")]
    #endregion
    #region Tooltip
    [Tooltip("The scriptable object that defines the parameters for the hit effect prefab")]
    #endregion
    public AmmoHitEffectSO ammoHitEffect;

    #region Header AMMO BASE PARAMETERS
    [Space(10)]
    [Header("AMMO BASE PARAMETERS")]
    #endregion Header AMMO BASE PARAMETERS
    #region Toolip
    [Tooltip("The damage each ammo deals")]
    #endregion
    public int ammoDamage = 1;
    #region Toolip
    [Tooltip("Speed at which ammo moves")]
    #endregion
    public float ammoSpeed = 20f;
    #region Toolip
    [Tooltip("The range of the ammo in Unity units")]
    #endregion
    public float ammoRange = 20f;
    #region Toolip
    [Tooltip("The rotation speed in degrees per second of the ammo pattern")]
    #endregion
    public float ammoRotationSpeed = 1f;

    #region Header AMMO SPREAD DETAILS
    [Space(10)]
    [Header("AMMO SPREAD DETAILS")]
    #endregion
    #region Toolip
    [Tooltip("The spread angle of the ammo. A higher spread means less accuracy")]
    #endregion
    public float minSpread = 0f;
    #region Toolip
    [Tooltip("The spread angle of the ammo. A higher spread means less accuracy")]
    #endregion
    public float maxSpread = 0f;

    #region Header AMMO SPAWN DETAILS
    [Space(10)]
    [Header("AMMO SPAWN DETAILS")]
    #endregion
    #region Toolip
    [Tooltip("The number of ammo spawned per shot")]
    #endregion
    public int ammoSpawnAmount = 1;
    #region Toolip
    [Tooltip("The time interval in seconds between spawned ammo.")]
    #endregion
    public float ammoSpawnInterval = 0f;

    #region Header AMMO TRAIL DETAILS
    [Space(10)]
    [Header("AMMO TRAIL DETAILS")]
    #endregion
    #region Toolip
    [Tooltip("Selected if an ammo trail is required, otherwise deselect. If selected then the rest of the ammo trail values should be populated.")]
    #endregion
    public bool isAmmoTrail = false;
    #region Toolip
    [Tooltip("Ammo trail lifetime in seconds")]
    #endregion
    public float ammoTrailTime = 3f;
    public Material ammoTrailMaterial;
    [Range(0f, 1f)] public float ammoTrailStartWidth;
    [Range(0f, 1f)] public float ammoTrailEndWidth;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(ammoName), ammoName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoSprite), ammoSprite);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(ammoPrefabArray), ammoPrefabArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoMaterial), ammoMaterial);
        if(ammoChargeTime > 0)
            HelperUtilities.ValidateCheckNullValue(this, nameof(ammoChargeMaterial), ammoChargeMaterial);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoDamage), ammoDamage, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoSpeed), ammoSpeed, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoRange), ammoRange, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(minSpread), minSpread, true);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoSpawnAmount), ammoSpawnAmount, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoSpawnInterval), ammoSpawnInterval, true);
        if(isAmmoTrail)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailTime), ammoTrailTime, false);
            HelperUtilities.ValidateCheckNullValue(this, nameof(ammoTrailMaterial), ammoTrailMaterial);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailStartWidth), ammoTrailStartWidth, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailEndWidth), ammoTrailEndWidth, false);
        }
    }
#endif
    #endregion Validation
}
