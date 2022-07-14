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
}
