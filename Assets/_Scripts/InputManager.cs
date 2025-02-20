using UnityEngine;
using System;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Grid grid;

    public event Action<Vector2Int> OnLeftClickDown;
    public event Action<Vector2Int> OnLeftClickHold;
    public event Action<Vector2Int> OnLeftClickUp;

    public event Action<Vector2Int> OnRightClickDown;
    public event Action<Vector2Int> OnRightClickHold;
    public event Action<Vector2Int> OnRightClickUp;

    public event Action<Vector2Int> OnMiddleClickDown;
    public event Action<Vector2Int> OnMiddleClickHold;
    public event Action<Vector2Int> OnMiddleClickUp;

    private bool isLeftMouseButtonDown = false;
    private bool isRightMouseButtonDown = false;
    private bool isMiddleMouseButtonDown = false;

    private Vector2Int? lastGridPosition = null;

    // :::::::::: Methods ::::::::::
    private void Update()
    {
        HandleMouseInput();
    }

    // ::::: Mouse, RayCast and Events
    private void HandleMouseInput()
    {
        if (mainCamera == null || grid == null) return;

        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        Vector2Int? gridPosition = null;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 worldPosition = hit.point;
            gridPosition = grid.GetCellFromWorldPosition(worldPosition);

            if (Input.GetKeyDown(KeyCode.R)) // Debug Purposes
                Debug.LogWarning($"{gridPosition}");

            if (gridPosition.HasValue)
            {
                Vector2Int validGridPosition = gridPosition.Value;
                lastGridPosition = validGridPosition;

                // Left Click
                if (Input.GetMouseButtonDown(0))
                {
                    isLeftMouseButtonDown = true;
                    OnLeftClickDown?.Invoke(validGridPosition);
                }
                if (Input.GetMouseButton(0)) OnLeftClickHold?.Invoke(validGridPosition);

                // Right Click
                if (Input.GetMouseButtonDown(1))
                {
                    isRightMouseButtonDown = true;
                    OnRightClickDown?.Invoke(validGridPosition);
                }
                if (Input.GetMouseButton(1)) OnRightClickHold?.Invoke(validGridPosition);

                // Middle Click
                if (Input.GetMouseButtonDown(2))
                {
                    isMiddleMouseButtonDown = true;
                    OnMiddleClickDown?.Invoke(validGridPosition);
                }
                if (Input.GetMouseButton(2)) OnMiddleClickHold?.Invoke(validGridPosition);

                Debug.DrawRay(ray.origin, ray.direction * 1000, Color.blue);
            }
        }
        else
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);

        // Handle Mouse Button Up Events Globally
        if (isLeftMouseButtonDown && Input.GetMouseButtonUp(0))
        {
            isLeftMouseButtonDown = false;
            OnLeftClickUp?.Invoke(lastGridPosition ?? Vector2Int.zero);
        }

        if (isRightMouseButtonDown && Input.GetMouseButtonUp(1))
        {
            isRightMouseButtonDown = false;
            OnRightClickUp?.Invoke(lastGridPosition ?? Vector2Int.zero);
        }

        if (isMiddleMouseButtonDown && Input.GetMouseButtonUp(2))
        {
            isMiddleMouseButtonDown = false;
            OnMiddleClickUp?.Invoke(lastGridPosition ?? Vector2Int.zero);
        }
    }

    // ::::: 2D Cursor to 3D World
    public Vector3 GetCursorWorldPosition()
    {
        if (mainCamera == null) return Vector3.zero;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.point;
        //    Default <¬
        return Vector3.zero;
    }
}