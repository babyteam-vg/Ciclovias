using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button continueButton;
    public GameObject playerNameUI;
    public TMP_InputField nameInputField;
    public Button confirmButton;

    private string mostRecentSave;

    private StorageManager storageManager = new StorageManager();

    // :::::::::: PUBLIC METHODS ::::::::::
    private void Start()
    {
        mostRecentSave = storageManager.GetMostRecentSaveFile();
        if (!(mostRecentSave != null))
            continueButton.interactable = false;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Continue
    public void OnContinuePress()
    {
        SettingsPanel.Instance.OnSettingsClose();

        if (mostRecentSave != null)
        {
            LoadingScene.Instance.LoadScene(2); // Loading Screen

            GameData gameData = storageManager.LoadGame(mostRecentSave);

            if (gameData != null)
                GameStateManager.Instance.SetLoadedGameData(gameData);
            else Debug.LogWarning("Failed to load the most recent savefile.");
        }
    }

    // ::::: New Game
    public void OnNewGamePress()
    {
        SettingsPanel.Instance.OnSettingsClose();
        playerNameUI.SetActive(true);
    }
    public void OnNameConfirmed()
    {
        string playerName = nameInputField.text.Trim();

        if (!string.IsNullOrEmpty(playerName))
        {
            PlayerNameManager.Instance.SetPlayerName(playerName);
            LoadingScene.Instance.LoadScene(1);
            GameStateManager.Instance.ResetLoadedGameData();
        }
    }

    // ::::: Settings
    public void OnSettingsPress()
    {
        SettingsPanel.Instance.OnSettingsOpen();
    }

    // ::::: Exit
    public void OnExitPress()
    {
        Application.Quit();
    }
}
