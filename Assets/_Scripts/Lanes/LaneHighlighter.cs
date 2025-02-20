using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class LaneHighlighter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Material highlightMaterial;

    [Header("Variables")]
    public float fadeRadius = 2.5f; // Maximum Radius
    public float softness = 3f; // Opacity's Aggressiveness
    public float elevation = 0.11f;

    private List<GameObject> highlights = new List<GameObject>();

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: When OnBuildStarted & OnLaneBuilt
    public void HighlightBuildableCells(Vector2Int gridPosition)
    {
        ClearHighlight();

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
            GameObject highlight = CreateHighlightBorder(worldPos, centerWorldPos);
            highlights.Add(highlight);
        }
    }

    // ::::: When OnBuildFinished
    public void ClearHighlight()
    {
        foreach (GameObject highlight in highlights)
            Destroy(highlight);

        highlights.Clear();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Radial Border Shader
    private GameObject CreateHighlightBorder(Vector3 position, Vector3 centerWorldPos)
    {
        GameObject highlight = new GameObject("HighlightBorder");
        highlight.transform.position = position + new Vector3(grid.GetCellSize() / 2, elevation, grid.GetCellSize() / 2);
        highlight.transform.localScale = new Vector3(grid.GetCellSize(), 1, grid.GetCellSize());

        MeshRenderer meshRenderer = highlight.AddComponent<MeshRenderer>();
        Material material = new Material(highlightMaterial);
        material.SetVector("_Center", centerWorldPos);
        material.SetFloat("_FadeRadius", fadeRadius);
        material.SetFloat("_Softness", softness);
        meshRenderer.material = material;

        MeshFilter meshFilter = highlight.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateBorderMesh();

        return highlight;
    }

    // ::::: Border Mesh
    private Mesh CreateBorderMesh()
    {
        Mesh mesh = new Mesh();
        float innerOffset = 0.48f;

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0, -0.5f), // Outer Vertices
            new Vector3(0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, 0.5f),
            new Vector3(-0.5f, 0, 0.5f),

            new Vector3(-innerOffset, 0, -innerOffset), // Inner Vertices
            new Vector3(innerOffset, 0, -innerOffset),
            new Vector3(innerOffset, 0, innerOffset),
            new Vector3(-innerOffset, 0, innerOffset)
        };

        int[] triangles = new int[]
        {
            0, 4, 1, // Bottom Border
            1, 4, 5,

            1, 5, 2, // Right Border
            2, 5, 6,

            2, 6, 3, // Top Border
            3, 6, 7,

            3, 7, 0, // Left Border
            0, 7, 4
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}