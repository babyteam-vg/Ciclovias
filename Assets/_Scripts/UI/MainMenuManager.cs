using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // :::::::::: MONO METHODS ::::::::::

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Pause Menu
    public void OnContinuePress()
    {
        SceneManager.LoadScene("GameMap");
    }

    public void OnExitPress()
    {
        Application.Quit();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
}
