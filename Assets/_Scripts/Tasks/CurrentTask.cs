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

    [Header("UI References - Texts")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI fromText;
    [SerializeField] private TextMeshProUGUI toText;
    [SerializeField] private TextMeshProUGUI flavorText;

    public event Action<Task> TaskPinned;
    public event Action TaskUnpinned;

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
        taskManager.TaskAccepted += PinTask;
        taskManager.TaskActivated += PinTask;
        taskManager.TaskSealed += UnpinTask;
    }
    private void OnDisable()
    {
        taskManager.TaskAccepted -= PinTask;
        taskManager.TaskActivated -= PinTask;
        taskManager.TaskSealed -= UnpinTask;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
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

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: UI Only Affected When Changing the Pinned Task
    private void UpdateTaskUI()
    {
        // Texts
        if (ThereIsPinned())
        {
            titleText.text = PinnedTask.info.title;
            fromText.text = PinnedTask.from.info.compoundName;
            toText.text = PinnedTask.to.info.compoundName;
            flavorText.text = PinnedTask.info.flavourDetails.flavorMessage;
        }
        else
        {
            titleText.text = "Task";
            fromText.text = "From";
            toText.text = "To";
            flavorText.text = "Flavor";
        }
    }
}