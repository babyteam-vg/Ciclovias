using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class CellBorderHighlighter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject plane;
    [SerializeField] private CurrentTask currentTask;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("Varibles")]
    [SerializeField] private Material fromMaterial;
    [SerializeField] private Material toMaterial;

    private float elevation = 0.003f;
    private List<GameObject> highlights = new List<GameObject>();

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        currentTask.TaskPinned += HighlightCompoundCells;
        currentTask.TaskUnpinned += ClearHighlight;

        tutorialManager.TutorialSectionStarted += HighlightTutorialCells;
        tutorialManager.TutorialCompleted += ClearHighlight;
    }
    private void OnDisable()
    {
        currentTask.TaskPinned -= HighlightCompoundCells;
        currentTask.TaskUnpinned -= ClearHighlight;
        tutorialManager.TutorialSectionStarted -= HighlightTutorialCells;
        tutorialManager.TutorialCompleted -= ClearHighlight;
    }

    private void Start()
    {
        if (plane != null)
        {
            Renderer renderer = plane.GetComponent<Renderer>();
            elevation += renderer.bounds.max.y;
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: 
    public void HighlightCompoundCells(Task task)
    {
        ClearHighlight();

        foreach (Vector2Int pos in task.from.info.surroundings)
        {
            Cell cell = grid.GetCell(pos.x, pos.y);
            if (cell == null || !cell.GetBuildable()) continue;
            CreateHighlightCorners(new Vector2Int(cell.x, cell.y), fromMaterial);
        }

        foreach (Vector2Int pos in task.to.info.surroundings)
        {
            Cell cell = grid.GetCell(pos.x, pos.y);
            if (cell == null || !cell.GetBuildable()) continue;
            CreateHighlightCorners(new Vector2Int(cell.x, cell.y), toMaterial);
        }
    }

    public void HighlightTutorialCells(TutorialSection section)
    {
        ClearHighlight();

        Cell start = grid.GetCell(section.start.x, section.start.y);
        if (start == null || !start.GetBuildable()) return;
        CreateHighlightCorners(new Vector2Int(start.x, start.y), fromMaterial);

        Cell end = grid.GetCell(section.end.x, section.end.y);
        if (end == null || !end.GetBuildable()) return;
        CreateHighlightCorners(new Vector2Int(end.x, end.y), toMaterial);
    }

    // ::::: Clear All teh Highlights
    public void ClearHighlight()
    {
        foreach (GameObject highlight in highlights)
            Destroy(highlight);

        highlights.Clear();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void CreateHighlightCorners(Vector2Int gridPosition, Material material)
    {
        Vector3 worldPosition = grid.GetWorldPositionFromCellCentered(gridPosition.x, gridPosition.y);
        float cellSize = grid.GetCellSize();
        float cornerSize = cellSize * 0.25f;
        float thickness = cornerSize * 0.5f;

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