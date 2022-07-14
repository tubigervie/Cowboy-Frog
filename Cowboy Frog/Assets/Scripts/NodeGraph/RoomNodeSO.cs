using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id; //guid
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childrenRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
}
