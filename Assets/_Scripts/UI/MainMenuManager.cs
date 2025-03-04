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
        GameData gameData = storageManager.LoadGame();

        if (gameData != null)
        {
            GameStateManager.Instance.SetLoadedGameData(gameData);
            SceneManager.LoadScene("GameMap");
        }
        else
        {
            Debug.LogWarning("No savefile found!");
        }
    }

    public void OnExitPress()
    {
        Application.Quit();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
}
