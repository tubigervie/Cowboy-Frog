using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{
    #region Header GAMEOBJECT REFS
    [Header("GAMEOBJECT REFS")]
    #endregion
    [Tooltip("Populate with the MessageText textmeshpro component in FadeScreenUI")]
    [SerializeField] private TextMeshProUGUI messageTextTMP;
    [Tooltip("Populate with the CanvasGroup component in FadeScreenUI")]
    [SerializeField] private CanvasGroup canvasGroup;

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

    public GameState previousGameState;

    private InstantiatedRoom bossRoom;

    protected override void Awake()
    {
        base.Awake();

        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        InstantiatePlayer();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;

        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;

        player.destroyedEvent.OnDestroyed += Player_OnDestroyed;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;

        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;

        player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;
    }

    private void Start()
    {
        previousGameState = GameState.gameStarted;
        gameState = GameState.gameStarted;
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));
    }

    private IEnumerator Fade(float startAlpha, float targetAlpha, float fadeSeconds, Color backgroundColor)
    {
        Image image = canvasGroup.GetComponent<Image>();
        image.color = backgroundColor;

        float time = 0;
        while(time <= fadeSeconds)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeSeconds);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }

    private void Update()
    {
        HandleGameState();
    }

    private void Player_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        previousGameState = gameState;
        gameState = GameState.gameLost;
    }

    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        RoomEnemiesDefeated();
    }

    private void RoomEnemiesDefeated()
    {
        bool isDungeonClearOfRegularEnemies = true;
        bossRoom = null;

        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            if (keyValuePair.Value.roomNodeType.isBossRoom)
            {
                bossRoom = keyValuePair.Value.instantiatedRoom;
                continue;
            }

            if (!keyValuePair.Value.isClearedOfEnemies)
            {
                isDungeonClearOfRegularEnemies = false;
                break;
            }
        }

        if ((isDungeonClearOfRegularEnemies && bossRoom == null) || (isDungeonClearOfRegularEnemies && bossRoom.room.isClearedOfEnemies))
        {
            if (currentDungeonLevelListIndex < dungeonLevelList.Count - 1)
            {
                gameState = GameState.levelCompleted;
            }
            else
            {
                gameState = GameState.gameWon;
            }
        }
        else if (isDungeonClearOfRegularEnemies)
        {
            gameState = GameState.bossStage;
            StartCoroutine(BossStage());
        }
    }

    private IEnumerator BossStage()
    {
        bossRoom.gameObject.SetActive(true);

        bossRoom.UnlockDoors(0f);

        yield return new WaitForSeconds(2f);
        ClearMessageText();
        yield return StartCoroutine(Fade(0f, 1f, .5f, new Color(0f, 0f, 0f, 0.4f)));

        yield return StartCoroutine(DisplayMessageRoutine("DEFEAT THE BOSS...", Color.white, 5f));
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);
    }

    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                PlayDungeonLevel(currentDungeonLevelListIndex);
                gameState = GameState.playingLevel;

                //Trigger room enemies defeated since we start in the entrance where there are no enemies (just in case level with just a boss room!)
                RoomEnemiesDefeated();
                break;
            case GameState.playingLevel:
                break;
            case GameState.engagingEnemies:
                break;
            case GameState.bossStage:
                break;
            case GameState.engagingBoss:
                break;
            case GameState.levelCompleted:
                StartCoroutine(LevelCompleted());
                break;
            case GameState.gameWon:
                if(previousGameState != GameState.gameWon)
                    StartCoroutine(GameWon());
                break;
            case GameState.gameLost:
                if (previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines(); //prevent messages if clearing level just as player get killeds
                    StartCoroutine(GameLost());
                }
                break;
            case GameState.GamePaused:
                break;
            case GameState.dungeonOverviewMap:
                break;
            case GameState.restartGame:
                RestartGame();
                break;
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("MainGameScene");
    }

    private IEnumerator LevelCompleted()
    {
        gameState = GameState.playingLevel;

        yield return new WaitForSeconds(2f);
        ClearMessageText();
        yield return StartCoroutine(Fade(0f, 1f, .5f, new Color(0f, 0f, 0f, 0.4f)));

        yield return StartCoroutine(DisplayMessageRoutine("YOU SURVIVED...", Color.white, 2f));

        yield return StartCoroutine(DisplayMessageRoutine("PRESS RETURN TO DESCEND FURTHER.", Color.white, 0f));
        
        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        currentDungeonLevelListIndex++;

        PlayDungeonLevel(currentDungeonLevelListIndex);
    }

    private IEnumerator GameWon()
    {
        previousGameState = GameState.gameWon;

        GetPlayer().playerControl.DisablePlayer();
        ClearMessageText();
        yield return StartCoroutine(Fade(0f, 1f, 1f, Color.black));

        yield return StartCoroutine(DisplayMessageRoutine("YOU BEAT THE DUNGEON...", Color.white, 5f));

        yield return StartCoroutine(DisplayMessageRoutine("PRESS RETURN TO RESTART THE GAME.", Color.white, 0f));

        gameState = GameState.restartGame;
    }

    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;

        GetPlayer().playerControl.DisablePlayer();
        ClearMessageText();
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(Fade(0f, 1f, 1f, Color.black));

        //Resource hungry but ok since end game state
        Enemy[] enemyArray = GameObject.FindObjectsOfType<Enemy>();
        
        foreach(Enemy enemy in enemyArray)
        {
            enemy.gameObject.SetActive(false);
        }

        yield return StartCoroutine(DisplayMessageRoutine("YOU HAVE BEEN SLAYED...", Color.white, 5f));

        yield return StartCoroutine(DisplayMessageRoutine("PRESS RETURN TO RESTART THE GAME.", Color.white, 0f));

        currentDungeonLevelListIndex = 0;

        gameState = GameState.restartGame;
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

        StartCoroutine(DisplayDungeonLevelText());
    }

    private IEnumerator DisplayDungeonLevelText()
    {
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

        GetPlayer().playerControl.DisablePlayer();

        string messageText = "LEVEL " + (currentDungeonLevelListIndex + 1).ToString();

        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 2f));

        GetPlayer().playerControl.EnablePlayer();

        yield return StartCoroutine(Fade(1f, 0f, 2f, Color.black));
    }

    private IEnumerator DisplayMessageRoutine(string messageText, Color messageColor, float displaySeconds)
    {
        messageTextTMP.SetText(messageText);
        messageTextTMP.color = messageColor;

        if(displaySeconds > 0f)
        {
            float timer = displaySeconds;
            while(timer > 0f && !Input.GetKeyDown(KeyCode.Return))
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            //else display the message until the return button is pressed
            while(!Input.GetKeyDown(KeyCode.Return))
            {
                yield return null;
            }
        }
    }

    private void ClearMessageText()
    {
        messageTextTMP.SetText("");
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
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
