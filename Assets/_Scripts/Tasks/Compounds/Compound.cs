using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Compound : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Compound compound;
    [SerializeField] private Camera mainCamera;

    [Header("UI References")]
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private Image newTaskImg;

    private Task givingTask;

    // :::::::::: MONO METHODS ::::::::::
    public void Update()
    {
        // Get Screen Borders
        float minX = newTaskImg.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;

        float minY = newTaskImg.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        if (IsGivingTask())
        {
            newTaskImg.gameObject.SetActive(true);
            Vector2 newTaskPos = mainCamera.WorldToScreenPoint(this.transform.position);

            // Limit to Borders of the Screen
            newTaskPos.x = Mathf.Clamp(newTaskPos.x, minX, maxX);
            newTaskPos.y = Mathf.Clamp(newTaskPos.y, minY, maxY);

            newTaskImg.transform.position = newTaskPos;
        }
    }

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

            newTaskImg.gameObject.SetActive(false);
            TaskReceiver.Instance.ReceiveTask(givingTask);
            menuManager.OnReceiveTaskPress();
            givingTask = null;
        }
    }
}
