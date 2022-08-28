using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthEvent))]
[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    private int startingHealth;
    private int currentHealth;
    public HealthEvent healthEvent;
    private Player player;
    private Coroutine immunityCoroutine;
    private bool isImmuneAfterHit = false;
    private float immunityTime = 0f;
    private SpriteRenderer[] spriteRenderers = null;
    private const float spriteFlashInterval = 0.2f;
    private WaitForSeconds WaitForSecondsSpriteFlashInterval = new WaitForSeconds(spriteFlashInterval);

    [HideInInspector] public HealthBar healthBar;
    [HideInInspector] public bool alwaysDisplayHealthBar = false;
    [HideInInspector] public bool isDamageable = true;
    [HideInInspector] public Enemy enemy;
    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        healthBar = GetComponentInChildren<HealthBar>();
    }

    private void Start()
    {
        //Trigger a health event for UI update
        CallHealthEvent(0);

        //Attempt to load enemy / player components
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();

        if(player != null)
        {
            if(player.playerDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = player.playerDetails.hitImmunityTime;
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            }
        }
        else if(enemy != null)
        {
            if(enemy.enemyDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = enemy.enemyDetails.hitImmunityTime;
                spriteRenderers = enemy.spriteRendererArray;
            }

            if(enemy.enemyDetails.healthBarAlwaysOn)
            {
                alwaysDisplayHealthBar = true;
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        bool isRolling = false;

        if (player != null)
            isRolling = player.playerControl.isPlayerRolling;
        
        if(isDamageable && !isRolling)
        {
            if (player != null && player.playerDetails.hurtSFX != null)
                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.hurtSFX);
            currentHealth -= damageAmount;
            CallHealthEvent(damageAmount);
            PostHitImmunity();
        }
        //else Avoided damage due to immunity
    }

    private void PostHitImmunity()
    {
        if (gameObject.activeSelf == false) return;
        if(isImmuneAfterHit)
        {
            if (immunityCoroutine != null)
                StopCoroutine(immunityCoroutine);
            immunityCoroutine = StartCoroutine(PostHitImmunityCoroutine(immunityTime, spriteRenderers));
        }
    }

    private IEnumerator PostHitImmunityCoroutine(float immunityTime, SpriteRenderer[] spriteRenderers)
    {
        int iterations = Mathf.RoundToInt(immunityTime / spriteFlashInterval / 2f); //use sprite flash interval twice for every iteration

        isDamageable = false;
        while(iterations > 0)
        {
            foreach(SpriteRenderer spriteRenderer in spriteRenderers)
                spriteRenderer.color = Color.red;
            yield return WaitForSecondsSpriteFlashInterval;
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                spriteRenderer.color = Color.white;
            yield return WaitForSecondsSpriteFlashInterval;
            iterations--;
            yield return null;
        }
        isDamageable = true;
    }

    private void CallHealthEvent(int damageAmount)
    {
        healthEvent.CallHealthChangedEvent(((float)currentHealth / (float)startingHealth), currentHealth, damageAmount);
        if(healthBar != null)
            healthBar.EnableHealthBar(GetHealthPercent());
    }

    public float GetHealthPercent()
    {
        return (float)currentHealth / (float)startingHealth;
    }

    public void SetStartingHealth(int startingHealth)
    {
        this.startingHealth = startingHealth;
        currentHealth = startingHealth;
        if(alwaysDisplayHealthBar && healthBar != null)
        {
            healthBar.SetBarAlwaysEnabledFlag(true);
            healthBar.EnableHealthBar(GetHealthPercent());
        }
    }

    public int GetStartingHealth()
    {
        return startingHealth;
    }


    public void AddHealth(int healthToAdd)
    {
        int totalHealth = currentHealth + healthToAdd;
        if(totalHealth > startingHealth)
        {
            currentHealth = startingHealth;
        }
        else
        {
            currentHealth = totalHealth;
        }
        CallHealthEvent(0);
    }
}
