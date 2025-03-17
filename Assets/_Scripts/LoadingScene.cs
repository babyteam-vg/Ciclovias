using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    public static LoadingScene Instance { get; private set; }

    [SerializeField] private GameObject loadingScreenUI;

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
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    IEnumerator LoadSceneAsync(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);

        loadingScreenUI.SetActive(true);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            yield return null;
        }

        loadingScreenUI.SetActive(false);
        SceneLoaded?.Invoke(sceneId);
    }
}