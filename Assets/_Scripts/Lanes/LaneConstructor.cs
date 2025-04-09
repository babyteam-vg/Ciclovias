using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaneConstructor : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private InGameMenuManager inGameMenuManager;

    private bool isAllowed = true;
    private bool isBuilding = false;
    private Vector2Int? lastCellPosition = null;

    public event Action<Vector2Int> OnBuildStarted;
    public event Action<Vector2Int> OnLaneBuilt;
    public event Action<Vector2Int> OnBuildFinished;

    public event Action BuiltOnZebra;
    public event Action BuiltOnDangerous;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        inputManager.OnLeftClickDown += StartBuilding;
        inputManager.OnLeftClickHold += ContinueBuilding;
        inputManager.OnLeftClickUp += StopBuilding;

        tutorialManager.TutorialSectionPresentationStarted += BlockDestroying;
        tutorialManager.TutorialSectionPresentationDone += UnblockDestroying;

        inGameMenuManager.MenuOpened += BlockDestroying;
        inGameMenuManager.MenuClosed += UnblockDestroying;
    }
    private void OnDisable()
    {
        inputManager.OnLeftClickDown -= StartBuilding;
        inputManager.OnLeftClickHold -= ContinueBuilding;
        inputManager.OnLeftClickUp -= StopBuilding;

        tutorialManager.TutorialSectionPresentationStarted += BlockDestroying;
        tutorialManager.TutorialSectionPresentationDone += UnblockDestroying;

        inGameMenuManager.MenuOpened += BlockDestroying;
        inGameMenuManager.MenuClosed += UnblockDestroying;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Mouse Input: Down
    private void StartBuilding(Vector2Int gridPosition)
    {
        int neighbors = graph.GetNeighborsCount(gridPosition);

        if (isAllowed && neighbors < 3) 
        {
            isBuilding = true;
            OnBuildStarted?.Invoke(gridPosition); // !
            lastCellPosition = gridPosition;
        }
    }

    // ::::: Mouse Input: Hold
    private void ContinueBuilding(Vector2Int gridPosition)
    {
        Node node = graph.GetNode(gridPosition);
        if (node == null)
            return;

        if (isAllowed && isBuilding && lastCellPosition.HasValue        // Not in Menu & Started Building
            && grid.IsAdjacent(lastCellPosition.Value, gridPosition)    // One of the 8 Adjacent Cells
            && IsInCriticalArea(gridPosition))                          // Input Response Improvement
        {
            if (lastCellPosition.Value == gridPosition)
                return; // To Prevent Duplicates

            Node lastNode = graph.GetNode(lastCellPosition.Value);
            if (lastNode.blockedPositions.Contains(gridPosition))
                return;

            if (graph.AreNeighbors(lastCellPosition.Value, gridPosition))
                return; // Already Connected

            if (!MaterialManager.Instance.ConsumeMaterial(1))
                return; // Not Enough Material

            // Check Intersection
            List<Vector2Int> lastNeighbors = graph.GetNeighborsPos(lastCellPosition.Value);
            foreach (Vector2Int lastNeighbor in lastNeighbors)
                if (!graph.IsCollinear(lastNeighbor, lastCellPosition.Value, gridPosition))
                    graph.NewIntersection(lastNeighbor, lastCellPosition.Value, node, gridPosition);

            graph.AddEdge(lastCellPosition.Value, gridPosition); // Connect Nodes
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxs[0]);

            OnLaneBuilt?.Invoke(gridPosition); // !
            lastCellPosition = gridPosition;

            // Tips Events
            CellContent content = grid.GetCellContent(gridPosition.x, gridPosition.y);
            switch (content)
            {
                case CellContent.Zebra:
                    BuiltOnZebra?.Invoke();
                    break;

                case CellContent.Dangerous:
                    BuiltOnDangerous?.Invoke();
                    break;
            }                
        }
    }

    // ::::: Mouse Input: Up
    private void StopBuilding(Vector2Int gridPosition)
    {
        isBuilding = false;
        OnBuildFinished?.Invoke(gridPosition); // !
        lastCellPosition = null;
    }

    // ::::: Critical Areas
    private bool IsInCriticalArea(Vector2Int gridPosition)
    {
        if (!lastCellPosition.HasValue)
            return false;

        Vector3 cursorWorldPosition = inputManager.GetCursorWorldPosition();
        Vector3 cellWorldPosition = grid.GetWorldPositionFromCellCentered(gridPosition.x, gridPosition.y);

        Vector2Int direction = gridPosition - lastCellPosition.Value;
        bool isDiagonal = Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1;

        if (isDiagonal) // Diagonal
        {
            float halfCellSize = grid.GetCellSize() / 2;

            float minX = cellWorldPosition.x - halfCellSize;
            float maxX = cellWorldPosition.x + halfCellSize;
            float minZ = cellWorldPosition.z - halfCellSize;
            float maxZ = cellWorldPosition.z + halfCellSize;

            bool isInside = cursorWorldPosition.x >= minX && cursorWorldPosition.x <= maxX &&
                            cursorWorldPosition.z >= minZ && cursorWorldPosition.z <= maxZ;

            return isInside;
        }
        else // Directly Adjacent
        {
            float halfCellSize = grid.GetCellSize() / 2;
            float quarterCellSize = grid.GetCellSize() / 4;

            float minX, maxX, minZ, maxZ;

            if (direction.x != 0)
            {
                // Horizontal
                minX = cellWorldPosition.x + (direction.x > 0 ? 0 : -halfCellSize);
                maxX = cellWorldPosition.x + (direction.x > 0 ? halfCellSize : 0);
                minZ = cellWorldPosition.z - halfCellSize;
                maxZ = cellWorldPosition.z + halfCellSize;
            }
            else if (direction.y != 0)
            {
                // Vertical
                minX = cellWorldPosition.x - halfCellSize;
                maxX = cellWorldPosition.x + halfCellSize;
                minZ = cellWorldPosition.z + (direction.y > 0 ? 0 : -halfCellSize);
                maxZ = cellWorldPosition.z + (direction.y > 0 ? halfCellSize : 0);
            }
            else
                return false;

            bool isInside = cursorWorldPosition.x >= minX && cursorWorldPosition.x <= maxX &&
                            cursorWorldPosition.z >= minZ && cursorWorldPosition.z <= maxZ;

            return isInside;
        }
    }

    // ::::: Menu? Allowing
    private void BlockDestroying() { isAllowed = false; }
    private void UnblockDestroying() { isAllowed = true; }
}
