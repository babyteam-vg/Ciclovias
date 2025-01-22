using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneDestructor : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private InputManager inputManager;

    private bool isDestroying = false;
    private Vector2Int? lastCellPosition = null;

    public event Action<Vector2Int> OnLaneDestroyed;

    // === Methods ===
    private void Start()
    {
        inputManager.OnRightClickDown += StartDestroying;
        inputManager.OnRightClickHold += ContinueDestroying;
        inputManager.OnRightClickUp += StopDestroying;
    }

    // Mouse Input: Down
    private void StartDestroying(Vector2Int gridPosition)
    {
        if (graph.GetNode(gridPosition) != null)
        {
            isDestroying = true;
            lastCellPosition = gridPosition;
        }
    }

    // Mouse Input: Hold
    private void ContinueDestroying(Vector2Int gridPosition)
    {
        if (isDestroying && IsAdjacent(lastCellPosition.Value, gridPosition))
        {
            if (lastCellPosition.HasValue && IsInCriticalArea(gridPosition))
            {
                if (graph.AreConnected(lastCellPosition.Value, gridPosition))
                {
                    graph.RemoveEdge(lastCellPosition.Value, gridPosition);
                    OnLaneDestroyed?.Invoke(gridPosition); // Notify Lane Destruction
                }

                CheckAndRemoveNode(lastCellPosition.Value);
                CheckAndRemoveNode(gridPosition);

                lastCellPosition = gridPosition;
            }
        }
    }

    // Mouse Input: Up
    private void StopDestroying(Vector2Int gridPosition)
    {
        isDestroying = false;
        lastCellPosition = null;
    }

    // Critical Area
    private bool IsInCriticalArea(Vector2Int gridPosition)
    { //                       Getting 3D World Position <¬
        Vector3 cursorWorldPosition = inputManager.GetCursorWorldPosition();
        Vector3 cellWorldPosition = grid.GetWorldPositionFromCell(gridPosition.x, gridPosition.y);

        Vector3 criticalCenter = cellWorldPosition + new Vector3(
            Mathf.Cos(Mathf.PI / 4) * grid.cellSize / 2,
            0,
            Mathf.Sin(Mathf.PI / 4) * grid.cellSize / 2
        );

        float criticalRadius = grid.cellSize / 2;

        Vector2 delta = new Vector2(
            cursorWorldPosition.x - criticalCenter.x,
            cursorWorldPosition.z - criticalCenter.z
        );

        return delta.sqrMagnitude <= criticalRadius * criticalRadius;
    }

    // Only Remove a Lonely Node
    private void CheckAndRemoveNode(Vector2Int position)
    {
        Node node = graph.GetNode(position);

        if (node != null && node.neighbors.Count == 0)
            graph.RemoveNode(position);
    }

    // Check 8 Directions
    private bool IsAdjacent(Vector2Int current, Vector2Int target)
    {
        int dx = Mathf.Abs(current.x - target.x);
        int dy = Mathf.Abs(current.y - target.y);
        return (dx <= 1 && dy <= 1);
    }

    private void OnDestroy()
    {
        inputManager.OnRightClickDown -= StartDestroying;
        inputManager.OnRightClickHold -= ContinueDestroying;
        inputManager.OnRightClickUp -= StopDestroying;
    }
}
