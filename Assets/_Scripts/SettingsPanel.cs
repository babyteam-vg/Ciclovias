using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SettingsPanel : MonoBehaviour
{
    public static SettingsPanel Instance { get; private set; }

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    public GameObject panelSettingsUI;

    public event Action SettingsClosed;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        // From PlayerPrefs
        musicSlider.value = Mathf.Round(PlayerPrefs.GetFloat("MusicVolume", 1f) * 4);
        sfxSlider.value = Mathf.Round(PlayerPrefs.GetFloat("SFXVolume", 1f) * 4);

        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void OnSettingsOpen() { panelSettingsUI.SetActive(true); }
    public void OnSettingsClose()
    {
        panelSettingsUI.SetActive(false);
        SettingsClosed?.Invoke();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void OnMusicVolumeChanged(float value)
    {
        float volume = value / 4f;
        AudioManager.Instance.SetMusicVolume(volume);
    }

    private void OnSFXVolumeChanged(float value)
    {
        float volume = value / 4f;
        AudioManager.Instance.SetSFXVolume(volume);
    }
}
