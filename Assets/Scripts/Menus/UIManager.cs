using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject pauseUI;

    public void OnRestartPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnPauseMenuPress()
    {
        pauseUI.SetActive(true);
    }

    public void OnContinuePress()
    {
        pauseUI.SetActive(false);
    }

    public void OnMainMenuPress()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
