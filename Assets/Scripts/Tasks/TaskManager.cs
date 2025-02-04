using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager
{
    private Graph graph;
    private Pathfinder pathfinder;
    private CellScoresCalculator cellScoresCalculator;

    // === Methods ===
    public TaskManager(Graph graph, Pathfinder pathfinder, CellScoresCalculator cellScoresCalculator)
    {
        this.graph = graph;
        this.pathfinder = pathfinder;
        this.cellScoresCalculator = cellScoresCalculator;
    }

    // Active <-> Deactive
    public void UpdateActiveTasks(List<Task> tasks, List<Task> activeTasks)
    {
        foreach (var task in tasks)
        {
            if (task.GetState() == 3 && !TaskLaneCompleted(task))
                ChangeTaskState(1, task, activeTasks); // Deactivate Task
            
            if (task.GetState() == 2 && !TaskLaneStarted(task))
                ChangeTaskState(1, task, activeTasks); // Deactivate Task

            if (task.GetState() == 1 && TaskLaneStarted(task))
                ChangeTaskState(2, task, activeTasks); // Activate Task
        }
    }

    // Unlock a Task
    public void UnlockTask(Task task)
    {
        ChangeTaskState(2, task);
    }

    // Lane Relative to Task
    public bool TaskLaneStarted(Task task) { return graph.ContainsAny(task.info.startCells); }
    public bool TaskLaneCompleted(Task task) { return graph.ContainsAny(task.info.startCells) && graph.ContainsAny(task.info.destinationCells); }

    // Lane Building Feedback
    public void TaskInProgress(List<Task> activeTasks)
    {
        List<Task> auxTasks = new List<Task>();
        foreach (var activeTask in activeTasks)
        { //                 Does the Lane Passes Through the Start? <¬            Does Not <¬
            Vector2Int startNode = graph.FindNodeInCells(activeTask.info.startCells) ?? activeTask.info.destinationCells.FirstOrDefault();
            Vector2Int destinationNode = graph.FindNodeInCells(activeTask.info.destinationCells) ?? activeTask.info.destinationCells.FirstOrDefault();
            //                       Execute A* <¬
            var (pathFound, path) = pathfinder.FindPath(startNode, destinationNode);

            int safety = cellScoresCalculator.CalculatePathSafety(path);
            int charm = cellScoresCalculator.CalculatePathCharm(path);
            float flow = cellScoresCalculator.CalculatePathFlow(path, destinationNode);

            activeTask.SetSafety(safety);
            activeTask.SetCharm(charm);
            activeTask.SetFlow(flow);

            if (pathFound)
                if (activeTask.MeetsRequirements())
                    ChangeTaskState(3, activeTask, activeTasks, path);
        } foreach (var task in auxTasks)
            activeTasks.Remove(task); // To Prevent Errors w/the Foreach
    }

    // === Methods ===
    // Change the State of a Task
    private void ChangeTaskState(int state, Task task, List<Task> activeTasks = null, List<Vector2Int> path = null)
    {
        switch (state)
        {
            case 1: // Unlock or Deactivate
                if (task.GetState() != 1)
                {
                    Debug.Log($"{task.info.title} is now Available!");
                    task.SetState(1);
                    activeTasks?.Remove(task);
                } break;

            case 2: // Activate
                if (task.GetState() != 2)
                {
                    Debug.Log($"{task.info.title} is now Active!");
                    task.SetState(2);
                    activeTasks?.Add(task);
                } break;

            case 3: // Complete
                if (task.GetState() != 3)
                {
                    Debug.Log($"{task.info.title} is now Completed!");
                    task.SetState(3);
                    //activeTasks.Remove(task);
                    task.SetCompletedLane(path);
                } break;
        }
    }
}