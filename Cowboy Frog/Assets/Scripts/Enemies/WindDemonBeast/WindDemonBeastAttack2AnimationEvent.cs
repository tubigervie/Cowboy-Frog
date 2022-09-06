using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindDemonBeastAttack2AnimationEvent : MonoBehaviour
{

    public void DisableEffect()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
