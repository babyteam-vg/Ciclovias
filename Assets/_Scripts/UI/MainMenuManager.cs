using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private LoadingScene loadingScene;

    private StorageManager storageManager = new StorageManager();

    // :::::::::: MONO METHODS ::::::::::

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Pause Menu
    public void OnContinuePress()
    {
        loadingScene.LoadScene(1); // Loading Screen

        string mostRecentSave = storageManager.GetMostRecentSaveFile();
        if (mostRecentSave != null)
        {
            GameData gameData = storageManager.LoadGame(mostRecentSave);

            if (gameData != null)
            {
                GameStateManager.Instance.SetLoadedGameData(gameData);
                SceneManager.LoadScene("GameMap");
            }
            else Debug.LogWarning("Failed to load the most recent savefile.");
        }
        else Debug.LogWarning("No savefile found!");
    }

    public void OnNewGamePress()
    {
        loadingScene.LoadScene(1); // Loading Screen

        GameStateManager.Instance.ResetLoadedGameData();
        SceneManager.LoadScene("GameMap");
    }

    public void OnExitPress()
    {
        Application.Quit();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
}
