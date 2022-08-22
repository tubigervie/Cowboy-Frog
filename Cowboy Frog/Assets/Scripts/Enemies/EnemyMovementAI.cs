using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyMovementAI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion
    [SerializeField] private MovementDetailsSO movementDetails;
    private Enemy enemy;
    private Stack<Vector3> movementSteps = new Stack<Vector3>();
    private Vector3 playerReferencePosition;
    private Coroutine moveEnemyRoutine;
    private float currentEnemyPathRebuildCooldown;
    private WaitForFixedUpdate waitForFixedUpdate;
    [HideInInspector] public float moveSpeed;
    private bool chasePlayer = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        moveSpeed = movementDetails.moveSpeed;
    }

    private void Update()
    {
        MoveEnemy();
    }

    private void MoveEnemy()
    {
        //Movement cooldown timer
        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        //Check distance to player to see if enemy should start chasing
        if(!chasePlayer && Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
        {
            chasePlayer = true;
        }

        //Not close enough to chase player then return
        if (!chasePlayer) return;

        //if the movement cooldown timer reached or player has moved more than required distance
        //then rebuild the enemy path and move the enemy
        if(currentEnemyPathRebuildCooldown <= 0f || (Vector3.Distance(playerReferencePosition, GameManager.Instance.GetPlayer().GetPlayerPosition()) > Settings.playerMoveDistanceToRebuildPath))
        {
            currentEnemyPathRebuildCooldown = Settings.enemyPathRebuildCooldown;

            playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

            CreatePath();

            if(movementSteps != null)
            {
                enemy.idleEvent.CallIdleEvent();
                if(moveEnemyRoutine != null)
                    StopCoroutine(moveEnemyRoutine);
            }

            moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps));
        }
    }

    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();
        Grid grid = currentRoom.instantiatedRoom.grid;
        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);

        Vector3Int playerGridPosition = GetNearestNonObstaclePlayerPosition(currentRoom); 

        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        if(movementSteps != null)
        {
            movementSteps.Pop();
        }
        else
        {
            enemy.idleEvent.CallIdleEvent();
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
            for(int i = -1; i <= 1; i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    try
                    {
                        obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x + i, adjustedPlayerCellPosition.y + j];
                        if(obstacle != 0)
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

    private IEnumerator MoveEnemyRoutine(Stack<Vector3> movementSteps)
    {
        while(movementSteps != null && movementSteps.Count > 0)
        {
            Vector3 nextPosition = movementSteps.Pop();

            while(Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                enemy.movementToPositionEvent.CallMovementToPositionEvent(nextPosition, transform.position, moveSpeed, (nextPosition - transform.position).normalized);
                yield return waitForFixedUpdate; // moving the enemy using 2D physics so wait until next fixed update
            }

            yield return waitForFixedUpdate;
        }

        enemy.idleEvent.CallIdleEvent();
    }
}
