using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clips")]
    [SerializeField] List<AudioClip> songs = new List<AudioClip>();
    public AudioClip build;
    public AudioClip complete;

    private int currentSongIndex = 0;

    // :::::::::: MONO METHODS ::::::::::
    private void Start()
    {
        if (songs.Count > 0)
        {
            PlaySong(currentSongIndex);
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void PlaySFX(AudioClip sfx)
    {
        SFXSource.PlayOneShot(sfx);
    }

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
        while (newIndex == currentSongIndex) // Prevent Repetition
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

    // :::::::::: PRIVATE METHODS ::::::::::
    private void PlaySong(int index)
    {
        if (songs.Count == 0) return;

        musicSource.clip = songs[index];
        musicSource.Play();
    }
}
