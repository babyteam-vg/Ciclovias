using System.Collections.Generic;
using UnityEngine;

public class TasksDiary : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    public List<Task> tasks = new List<Task>();
    public List<Task> activeTasks = new List<Task>();

    private TasksManager tasksManager;

    // === Methods ===
    private void Awake()
    {
        Pathfinder pathfinder = new Pathfinder(graph);
        CellScoresCalculator calculator = new CellScoresCalculator(grid);

        tasksManager = new TasksManager(graph, pathfinder, calculator);
    }

    private void OnEnable()
    {
        laneConstructor.OnLaneBuilt += HandleLaneBuilt;
        laneDestructor.OnLaneDestroyed += HandleLaneDestroyed;
    }
    private void OnDisable()
    {
        laneConstructor.OnLaneBuilt -= HandleLaneBuilt;
        laneDestructor.OnLaneDestroyed -= HandleLaneDestroyed;
    }

    // When a Lane is Built
    private void HandleLaneBuilt(Vector2Int newNode)
    {
        tasksManager.UpdateActiveTasks(tasks, activeTasks);
        tasksManager.TaskInProgress(activeTasks);
    }

    // When a Lane is Destroyed
    private void HandleLaneDestroyed(Vector2Int destroyedNode)
    {
        tasksManager.UpdateActiveTasks(tasks, activeTasks);
        tasksManager.TaskInProgress(activeTasks);
    }

    // Is the New Node Part of a Task?
    private bool BelongsToTask(Task task, Vector2Int nodePosition)
    {
        Vector2Int gridCell = Vector2Int.FloorToInt(nodePosition);
        return task.info.startCells.Contains(gridCell) || task.info.destinationCells.Contains(gridCell);
    }

    private void OnDestroy()
    {
        laneConstructor.OnLaneBuilt -= HandleLaneBuilt;
        laneDestructor.OnLaneDestroyed -= HandleLaneDestroyed;
    }
}
