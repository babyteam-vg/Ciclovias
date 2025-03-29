using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskWaypoint : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CurrentTask currentTask;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("UI References")]
    [SerializeField] private GameObject from;
    [SerializeField] private GameObject to;

    private Vector3 fromOffset, toOffset;
    private Transform fromCompoundPos, toCompundPos;
    private Image fromImg, toImg;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        currentTask.TaskPinned += UpdateTaskWaypoints;
        currentTask.TaskUnpinned += HideWaypoints;

        tutorialManager.TutorialStarted += HideTutorialWaypoints;
        tutorialManager.TutorialCompleted += RecoverWaypoints;
    }
    private void OnDisable()
    {
        currentTask.TaskPinned -= UpdateTaskWaypoints;
        currentTask.TaskUnpinned -= HideWaypoints;

        tutorialManager.TutorialStarted -= HideTutorialWaypoints;
        tutorialManager.TutorialCompleted -= RecoverWaypoints;
    }

    private void Start()
    {
        fromImg = from.GetComponentInChildren<Image>(true);
        toImg = to.GetComponentInChildren<Image>(true);
    }

    public void Update()
    {
        // Get Screen Borders
        float minX = fromImg.GetPixelAdjustedRect().width / 3;
        float maxX = Screen.width - minX;

        float minY = -fromImg.GetPixelAdjustedRect().height / 3;
        float maxY = Screen.height - fromImg.GetPixelAdjustedRect().height;

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
            fromImg.gameObject.SetActive(!isFromOutOfBounds); // Deactivate 'Full' if Out of Bounds

            bool isToOutOfBounds = toPos.x <= minX || toPos.x >= maxX || toPos.y <= minY || toPos.y >= maxY;
            toImg.gameObject.SetActive(!isToOutOfBounds); // Deactivate 'Full' if Out of Bounds
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

    // ::::: When a Tutorial is Completed
    private void RecoverWaypoints()
    {
        if (CurrentTask.Instance.ThereIsPinned())
        {
            from.gameObject.SetActive(true);
            to.gameObject.SetActive(true);
        }
    }

    // ::::: When Unpinned or Tutorial Started
    private void HideTutorialWaypoints(Tutorial _) { HideWaypoints(); }

    private void HideWaypoints()
    {
        from.gameObject.SetActive(false);
        to.gameObject.SetActive(false);
    }
}
