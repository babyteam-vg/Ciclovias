using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Compound : MonoBehaviour
{
    public CompoundInfo info;

    [Header("Dependencies")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("UI References")]
    public GameObject givingTaskPrefab;
    public Transform markers;
    public Vector3 offset = new Vector3(0, 2, 0);

    private GameObject givingTaskUI;
    private Button givingTaskButton;
    private Image portrait, full, border;
    private Task givingTask;
    private float minX, maxX, minY, maxY;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        gameManager.MapStateAdvanced += GetNextAvailableTask;

        taskManager.TaskAccepted += OnAcceptedTask;

        tutorialManager.TutorialStarted += HideGivingTask;
        tutorialManager.TutorialCompleted += RecoverGivingTask;
    }
    private void OnDisable()
    {
        gameManager.MapStateAdvanced -= GetNextAvailableTask;

        taskManager.TaskAccepted -= OnAcceptedTask;

        tutorialManager.TutorialStarted -= HideGivingTask;
        tutorialManager.TutorialCompleted -= RecoverGivingTask;
    }

    private void Update()
    {
        if (IsGivingTask())
        {
            Vector2 newTaskPos = mainCamera.WorldToScreenPoint(this.transform.position + offset);

            newTaskPos.x = Mathf.Clamp(newTaskPos.x, minX, maxX);
            newTaskPos.y = Mathf.Clamp(newTaskPos.y, minY, maxY);

            givingTaskUI.transform.position = newTaskPos;

            bool isOutOfBounds = newTaskPos.x <= minX || newTaskPos.x >= maxX || newTaskPos.y <= minY || newTaskPos.y >= maxY;
            givingTaskButton.gameObject.SetActive(!isOutOfBounds); // Deactivate if Out of Bounds
            border.gameObject.SetActive(isOutOfBounds);
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Compound Giving a Task?
    public bool IsGivingTask() { return givingTask != null; }

    // ::::: Get Task for the Player to Receive from Compound
    public void GetNextAvailableTask(int currentMapState)
    {
        List<Task> tasks = TaskDiary.Instance.tasks;
        var currentStateTasks = tasks.Where(t => t.from == this && t.info.id.x == currentMapState)
            .OrderBy(t => t.info.id.y).ToList();

        givingTask = currentStateTasks.FirstOrDefault(t => t.state == TaskState.Unlocked);
        if (IsGivingTask())
        {
            // Instantiate From Prefab
            givingTaskUI = Instantiate(givingTaskPrefab, markers);
            givingTaskUI.gameObject.SetActive(true);

            // Assign Components
            givingTaskButton = givingTaskUI.GetComponentInChildren<Button>();
            givingTaskButton.onClick.AddListener(AcceptTaskFromButton);

            portrait = givingTaskUI.GetComponentsInChildren<Image>(true)[1];
            full = givingTaskUI.GetComponentsInChildren<Image>(true)[2];
            border = givingTaskUI.GetComponentsInChildren<Image>(true)[3];

            // Update Portrait
            portrait.sprite = givingTask.info.character.portrait;

            // Get Screen Borders
            minX = full.GetPixelAdjustedRect().width / 4f;
            maxX = Screen.width - minX;

            minY = -full.GetPixelAdjustedRect().height * 0.38f;
            maxY = Screen.height - full.GetPixelAdjustedRect().height * 0.88f;
        }
    }

    public void OnAcceptedTask(Task task, bool isManualAccept)
    {
        if (IsGivingTask())
            if (task == givingTask)
            {
                givingTaskUI.gameObject.SetActive(false);
                givingTask = null;
            }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Click on GivingTaskUI
    public void AcceptTaskFromButton()
    {
        if (IsGivingTask())
            TaskReceiver.Instance.ReceiveTask(givingTask);
    }

    // ::::: When a Tutorial is Started or Completed
    private void RecoverGivingTask()
    {
        if (IsGivingTask())
            givingTaskUI.gameObject.SetActive(true);
    }
    private void HideGivingTask(Tutorial _)
    {
        if (IsGivingTask())
            givingTaskUI.gameObject.SetActive(false);
    }
}