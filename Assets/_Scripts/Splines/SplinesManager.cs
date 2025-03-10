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

    public SplineContainer splineContainer;

    private Spline spline;
    private Queue<BezierKnot> knotQueue = new Queue<BezierKnot>(3);
    private Dictionary<Vector3, (Spline, int)> intersections = new Dictionary<Vector3, (Spline, int)>();
    private bool recentIntersection = false;

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

    // :::::::::: EVENT METHODS ::::::::::
    // ::::: Building Lane
    private void HandleEdgeAdded(Vector2Int firstNodePosition, Vector2Int secondNodePosition)
    {
        Vector3 firstWorldPosition = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
        Vector3 secondWorldPosition = grid.GetWorldPositionFromCellCentered(secondNodePosition.x, secondNodePosition.y);

        List<Vector2Int> firstNeighbors = graph.GetNeighborsPos(firstNodePosition);
        List<Vector2Int> secondNeighbors = graph.GetNeighborsPos(secondNodePosition);

        // Find Knot and Spline
        var (firstSpline, firstIndex) = FindKnotAndSpline(firstWorldPosition);

        // 'T' and 'Y' Intersections
        HandleTandY(firstWorldPosition);
        HandleTandY(secondWorldPosition);

        // New Spline
        if (firstSpline == null
            || (firstSpline != null && firstIndex != firstSpline.Count - 1)) // Mid Spline
        {
            knotQueue.Clear();
            StartNewSpline(firstWorldPosition, secondWorldPosition);
            SplineChanged?.Invoke();
            return;
        }

        // Continue Spline
        spline = firstSpline;

        // Check Collinearity
        bool isIntersection = false;
        foreach (var neighbor in firstNeighbors)
            if (!graph.IsCollinear(firstNodePosition, neighbor, secondNodePosition))
            {
                isIntersection = true;

                Vector3 position = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
                if (!intersections.ContainsKey(position)) intersections.Add(position, (spline, firstIndex));

                break;
            }

        if (isIntersection && !recentIntersection) HandleIntersection(secondWorldPosition);
        else AddKnotToCurrentSpline(secondWorldPosition);

        SplineChanged?.Invoke(); // !
    }

    // ::::: Destroying Lane
    private void HandleEdgeRemoved(Vector2Int firstNodePosition, Vector2Int secondNodePosition)
    {
        Vector3 firstWorldPosition = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
        Vector3 secondWorldPosition = grid.GetWorldPositionFromCellCentered(secondNodePosition.x, secondNodePosition.y);

        List<Vector2Int> secondNeighbors = graph.GetNeighborsPos(secondNodePosition);

        // Find Knot and Spline
        var (firstSpline, firstIndex) = FindKnotAndSpline(firstWorldPosition);
        var (secondSpline, secondIndex) = FindKnotAndSpline(secondWorldPosition);

        if (firstSpline == null) return;
        firstSpline.RemoveAt(firstIndex);

        // Check Collinearity
        foreach (var neighbor in secondNeighbors)
            if (!graph.IsCollinear(secondNodePosition, firstNodePosition, neighbor)
                && secondIndex == -1)
            {
                BezierKnot newKnot = new BezierKnot(secondWorldPosition);
                firstSpline.Insert(firstIndex, newKnot, TangentMode.Broken, 0.5f);
                break;
            }

        if (firstSpline.Count == 1)
            splineContainer.RemoveSpline(firstSpline);

        SplineChanged?.Invoke();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: New Spline (Limit)
    private void StartNewSpline(Vector3 positionA, Vector3 positionB)
    {
        Spline newSpline = new Spline();

        Vector3[] positions = new Vector3[] { positionA, positionB };
        foreach (Vector3 pos in positions)
        {
            BezierKnot newKnot = new BezierKnot(pos);
            newSpline.Insert(newSpline.Count, newKnot, TangentMode.Broken, 0.5f);
            if (knotQueue.Count == 3) knotQueue.Dequeue();
            knotQueue.Enqueue(newKnot);
        }

        splineContainer.AddSpline(newSpline);
        spline = newSpline;
    }

    // ::::: Continue Spline (Collinear)
    private void AddKnotToCurrentSpline(Vector3 position)
    {
        recentIntersection = false;

        BezierKnot newKnot = new BezierKnot(position);
        spline.Insert(spline.Count, newKnot, TangentMode.Broken, 0.5f);
        if (knotQueue.Count == 3) knotQueue.Dequeue();
        knotQueue.Enqueue(newKnot);
    }

    // ::::: Continue Spline (Intersection)
    private void HandleIntersection(Vector3 position)
    {
        recentIntersection = true;
        Vector3 lastPosition = spline.ElementAt(spline.Count - 1).Position;

        if (spline.Count >= 2)
            spline.RemoveAt(spline.Count - 1);

        BezierKnot beforeIntersection = new BezierKnot(knotQueue.Last().Position);
        BezierKnot afterIntersection = new BezierKnot(position);

        // Direction
        Vector3 beforeDir = (afterIntersection.Position - beforeIntersection.Position);
        beforeDir = beforeDir.normalized;

        // Tangents
        float tangentWeight = 1f;
        beforeIntersection.TangentOut = beforeDir * tangentWeight;
        afterIntersection.TangentIn = -beforeDir * tangentWeight;

        spline.Insert(spline.Count, afterIntersection, TangentMode.Broken, 0.5f);

        if (knotQueue.Count == 3) knotQueue.Dequeue();
        knotQueue.Enqueue(afterIntersection);
    }

    // ::::: 'T' and 'Y' Intersections
    private void HandleTandY(Vector3 position)
    {
        if (intersections.ContainsKey(position))
        {
            var (spline, index) = intersections[position];
            BezierKnot intersectionKnot = new BezierKnot(position);
            spline.Insert(index, intersectionKnot);
            intersections.Remove(position);
        }
    }

    // :::::::::: SUPPORT METHODS ::::::::::
    // ::::: Select the Right Spline at the Right Knot
    private (Spline foundSpline, int knotIndex) FindKnotAndSpline(Vector3 worldPosition)
    {
        foreach (var currentSpline in splineContainer.Splines)
            for (int i = 0; i < currentSpline.Count; i++)
                if (currentSpline[i].Position.Equals((float3)worldPosition))
                    return (currentSpline, i);

        return (null, -1);
    }

    // ::::: 
    private void ReverseSpline(Spline spline)
    {
        List<BezierKnot> reversedKnots = new List<BezierKnot>();

        for (int i = spline.Count - 1; i >= 0; i--)
        {
            BezierKnot knot = spline[i];

            Vector3 tempTangentIn = knot.TangentIn;
            knot.TangentIn = knot.TangentOut;
            knot.TangentOut = tempTangentIn;

            reversedKnots.Add(knot);
        }

        spline.Clear();
        foreach (BezierKnot knot in reversedKnots)
            spline.Add(knot);
    }
}