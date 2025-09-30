using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Handles and coordinates all audio sources and clips from the game.
/// Manages the Music and SFX sliders.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource SFXsrc;
    [SerializeField] private AudioSource Musicsrc;
    [SerializeField] private AudioSource Playersrc;
    [SerializeField] private AudioClip collectSFX;
    [SerializeField] private AudioClip depositSFX;
    [SerializeField] private AudioClip depositFailSFX;
    [SerializeField] private AudioClip buttonSFX;
    [SerializeField] private AudioClip gameOverSFX;
    [SerializeField] private AudioClip vehicleStartSFX;
    [SerializeField] private AudioClip respawnSFX;
    [SerializeField] private AudioClip gasSpawnSFX;
    [SerializeField] private AudioClip rockSpawnSFX;
    [SerializeField] private AudioClip oilSpawnSFX;
    [SerializeField] private AudioClip oilTriggerSFX;
    [SerializeField] private AudioClip gasTriggerSFX;
    [SerializeField] private AudioClip rockTriggerSFX;
    [SerializeField] private AudioClip gameOverRecordSFX;
    [SerializeField] private AudioClip registerRecordSFX;
    [SerializeField] private AudioClip puddleSFX;
    [SerializeField] private AudioClip clockSFX;
    [SerializeField] private AudioClip hornSFX;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip lowTimeMusic;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume") && PlayerPrefs.HasKey("sfxVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
        }
    }
    public void PlayCollectSFX()
    {
        Playersrc.clip = collectSFX;
        Playersrc.volume = 0.80f;
        Playersrc.Play();
    }

    public void PlayDepositSFX()
    {
        Playersrc.clip = depositSFX;
        Playersrc.volume = 0.08f;
        Playersrc.Play();
    }

    public void PlayHornSFX()
    {
        Playersrc.clip = hornSFX;
        Playersrc.volume = 0.16f;
        Playersrc.Play();
    }

    public void PlayDepositFailSFX()
    {
        Playersrc.clip = depositFailSFX;
        Playersrc.volume = 1f;
        Playersrc.Play();
    }

    public void PlayButtonSFX()
    {
        SFXsrc.clip = buttonSFX;
        SFXsrc.volume = 1f;
        SFXsrc.Play();
    }

    public void PlayPuddleSFX()
    {
        SFXsrc.clip = puddleSFX;
        SFXsrc.volume = 0.5f;
        SFXsrc.Play();
    }

    public void PlayVehicleStartSFX()
    {
        SFXsrc.clip = vehicleStartSFX;
        SFXsrc.volume = 0.2f;
        SFXsrc.Play();
    }

    public void PlayGameOverSFX()
    {
        SFXsrc.clip = gameOverSFX;
        SFXsrc.volume = 1f;
        SFXsrc.Play();
    }

    public void PlayGasSpawnSFX()
    {
        SFXsrc.clip = gasSpawnSFX;
        SFXsrc.volume = 0.12f;
        SFXsrc.Play();
    }

    public void PlayGasTriggerSFX()
    {
        SFXsrc.clip = gasTriggerSFX;
        SFXsrc.volume = 0.19f;
        SFXsrc.Play();
    }

    public void PlayOilSpawnSFX()
    {
        SFXsrc.clip = oilSpawnSFX;
        SFXsrc.volume = 0.08f;
        SFXsrc.Play();
    }

    public void PlayOilTriggerSFX()
    {
        SFXsrc.clip = oilTriggerSFX;
        SFXsrc.volume = 0.18f;
        SFXsrc.Play();
    }

    public void PlayRockSpawnSFX()
    {
        SFXsrc.clip = rockSpawnSFX;
        SFXsrc.volume = 0.20f;
        SFXsrc.Play();
    }

    public void PlayRockTriggerSFX()
    {
        SFXsrc.clip = rockTriggerSFX;
        SFXsrc.volume = 0.13f;
        SFXsrc.Play();
    }

    public void PlayRespawnSFX()
    {
        SFXsrc.clip = respawnSFX;
        SFXsrc.volume = 0.11f;
        SFXsrc.Play();
    }

    public void PlayGameOverRecordSFX()
    {
        SFXsrc.clip = gameOverRecordSFX;
        SFXsrc.volume = 0.6f;
        SFXsrc.Play();
    }

    public void PlayRegisterRecordSFX()
    {
        SFXsrc.clip = registerRecordSFX;
        SFXsrc.volume = 0.17f;
        SFXsrc.Play();
    }

    public void PlayClockSFX()
    {
        SFXsrc.clip = clockSFX;
        SFXsrc.volume = 0.7f;
        SFXsrc.Play();
    }

    public void SwitchLowTimeMusic()
    {
        Musicsrc.Stop();
        Musicsrc.clip = lowTimeMusic;
        Musicsrc.volume = 0.15f;
        Musicsrc.Play();
    }

    public void SwitchGameOverMusic()
    {
        Musicsrc.Stop();
        Musicsrc.clip = gameOverMusic;
        Musicsrc.pitch = 0.5f;
        Musicsrc.volume = 0.1f;
        Musicsrc.Play();
    }

    public void SwitchMainMenuMusic()
    {
        Musicsrc.Stop();
        Musicsrc.clip = gameOverMusic;
        Musicsrc.pitch = 1f;
        Musicsrc.volume = 0.082f;
        Musicsrc.Play();
    }

    public void SwitchRegularMusic()
    {

        Musicsrc.clip = gameMusic;
        Musicsrc.pitch = 1.0f;
        Musicsrc.volume = 0.2f;


    }

    public void PlayBackgroundMusic()
    {
        Musicsrc.Play();
    }

    public void StopBackgroundMusic()
    {
        Musicsrc.Stop();
    }

    public void PauseBackgroundMusic()
    {
        Musicsrc.Pause();
    }

    public void UnpauseBackgroundMusic()
    {
        Musicsrc.UnPause();
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        float dB = Mathf.Log10(volume) * 20;
        // Scaled to go from -80db to +20db
        float scaledDB = Mathf.Lerp(-80f, 20f, (dB + 80f) / 80f);
        audioMixer.SetFloat("music", scaledDB);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        float dB = Mathf.Log10(volume) * 20;
        // Scaled to go from -80db to +20db
        float scaledDB = Mathf.Lerp(-80f, 20f, (dB + 80f) / 80f);
        audioMixer.SetFloat("sfx", scaledDB);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");

        SetMusicVolume();
        SetSFXVolume();
    }
}
