using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private StorageManager storageManager = new StorageManager();

    // :::::::::: MONO METHODS ::::::::::

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Pause Menu
    public void OnContinuePress()
    {
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
        GameStateManager.Instance.ResetLoadedGameData();
        SceneManager.LoadScene("GameMap");
    }

    public void OnExitPress()
    {
        Application.Quit();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
}
