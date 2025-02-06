using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskReceiver : MonoBehaviour
{
    public static TaskReceiver Instance { get; private set; }
    public Task ReceivedTask { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI dialog;
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
    // ::::: 
    public void ReceiveTask(Task task)
    {
        if (ReceivedTask == task) return;

        ReceivedTask = task;
        UpdateTaskUI();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: UI Only Affected When Changing the Pinned Task
    private void UpdateTaskUI()
    {
        Task task = ReceivedTask;

        title.text = task.info.title;
        dialog.text = task.info.dialog;
        portrait.sprite = task.info.character.portrait;
    }
}
