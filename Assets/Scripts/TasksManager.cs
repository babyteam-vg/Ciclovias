using System.Collections.Generic;
using UnityEngine;

public class TasksManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;

    private Pathfinder pathfinder;

    public List<Task> tasks = new List<Task>();
    public List<Task> activeTasks = new List<Task>();

    // === Methods ===
    private void Awake() { pathfinder = new Pathfinder(graph); }

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
    public void ValidateTask(Task task)
    {
        if (task.completed || !task.available) return;

        Vector2? startNode = FindNodeInCells(task.info.startCells);
        Vector2? destinationNode = FindNodeInCells(task.info.destinationCells);

        if (startNode.HasValue && destinationNode.HasValue)
        { //                  A* <¬
            var path = pathfinder.FindPath(startNode.Value, destinationNode.Value);
            if (path != null)
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

        Vector2? startNode = FindNodeInCells(task.info.startCells);
        Vector2? destinationNode = FindNodeInCells(task.info.destinationCells);

        if (!startNode.HasValue || !destinationNode.HasValue ||
            pathfinder.FindPath(startNode.Value, destinationNode.Value) == null)
        {
            task.completed = false;
            Debug.Log($"Task '{task.info.title}' Decompleted");
        }
    }

    // 
    private Vector2? FindNodeInCells(List<Vector2Int> cells)
    {
        foreach (var cell in cells)
        {
            Vector2 nodePosition = grid.EdgeToMid(cell);
            if (graph.GetNode(nodePosition) != null) return nodePosition;
        }
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