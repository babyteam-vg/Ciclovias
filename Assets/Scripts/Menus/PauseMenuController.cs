using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    public VisualElement ui;

    private Button pauseButton;
    private bool isPaused = false;

    private Button resumeButton;
    private Button settingsButton;
    private Button exitButton;

    // === Methods ===
    public void Awake() { ui = GetComponent<UIDocument>().rootVisualElement; }

    public void OnEnable()
    {
        resumeButton = ui.Q<Button>("Resume");
        resumeButton.clicked += OnPlayButtonClicked;

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
