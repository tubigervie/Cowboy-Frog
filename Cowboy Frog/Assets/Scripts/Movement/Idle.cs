using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class Idle : MonoBehaviour
{
    private Rigidbody2D rb;
    private IdleEvent idleEvent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        idleEvent = GetComponent<IdleEvent>();
    }

    private void OnEnable()
    {
        idleEvent.OnIdle += IdleEvent_OnIdle;
    }

    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {
        MoveRigidbody();
    }

    private void MoveRigidbody()
    {
        //ensure the rb collision detection is set to continuous
        rb.velocity = Vector2.zero;
    }
}
