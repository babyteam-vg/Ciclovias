using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskWaypoint : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CurrentTask currentTask;
    [SerializeField] private Camera mainCamera;

    [Header("UI References")]
    [SerializeField] private Image fromImg;
    [SerializeField] private Image toImg;
    public Vector3Int offset = new Vector3Int(0, 3, 0);

    private Transform fromCompoundPos;
    private Transform toCompundPos;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        currentTask.TaskPinned += UpdateTaskWaypoints;
        currentTask.TaskUnpinned += HideTaskWaypoints;
    }
    private void OnDisable()
    {
        currentTask.TaskPinned -= UpdateTaskWaypoints;
        currentTask.TaskUnpinned -= HideTaskWaypoints;
    }

    public void Update()
    {
        // Get Screen Borders
        float minX = fromImg.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;

        float minY = fromImg.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        if (CurrentTask.Instance.ThereIsPinned())
        {
            // Waypoints Reposition
            Vector2 fromPos = mainCamera.WorldToScreenPoint(fromCompoundPos.position + offset);
            Vector2 toPos = mainCamera.WorldToScreenPoint(toCompundPos.position + offset);

            // Limit to Borders of the Screen
            fromPos.x = Mathf.Clamp(fromPos.x, minX, maxX);
            fromPos.y = Mathf.Clamp(fromPos.y, minY, maxY);

            toPos.x = Mathf.Clamp(toPos.x, minX, maxX);
            toPos.y = Mathf.Clamp(toPos.y, minY, maxY);

            fromImg.transform.position = fromPos;
            toImg.transform.position = toPos;
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: When Pinned
    public void UpdateTaskWaypoints(Task task)
    {
        fromCompoundPos = task.from.transform;
        toCompundPos = task.to.transform;

        fromImg.gameObject.SetActive(true);
        toImg.gameObject.SetActive(true);
    }

    // ::::: When Unpinned
    public void HideTaskWaypoints()
    {
        fromImg.gameObject.SetActive(false);
        toImg.gameObject.SetActive(false);
    }
}
