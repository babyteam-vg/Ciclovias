using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskDiary : MonoBehaviour
{
    public static TaskDiary Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    [Header("UI References")]
    [SerializeField] private Transform contentTransform;
    [SerializeField] private TextMeshProUGUI taskTitle;
    [SerializeField] private TextMeshProUGUI taskFrom;
    [SerializeField] private TextMeshProUGUI taskTo;
    [SerializeField] private TextMeshProUGUI requirementsSafety;
    [SerializeField] private TextMeshProUGUI requirementsCharm;
    [SerializeField] private TextMeshProUGUI requirementsFlow;
    [SerializeField] private TextMeshProUGUI minimumMaterial;
    [SerializeField] private TextMeshProUGUI maximumMaterial;
    [SerializeField] private Button pinButton;
    [SerializeField] private TextMeshProUGUI taskCharacter;
    [SerializeField] private TextMeshProUGUI taskDialog;

    public List<Task> tasks = new List<Task>();

    private TaskManager taskManager;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        Pathfinder pathfinder = new Pathfinder(graph);
        CellScoresCalculator calculator = new CellScoresCalculator(grid);

        taskManager = new TaskManager(graph, pathfinder, calculator);
    }

    private void OnEnable()
    {
        laneConstructor.OnLaneBuilt += HandleLaneUpdated;
        laneDestructor.OnLaneDestroyed += HandleLaneUpdated;
    }

    private void OnDisable()
    {
        laneConstructor.OnLaneBuilt -= HandleLaneUpdated;
        laneDestructor.OnLaneDestroyed -= HandleLaneUpdated;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Get All Unlocked Tasks Starting Cells
    public List<Vector2Int> GetTasksStartCells()
    {
        List<Vector2Int> tasksStartCells = new List<Vector2Int>();

        foreach (Task task in tasks)
            if (task.state == 2)
                foreach (Vector2Int startCell in task.info.from.surroundings)
                    tasksStartCells.Add(startCell);

        return tasksStartCells;
    }

    // ::::: Unlock a Task
    public void UnlockTask(Task task) { taskManager.UnlockTask(task); }

    // ::::: New Task UI
    public void AcceptTask(Task task) { taskManager.AcceptTask(task); }

    // ::::: Tasks Diary UI
    public void ShowAvailableTasks()
    {
        foreach (Transform child in contentTransform)
            Destroy(child.gameObject);

        foreach (Task task in tasks)
        { //       Accepted <¬          Active <¬
            if (task.state == 2 || task.state == 3)
            {
                taskTitle.text = task.info.title;
                taskFrom.text = task.info.from.compoundName;
                taskTo.text = task.info.to.compoundName;

                requirementsSafety.text = task.info.maxSafetyDiscount.ToString();
                requirementsCharm.text = task.info.minCharmCount.ToString();
                requirementsFlow.text = task.info.minFlowPercentage.ToString();
                minimumMaterial.text = "Min. " + task.info.minMaterial.ToString();
                maximumMaterial.text = "Max. " + task.info.maxMaterial.ToString();

                pinButton.onClick.AddListener(() => {
                    CurrentTask.Instance.PinTask(task);
                });

                //taskCharacter.text = task.info.character.characterName;
                //taskDialog.text = task.info.dialog;
            }
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: When a Lane is Built or Destroyed
    private void HandleLaneUpdated(Vector2Int newNode)
    {
        taskManager.UpdateActiveTasks(tasks);
        taskManager.TaskInProgress(newNode);
    }
}
