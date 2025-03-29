using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Grid grid;
    [SerializeField] private InGameMenuManager inGameMenuManager;
    [SerializeField] private TaskDialogManager taskDialogManager;
    [SerializeField] private TutorialDialogManager tutorialDialogManager;

    public event Action<Vector2Int> OnCursorMove;
    public event Action<Vector2Int> NothingDetected;

    public event Action<Vector2Int> OnLeftClickDown;
    public event Action<Vector2Int> OnLeftClickHold;
    public event Action<Vector2Int> OnLeftClickUp;

    public event Action<Vector2Int> OnRightClickDown;
    public event Action<Vector2Int> OnRightClickHold;
    public event Action<Vector2Int> OnRightClickUp;

    public event Action<Vector2Int> OnMiddleClickDown;
    public event Action<Vector2Int> OnMiddleClickHold;
    public event Action<Vector2Int> OnMiddleClickUp;

    public event Action OnHighlightToggleDown;

    private bool isLeftMouseButtonDown = false;
    private bool isRightMouseButtonDown = false;
    private bool isMiddleMouseButtonDown = false;

    private bool isAllowed = true;
    private Vector2Int? lastGridPosition = null;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        inGameMenuManager.MenuOpened += LockInput;
        inGameMenuManager.MenuClosed += UnlockInput;

        taskDialogManager.StrictDialogOpened += LockInput;
        taskDialogManager.StrictDialogClosed += UnlockInput;

        tutorialDialogManager.StrictDialogOpened += LockInput;
        tutorialDialogManager.StrictDialogClosed += UnlockInput;
    }
    private void OnDisable()
    {
        inGameMenuManager.MenuOpened -= LockInput;
        inGameMenuManager.MenuClosed -= UnlockInput;

        taskDialogManager.StrictDialogOpened -= LockInput;
        taskDialogManager.StrictDialogClosed -= UnlockInput;

        tutorialDialogManager.StrictDialogOpened -= LockInput;
        tutorialDialogManager.StrictDialogClosed -= UnlockInput;
    }

    private void Update()
    {
        if (isAllowed)
        {
            HandleMouseInput();
            HandleHighlightInput();
        }

        if (!GameStateManager.Instance.InBrowser) HandleEscapeInput();
    }

    // :::::::::: PUBLIC METHODS ::::::::::
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

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Mouse, RayCast and Events
    private void HandleMouseInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) // Not in UI
        {
            NothingDetected?.Invoke(new Vector2Int());
            return;
        }

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
                if (lastGridPosition != validGridPosition)
                {
                    OnCursorMove?.Invoke(validGridPosition);
                    lastGridPosition = validGridPosition;
                }

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
            else NothingDetected?.Invoke(new Vector2Int());
        }

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

    // ::::: ESC
    private void HandleEscapeInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isAllowed) inGameMenuManager.CloseMenu();
            else inGameMenuManager.OnPauseMenuPress();
        }
    }

    // ::::: Highlight the Cells
    private void HandleHighlightInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            OnHighlightToggleDown?.Invoke();
    }

    // ::::: Menu? Allowing
    private void LockInput() { isAllowed = false; }
    private void UnlockInput() { isAllowed = true; }
}