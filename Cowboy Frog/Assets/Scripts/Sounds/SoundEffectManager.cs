using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SoundEffectManager : SingletonMonobehaviour<SoundEffectManager>
{
    public int soundsVolume = 8;

    private void Start()
    {
        SetSoundsVolume(soundsVolume);
    }

    public void PlaySoundEffect(SoundEffectSO soundEffect)
    {
        SoundEffect sound = (SoundEffect)PoolManager.Instance.ReuseComponent(soundEffect.soundPrefab, Vector3.zero, Quaternion.identity);
        sound.SetSound(soundEffect);
        sound.gameObject.SetActive(true);
        StartCoroutine(DisableSound(sound, soundEffect.soundEffectClip.length));
    }

    private IEnumerator DisableSound(SoundEffect sound, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        sound.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets overall volume of SoundsMaster mixer group
    /// </summary>
    /// <param name="soundsVolume"></param>
    private void SetSoundsVolume(int soundsVolume)
    {
        float muteDecibels = -80f;

        if(soundsVolume == 0)
        {
            GameResources.Instance.soundsMasterMixerGroup.audioMixer.SetFloat("soundsVolume", muteDecibels);
        }
        else
        {
            GameResources.Instance.soundsMasterMixerGroup.audioMixer.SetFloat("soundsVolume", HelperUtilities.LinearToDecibels(soundsVolume));
        }
    }
}
