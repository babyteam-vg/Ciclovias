using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Compound", menuName = "Compound", order = 1)]
public class Compound : ScriptableObject
{
    [Header("Basic")]
    public string name;
    public Vector2 reference;
    public List<Vector2Int> surroundings;

    private Task givingTask;

    public event System.Action OnTaskUnlocked;

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Compound Giving a Task?
    public bool IsGivingTask() { return givingTask != null; }

    // ::::: Get Task for the Player to Receive from Compound
    public Task GetNextAvailableTask(int currentMapState)
    {
        List<Task> tasks = TaskDiary.Instance.tasks;
        var currentStateTasks = tasks.Where(t => t.info.from == this && t.info.map == currentMapState)
                                     .OrderBy(t => t.info.number).ToList();

        foreach (Task task in currentStateTasks)
        {
            if (task.state == 1) // Unlocked
            {
                if (givingTask != task)
                {
                    givingTask = task;
                    OnTaskUnlocked?.Invoke();
                }
                return task;
            }
        }

        if (givingTask != null) // No más tareas disponibles
        {
            givingTask = null;
            OnTaskUnlocked?.Invoke();
        }

        return null;
    }

    // ::::: When the Player Clicks the Compound to Receive a Task
    public void OnPlayerInteract()
    {
        if (givingTask != null)
        {
            TaskReceiver.Instance.ReceiveTask(givingTask);
            givingTask = null;
            OnTaskUnlocked?.Invoke();
        }
    }
}
