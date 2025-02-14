using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] CameraController cameraController;

    [Header("UI Objects")]
    public GameObject pauseUI;
    public GameObject tasksDiaryUI;
    public GameObject receiveTaskUI;
    public GameObject dialogUI;
    public GameObject endBuildUI;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // :::::::::: PAUSE MANAGER ::::::::::
    // ::::: Pause Menu
    public void OnPauseMenuPress()
    {
        pauseUI.SetActive(true);
        Time.timeScale = 0;
    }
    public void OnContinuePress()
    {
        pauseUI.SetActive(false);
        Time.timeScale = 1;
    }
    public void OnMainMenuPress()
    {
        //SceneManager.LoadScene("MainMenu");
    }

    // :::::::::: TASK MANAGER ::::::::::
    // ::::: Tasks Diary
    public void OnTasksDiaryPress()
    {
        TaskDiary.Instance.ShowAvailableTasks();
        tasksDiaryUI.SetActive(true);
        Time.timeScale = 0;
    }
    public void OnTasksDiaryClose()
    {
        tasksDiaryUI.SetActive(false);
        Time.timeScale = 1;
    }

    // ::::: Receive Task
    public void OnReceiveTaskPress()
    {
        receiveTaskUI.SetActive(true);
        Time.timeScale = 0;
    }
    public void OnAcceptTaskPress()
    {
        Time.timeScale = 1;
        TaskDiary.Instance.AcceptTask(TaskReceiver.Instance.ReceivedTask);
        receiveTaskUI.SetActive(false);
    }

    // :::::::::: DIALOG MANAGER ::::::::::
    // ::::: Dialog
    public void OnOpenDialog()
    {
        dialogUI.SetActive(true);
        cameraController.LockCamera(Vector2Int.zero);
        DialogManager.Instance.StartDialog();
    }
    public void OnCloseDialog()
    {
        cameraController.UnlockCamera(Vector2Int.zero);
        dialogUI.SetActive(false);
    }

    public void OnOpenEndBuild() // 4 THE BUILD 1!!!
    {
        endBuildUI.SetActive(true);
        Time.timeScale = 0;
    }
}
