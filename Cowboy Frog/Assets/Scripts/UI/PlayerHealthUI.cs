using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealthUI : MonoBehaviour
{
    private List<GameObject> healthHeartsList = new List<GameObject>();

    private void OnEnable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        SetHealthBar(healthEventArgs);
    }

    private void SetHealthBar(HealthEventArgs healthEventArgs)
    {
        ClearHealthBar();
        for(int i = 0; i < healthEventArgs.healthAmount; i++)
        {
            GameObject heart = Instantiate(GameResources.Instance.heartIconPrefab, transform);
            heart.GetComponent<RectTransform>().anchoredPosition = new Vector2(Settings.uiHeartIconSpacing * i, 0f);
            healthHeartsList.Add(heart);
        }
    }

    private void ClearHealthBar()
    {
        foreach(GameObject heartIcon in healthHeartsList)
        {
            Destroy(heartIcon);
        }
        healthHeartsList.Clear();
    }
}
