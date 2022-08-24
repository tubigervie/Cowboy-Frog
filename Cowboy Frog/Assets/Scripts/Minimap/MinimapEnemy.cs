using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapEnemy : MonoBehaviour
{
    private Enemy enemy;

    // Start is called before the first frame update
    public void Initialise(Enemy enemy)
    {
        this.enemy = enemy;
        this.enemy.destroyedEvent.OnDestroyed += DestroyedEvent_OnDestroyed;
    }

    private void DestroyedEvent_OnDestroyed(DestroyedEvent arg1, DestroyedEventArgs arg2)
    {
        Destroy(this.gameObject);
    }

    void Update()
    {
        if (enemy != null)
        {
            transform.position = enemy.transform.position;
        }
    }

}
