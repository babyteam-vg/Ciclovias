using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Compound : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TutorialManager tutorialManager;
    public CompoundInfo info;

    [Header("UI References")]
    [SerializeField] private GameObject givingTaskUI;
    public Vector3 offset = new Vector3(0, 2, 0);

    private Task givingTask;
    private Image full, portrait;

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

    private void Start()
    {
        full = givingTaskUI.GetComponentInChildren<Image>(true);
        portrait = givingTaskUI.GetComponentsInChildren<Image>(true)[3];
    }

    private void Update()
    {
        // Get Screen Borders
        float minX = full.GetPixelAdjustedRect().width / 3;
        float maxX = Screen.width - minX;

        float minY = -full.GetPixelAdjustedRect().height / 3;
        float maxY = Screen.height + minY;

        if (IsGivingTask())
        {
            portrait.sprite = givingTask.info.character.portrait;

            Vector2 newTaskPos = mainCamera.WorldToScreenPoint(this.transform.position + offset);

            newTaskPos.x = Mathf.Clamp(newTaskPos.x, minX, maxX);
            newTaskPos.y = Mathf.Clamp(newTaskPos.y, minY, maxY);

            givingTaskUI.transform.position = newTaskPos;

            bool isOutOfBounds = newTaskPos.x <= minX || newTaskPos.x >= maxX || newTaskPos.y <= minY || newTaskPos.y >= maxY;
            full.gameObject.SetActive(!isOutOfBounds); // Deactivate if Out of Bounds
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
        givingTaskUI.SetActive(true);
    }

    public void OnAcceptedTask(Task task, bool isManualAccept)
    {
        if (IsGivingTask())
            if (task == givingTask)
            {
                givingTaskUI.SetActive(false);
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

    // ::::: When a Tutorial is Started or Completed
    private void RecoverGivingTask()
    {
        if (IsGivingTask())
            givingTaskUI.SetActive(true);
    }
    private void HideGivingTask(Tutorial _)
    {
        givingTaskUI.SetActive(false);
    }
}