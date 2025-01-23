using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameUIController : MonoBehaviour
{
    private VisualElement root;

    private Button homeButton;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        homeButton = root.Q<Button>("Home");

        homeButton.clicked += OnHomeClicked;
    }

    private void OnHomeClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
