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
    [SerializeField] private GameObject from;
    [SerializeField] private GameObject to;

    private Vector3 fromOffset, toOffset;
    private Transform fromCompoundPos, toCompundPos;
    private Image fromFull, toFull, fromCircleBorder;

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

    private void Start()
    {
        fromFull = from.GetComponentInChildren<Image>(true);
        fromCircleBorder = from.GetComponentsInChildren<Image>(true)[2];

        toFull = to.GetComponentInChildren<Image>(true);
    }

    public void Update()
    {
        // Get Screen Borders
        float minX = fromCircleBorder.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;

        float minY = fromCircleBorder.GetPixelAdjustedRect().height - fromFull.GetPixelAdjustedRect().height;
        float maxY = Screen.height - fromFull.GetPixelAdjustedRect().height;

        if (CurrentTask.Instance.ThereIsPinned())
        {
            // Waypoints Reposition
            Vector2 fromPos = mainCamera.WorldToScreenPoint(fromCompoundPos.position + fromOffset);
            Vector2 toPos = mainCamera.WorldToScreenPoint(toCompundPos.position + toOffset);

            // Limit to Borders of the Screen
            fromPos.x = Mathf.Clamp(fromPos.x, minX, maxX);
            fromPos.y = Mathf.Clamp(fromPos.y, minY, maxY);

            toPos.x = Mathf.Clamp(toPos.x, minX, maxX);
            toPos.y = Mathf.Clamp(toPos.y, minY, maxY);

            // Update positions
            from.transform.position = fromPos;
            to.transform.position = toPos;

            bool isFromOutOfBounds = fromPos.x <= minX || fromPos.x >= maxX || fromPos.y <= minY || fromPos.y >= maxY;
            fromFull.gameObject.SetActive(!isFromOutOfBounds); // Deactivate 'Full' if Out of Bounds

            bool isToOutOfBounds = toPos.x <= minX || toPos.x >= maxX || toPos.y <= minY || toPos.y >= maxY;
            toFull.gameObject.SetActive(!isToOutOfBounds); // Deactivate 'Full' if Out of Bounds
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: When Pinned
    private void UpdateTaskWaypoints(Task task)
    {
        fromCompoundPos = task.from.transform;
        toCompundPos = task.to.transform;

        fromOffset = task.from.offset;
        toOffset = task.to.offset;

        from.gameObject.SetActive(true);
        to.gameObject.SetActive(true);
    }

    // ::::: When Unpinned
    private void HideTaskWaypoints()
    {
        from.gameObject.SetActive(false);
        to.gameObject.SetActive(false);
    }
}
