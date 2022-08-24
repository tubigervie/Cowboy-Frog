using System.Collections;
using UnityEngine.Rendering;
using UnityEngine;
using System;

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
[RequireComponent(typeof(MaterializeEffect))]
[RequireComponent(typeof(DealContactDamage))]
#endregion REQUIRE COMPONENTS
[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyDetailsSO enemyDetails;
    public EnemyMovementAI enemyMovementAI;
    public MovementToPositionEvent movementToPositionEvent;
    public IdleEvent idleEvent;
    public HealthEvent healthEvent;
    public DestroyedEvent destroyedEvent;
    public Health health;
    public Destroyed destroyed;

    public Room currentRoom;
    [HideInInspector] public MaterializeEffect materializeEffect;
    [HideInInspector] public CircleCollider2D circleCollider2D;
    [HideInInspector] public PolygonCollider2D polygonCollider2D;
    [HideInInspector] public Animator animator;
    [HideInInspector] public SpriteRenderer[] spriteRendererArray;


    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        destroyed = GetComponent<Destroyed>();
        destroyedEvent = GetComponent<DestroyedEvent>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        enemyMovementAI = GetComponent<EnemyMovementAI>();
        idleEvent = GetComponent<IdleEvent>();
        materializeEffect = GetComponent<MaterializeEffect>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        spriteRendererArray = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        healthEvent.OnHealthChanged += HealthEvent_OnHealthLost;
    }

    private void OnDisable()
    {
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthLost;
    }

    private void HealthEvent_OnHealthLost(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        if(healthEventArgs.healthAmount <= 0)
        {
            EnemyDestroyed();
        }
    }

    private void EnemyDestroyed()
    {
        if (GameManager.Instance.gameState == GameState.engagingBoss)
        {
            CinemachineTarget.Instance.RemoveFromTargetGroup(this.transform);
        }
        destroyedEvent.CallDestroyedEvent(false);
    }

    public void Initialization(EnemyDetailsSO enemyDetails, int enemySpawnNumber, DungeonLevelSO dungeonLevel, Room currentRoom)
    {
        this.enemyDetails = enemyDetails;
        this.currentRoom = currentRoom;
        SetEnemyMovementUpdateFrame(enemySpawnNumber);
        SetEnemyStartingHealth(dungeonLevel);
        StartCoroutine(MaterializeEnemy());
        GameObject minimapEnemy = Instantiate(GameResources.Instance.enemyMinimapPrefab, Minimap.Instance.transform);
        minimapEnemy.GetComponent<MinimapEnemy>().Initialise(this);
    }

    private void SetEnemyStartingHealth(DungeonLevelSO dungeonLevel)
    {
        foreach(EnemyHealthDetails enemyHealthDetails in enemyDetails.enemyHealthDetailsArray)
        {
            if(enemyHealthDetails.dungeonLevel == dungeonLevel)
            {
                health.SetStartingHealth(enemyHealthDetails.enemyHealthAmount);
                return;
            }
        }
        health.SetStartingHealth(Settings.defaultEnemyHealth);
    }

    private void SetEnemyMovementUpdateFrame(int enemySpawnNumber)
    {
        this.enemyMovementAI.SetUpdateFrameNumber(enemySpawnNumber % Settings.targetFrameRateToSpreadPathFindingOver);
    }

    private IEnumerator MaterializeEnemy()
    {
        EnemyEnable(false);

        yield return StartCoroutine(materializeEffect.MaterialiseRoutine(enemyDetails.enemyMaterializeShader, enemyDetails.enemyMaterializeColor,
            enemyDetails.enemyMaterializeTime, spriteRendererArray, enemyDetails.enemyStandardMaterial));
        if (enemyDetails.healthBarAlwaysOn) 
            health.healthBar.EnableHealthBar(health.GetHealthPercent());
        EnemyEnable(true);
    }

    private void EnemyEnable(bool isEnabled)
    {
        circleCollider2D.enabled = isEnabled;
        polygonCollider2D.enabled = isEnabled;

        enemyMovementAI.enabled = isEnabled;
    }
}
