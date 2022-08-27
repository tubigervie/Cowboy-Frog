using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    public Grid dungeonGrid;
    public int[,] aStarMovementPenalty;

    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;
    public Tilemap dungeonTileMap;


    public Vector2Int dungeonLowerBounds = new Vector2Int(int.MaxValue, int.MaxValue);
    public Vector2Int dungeonUpperBounds = new Vector2Int(int.MinValue, int.MinValue);


    private void OnEnable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 0f);
    }

    private void OnDisable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    protected override void Awake()
    {
        base.Awake();
        dungeonGrid = GetComponentInChildren<Grid>();
        dungeonTileMap = GetComponentInChildren<Tilemap>();
        LoadRoomNodeTypeList();
    }



    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplates;

        //Load the Scriptable object room templates into dictionary
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;
        while(!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            //Select a random node graph from the list
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphs);

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            //Loop until dungeon successfully built or more than max attempts for node graph
            while(!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                //Clear dungeon room gameobjects and dungeon room dictionary
                ClearDungeon();
                dungeonRebuildAttemptsForNodeGraph++;
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }
            if (dungeonBuildSuccessful)
            {
                InstantiateRoomGameobjects();
            }
        }
        AddObstaclesAndPreferredPaths();
        return dungeonBuildSuccessful;
    }

    private void AddObstaclesAndPreferredPaths()
    {
        aStarMovementPenalty = new int[dungeonUpperBounds.x - dungeonLowerBounds.x + 1, dungeonUpperBounds.y - dungeonLowerBounds.y + 1];
        for (int x = 0; x < (dungeonUpperBounds.x - dungeonLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (dungeonUpperBounds.y - dungeonLowerBounds.y + 1); y++)
            {
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;
            }
        }

        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
            {
                for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
                {
                    //Debug.Log("x, y - dungbounds " + ((x + room.lowerBounds.x - dungeonLowerBounds.x) + " " + (y + room.lowerBounds.y - dungeonLowerBounds.y)));
                    //Set default movement penalty for grid squares
                    aStarMovementPenalty[(x + room.lowerBounds.x - dungeonLowerBounds.x), (y + room.lowerBounds.y - dungeonLowerBounds.y)] = Settings.defaultAStarMovementPenalty;

                    //Add obstacles for collision tiles the enemy can't walk on
                    TileBase tile = room.instantiatedRoom.collisionTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));

                    if (tile == GameResources.Instance.preferredEnemyPathTile)
                    {
                        aStarMovementPenalty[(x + room.lowerBounds.x - dungeonLowerBounds.x), (y + room.lowerBounds.y - dungeonLowerBounds.y)] = Settings.preferredPathAStarMovementPenalty;
                        continue;
                    }

                    foreach (TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollisionTilesArray)
                    {
                        if (tile == collisionTile)
                        {
                            aStarMovementPenalty[(x + room.lowerBounds.x - dungeonLowerBounds.x), (y + room.lowerBounds.y - dungeonLowerBounds.y)] = 0;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void InstantiateRoomGameobjects()
    {
        foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            //Calculate room position (the room instantiation position needs to be adjusted by the room template lower bounds)
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            GameObject roomGameObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);
            //Get instantiated room component from instantiated prefab
            InstantiatedRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;

            instantiatedRoom.Initialize(roomGameObject);
            //Save gameobject reference
            room.instantiatedRoom = instantiatedRoom;

            UpdateDungeonGridBounds(room);
        }
    }

    private void UpdateDungeonGridBounds(Room room)
    {
        int lowerX = dungeonLowerBounds.x;
        int upperX = dungeonUpperBounds.x;
        int lowerY = dungeonLowerBounds.y;
        int upperY = dungeonUpperBounds.y;

        if (room.lowerBounds.x < dungeonLowerBounds.x)
            lowerX = room.lowerBounds.x;
        if (room.lowerBounds.y < dungeonLowerBounds.y)
            lowerY = room.lowerBounds.y;
        if (room.upperBounds.x > dungeonUpperBounds.x)
            upperX = room.upperBounds.x;
        if (room.upperBounds.y > dungeonUpperBounds.y)
            upperY = room.upperBounds.y;

        dungeonLowerBounds = new Vector2Int(lowerX, lowerY);
        dungeonUpperBounds = new Vector2Int(upperX, upperY);
    }

    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if(entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No Entrance Node");
            return false; //Dungeon not built
        }

        bool noRoomOverlaps = true;

        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        return openRoomNodeQueue.Count == 0 && noRoomOverlaps;
    }

    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        while(openRoomNodeQueue.Count > 0 && noRoomOverlaps)
        {
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();
            foreach(RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            if(roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverlaps;
    }

    /// <summary>
    /// Attempt to place the room node in the dungeon - if room can be placed return the room, else return null
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="parentRoom"></param>
    /// <returns></returns>
    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        bool roomOverlaps = true;

        //try to place against all available doorways of the parent until the room is successfully placed without overlap
        while(roomOverlaps)
        {
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            if(unconnectedAvailableParentDoorways.Count == 0)
            {
                return false; //if no more doorways to try then overlap failure
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            if(PlaceRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;
                room.isPositioned = true;
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }
        return true;
    }

    private bool PlaceRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

        //Return if no doorway in room opposite to parent doorway
        if(doorway == null)
        {
            //Just mark the parent doorway as unavailable so we don't try and connect it again
            doorwayParent.isUnavailable = true;
            return false;
        }

        //Calculate 'world' grid parent doorway position
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        //Calculate adjustment position offset based on room doorway position that we are trying to connect (e.eg. if this doorway is west then we need
        //to add (1,0) to the east parent doorway
        Vector2Int adjustment = Vector2Int.zero;

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1); 
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;
            case Orientation.none:
                break;
        }

        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overLappingRoom = CheckForRoomOverlap(room);

        if(overLappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;
            doorway.isConnected = true;
            doorway.isUnavailable = true;
            return true;
        }
        else
        {
            //Just mark the parent doorway as unavailable so we don't try to connect it again
            doorwayParent.isUnavailable = true;
            return false;
        }
    }


    /// <summary>
    /// Check for rooms that overlap the upper and lower bound parameters and if there are overlapping rooms then return room else return null
    /// </summary>
    /// <param name="roomToTest"></param>
    /// <returns></returns>
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;
            
            if (room.id == roomToTest.id || !room.isPositioned)
                continue;

            if (IsOverlappingRoom(roomToTest, room))
                return room;
        }
        return null;
    }

    private bool IsOverlappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = IsOverlappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);
        bool isOverlappingY = IsOverlappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);
        return isOverlappingX && isOverlappingY;
    }

    private bool IsOverlappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        return (Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2));
    }


    private Doorway GetOppositeDoorway(Doorway doorwayParent, List<Doorway> doorWayList)
    {
        foreach(Doorway doorwayToCheck in doorWayList)
        {
            if(doorwayParent.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (doorwayParent.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (doorwayParent.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
            else if (doorwayParent.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }
        return null;
    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;

        if(roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;
                case Orientation.west:
                case Orientation.east:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;
                case Orientation.none:
                    break;
            }
        }
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }
        return roomTemplate;
    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> doorWayList)
    {
        foreach(Doorway doorway in doorWayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
                yield return doorway;
        }
    }

    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        //Initialize room from template
        Room room = new Room();

        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.musicTrack = roomTemplate.roomMusic;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.enemiesByLevelList = roomTemplate.enemiesByLevelList;
        room.roomLevelEnemySpawnParametersList = roomTemplate.roomEnemySpawnParametersList;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;
        room.childRoomIDList = CopyStringList(roomNode.childrenRoomNodeIDList);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        if(roomNode.parentRoomNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;

            //Set entrance in game manager
            GameManager.Instance.SetCurrentRoom(room);
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }

        if(room.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel()) == 0)
        {
            room.isClearedOfEnemies = true;
        }
        return room;
    }

    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
            return null;
    }

    public Room GetRoomByRoomID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
            return null;
    }

    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();
        foreach(Doorway doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();
            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);
        }
        return newDoorwayList;
    }

    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();
        foreach(string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }
        return newStringList;
    }

    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        foreach(RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if(roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        if (matchingRoomTemplateList.Count == 0) return null;
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    private void ClearDungeon()
    {
        if(dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
            {
                Room room = keyvaluepair.Value;
                if (room.instantiatedRoom != null)
                    Destroy(room.instantiatedRoom.gameObject);
            }
            dungeonBuilderRoomDictionary.Clear();
        }
    }

    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphs)
    {
        if(roomNodeGraphs.Count > 0)
        {
            return roomNodeGraphs[UnityEngine.Random.Range(0, roomNodeGraphs.Count)];
        }
        else
        {
            Debug.Log("No room node graphs in list");
            return null;
        }
    }

    private void LoadRoomTemplatesIntoDictionary()
    {
        roomTemplateDictionary.Clear();

        foreach(RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if(!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Template Key In " + roomTemplateList);
            }
        }
    }
}
