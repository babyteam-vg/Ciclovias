using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private Grid grid; // Referencia al componente Grid
    [SerializeField] private Material defaultMaterial; // Material por defecto
    [SerializeField] private CellContentMesh[] contentMeshes; // Meshes por tipo de contenido

    private Dictionary<CellContent, MeshData> meshDataDict;

    private void Start()
    {
        InitializeMeshData();
        GenerateMeshes();
    }

    private void InitializeMeshData()
    {
        // Crear un diccionario para almacenar MeshData para cada contenido
        meshDataDict = new Dictionary<CellContent, MeshData>();

        foreach (CellContentMesh contentMesh in contentMeshes)
        {
            meshDataDict[contentMesh.content] = new MeshData();
        }
    }

    private void GenerateMeshes()
    {
        int width = grid.GetGridDimensions().x;
        int height = grid.GetGridDimensions().y;

        // Llenar MeshData para cada contenido
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid.GetCell(x, y);
                if (cell == null) continue;

                CellContent content = cell.GetContent();
                if (!meshDataDict.ContainsKey(content)) continue;

                // Agregar la celda a su MeshData correspondiente
                Vector3 cellPosition = grid.GetWorldPositionFromCell(x, y);
                meshDataDict[content].AddQuad(cellPosition, grid.GetCellSize());
            }
        }

        // Crear un GameObject y MeshRenderer para cada contenido
        foreach (var kvp in meshDataDict)
        {
            CellContent content = kvp.Key;
            MeshData meshData = kvp.Value;

            if (meshData.Vertices.Count == 0) continue; // Ignorar contenido sin celdas

            // Crear el mesh
            Mesh mesh = new Mesh
            {
                vertices = meshData.Vertices.ToArray(),
                triangles = meshData.Triangles.ToArray()
            };
            mesh.RecalculateNormals();

            // Crear un GameObject para este contenido
            GameObject contentObject = new GameObject(content.ToString());
            contentObject.transform.SetParent(transform);

            // Agregar MeshFilter y MeshRenderer
            MeshFilter meshFilter = contentObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = contentObject.AddComponent<MeshRenderer>();
            meshRenderer.material = GetMaterialForContent(content);
        }
    }

    private Material GetMaterialForContent(CellContent content)
    {
        foreach (CellContentMesh contentMesh in contentMeshes)
        {
            if (contentMesh.content == content)
                return contentMesh.material;
        }

        return defaultMaterial; // Usar el material por defecto si no se encuentra
    }
}

[System.Serializable]
public class CellContentMesh
{
    public CellContent content; // Tipo de contenido
    public Material material; // Material para este contenido
}

// Clase auxiliar para almacenar datos del Mesh
public class MeshData
{
    public List<Vector3> Vertices { get; } = new List<Vector3>();
    public List<int> Triangles { get; } = new List<int>();
    private int vertexIndex = 0;

    // Agregar un quad (rectángulo) al MeshData
    public void AddQuad(Vector3 position, float size)
    {
        Vertices.Add(position);
        Vertices.Add(position + new Vector3(size, 0, 0));
        Vertices.Add(position + new Vector3(0, 0, size));
        Vertices.Add(position + new Vector3(size, 0, size));

        Triangles.Add(vertexIndex + 0);
        Triangles.Add(vertexIndex + 2);
        Triangles.Add(vertexIndex + 1);

        Triangles.Add(vertexIndex + 1);
        Triangles.Add(vertexIndex + 2);
        Triangles.Add(vertexIndex + 3);

        vertexIndex += 4;
    }
}
