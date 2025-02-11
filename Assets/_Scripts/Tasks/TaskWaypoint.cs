using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskWaypoint : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Image fromImg;
    [SerializeField] private Image toImg;

    private Transform fromCompoundPos;
    private Transform toCompundPos;

    // :::::::::: MONO METHODS ::::::::::
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
            Vector2 fromPos = mainCamera.WorldToScreenPoint(fromCompoundPos.position);
            Vector2 toPos = mainCamera.WorldToScreenPoint(toCompundPos.position);

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
    public void UpdateTaskWaypoints(Transform fromTaskPos, Transform toTaskPos)
    {
        fromImg.gameObject.SetActive(true);
        toImg.gameObject.SetActive(true);

        fromCompoundPos = fromTaskPos;
        toCompundPos = toTaskPos;
    }

    // ::::: When Unpinned
    public void HideTaskWaypoints()
    {
        fromImg.gameObject.SetActive(false);
        toImg.gameObject.SetActive(false);
    }
}
