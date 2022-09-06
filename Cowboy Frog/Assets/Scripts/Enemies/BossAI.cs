using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DestroyedEvent))]
[RequireComponent(typeof(DieState))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(MoveState))]
[RequireComponent(typeof(IdleState))]
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class BossAI : MonoBehaviour
{
    [HideInInspector] public DestroyedEvent destroyedEvent;
    [HideInInspector] public EnemyDetailsSO enemyDetails;
    [HideInInspector] public Health health;
    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public Animator animator;
    [SerializeField] AIState currentState;
    Dictionary<AIStateType, AIState> stateMap = new Dictionary<AIStateType, AIState>();
    
    BoxCollider2D boxCollider2D;

    bool isEnabled = false;

    private void Awake()
    {
        health = GetComponent<Health>();
        healthEvent = GetComponent<HealthEvent>();
        destroyedEvent = GetComponent<DestroyedEvent>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        AIState[] states = GetComponents<AIState>();
        foreach(AIState state in states)
        {
            stateMap.Add(state.GetStateType(), state);
        }
    }

    public void Initialization(EnemyDetailsSO enemyDetails,DungeonLevelSO dungeonLevel)
    {
        this.enemyDetails = enemyDetails;
        SetEnemyStartingHealth(dungeonLevel);
        BossEnable(false);
        ChangeState(AIStateType.Spawn);
    }

    private void SetEnemyStartingHealth(DungeonLevelSO dungeonLevel)
    {
        foreach (EnemyHealthDetails enemyHealthDetails in enemyDetails.enemyHealthDetailsArray)
        {
            if (enemyHealthDetails.dungeonLevel == dungeonLevel)
            {
                health.SetStartingHealth(enemyHealthDetails.enemyHealthAmount);
                return;
            }
        }
        health.SetStartingHealth(Settings.defaultEnemyHealth);
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
        if (healthEventArgs.healthAmount <= 0)
        {
            ChangeState(AIStateType.Die);
            destroyedEvent.CallDestroyedEvent(false, health.GetStartingHealth());
        }
    }

    public void ChooseState()
    {
        AIState chosenState = null;
        int minCost = int.MaxValue;
        foreach(AIState state in stateMap.Values)
        {
            int stateCost = state.CalculateStateCost(); 
            if(stateCost < minCost)
            {
                minCost = stateCost;
                chosenState = state;
            }
        }
        ChangeState(chosenState.GetStateType());
    }

    public void ChangeState(AIStateType newState)
    {
        if(currentState != null)
        {
            currentState.OnExit();
        }

        foreach(AIStateType state in stateMap.Keys)
        {
            if(state == newState)
            {
                currentState = stateMap[state];
                currentState.OnEnter();
                return;
            }
        }
        Debug.LogError("Could not find AI state: " + newState + " in " + this.gameObject + "'s state map.");
    }

    private void Update()
    {
        if (!isEnabled) return;
        if(currentState == null)
        {
            ChooseState();
        }
        if(currentState != null)
        {
            currentState.StateUpdate(Time.deltaTime);
        }
    }

    public void BossEnable(bool isEnabled)
    {
        this.isEnabled = isEnabled;
        boxCollider2D.enabled = isEnabled;
    }

}
