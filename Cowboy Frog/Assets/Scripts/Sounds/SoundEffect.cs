using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class SoundEffect : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if(audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    private void OnDisable()
    {
        if(audioSource.clip != null)
        {
            audioSource.Stop();
        }
    }

    public void SetSound(SoundEffectSO soundEffect)
    {
        audioSource.pitch = UnityEngine.Random.Range(soundEffect.soundEffectPitchRandomVariationMin, soundEffect.soundEffectPitchRandomVariationMax);
        audioSource.volume = soundEffect.soundEffectVolume;
        audioSource.clip = soundEffect.soundEffectClip;
    }

}
