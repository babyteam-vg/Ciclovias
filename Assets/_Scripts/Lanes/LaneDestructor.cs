using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneDestructor : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private AudioManager audioManager;

    private bool isDestroying = false;
    private Vector2Int? lastCellPosition = null;

    public event Action<Vector2Int> OnDestroyStarted;
    public event Action<Vector2Int> OnLaneDestroyed;
    public event Action<Vector2Int> OnDestroyFinished;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        inputManager.OnRightClickDown += StartDestroying;
        inputManager.OnRightClickHold += ContinueDestroying;
        inputManager.OnRightClickUp += StopDestroying;
    }
    private void OnDisable()
    {
        inputManager.OnRightClickDown -= StartDestroying;
        inputManager.OnRightClickHold -= ContinueDestroying;
        inputManager.OnRightClickUp -= StopDestroying;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Mouse Input: Down
    private void StartDestroying(Vector2Int gridPosition)
    {
        if (graph.GetNode(gridPosition) != null)
        {
            isDestroying = true;
            OnDestroyStarted?.Invoke(gridPosition);
            DestroyNodeAndEdges(gridPosition);

            lastCellPosition = gridPosition;
        }
    }

    // ::::: Mouse Input: Hold
    private void ContinueDestroying(Vector2Int gridPosition)
    {
        if (isDestroying &&
            grid.IsAdjacent(lastCellPosition.Value, gridPosition) &&
            laneConstructor.IsInCriticalArea(gridPosition))
        {
            if (lastCellPosition.HasValue && lastCellPosition.Value == gridPosition)
                return; // To Prevent Duplicates

            DestroyNodeAndEdges(gridPosition);

            lastCellPosition = gridPosition;
        }
    }

    // ::::: Mouse Input: Up
    private void StopDestroying(Vector2Int gridPosition)
    {
        graph.CheckAndRemoveNode(gridPosition);
        isDestroying = false;
        lastCellPosition = null;
        OnDestroyFinished?.Invoke(gridPosition); // !
    }

    // ::::: Destruction Waterfall
    private void DestroyNodeAndEdges(Vector2Int gridPosition)
    {
        Node node = graph.GetNode(gridPosition);
        if (node != null)
        {
            int edgeCount = node.neighbors.Count;
            ConstructionMaterial.Instance.AddMaterial(edgeCount); // Add material for each removed edge
            OnLaneDestroyed?.Invoke(gridPosition);
        }

        graph.RemoveNode(gridPosition);
    }
}
