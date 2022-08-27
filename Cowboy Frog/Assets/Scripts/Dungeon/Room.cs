using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public string id;
    public string templateID; //id from the room template SO
    public GameObject prefab;
    public RoomNodeTypeSO roomNodeType;
    public Vector2Int lowerBounds; //grid coordinates as the room is placed;
    public Vector2Int upperBounds;
    public Vector2Int templateLowerBounds;
    public Vector2Int templateUpperBounds;
    public Vector2Int[] spawnPositionArray;
    public List<string> childRoomIDList;
    public string parentRoomID;
    public List<Doorway> doorWayList;
    public bool isPositioned = false;
    public InstantiatedRoom instantiatedRoom;
    public bool isLit = false;
    public bool isClearedOfEnemies = false;
    public bool isPreviouslyVisited = false;
    public List<SpawnableObjectsByLevel<EnemyDetailsSO>> enemiesByLevelList;
    public List<RoomEnemySpawnParameters> roomLevelEnemySpawnParametersList;
    public MusicTrackSO musicTrack;

    public Room()
    {
        childRoomIDList = new List<string>();
        doorWayList = new List<Doorway>();
    }

    /// <summary>
    /// Get the number of enemies to spawn for this room in this dungeon level
    /// </summary>
    /// <param name="dungeonLevel"></param>
    /// <returns></returns>
    public int GetNumberOfEnemiesToSpawn(DungeonLevelSO dungeonLevel)
    {
        foreach(RoomEnemySpawnParameters roomEnemySpawnParameters in roomLevelEnemySpawnParametersList)
        {
            if(roomEnemySpawnParameters.dungeonLevel == dungeonLevel)
            {
                return Random.Range(roomEnemySpawnParameters.minTotalEnemiesToSpawn, roomEnemySpawnParameters.maxTotalEnemiesToSpawn);
            }
        }

        return 0;
    }

    public RoomEnemySpawnParameters GetRoomEnemySpawnParameters(DungeonLevelSO dungeonLevel)
    {
        foreach(RoomEnemySpawnParameters roomEnemySpawnParameters in roomLevelEnemySpawnParametersList)
        {
            if(roomEnemySpawnParameters.dungeonLevel == dungeonLevel)
            {
                return roomEnemySpawnParameters;
            }
        }
        return null;
    }

}
