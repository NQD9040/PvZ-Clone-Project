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
    [Header("BGM")]
    public AudioSource musicSource;
    public AudioClip bgMusic;

    private Dictionary<AudioClip, float> lastPlayTime = new Dictionary<AudioClip, float>();

    public float soundCooldown = 0.05f; // 50ms

    void Awake()
    {
        instance = this;
        musicSource.clip = bgMusic;
        musicSource.loop = true;
        musicSource.Play();
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