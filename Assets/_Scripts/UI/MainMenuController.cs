using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private VisualElement root;

    private Button continueButton;
    private Button settingsButton;
    private Button exitButton;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        continueButton = root.Q<Button>("Continue");
        settingsButton = root.Q<Button>("Settings");
        exitButton = root.Q<Button>("Exit");

        continueButton.clicked += OnContinueClicked;
        settingsButton.clicked += OnSettingsClicked;
        exitButton.clicked += OnExitClicked;
    }

    private void OnContinueClicked()
    {
        SceneManager.LoadScene("TestRoads");
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings!");
    }

    private void OnExitClicked()
    {
        Application.Quit();
    }
}
