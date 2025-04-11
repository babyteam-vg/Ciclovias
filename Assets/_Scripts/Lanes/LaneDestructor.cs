using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LaneDestructor : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private InGameMenuManager inGameMenuManager;

    private bool isAllowed = true;
    private bool isDestroying = false;
    private Vector2Int? lastCellPosition = null;
    private List<Vector2Int> lastNeighbors = new List<Vector2Int>();
    private Coroutine destroySFXCoroutine;

    public event Action<Vector2Int> OnLaneDestroyed;
    public event Action<Vector2Int> OnDestroyFinished;

    public event Action OnTryDestroySealed;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        inputManager.OnRightClickDown += StartDestroying;
        inputManager.OnRightClickHold += ContinueDestroying;
        inputManager.OnRightClickUp += StopDestroying;

        tutorialManager.TutorialSectionPresentationStarted += BlockDestroying;
        tutorialManager.TutorialSectionPresentationDone += UnblockDestroying;

        inGameMenuManager.MenuOpened += BlockDestroying;
        inGameMenuManager.MenuClosed += UnblockDestroying;
    }
    private void OnDisable()
    {
        inputManager.OnRightClickDown -= StartDestroying;
        inputManager.OnRightClickHold -= ContinueDestroying;
        inputManager.OnRightClickUp -= StopDestroying;

        tutorialManager.TutorialSectionPresentationStarted -= BlockDestroying;
        tutorialManager.TutorialSectionPresentationDone -= UnblockDestroying;

        inGameMenuManager.MenuOpened -= BlockDestroying;
        inGameMenuManager.MenuClosed -= UnblockDestroying;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Mouse Input: Down
    private void StartDestroying(Vector2Int gridPosition)
    {
        Node node = graph.GetNode(gridPosition);
        if (node == null)
            return;

        if (isAllowed               // Not in Menu
        && !node.indestructible)    // Not Part of a Sealed Task
        {
            isDestroying = true;
            lastNeighbors = graph.GetNeighborsPos(gridPosition);

            List<Vector2Int> neighbors = graph.GetNeighborsPos(gridPosition);
            foreach (Vector2Int neighbor in neighbors)
                DestroyWaterfall(node, gridPosition, neighbors);
        }
        else if (node.indestructible)
            OnTryDestroySealed?.Invoke();
    }

    // ::::: Mouse Input: Hold
    private void ContinueDestroying(Vector2Int gridPosition)
    {
        Node node = graph.GetNode(gridPosition);
        if (node == null)
            return;

        if (node.indestructible)
        {
            OnTryDestroySealed?.Invoke();
            return;
        }

        if (isAllowed && isDestroying && lastCellPosition.HasValue
            && grid.IsAdjacent(lastCellPosition.Value, gridPosition)
            && IsInCriticalArea(gridPosition)) // Flags
        {
            if (lastCellPosition.Value == gridPosition)
                return; // Prevent Duplicates

            if (!lastNeighbors.Contains(gridPosition)) // Continued Destruction w/o Jumps
                return;

            List<Vector2Int> neighbors = graph.GetNeighborsPos(gridPosition);
            DestroyWaterfall(node, gridPosition, neighbors);
        }
    }

    // ::::: Mouse Input: Up
    private void StopDestroying(Vector2Int gridPosition)
    {
        isDestroying = false;
        AudioManager.Instance.ResetSFXPitch();
        OnDestroyFinished?.Invoke(gridPosition); // !
        lastCellPosition = null;
    }

    // ::::: Destruction Waterfall
    private void DestroyWaterfall(Node node, Vector2Int gridPosition, List<Vector2Int> neighbors)
    {
        lastNeighbors = graph.GetNeighborsPos(gridPosition);

        // Check Intersection
        foreach (Vector2Int neighbor in neighbors)
        {
            graph.RemoveEdge(gridPosition, neighbor);

            List<Vector2Int> neighbors2 = graph.GetNeighborsPos(neighbor);
            foreach (Vector2Int neighbor2 in neighbors2)
            {
                Node node2 = graph.GetNode(neighbor2);
                if (node2 != null)
                {
                    if (node2.intersectionEdges.Contains(gridPosition))
                        graph.RemoveIntersection(node, gridPosition, neighbor, node2, neighbor2);
                }
            }
        }

        MaterialManager.Instance.AddMaterial(1);
        if (destroySFXCoroutine == null)
            destroySFXCoroutine = StartCoroutine(PlayDstroySFX());

        OnLaneDestroyed?.Invoke(gridPosition);
        lastCellPosition = gridPosition;
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

    private IEnumerator PlayDstroySFX()
    {
        AudioManager.Instance.SetSFXPitch(UnityEngine.Random.Range(0.98f, 1.02f));
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxs[5]);
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.15f, 0.2f));
        destroySFXCoroutine = null;
    }

    // ::::: Menu? Allowing
    private void BlockDestroying() { isAllowed = false; }
    private void UnblockDestroying() { isAllowed = true; }
}
