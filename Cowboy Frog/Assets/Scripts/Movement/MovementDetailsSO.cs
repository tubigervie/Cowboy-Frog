using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementDetails_", menuName = "Scriptable Objects/Movement/MovementDetails")]
public class MovementDetailsSO : ScriptableObject
{
    #region Header MOVEMENT DETAILS
    [Space(10)]
    [Header("MOVEMENT DETAILS")]
    #endregion Header
    #region Tooltip
    [Tooltip("Movement speed")]
    #endregion Tooltip
    public float moveSpeed = 8f;

    #region Tooltip
    [Tooltip("Roll speed")]
    #endregion Tooltip
    public float rollSpeed; //for player

    #region Tooltip
    [Tooltip("Roll distance")]
    #endregion Tooltip
    public float rollDistance;

    #region Tooltip
    [Tooltip("Cooldown time in seconds between rolls")]
    #endregion Tooltip
    public float rollCooldownTime;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(moveSpeed), moveSpeed, false);
        if(rollDistance != 0f || rollSpeed != 0 || rollCooldownTime != 0)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollDistance), rollDistance, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollSpeed), rollSpeed, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollCooldownTime), rollCooldownTime, false);
        }
    }
#endif
    #endregion Validation
}
