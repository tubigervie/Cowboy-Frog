using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Paintable : MonoBehaviour
{
    private Tilemap paintMap;
    private TileBase playerPaintTile;
    private InstantiatedRoom instantiatedRoom;
    private bool canPaint;
    private List<PaintTimers> activePaintTimers = new List<PaintTimers>();

    public bool TileIsPainted(Vector3 pos)
    {
        Vector3Int cellPosition = instantiatedRoom.grid.WorldToCell(pos);
        return paintMap.GetTile(cellPosition) != null;
    }

    private void OnEnable()
    {
        instantiatedRoom = GetComponentInParent<InstantiatedRoom>();
        paintMap = GetComponent<Tilemap>();
        playerPaintTile = GameResources.Instance.paintableTileBase;
        canPaint = true;
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        if(roomChangedEventArgs.room == instantiatedRoom.room)
        {
            canPaint = true;
        }
        else
        {
            canPaint = false;
        }
    }

    private void FixedUpdate()
    {
        float timeStep = Time.fixedDeltaTime;
        if(activePaintTimers.Count != 0)
        {
            foreach(PaintTimers paintTimer in activePaintTimers)
            {
                paintTimer.timeToExpire -= timeStep;
                if(paintTimer.timeToExpire <= 0)
                {
                    paintMap.SetTile(paintTimer.tilePos, null);
                }
            }
        }
    }

    public void PaintTile(Vector3 position)
    {
        Vector3Int cellPosition = instantiatedRoom.grid.WorldToCell(position);
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector3Int currTile = new Vector3Int(cellPosition.x + i, cellPosition.y + j);
                paintMap.SetTile(currTile, playerPaintTile);
                activePaintTimers.Add(new PaintTimers(currTile, 2.5f));
            }
        }
    }

    private class PaintTimers
    {
        public Vector3Int tilePos;
        public float timeToExpire;

        public PaintTimers(Vector3Int tilePos, float timeToExpire)
        {
            this.tilePos = tilePos;
            this.timeToExpire = timeToExpire;
        }
    }
}
