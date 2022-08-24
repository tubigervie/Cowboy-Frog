using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarTest : MonoBehaviour
{
    [SerializeField] bool useDungeonBuildGrid;
    private InstantiatedRoom instantiatedRoom;
    private Grid grid;
    private Tilemap frontTilemap;
    private Tilemap pathTilemap;
    private Vector3Int startGridPosition;
    private Vector3Int endGridPosition;
    private TileBase startPathTile;
    private TileBase finishPathTile;

    private Vector3Int noValue = new Vector3Int(9999, 9999, 9999);
    private Stack<Vector3> pathStack;

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void Start()
    {
        startPathTile = GameResources.Instance.preferredEnemyPathTile;
        finishPathTile = GameResources.Instance.enemyUnwalkableCollisionTilesArray[0];
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        pathStack = null;
        instantiatedRoom = roomChangedEventArgs.room.instantiatedRoom;
        frontTilemap = instantiatedRoom.transform.Find("Grid/Tilemap4_Front").GetComponent<Tilemap>();
        grid = instantiatedRoom.transform.GetComponentInChildren<Grid>();
        startGridPosition = noValue;
        endGridPosition = noValue;

        SetUpPathTilemap();
    }

    /// <summary>
    /// Use a clone of the front tilemap for the path tilemap. if not created then create one else use the existing one.
    /// </summary>
    private void SetUpPathTilemap()
    {
        Transform tilemapCloneTransform = instantiatedRoom.transform.Find("Grid/Tilemap4_Front(Clone)");

        //if the front tilemap hasn't been cloned then clone it
        if(tilemapCloneTransform == null)
        {
            pathTilemap = Instantiate(frontTilemap, grid.transform);
            pathTilemap.GetComponent<TilemapRenderer>().sortingOrder = 2;
            pathTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
            pathTilemap.gameObject.tag = "Untagged";
        }
        //else use it
        {
            pathTilemap = instantiatedRoom.transform.Find("Grid/Tilemap4_Front(Clone)").GetComponent<Tilemap>();
            pathTilemap.ClearAllTiles();
        }
    }

    private void Update()
    {
        if (instantiatedRoom == null || startPathTile == null || finishPathTile == null || grid == null || pathTilemap == null) return;

        if(Input.GetKeyDown(KeyCode.I))
        {
            ClearPath();
            SetStartPosition();
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            ClearPath();
            SetEndPosition();
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            DisplayPath();
        }
    }

    private void DisplayPath()
    {
        if (startGridPosition == noValue || endGridPosition == noValue) return;
        Grid gridToUse = (useDungeonBuildGrid) ? DungeonBuilder.Instance.dungeonGrid : grid;
        pathStack = AStar.BuildPath(instantiatedRoom.room, startGridPosition, endGridPosition, useDungeonBuildGrid);
        if (pathStack == null) return;
        foreach(Vector3 worldPosition in pathStack)
        {
            Vector3Int cellPos = gridToUse.WorldToCell(worldPosition);
            Vector2Int dungBoun = DungeonBuilder.Instance.dungeonLowerBounds;
            Vector3Int worldPos = gridToUse.WorldToCell(worldPosition) + (Vector3Int)(DungeonBuilder.Instance.dungeonLowerBounds - instantiatedRoom.room.lowerBounds);
            if (useDungeonBuildGrid)
                DungeonBuilder.Instance.dungeonTileMap.SetTile(worldPos, startPathTile);
            else
                pathTilemap.SetTile(cellPos, startPathTile);
        }
    }

    private void SetEndPosition()
    {
        Grid gridToUse = (useDungeonBuildGrid) ? DungeonBuilder.Instance.dungeonGrid : grid;
        if (endGridPosition == noValue)
        {
            endGridPosition = gridToUse.WorldToCell(HelperUtilities.GetMouseWorldPosition());
            if (!IsPositionWithinBounds(endGridPosition))
            {
                endGridPosition = noValue;
                return;
            }
            if (useDungeonBuildGrid)
                DungeonBuilder.Instance.dungeonTileMap.SetTile(endGridPosition, finishPathTile);
            else
                pathTilemap.SetTile(endGridPosition, finishPathTile);
        }
        else
        {
            if (useDungeonBuildGrid)
                DungeonBuilder.Instance.dungeonTileMap.SetTile(endGridPosition, null);
            else
                pathTilemap.SetTile(endGridPosition, null);
            endGridPosition = noValue;
        }
    }

    private void SetStartPosition()
    {
        if(startGridPosition == noValue)
        {
            Grid gridToUse = (useDungeonBuildGrid) ? DungeonBuilder.Instance.dungeonGrid : grid;
            startGridPosition = gridToUse.WorldToCell(HelperUtilities.GetMouseWorldPosition());
            if(!IsPositionWithinBounds(startGridPosition))
            {
                startGridPosition = noValue;
                return;
            }
            if (useDungeonBuildGrid)
                DungeonBuilder.Instance.dungeonTileMap.SetTile(startGridPosition, startPathTile);
            else
                pathTilemap.SetTile(startGridPosition, startPathTile);
        }
        else
        {
            if (useDungeonBuildGrid)
                DungeonBuilder.Instance.dungeonTileMap.SetTile(startGridPosition, null);
            else
                pathTilemap.SetTile(startGridPosition, null);
            startGridPosition = noValue;
        }
    }

    private bool IsPositionWithinBounds(Vector3Int position)
    {
        Vector2Int lowerBoundsToUse = (useDungeonBuildGrid) ? DungeonBuilder.Instance.dungeonLowerBounds : instantiatedRoom.room.templateLowerBounds;
        Vector2Int upperBoundsToUse = (useDungeonBuildGrid) ? DungeonBuilder.Instance.dungeonUpperBounds : instantiatedRoom.room.templateUpperBounds;

        //if position is beyond grid return false;
        if (position.x < lowerBoundsToUse.x || position.x > upperBoundsToUse.x
            || position.y < lowerBoundsToUse.y || position.x > upperBoundsToUse.y)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void ClearPath()
    {
        if (pathStack == null) return;
        Grid gridToUse = (useDungeonBuildGrid) ? DungeonBuilder.Instance.dungeonGrid : grid;
        foreach (Vector3 worldPosition in pathStack)
        {
            Vector3Int cellPos = gridToUse.WorldToCell(worldPosition);
            Vector3Int worldPos = gridToUse.WorldToCell(worldPosition) + (Vector3Int)(DungeonBuilder.Instance.dungeonLowerBounds - instantiatedRoom.room.templateLowerBounds);
            if (useDungeonBuildGrid)
                DungeonBuilder.Instance.dungeonTileMap.SetTile(worldPos, null);
            else
                pathTilemap.SetTile(cellPos, null);
        }

        pathStack = null;

        endGridPosition = noValue;
        startGridPosition = noValue;
    }
}
