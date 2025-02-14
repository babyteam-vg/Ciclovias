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
            portrait.sprite = ReceivedTask.info.character.portrait;
        }
    }
}
