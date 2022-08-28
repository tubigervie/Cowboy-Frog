using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] GameObject minimapIcon;
    [SerializeField] SoundEffectSO teleportSFX;
    public bool isActivated = false;
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        minimapIcon.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isActivated && collision.CompareTag(Settings.playerTag))
        {
            ActivateTeleporter();
        }
    }

    private void ActivateTeleporter()
    {
        //play some effects
        isActivated = true;
        PlayTeleportEffect();
        minimapIcon.SetActive(true);
    }

    public void PlayTeleportEffect()
    {
        SoundEffectManager.Instance.PlaySoundEffect(teleportSFX);
        animator.SetTrigger("Teleport");
    }

    public void Teleport()
    {
        StartCoroutine(TeleportRoutine());
    }

    private IEnumerator TeleportRoutine()
    {
        yield return GameManager.Instance.CallFade(0, 1, 0.5f, Color.black);
        GameManager.Instance.GetPlayer().transform.position = transform.position;
        PlayTeleportEffect();
        DungeonMap.Instance.ClearDungeonOverviewMap(false);
        yield return GameManager.Instance.CallFade(1, 0, 0.5f, Color.black);
        GameManager.Instance.GetPlayer().playerControl.EnablePlayer();
    }
}
