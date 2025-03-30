using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private LoadingScene loadingScene;

    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clips")]
    public List<AudioClip> songs = new List<AudioClip>();
    public List<AudioClip> sfxs = new List<AudioClip>();

    private int currentSongIndex = 0;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        // Load From PlayerPrefs
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SFXSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    private void OnEnable() { loadingScene.SceneLoaded += NewSceneMusic; }
    private void OnDisable() { loadingScene.SceneLoaded -= NewSceneMusic; }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void PlaySFX(AudioClip sfx)
    {
        SFXSource.PlayOneShot(sfx);
    }
    public bool IsSFXPlaying()
    {
        return SFXSource.isPlaying;
    }
    public void SetSFXPitch(float pitch)
    {
        SFXSource.pitch = Mathf.Clamp(pitch, -3f, 3f);
    }
    public void ResetSFXPitch()
    {
        SFXSource.pitch = 1f;
    }
    public void StopSFX()
    {
        SFXSource.Stop();
    }

    // ::::: Media Player
    public void PlayNextSong()
    {
        currentSongIndex = (currentSongIndex + 1) % songs.Count;
        PlaySong(currentSongIndex);
    }
    public void PlayPreviousSong()
    {
        currentSongIndex = (currentSongIndex - 1 + songs.Count) % songs.Count;
        PlaySong(currentSongIndex);
    }
    public void PlayRandomSong()
    {
        int newIndex = Random.Range(0, songs.Count);
        while (newIndex == currentSongIndex)
        {
            newIndex = Random.Range(0, songs.Count);
        }
        currentSongIndex = newIndex;
        PlaySong(currentSongIndex);
    }
    public void StopMusic()
    {
        musicSource.Stop();
    }
    public void PauseMusic()
    {
        musicSource.Pause();
    }
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    // ::::: Settings
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }
    public void SetSFXVolume(float volume)
    {
        SFXSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void NewSceneMusic(int sceneId)
    {
        if (sceneId == 0) // Main Menu
            StopMusic();
        else if (sceneId == 1) // GameMap
            PlaySong(0); // Train to the Sky City
    }

    private void PlaySong(int index)
    {
        if (songs.Count == 0) return;

        musicSource.clip = songs[index];
        musicSource.Play();
    }
}
