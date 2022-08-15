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
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public Bounds roomColliderBounds;

    private BoxCollider2D boxCollider2D;

    private const string groundTag = "groundTilemap";
    private const string decoration1Tag = "decoration1Tilemap";
    private const string decoration2Tag = "decoration2Tilemap";
    private const string frontTag = "frontTilemap";
    private const string collisionTag = "collisionTilemap";
    private const string minimapTag = "minimapTilemap";


    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        roomColliderBounds = boxCollider2D.bounds;
    }

    public void Initialize(GameObject roomGameObject)
    {
        PopulateTilemapMemberVariables(roomGameObject);
        BlockOffUnusedDoorways();
        AddDoorsToRooms();
        DisableCollisionTilemapRenderer();
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
            if (tilemap.gameObject.CompareTag(groundTag))
                groundTilemap = tilemap;
            else if (tilemap.gameObject.CompareTag(decoration1Tag))
                decoration1Tilemap = tilemap;
            else if (tilemap.gameObject.CompareTag(decoration2Tag))
                decoration2Tilemap = tilemap;
            else if (tilemap.gameObject.CompareTag(frontTag))
                frontTilemap = tilemap;
            else if (tilemap.gameObject.CompareTag(collisionTag)) //change tags to static strings
                collisionTilemap = tilemap;
            else if (tilemap.gameObject.CompareTag(minimapTag))
                minimapTilemap = tilemap;
        }
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
    }
}
