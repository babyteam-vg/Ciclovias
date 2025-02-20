using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuManager : MonoBehaviour
{
    public static InGameMenuManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private DialogManager dialogManager;

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
    }
    public void OnContinuePress()
    {
        pauseUI.SetActive(false);
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
    }
    public void OnTasksDiaryClose()
    {
        tasksDiaryUI.SetActive(false);
    }

    // ::::: Receive Task
    public void OnReceiveTaskPress()
    {
        receiveTaskUI.SetActive(true);
    }
    public void OnAcceptTaskPress()
    {
        taskManager.AcceptTask(TaskReceiver.Instance.ReceivedTask);
        receiveTaskUI.SetActive(false);
    }

    // :::::::::: DIALOG MANAGER ::::::::::
    // ::::: Dialog
    public void OnOpenDialog(Task task)
    {
        dialogUI.SetActive(true);
        dialogManager.StartDialog(task);
    }
    public void OnCloseDialog()
    {
        dialogUI.SetActive(false);
    }

    public void OnOpenEndBuild() // 4 THE BUILD 1!!!
    {
        endBuildUI.SetActive(true);
        Time.timeScale = 0;
    }
}
