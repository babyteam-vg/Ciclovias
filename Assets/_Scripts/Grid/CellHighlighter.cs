using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellHighlighter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject plane;
    [SerializeField] private CellContentMesh[] contentMeshes;

    private float elevation = 0.1f;
    private Dictionary<CellContent, Material> contentMaterialMap;
    private List<GameObject> highlights = new List<GameObject>();

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        contentMaterialMap = new Dictionary<CellContent, Material>();
        foreach (var mesh in contentMeshes)
            contentMaterialMap[mesh.content] = mesh.material;
    }

    private void Start()
    {
        if (plane != null)
        {
            Renderer renderer = plane.GetComponent<Renderer>();
            elevation = renderer.bounds.max.y;
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: When OnBuildStarted & OnLaneBuilt
    public void HighlightBuildableCells(Vector2Int gridPosition)
    {
        ClearHighlight(); // Limpiar resaltados anteriores

        List<Cell> adjacentCells = grid.GetAdjacentCells(gridPosition.x, gridPosition.y);

        foreach (Cell cell in adjacentCells)
        {
            if (cell == null || !cell.GetBuildable()) continue;

            CellContent cellContent = cell.GetContent();

            if (contentMaterialMap.ContainsKey(cellContent))
            {
                List<Cell> cellsWithSameContent = grid.GetAdjacentCellsOfContent(
                    new Vector2Int(cell.x, cell.y), cellContent);

                foreach (Cell sameContentCell in cellsWithSameContent)
                    CreateHighlightFill(new Vector2Int(sameContentCell.x, sameContentCell.y), cellContent);
            }
        }
    }

    // ::::: Limpiar resaltados
    public void ClearHighlight()
    {
        foreach (GameObject highlight in highlights)
            Destroy(highlight);

        highlights.Clear();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void CreateHighlightFill(Vector2Int gridPosition, CellContent content)
    {
        Vector3 worldPosition = grid.GetWorldPositionFromCellCentered(gridPosition.x, gridPosition.y);

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.position = worldPosition + new Vector3(0, elevation, 0);
        quad.transform.rotation = Quaternion.Euler(90, 0, 0);
        quad.transform.localScale = new Vector3(grid.GetCellSize(), grid.GetCellSize(), 1);

        if (contentMaterialMap.TryGetValue(content, out Material material))
            quad.GetComponent<Renderer>().material = material;

        highlights.Add(quad);
    }
}

[System.Serializable]
public class CellContentMesh
{
    public CellContent content;
    public Material material;
}