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
    [SerializeField] private LaneScores laneScores;

    [Header("UI References")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI context;
    public Image variant;
    public TextMeshProUGUI safety;
    public TextMeshProUGUI charm;
    public TextMeshProUGUI flow;
    public TextMeshProUGUI minMat;
    public TextMeshProUGUI maxMat;
    public Image portrait;

    [Header("UI Variants")]
    public Sprite safetyVariant;
    public Sprite charmVariant;
    public Sprite flowVariant;

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

            if (task.info.flowRequirement) variant.sprite = flowVariant;
            else if (task.info.charmRequirement) variant.sprite = charmVariant;
            else if (task.info.safetyRequirement) variant.sprite = safetyVariant;

            safety.text = task.info.safetyRequirement
                ? laneScores.ConvertToUI(ScoreType.RequirementPercentage, task.info.minSafety)
                : "-";

            charm.text = task.info.charmRequirement
                ? laneScores.ConvertToUI(ScoreType.RequirementPercentage, task.info.minCharm)
                : "-";

            flow.text = task.info.flowRequirement
                ? laneScores.ConvertToUI(ScoreType.RequirementPercentage, task.info.minFlow)
                : "-";

            minMat.text = task.info.minMaterialRequirement
                ? laneScores.ConvertToUI(ScoreType.MinimumMaterial, task.info.minMaterial)
                : "-";

            maxMat.text = task.info.maxMaterialRequirement
                ? laneScores.ConvertToUI(ScoreType.MaximumMaterial, task.info.maxMaterial)
                : "-";

            portrait.sprite = task.info.character.portrait;
        }
        else
        {
            title.text = "-";
            context.text = "-";
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
