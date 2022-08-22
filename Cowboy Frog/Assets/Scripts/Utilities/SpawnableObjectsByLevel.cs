using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableObjectsByLevel<T>
{
    public DungeonLevelSO dungeonLevel;
    public List<SpawnableObjectRatio<T>> spawnableObjectRatioList;
}
