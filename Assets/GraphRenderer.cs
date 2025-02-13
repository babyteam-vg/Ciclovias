using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphRenderer : MonoBehaviour
{
    public Material standardMaterial; // Material for standard nodes and edges
    public Material highlightMaterial; // Material for highlighted nodes and edges
    public float nodeSize = 0.2f;
    public float edgeWidth = 0.1f;

    [SerializeField] private Graph graph;
    private Dictionary<Vector2Int, GameObject> nodeObjects;
    private Dictionary<(Vector2Int, Vector2Int), LineRenderer> edgeRenderers;

    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    // Public field to store the current path (provided by TasksManager)
    public List<Vector2Int> currentPath = new List<Vector2Int>();

    // :::::::::: MONO MEHTODS ::::::::::
    private void Awake()
    {
        nodeObjects = new Dictionary<Vector2Int, GameObject>();
        edgeRenderers = new Dictionary<(Vector2Int, Vector2Int), LineRenderer>();
    }

    private void OnEnable()
    {
        laneConstructor.OnLaneBuilt += OnLaneBuilt;
        laneDestructor.OnLaneDestroyed += OnLaneDestroyed;
    }

    private void OnDisable()
    {
        laneConstructor.OnLaneBuilt -= OnLaneBuilt;
        laneDestructor.OnLaneDestroyed -= OnLaneDestroyed;
    }

    private void Update()
    {
        // Update materials for nodes and edges based on the current path
        UpdateMaterials();
    }

    // :::::::::: PUBLIC MEHTODS ::::::::::

    // :::::::::: PRIVATE MEHTODS ::::::::::
    // ::::: Lane Constructor Subscriber
    private void OnLaneBuilt(Vector2Int newNodePosition)
    {
        Node newNode = graph.GetNode(newNodePosition);
        if (newNode == null) return;

        // Draw Node
        GameObject nodeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nodeObject.transform.position = new Vector3(newNode.worldPosition.x, 0.1f, newNode.worldPosition.y);
        nodeObject.transform.localScale = Vector3.one * nodeSize;
        nodeObject.GetComponent<Renderer>().material = standardMaterial; // Use standard material initially

        nodeObjects[newNodePosition] = nodeObject;

        // Draw Edges
        foreach (Node neighbor in newNode.neighbors)
        {
            Vector2Int neighborPosition = neighbor.position;
            CreateEdge(newNodePosition, neighborPosition);
        }
    }

    // ::::: Lane Destructor Subscriber
    private void OnLaneDestroyed(Vector2Int removedNodePosition)
    {
        // Erase Node
        if (nodeObjects.TryGetValue(removedNodePosition, out GameObject nodeObject))
        {
            Destroy(nodeObject);
            nodeObjects.Remove(removedNodePosition);
        }

        // Erase Edges
        List<(Vector2Int, Vector2Int)> edgesToRemove = new List<(Vector2Int, Vector2Int)>();
        foreach (var edge in edgeRenderers.Keys)
            if (edge.Item1 == removedNodePosition || edge.Item2 == removedNodePosition)
                edgesToRemove.Add(edge);

        foreach (var edge in edgesToRemove)
        {
            Destroy(edgeRenderers[edge].gameObject);
            edgeRenderers.Remove(edge);
        }
    }

    // ::::: Edges Drawer
    private void CreateEdge(Vector2Int positionA, Vector2Int positionB)
    {
        if (edgeRenderers.ContainsKey((positionA, positionB)) || edgeRenderers.ContainsKey((positionB, positionA)))
            return; // Avoid Duplicate Edges

        // Draw an Edge
        GameObject edgeObject = new GameObject("Edge");
        LineRenderer lineRenderer = edgeObject.AddComponent<LineRenderer>();

        lineRenderer.material = standardMaterial; // Use standard material initially
        lineRenderer.startWidth = edgeWidth;
        lineRenderer.endWidth = edgeWidth;

        Vector3 startPos = new Vector3(graph.GetNode(positionA).worldPosition.x, 0.1f, graph.GetNode(positionA).worldPosition.y);
        Vector3 endPos = new Vector3(graph.GetNode(positionB).worldPosition.x, 0.1f, graph.GetNode(positionB).worldPosition.y);

        lineRenderer.SetPositions(new Vector3[] { startPos, endPos });

        edgeRenderers[(positionA, positionB)] = lineRenderer;
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