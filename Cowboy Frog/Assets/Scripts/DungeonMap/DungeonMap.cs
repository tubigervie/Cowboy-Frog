using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMap : SingletonMonobehaviour<DungeonMap>
{
    #region Header GameObject References
    [Space(10)]
    [Header("GameObject References")]
    #endregion
    [Tooltip("Populate with the MinimapUI gameObject")]
    [SerializeField] private GameObject minimapUI;

    private Camera dungeonMapCamera;
    private Camera cameraMain;

    private void Start()
    {
        cameraMain = Camera.main;

        Transform playerTransform = GameManager.Instance.GetPlayer().transform;

        CinemachineVirtualCamera cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = playerTransform;

        dungeonMapCamera = GetComponentInChildren<Camera>();
        dungeonMapCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && GameManager.Instance.gameState == GameState.dungeonOverviewMap)
        {
            GetRoomClicked();
        }
    }

    public void DisplayDungeonOverviewMap()
    {
        GameManager.Instance.previousGameState = GameManager.Instance.gameState;
        GameManager.Instance.gameState = GameState.dungeonOverviewMap;

        GameManager.Instance.GetPlayer().playerControl.DisablePlayer();

        //disable main camera and enable dungeon overview camera
        cameraMain.gameObject.SetActive(false);
        dungeonMapCamera.gameObject.SetActive(true);

        //ensure all rooms are active so they can be displayed
        ActivateRoomsForDisplay();

        minimapUI.SetActive(false);
    }

    private void ActivateRoomsForDisplay()
    {
        foreach(KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;
            room.instantiatedRoom.gameObject.SetActive(true);
        }
    }

    private void DeactivateRoomsForDisplay()
    {
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;
            room.instantiatedRoom.gameObject.SetActive(false);
        }
    }

    private void GetRoomClicked()
    {
        Vector3 worldPosition = dungeonMapCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPosition = new Vector3(worldPosition.x, worldPosition.y, 0f);

        Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 1f);

        foreach(Collider2D collider2D in collider2DArray)
        {
            Teleporter teleport = collider2D.GetComponent<Teleporter>();
            if(teleport != null && teleport.isActivated)
            {
                teleport.Teleport();              
            }
        }
    }

    public void ClearDungeonOverviewMap(bool enablePlayer = true)
    {
        GameManager.Instance.gameState = GameManager.Instance.previousGameState;
        GameManager.Instance.previousGameState = GameState.dungeonOverviewMap;
        if(enablePlayer)
            GameManager.Instance.GetPlayer().playerControl.EnablePlayer();

        //disable main camera and enable dungeon overview camera
        cameraMain.gameObject.SetActive(true);
        dungeonMapCamera.gameObject.SetActive(false);

        minimapUI.SetActive(true);
    }
}
