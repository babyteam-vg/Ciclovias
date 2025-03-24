using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BackgroundVideosManager : MonoBehaviour
{
    public VideoPlayer player;
    public VideoClip loopClip;
    public CanvasGroup uiGroup;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable() { player.loopPointReached += StartLoopVideo; }
    private void OnDisable() { player.loopPointReached -= StartLoopVideo; }
    private void Start() { Invoke(nameof(StartFadeIn), 1.5f); }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void StartLoopVideo(VideoPlayer vp)
    {
        player.clip = loopClip;
        player.loopPointReached -= StartLoopVideo;
    }

    // ::::: 
    private void StartFadeIn() { StartCoroutine(FadeUI()); }

    IEnumerator FadeUI()
    {
        if (uiGroup == null) yield break;

        float duration = 1f;
        float startAlpha = 0f;
        float targetAlpha = 1f;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            uiGroup.alpha = alpha;
            yield return null;
        }

        uiGroup.alpha = targetAlpha;

        uiGroup.interactable = uiGroup.blocksRaycasts = true;
    }
}
