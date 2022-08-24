using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    #region Header GAMEOBJECT REFS
    [Space(10)]
    [Header("GAMEOBJECT REFS")]
    [Tooltip("Populate with the child Bar gameobject")]
    [SerializeField] private GameObject healthBar;
    #endregion Header GAMEOBJECT REFS

    bool alwaysEnabled;
    float displayTimer = 0f;

    private void Start()
    {
        DisableHealthBar();
    }

    private void Update()
    {
        if(!alwaysEnabled && displayTimer < 0f)
        {
            DisableHealthBar();
        }
        displayTimer -= Time.deltaTime;
    }

    public void SetBarAlwaysEnabledFlag(bool flag)
    {
        alwaysEnabled = flag;
    }

    public void EnableHealthBar(float healthPercent)
    {
        SetHealthBarValue(healthPercent);
        gameObject.SetActive(true);
        displayTimer = 5f;
    }

    public void DisableHealthBar()
    {
        gameObject.SetActive(false);
    }

    public void SetHealthBarValue(float healthPercent)
    {
        healthBar.transform.localScale = new Vector3(healthPercent, 1f, 1f);
    }
}
