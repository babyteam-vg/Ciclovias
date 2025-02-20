using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private Grid grid; // Referencia a la grilla
    [SerializeField] private Material lineMaterial; // Material para los bordes
    [SerializeField] private CellContentMesh[] contentMeshes; // Materiales por contenido

    private Dictionary<Vector2Int, LineRenderer> cellBorders = new Dictionary<Vector2Int, LineRenderer>();
    private void Start()
    {
        GenerateBorders();
    }

    private void GenerateBorders()
    {
        int width = grid.GetGridDimensions().x;
        int height = grid.GetGridDimensions().y;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cellPos = new Vector2Int(x, y);
                Cell cell = grid.GetCell(x, y);

                // No dibujar si la celda no tiene contenido
                if (cell == null || cell.GetContent() == CellContent.None)
                    continue;

                Vector3 worldPos = grid.GetWorldPositionFromCell(x, y);
                float size = grid.GetCellSize();

                // Crear los bordes de la celda
                LineRenderer borderRenderer = CreateBorderRenderer(worldPos, size, GetMaterialForCell(cell));
                cellBorders[cellPos] = borderRenderer;
            }
        }
    }

    private LineRenderer CreateBorderRenderer(Vector3 position, float size, Material material)
    {
        GameObject borderObject = new GameObject($"Border_{position.x}_{position.z}");
        borderObject.transform.SetParent(transform);

        LineRenderer lineRenderer = borderObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 5;
        lineRenderer.SetPositions(GetCellBorder(position, size));
        lineRenderer.material = material;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.loop = false;

        return lineRenderer;
    }

    private Vector3[] GetCellBorder(Vector3 position, float size)
    {
        return new Vector3[]
        {
            position,
            position + new Vector3(size, 0, 0),
            position + new Vector3(size, 0, size),
            position + new Vector3(0, 0, size),
            position // Cierra el cuadro volviendo al punto inicial
        };
    }

    private Material GetMaterialForCell(Cell cell)
    {
        foreach (CellContentMesh contentMesh in contentMeshes)
        {
            if (contentMesh.content == cell.GetContent())
                return contentMesh.material;
        }

        return lineMaterial; // Material por defecto
    }
}

//[System.Serializable]
//public class CellContentMesh
//{
//    public CellContent content; // Tipo de contenido
//    public Material material; // Material para este contenido
//}