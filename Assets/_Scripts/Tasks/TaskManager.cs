using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private TaskDialogManager taskDialogManager;

    [Header("UI References")]
    public GameObject confirmButton;

    private Pathfinder pathfinder;
    private CellScoresCalculator cellScoresCalculator;

    public event Action<List<Vector2Int>> ActiveTaskScoresUpdated;
    public event Action<Task> TaskUnlocked;
    public event Action<Task, bool> TaskAccepted;
    public event Action<Task, bool> TaskActivated;
    public event Action<Task> TaskCompleted;
    public event Action<Task> TaskSealed;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        pathfinder = new Pathfinder(graph);
        cellScoresCalculator = new CellScoresCalculator(grid, graph);
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Active <-> Deactive
    public void UpdateActiveTasks(List<Task> tasks)
    {
        foreach (var task in tasks)
        {
            if (task.state == TaskState.Completed) // Deactivate
                if (!graph.AreConnectedByPath(task.path.Last(), task.path.First()))
                    DecompleteTask(task); // Completed -> Accepted
            
            if (task.state == TaskState.Active && !TaskLaneStarted(task)) // Deactivate
                ChangeTaskState(TaskState.Accepted, task); // Active -> Accepted

            if (task.state == TaskState.Accepted && TaskLaneStarted(task)) // Activate
                ChangeTaskState(TaskState.Active, task); // Accepted -> Active
        }
    }

    // ::::: UI Purposes
    public void AcceptTask(Task task) { ChangeTaskState(TaskState.Accepted, task); }
    public void ConfirmTask(Task task)
    {
        if (task.state == TaskState.Completed)
            ChangeTaskState(TaskState.Sealed, task);
    }

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
            Vector2Int tentativeStart = activeTask.from.info.surroundings.First();
            Vector2Int tentativeEnd = activeTask.to.info.surroundings.First();

            Vector2Int startPos = graph.GetAllNodesPosition().Contains(tentativeStart)
                ? tentativeStart : tentativeEnd;

            Vector2Int endPos = startPos.Equals(tentativeStart)
                ? tentativeEnd : tentativeStart;

            var (pathFound, path) = pathfinder.FindPath(startPos, gridPosition, endPos); // Execute A*

            float safety = cellScoresCalculator.CalculatePathSafety(path);
            float charm = cellScoresCalculator.CalculatePathCharm(path);
            float flow = cellScoresCalculator.CalculatePathFlow(path);
            int usedMaterial = path.Count - 1;

            activeTask.currentSafety = safety;
            activeTask.currentCharm = charm;
            activeTask.currentFlow = flow;
            activeTask.usedMaterial = usedMaterial;

            activeTask.flavorMet = MeetsFlavour(activeTask, path);

            ActiveTaskScoresUpdated?.Invoke(path);

            if (pathFound)
            {
                activeTask.path = path;

                if (activeTask.MeetsRequirements())
                    ChangeTaskState(TaskState.Completed, activeTask); // Complete the Task
            }
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: 
    private void DecompleteTask(Task task)
    {
        confirmButton.SetActive(false);
        ChangeTaskState(TaskState.Accepted, task); // Completed -> Accepted
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
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxs[1]);
                confirmButton.SetActive(true);
                TaskCompleted?.Invoke(task);
                break;

            case TaskState.Sealed:
                MaterialManager.Instance.AddMaterial(task.info.materialReward);
                graph.SealNodes(task.path);
                UnlockTasks(task);
                confirmButton.SetActive(false);
                TaskSealed?.Invoke(task);
                break;

            default:
                break;
        }
    }

    // ::::: Unlock Reward Tasks
    private void UnlockTasks(Task task)
    {
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
    }

    // ::::: Meets the Flavour?
    private bool MeetsFlavour(Task task, List<Vector2Int> path)
    {
        FlavorType type = task.info.flavorDetails.flavorType;
        switch (type)
        {
            case FlavorType.Visit:
                List<Vector2Int> visitSurroundings = task.info.flavorDetails.compound.surroundings;
                return visitSurroundings.Intersect(path).Any();

            case FlavorType.Avoid:
                List<Vector2Int> avoidSurroundings = task.info.flavorDetails.compound.surroundings;
                return !avoidSurroundings.Intersect(path).Any();

            case FlavorType.Cross:
                if (path.Count > 1)
                {
                    int index = path.Count - 1;

                    Cell lastCell = grid.GetCell(path[index - 1].x, path[index - 1].y);
                    Cell currentCell = grid.GetCell(path[index].x, path[index].y);

                    if (currentCell.GetContent() == task.info.flavorDetails.toCross
                        && lastCell.GetContent() != currentCell.GetContent())
                        task.currentToCross++;
                }
                if (task.currentToCross >= task.info.flavorDetails.numberToCross) return true;
                break;

            case FlavorType.UseLane:
                return graph.ContainsIndestructibleNode(path);

            case FlavorType.AvoidLane:
                return !graph.ContainsIndestructibleNode(path);
        }
        return false;
    }
}