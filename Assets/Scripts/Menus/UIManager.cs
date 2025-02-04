using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] CameraController cameraController;
    [SerializeField] TaskDiary taskDiary;

    public GameObject pauseUI;
    public GameObject tasksDiaryUI;

    // === Methods ===
    public void OnRestartPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Pause Menu
    public void OnPauseMenuPress()
    {
        cameraController.lockCamera = true;
        pauseUI.SetActive(true);
    }

    public void OnContinuePress()
    {
        cameraController.lockCamera = false;
        pauseUI.SetActive(false);
    }

    public void OnMainMenuPress()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Tasks Diary
    public void OnTasksDiaryPress()
    {
        taskDiary.ShowAvailableTasks();
        tasksDiaryUI.SetActive(true);
        cameraController.lockCamera = true;
    }

    public void OnTasksDiaryClose()
    {
        tasksDiaryUI.SetActive(false);
        cameraController.lockCamera = false;
    }
}
