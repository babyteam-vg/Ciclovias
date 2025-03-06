using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class SplinesManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    [Header("Variables")]
    [SerializeField] private Material splineMaterial;
    [SerializeField] private float splineWidth = 0.1f;

    private bool isIntersection = false;
    private float tolerance = 0.9f;
    private SplineContainer splineContainer;
    private Spline spline;

    private Vector3 startWorldPosition;
    private Vector2Int previousNodePosition;
    private Queue<BezierKnot> knotQueue = new Queue<BezierKnot>(3);

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null)
            splineContainer = gameObject.AddComponent<SplineContainer>();

        spline = splineContainer.Spline;
    }

    private void OnEnable()
    {
        laneConstructor.OnBuildStarted += UpdateStartNodePosition;
        laneConstructor.OnLaneBuilt += HandleLaneBuilt;
        //laneDestructor.OnLaneDestroyed += HandleLaneDestroyed;
    }
    private void OnDisable()
    {
        laneConstructor.OnBuildStarted -= UpdateStartNodePosition;
        laneConstructor.OnLaneBuilt -= HandleLaneBuilt;
        //laneDestructor.OnLaneDestroyed -= HandleLaneDestroyed;
    }

    private void Start()
    {
        
    }

    // :::::::::: PUBLIC METHODS ::::::::::

    // :::::::::: PRIVATE METHODS ::::::::::
    private void UpdateStartNodePosition(Vector2Int startNodePosition) { startWorldPosition = grid.GetWorldPositionFromCellCentered(startNodePosition.x, startNodePosition.y); }

    // ::::: When a Lane is Built
    private void HandleLaneBuilt(Vector2Int addedNodePosition)
    {
        Vector3 addedWorldPosition = grid.GetWorldPositionFromCellCentered(addedNodePosition.x, addedNodePosition.y);

        Node addedNode = graph.GetNode(addedNodePosition);
        if (addedNode == null) return;

        if (previousNodePosition == null) return;
        List<Vector2Int> previousNeighbors = graph.GetNeighborsPos(previousNodePosition);

        // Edge of the Lane
        if (previousNeighbors.Count < 2) // Current Spline
        {
            knotQueue.Clear();
            StartNewSpline(startWorldPosition);
            previousNodePosition = addedNodePosition;
            return;
        }

        // Check Collinearity
        foreach (var neighbor in previousNeighbors)
            if (!IsCollinear(previousNodePosition, neighbor, addedNodePosition))
            {
                isIntersection = true;
                break;
            }
            else isIntersection = false;

        if (isIntersection) StartNewSpline(addedWorldPosition);
        else AddKnotToCurrentSpline(addedWorldPosition);

        previousNodePosition = addedNodePosition;
        if (knotQueue.Count == 3) knotQueue.Dequeue();
    }

    // ::::: Detect Collinearity
    private bool IsCollinear(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        Vector2Int v1 = b - a;
        Vector2Int v2 = c - a;

        int crossProduct = v1.x * v2.y - v1.y * v2.x;

        return crossProduct == 0;
    }

    // ::::: New Spline (Limit or Intersection)
    private void StartNewSpline(Vector3 position)
    {
        if (isIntersection)
        {
            spline.Remove(spline.Knots.Last());
            spline.Insert(spline.Count, knotQueue.Dequeue(), TangentMode.Broken, 0.5f);
        }

        Spline newSpline = new Spline();

        BezierKnot startKnot = new BezierKnot(position);
        newSpline.Insert(0, startKnot, TangentMode.Broken, 0.5f);
        splineContainer.AddSpline(newSpline);
        spline = newSpline;

        knotQueue.Enqueue(startKnot);
    }

    // ::::: Continue Spline (Collinear)
    private void AddKnotToCurrentSpline(Vector3 worldPosition)
    {
        BezierKnot newKnot = new BezierKnot(worldPosition);

        if (spline != null)
        {
            if (spline.Count == 0)
            {
                // First Knot
                spline.Insert(0, newKnot, TangentMode.Broken, 0.5f);
            }
            else
            {
                // Add Knot
                if (spline.Count > 1) spline.RemoveAt(spline.Count - 1);
                spline.Insert(spline.Count, newKnot, TangentMode.Broken, 0.5f);
            }
        }

        knotQueue.Enqueue(newKnot);
    }
}