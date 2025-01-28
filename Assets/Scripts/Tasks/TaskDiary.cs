using System.Collections.Generic;
using UnityEngine;

public class TaskDiary : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

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

    // Is the New Node Part of a Task?
    private bool BelongsToTask(Task task, Vector2Int nodePosition)
    {
        Vector2Int gridCell = Vector2Int.FloorToInt(nodePosition);
        return task.info.startCells.Contains(gridCell) || task.info.destinationCells.Contains(gridCell);
    }
}
