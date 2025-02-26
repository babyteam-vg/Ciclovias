using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompoundHighlighter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private CurrentTask currentTask;
    [SerializeField] private GameObject plane;

    [Header("Varibles")]
    [SerializeField] private Material fromMaterial;
    [SerializeField] private Material toMaterial;

    private float elevation = 0.002f;
    private List<GameObject> highlights = new List<GameObject>();

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        currentTask.TaskPinned += HighlightCompoundCells;
        currentTask.TaskUnpinned += ClearHighlight;
    }
    private void OnDisable()
    {
        currentTask.TaskPinned -= HighlightCompoundCells;
        currentTask.TaskUnpinned -= ClearHighlight;
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
        float cornerSize = cellSize * 0.25f; // Tamaño de las esquinas como un porcentaje de la celda
        float thickness = cornerSize * 0.25f; // Grosor de las líneas de la "L"

        Vector3[] cornerOffsets = new Vector3[]
        {
        new Vector3(-cellSize / 2, elevation, -cellSize / 2), // Esquina inferior izquierda
        new Vector3(cellSize / 2, elevation, -cellSize / 2),  // Esquina inferior derecha
        new Vector3(-cellSize / 2, elevation, cellSize / 2),  // Esquina superior izquierda
        new Vector3(cellSize / 2, elevation, cellSize / 2)    // Esquina superior derecha
        };

        foreach (Vector3 offset in cornerOffsets)
        {
            // Crear el quad horizontal de la "L"
            GameObject horizontalQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            horizontalQuad.transform.position = worldPosition + offset;
            horizontalQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
            horizontalQuad.transform.localScale = new Vector3(cornerSize, thickness, 1);
            horizontalQuad.GetComponent<Renderer>().material = material;

            // Crear el quad vertical de la "L"
            GameObject verticalQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            verticalQuad.transform.position = worldPosition + offset;
            verticalQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
            verticalQuad.transform.localScale = new Vector3(thickness, cornerSize, 1);
            verticalQuad.GetComponent<Renderer>().material = material;

            // Crear el cuadradito de la esquina para unir las líneas
            GameObject cornerQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cornerQuad.transform.position = worldPosition + offset;
            cornerQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
            cornerQuad.transform.localScale = new Vector3(thickness, thickness, 1); // Mismo grosor que las líneas
            cornerQuad.GetComponent<Renderer>().material = material;

            // Ajustar la posición de los quads para formar la "L" y el cuadradito
            if (offset.x < 0 && offset.z < 0) // Esquina inferior izquierda
            {
                horizontalQuad.transform.position += new Vector3(cornerSize / 2, 0, 0);
                verticalQuad.transform.position += new Vector3(0, 0, cornerSize / 2);
            }
            else if (offset.x > 0 && offset.z < 0) // Esquina inferior derecha
            {
                horizontalQuad.transform.position += new Vector3(-cornerSize / 2, 0, 0);
                verticalQuad.transform.position += new Vector3(0, 0, cornerSize / 2);
            }
            else if (offset.x < 0 && offset.z > 0) // Esquina superior izquierda
            {
                horizontalQuad.transform.position += new Vector3(cornerSize / 2, 0, 0);
                verticalQuad.transform.position += new Vector3(0, 0, -cornerSize / 2);
            }
            else if (offset.x > 0 && offset.z > 0) // Esquina superior derecha
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