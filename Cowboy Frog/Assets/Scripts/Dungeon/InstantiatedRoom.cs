using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Paintable paintMap;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public Tilemap puddleTilemap;
    [HideInInspector] public Bounds roomColliderBounds;
    [HideInInspector] public int[,] aStarMovementPenalty;

    [Header("OBJECT REFERENCES")]
    [Tooltip("Populate with the environment child placeholder gameobject")]
    [SerializeField] private GameObject environmentGameObject;

    private BoxCollider2D boxCollider2D;


    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        roomColliderBounds = boxCollider2D.bounds;
    }

    public void Initialize(GameObject roomGameObject)
    {
        PopulateTilemapMemberVariables(roomGameObject);
        BlockOffUnusedDoorways();
        AddObstaclesAndPreferredPaths();
        AddDoorsToRooms();
        DisableCollisionTilemapRenderer();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomArg)
    {
        //edge case for when reentering a boss room after already clearing, try to fix later
        if (roomArg.room.roomNodeType.isBossRoom && roomArg.room.isClearedOfEnemies) return;

        MusicManager.Instance.PlayMusic(roomArg.room.musicTrack, 0.2f, 1f);
    }

    public void ActivateEnvironmentGameObjects()
    {
        if (environmentGameObject != null)
            environmentGameObject.SetActive(true);
    }

    public void DeactivateEnvironmentGameObjects()
    {
        if (environmentGameObject != null)
            environmentGameObject.SetActive(false);
    }

    //Update obsctacles by AStar pathfinding
    private void AddObstaclesAndPreferredPaths()
    {
        aStarMovementPenalty = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1];

        for(int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for(int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                //Set default movement penalty for grid squares
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;

                //Add obstacles for collision tiles the enemy can't walk on
                TileBase tile = collisionTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));
            
                if(tile == GameResources.Instance.preferredEnemyPathTile)
                {
                    aStarMovementPenalty[x, y] = Settings.preferredPathAStarMovementPenalty;
                    continue;
                }

                foreach(TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollisionTilesArray)
                {
                    if(tile == collisionTile)
                    {
                        aStarMovementPenalty[x, y] = 0;
                        break;
                    }
                }
            }
        }
    }

    private void AddDoorsToRooms()
    {
        if (room.roomNodeType.isCorridorEW || room.roomNodeType.isCorridorNS) return;
        foreach(Doorway doorway in room.doorWayList)
        {
            if(doorway.doorPrefab != null && doorway.isConnected)
            {
                float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;
                GameObject door = null;
                if(doorway.orientation == Orientation.north)
                {
                    door = Instantiate(doorway.doorPrefab, this.gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y + tileDistance, 0f);
                }
                else if(doorway.orientation == Orientation.south)
                {
                    door = Instantiate(doorway.doorPrefab, this.gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y, 0f);
                }
                else if(doorway.orientation == Orientation.east)
                {
                    door = Instantiate(doorway.doorPrefab, this.gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance, doorway.position.y + tileDistance * 1.25f, 0f);
                }
                else if(doorway.orientation == Orientation.west)
                {
                    door = Instantiate(doorway.doorPrefab, this.gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x, doorway.position.y + tileDistance * 1.25f, 0f);
                }

                Door doorComponent = door.GetComponent<Door>();
                if(room.roomNodeType.isBossRoom)
                {
                    doorComponent.isBossRoomDoor = true;
                    GameObject skullIcon = Instantiate(GameResources.Instance.bossMapIconPrefab, gameObject.transform);
                    skullIcon.transform.localPosition = door.transform.localPosition;
                    //lock door to prevent access
                    doorComponent.LockDoor();
                }
            }
        }
    }

    private void BlockOffUnusedDoorways()
    {
        foreach(Doorway doorway in room.doorWayList)
        {
            if (doorway.isConnected)
                continue;
            if(collisionTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(collisionTilemap, doorway);
            }
            if (minimapTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(minimapTilemap, doorway);
            }
            if (groundTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(groundTilemap, doorway);
            }
            if (decoration1Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration1Tilemap, doorway);
            }
            if (decoration2Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration2Tilemap, doorway);
            }
            if (frontTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(frontTilemap, doorway);
            }
        }
    }

    private void BlockADoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch(doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;
            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;
        }
    }

    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;
        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {
            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                // Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x +
                    xPos, startPosition.y - yPos, 0)));

                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);

            }
        }
    }

    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;
        for(int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for(int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                // Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x +
                    xPos, startPosition.y - yPos, 0)));

                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);

            }
        }
    }

    private void DisableCollisionTilemapRenderer()
    {
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    private void PopulateTilemapMemberVariables(GameObject roomGameObject)
    {
        grid = roomGameObject.GetComponentInChildren<Grid>();

        Tilemap[] tilemaps = roomGameObject.GetComponentsInChildren<Tilemap>();

        foreach(Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.CompareTag(Settings.groundTag))
                groundTilemap = tilemap;
            else if (tilemap.gameObject.CompareTag(Settings.decoration1Tag))
                decoration1Tilemap = tilemap;
            else if (tilemap.gameObject.CompareTag(Settings.decoration2Tag))
                decoration2Tilemap = tilemap;
            else if (tilemap.gameObject.CompareTag(Settings.frontTag))
                frontTilemap = tilemap;
            else if (tilemap.gameObject.CompareTag(Settings.collisionTag))
                collisionTilemap = tilemap;
            else if (tilemap.gameObject.CompareTag(Settings.minimapTag))
                minimapTilemap = tilemap;
            else if (tilemap.gameObject.CompareTag("puddleTilemap"))
            {
                puddleTilemap = tilemap;
                paintMap = puddleTilemap.gameObject.GetComponent<Paintable>();
            }
        }
    }

    public void UnlockDoors(float doorUnlockDelay)
    {
        StartCoroutine(UnlockDoorsRoutine(doorUnlockDelay));
    }

    private IEnumerator UnlockDoorsRoutine(float doorUnlockDelay)
    {
        if (doorUnlockDelay > 0f)
            yield return new WaitForSeconds(doorUnlockDelay);
        Door[] doorArray = GetComponentsInChildren<Door>();
        foreach(Door door in doorArray)
        {
            door.UnlockDoor();
        }

        //Enable room trigger collider
        EnableRoomCollider();
    }

    public void LockDoors()
    {
        Door[] doorArray = GetComponentsInChildren<Door>();

        foreach(Door door in doorArray)
        {
            door.LockDoor();
        }

        DisableRoomCollider();
    }

    public Door[] GetDoors()
    {
        return GetComponentsInChildren<Door>();
    }

    private void DisableRoomCollider()
    {
        boxCollider2D.enabled = false;
    }

    private void EnableRoomCollider()
    {
        boxCollider2D.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == Settings.playerTag && room != GameManager.Instance.GetCurrentRoom())
        {
            //Set room as visited
            this.room.isPreviouslyVisited = true;

            //Call room changed event
            StaticEventHandler.CallRoomChangedEvent(room);
        }
        else if(collision.tag == Settings.enemyTag)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy.currentRoom == this.room) return;
            enemy.currentRoom = this.room;
        }
    }
}
