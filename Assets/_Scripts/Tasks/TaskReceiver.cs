using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskReceiver : MonoBehaviour
{
    public static TaskReceiver Instance { get; private set; }
    public Task ReceivedTask { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TaskDialogManager taskDialogManager;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI context;
    [SerializeField] private TextMeshProUGUI safety;
    [SerializeField] private TextMeshProUGUI charm;
    [SerializeField] private TextMeshProUGUI flow;
    [SerializeField] private TextMeshProUGUI minMat;
    [SerializeField] private TextMeshProUGUI maxMat;

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
        taskManager.TaskAccepted += TaskAlreadyReceived;
    }
    private void OnDisable()
    {
        taskManager.TaskAccepted -= TaskAlreadyReceived;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Is There a Received Task?
    public bool ThereIsReceived() { return ReceivedTask != null; }

    // ::::: Receive a Task
    public void ReceiveTask(Task task)
    {
        if (ReceivedTask == task) return;

        ReceivedTask = task;
        taskDialogManager.StartTaskDialogs(task);
        UpdateTaskUI();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: UI Only Affected When Changing the Pinned Task
    private void UpdateTaskUI()
    {
        if (ThereIsReceived())
        {
            Task task = ReceivedTask;

            title.text = task.info.title;
            context.text = task.info.context;
            safety.text = task.info.safetyRequirement ? task.info.minSafety.ToString() : "-";
            charm.text = task.info.charmRequirement ? task.info.minCharm.ToString() : "-";
            flow.text = task.info.flowRequirement ? task.info.minFlow.ToString() : "-";
            minMat.text = task.info.minMaterialRequirement ? task.info.minMaterial.ToString() : "-";
            maxMat.text = task.info.maxMaterialRequirement ? task.info.maxMaterial.ToString() : "-";
        }
        else
        {
            title.text = "Task Title";
            context.text = "Task Context";
            safety.text = "-";
            charm.text =  "-";
            flow.text = "-";
            minMat.text = "-";
            maxMat.text = "-";
        }
    }

    // ::::: When a Task is Accepted
    private void TaskAlreadyReceived(Task task, bool _)
    {
        ReceivedTask = null;
        UpdateTaskUI();
    }
}
