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

    [Header("UI References - From")]
    public GameObject from;

    [Header("UI References - To")]
    public GameObject to;

    private Vector3 fromOffset, toOffset;
    private Transform fromCompoundPos, toCompundPos;
    private Image fromFull, fromBorder, fromIll, toFull, toBorder, toIll;

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
        fromFull = from.GetComponentsInChildren<Image>(true)[0];
        fromBorder = from.GetComponentsInChildren<Image>(true)[1];
        fromIll = from.GetComponentsInChildren<Image>(true)[2];

        toFull = to.GetComponentsInChildren<Image>(true)[0];
        toBorder = to.GetComponentsInChildren<Image>(true)[1];
        toIll = to.GetComponentsInChildren<Image>(true)[2];
    }

    public void Update()
    {
        // Get Screen Borders
        float minX = fromFull.GetPixelAdjustedRect().width / 3;
        float maxX = Screen.width - minX;

        float minY = -fromFull.GetPixelAdjustedRect().height / 3;
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
            fromBorder.gameObject.SetActive(isFromOutOfBounds);

            bool isToOutOfBounds = toPos.x <= minX || toPos.x >= maxX || toPos.y <= minY || toPos.y >= maxY;
            toFull.gameObject.SetActive(!isToOutOfBounds); // Deactivate 'Full' if Out of Bounds
            toBorder.gameObject.SetActive(isToOutOfBounds);
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

        fromIll.sprite = task.from.info.illustration;
        toIll.sprite = task.to.info.illustration;
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
