using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LaneScores : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform scoresContainer;
    public Vector3Int offset = new Vector3Int(1, 1, 1);

    [Header("UI References - Safety")]
    [SerializeField] private Image safetyFill;
    [SerializeField] private Image safetyWarning;

    [Header("UI References - Charm")]
    [SerializeField] private Image charmFill;
    [SerializeField] private Image charmWarning;

    [Header("UI References - Flow")]
    [SerializeField] private Image flowFill;
    [SerializeField] private Image flowWarning;

    public Vector2Int lastCellPosition = new Vector2Int();

    private Coroutine deactivateCoroutine;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        laneConstructor.OnBuildStarted += HandleLaneStarted;
        laneConstructor.OnLaneBuilt += HandleLaneUpdated;
        laneConstructor.OnBuildFinished += HandleLaneFinished;

        taskManager.TaskCompleted += HandleTaskCompleted;

        laneDestructor.OnDestroyStarted += HandleLaneStarted;
        laneDestructor.OnLaneDestroyed += HandleLaneUpdated;
        laneDestructor.OnDestroyFinished += HandleLaneFinished;

    }
    private void OnDisable()
    {
        laneConstructor.OnBuildStarted -= HandleLaneStarted;
        laneConstructor.OnLaneBuilt -= HandleLaneUpdated;
        laneConstructor.OnBuildFinished -= HandleLaneFinished;

        taskManager.TaskCompleted -= HandleTaskCompleted;

        laneDestructor.OnDestroyStarted -= HandleLaneStarted;
        laneDestructor.OnLaneDestroyed -= HandleLaneUpdated;
        laneDestructor.OnDestroyFinished -= HandleLaneFinished;
    }

    public void Update()
    {
        if (CurrentTask.Instance.ThereIsPinned())
        {
            Task task = CurrentTask.Instance.PinnedTask;

            UpdateScoreUI(task);

            //if (task.state == TaskState.Active && graph.AreConnectedByPath(task.start, task.end))
            //{
            //    if (!task.MeetsSafetyRequirement()) safetyWarning.gameObject.SetActive(true);
            //    if (!task.MeetsCharmRequirement()) charmWarning.gameObject.SetActive(true);
            //}
            //else
            //{
            //    safetyWarning.gameObject.SetActive(false);
            //    charmWarning.gameObject.SetActive(false);
            //}
        }

        Vector3 worldPos = grid.GetWorldPositionFromCell(lastCellPosition.x, lastCellPosition.y) + offset;
        Vector2 newPos = mainCamera.WorldToScreenPoint(worldPos);
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
            Vector3 worldPos = grid.GetWorldPositionFromCell(lastCellPosition.x, lastCellPosition.y) + offset;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            scoresContainer.transform.position = screenPos;
        }
    }

    private void HandleLaneFinished(Vector2Int gridPosition)
    {
        if (CurrentTask.Instance.ThereIsPinned())
            deactivateCoroutine = StartCoroutine(DeactivateAfterDelay(5f));
    }

    private void HandleTaskCompleted(Task task)
    {
        if (CurrentTask.Instance.PinnedTask.info.safetyRequirement) scoresContainer.GetChild(0).gameObject.SetActive(false);
        if (CurrentTask.Instance.PinnedTask.info.charmRequirement) scoresContainer.GetChild(1).gameObject.SetActive(false);
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (CurrentTask.Instance.PinnedTask.info.safetyRequirement) scoresContainer.GetChild(0).gameObject.SetActive(false);
        if (CurrentTask.Instance.PinnedTask.info.charmRequirement) scoresContainer.GetChild(1).gameObject.SetActive(false);
        deactivateCoroutine = null;
    }

    // ::::: 
    private void UpdateScoreUI(Task task)
    {
        // Safety
        float safetyUI = task.info.safetyRequirement
            ? (float)task.currentSafetyCount / (float)task.info.minSafetyCount
            : 0f;
        safetyFill.fillAmount = Mathf.Clamp(safetyUI, 0f, 1f);

        // Charm
        float charmUI = task.info.charmRequirement
            ? (float)task.currentCharmCount / (float)task.info.minCharmCount
            : 0f;
        charmFill.fillAmount = Mathf.Clamp(charmUI, 0f, 1f);

        // Flow
        float flowUI = task.info.flowRequirement
            ? task.currentFlowPercentage
            : 0f;
        flowUI = Mathf.Clamp(flowUI, 0f, 1f);
    }
}
