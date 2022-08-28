using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in audioSources)
            audio.Pause();
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in audioSources)
            audio.Play();
    }
}
