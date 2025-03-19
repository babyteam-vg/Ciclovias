using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class SplineManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TutorialManager tutorialManager;

    public SplineContainer splineContainer;
    private Spline spline;

    private bool recentIntersection = false;
    private Dictionary<Vector3, Intersection> intersections = new Dictionary<Vector3, Intersection>();

    public event Action<Spline> SplineUpdated;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        spline = splineContainer.Spline;
    }

    private void OnEnable()
    {
        graph.OnEdgeAdded += HandleEdgeAdded;
        graph.OnEdgeRemoved += HandleEdgeRemoved;
    }
    private void OnDisable()
    {
        graph.OnEdgeAdded -= HandleEdgeAdded;
        graph.OnEdgeRemoved -= HandleEdgeRemoved;
    }

    // :::::::::: EVENT METHODS ::::::::::
    // ::::: Building Lane
    private void HandleEdgeAdded(Vector2Int firstNodePosition, Vector2Int secondNodePosition)
    {
        Vector3 firstWorldPosition = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
        Vector3 secondWorldPosition = grid.GetWorldPositionFromCellCentered(secondNodePosition.x, secondNodePosition.y);

        List<Vector2Int> firstNeighbors = graph.GetNeighborsPos(firstNodePosition);
        List<Vector2Int> secondNeighbors = graph.GetNeighborsPos(secondNodePosition);

        // Find Knot and Spline
        var (firstSpline, splineIndex, firstIndex) = FindKnotAndSpline(firstWorldPosition);

        if (firstSpline == null) // Not Found
        {
            recentIntersection = false;
            StartNewSpline(firstWorldPosition, secondWorldPosition);
        }
        else // Continue Spline
        {
            if (firstIndex == 0) firstSpline.Reverse();
            spline = firstSpline;

            // Check Collinearity
            bool isIntersection = false;
            foreach (var neighbor in firstNeighbors)
                if (!graph.IsCollinear(firstNodePosition, neighbor, secondNodePosition))
                {
                    isIntersection = true;
                    break;
                }

            if (isIntersection && !recentIntersection)
            {
                recentIntersection = true;
                HandleNewIntersection(firstWorldPosition, secondWorldPosition);
            }
            else
            {
                recentIntersection = false;
                AddKnotToCurrentSpline(secondWorldPosition);
            }
        }
    }

    // ::::: Destroying Lane
    private void HandleEdgeRemoved(Vector2Int firstNodePosition, Vector2Int secondNodePosition)
    {
        Vector3 firstWorldPosition = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
        Vector3 secondWorldPosition = grid.GetWorldPositionFromCellCentered(secondNodePosition.x, secondNodePosition.y);

        // Find Knot and Spline
        var (firstSpline, splineIndex, firstIndex) = FindKnotAndSpline(firstWorldPosition);

        if (firstSpline != null)
        {
            spline = firstSpline;

            // Edges
            //if (firstIndex == 0 || firstIndex == spline.Count - 1)
            //{
            //    SplineUpdated?.Invoke(spline);
            //}

            // Intersection
            if (intersections.ContainsKey(secondWorldPosition))
            {
                recentIntersection = false;
                Intersection intersection = intersections[secondWorldPosition];   
                HandleRemoveIntersection(secondWorldPosition, intersection);
            }
            else // Straight
            {
                spline.RemoveAt(firstIndex);
                SplineUpdated?.Invoke(spline);
            }
        }

        if (spline.Count == 1) splineContainer.RemoveSpline(spline);
    }

    // :::::::::: SPLINE METHODS ::::::::::
    // ::::: New Spline (Edge)
    private void StartNewSpline(Vector3 from, Vector3 to)
    {
        Spline newSpline = new Spline();

        Vector3[] positions = new Vector3[] { from, to };
        foreach (Vector3 pos in positions)
        {
            BezierKnot newKnot = new BezierKnot(pos);
            newSpline.Insert(newSpline.Count, newKnot, TangentMode.Broken, 0.5f);
        }

        splineContainer.AddSpline(newSpline);
        SplineUpdated?.Invoke(newSpline);

        spline = newSpline;
    }

    // ::::: Continue Spline (Collinear)
    private void AddKnotToCurrentSpline(Vector3 position, int? index = null)
    {
        int i = index.HasValue ? index.Value : spline.Count;

        BezierKnot newKnot = new BezierKnot(position);
        spline.Insert(i, newKnot, TangentMode.Broken, 0.5f);
        SplineUpdated?.Invoke(spline);
    }

    // ::::: Continue Spline (Intersection)
    private void HandleNewIntersection(Vector3 intersection, Vector3 to)
    {
        Vector3 from = spline.ElementAt(spline.Count - 2).Position; // * Original Straight Spline
        BezierKnot fromKnot = new BezierKnot(from);

        Vector3 intersectionPosition = spline.ElementAt(spline.Count - 1).Position; // * Original Straight Spline
        BezierKnot intersectionKnot = new BezierKnot(intersectionPosition);
        spline.RemoveAt(spline.Count - 1);
        SplineUpdated?.Invoke(spline);

        Spline intersectionSpline = new Spline(); // ** New Intersection Spline  
        intersectionSpline.Insert(intersectionSpline.Count, fromKnot, TangentMode.Broken, 0.5f);

        BezierKnot toKnot = new BezierKnot(to);
        
        Spline newSpline = new Spline(); // *** New Straight Spline
        newSpline.Insert(newSpline.Count, toKnot, TangentMode.Broken, 0.5f);

        // Tangents
        float tangentWeight = 1f;
        Vector3 dir = (to - intersection).normalized;
        toKnot.TangentIn = -dir * tangentWeight;

        intersectionSpline.Insert(intersectionSpline.Count, toKnot, TangentMode.Broken, 0.5f);  // ** New Intersection Spline  
        splineContainer.AddSpline(intersectionSpline);
        SplineUpdated?.Invoke(intersectionSpline);

        // Intersections
        if (!intersections.ContainsKey(intersectionPosition))
            intersections.Add(
                intersectionPosition,
                new Intersection
                {
                    spline = intersectionSpline,
                    from = (from, spline),
                    to = (to, newSpline)
                });

        splineContainer.AddSpline(newSpline); // *** New Straight Spline
    }

    // :::::
    private void HandleRemoveIntersection(Vector3 position, Intersection intersection)
    {
        Spline intersectionSpline = intersection.spline;
        (Vector3 from, Spline fromSpline) = intersection.from;
        (Vector3 to, Spline toSpline) = intersection.to;

        // Erase Intersection Spline
        intersectionSpline.Clear();
        SplineUpdated?.Invoke(intersectionSpline);
        splineContainer.RemoveSpline(intersectionSpline);

        // Intersections
        intersections.Remove(position);

        // Modify Edge Spline
        BezierKnot newKnot = new BezierKnot(position);

        if (fromSpline.Count > 1)
        {
            fromSpline.Insert(fromSpline.Count, newKnot, TangentMode.Broken, 0.5f);
            SplineUpdated?.Invoke(fromSpline);
        }
        else if (toSpline.Count > 1)
        {
            toSpline.Reverse();
            toSpline.Insert(toSpline.Count, newKnot, TangentMode.Broken, 0.5f);
            SplineUpdated?.Invoke(toSpline);
        }
    }

    // :::::::::: SUPPORT METHODS ::::::::::
    // ::::: Select the Right Spline at the Right Knot
    private (Spline foundSpline, int splineIndex, int knotIndex) FindKnotAndSpline(Vector3 worldPosition)
    {
        var sortedSplines = splineContainer.Splines.OrderBy(spline => spline.Count);

        int i = 0;
        foreach (var currentSpline in sortedSplines) // Splines
        {
            for (int j = 0; j < currentSpline.Count; j++) // Spline's Knots
                if (currentSpline[j].Position.Equals((float3)worldPosition))
                    return (currentSpline, i, j);
            i++;
        }                  

        return (null, -1, -1);
    }
}

[System.Serializable]
public class Intersection
{
    public Spline spline;
    public (Vector3, Spline) from;
    public (Vector3, Spline) to;
}