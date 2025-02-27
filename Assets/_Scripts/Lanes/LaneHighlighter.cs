using System.Collections.Generic;
using UnityEngine;

public class LaneHighlighter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private GameObject plane;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private LaneConstructor laneConstructor;

    [Header("Variables")]
    [SerializeField] private CellContentMesh[] contentMeshes;

    private float outerOffset = 0.55f;
    private float innerOffset = 0.45f;
    private float fadeRadius = 2.5f; // Maximum Radius
    private float softness = 2.5f; // Opacity's Aggressiveness
    private float elevation = 0.002f;
    private List<GameObject> highlights = new List<GameObject>();
    private Dictionary<CellContent, Material> contentMaterialMap;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        contentMaterialMap = new Dictionary<CellContent, Material>();
        foreach (var mesh in contentMeshes)
            contentMaterialMap[mesh.content] = mesh.material;
    }

    private void OnEnable() { inputManager.OnCursorMove += HighlightBuildableCells; }
    private void OnDisable() { inputManager.OnCursorMove -= HighlightBuildableCells; }

    private void Start()
    {
        if (plane != null)
        {
            Renderer renderer = plane.GetComponent<Renderer>();
            elevation += renderer.bounds.max.y;
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void HighlightBuildableCells(Vector2Int gridPosition)
    {
        ClearHighlight(gridPosition);

        List<Cell> firstLayerCells = grid.GetAdjacentCells(gridPosition.x, gridPosition.y);
        List<Cell> secondLayerCells = new List<Cell>();

        foreach (Cell cell in firstLayerCells)
            if (cell != null)
            {
                List<Cell> adjacentToFirstLayer = grid.GetAdjacentCells(cell.x, cell.y);
                secondLayerCells.AddRange(adjacentToFirstLayer);
            }

        HashSet<Cell> allCells = new HashSet<Cell>(firstLayerCells);
        allCells.UnionWith(secondLayerCells);

        Vector3 centerWorldPos = grid.GetWorldPositionFromCellCentered(gridPosition.x, gridPosition.y);

        foreach (Cell cell in allCells)
        {
            if (cell == null || !cell.GetBuildable()) continue;

            Vector3 worldPos = grid.GetWorldPositionFromCell(cell.x, cell.y);
            GameObject highlight = CreateHighlightBorder(worldPos, centerWorldPos, cell.GetContent());
            highlights.Add(highlight);
        }
    }

    public void ClearHighlight(Vector2Int _)
    {
        foreach (GameObject highlight in highlights)
            Destroy(highlight);

        highlights.Clear();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private GameObject CreateHighlightBorder(Vector3 position, Vector3 centerWorldPos, CellContent content)
    {
        GameObject highlight = new GameObject("HighlightBorder");
        highlight.transform.position = position + new Vector3(grid.GetCellSize() / 2, elevation, grid.GetCellSize() / 2);
        highlight.transform.localScale = new Vector3(grid.GetCellSize(), 1, grid.GetCellSize());

        MeshRenderer meshRenderer = highlight.AddComponent<MeshRenderer>();
        Material material = new Material(highlightMaterial);

        if (contentMaterialMap.TryGetValue(content, out Material contentMaterial))
            material.color = contentMaterial.color;

        material.SetVector("_Center", centerWorldPos);
        material.SetFloat("_FadeRadius", fadeRadius);
        material.SetFloat("_Softness", softness);
        meshRenderer.material = material;

        MeshFilter meshFilter = highlight.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateBorderMesh();

        return highlight;
    }

    private Mesh CreateBorderMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[]
        {
            // Outer Vertices
            new Vector3(-outerOffset, 0, -outerOffset),
            new Vector3(outerOffset, 0, -outerOffset),
            new Vector3(outerOffset, 0, outerOffset),
            new Vector3(-outerOffset, 0, outerOffset),

            // Inner Vertices
            new Vector3(-innerOffset, 0, -innerOffset),
            new Vector3(innerOffset, 0, -innerOffset),
            new Vector3(innerOffset, 0, innerOffset),
            new Vector3(-innerOffset, 0, innerOffset)
        };

        int[] triangles = new int[]
        {
            // Bottom Border
            0, 4, 1,
            1, 4, 5,

            // Right Border
            1, 5, 2,
            2, 5, 6,

            // Top Border
            2, 6, 3,
            3, 6, 7,

            // Left Border
            3, 7, 0,
            0, 7, 4
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}