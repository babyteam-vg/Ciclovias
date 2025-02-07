using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private Grid grid; // Referencia al componente Grid
    [SerializeField] private Material lineMaterial; // Material para los bordes
    [SerializeField] private CellContentMesh[] contentMeshes; // Configuración de materiales por contenido

    private Dictionary<CellContent, List<Vector3[]>> borderDataDict;

    private void Start()
    {
        InitializeBorderData();
        GenerateBorders();
    }

    private void InitializeBorderData()
    {
        borderDataDict = new Dictionary<CellContent, List<Vector3[]>>();

        foreach (CellContentMesh contentMesh in contentMeshes)
        {
            borderDataDict[contentMesh.content] = new List<Vector3[]>();
        }
    }

    private void GenerateBorders()
    {
        int width = grid.GetGridDimensions().x;
        int height = grid.GetGridDimensions().y;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid.GetCell(x, y);
                if (cell == null) continue;

                CellContent content = cell.GetContent();
                if (!borderDataDict.ContainsKey(content)) continue;

                Vector3 cellPosition = grid.GetWorldPositionFromCell(x, y);
                float size = grid.GetCellSize();

                // Agregar los bordes de la celda a la lista
                borderDataDict[content].Add(GetCellBorder(cellPosition, size));
            }
        }

        // Crear GameObjects con LineRenderer para cada contenido
        foreach (var kvp in borderDataDict)
        {
            CellContent content = kvp.Key;
            List<Vector3[]> borders = kvp.Value;

            if (borders.Count == 0) continue;

            GameObject borderObject = new GameObject(content.ToString() + "_Borders");
            borderObject.transform.SetParent(transform);

            foreach (Vector3[] border in borders)
            {
                CreateBorderRenderer(borderObject, border, GetMaterialForContent(content));
            }
        }
    }

    private Vector3[] GetCellBorder(Vector3 position, float size)
    {
        return new Vector3[]
        {
            position, // Esquina inferior izquierda
            position + new Vector3(size, 0, 0), // Esquina inferior derecha
            position + new Vector3(size, 0, size), // Esquina superior derecha
            position + new Vector3(0, 0, size), // Esquina superior izquierda
            position // Cierra el cuadro volviendo al punto inicial
        };
    }

    private void CreateBorderRenderer(GameObject parent, Vector3[] border, Material material)
    {
        GameObject borderObject = new GameObject("Border");
        borderObject.transform.SetParent(parent.transform);

        LineRenderer lineRenderer = borderObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = border.Length;
        lineRenderer.SetPositions(border);
        lineRenderer.material = material;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.loop = false;
    }

    private Material GetMaterialForContent(CellContent content)
    {
        foreach (CellContentMesh contentMesh in contentMeshes)
        {
            if (contentMesh.content == content)
                return contentMesh.material;
        }
        return lineMaterial; // Usar el material por defecto si no se encuentra
    }
}


[System.Serializable]
public class CellContentMesh
{
    public CellContent content; // Tipo de contenido
    public Material material; // Material para este contenido
}