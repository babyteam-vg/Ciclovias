using UnityEngine;
using System;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
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

    // === Methods ===
    private void Update()
    {
        HandleMouseInput();
    }

    // Mouse, RayCast and Events
    private void HandleMouseInput()
    {
        if (mainCamera == null || grid == null) return;

        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 worldPosition = hit.point;
            Vector2Int? gridPosition = grid.GetCellFromWorldPosition(worldPosition);

            if (gridPosition.HasValue)
            {
                Vector2Int validGridPosition = gridPosition.Value;
                if (Input.GetKeyDown(KeyCode.R))
                    Debug.LogWarning(validGridPosition);
                //              Left Click <¬
                if (Input.GetMouseButtonDown(0)) OnLeftClickDown?.Invoke(validGridPosition);
                if (Input.GetMouseButton(0)) OnLeftClickHold?.Invoke(validGridPosition);
                if (Input.GetMouseButtonUp(0)) OnLeftClickUp?.Invoke(validGridPosition);
                //             Right Click <¬
                if (Input.GetMouseButtonDown(1)) OnRightClickDown?.Invoke(validGridPosition);
                if (Input.GetMouseButton(1)) OnRightClickHold?.Invoke(validGridPosition);
                if (Input.GetMouseButtonUp(1)) OnRightClickUp?.Invoke(validGridPosition);
                //           Middlle Click <¬
                if (Input.GetMouseButtonDown(2)) OnMiddleClickDown?.Invoke(validGridPosition);
                if (Input.GetMouseButton(2)) OnMiddleClickHold?.Invoke(validGridPosition);
                if (Input.GetMouseButtonUp(2)) OnMiddleClickUp?.Invoke(validGridPosition);

                List<Cell> adjacentCells = grid.GetAdjacentCells(validGridPosition.x, validGridPosition.y);

                Debug.DrawRay(ray.origin, ray.direction * 1000, Color.blue);
            }
        }
        else Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
    }

    // 2D Cursor to 3D World
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