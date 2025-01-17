using System.Collections.Generic;
using UnityEngine;

public class TasksManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;

    private Pathfinder pathfinder;
    private CellScoresCalculator cellScoresCalculator;

    public List<Task> tasks = new List<Task>();
    public List<Task> activeTasks = new List<Task>();

    // === Methods ===
    private void Awake()
    {
        pathfinder = new Pathfinder(graph);
        cellScoresCalculator = new CellScoresCalculator(grid);
    }

    // Activate a Task
    public void ActivateTask(Task task)
    {
        if (task == null || task.active || !task.available) return;

        task.active = true;
        activeTasks.Add(task);
        Debug.Log($"Task '{task.info.title}' Activated");
    }

    // Deactivate a Task
    public void DeactivateTask(Task task)
    {
        if (task == null || !task.active) return;

        task.active = false;
        activeTasks.Remove(task);
        Debug.Log($"Task '{task.info.title}' Deactivated");
    }

    // When a Lane is Built
    public void CheckIfTaskCompleted(Task task)
    {
        if (task.completed || !task.available) return;

        Vector2Int? startNode = FindNodeInCells(task.info.startCells);
        Vector2Int? destinationNode = FindNodeInCells(task.info.destinationCells);

        if (startNode.HasValue && destinationNode.HasValue)
        {
            var (pathFound, path) = pathfinder.FindPath(startNode.Value, destinationNode.Value);
            if (pathFound && PathMeetsRequirements(path, task))
            {
                task.completed = true;
                Debug.Log($"Task '{task.info.title}' Completed!");
            }
        }
    }

    // When a Lane is Destroyed
    public void CheckIfTaskDecompleted(Task task)
    {
        if (!task.completed || !task.available) return;

        Vector2Int? startNode = FindNodeInCells(task.info.startCells);
        Vector2Int? destinationNode = FindNodeInCells(task.info.destinationCells);

        if (startNode.HasValue && destinationNode.HasValue)
        {
            var (pathFound, path) = pathfinder.FindPath(startNode.Value, destinationNode.Value);

            if (!pathFound || !PathMeetsRequirements(path, task))
            {
                task.completed = false;
                Debug.Log($"Task '{task.info.title}' Decompleted");
            }
        }
    }

    // Task Requirements
    private bool PathMeetsRequirements(List<Vector2Int> path, Task task)
    {
        float safety = cellScoresCalculator.CalculatePathSafety(path);
        float charm = cellScoresCalculator.CalculatePathCharm(path);
        float flow = cellScoresCalculator.CalculatePathFlow(path);

        return safety >= task.info.minSafety && charm >= task.info.minCharm && flow >= task.info.minFlow;
    }

    // 
    private Vector2Int? FindNodeInCells(List<Vector2Int> cells)
    {
        foreach (var cell in cells)
            if (graph.GetNode(cell) != null) return cell;

        return null;
    }
}

[System.Serializable]
public class Task
{
    [Header("Bools")]
    public bool available;
    public bool active; // UI Only
    public bool completed;

    [Header("Info")]
    public TaskInfo info;
}