using System;
using System.Collections;
using UnityEngine;

public class LaneScores : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform scoresContainer;
    public Vector3Int offset = new Vector3Int(1, 1, 1);

    private Coroutine deactivateCoroutine;
    private Vector3 lastPosition = new Vector3();

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        laneConstructor.OnBuildStarted += HandleLaneStarted;
        laneConstructor.OnLaneBuilt += HandleLaneUpdated;
        laneConstructor.OnBuildFinished += HandleLaneFinished;

        laneDestructor.OnDestroyStarted += HandleLaneStarted;
        laneDestructor.OnLaneDestroyed += HandleLaneUpdated;
        laneDestructor.OnDestroyFinished += HandleLaneFinished;
    }
    private void OnDisable()
    {
        laneConstructor.OnBuildStarted -= HandleLaneStarted;
        laneConstructor.OnLaneBuilt -= HandleLaneUpdated;
        laneConstructor.OnBuildFinished -= HandleLaneFinished;

        laneDestructor.OnDestroyStarted -= HandleLaneStarted;
        laneDestructor.OnLaneDestroyed -= HandleLaneUpdated;
        laneDestructor.OnDestroyFinished -= HandleLaneFinished;
    }

    public void Update()
    {
        Vector2 newPos = mainCamera.WorldToScreenPoint(lastPosition);
        scoresContainer.transform.position = newPos;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void HandleLaneStarted(Vector2Int gridPosition)
    {
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = null;
        }

        if (CurrentTask.Instance.ThereIsPinned())
        {
            if (CurrentTask.Instance.PinnedTask.info.safetyRequirement) scoresContainer.GetChild(0).gameObject.SetActive(true);
            if (CurrentTask.Instance.PinnedTask.info.charmRequirement) scoresContainer.GetChild(1).gameObject.SetActive(true);
        }
    }

    private void HandleLaneUpdated(Vector2Int gridPosition)
    {
        if (CurrentTask.Instance.ThereIsPinned())
        {
            Vector3 worldPos = grid.GetWorldPositionFromCell(gridPosition.x, gridPosition.y) + offset;
            lastPosition = worldPos;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            scoresContainer.transform.position = screenPos;
        }
    }

    private void HandleLaneFinished(Vector2Int gridPosition)
    {
        if (CurrentTask.Instance.ThereIsPinned())
            deactivateCoroutine = StartCoroutine(DeactivateAfterDelay(5f));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (CurrentTask.Instance.PinnedTask.info.safetyRequirement) scoresContainer.GetChild(0).gameObject.SetActive(false);
        if (CurrentTask.Instance.PinnedTask.info.charmRequirement) scoresContainer.GetChild(1).gameObject.SetActive(false);
        deactivateCoroutine = null;
    }
}
