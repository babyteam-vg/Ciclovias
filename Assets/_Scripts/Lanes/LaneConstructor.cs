using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneConstructor : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private AudioManager audioManager;

    private bool isBuilding = false;
    private Vector2Int? lastCellPosition = null;

    public event Action<Vector2Int> OnBuildStarted;
    public event Action<Vector2Int> OnLaneBuilt;
    public event Action<Vector2Int> OnBuildFinished;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        inputManager.OnLeftClickDown += StartBuilding;
        inputManager.OnLeftClickHold += ContinueBuilding;
        inputManager.OnLeftClickUp += StopBuilding;
    }
    private void OnDisable()
    {
        inputManager.OnLeftClickDown -= StartBuilding;
        inputManager.OnLeftClickHold -= ContinueBuilding;
        inputManager.OnLeftClickUp -= StopBuilding;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Critical Area
    public bool IsInCriticalArea(Vector2Int gridPosition)
    { //                       Getting 3D World Position <¬
        Vector3 cursorWorldPosition = inputManager.GetCursorWorldPosition();
        Vector3 cellWorldPosition = grid.GetWorldPositionFromCell(gridPosition.x, gridPosition.y);

        Vector3 criticalCenter = cellWorldPosition + new Vector3(
            Mathf.Cos(Mathf.PI / 4) * grid.GetCellSize() / 2,
            0,
            Mathf.Sin(Mathf.PI / 4) * grid.GetCellSize() / 2
        );

        float criticalRadius = grid.GetCellSize() / 2;

        Vector2 delta = new Vector2(
            cursorWorldPosition.x - criticalCenter.x,
            cursorWorldPosition.z - criticalCenter.z
        );

        return delta.sqrMagnitude <= criticalRadius * criticalRadius;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Mouse Input: Down
    private void StartBuilding(Vector2Int gridPosition)
    {
        if (grid.GetCellBuildable(gridPosition.x, gridPosition.y))
        {
            if (graph.GetNode(gridPosition) == null)
                graph.AddNode(gridPosition, grid.EdgeToMid(gridPosition)); // Add Node

            isBuilding = true;
            OnBuildStarted?.Invoke(gridPosition); // !
            lastCellPosition = gridPosition;
        }
    }

    // ::::: Mouse Input: Hold
    private void ContinueBuilding(Vector2Int gridPosition)
    {
        if (isBuilding &&
            grid.IsAdjacent(lastCellPosition.Value, gridPosition) &&
            grid.GetCellBuildable(gridPosition.x, gridPosition.y) &&
            IsInCriticalArea(gridPosition))
        {
            if (lastCellPosition.HasValue && lastCellPosition.Value == gridPosition)
                return; // To Prevent Duplicates

            if (ConstructionMaterial.Instance.material > 0)
            {
                if (graph.GetNode(gridPosition) == null)
                    graph.AddNode(gridPosition, grid.EdgeToMid(gridPosition)); // Adds the Node

                if (lastCellPosition.HasValue && !graph.AreConnected(lastCellPosition.Value, gridPosition))
                {
                    graph.AddEdge(lastCellPosition.Value, gridPosition); // Connect Nodes
                    ConstructionMaterial.Instance.ConsumeMaterial(1); // Construction Material: -1
                    audioManager.PlaySFX(audioManager.build);

                    OnLaneBuilt?.Invoke(gridPosition); // !
                    lastCellPosition = gridPosition;
                }
            }
        }
    }

    // ::::: Mouse Input: Up
    private void StopBuilding(Vector2Int gridPosition)
    {
        graph.CheckAndRemoveNode(gridPosition);
        isBuilding = false;
        lastCellPosition = null;
        OnBuildFinished?.Invoke(gridPosition); // !
    }
}
