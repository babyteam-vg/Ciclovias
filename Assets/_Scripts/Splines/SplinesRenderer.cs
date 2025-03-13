using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplinesRenderer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SplinesManager splinesManager;
    [SerializeField] private GameObject plane;

    [Header("Variables")]
    [SerializeField] private float laneWidth = 0.5f;
    [SerializeField] private int segmentsPerUnit = 16;
    [SerializeField] private Material laneMaterial;

    private float elevation = 0.004f;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        if (laneMaterial != null)
            meshRenderer.material = laneMaterial;
    }

    private void OnEnable() { splinesManager.SplineChanged += UpdateMesh; }
    private void OnDisable() { splinesManager.SplineChanged -= UpdateMesh; }

    private void Start()
    {
        if (plane != null)
        {
            Renderer renderer = plane.GetComponent<Renderer>();
            elevation += renderer.bounds.max.y;
        }

        UpdateMesh();
    }

    // :::::::::: PUBLIC METHODS ::::::::::

    // :::::::::: PRIVATE METHODS ::::::::::
    private void UpdateMesh()
    {
        if (splinesManager == null || splinesManager.splineContainer == null)
            return;

        mesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        foreach (var spline in splinesManager.splineContainer.Splines)
        {
            int resolution = Mathf.CeilToInt(spline.GetLength() * segmentsPerUnit);
            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                Vector3 position = spline.EvaluatePosition(t);
                Vector3 tangent = spline.EvaluateTangent(t);
                tangent = tangent.normalized;
                Vector3 normal = -Vector3.Cross(tangent, Vector3.up).normalized;

                Vector3 v1 = position + normal * (laneWidth * 0.5f) + Vector3.up * elevation;
                Vector3 v2 = position - normal * (laneWidth * 0.5f) + Vector3.up * elevation;

                int baseIndex = vertices.Count;
                vertices.Add(v1);
                vertices.Add(v2);

                // Texture Mapping
                float uvY = t * spline.GetLength(); // Repeat
                uvs.Add(new Vector2(0, uvY));
                uvs.Add(new Vector2(1, uvY));

                if (i > 0)
                {
                    // Triangles
                    triangles.Add(baseIndex - 2);
                    triangles.Add(baseIndex - 1);
                    triangles.Add(baseIndex + 1);

                    triangles.Add(baseIndex - 2);
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex);
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
    }
}
