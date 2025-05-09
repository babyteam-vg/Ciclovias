using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class CellBorderHighlighter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject plane;
    [SerializeField] private CurrentTask currentTask;
    [SerializeField] private TutorialManager tutorialManager;

    private RendererUtility rendererUtility;

    [Header("Varibles")]
    [SerializeField] private Material material;

    private List<GameObject> highlights = new List<GameObject>();

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        rendererUtility = new RendererUtility();
    }

    private void OnEnable()
    {
        currentTask.TaskPinned += HighlightCompoundCells;
        currentTask.TaskUnpinned += ClearHighlight;

        tutorialManager.TutorialSectionReadyToCompleteStarted += HighlightTutorialCells;
        tutorialManager.TutorialSectionCompleted += ClearHighlight;
        tutorialManager.TutorialCompleted += ClearHighlight;
    }
    private void OnDisable()
    {
        currentTask.TaskPinned -= HighlightCompoundCells;
        currentTask.TaskUnpinned -= ClearHighlight;

        tutorialManager.TutorialSectionReadyToCompleteStarted -= HighlightTutorialCells;
        tutorialManager.TutorialSectionCompleted -= ClearHighlight;
        tutorialManager.TutorialCompleted -= ClearHighlight;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: 
    private void HighlightCompoundCells(Task task)
    {
        ClearHighlight();

        foreach (Vector2Int pos in task.from.info.surroundings)
        {
            Cell cell = grid.GetCell(pos.x, pos.y);
            if (cell == null || !cell.GetBuildable()) continue;
            CreateHighlightCorners(new Vector2Int(cell.x, cell.y), material);
        }

        foreach (Vector2Int pos in task.to.info.surroundings)
        {
            Cell cell = grid.GetCell(pos.x, pos.y);
            if (cell == null || !cell.GetBuildable()) continue;
            CreateHighlightCorners(new Vector2Int(cell.x, cell.y), material);
        }

        if (task.info.flavorDetails.flavorType == FlavorType.Visit
            || task.info.flavorDetails.flavorType == FlavorType.Avoid)
            foreach (Vector2Int pos in task.info.flavorDetails.compound.surroundings)
            {
                Cell cell = grid.GetCell(pos.x, pos.y);
                if (cell == null || !cell.GetBuildable()) continue;
                CreateHighlightCorners(new Vector2Int(cell.x, cell.y), material, 0.25f);
            }
    }

    private void HighlightTutorialCells(TutorialSection section)
    {
        Cell start = grid.GetCell(section.start.x, section.start.y);
        if (start == null || !start.GetBuildable()) return;
        CreateHighlightCorners(new Vector2Int(start.x, start.y), material);

        Cell end = grid.GetCell(section.end.x, section.end.y);
        if (end == null || !end.GetBuildable()) return;
        CreateHighlightCorners(new Vector2Int(end.x, end.y), material);
    }

    // ::::: Clear All teh Highlights
    private void ClearHighlight()
    {
        foreach (GameObject highlight in highlights)
            Destroy(highlight);

        highlights.Clear();
    }

    // ::::: 
    private void CreateHighlightCorners(Vector2Int gridPosition, Material material, float thickness = 0.75f, float cornerSize = 0.25f)
    {
        Vector3 worldPosition = grid.GetWorldPositionFromCellCentered(gridPosition.x, gridPosition.y);
        float cellSize = grid.GetCellSize();
        cornerSize *= cellSize;
        thickness *= cornerSize;

        float elevation = rendererUtility.GetMaxElevationAtPoint(worldPosition, plane) + 0.0001f;
        Vector3[] cornerOffsets = new Vector3[]
        {
            new Vector3(-cellSize / 2, elevation, -cellSize / 2), // Bottom-Left
            new Vector3(cellSize / 2, elevation, -cellSize / 2),  // Bottom-Right
            new Vector3(-cellSize / 2, elevation, cellSize / 2),  // Top-Left
            new Vector3(cellSize / 2, elevation, cellSize / 2)    // Top-Right
        };

        foreach (Vector3 offset in cornerOffsets)
        {
            // Horizontal Part of the "L"
            GameObject horizontalQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            horizontalQuad.transform.position = worldPosition + offset;
            horizontalQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
            horizontalQuad.transform.localScale = new Vector3(cornerSize, thickness, 1);
            horizontalQuad.GetComponent<Renderer>().material = material;

            // Vertical Part of the "L"
            GameObject verticalQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            verticalQuad.transform.position = worldPosition + offset;
            verticalQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
            verticalQuad.transform.localScale = new Vector3(thickness, cornerSize, 1);
            verticalQuad.GetComponent<Renderer>().material = material;

            // Small Square to Unify the "L" Parts
            GameObject cornerQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cornerQuad.transform.position = worldPosition + offset;
            cornerQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
            cornerQuad.transform.localScale = new Vector3(thickness, thickness, 1);
            cornerQuad.GetComponent<Renderer>().material = material;

            if (offset.x < 0 && offset.z < 0) // Bottom-Left
            {
                horizontalQuad.transform.position += new Vector3(cornerSize / 2, 0, 0);
                verticalQuad.transform.position += new Vector3(0, 0, cornerSize / 2);
            }
            else if (offset.x > 0 && offset.z < 0) // Bottom-Right
            {
                horizontalQuad.transform.position += new Vector3(-cornerSize / 2, 0, 0);
                verticalQuad.transform.position += new Vector3(0, 0, cornerSize / 2);
            }
            else if (offset.x < 0 && offset.z > 0) // Top-Left
            {
                horizontalQuad.transform.position += new Vector3(cornerSize / 2, 0, 0);
                verticalQuad.transform.position += new Vector3(0, 0, -cornerSize / 2);
            }
            else if (offset.x > 0 && offset.z > 0) // Top-Right
            {
                horizontalQuad.transform.position += new Vector3(-cornerSize / 2, 0, 0);
                verticalQuad.transform.position += new Vector3(0, 0, -cornerSize / 2);
            }

            highlights.Add(horizontalQuad);
            highlights.Add(verticalQuad);
            highlights.Add(cornerQuad);
        }
    }
}