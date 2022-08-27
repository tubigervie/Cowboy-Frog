using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class Ammo : MonoBehaviour, IFireable
{
    [Tooltip("Populate with child TrailRenderer component")]
    [SerializeField] private TrailRenderer trailRenderer;
    private float ammoRange = 0f;
    private float ammoSpeed;
    private Vector3 fireDirectionVector;
    private float fireDirectionAngle;
    private SpriteRenderer spriteRenderer;
    private AmmoDetailsSO ammoDetails;
    private float ammoChargeTimer;
    private bool isAmmoMaterialSet = false;
    private bool overrideAmmoMovement;
    private InstantiatedRoom currentRoom;

    [SerializeField] float frequency = 2;
    [SerializeField] float amplitude = 2;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentRoom = GameManager.Instance.GetCurrentRoom().instantiatedRoom;
    }

    private void Update()
    {
        if (ammoChargeTimer > 0f)
        {
            ammoChargeTimer -= Time.deltaTime;
            return;
        }
        else if (!isAmmoMaterialSet)
        {
            SetAmmoMaterial(ammoDetails.ammoMaterial);
            isAmmoMaterialSet = true;
        }

        //Calculate distance vector to move ammo (small ammount each frame)
        Vector3 horizontalDistanceVector = fireDirectionVector * ammoSpeed * Time.deltaTime;
        transform.position += horizontalDistanceVector;

        //if(currentRoom.paintMap != null)
        //    currentRoom.paintMap.PaintTile(transform.position);

        ammoRange -= horizontalDistanceVector.magnitude;
        if (ammoRange < 0f)
        {
            DisableAmmo();
        }
    }

    Vector3 GetPerpendicularDirection(Vector3 direction)
    {
        return new Vector3(-direction.y, direction.x, 0);
    }

    private void DisableAmmo()
    {
        gameObject.SetActive(false);
    }

    private void SetAmmoMaterial(Material ammoMaterial)
    {
        spriteRenderer.material = ammoMaterial;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DealDamage(collision);
        AmmoHitEffect();
        DisableAmmo();
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

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void InitializeAmmo(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirectionVector, bool overrideAmmoMovement = false)
    {
        this.ammoDetails = ammoDetails;
        SetFireDirection(ammoDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);
        spriteRenderer.sprite = ammoDetails.ammoSprite;
        if(ammoDetails.ammoChargeTime > 0f)
        {
            ammoChargeTimer = ammoDetails.ammoChargeTime;
            SetAmmoMaterial(ammoDetails.ammoChargeMaterial);
            isAmmoMaterialSet = false;
        }
        else
        {
            ammoChargeTimer = 0f;
            SetAmmoMaterial(ammoDetails.ammoMaterial);
            isAmmoMaterialSet = true;
        }

        this.ammoRange = ammoDetails.ammoRange;
        this.ammoSpeed = ammoSpeed;
        this.overrideAmmoMovement = overrideAmmoMovement;
        this.fireDirectionAngle = HelperUtilities.GetAngleFromVector(fireDirectionVector);

        gameObject.SetActive(true);

        if(ammoDetails.isAmmoTrail)
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

    /// <summary>
    /// Set ammo fire direction and angle based on the input angle and direction adjusted by the random spread
    /// </summary>
    /// <param name="ammoDetails"></param>
    /// <param name="aimAngle"></param>
    /// <param name="weaponAimAngle"></param>
    /// <param name="weaponAimDirectionVector"></param>
    private void SetFireDirection(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        //Get a random spread toggle of 1 or -1
        int spreadToggle = UnityEngine.Random.Range(0, 2) * 2 - 1;
        float spread = UnityEngine.Random.Range(ammoDetails.minSpread, ammoDetails.maxSpread);
        fireDirectionAngle = (weaponAimDirectionVector.magnitude < Settings.useAimAngleDistance) ? aimAngle : weaponAimAngle;

        //Adjust ammo fire angle by random spread
        fireDirectionAngle += spreadToggle * spread;

        //Set ammo rotation
        transform.eulerAngles = new Vector3(0f, 0f, fireDirectionAngle);

        //Set ammo fire direction
        fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(trailRenderer), trailRenderer);
    }
#endif
    #endregion Validation
}
