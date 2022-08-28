using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MaterializeEffect))]
public class Chest : MonoBehaviour, IUseable
{
    #region Tooltip
    [Tooltip("Set this to the color to be used for the materialization effect")]
    #endregion
    [ColorUsage(false, true)]
    [SerializeField] private Color materializeColor;
    [Tooltip("Set this to the time it will take to materialize the chest")]
    [SerializeField] private float materializeTime = 2f;
    [SerializeField] private Transform itemSpawnPoint;
    
    //move this out eventually
    [SerializeField] private int healthToGive;
    [SerializeField] private WeaponDetailsSO itemToProvide;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private MaterializeEffect materializeEffect;
    private bool isEnabled = false;
    private ChestState chestState = ChestState.closed;
    private GameObject chestItemGameObject;
    private ChestItem chestItem;
    private TextMeshPro messageTextTMP;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        materializeEffect = GetComponent<MaterializeEffect>();
        messageTextTMP = GetComponentInChildren<TextMeshPro>();
    }

    public void Initialize(bool shouldMaterialize, int healthToRecover, WeaponDetailsSO itemToProvide)
    {
        this.healthToGive = healthToRecover;
        this.itemToProvide = itemToProvide;
        if(shouldMaterialize)
        {
            StartCoroutine(MaterializeChest());
        }
        else
        {
            EnableChest();
        }
    }

    private IEnumerator MaterializeChest()
    {
        SpriteRenderer[] spriteRendererArray = new SpriteRenderer[] { spriteRenderer };
        yield return StartCoroutine(materializeEffect.MaterialiseRoutine(GameResources.Instance.materializeShader, materializeColor, materializeTime,
            spriteRendererArray, GameResources.Instance.litMaterial));
        EnableChest();
    }

    private void EnableChest()
    {
        isEnabled = true;
    }

    public void Use()
    {
        if (!isEnabled) 
            return;
        switch (chestState)
        {
            case ChestState.closed:
                OpenChest();
                break;
            case ChestState.healthItem:
                CollectHealthItem();
                break;
            case ChestState.item:
                CollectItem();
                break;
            case ChestState.empty:
                break;
            default:
                return;
        }
    }

    private void CollectItem()
    {
        if (chestItem == null || !chestItem.isItemMaterialized) return;

        WeaponDetailsSO replacedWeapon = GameManager.Instance.GetPlayer().SwapWeapon(itemToProvide);
        Debug.Log("swapping chest item with " + replacedWeapon.weaponName);
        Destroy(chestItemGameObject);
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.itemPickup);
        if(replacedWeapon != null)
        {
            itemToProvide = replacedWeapon;
        }
        UpdateChestState();
    }

    private void CollectHealthItem()
    {
        if (chestItem == null || !chestItem.isItemMaterialized) return;

        GameManager.Instance.GetPlayer().health.AddHealth(healthToGive);

        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);

        healthToGive = 0;

        Destroy(chestItemGameObject);

        UpdateChestState();
    }

    private void OpenChest()
    {
        animator.SetBool(Settings.use, true);

        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.chestOpen);

        UpdateChestState();
    }

    private void UpdateChestState()
    {
        if(healthToGive != 0)
        {
            chestState = ChestState.healthItem;
            InstantiateHealthItem();
        }
        else if(itemToProvide != null)
        {
            chestState = ChestState.item;
            InstantiateWeapItem();
        }
    }

    private void InstantiateWeapItem()
    {
        InstantiateItem();
        chestItem.Initialize(itemToProvide.weaponSprite, itemToProvide.weaponName, itemSpawnPoint.position, materializeColor);
    }

    private void InstantiateHealthItem()
    {
        InstantiateItem();
        chestItem.Initialize(GameResources.Instance.heartIcon, healthToGive.ToString() + " HEART", itemSpawnPoint.position, materializeColor);
    }

    private void InstantiateItem()
    {
        chestItemGameObject = Instantiate(GameResources.Instance.chestItemPrefab, this.transform);
        chestItem = chestItemGameObject.GetComponent<ChestItem>();
    }
}
