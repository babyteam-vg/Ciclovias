using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class SplinesManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;

    public SplineContainer splineContainer;
    
    private Spline spline;
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
        //graph.OnNodeAdded += HandleNodeAdded;
        graph.OnEdgeAdded += HandleEdgeAdded;
        //graph.OnNodeRemoved += HandleNodeRemoved;
        graph.OnEdgeRemoved += HandleEdgeRemoved;
    }
    private void OnDisable()
    {
        //graph.OnNodeAdded -= HandleNodeAdded;
        graph.OnEdgeAdded -= HandleEdgeAdded;
        //graph.OnNodeRemoved -= HandleNodeRemoved;
        graph.OnEdgeRemoved -= HandleEdgeRemoved;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public List<Spline> GetSplines() { return splineContainer.Splines.ToList(); }
    public Spline GetCurrentSpline() { return spline; }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Building Lane
    private void HandleEdgeAdded(Vector2Int firstNodePosition, Vector2Int secondNodePosition)
    {
        Vector3 firstWorldPosition = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
        Vector3 secondWorldPosition = grid.GetWorldPositionFromCellCentered(secondNodePosition.x, secondNodePosition.y);

        List<Vector2Int> firstNeighbors = graph.GetNeighborsPos(firstNodePosition);

        // Edge of the Lane
        if (spline.Closed || firstNeighbors.Count < 2 || !grid.IsAdjacent(previousNodePosition, firstNodePosition))
        {
            knotQueue.Clear();
            StartNewSpline(firstWorldPosition);
            previousNodePosition = secondNodePosition;
            return;
        }

        // Check Collinearity
        bool isIntersection = false;
        foreach (var neighbor in firstNeighbors)
            if (!IsCollinear(firstNodePosition, neighbor, secondNodePosition))
            {
                isIntersection = true;
                break;
            }

        if (isIntersection) HandleIntersection(secondWorldPosition);
        else AddKnotToCurrentSpline(secondWorldPosition);

        // Closed Lane
        if (secondWorldPosition == (Vector3)spline.First().Position)
        {
            spline.Closed = true;
            SplineChanged?.Invoke(); // !
            return;
        }

        previousNodePosition = secondNodePosition;
        if (knotQueue.Count == 3) knotQueue.Dequeue();
        SplineChanged?.Invoke(); // !
    }

    // ::::: Destroying Lane
    private void HandleEdgeRemoved(Vector2Int firstNodePosition, Vector2Int secondNodePosition)
    {
        Vector3 firstWorldPosition = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
        Vector3 secondWorldPosition = grid.GetWorldPositionFromCellCentered(secondNodePosition.x, secondNodePosition.y);

        List<Vector2Int> secondNeighbors = graph.GetNeighborsPos(secondNodePosition);

        int knotIndex = -1;
        for (int i = 0; i < spline.Count; i++)
            if (spline[i].Position.Equals((float3)firstWorldPosition))
            {
                knotIndex = i;
                break;
            }

        if (knotIndex == -1) return;

        bool hasBefore = knotIndex > 0;
        bool hasAfter = knotIndex < spline.Count - 1;

        if (!hasBefore && !hasAfter) // (X)
        {
            splineContainer.RemoveSpline(spline);
            return;
        }

        spline.RemoveAt(knotIndex);

        // Check Collinearity
        foreach (var neighbor in secondNeighbors)
            if (!IsCollinear(firstNodePosition, secondNodePosition, neighbor))
            {
                BezierKnot newKnot = new BezierKnot(secondWorldPosition);
                spline.Insert(knotIndex, newKnot, TangentMode.Broken, 0.5f);
                break;
            }

        if (spline.Count == 1) splineContainer.RemoveSpline(spline);
        SplineChanged?.Invoke();
    }


    // :::::::::: SUPPORT METHODS ::::::::::
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
            //if (spline.Count > 1) spline.RemoveAt(spline.Count - 1);
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

        //spline.Insert(spline.Count, knotQueue.Dequeue(), TangentMode.Broken, 0.5f);
        spline.Insert(spline.Count, afterIntersection, TangentMode.Broken, 0.5f);
        //spline.Insert(spline.Count, afterIntersection, TangentMode.Broken, 0.5f);

        knotQueue.Enqueue(afterIntersection);
    }
}