using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class StaticEventHandler
{
    //Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static event Action<RoomEnemiesDefeatedArgs> OnRoomEnemiesDefeated;

    public static event Action<PointsScoredArgs> OnPointsScored;

    public static event Action<ScoreChangedArgs> OnScoreChanged;

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

    public static void CallOnPointsScoredEvent(int points)
    {
        OnPointsScored?.Invoke(new PointsScoredArgs() { points = points });
    }

    public static void CallOnScoreChangedEvent(long score)
    {
        OnScoreChanged?.Invoke(new ScoreChangedArgs() { score = score });
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

public class PointsScoredArgs : EventArgs
{
    public int points;
}
public class ScoreChangedArgs : EventArgs
{
    public long score;
}
