using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class SplinesManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    public SplineContainer splineContainer;
    
    private Spline spline;
    private bool isIntersection = false;
    private Vector3 startWorldPosition;
    private Vector2Int previousNodePosition;
    private Queue<BezierKnot> knotQueue = new Queue<BezierKnot>(3);

    public event Action SplineChanged;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        spline = splineContainer.Spline;
    }

    private void OnEnable()
    {
        laneConstructor.OnBuildStarted += UpdateStartNodePosition;
        laneConstructor.OnLaneBuilt += HandleLaneBuilt;
    }
    private void OnDisable()
    {
        laneConstructor.OnBuildStarted -= UpdateStartNodePosition;
        laneConstructor.OnLaneBuilt -= HandleLaneBuilt;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public List<Spline> GetSplines() { return splineContainer.Splines.ToList(); }
    public Spline GetCurrentSpline() { return spline; }

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
        if (spline.Closed || previousNeighbors.Count < 2)
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

        if (isIntersection) HandleIntersection(addedWorldPosition);
        else AddKnotToCurrentSpline(addedWorldPosition);

        // Closed Lane
        if (addedWorldPosition == (Vector3)spline.First().Position)
        {
            spline.Closed = true;
            SplineChanged?.Invoke(); // !
            return;
        }

        previousNodePosition = addedNodePosition;
        if (knotQueue.Count == 3) knotQueue.Dequeue();
        SplineChanged?.Invoke(); // !
    }

    // ::::: Detect Collinearity
    private bool IsCollinear(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        Vector2Int v1 = b - a;
        Vector2Int v2 = c - a;

        int crossProduct = v1.x * v2.y - v1.y * v2.x;

        return crossProduct == 0;
    }

    // ::::: New Spline (Limit)
    private void StartNewSpline(Vector3 position)
    {
        Spline newSpline = new Spline();

        BezierKnot startKnot = new BezierKnot(position);
        newSpline.Insert(0, startKnot, TangentMode.Broken, 0.5f);
        splineContainer.AddSpline(newSpline);
        spline = newSpline;

        knotQueue.Enqueue(startKnot);
    }

    // ::::: Continue Spline (Collinear)
    private void AddKnotToCurrentSpline(Vector3 position)
    {
        BezierKnot newKnot = new BezierKnot(position);

        if (spline != null)
        {
            if (spline.Count > 1) spline.RemoveAt(spline.Count - 1);
            spline.Insert(spline.Count, newKnot, TangentMode.Broken, 0.5f);
            knotQueue.Enqueue(newKnot);
        }
    }

    // ::::: Continue Spline (Intersection)
    private void HandleIntersection(Vector3 position)
    {
        if (spline.Count >= 2) spline.RemoveAt(spline.Count - 1);

        BezierKnot beforeIntersection = new BezierKnot(knotQueue.Last().Position);
        BezierKnot afterIntersection = new BezierKnot(position);

        // Direction
        Vector3 beforeDir = (afterIntersection.Position - beforeIntersection.Position);
        beforeDir = beforeDir.normalized;

        // Tangents
        float tangentWeight = 1f;
        beforeIntersection.TangentOut = beforeDir * tangentWeight;
        afterIntersection.TangentIn = -beforeDir * tangentWeight;

        spline.Insert(spline.Count, knotQueue.Dequeue(), TangentMode.Broken, 0.5f);
        spline.Insert(spline.Count, afterIntersection, TangentMode.Broken, 0.5f);
        spline.Insert(spline.Count, afterIntersection, TangentMode.Broken, 0.5f);

        knotQueue.Enqueue(afterIntersection);
    }
}