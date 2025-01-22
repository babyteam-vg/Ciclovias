using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public VisualElement ui;

    public Button playButton;
    public Button settingsButton;
    public Button exitButton;

    // === Methods ===
    public void Awake() { ui = GetComponent<UIDocument>().rootVisualElement; }

    public void OnEnable()
    {
        playButton = ui.Q<Button>("Play");
        playButton.clicked += OnPlayButtonClicked;

        settingsButton = ui.Q<Button>("Settings");
        settingsButton.clicked += OnSettingsButtonClicked;

        exitButton = ui.Q<Button>("Exit");
        exitButton.clicked += OnExitButtonClicked;
    }

    private void OnPlayButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("Settings!");
    }

    private void OnExitButtonClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPaused = false;
#endif
    }
}
