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
    private SettingsPanel settingsPanel = SettingsPanel.Instance;

    [Header("UI References")]
    public GameObject pauseUI;
    public GameObject tasksDiaryUI;
    public GameObject receiveTaskUI;
    private GameObject settingsPanelUI;

    private Stack<GameObject> menuStack = new Stack<GameObject>(3);

    public event Action MenuOpened;
    public event Action MenuClosed;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        settingsPanelUI = SettingsPanel.Instance.panelSettingsUI;
    }

    private void OnEnable()
    {
        taskManager.TaskSealed += EndBuild;
        settingsPanel.SettingsClosed += CloseMenu;
    }
    private void OnDisable()
    {
        taskManager.TaskSealed -= EndBuild;
        settingsPanel.SettingsClosed -= CloseMenu;
    }

    // :::::::::: MENU QUEUE MANAGEMENT ::::::::::
    public void OpenMenu(GameObject menuUI)
    {
        if (menuStack.Count >= 3)
        {
            GameObject oldestMenu = menuStack.Pop();
            oldestMenu.SetActive(false);
        }

        menuStack.Push(menuUI);
        menuUI.SetActive(true);
        MenuOpened?.Invoke();
    }

    public void CloseMenu()
    {
        if (menuStack.Count > 0)
        {
            GameObject latestMenu = menuStack.Pop();
            latestMenu.SetActive(false);
        }
        
        if (menuStack.Count == 0) MenuClosed?.Invoke();
    }

    // :::::::::: PAUSE MANAGER ::::::::::
    // ::::: Pause Menu
    public void OnPauseMenuPress() { OpenMenu(pauseUI); }
    public void OnContinuePress() { CloseMenu(); }
    public void OnSettingsPress() { OpenMenu(settingsPanelUI); }
    public void OnMainMenuPress()
    {
        LoadingScene.Instance.LoadScene(0); // Loading Screen
        SceneManager.LoadScene("MainMenu");
    }

    // :::::::::: TASKS MANAGER ::::::::::
    // ::::: Tasks Diary
    public void OnTasksDiaryPress()
    {
        TaskDiary.Instance.ShowAvailableTasks();
        OpenMenu(tasksDiaryUI);
    }
    public void OnTasksDiaryClose() { CloseMenu(); }

    // ::::: Receive Task
    public void OnReceiveTaskPress() { OpenMenu(receiveTaskUI); }
    public void OnAcceptTaskPress()
    {
        taskManager.AcceptTask(TaskReceiver.Instance.ReceivedTask);
        CloseMenu();
    }

    // ::::: Confirm Task
    public void OnConfirmTaskPress() { taskManager.ConfirmTask(CurrentTask.Instance.PinnedTask); }

    // :::::::::: BUILD MANAGER ::::::::::
    // ::::: To End the Builds
    public void EndBuild(Task task)
    {
        if (TaskDiary.Instance.tasks.Last().state == TaskState.Completed)
            pauseUI.SetActive(true);
    }
}
