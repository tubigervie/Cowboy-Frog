using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{
    #region Header DUNGEON LEVELS
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    #endregion Header DUNGEON LEVELS
    #region Tooltip
    [Tooltip("Populate with the dungeon level scriptable objects")]
    #endregion Tooltip
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip
    [Tooltip("Populate with the starting dungeon level for testing, first level = 0")]
    #endregion Tooltip
    [SerializeField] private int currentDungeonLevelListIndex = 0;

    private Room currentRoom;

    private Room previousRoom;

    private PlayerDetailsSO playerDetails;

    private Player player;

    public GameState gameState;

    protected override void Awake()
    {
        base.Awake();

        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        InstantiatePlayer();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);
    }

    private void Start()
    {
        gameState = GameState.gameStarted;
    }

    private void Update()
    {
        HandleGameState();
    }

    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                PlayDungeonLevel(currentDungeonLevelListIndex);
                gameState = GameState.playingLevel;
                break;
            case GameState.playingLevel:
                if (Input.GetKeyDown(KeyCode.P))
                    PlayDungeonLevel(currentDungeonLevelListIndex);
                break;
            case GameState.engagingEnemies:
                break;
            case GameState.bossStage:
                break;
            case GameState.engagingBoss:
                break;
            case GameState.levelCompleted:
                break;
            case GameState.gameWon:
                break;
            case GameState.gameLost:
                break;
            case GameState.GamePaused:
                break;
            case GameState.dungeonOverviewMap:
                break;
            case GameState.restartGame:
                break;
        }
    }

    private void InstantiatePlayer()
    {
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);
        player = playerGameObject.GetComponent<Player>();
        player.Initialize(playerDetails);
    }

    public Sprite GetPlayerIcon()
    {
        return playerDetails.playerMiniMapIcon;
    }

    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;
    }

    public Player GetPlayer()
    {
        return player;
    }

    private void PlayDungeonLevel(int currentDungeonLevelListIndex)
    {
        bool dungeonBuiltSuccessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[currentDungeonLevelListIndex]);

        if(!dungeonBuiltSuccessfully)
        {
            Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
        }

        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        player.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f, (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);

        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(player.gameObject.transform.position);
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }

#endif
    #endregion
}
