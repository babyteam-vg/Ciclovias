using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskDiary : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    [Header("UI References")]
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject availableTaskPrefab;

    public List<Task> tasks = new List<Task>();
    public List<Task> activeTasks = new List<Task>();

    private TaskManager taskManager;

    // === Methods ===
    private void Awake()
    {
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

    // Get All Unlocked Tasks Starting Cells
    public List<Vector2Int> GetTasksStartCells()
    {
        List<Vector2Int> tasksStartCells = new List<Vector2Int>();

        foreach (Task task in tasks)
            if (task.state == 1)
                foreach (Vector2Int startCell in task.info.startCells)
                    tasksStartCells.Add(startCell);

        return tasksStartCells;
    }

    // When a Lane is Built
    private void HandleLaneUpdated(Vector2Int newNode)
    {
        taskManager.UpdateActiveTasks(tasks, activeTasks);
        taskManager.TaskInProgress(activeTasks);
    }

    public void ShowAvailableTasks()
    {
        foreach (Transform child in contentTransform)
            Destroy(child.gameObject);

        foreach (Task task in tasks)
        {
            if (task.state == 1 || task.state == 2)
            { //                              Child <¬            Parent <¬
                GameObject newItem = Instantiate(availableTaskPrefab, contentTransform);
                //                                   TMP Part of the Prefab <¬
                TextMeshProUGUI tmpText = newItem.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                    tmpText.text = $"Task: {task.info.title}";
                //                                        Button Part of the Prefab <¬
                Button pinButton = newItem.transform.Find("Pin Task Button").GetComponent<Button>();
                pinButton.onClick.AddListener(() => {
                    CurrentTask.Instance.PinTask(task);
                });
            }
        }
    }
}
