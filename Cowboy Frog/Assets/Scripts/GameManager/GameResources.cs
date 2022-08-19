using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Essentially a resources repository. Any resources (SOs) we want accessible by multiple components can be added to this script. (e.g. GameResources.Instance.roomNodeTypeList)
/// A neat way of centralizing any resources we need to share to make them easily accessible.
/// </summary>
public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if(instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }

    #region Header DUNGEON
    [Space(10)]
    [Header("DUNGEON")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the dungeon RoomNodeTypeListSO")]
    #endregion
    public RoomNodeTypeListSO roomNodeTypeList;

    #region Header PLAYER
    [Space(10)]
    [Header("PLAYER")]
    #endregion Header PLAYER
    #region Tooltip
    [Tooltip("The current player scriptable object - this is used to reference the current player between scenes")]
    #endregion
    public CurrentPlayerSO currentPlayer;

    #region Header MATERIALS
    [Space(10)]
    [Header("MATERIALS")]
    #endregion
    #region Tooltip
    [Tooltip("Dimmed Material")]
    #endregion
    public Material dimmedMaterial;
    #region Tooltip
    [Tooltip("Sprite-Lit-Default Material")]
    #endregion
    public Material litMaterial;

    #region Tooltip
    [Tooltip("Populate with the Variable Lit Shader")]
    #endregion
    public Shader variableLitShader;

    #region Header UI
    [Space(10)]
    [Header("UI")]
    #endregion UI
    #region Tooltip
    #endregion
    public GameObject ammoIconPrefab;


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(roomNodeTypeList), roomNodeTypeList);
        HelperUtilities.ValidateCheckNullValue(this, nameof(currentPlayer), currentPlayer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(dimmedMaterial), dimmedMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(litMaterial), litMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(variableLitShader), variableLitShader);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoIconPrefab), ammoIconPrefab);
    }
#endif
    #endregion
}
