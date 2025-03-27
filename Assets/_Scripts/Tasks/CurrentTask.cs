using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTask : MonoBehaviour
{
    public static CurrentTask Instance { get; private set; }
    public Task PinnedTask { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("UI References - Texts")]
    public TextMeshProUGUI titleText;
    public GameObject flavorUI;

    private TextMeshProUGUI flavorText;

    public event Action<Task> TaskPinned;
    public event Action TaskUnpinned;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        flavorText = flavorUI.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        taskManager.TaskAccepted += PinTask;
        taskManager.TaskActivated += PinTask;
        taskManager.TaskSealed += UnpinTask;

        tutorialManager.TutorialStarted += PinTutorial;
        tutorialManager.TutorialCompleted += ClearUI;
    }
    private void OnDisable()
    {
        taskManager.TaskAccepted -= PinTask;
        taskManager.TaskActivated -= PinTask;
        taskManager.TaskSealed -= UnpinTask;

        tutorialManager.TutorialStarted -= PinTutorial;
        tutorialManager.TutorialCompleted -= ClearUI;
    }

    private void Start()
    {
        ClearUI();
    }

    // :::::::::: TASK METHODS ::::::::::
    // ::::: Is There a Task Pinned?
    public bool ThereIsPinned() { return PinnedTask != null; }

    // ::::: Pin & Unpin a Task
    public void PinTask(Task task, bool isManualPin)
    {
        if (PinnedTask == task) return;
        if (!isManualPin && ThereIsPinned()) return;

        PinnedTask = task;
        TaskPinned?.Invoke(task);
        UpdateTaskUI();
    }
    public void UnpinTask(Task task)
    {
        if (!ThereIsPinned()) return;

        PinnedTask = null;
        TaskUnpinned?.Invoke();
        UpdateTaskUI();
    }

    // :::::::::: TUTORIAL METHODS ::::::::::
    private void PinTutorial(Tutorial tutorial)
    {
        ClearUI();
        titleText.text = tutorial.info.title;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: UI Only Affected When Changing the Pinned Task
    private void UpdateTaskUI()
    {
        // Texts
        if (ThereIsPinned())
        {
            if (PinnedTask.info.flavorDetails.flavorType != FlavorType.None)
            {
                flavorUI.SetActive(true);
                flavorText.text = PinnedTask.GenerateFlavorMessage();
            }

            titleText.text = PinnedTask.info.title;
        }
        else ClearUI();
    }

    private void ClearUI()
    {
        titleText.text = "";

        flavorText.text = "";
        flavorUI.SetActive(false);
    }
}