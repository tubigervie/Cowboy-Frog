using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{

    [System.Serializable]
    private struct RangeByLevel
    {
        public DungeonLevelSO dungeonLevelSO;
        [Range(0, 100)] public int min;
        [Range(0, 100)] public int max;
    }

    #region Header CHEST PREFAB
    [Space(10)]
    [Header("CHEST PREFAB")]
    #endregion
    [SerializeField] private GameObject chestPrefab;

    [Header("CHEST SPAWN CHANCE")]
    [SerializeField][Range(0, 100)] private int chestSpawnChanceMin;
    [SerializeField] [Range(0, 100)] private int chestSpawnChanceMax;
    [SerializeField] ChestType chestType = ChestType.random;

    [Tooltip("You can override the chest spawn chance by dungeon level")]
    [SerializeField] private List<RangeByLevel> chestSpawnChanceByLevellist;

    [Space(10)]
    [Header("CHEST SPAWN DETAILS")]
    [SerializeField] private ChestSpawnEvent chestSpawnEvent;
    [SerializeField] private ChestSpawnPosition chestSpawnPosition;
    [Tooltip("The minimum number of items to spawn (a maximum of 1 of each type will be spawned)")]
    [SerializeField] [Range(0, 3)] private int numberOfItemsToSpawnMin;
    [SerializeField] [Range(0, 3)] private int numberOfItemsToSpawnMax;

    [Space(10)]
    [Header("CHEST CONTENT DETAILS")]
    [Tooltip("The items to spawn for each dungeon level and their spawn ratios")]
    [SerializeField] private List<SpawnableObjectsByLevel<WeaponDetailsSO>> weaponSpawnByLevelList;
    [Tooltip("The range of health to spawn for each level")]
    [SerializeField] private List<RangeByLevel> healthSpawnByLevelList;

    private bool chestSpawned = false;
    private Room chestRoom;

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
    }

    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        if(roomEnemiesDefeatedArgs.room == chestRoom)
            SpawnChest();
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        if(chestRoom == null)
        {
            chestRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        if (roomChangedEventArgs.room != chestRoom) return;

        if(!chestSpawned && chestSpawnEvent == ChestSpawnEvent.onRoomEntry && chestRoom == roomChangedEventArgs.room)
        {
            SpawnChest();
        }
    }

    private void SpawnChest()
    {
        chestSpawned = true;
        if (!RandomSpawnChest()) return;
        int healthNum = 0;
        int weaponNum = 0;
        switch (chestType)
        {
            case ChestType.random:
                GetItemsToSpawn(out healthNum, out weaponNum);
                break;
            case ChestType.health:
                healthNum = 1;
                break;
            case ChestType.weapon:
                weaponNum = 1;
                break;
        }

        GameObject chestGameObject = Instantiate(chestPrefab, this.transform);

        if(chestSpawnPosition == ChestSpawnPosition.atSpawnerPosition)
        {
            chestGameObject.transform.position = this.transform.position;
        }
        else if(chestSpawnPosition == ChestSpawnPosition.atPlayerPosition)
        {
            Vector3 spawnPosition = HelperUtilities.GetSpawnPositionNearestToPlayer(GameManager.Instance.GetPlayer().transform.position);
            chestGameObject.transform.position = spawnPosition;
        }

        Chest chest = chestGameObject.GetComponent<Chest>();

        if(chestSpawnEvent == ChestSpawnEvent.onRoomEntry)
        {
            chest.Initialize(false, GetHealthToSpawn(healthNum), GetWeaponDetailsToSpawn(weaponNum));
        }
        else
        {
            chest.Initialize(true, GetHealthToSpawn(healthNum), GetWeaponDetailsToSpawn(weaponNum));
        }
    }

    private WeaponDetailsSO GetWeaponDetailsToSpawn(int weaponNum)
    {
        if (weaponNum == 0) return null;
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(weaponSpawnByLevelList);
        WeaponDetailsSO weaponDetails = weaponRandom.GetItem();
        return weaponDetails;
    }

    private int GetHealthToSpawn(int healthNum)
    {
        if (healthNum == 0) return 0;

        foreach(RangeByLevel spawnPercentByLevel in healthSpawnByLevelList)
        {
            if(spawnPercentByLevel.dungeonLevelSO == GameManager.Instance.GetCurrentDungeonLevel())
            {
                return UnityEngine.Random.Range(spawnPercentByLevel.min, spawnPercentByLevel.max + 1);
            }
        }
        return 0;
    }

    private void GetItemsToSpawn(out int healthNum, out int weaponNum)
    {
        healthNum = 0;
        weaponNum = 0;

        int numberOfItemsToSpawn = UnityEngine.Random.Range(numberOfItemsToSpawnMin, numberOfItemsToSpawnMax + 1);
        Debug.Log("number of items to spawn " + numberOfItemsToSpawn);
        int choice;

        if (numberOfItemsToSpawn == 1)
        {
            choice = UnityEngine.Random.Range(0, 2);
            if (choice == 0) { weaponNum++; return; }
            if (choice == 1) { healthNum++; return; }
            return;
        }
        else if(numberOfItemsToSpawn == 2)
        {
            //choice = UnityEngine.Random.Range(0, 2);
            //if (choice == 0) { weaponNum++; return; }
            //if (choice == 1) { healthNum++; return; }
            weaponNum++;
            healthNum++;
            return;
        }
    }

    private bool RandomSpawnChest()
    {
        int chancePercent = UnityEngine.Random.Range(chestSpawnChanceMin, chestSpawnChanceMax + 1);

        foreach(RangeByLevel rangeByLevel in chestSpawnChanceByLevellist)
        {
            if(rangeByLevel.dungeonLevelSO == GameManager.Instance.GetCurrentDungeonLevel())
            {
                chancePercent = UnityEngine.Random.Range(rangeByLevel.min, rangeByLevel.max + 1);
                break;
            }
        }

        int randomPercent = UnityEngine.Random.Range(1, 100 + 1);

        return (randomPercent <= chancePercent);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestPrefab), chestPrefab);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(chestSpawnChanceMin), chestSpawnChanceMin, nameof(chestSpawnChanceMax), chestSpawnChanceMax, true);

        if(chestSpawnChanceByLevellist != null && chestSpawnChanceByLevellist.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(chestSpawnChanceByLevellist), chestSpawnChanceByLevellist);

            foreach(RangeByLevel rangeByLevel in chestSpawnChanceByLevellist)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevelSO), rangeByLevel.dungeonLevelSO);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }

        HelperUtilities.ValidateCheckPositiveRange(this, nameof(numberOfItemsToSpawnMin), numberOfItemsToSpawnMin, nameof(numberOfItemsToSpawnMax), numberOfItemsToSpawnMax, true);
    }

#endif
    #endregion Validation
}
