using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(MeshRenderer))]
public class SplineRenderer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SplineManager splineManager;
    [SerializeField] private GameObject plane;

    private RendererUtility rendererUtility;

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material sealedMaterial;

    private float laneWidth = 0.4f;
    private float laneHeight = 0.02f;
    private int segmentsPerUnit = 7;

    private Dictionary<Spline, MeshFilter> normalMeshes = new Dictionary<Spline, MeshFilter>();
    private Dictionary<Spline, MeshFilter> sealedMeshes = new Dictionary<Spline, MeshFilter>();

    // :::::::::: MONO METHODS ::::::::::
    private void Awake() { rendererUtility = new RendererUtility(); }

    private void OnEnable()
    {
        splineManager.SplineUpdated += UpdateSplineMesh;
        //splineManager.SplineSealed += SealSpline;
    }

    private void OnDisable()
    {
        splineManager.SplineUpdated -= UpdateSplineMesh;
        //splineManager.SplineSealed -= SealSpline;
    }

    private void Start()
    {
        foreach (var spline in splineManager.splineContainer.Splines)
            UpdateSplineMesh(spline);
    }

    // :::::::::: EVENT METHODS ::::::::::
    // :::::
    private void UpdateSplineMesh(Spline spline)
    {
        if (sealedMeshes.TryGetValue(spline, out MeshFilter sealedMeshFilter))
        {
            BuildMesh(spline, sealedMeshFilter.mesh);
            return;
        }

        if (!normalMeshes.TryGetValue(spline, out MeshFilter meshFilter))
        {
            meshFilter = CreateSplineMesh(spline, normalMaterial);
            normalMeshes[spline] = meshFilter;
        }

        BuildMesh(spline, meshFilter.mesh);
    }

    // :::::
    private void SealSpline(Spline spline)
    {
        if (normalMeshes.TryGetValue(spline, out MeshFilter meshFilter))
        {
            Destroy(meshFilter.gameObject);
            normalMeshes.Remove(spline);
        }

        if (!sealedMeshes.ContainsKey(spline))
        {
            MeshFilter sealedMeshFilter = CreateSplineMesh(spline, sealedMaterial);
            sealedMeshes[spline] = sealedMeshFilter;
            BuildMesh(spline, sealedMeshFilter.mesh);
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // :::::
    private MeshFilter CreateSplineMesh(Spline spline, Material material)
    {
        GameObject splineObject = new GameObject("SplineMesh");
        splineObject.transform.SetParent(transform);

        MeshFilter meshFilter = splineObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = splineObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        meshFilter.mesh = new Mesh();
        return meshFilter;
    }

    // :::::
    private void BuildMesh(Spline spline, Mesh mesh)
    {
        mesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        int resolution = Mathf.CeilToInt(spline.GetLength() * segmentsPerUnit);

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            Vector3 position = spline.EvaluatePosition(t);
            Vector3 tangent = spline.EvaluateTangent(t);
            Vector3 normal = -Vector3.Cross(tangent.normalized, Vector3.up).normalized;

            float elevation = rendererUtility.GetMaxElevationAtPoint(position, plane);

            // Upper Vertex
            Vector3 topLeft = position + normal * (laneWidth * 0.5f) + Vector3.up * (elevation + laneHeight);
            Vector3 topRight = position - normal * (laneWidth * 0.5f) + Vector3.up * (elevation + laneHeight);

            // Bottom Vertex
            Vector3 bottomLeft = position + normal * (laneWidth * 0.5f) + Vector3.up * elevation;
            Vector3 bottomRight = position - normal * (laneWidth * 0.5f) + Vector3.up * elevation;

            int baseIndex = vertices.Count;

            vertices.Add(topLeft);    // 0
            vertices.Add(topRight);   // 1
            vertices.Add(bottomLeft); // 2
            vertices.Add(bottomRight);// 3

            float uvY = t * spline.GetLength();
            uvs.Add(new Vector2(0, uvY));
            uvs.Add(new Vector2(1, uvY));
            uvs.Add(new Vector2(0, uvY));
            uvs.Add(new Vector2(1, uvY));

            if (i > 0)
            {
                // Upper Face
                triangles.Add(baseIndex - 4);
                triangles.Add(baseIndex - 3);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex - 4);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex);

                // Bottom Face
                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);

                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex - 1);

                // Left Side
                triangles.Add(baseIndex - 4);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex - 2);

                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);

                // Right Side
                triangles.Add(baseIndex - 3);
                triangles.Add(baseIndex - 1);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex - 1);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 1);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
    }
}
