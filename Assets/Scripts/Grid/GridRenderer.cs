using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private Grid grid; // Referencia al componente Grid
    [SerializeField] private Material cellMaterial; // Material para las celdas
    [SerializeField] private Color defaultColor = Color.gray; // Color predeterminado
    [SerializeField] private CellContentColor[] contentColors; // Colores por tipo de contenido

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;

    private void Start() { GenerateMesh(); }

    private void GenerateMesh()
    {
        int width = grid.GetGridDimensions().x;
        int height = grid.GetGridDimensions().y;
        int numCells = width * height;

        mesh = new Mesh();
        vertices = new Vector3[numCells * 4];
        triangles = new int[numCells * 6];
        colors = new Color[numCells * 4];

        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid.GetCell(x, y) != null)
                {
                    Vector3 cellPosition = grid.GetWorldPositionFromCell(x, y);

                    // Definir los vértices de la celda
                    vertices[vertexIndex + 0] = cellPosition;
                    vertices[vertexIndex + 1] = cellPosition + new Vector3(grid.GetCellSize(), 0, 0);
                    vertices[vertexIndex + 2] = cellPosition + new Vector3(0, 0, grid.GetCellSize());
                    vertices[vertexIndex + 3] = cellPosition + new Vector3(grid.GetCellSize(), 0, grid.GetCellSize());

                    // Definir los triángulos de la celda
                    triangles[triangleIndex + 0] = vertexIndex + 0;
                    triangles[triangleIndex + 1] = vertexIndex + 2;
                    triangles[triangleIndex + 2] = vertexIndex + 1;

                    triangles[triangleIndex + 3] = vertexIndex + 1;
                    triangles[triangleIndex + 4] = vertexIndex + 2;
                    triangles[triangleIndex + 5] = vertexIndex + 3;

                    // Asignar color según el contenido
                    Cell cell = grid.GetCell(x, y);
                    Color cellColor = GetColorForContent(cell.GetContent());

                    colors[vertexIndex + 0] = cellColor;
                    colors[vertexIndex + 1] = cellColor;
                    colors[vertexIndex + 2] = cellColor;
                    colors[vertexIndex + 3] = cellColor;

                    vertexIndex += 4;
                    triangleIndex += 6;
                }
            }
        }

        // Configurar el Mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        // Crear un MeshFilter y un MeshRenderer
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = cellMaterial;
    }

    private Color GetColorForContent(CellContent content)
    {
        foreach (CellContentColor contentColor in contentColors)
        {
            if (contentColor.content == content)
                return contentColor.color;
        }
        return defaultColor; // Usar el color predeterminado si no se encuentra
    }
}

[System.Serializable]
public class CellContentColor
{
    public CellContent content; // Tipo de contenido
    public Color color; // Color asociado
}
