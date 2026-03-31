using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public static Settings Instance;

    [Header("UI References")]
    public Slider musicSlider;
    public Slider sfxSlider;

    public float musicVolume = 0.5f;
    public float sfxVolume = 0.5f;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
            // Listen for changes and update volume immediately
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        SoundManager.instance.UpdateVolume();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        SoundManager.instance.UpdateVolume();
    }
}