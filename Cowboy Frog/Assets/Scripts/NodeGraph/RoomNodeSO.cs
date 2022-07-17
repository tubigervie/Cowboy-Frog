using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class RoomNodeSO : ScriptableObject
{
    public string id; //guid
    public List<string> parentRoomNodeIDList = new List<string>();
    public List<string> childrenRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeTypeSO)
    {
        this.rect = rect;
        this.id = System.Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeTypeSO;

        //Load room node type list
        this.roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        //Draw Node Box
        GUILayout.BeginArea(rect, nodeStyle);
        
        //Start region to detect popup selection changes
        EditorGUI.BeginChangeCheck();

        //if the room node has a parent or is of type entrance then display a label else display a popup
        if(parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            //Display a label that can't be changed
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            //Display a popup using the RoomNodeType name values that can be selected from (default to the currently set roomNodeType)
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            //If the room type selection has changed making child connections potentially invalid
            if((roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor) || (!roomNodeTypeList.list[selected].isCorridor
                && roomNodeTypeList.list[selection].isCorridor) || (!roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom))
            {
                if (childrenRoomNodeIDList.Count > 0)
                {
                    for (int i = childrenRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomnode(childrenRoomNodeIDList[i]);
                        if (childRoomNode != null)
                        {
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this); //makes sure any changes made in display popup gets saved

        GUILayout.EndArea();
    }

    private string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];
        for(int i = 0; i < roomArray.Length; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
        }

        return roomArray;
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
            ProcessLeftMouseDragEvent(currentEvent);
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        if(IsChildRoomValid(childID))
        {
            childrenRoomNodeIDList.Add(childID);
            return true;
        }
        return false;
    }


    private bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;

        //Check if there is already a connected boss room in the node graph
        foreach(RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
                break;
            }
        }

        //if the child node has a type of boss room and there is already a connected boss room node
        if (roomNodeGraph.GetRoomnode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;

        //If the child node has a type of none 
        if (roomNodeGraph.GetRoomnode(childID).roomNodeType.isNone)
            return false;

        //If the node already has a child with this child Id
        if (childrenRoomNodeIDList.Contains(childID))
            return false;

        //If this node ID and the child ID are the same
        if (id == childID)
            return false;

        //If the child node is already in the parentID list
        if (parentRoomNodeIDList.Contains(childID))
            return false;

        //If the child node already has a parent (design constraint)
        if (roomNodeGraph.GetRoomnode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        //If child is a corridor and this node is a corridor
        if (roomNodeGraph.GetRoomnode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        //If child is not a corridor and this node is not a corridor
        if (!roomNodeGraph.GetRoomnode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        //if adding a corridor check that this node has < the maximum permitted child corridors
        if (roomNodeGraph.GetRoomnode(childID).roomNodeType.isCorridor && childrenRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        //entrance must always be the top level parent node
        if (roomNodeGraph.GetRoomnode(childID).roomNodeType.isEntrance)
            return false;

        //if adding a room to a corridor check that this corridor node doesn't already have a room added
        if (!roomNodeGraph.GetRoomnode(childID).roomNodeType.isCorridor && childrenRoomNodeIDList.Count > 0)
            return false;

        return true;
    }

    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
            ProcessLeftClickUpEvent();
    }

    private void ProcessLeftClickUpEvent()
    {
        if(isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if(currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this; //flags object as being selected in Unity Editor Inspector
        isSelected = !isSelected;
    }

    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        if(childrenRoomNodeIDList.Contains(childID))
        {
            childrenRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        if (parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }
        return false;
    }


#endif
    #endregion
}
