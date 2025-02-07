using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] CameraController cameraController;

    [Header("UI Objects")]
    public GameObject pauseUI;
    public GameObject tasksDiaryUI;
    public GameObject receiveTaskUI;

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Pause Menu
    public void OnPauseMenuPress()
    {
        pauseUI.SetActive(true);
        cameraController.LockCamera(Vector2Int.zero);
    }

    public void OnContinuePress()
    {
        pauseUI.SetActive(false);
        cameraController.UnlockCamera(Vector2Int.zero);
    }

    public void OnMainMenuPress()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // ::::: Tasks Diary
    public void OnTasksDiaryPress()
    {
        TaskDiary.Instance.ShowAvailableTasks();
        tasksDiaryUI.SetActive(true);
        cameraController.LockCamera(Vector2Int.zero);
    }

    public void OnTasksDiaryClose()
    {
        tasksDiaryUI.SetActive(false);
        cameraController.UnlockCamera(Vector2Int.zero);
    }

    // ::::: Receive Task
    public void OnReceiveTaskPress()
    {
        receiveTaskUI.SetActive(true);
        cameraController.LockCamera(Vector2Int.zero);
    }

    public void OnAcceptTaskPress()
    {
        TaskDiary.Instance.AcceptTask(TaskReceiver.Instance.ReceivedTask);
        receiveTaskUI.SetActive(false);
        cameraController.UnlockCamera(Vector2Int.zero);
    }
}
