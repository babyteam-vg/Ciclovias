using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanelUI;

    private StorageManager storageManager = new StorageManager();

    // :::::::::: MONO METHODS ::::::::::

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Pause Menu
    public void OnContinuePress()
    {
        SettingsPanel.Instance.OnSettingsClose();

        LoadingScene.Instance.LoadScene(2); // Loading Screen

        string mostRecentSave = storageManager.GetMostRecentSaveFile();
        if (mostRecentSave != null)
        {
            GameData gameData = storageManager.LoadGame(mostRecentSave);

            if (gameData != null)
                GameStateManager.Instance.SetLoadedGameData(gameData);
            else Debug.LogWarning("Failed to load the most recent savefile.");
        }
        else Debug.LogWarning("No savefile found!");
    }

    public void OnNewGamePress()
    {
        SettingsPanel.Instance.OnSettingsClose();

        LoadingScene.Instance.LoadScene(2); // Loading Screen

        GameStateManager.Instance.ResetLoadedGameData();
    }

    public void OnSettingsPress()
    {
        SettingsPanel.Instance.OnSettingsOpen();
    }

    public void OnExitPress()
    {
        Application.Quit();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
}
