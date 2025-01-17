using System.Collections.Generic;
using UnityEngine;

public class TasksDiary : MonoBehaviour
{
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;
    [SerializeField] private TasksManager tasksManager;

    // === Methods ===
    private void Start()
    {
        laneConstructor.OnLaneBuilt += HandleLaneBuilt;
        laneDestructor.OnLaneDestroyed += HandleLaneDestroyed;
    }

    // OnLaneBuilt
    private void HandleLaneBuilt(Vector2Int gridPosition)
    {
        foreach (var task in tasksManager.tasks)
        {
            if (!task.completed && task.available)
                if (BelongsToTask(task, gridPosition))
                    tasksManager.CheckIfTaskCompleted(task);
        }
    }

    // OnLaneDestroyed
    private void HandleLaneDestroyed(Vector2Int gridPosition)
    {
        foreach (var task in tasksManager.tasks)
            if (task.completed && task.available)
                tasksManager.CheckIfTaskDecompleted(task);
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
