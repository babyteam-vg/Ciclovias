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

    private bool recentIntersection = false;
    private Dictionary<Vector3, Intersection> intersections = new Dictionary<Vector3, Intersection>();

    public event Action<Spline> SplineUpdated;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
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

        // Check Collinearity
        bool isIntersection = false;
        foreach (Vector2Int neighbor in firstNeighbors)
            if (!graph.IsCollinear(neighbor, firstNodePosition, secondNodePosition))
            {
                isIntersection = true;
                break;
            }

        // Find Knot and Spline
        (Spline spline, int index) = FindKnotAndSpline(firstWorldPosition);

        if (spline == null) // Spline Not Found
            StartNewSpline(firstWorldPosition, secondWorldPosition);
        else // Spline Found...
        {
            if (IsIntersectionSpline(spline)) // ...was an Intersection Spline
            {
                //recentIntersection = false;
                StartNewSpline(firstWorldPosition, secondWorldPosition);
            }
            else // ...was a Straight Spline, Continue the Spline...
            {
                if (isIntersection && !recentIntersection) // ...as Intersection
                {
                    if (firstNeighbors.Count > 2) ExpandIntersection(spline, firstWorldPosition, secondWorldPosition, firstNeighbors);
                    else CreateIntersection(spline, index, firstWorldPosition, secondWorldPosition);
                }
                else // ...as Straight
                    AddKnotToSpline(spline, index, secondWorldPosition);
            }
        }
    }

    // ::::: Destroying Lane
    private void HandleEdgeRemoved(Vector2Int firstNodePosition, Vector2Int secondNodePosition)
    {
        Vector3 firstWorldPosition = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
        Vector3 secondWorldPosition = grid.GetWorldPositionFromCellCentered(secondNodePosition.x, secondNodePosition.y);

        // Find Knot and Spline
        (Spline spline, int index) = FindKnotAndSpline(firstWorldPosition);

        if (spline != null) // Destroying a Spline...
        {
            BezierKnot newKnot = GeneratePreviousKnot(spline, index);

            if (intersections.ContainsKey(secondWorldPosition)) // ...in an Intersection
            {
                Intersection intersection = intersections[secondWorldPosition];
                RemoveIntersection(index, secondWorldPosition, intersection);
            }
            else if (!newKnot.Equals(spline.ElementAt(1 - index))) // ...in a Straight Line
                AddKnotToSpline(spline, index, newKnot.Position);
            else spline.RemoveAt(index);

            // Remove Lonely Splines
            if (spline.Count < 2)
            {
                spline.Clear();
                SplineUpdated?.Invoke(spline);
                splineContainer.RemoveSpline(spline);
            }
        }
    }

    // :::::::::: SPLINE METHODS ::::::::::
    // ::::: Create a New Spline
    private void StartNewSpline(Vector3 from, Vector3 to)
    {
        recentIntersection = false;

        Spline newSpline = new Spline();

        Vector3[] positions = new Vector3[] { from, to };
        foreach (Vector3 pos in positions)
        {
            BezierKnot newKnot = new BezierKnot(pos);
            newSpline.Insert(newSpline.Count, newKnot, TangentMode.Broken, 0.5f);
        }

        splineContainer.AddSpline(newSpline);
        SplineUpdated?.Invoke(newSpline);
    }

    // ::::: Continue a Spline (Straight)
    private void AddKnotToSpline(Spline spline, int index, Vector3 position)
    {
        recentIntersection = false;
        spline.RemoveAt(index);
        BezierKnot newKnot = new BezierKnot(position);
        spline.Insert(index, newKnot, TangentMode.Broken, 0.5f);
        SplineUpdated?.Invoke(spline);
    }

    // ::::: Continue a Spline (Simple Intersection)
    private void CreateIntersection(Spline spline, int index, Vector3 intersection, Vector3 to)
    {
        recentIntersection = true;

        BezierKnot fromKnot = GeneratePreviousKnot(spline, index);
        BezierKnot intersectionKnot = new BezierKnot(intersection);
        BezierKnot toKnot = new BezierKnot(to);

        // Original Straight Spline
        spline.Remove(intersectionKnot);
        if (!fromKnot.Equals(spline.ElementAt(1 - index)))
            spline.Insert(index, fromKnot, TangentMode.Broken, 0.5f);
        if (spline.Count < 2)
        {
            spline.Clear();
            SplineUpdated?.Invoke(spline);
            splineContainer.RemoveSpline(spline);
        }
        else SplineUpdated?.Invoke(spline);

        // New Intersection Spline
        Spline intersectionSpline = new Spline();
        intersectionSpline.Insert(0, fromKnot, TangentMode.Broken, 0.5f);

        float tangentWeight = 1f; // Tangents
        Vector3 dir = (to - intersection).normalized;
        toKnot.TangentIn = -dir * tangentWeight;

        intersectionSpline.Insert(1, toKnot, TangentMode.Broken, 0.5f);
        splineContainer.AddSpline(intersectionSpline);
        SplineUpdated?.Invoke(intersectionSpline);

        // Intersection Dictionary
        intersections.Add(
            intersection,
            new Intersection
            {
                spline = intersectionSpline,
                edges = new List<Vector3>
                {
                    fromKnot.Position,
                    to
                },
            });
    }

    // ::::: Continue Spline (Multi Intersection)
    private void ExpandIntersection(Spline spline, Vector3 intersection, Vector3 to, List<Vector2Int> firstNeighbors)
    {

    }

    // ::::: Handle the Removal of an Intersection and Its Splines
    private void RemoveIntersection(int index, Vector3 intersectionPosition, Intersection intersection)
    {
        // Modify Edge Splines
        bool edgeSplineFound = false;
        foreach (Vector3 edgePos in intersection.edges)
        {
            (Spline edgeSpline, int edgeIndex) = FindKnotAndSpline(edgePos);

            if (edgeSpline != null // Spline Still Exists and...
                && !IsIntersectionSpline(edgeSpline)) // ...Isn't the Intersection Spline
            {
                edgeSplineFound = true;
                AddKnotToSpline(edgeSpline, edgeIndex, intersectionPosition);
            }
        }

        // Erase Intersection Spline
        Spline intersectionSpline = intersection.spline;
        intersectionSpline.Clear();
        SplineUpdated?.Invoke(intersectionSpline);

        // No Edge Splines = Intersection Spline w/o Straight Connections
        if (edgeSplineFound) splineContainer.RemoveSpline(intersectionSpline);
        else
        {
            Vector3 position = intersections[intersectionPosition].edges.ElementAt(1 - index);
            BezierKnot newKnot = new BezierKnot(position);
            BezierKnot intersectionKnot = new BezierKnot(intersectionPosition);

            intersectionSpline.Insert(0, intersectionKnot, TangentMode.Broken, 0.5f);
            intersectionSpline.Insert(1, newKnot, TangentMode.Broken, 0.5f);

            SplineUpdated?.Invoke(intersectionSpline);
        }

        intersections.Remove(intersectionPosition); // Intersections
    }

    // ::::: Reduce the Dimension of an Intersection and Its Splines
    private void ReduceIntersection(Vector3 removedPosition, Vector3 intersectionPosition, Intersection intersection)
    {
        // * Intersection Spline

        // ** Modify Edge Splines
        //BezierKnot newKnot = new BezierKnot(intersectionPosition);
        //foreach (Vector3 edgePos in intersection.edges)
        //{
        //    (Spline edgeSpline, int index) = FindKnotAndSpline(edgePos);

        //    if (edgeSpline != null) // Spline Still Exists
        //    {
        //        BezierKnot lastKnot = new BezierKnot(edgePos);
        //        edgeSpline.Remove(lastKnot);
        //        if (index == 1) edgeSpline.Insert(1, newKnot, TangentMode.Broken, 0.5f);
        //        else if (index == 0) edgeSpline.Insert(0, newKnot, TangentMode.Broken, 0.5f);
        //        SplineUpdated?.Invoke(edgeSpline);
        //    }
        //}
    }

    // :::::::::: SUPPORT METHODS ::::::::::
    private bool IsIntersectionSpline(Spline spline)
    {
        return (Vector3)spline.ElementAt(0).TangentIn != Vector3.zero
                || (Vector3)spline.ElementAt(1).TangentIn != Vector3.zero;
    }

    // ::::: Recalculate the Edge of a Spline
    private BezierKnot GeneratePreviousKnot(Spline spline, int index = 1)
    {
        Vector3 first = index == 1 ? spline.ElementAt(0).Position : spline.ElementAt(1).Position;
        Vector3 last = index == 1 ? spline.ElementAt(1).Position : spline.ElementAt(0).Position;

        Vector3 direction = (last - first).normalized;
        Vector2Int? previousCell = grid.GetCellFromWorldPosition(last - direction);

        if (previousCell.HasValue)
        {
            Vector3 previousPosition = grid.GetWorldPositionFromCellCentered(previousCell.Value.x, previousCell.Value.y);
            return new BezierKnot(previousPosition);
        }
        else return new BezierKnot(last);
    }

    // ::::: Find a Spline and Knot Index
    private (Spline, int) FindKnotAndSpline(Vector3 worldPosition)
    {
        foreach (Spline spline in splineContainer.Splines) // Splines
        {
            if (spline.ElementAt(0).Position.Equals(worldPosition)) return (spline, 0);
            if (spline.ElementAt(1).Position.Equals(worldPosition)) return (spline, 1);
        }

        return (null, -1);
    }

    // ::::: Spliting a Spline in Two
    private void SplitSpline(Spline splitedSpline, int splitIndex)
    {
        Spline newSpline = new Spline();

        //for (int i = splitIndex; i < splitedSpline.Count; i++) // ** New Spline
        //    newSpline.Insert(newSpline.Count, splitedSpline[i], TangentMode.Broken, 0.5f);

        //for (int i = splitedSpline.Count - 1; i >= splitIndex; i--) // * Original Spline
        //    currentSpline.RemoveAt(i);

        //splineContainer.AddSpline(newSpline);

        //SplineUpdated?.Invoke(currentSpline);
        //SplineUpdated?.Invoke(newSpline);
    }
}

[System.Serializable]
public class Intersection
{
    public Spline spline;
    public List<Vector3> edges;
}