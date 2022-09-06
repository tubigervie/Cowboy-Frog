using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class AIState : MonoBehaviour
{
    protected BossAI owner;

    [SerializeField] AIStateType state;
    [SerializeField] protected float stateCooldownMin;
    [SerializeField] protected float stateCooldownMax;

    protected float stateCooldownTimer;
    
    public virtual void Awake()
    {
        owner = GetComponent<BossAI>();
    }

    public virtual void Update()
    {
        stateCooldownTimer -= Time.deltaTime;
    }

    public abstract void OnEnter();

    public abstract void OnExit();

    public abstract int CalculateStateCost();

    public abstract void StateUpdate(float deltaTime);

    public AIStateType GetStateType()
    {
        return state;
    }
}
