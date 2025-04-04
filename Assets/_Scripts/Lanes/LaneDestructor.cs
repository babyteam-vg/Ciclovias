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
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private InGameMenuManager inGameMenuManager;

    private bool isAllowed = true;
    private bool isDestroying = false;
    private Vector2Int? lastCellPosition = null;
    private List<Vector2Int> lastNeighbors = new List<Vector2Int>();
    private Coroutine destroySFXCoroutine;

    public event Action<Vector2Int> OnDestroyStarted;
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

        if (node != null)
        {
            if (isAllowed                   // Not in Menu
            && !node.indestructible)    // Not Part of a Sealed Task
            {
                isDestroying = true;
                OnDestroyStarted?.Invoke(gridPosition);

                lastNeighbors = graph.GetNeighborsPos(gridPosition);
                DestroyNodeAndEdges(gridPosition);
                lastCellPosition = gridPosition;
            }
            else if (node.indestructible)
                OnTryDestroySealed?.Invoke();
        }
    }

    // ::::: Mouse Input: Hold
    private void ContinueDestroying(Vector2Int gridPosition)
    {
        Node node = graph.GetNode(gridPosition);

        if (node != null) // Node's Existence
        {
            if (!node.indestructible && node.neighbors.Count < 2)
            {
                if (isAllowed && isDestroying && lastCellPosition.HasValue) // Flags
                {
                    if (lastCellPosition.Value == gridPosition) return; // Prevent Duplicates

                    if (grid.IsAdjacent(lastCellPosition.Value, gridPosition) && IsInCriticalArea(gridPosition)) // Adjacency
                    {
                        if (lastNeighbors.Contains(gridPosition)) // Continued Destruction w/o Jumps
                        {
                            lastNeighbors = graph.GetNeighborsPos(gridPosition);
                            DestroyNodeAndEdges(gridPosition);
                            lastCellPosition = gridPosition;
                        }
                    }
                }
            }
            else if (node.indestructible)
                OnTryDestroySealed?.Invoke();
        }
    }

    // ::::: Mouse Input: Up
    private void StopDestroying(Vector2Int gridPosition)
    {
        graph.CheckAndRemoveNode(gridPosition);
        isDestroying = false;
        lastCellPosition = null;
        lastNeighbors = null;
        AudioManager.Instance.ResetSFXPitch();
        OnDestroyFinished?.Invoke(gridPosition); // !
    }

    // ::::: Destruction Waterfall
    private void DestroyNodeAndEdges(Vector2Int gridPosition)
    {
        Node node = graph.GetNode(gridPosition);
        if (node != null)
        {
            int edgeCount = node.neighbors.Count;
            MaterialManager.Instance.AddMaterial(edgeCount);
            graph.RemoveNode(gridPosition);
            if (destroySFXCoroutine == null)
                destroySFXCoroutine = StartCoroutine(PlayDstroySFX());
            OnLaneDestroyed?.Invoke(gridPosition);
        }
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
