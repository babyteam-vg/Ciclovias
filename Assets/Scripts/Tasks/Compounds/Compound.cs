using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Compound : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Compound compound;

    [Header("UI References")]
    [SerializeField] private MenuManager menuManager;

    private GameObject taskIconInstance;
    private Task givingTask;

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Compound Giving a Task?
    public bool IsGivingTask() { return givingTask != null; }

    // ::::: Get Task for the Player to Receive from Compound
    public Task GetNextAvailableTask(int currentMapState)
    {
        List<Task> tasks = TaskDiary.Instance.tasks;
        var currentStateTasks = tasks.Where(t => t.fromCompound == this && t.info.map == currentMapState)
            .OrderBy(t => t.info.number).ToList();

        givingTask = currentStateTasks.FirstOrDefault(t => t.state == 1);
        return givingTask;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Click on Compound (Mesh)
    private void OnMouseDown()
    {
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) // Prevent UI Interference
        {
            if (!IsGivingTask())
                return;

            TaskReceiver.Instance.ReceiveTask(givingTask);
            menuManager.OnReceiveTaskPress();
            givingTask = null;
        }
    }
}
