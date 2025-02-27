using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GraphRenderer graphRenderer;
    [SerializeField] private LaneScores laneScores;

    private Pathfinder pathfinder;
    private CellScoresCalculator cellScoresCalculator;

    public event Action<Task> TaskUnlocked;
    public event Action<Task, bool> TaskAccepted;
    public event Action<Task, bool> TaskActivated;
    public event Action<Task> TaskCompleted;
    public event Action<Task> TaskSealed;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        pathfinder = new Pathfinder(graph);
        cellScoresCalculator = new CellScoresCalculator(grid);
    }

    private void OnEnable() { GameManager.Instance.MapStateAdvanced += SealTasks; }
    private void OnDisable() { GameManager.Instance.MapStateAdvanced -= SealTasks; }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Active <-> Deactive
    public void UpdateActiveTasks(List<Task> tasks)
    {
        foreach (var task in tasks)
        {
            if (task.state == TaskState.Completed) // Deactivate
                if (!graph.AreConnectedByPath(task.start, task.end))
                    DecompleteTask(task); // Completed -> Accepted
            
            if (task.state == TaskState.Active && !TaskLaneStarted(task)) // Deactivate
                ChangeTaskState(TaskState.Accepted, task); // Active -> Accepted

            if (task.state == TaskState.Accepted && TaskLaneStarted(task)) // Activate
                ChangeTaskState(TaskState.Active, task); // Accepted -> Active
        }
    }

    // ::::: To Accept a Task in the Receive Task UI
    public void AcceptTask(Task task) { ChangeTaskState(TaskState.Accepted, task); }

    // ::::: Lane Relative to Task
    public bool TaskLaneStarted(Task task)
    {
        return graph.ContainsAny(task.from.info.surroundings)
            || graph.ContainsAny(task.to.info.surroundings);
    }

    // ::::: Lane Building Feedback
    public void TaskInProgress(Vector2Int gridPosition)
    {
        List<Task> activeTasks = TaskDiary.Instance.tasks.Where(t => t.state == TaskState.Active).ToList();

        foreach (Task activeTask in activeTasks) // Only Active Tasks
        {
            Vector2Int? tentativeStart = graph.FindNodePosInCells(activeTask.from.info.surroundings);
            Vector2Int? tentativeEnd = graph.FindNodePosInCells(activeTask.to.info.surroundings);

            Vector2Int? start = tentativeStart ?? tentativeEnd;

            Vector2Int startPos = start.Value;
            Vector2Int endPos = startPos.Equals(tentativeStart.Value)
                ? tentativeEnd ?? activeTask.to.info.surroundings.FirstOrDefault()
                : tentativeStart ?? activeTask.from.info.surroundings.FirstOrDefault();

            var (pathFound, path) = pathfinder.FindPath(startPos, gridPosition, endPos); // Execute A*

            graphRenderer.currentPath = path;
            laneScores.lastCellPosition = path.Any() ? path.Last() : gridPosition;

            int safety = cellScoresCalculator.CalculatePathSafety(path);
            int charm = cellScoresCalculator.CalculatePathCharm(path);
            float flow = cellScoresCalculator.CalculatePathFlow(path, endPos);
            int usedMaterial = path.Count;

            activeTask.currentSafetyCount = safety;
            activeTask.currentCharmCount = charm;
            activeTask.currentFlowPercentage = flow;
            activeTask.usedMaterial = usedMaterial;

            if (pathFound)
            {
                activeTask.start = path.First();
                activeTask.end = path.Last();

                if (activeTask.MeetsRequirements())
                    ChangeTaskState(TaskState.Completed, activeTask); // Complete the Task
            }
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void DecompleteTask(Task task)
    {
        GameManager.Instance.ConsumeMaterial(task.info.materialReward);
        ChangeTaskState(TaskState.Accepted, task); // Completed -> Accepted
    }

    // ::::: Seal All the Tasks From the Old Map State
    private void SealTasks(int newMapState)
    {
        int oldMapState = newMapState--;
        foreach (Task task in TaskDiary.Instance.tasks.Where(t => t.info.id.x == oldMapState))
            ChangeTaskState(TaskState.Sealed, task);
    }

    // ::::: Change the State of a Task
    private void ChangeTaskState(TaskState newState, Task task)
    {
        if (task.state == newState) return;

        task.state = newState;

        switch (newState)
        {
             case TaskState.Unlocked:
                TaskUnlocked?.Invoke(task);
                break;

            case TaskState.Accepted:
                TaskAccepted?.Invoke(task, false);
                break;

            case TaskState.Active:
                TaskActivated?.Invoke(task, false);
                break;

            case TaskState.Completed:
                GameManager.Instance.AddMaterial(task.info.materialReward);
                audioManager.PlaySFX(audioManager.complete);

                foreach (Vector2Int taskId in task.info.unlockedTasks)
                {
                    int map = taskId.x;
                    int number = taskId.y;

                    Task unlockedTask = TaskDiary.Instance.tasks.FirstOrDefault(t => t.info.id.x == map && t.info.id.y == number);
                    if (unlockedTask != null && unlockedTask.state == 0)
                    {
                        ChangeTaskState(TaskState.Unlocked, unlockedTask); // Unlock
                        Compound compound = unlockedTask.from;
                        compound.GetNextAvailableTask(GameManager.Instance.MapState);
                    }
                }

                InGameMenuManager.Instance.OnOpenDialog(task);
                
                TaskCompleted?.Invoke(task);
                break;

            case TaskState.Sealed:
                TaskSealed?.Invoke(task);
                break;

            default:
                break;
        }
    }
}