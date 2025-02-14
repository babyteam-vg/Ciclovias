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
        cameraController.LockCamera(Vector2Int.zero);
    }
    public void OnContinuePress()
    {
        pauseUI.SetActive(false);
        cameraController.UnlockCamera(Vector2Int.zero);
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

    // :::::::::: DIALOG MANAGER ::::::::::
    // ::::: Dialog
    public void OnOpenDialog()
    {
        dialogUI.SetActive(true);
        DialogManager.Instance.StartDialog();
        cameraController.LockCamera(Vector2Int.zero);
    }
    public void OnCloseDialog()
    {
        dialogUI.SetActive(false);
        cameraController.UnlockCamera(Vector2Int.zero);
    }
}
