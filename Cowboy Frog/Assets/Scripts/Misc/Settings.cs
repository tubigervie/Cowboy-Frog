using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    #region UNITS
    public const float pixelsPerUnit = 16f;
    public const float tileSizePixels = 16f;
    #endregion
    #region DUNGEON BUILD SETTINGS
    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion
    #region ROOM SETTINGS
    public const int maxChildCorridors = 3; //max number of child corridors leading from a room. maximum should be 3 although this is not recommended since it can cause dungeon building
                                            //to fail since the rooms are more likely to not fit together
    public const float fadeInTime = 0.5f; // time to fade in room
    #endregion

    #region ANIMATOR PARAMETERS
    // Animator parameters - Player
    public static int aimUp = Animator.StringToHash("aimUp");
    public static int aimDown = Animator.StringToHash("aimDown");
    public static int aimUpRight = Animator.StringToHash("aimUpRight");
    public static int aimUpLeft = Animator.StringToHash("aimUpLeft");
    public static int aimRight = Animator.StringToHash("aimRight");
    public static int aimLeft = Animator.StringToHash("aimLeft");
    public static int isIdle = Animator.StringToHash("isIdle");
    public static int isMoving = Animator.StringToHash("isMoving");
    public static int rollUp = Animator.StringToHash("rollUp");
    public static int rollDown = Animator.StringToHash("rollDown");
    public static int rollLeft = Animator.StringToHash("rollLeft");
    public static int rollRight = Animator.StringToHash("rollRight");

    public static int open = Animator.StringToHash("open");
    public static float baseSpeedForPlayerAnimations = 8f;
    #endregion

    #region GAMEOBJECT TAGS
    public const string playerTag = "Player";
    public const string playerWeapon = "playerWeapon";
    public const string groundTag = "groundTilemap";
    public const string decoration1Tag = "decoration1Tilemap";
    public const string decoration2Tag = "decoration2Tilemap";
    public const string frontTag = "frontTilemap";
    public const string collisionTag = "collisionTilemap";
    public const string minimapTag = "minimapTilemap";
    #endregion

    #region FIRING CONTROL
    public const float useAimAngleDistance = 3.5f; //if the target distance is less than this then the aim angle will be used (calculated from player) else the weapon aim angle will be used
    #endregion

    #region ASTAR PATHFINDING PARAMETERS
    public const int defaultAStarMovementPenalty = 40;
    public const int preferredPathAStarMovementPenalty = 1;
    public const float playerMoveDistanceToRebuildPath = 3f;
    public const float enemyPathRebuildCooldown = 2f;
    #endregion

    #region UI PARAMETERS
    public const float uiAmmoIconSpacing = 4f;
    #endregion
}
