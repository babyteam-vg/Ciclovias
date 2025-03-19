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

    [Header("Variables")]
    [SerializeField] private float laneWidth = 0.4f;
    [SerializeField] private int segmentsPerUnit = 15;
    [SerializeField] private Material laneMaterial;

    private Dictionary<Spline, MeshFilter> splineMeshes = new Dictionary<Spline, MeshFilter>();

    // :::::::::: MONO METHODS ::::::::::
    private void Awake() { rendererUtility = new RendererUtility(); }

    private void OnEnable() { splineManager.SplineUpdated += UpdateSplineMesh; }
    private void OnDisable() { splineManager.SplineUpdated -= UpdateSplineMesh; }

    private void Start()
    {
        foreach (var spline in splineManager.splineContainer.Splines)
            UpdateSplineMesh(spline);
    }

    // :::::::::: EVENT METHODS ::::::::::
    private void UpdateSplineMesh(Spline spline)
    {
        if (!splineMeshes.TryGetValue(spline, out MeshFilter meshFilter))
        {
            GameObject splineObject = new GameObject("SplineMesh");
            splineObject.transform.SetParent(transform);

            meshFilter = splineObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = splineObject.AddComponent<MeshRenderer>();
            meshRenderer.material = laneMaterial;

            meshFilter.mesh = new Mesh();

            splineMeshes[spline] = meshFilter;
        }

        BuildMesh(spline, meshFilter.mesh);
    }

    // :::::::::: PRIVATE METHODS ::::::::::
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
            tangent = tangent.normalized;
            Vector3 normal = -Vector3.Cross(tangent, Vector3.up).normalized;

            float elevation = rendererUtility.GetMaxElevationAtPoint(position, plane) + 0.0002f;
            Vector3 v1 = position + normal * (laneWidth * 0.5f) + Vector3.up * elevation;
            Vector3 v2 = position - normal * (laneWidth * 0.5f) + Vector3.up * elevation;

            int baseIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);

            float uvY = t * spline.GetLength(); // Repeat
            uvs.Add(new Vector2(0, uvY));
            uvs.Add(new Vector2(1, uvY));

            if (i > 0)
            {
                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex - 1);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
    }
}
