using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphRenderer : MonoBehaviour
{
    public Material standardMaterial; // Material for standard nodes and edges
    public Material highlightMaterial; // Material for highlighted nodes and edges
    public float nodeSize = 0.2f;
    public float edgeWidth = 0.1f;

    [Header("Dependencies")]
    [SerializeField] private Graph graph;
    [SerializeField] private GameObject plane;
    private Dictionary<Vector2Int, GameObject> nodeObjects;
    private Dictionary<(Vector2Int, Vector2Int), LineRenderer> edgeRenderers;

    // Public field to store the current path (provided by TasksManager)
    public List<Vector2Int> currentPath = new List<Vector2Int>();

    private float elevation = 0.004f;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        nodeObjects = new Dictionary<Vector2Int, GameObject>();
        edgeRenderers = new Dictionary<(Vector2Int, Vector2Int), LineRenderer>();
    }

    private void Start()
    {
        if (plane != null)
        {
            Renderer renderer = plane.GetComponent<Renderer>();
            elevation += renderer.bounds.max.y;
        }
    }

    private void OnEnable()
    {
        // Subscribe to Graph events
        graph.OnNodeAdded += HandleNodeAdded;
        graph.OnEdgeAdded += HandleEdgeAdded;
        graph.OnNodeRemoved += HandleNodeRemoved;
        graph.OnEdgeRemoved += HandleEdgeRemoved;
        graph.OnLonelyNodeRemoved += HandleLonelyNodeRemoved;
    }
    private void OnDisable()
    {
        // Unsubscribe from Graph events
        graph.OnNodeAdded -= HandleNodeAdded;
        graph.OnEdgeAdded -= HandleEdgeAdded;
        graph.OnNodeRemoved -= HandleNodeRemoved;
        graph.OnEdgeRemoved -= HandleEdgeRemoved;
        graph.OnLonelyNodeRemoved -= HandleLonelyNodeRemoved;
    }

    private void Update()
    {
        // Update materials for nodes and edges based on the current path
        UpdateMaterials();
    }

    // :::::::::: EVENT HANDLERS ::::::::::
    // ::::: Handle Node Added
    private void HandleNodeAdded(Vector2Int nodePosition)
    {
        Node node = graph.GetNode(nodePosition);
        if (node == null) return;

        DrawNode(nodePosition, node.worldPosition);
    }

    // ::::: Handle Edge Added
    private void HandleEdgeAdded(Vector2Int positionA, Vector2Int positionB)
    {
        DrawEdge(positionA, positionB);
    }

    // ::::: Handle Node Removed
    private void HandleNodeRemoved(Vector2Int nodePosition)
    {
        EraseNode(nodePosition);

        // Remove all edges connected to this node
        List<(Vector2Int, Vector2Int)> edgesToRemove = new List<(Vector2Int, Vector2Int)>();
        foreach (var edge in edgeRenderers.Keys)
        {
            if (edge.Item1 == nodePosition || edge.Item2 == nodePosition)
            {
                edgesToRemove.Add(edge);
            }
        }

        foreach (var edge in edgesToRemove)
        {
            EraseEdge(edge);
        }
    }

    // ::::: Handle Edge Removed
    private void HandleEdgeRemoved(Vector2Int positionA, Vector2Int positionB)
    {
        EraseEdge((positionA, positionB));
    }

    // ::::: Handle Lonely Node Removed
    private void HandleLonelyNodeRemoved(Vector2Int nodePosition)
    {
        EraseNode(nodePosition);
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Node Drawer
    private void DrawNode(Vector2Int nodePosition, Vector2 worldPosition)
    {
        if (nodeObjects.ContainsKey(nodePosition)) return; // Avoid duplicate nodes

        GameObject nodeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nodeObject.transform.position = new Vector3(worldPosition.x, elevation, worldPosition.y);
        nodeObject.transform.localScale = Vector3.one * nodeSize;
        nodeObject.GetComponent<Renderer>().material = standardMaterial; // Use standard material initially

        nodeObjects[nodePosition] = nodeObject;
    }

    // ::::: Node Eraser
    private void EraseNode(Vector2Int nodePosition)
    {
        if (nodeObjects.TryGetValue(nodePosition, out GameObject nodeObject))
        {
            Destroy(nodeObject);
            nodeObjects.Remove(nodePosition);
        }
    }

    // ::::: Edge Drawer
    private void DrawEdge(Vector2Int positionA, Vector2Int positionB)
    {
        if (edgeRenderers.ContainsKey((positionA, positionB)) || edgeRenderers.ContainsKey((positionB, positionA)))
            return; // Avoid duplicate edges

        // Draw an Edge
        GameObject edgeObject = new GameObject("Edge");
        LineRenderer lineRenderer = edgeObject.AddComponent<LineRenderer>();

        lineRenderer.material = standardMaterial; // Use standard material initially
        lineRenderer.startWidth = edgeWidth;
        lineRenderer.endWidth = edgeWidth;

        Vector3 startPos = new Vector3(graph.GetNode(positionA).worldPosition.x, elevation, graph.GetNode(positionA).worldPosition.y);
        Vector3 endPos = new Vector3(graph.GetNode(positionB).worldPosition.x, elevation, graph.GetNode(positionB).worldPosition.y);

        lineRenderer.SetPositions(new Vector3[] { startPos, endPos });

        edgeRenderers[(positionA, positionB)] = lineRenderer;
    }

    // ::::: Edge Eraser
    private void EraseEdge((Vector2Int, Vector2Int) edge)
    {
        if (edgeRenderers.TryGetValue(edge, out LineRenderer lineRenderer))
        {
            Destroy(lineRenderer.gameObject);
            edgeRenderers.Remove(edge);
        }
    }

    // ::::: Material Updater
    private void UpdateMaterials()
    {
        // Update node materials
        foreach (var nodeEntry in nodeObjects)
        {
            Vector2Int nodePosition = nodeEntry.Key;
            GameObject nodeObject = nodeEntry.Value;

            // Check if the node is part of the current path
            if (currentPath.Contains(nodePosition))
            {
                nodeObject.GetComponent<Renderer>().material = highlightMaterial;
            }
            else
            {
                nodeObject.GetComponent<Renderer>().material = standardMaterial;
            }
        }

        // Update edge materials
        foreach (var edgeEntry in edgeRenderers)
        {
            (Vector2Int positionA, Vector2Int positionB) = edgeEntry.Key;
            LineRenderer lineRenderer = edgeEntry.Value;

            // Check if both nodes of the edge are part of the current path
            if (currentPath.Contains(positionA) && currentPath.Contains(positionB))
            {
                lineRenderer.material = highlightMaterial;
            }
            else
            {
                lineRenderer.material = standardMaterial;
            }
        }
    }
}