using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskReceiver : MonoBehaviour
{
    public static TaskReceiver Instance { get; private set; }
    public Task ReceivedTask { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI context;
    [SerializeField] private TextMeshProUGUI safety;
    [SerializeField] private TextMeshProUGUI charm;
    [SerializeField] private TextMeshProUGUI flow;
    [SerializeField] private TextMeshProUGUI minMat;
    [SerializeField] private TextMeshProUGUI maxMat;
    [SerializeField] private Image portrait;

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
        InGameMenuManager.Instance.OnOpenDialog(ReceivedTask);
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
            safety.text = task.info.safetyRequirement ? task.info.minSafetyCount.ToString() : "No safety requirement";
            charm.text = task.info.charmRequirement ? task.info.minCharmCount.ToString() : "No charm requirement";
            flow.text = task.info.flowRequirement ? task.info.minFlowPercentage.ToString() : "No flow requirement";
            minMat.text = task.info.minMaterialRequirement ? task.info.minMaterial.ToString() : "Min. NO";
            maxMat.text = task.info.maxMaterialRequirement ? task.info.maxMaterial.ToString() : "Max. NO";
            portrait.sprite = task.info.character.portrait;
        }
        else
        {
            title.text = "Task";
            context.text = "Context";
            safety.text = "No safety requirement";
            charm.text =  "No charm requirement";
            flow.text = "No flow requirement";
            minMat.text = "Min. NO";
            maxMat.text = "Max. NO";
        }
    }

    // ::::: When a Task is Accepted
    private void TaskAlreadyReceived(Task task, bool _)
    {
        ReceivedTask = null;
        UpdateTaskUI();
    }
}
