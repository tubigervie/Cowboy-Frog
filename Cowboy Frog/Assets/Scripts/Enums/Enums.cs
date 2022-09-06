public enum Orientation
{
    north,
    east,
    south,
    west,
    none
}

public enum AIStateType
{
    Spawn,
    Idle,
    Move,
    Die,
    Attack1,
    Attack2
}

public enum ChestSpawnEvent
{
    onRoomEntry,
    onEnemiesDefeated
}

public enum ChestSpawnPosition
{
    atSpawnerPosition,
    atPlayerPosition
}

public enum ChestState
{
    closed,
    healthItem,
    item,
    empty
}

public enum ChestType
{
    random,
    health,
    weapon
}

public enum AimDirection
{
    Up,
    UpRight,
    UpLeft,
    Left,
    Right,
    Down
}

public enum GameState
{
    gameStarted,
    playingLevel,
    engagingEnemies,
    bossStage,
    engagingBoss,
    levelCompleted,
    gameWon,
    gameLost,
    GamePaused,
    dungeonOverviewMap,
    restartGame
}

