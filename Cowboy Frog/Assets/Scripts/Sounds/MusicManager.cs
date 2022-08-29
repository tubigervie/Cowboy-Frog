using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class MusicManager : SingletonMonobehaviour<MusicManager>
{
    private AudioSource musicAudioSource;
    private MusicTrackSO currentMusicTrack;
    private MusicTrackSO previousMusicTrack;
    private Coroutine fadeOutMusicCoroutine;
    private Coroutine fadeInMusicCoroutine;

    public int musicVolume = 10;

    protected override void Awake()
    {
        base.Awake();
        musicAudioSource = GetComponent<AudioSource>();

        GameResources.Instance.musicOffSnapshot.TransitionTo(0f);
    }

    private void Start()
    {
        if(PlayerPrefs.HasKey("musicVolume"))
        {
            musicVolume = PlayerPrefs.GetInt("musicVolume");
        }
        SetMusicVolume(musicVolume);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt("musicVolume", musicVolume);
    }

    private void SetMusicVolume(int musicVolume)
    {
        float muteDecibels = -80f;
        if(musicVolume == 0)
        {
            GameResources.Instance.musicMasterMixerGroup.audioMixer.SetFloat("musicVolume", muteDecibels);
        }
        else
        {
            GameResources.Instance.musicMasterMixerGroup.audioMixer.SetFloat("musicVolume", HelperUtilities.LinearToDecibels(musicVolume));
        }
    }

    public void IncreaseMusicVolume()
    {
        int maxMusicVolume = 20;
        if (musicVolume >= maxMusicVolume) return;
        musicVolume += 1;
        SetMusicVolume(musicVolume);
    }

    public void DecreaseMusicVolume()
    {
        if (musicVolume == 0) return;
        musicVolume -= 1;
        SetMusicVolume(musicVolume);
    }

    public void PlayMusic(MusicTrackSO musicTrack, float fadeOutTime = Settings.musicFadeOutTime, float fadeInTime = Settings.musicFadeInTime)
    {
        if (musicTrack == null) return;
        if (currentMusicTrack == null ||  musicTrack.musicClip != currentMusicTrack.musicClip)
            StartCoroutine(PlayMusicRoutine(musicTrack, fadeOutTime, fadeInTime));
    }

    public void PlayPreviousTrack()
    {
        if (previousMusicTrack == null) return;
        StartCoroutine(PlayMusicRoutine(previousMusicTrack, Settings.musicFadeOutTime, Settings.musicFadeInTime));
    }

    private IEnumerator PlayMusicRoutine(MusicTrackSO musicTrack, float fadeOutTime, float fadeInTime)
    {
        if (fadeOutMusicCoroutine != null)
            StopCoroutine(fadeOutMusicCoroutine);
        if (fadeInMusicCoroutine != null)
            StopCoroutine(fadeInMusicCoroutine);
        previousMusicTrack = currentMusicTrack;
        currentMusicTrack = musicTrack;

        yield return fadeOutMusicCoroutine = StartCoroutine(FadeOutMusicRoutine(fadeOutTime));
        yield return fadeInMusicCoroutine = StartCoroutine(FadeInMusicCoroutine(musicTrack, fadeInTime));
    }

    private IEnumerator FadeInMusicCoroutine(MusicTrackSO musicTrack, float fadeInTime)
    {
        musicAudioSource.clip = musicTrack.musicClip;
        musicAudioSource.volume = musicTrack.musicVolume;
        musicAudioSource.Play();
        GameResources.Instance.musicOnFullSnapshot.TransitionTo(fadeInTime);
        yield return new WaitForSeconds(fadeInTime);
    }

    private IEnumerator FadeOutMusicRoutine(float fadeOutTime)
    {
        GameResources.Instance.musicLowSnapshot.TransitionTo(fadeOutTime);
        yield return new WaitForSeconds(fadeOutTime);
    }
}
