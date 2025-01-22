using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneConstructor : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private InputManager inputManager;

    private bool isBuilding = false;
    private Vector2Int? lastCellPosition = null;

    public event Action<Vector2Int> OnLaneBuilt;

    // === Methods ===
    private void OnEnable()
    {
        inputManager.OnLeftClickDown += StartBuilding;
        inputManager.OnLeftClickHold += ContinueBuilding;
        inputManager.OnLeftClickUp += ConfirmConstruction;
    }
    private void OnDisable()
    {
        inputManager.OnLeftClickDown -= StartBuilding;
        inputManager.OnLeftClickHold -= ContinueBuilding;
        inputManager.OnLeftClickUp -= ConfirmConstruction;
    }

    // Mouse Input: Down
    private void StartBuilding(Vector2Int gridPosition)
    {
        if (grid.GetCellBuildable(gridPosition.x, gridPosition.y))
        {
            AddNodeAndConnections(gridPosition);
            isBuilding = true;
            lastCellPosition = gridPosition;
        }
    }

    // Mouse Input: Hold
    private void ContinueBuilding(Vector2Int gridPosition)
    {
        if (isBuilding && IsAdjacent(lastCellPosition.Value, gridPosition) && grid.GetCellBuildable(gridPosition.x, gridPosition.y))
        {
            if (lastCellPosition.HasValue && IsInCriticalArea(gridPosition))
            {
                AddNodeAndConnections(gridPosition);
                lastCellPosition = gridPosition;
            }
        }
    }

    // Mouse Input: Up
    private void ConfirmConstruction(Vector2Int gridPosition)
    {
        CheckAndRemoveNode(gridPosition);
        isBuilding = false;
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

    // Add a Node and Its Connections
    private void AddNodeAndConnections(Vector2Int gridPosition)
    {
        if (graph.GetNode(gridPosition) == null)
            graph.AddNode(gridPosition, grid.EdgeToMid(gridPosition));

        if (lastCellPosition.HasValue)
        {
            if (!graph.AreConnected(lastCellPosition.Value, gridPosition))
            {
                graph.AddEdge(lastCellPosition.Value, gridPosition);
                OnLaneBuilt?.Invoke(gridPosition); // Notify Lane Construction
            }
        }
    }

    // Only Remove a Lonely Node
    private void CheckAndRemoveNode(Vector2Int position)
    {
        Node node = graph.GetNode(position);

        if (node != null && node.neighbors.Count == 0)
            graph.RemoveNode(position);
    }

    // Check Adjacency in 8 Directions
    private bool IsAdjacent(Vector2Int current, Vector2Int target)
    {
        int dx = Mathf.Abs(current.x - target.x);
        int dy = Mathf.Abs(current.y - target.y);
        return (dx <= 1 && dy <= 1);
    }
}
