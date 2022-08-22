using System.Collections;
using UnityEngine.Rendering;
using UnityEngine;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyMovementAI))]
[RequireComponent(typeof(MovementToPositionEvent))]
[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(AnimateEnemy))]
#endregion REQUIRE COMPONENTS
[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyDetailsSO enemyDetails;
    public EnemyMovementAI enemyMovementAI;
    public MovementToPositionEvent movementToPositionEvent;
    public IdleEvent idleEvent;

    [HideInInspector] public CircleCollider2D circleCollider2D;
    [HideInInspector] public PolygonCollider2D polygonCollider2D;
    [HideInInspector] public Animator animator;
    [HideInInspector] public SpriteRenderer[] spriteRendererArray;

    private void Awake()
    {
        circleCollider2D = GetComponent<CircleCollider2D>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        enemyMovementAI = GetComponent<EnemyMovementAI>();
        idleEvent = GetComponent<IdleEvent>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        spriteRendererArray = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public void Initialization(EnemyDetailsSO enemyDetails, int enemySpawnNumber, DungeonLevelSO dungeonLevel)
    {
        this.enemyDetails = enemyDetails;
        this.enemyMovementAI.SetUpdateFrameNumber(enemySpawnNumber % Settings.targetFrameRateToSpreadPathFindingOver);
    }
}
