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
            if (task.state == 4 && !TaskLaneCompleted(task)) // Deactivate Task
                DecompleteTask(task); // Completed -> Accepted
            
            if (task.state == 3 && !TaskLaneStarted(task)) // Deactivate Task
                ChangeTaskState(2, task); // Active -> Accepted

            if (task.state == 2 && TaskLaneStarted(task)) // Activate Task
                ChangeTaskState(3, task); // Accepted -> Active
        }
    }

    // ::::: To Accept a Task in the Receive Task UI
    public void AcceptTask(Task task) { ChangeTaskState(2, task); }

    // ::::: Lane Relative to Task
    public bool TaskLaneStarted(Task task) { return graph.ContainsAny(task.from.info.surroundings); }
    public bool TaskLaneCompleted(Task task) { return graph.ContainsAny(task.from.info.surroundings) && graph.ContainsAny(task.to.info.surroundings); }

    // ::::: Lane Building Feedback
    public void TaskInProgress(Vector2Int gridPosition)
    {
        foreach (var activeTask in TaskDiary.Instance.tasks.Where(t => t.state == 3).ToList()) // Only Active Tasks
        { //                 Does the Lane Passes Through the Start? <¬            Does Not <¬
            Vector2Int startNode = graph.FindNodeInCells(activeTask.from.info.surroundings) ?? activeTask.from.info.surroundings.FirstOrDefault();
            Vector2Int destinationNode = graph.FindNodeInCells(activeTask.to.info.surroundings) ?? activeTask.to.info.surroundings.FirstOrDefault();
            //                       Execute A* <¬
            var (pathFound, path) = pathfinder.FindPath(startNode, gridPosition, destinationNode);

            graphRenderer.currentPath = path;

            int safety = cellScoresCalculator.CalculatePathSafety(path);
            int charm = cellScoresCalculator.CalculatePathCharm(path);
            float flow = cellScoresCalculator.CalculatePathFlow(path, destinationNode);

            activeTask.currentSafetyCount = safety;
            activeTask.currentCharmCount = charm;
            activeTask.currentFlowPercentage = flow;

            if (pathFound && activeTask.MeetsRequirements())
                CompleteTask(activeTask, path); // Complete the Task
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Complete & Decomplete a Task
    private void CompleteTask(Task task, List<Vector2Int> path)
    {
        GameManager.Instance.AddMaterial(task.info.materialReward);

        foreach (Vector2Int taskId in task.info.unlockedTasks)
        {
            int map = taskId.x;
            int number = taskId.y;

            Task unlockedTask = TaskDiary.Instance.tasks.FirstOrDefault(t => t.info.map == map && t.info.number == number);
            if (unlockedTask != null && unlockedTask.state == 0)
            {
                ChangeTaskState(1, unlockedTask); // Unlock
                Compound compound = unlockedTask.from;
                compound.GetNextAvailableTask(GameManager.Instance.MapState);
            }
        }

        ChangeTaskState(4, task, path); // Completed
        InGameMenuManager.Instance.OnOpenDialog(task);
    }
    private void DecompleteTask(Task task)
    {
        GameManager.Instance.ConsumeMaterial(task.info.materialReward);
        ChangeTaskState(2, task); // Completed -> Accepted
    }

    // :::::
    private void SealTasks(int newMapState)
    {
        int oldMapState = newMapState--;
        foreach (Task task in TaskDiary.Instance.tasks.Where(t => t.info.map == oldMapState))
            ChangeTaskState(5, task);
    }

    // ::::: Change the State of a Task
    private void ChangeTaskState(int state, Task task, List<Vector2Int> path = null)
    {
        switch (state)
        {
            case 0: // Lock
                if (task.state != 0)
                    task.state = 0;
                break;

            case 1: // Unlock
                if (task.state != 1)
                {
                    task.state = 1;
                    TaskUnlocked?.Invoke(task); // !
                } break;

            case 2: // Accept
                if (task.state != 2)
                {
                    task.state = 2;
                    TaskAccepted?.Invoke(task, false); // !
                } break;

            case 3: // Activate
                if (task.state != 3)
                {
                    task.state = 3;
                    TaskActivated?.Invoke(task, false); // !
                } break;

            case 4: // Complete
                if (task.state != 4)
                {
                    task.state = 4;
                    TaskCompleted?.Invoke(task); // !
                    audioManager.PlaySFX(audioManager.complete);
                } break;

            case 5: // Seal
                if (task.state != 5)
                {
                    task.state = 5;
                    TaskSealed?.Invoke(task); // !
                } break;
        }
    }
}