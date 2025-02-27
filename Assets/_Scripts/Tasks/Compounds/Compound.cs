using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Compound : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TaskManager taskManager;
    public CompoundInfo info;

    [Header("UI References")]
    [SerializeField] private Image givingTaskImg;
    public Vector3Int offset = new Vector3Int(0, 3, 0);

    private Task givingTask;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable() { taskManager.TaskAccepted += OnAcceptedTask; }
    private void OnDisable() { taskManager.TaskAccepted -= OnAcceptedTask; }

    private void Update()
    {
        // Get Screen Borders
        float minX = givingTaskImg.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;

        float minY = givingTaskImg.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        if (IsGivingTask())
        {
            givingTaskImg.gameObject.SetActive(true);
            Vector2 newTaskPos = mainCamera.WorldToScreenPoint(this.transform.position + offset);

            // Limit to Borders of the Screen
            newTaskPos.x = Mathf.Clamp(newTaskPos.x, minX, maxX);
            newTaskPos.y = Mathf.Clamp(newTaskPos.y, minY, maxY);

            givingTaskImg.transform.position = newTaskPos;
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Compound Giving a Task?
    public bool IsGivingTask() { return givingTask != null; }

    // ::::: Get Task for the Player to Receive from Compound
    public Task GetNextAvailableTask(int currentMapState)
    {
        List<Task> tasks = TaskDiary.Instance.tasks;
        var currentStateTasks = tasks.Where(t => t.from == this && t.info.id.x == currentMapState)
            .OrderBy(t => t.info.id.y).ToList();

        givingTask = currentStateTasks.FirstOrDefault(t => t.state == TaskState.Unlocked);
        return givingTask;
    }

    public void OnAcceptedTask(Task task, bool isManualAccept)
    {
        if (IsGivingTask())
            if (task == givingTask)
            {
                givingTaskImg.gameObject.SetActive(false);
                givingTask = null;
            }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Click on Compound (Mesh)
    private void OnMouseDown()
    {
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) // Prevent UI Interference
        {
            if (!IsGivingTask()) return;
            TaskReceiver.Instance.ReceiveTask(givingTask);
        }
    }
}
