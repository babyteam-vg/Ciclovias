using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CellFillHighlighter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject plane;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private CellContentMesh[] contentMeshes;

    private RendererUtility rendererUtility;

    private float cellScale = 1f;
    private Dictionary<CellContent, Material> contentMaterialMap;
    private List<GameObject> highlights = new List<GameObject>();
    private bool isHighlightActive = false;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        rendererUtility = new RendererUtility();

        contentMaterialMap = new Dictionary<CellContent, Material>();
        foreach (var mesh in contentMeshes)
            contentMaterialMap[mesh.content] = mesh.material;
    }

    private void OnEnable() { inputManager.OnHighlightToggleDown += ToggleHighlight; }

    private void OnDisable() { inputManager.OnHighlightToggleDown -= ToggleHighlight; }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void ToggleHighlight()
    {
        isHighlightActive = !isHighlightActive;

        if (isHighlightActive)
            HighlightAllBuildableCells();
        else ClearHighlight();
    }

    private void HighlightAllBuildableCells()
    {
        ClearHighlight();

        for (int x = 0; x < grid.GetGridDimensions().x; x++)
            for (int y = 0; y < grid.GetGridDimensions().y; y++)
            {
                Cell cell = grid.GetCell(x, y);
                if (cell == null || !cell.GetBuildable()) continue;

                CellContent cellContent = cell.GetContent();
                if (contentMaterialMap.ContainsKey(cellContent))
                    CreateHighlightFill(new Vector2Int(cell.x, cell.y), cellContent);
            }
    }

    // ::::: Clear All the Highlighted Cells
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
        float elevation = rendererUtility.GetMaxElevationAtPoint(worldPosition, plane);
        quad.transform.position = worldPosition + new Vector3(0, elevation, 0);
        quad.transform.rotation = Quaternion.Euler(90, 0, 0);
        quad.transform.localScale = new Vector3(grid.GetCellSize() * cellScale, grid.GetCellSize() * cellScale, 1);

        if (contentMaterialMap.TryGetValue(content, out Material material))
        {
            Material materialCopy = new Material(material);
            Color color = materialCopy.color;
            color.a = 0.5f;
            materialCopy.color = color;
            quad.GetComponent<Renderer>().material = materialCopy;
        }

        highlights.Add(quad);
    }
}

[System.Serializable]
public class CellContentMesh
{
    public CellContent content;
    public Material material;
}