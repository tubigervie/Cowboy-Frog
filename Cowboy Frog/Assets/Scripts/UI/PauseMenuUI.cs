using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI musicVolumeText;
    [SerializeField] TextMeshProUGUI soundVolumeText;

    // Start is called before the first frame update
    private void Start()
    {
        soundVolumeText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
        musicVolumeText.SetText(MusicManager.Instance.musicVolume.ToString());
        gameObject.SetActive(false);
    }

    private IEnumerator InitializeUI()
    {
        yield return null;
        soundVolumeText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
        musicVolumeText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
        StartCoroutine(InitializeUI());
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    public void IncreaseMusicVolume()
    {
        MusicManager.Instance.IncreaseMusicVolume();
        musicVolumeText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    public void DecreaseMusicVolume()
    {
        MusicManager.Instance.DecreaseMusicVolume();
        musicVolumeText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    public void IncreaseSoundVolume()
    {
        SoundEffectManager.Instance.IncreaseSoundsVolume();
        soundVolumeText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
    }

    public void DecreaseSoundVolume()
    {
        SoundEffectManager.Instance.DecreaseSoundsVolume();
        soundVolumeText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
