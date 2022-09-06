using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(MovementToPositionEvent))]
public class MoveState : AIState
{
    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public int updateFrameNumber = 1; //default value. This is set by the enemy spawner

    [SerializeField] MovementDetailsSO movementDetailsSO;

    private float moveSpeed;
    private Stack<Vector3> movementSteps = new Stack<Vector3>();
    private Vector3 playerReferencePosition;
    private Coroutine moveEnemyRoutine;
    private float currentEnemyPathRebuildCooldown;
    private WaitForFixedUpdate waitForFixedUpdate;


    public override void Awake()
    {
        base.Awake();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        moveSpeed = movementDetailsSO.moveSpeed;
    }

    public override void OnEnter()
    {
 
    }

    public override void OnExit()
    {
        if(movementSteps != null)
        {
            movementSteps.Clear();
            movementSteps = null;
        }
        if (moveEnemyRoutine != null)
        {
            StopCoroutine(moveEnemyRoutine);
            moveEnemyRoutine = null;
        }
    }

    public override void StateUpdate(float deltaTime)
    {
        MoveEnemy();
    }

    private void MoveEnemy()
    {
        //Movement cooldown timer
        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        //Only process A Star path rebuild on certain frames to spread the load between enemies
        if (Time.frameCount % Settings.targetFrameRateToSpreadPathFindingOver != updateFrameNumber) return;

        owner.ChooseState();

        //if the movement cooldown timer reached or player has moved more than required distance
        //then rebuild the enemy path and move the enemy
        if (currentEnemyPathRebuildCooldown <= 0f || (Vector3.Distance(playerReferencePosition, GameManager.Instance.GetPlayer().GetPlayerPosition()) > Settings.playerMoveDistanceToRebuildPath))
        {
            currentEnemyPathRebuildCooldown = Settings.enemyPathRebuildCooldown;

            playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

            CreatePath();

            if (movementSteps != null)
            {
                if (moveEnemyRoutine != null)
                    StopCoroutine(moveEnemyRoutine);
            }

            moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps));
        }
    }

    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();
        Vector3Int enemyGridPosition = currentRoom.instantiatedRoom.grid.WorldToCell(transform.position);

        Vector3Int playerGridPosition = GetNearestNonObstaclePlayerPosition(GameManager.Instance.GetCurrentRoom());

        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition, ignoreMovementPenalty : true);

        if (movementSteps != null)
        {
            movementSteps.Pop();
        }
    }

    private Vector3Int GetNearestNonObstaclePlayerPosition(Room currentRoom)
    {
        Vector3 playerPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

        Vector3Int playerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(playerPosition);

        Vector2Int adjustedPlayerCellPosition = new Vector2Int(playerCellPosition.x - currentRoom.templateLowerBounds.x, playerCellPosition.y - currentRoom.templateLowerBounds.y);

        int obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y];

        if (obstacle != 0) return playerCellPosition;

        //find a surrounding cell that isn-t an obstacle - required because with the 'half collision' tiles
        //the player can be on a grid square that is marked as an obstacle
        else
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    try
                    {
                        obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x + i, adjustedPlayerCellPosition.y + j];
                        if (obstacle != 0)
                        {
                            return new Vector3Int(playerCellPosition.x + i, playerCellPosition.y + j, 0);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
        return playerCellPosition;
    }

    public void SetUpdateFrameNumber(int updateFrameNumber)
    {
        this.updateFrameNumber = updateFrameNumber;
    }

    private IEnumerator MoveEnemyRoutine(Stack<Vector3> movementSteps)
    {
        while (movementSteps != null && movementSteps.Count > 0)
        {
            Vector3 nextPosition = movementSteps.Pop();
            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                movementToPositionEvent.CallMovementToPositionEvent(nextPosition, transform.position, moveSpeed, (nextPosition - transform.position).normalized);
                yield return waitForFixedUpdate; // moving the enemy using 2D physics so wait until next fixed update
            }

            yield return waitForFixedUpdate;
        }

    }

    public override int CalculateStateCost()
    {
        int cost = int.MaxValue;
        float distToPlayer = Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition());
        if (distToPlayer > maxDistance)
        {
            cost = 0;
        }
        else if(distToPlayer <= maxDistance && distToPlayer > minDistance)
        {
            cost = 1;
        }
        return cost;
    }
}
