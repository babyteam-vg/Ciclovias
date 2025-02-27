using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public event Action MenuOpened;
    public event Action MenuClosed;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        taskManager.TaskCompleted += EndBuild;
    }
    private void OnDisable()
    {
        taskManager.TaskCompleted -= EndBuild;
    }

    // :::::::::: PAUSE MANAGER ::::::::::
    // ::::: Pause Menu
    public void OnPauseMenuPress()
    {
        pauseUI.SetActive(true);
        MenuOpened?.Invoke();
    }
    public void OnContinuePress()
    {
        pauseUI.SetActive(false);
        MenuClosed?.Invoke();
    }
    public void OnMainMenuPress()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // :::::::::: TASK MANAGER ::::::::::
    // ::::: Tasks Diary
    public void OnTasksDiaryPress()
    {
        TaskDiary.Instance.ShowAvailableTasks();
        tasksDiaryUI.SetActive(true);
        MenuOpened?.Invoke(); // !
    }
    public void OnTasksDiaryClose()
    {
        tasksDiaryUI.SetActive(false);
        MenuClosed?.Invoke(); // !
    }

    // ::::: Receive Task
    public void OnReceiveTaskPress()
    {
        receiveTaskUI.SetActive(true);
        MenuOpened?.Invoke(); // !
    }
    public void OnAcceptTaskPress()
    {
        taskManager.AcceptTask(TaskReceiver.Instance.ReceivedTask);
        receiveTaskUI.SetActive(false);
        MenuClosed?.Invoke(); // !
    }

    // :::::::::: DIALOG MANAGER ::::::::::
    // ::::: Dialog
    public void OnOpenDialog(Task task)
    {
        dialogUI.SetActive(true);
        dialogManager.StartDialog(task);
        MenuOpened?.Invoke(); // !
    }
    public void OnCloseDialog()
    {
        dialogUI.SetActive(false);
        MenuClosed?.Invoke(); // !
    }

    // ::::: To End the Builds
    public void EndBuild(Task task)
    {
        if (TaskDiary.Instance.tasks.Last().state == TaskState.Completed)
            endBuildUI.SetActive(true);
    }
}
