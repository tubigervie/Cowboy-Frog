using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks; //allows us to capture callbacks
using System;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    // Node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25; //spacing inside GUI elements
    private const int nodeBorder = 12; //spacing outside GUI elements
    
    //Connecting line values
    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;


    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor"); //Returns first editorWindow of type RoomNodeGraphEditor, if none, creates new window and returns instance of it
    }

    /// <summary>
    /// Open the room node graph editor window if a room node graph scriptable object is double clicked in inspector
    /// </summary>
    /// <param name="instanceID"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    [OnOpenAsset(0)] //from UnityEditor.Callbacks, makes a method called when Unity is about to open an asset (fired when double clicking on that asset)
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if(roomNodeGraph != null)
        {
            OpenWindow();
            currentRoomNodeGraph = roomNodeGraph;
            return true;
        }
        return false;
    }

    private void OnEnable()
    {
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D; //loads predefined assets bundled with Unity data
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // Load Room node types
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Draw Editor Gui - This is how the editor draws content onto the GUI
    /// </summary>
    private void OnGUI()
    {
        //If a scriptable object of type RoomNodeGraphSO has been selected then process
       if(currentRoomNodeGraph != null)
        {
            DrawDraggedLine();

            //Process Events (mouse clicks, selections, etc)
            ProcessEvents(Event.current);

            DrawRoomNodeConnections();

            //Draw Room Nodes
            DrawRoomNodes();
        }

        if (GUI.changed)
            Repaint();
    }

    private void DrawRoomNodeConnections()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if(roomNode.childrenRoomNodeIDList.Count > 0)
            {
                foreach(string childRoomId in roomNode.childrenRoomNodeIDList)
                {
                    if(currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomId))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomId]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        //calculate midway point
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        //Vector from start to end position of line
        Vector2 direction = endPosition - startPosition;

        //Calculate normalized perpendicular positions from the midpoint
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        //Calculate midpoint offset position for arrow head
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    private void DrawDraggedLine()
    {
        if(currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, 
                Color.white, null, connectingLineWidth);
        }
    }

    private void DrawRoomNodes()
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            currentRoomNodeGraph.roomNodeList[i].Draw(roomNodeStyle);
        }
        GUI.changed = true;
    }

    private void ProcessEvents(Event currentEvent)
    {
        //Get room node that mnouse is over if it's null or not currently being dragged
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
            currentRoomNode = IsMouseOverRoomNode(currentEvent);

        //if mouse isn't over a room node or currently dragging a line from a room node then process graph events
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
            ProcessRoomNodeGraphEvents(currentEvent);
        //else process room node events
        else
            currentRoomNode.ProcessEvents(currentEvent);
    }

    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for(int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
                return currentRoomNodeGraph.roomNodeList[i];
        }
        return null;
    }

    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);
            if(roomNode != null && roomNode != currentRoomNodeGraph.roomNodeToDrawLineFrom)
            {
                if(currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }
            ClearLineDrag();
        }
    }

    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if(currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
    }

    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if(currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// Process mouse down events on the room node graph (not over a node)
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //Process right click mouse down on graph event (show context menu)
        if(currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.ShowAsContext();
    }

    private void CreateRoomNode(object mousePositionObject)
    {
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    //Method overloaded type
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        //Create a new room node scriptable object asset and add to current room node graph
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        //Set room node values
        roomNode.Initialize(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);
        
        //Add room node to room node graph scriptable object asset database
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();

        currentRoomNodeGraph.OnValidate();
    }
}
