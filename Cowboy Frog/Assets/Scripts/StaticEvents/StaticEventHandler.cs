using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class StaticEventHandler
{
    //Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static event Action<RoomEnemiesDefeatedArgs> OnRoomEnemiesDefeated;

    //any subscribers who are interested in whether the players entered a different room can now subscribe to this
    //event and get notified when it happens
    public static void CallRoomChangedEvent(Room room)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs() { room = room });
    }

    public static void CallRoomEnemiesDefeatedEvent(Room room)
    {
        OnRoomEnemiesDefeated?.Invoke(new RoomEnemiesDefeatedArgs() { room = room });
    }
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}

public class RoomEnemiesDefeatedArgs : EventArgs
{
    public Room room;
}
