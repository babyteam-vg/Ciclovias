using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskReceiver : MonoBehaviour
{
    public static TaskReceiver Instance { get; private set; }
    public Task ReceivedTask { get; private set; }

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

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Is There a Received Task?
    public bool ThereIsReceived() { return ReceivedTask != null; }

    // ::::: Receive a Task
    public void ReceiveTask(Task task)
    {
        if (ReceivedTask == task)
            return;

        ReceivedTask = task;
        MenuManager.Instance.OnOpenDialog();
        UpdateTaskUI();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: UI Only Affected When Changing the Pinned Task
    private void UpdateTaskUI()
    {
        if (ThereIsReceived())
        {
            title.text = ReceivedTask.info.title;
            context.text = ReceivedTask.info.context;
            safety.text = ReceivedTask.info.safetyRequirement ? ReceivedTask.info.minSafetyCount.ToString() : "No safety requirement";
            charm.text = ReceivedTask.info.charmRequirement ? ReceivedTask.info.minCharmCount.ToString() : "No charm requirement";
            flow.text = ReceivedTask.info.flowRequirement ? ReceivedTask.info.minFlowPercentage.ToString() : "No flow requirement";
            minMat.text = ReceivedTask.info.minMaterialRequirement ? ReceivedTask.info.minMaterial.ToString() : "Min. NO";
            maxMat.text = ReceivedTask.info.maxMaterialRequirement ? ReceivedTask.info.maxMaterial.ToString() : "Max. NO";
            portrait.sprite = ReceivedTask.info.character.portrait;
        }
    }
}
