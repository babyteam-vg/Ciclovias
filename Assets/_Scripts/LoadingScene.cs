using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public static LoadingScene Instance { get; private set; }

    public GameObject loadingScreenUI;
    public Image backWheel;
    public Image frontWheel;

    private CanvasGroup canvasGroup;

    const float DURATION = 0.25f;

    public event Action<int> SceneLoaded;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        canvasGroup = loadingScreenUI.GetComponent<CanvasGroup>();
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    // ::::: Public Method to Fade Canvas Groups
    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float timeElapsed = 0f;
        canvasGroup.alpha = startAlpha;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timeElapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private IEnumerator LoadSceneAsync(int sceneId)
    {
        loadingScreenUI.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, DURATION));
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxs[4]);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);

        while (!operation.isDone)
        {
            backWheel.rectTransform.Rotate(0, 0, -400f * Time.deltaTime);
            frontWheel.rectTransform.Rotate(0, 0, -400f * Time.deltaTime);

            yield return null;
        }
        SceneLoaded?.Invoke(sceneId);

        AudioManager.Instance.StopSFX();
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, DURATION));
        loadingScreenUI.SetActive(false);
    }
}