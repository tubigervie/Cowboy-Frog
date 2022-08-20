using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class Door : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("Populate this with the BoxCollider2D component on the DoorCollider gameobject")]
    #endregion
    [SerializeField] private BoxCollider2D doorCollider;
    [HideInInspector] public bool isBossRoomDoor = false;
    private BoxCollider2D doorTrigger;
    private bool isOpen = false;
    private bool previouslyOpened = false;
    private Animator animator;

    private void Awake()
    {
        doorCollider.enabled = false;
        animator = GetComponent<Animator>();
        doorTrigger = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            OpenDoor();
        }
    }

    private void OnEnable()
    {
        ///When the parent gameobject is disabled (when player moves far enough away from room) the animator states gets reset. Therefore need to rrestore animator state
        animator.SetBool(Settings.open, isOpen);
    }

    // Start is called before the first frame update
    public void OpenDoor()
    {
        if(!isOpen)
        {
            isOpen = true;
            previouslyOpened = true;
            doorCollider.enabled = false;
            doorTrigger.enabled = false;
            animator.SetBool(Settings.open, true);
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.doorOpenCloseSoundEffect);
        }
    }

    public void LockDoor()
    {
        isOpen = false;
        doorCollider.enabled = true;
        doorTrigger.enabled = false;
        animator.SetBool(Settings.open, false);
    }

    public void UnlockDoor()
    {
        doorCollider.enabled = false;
        doorTrigger.enabled = true;
        
        if(previouslyOpened)
        {
            isOpen = false;
            OpenDoor();
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorCollider), doorCollider);
    }
#endif
    #endregion
}
