using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip plantPick;
    public AudioClip plantPlace;
    public AudioClip shoot;
    public AudioClip normalHit;
    public AudioClip coneHeadHit;
    public AudioClip ironHit;
    public AudioClip zombieDeath;
    public AudioClip sunPickup;
    public AudioClip canNotPick;
    public AudioClip zombieEat;
    public AudioClip zombieGulp;
    public AudioClip potatoMineActivate;
    public AudioClip cherryBombActivate;
    public AudioClip snowEffect;
    public AudioClip slowDownEffect;
    public AudioClip pickupShovel;
    public AudioClip removePlant;
    public AudioClip chomperEat;
    public AudioClip gameOverSound;
    public AudioClip firstWaveSound;
    public AudioClip waveSound;
    public AudioClip pauseSound;
    [Header("BGM")]
    public AudioSource musicSource;
    public AudioClip bgMusic;
    public AudioClip menuMusic;
    private Dictionary<AudioClip, float> lastPlayTime = new Dictionary<AudioClip, float>();

    public float soundCooldown = 0.05f; // 50ms

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        UpdateVolume();
    }
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }
    public void UpdateVolume()
    {
        if (Settings.Instance == null) return;

        // Update volume for Music (BGM)
        if (musicSource != null)
        {
            musicSource.volume = Settings.Instance.musicVolume;
        }

        // Update volume for SFX (Sound Effects)
        if (audioSource != null)
        {
            audioSource.volume = Settings.Instance.sfxVolume;
        }
    }
    public void SetPause(bool pause)
    {
        if (pause)
        {
            if (musicSource.isPlaying)
                musicSource.Pause();
        }
        else
        {
            if (!musicSource.isPlaying && musicSource.clip != null)
                musicSource.UnPause();
        }
    }
    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (lastPlayTime.ContainsKey(clip))
        {
            if (Time.time - lastPlayTime[clip] < soundCooldown)
                return;
        }

        lastPlayTime[clip] = Time.time;

        audioSource.PlayOneShot(clip);
    }
}