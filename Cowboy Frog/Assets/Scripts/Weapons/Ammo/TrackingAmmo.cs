using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TrackingAmmo : MonoBehaviour, IFireable
{
    [Tooltip("Populate with child TrailRenderer component")]
    [SerializeField] private TrailRenderer trailRenderer;
    //private float ammoRange = 0f;
    private float ammoSpeed;
    private Vector3 fireDirectionVector;
    private SpriteRenderer spriteRenderer;
    private AmmoDetailsSO ammoDetails;
    private bool isTracking = false;
    private float trackingDistance = 2f;
    private Transform target;
    private Health bulletHealth;

    float startTrackingTimer = 0f;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void InitializeAmmo(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirectionVector, bool overrideAmmoMovement = false)
    {
    }

    public void InitializeAmmo(AmmoDetailsSO ammoDetails, Transform target)
    {
        this.bulletHealth = GetComponent<Health>();
        this.bulletHealth.SetStartingHealth(30);
        this.bulletHealth.healthEvent.OnHealthChanged += HealthEvent_OnHealthLost; 
        this.isTracking = true;
        this.ammoDetails = ammoDetails;
        this.ammoSpeed = ammoDetails.ammoSpeed;
        this.target = target;
        gameObject.SetActive(true);
        if (ammoDetails.isAmmoTrail)
        {
            trailRenderer.gameObject.SetActive(true);
            trailRenderer.emitting = true;
            trailRenderer.material = ammoDetails.ammoTrailMaterial;
            trailRenderer.startWidth = ammoDetails.ammoTrailStartWidth;
            trailRenderer.endWidth = ammoDetails.ammoTrailEndWidth;
            trailRenderer.time = ammoDetails.ammoTrailTime;
        }
        else
        {
            trailRenderer.emitting = false;
            trailRenderer.gameObject.SetActive(false);
        }
    }

    private void HealthEvent_OnHealthLost(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        if (healthEventArgs.healthAmount <= 0)
        {
            DisableAmmo();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startTrackingTimer < 0)
        {
            isTracking = true;
        }

        if (target != null && isTracking)
        {
            fireDirectionVector = (target.position - transform.position).normalized;
            if (Vector3.Distance(target.position, transform.position) < trackingDistance)
            {
                isTracking = false;
                startTrackingTimer = Random.Range(.5f, 1f);
            }
        }
        Vector3 horizontalDistanceVector = fireDirectionVector * ammoSpeed * Time.deltaTime;
        transform.position += horizontalDistanceVector;

        startTrackingTimer -= Time.deltaTime;
    }

    private void SetAmmoMaterial(Material ammoMaterial)
    {
        spriteRenderer.material = ammoMaterial;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DealDamage(collision);
        AmmoHitEffect();
    }

    private void DisableAmmo()
    {
        gameObject.SetActive(false);
    }

    private void DealDamage(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();
        if (health != null) health.TakeDamage(ammoDetails.ammoDamage);
    }

    private void AmmoHitEffect()
    {
        if (ammoDetails.ammoHitEffect != null && ammoDetails.ammoHitEffect.ammoHitEffectPrefab != null)
        {
            AmmoHitEffect ammoHitEffect = (AmmoHitEffect)PoolManager.Instance.ReuseComponent(ammoDetails.ammoHitEffect.ammoHitEffectPrefab,
                transform.position, Quaternion.identity);

            ammoHitEffect.SetHitEffect(ammoDetails.ammoHitEffect);

            ammoHitEffect.gameObject.SetActive(true);
        }
    }
}
