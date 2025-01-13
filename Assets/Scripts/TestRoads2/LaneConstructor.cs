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
    private Vector2Int? lastNodePosition = null;

    private HashSet<Vector2Int> provisionalNodes = new HashSet<Vector2Int>();
    private List<(Vector2Int, Vector2Int)> provisionalConnections = new List<(Vector2Int, Vector2Int)>();

    // === Methods ===
    private void Start()
    {
        inputManager.OnLeftClickDown += StartBuilding;
        inputManager.OnLeftClickHold += ContinueBuilding;
        inputManager.OnLeftClickUp += ConfirmConstruction;
    }

    // Mouse Input: Down
    private void StartBuilding(Vector2Int gridPosition)
    {
        if (grid.GetCellBuildable(gridPosition.x, gridPosition.y) == true)
        {
            AddProvisionalNode(gridPosition);
            isBuilding = true;
            lastNodePosition = gridPosition;
        }
    }

    // Mouse Input: Hold
    private void ContinueBuilding(Vector2Int gridPosition)
    {
        if (isBuilding == true && grid.GetCellBuildable(gridPosition.x, gridPosition.y) == true)
        {
            if (lastNodePosition.HasValue && IsAdjacent(lastNodePosition.Value, gridPosition))
            {
                AddProvisionalNode(gridPosition);
                AddProvisionalConnection(lastNodePosition.Value, gridPosition);
                lastNodePosition = gridPosition;
            }
        }
    }

    // Mouse Input: Up
    private void ConfirmConstruction(Vector2Int gridPosition)
    {
        if (!isBuilding) return;

        foreach (var node in provisionalNodes)
        {
            Vector2 centeredPosition = grid.EdgeToMid(node);
            graph.AddNode(centeredPosition);
        }

        foreach (var connection in provisionalConnections)
            if (connection.Item1 != connection.Item2)
            {
                Vector2 centeredPosition1 = grid.EdgeToMid(connection.Item1);
                Vector2 centeredPosition2 = grid.EdgeToMid(connection.Item2);

                graph.AddEdge(centeredPosition1, centeredPosition2);
            }

        isBuilding = false;
        lastNodePosition = null;
        provisionalNodes.Clear();
        provisionalConnections.Clear();
    }

    // Provisional Lane Build
    private void AddProvisionalNode(Vector2Int gridPosition)
    {
        if (!provisionalNodes.Contains(gridPosition))
            provisionalNodes.Add(gridPosition);
    }
    private void AddProvisionalConnection(Vector2Int positionA, Vector2Int positionB)
    {
        if (positionA != positionB && !provisionalConnections.Contains((positionA, positionB)))
            provisionalConnections.Add((positionA, positionB));
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
        inputManager.OnLeftClickDown -= StartBuilding;
        inputManager.OnLeftClickHold -= ContinueBuilding;
        inputManager.OnLeftClickUp -= ConfirmConstruction;
    }
}
