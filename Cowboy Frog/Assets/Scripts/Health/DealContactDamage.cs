using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DealContactDamage : MonoBehaviour
{
    #region Header DEAL DAMAGE
    [Space(10)]
    [Header("DEAL DAMAGE")]
    #endregion
    [Tooltip("The contact damage to deal (is overridden by the receiver)")]
    [SerializeField] private int contactDamageAmount;
    [Tooltip("Specify what layers objects should be on to receive contact damage")]
    [SerializeField] private LayerMask layerMask;
    private bool isColliding = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isColliding) return;
        ContactDamage(collision);
    }

    //Trigger contact damage when staying within a collider
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isColliding) return;
        ContactDamage(collision);
    }

    private void ContactDamage(Collider2D collision)
    {
        //if the collision object isn't in specified layer then return using bitwise comparison
        int collisionObjectLayerMask = (1 << collision.gameObject.layer);

        if ((layerMask.value & collisionObjectLayerMask) == 0) return;

        ReceiveContactDamage receiveContactDamage = collision.gameObject.GetComponent<ReceiveContactDamage>();

        if(receiveContactDamage != null)
        {
            isColliding = true;
            Invoke("ResetContactCollision", Settings.contactDamageCollisionResetDelay);
            receiveContactDamage.TakeContactDamage(contactDamageAmount);
        }
    }

    private void ResetContactCollision()
    {
        isColliding = false;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(contactDamageAmount), contactDamageAmount, true);
    }
#endif
    #endregion

}
