using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TaskManager
{
    private Graph graph;
    private Pathfinder pathfinder;
    private CellScoresCalculator cellScoresCalculator;

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Construuctor
    public TaskManager(Graph graph, Pathfinder pathfinder, CellScoresCalculator cellScoresCalculator)
    {
        this.graph = graph;
        this.pathfinder = pathfinder;
        this.cellScoresCalculator = cellScoresCalculator;
    }

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

    // ::::: Unlock & Accept a Task
    public void UnlockTask(Task task) { ChangeTaskState(1, task); }
    public void AcceptTask(Task task) { ChangeTaskState(2, task); }

    // ::::: Lane Relative to Task
    public bool TaskLaneStarted(Task task) { return graph.ContainsAny(task.info.from.surroundings); }
    public bool TaskLaneCompleted(Task task) { return graph.ContainsAny(task.info.from.surroundings) && graph.ContainsAny(task.info.to.surroundings); }

    // ::::: Lane Building Feedback
    public void TaskInProgress()
    {
        foreach (var activeTask in TaskDiary.Instance.tasks.Where(t => t.state == 3).ToList()) // Only Active Tasks
        { //                 Does the Lane Passes Through the Start? <¬            Does Not <¬
            Vector2Int startNode = graph.FindNodeInCells(activeTask.info.from.surroundings) ?? activeTask.info.to.surroundings.FirstOrDefault();
            Vector2Int destinationNode = graph.FindNodeInCells(activeTask.info.to.surroundings) ?? activeTask.info.to.surroundings.FirstOrDefault();
            //                       Execute A* <¬
            var (pathFound, path) = pathfinder.FindPath(startNode, destinationNode);

            int safety = cellScoresCalculator.CalculatePathSafety(path);
            int charm = cellScoresCalculator.CalculatePathCharm(path);
            float flow = cellScoresCalculator.CalculatePathFlow(path, destinationNode);

            activeTask.currentSafetyDiscount = safety;
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
        ConstructionMaterial.Instance.AddMaterial(task.info.materialReward);

        foreach (Vector2Int taskId in task.info.unlockedTasks)
        {
            int map = taskId.x;
            int number = taskId.y;

            Task unlockedTask = TaskDiary.Instance.tasks.FirstOrDefault(t => t.info.map == map && t.info.number == number);
            if (unlockedTask != null && unlockedTask.state == 0)
            {
                UnlockTask(unlockedTask);
                Compound compound = unlockedTask.fromCompound;
                compound.GetNextAvailableTask(GameStateManager.Instance.CurrentMapState);
            }
        }

        ChangeTaskState(4, task, path); // Completed
    }
    private void DecompleteTask(Task task)
    {
        ConstructionMaterial.Instance.ConsumeMaterial(task.info.materialReward);
        ChangeTaskState(2, task); // Completed -> Accepted
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
                    task.state = 1;
                break;

            case 2: // Accept
                if (task.state != 2)
                {
                    task.state = 2;
                    if (CurrentTask.Instance != null)
                        CurrentTask.Instance.PinTask(task); // Pin Task
                }
                break;

            case 3: // Activate
                if (task.state != 3)
                {
                    task.state = 3;
                    if (CurrentTask.Instance != null)
                        CurrentTask.Instance.PinTask(task); // Pin Task
                } break;

            case 4: // Complete
                if (task.state != 4)
                {
                    task.state = 4;
                    task.SetCompletedLane(path);
                    CurrentTask.Instance.UnpinTask(task);
                } break;
        }
    }
}