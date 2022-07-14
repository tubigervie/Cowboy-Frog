using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;

    // Node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25; //spacing inside GUI elements
    private const int nodeBorder = 12; //spacing outside GUI elements


    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor"); //Returns first editorWindow of type RoomNodeGraphEditor, if none, creates new window and returns instance of it
    }

    private void OnEnable()
    {
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D; //loads predefined assets bundled with Unity data
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    }

    /// <summary>
    /// Draw Editor Gui - This is how the editor draws content onto the GUI
    /// </summary>
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(new Vector2(100f, 100f), new Vector2(nodeWidth, nodeHeight)), roomNodeStyle);
        EditorGUILayout.LabelField("Node 1");
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(new Vector2(300f, 300f), new Vector2(nodeWidth, nodeHeight)), roomNodeStyle);
        EditorGUILayout.LabelField("Node 1");
        GUILayout.EndArea();
    }
}
