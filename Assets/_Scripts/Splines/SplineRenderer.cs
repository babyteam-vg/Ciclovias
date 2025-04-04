using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(MeshRenderer))]
public class SplineRenderer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private SplineManager splineManager;
    [SerializeField] private GameObject plane;

    private RendererUtility rendererUtility;

    [Header("Material")]
    [SerializeField] private Material splineMaterial;

    private float laneWidth = 0.4f;
    private float laneHeight = 0.05f;
    private int segmentsPerUnit = 27;

    private Dictionary<Spline, MeshFilter> splineMeshes = new Dictionary<Spline, MeshFilter>();
    private HashSet<Spline> activeSplines = new HashSet<Spline>();

    // :::::::::: MONO METHODS ::::::::::
    private void Awake() { rendererUtility = new RendererUtility(); }

    private void OnEnable()
    {
        taskManager.ActiveTaskScoresUpdated += UpdateActiveSplines;
        taskManager.TaskSealed += ClearActiveSplines;
        splineManager.SplineUpdated += UpdateSplineMesh;
    }
    private void OnDisable()
    {
        taskManager.ActiveTaskScoresUpdated -= UpdateActiveSplines;
        taskManager.TaskSealed -= ClearActiveSplines;
        splineManager.SplineUpdated -= UpdateSplineMesh;

        foreach (var mesh in splineMeshes.Values)
        {
            if (mesh != null)
                Destroy(mesh.gameObject);
        }
        splineMeshes.Clear();
    }

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
            meshFilter = CreateSplineMesh(spline);
            splineMeshes[spline] = meshFilter;
        }

        BuildMesh(spline, meshFilter.mesh);
        meshFilter.GetComponent<MeshRenderer>().material = splineMaterial;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private MeshFilter CreateSplineMesh(Spline spline)
    {
        GameObject splineObject = new GameObject("SplineMesh");
        splineObject.transform.SetParent(transform);

        MeshFilter meshFilter = splineObject.AddComponent<MeshFilter>();
        splineObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = new Mesh();
        return meshFilter;
    }

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

            float elevation = rendererUtility.GetMaxElevationAtPoint(position, plane) - laneHeight / 2;

            Vector3 topLeft = position + normal * (laneWidth * 0.5f) + Vector3.up * (elevation + laneHeight);
            Vector3 topRight = position - normal * (laneWidth * 0.5f) + Vector3.up * (elevation + laneHeight);
            Vector3 bottomLeft = position + normal * (laneWidth * 0.5f) + Vector3.up * elevation;
            Vector3 bottomRight = position - normal * (laneWidth * 0.5f) + Vector3.up * elevation;

            int baseIndex = vertices.Count;

            vertices.Add(topLeft);
            vertices.Add(topRight);
            vertices.Add(bottomLeft);
            vertices.Add(bottomRight);

            float uvY = t * spline.GetLength();
            uvs.Add(new Vector2(0, uvY));
            uvs.Add(new Vector2(1, uvY));
            uvs.Add(new Vector2(0, uvY));
            uvs.Add(new Vector2(1, uvY));

            if (i > 0)
            {
                triangles.Add(baseIndex - 4);
                triangles.Add(baseIndex - 3);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex - 4);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex);

                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex - 1);

                triangles.Add(baseIndex - 4);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex - 2);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);

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

    // :::::::::: ACTIVE SPLINES METHODS ::::::::::
    private void UpdateActiveSplines(List<Vector2Int> path)
    {
        activeSplines.Clear();

        foreach (Vector2Int position in path)
            activeSplines.Add(FindSplineSimplified(position));
    }

    private void ClearActiveSplines(Task _)
    {
        activeSplines.Clear();
    }

    private Spline FindSplineSimplified(Vector2Int gridPosition)
    {
        Vector3 worldPosition = grid.GetWorldPositionFromCellCentered(gridPosition.x, gridPosition.y);

        foreach (Spline spline in splineManager.splineContainer.Splines)
        {
            Vector3 p0 = spline.ElementAt(0).Position;
            Vector3 p1 = spline.ElementAt(1).Position;

            if (p0.Equals(worldPosition) || p1.Equals(worldPosition))
                return spline;
        }

        return null;
    }
}
