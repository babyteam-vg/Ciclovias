using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneConstructor : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private InputManager inputManager;

    [SerializeField] private AudioManager audioManager;

    private bool isBuilding = false;
    private Vector2Int? lastCellPosition = null;

    public event Action<Vector2Int> OnLaneStarted;
    public event Action<Vector2Int> OnLaneBuilt;
    public event Action<Vector2Int> OnLaneFinished;
    public event Action<Vector2Int> LonelyNodeRemoved;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        inputManager.OnLeftClickDown += StartBuilding;
        inputManager.OnLeftClickHold += ContinueBuilding;
        inputManager.OnLeftClickUp += EndBuilding;
    }
    private void OnDisable()
    {
        inputManager.OnLeftClickDown -= StartBuilding;
        inputManager.OnLeftClickHold -= ContinueBuilding;
        inputManager.OnLeftClickUp -= EndBuilding;
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
            OnLaneStarted?.Invoke(gridPosition); // Notify Lane Start
            lastCellPosition = gridPosition;
        }
    }

    // ::::: Mouse Input: Hold
    private void ContinueBuilding(Vector2Int gridPosition)
    {
        if (isBuilding &&
            IsAdjacent(lastCellPosition.Value, gridPosition) &&
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

                    OnLaneBuilt?.Invoke(gridPosition); // Notify Lane Construction
                    audioManager.PlaySFX(audioManager.build);

                    lastCellPosition = gridPosition;
                }
            }
        }
    }

    // ::::: Mouse Input: Up
    private void EndBuilding(Vector2Int gridPosition)
    {
        CheckAndRemoveNode(gridPosition);
        isBuilding = false;
        lastCellPosition = null;
        OnLaneFinished?.Invoke(gridPosition);
    }

    // :::::::::: SUPPORT METHODS ::::::::::
    // ::::: Critical Area
    private bool IsInCriticalArea(Vector2Int gridPosition)
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

    // ::::: Remove a Lonely Node
    private void CheckAndRemoveNode(Vector2Int position)
    {
        Node node = graph.GetNode(position);

        if (node != null && node.neighbors.Count == 0)
        {
            graph.RemoveNode(position);
            LonelyNodeRemoved?.Invoke(position);
        }   
    }

    // ::::: Check Adjacency in 8 Directions
    private bool IsAdjacent(Vector2Int current, Vector2Int target)
    {
        int dx = Mathf.Abs(current.x - target.x);
        int dy = Mathf.Abs(current.y - target.y);
        return (dx <= 1 && dy <= 1);
    }
}
