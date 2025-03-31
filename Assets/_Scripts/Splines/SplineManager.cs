using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class SplineManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TutorialManager tutorialManager;

    public SplineContainer splineContainer;

    //[SerializeField] private List<IntersectionEntry> intersectionsList = new List<IntersectionEntry>();
    private Dictionary<Vector3, Intersection> intersections = new Dictionary<Vector3, Intersection>();
    private HashSet<Spline> sealedSplines = new HashSet<Spline>();

    public event Action<Spline> SplineUpdated;
    public event Action<Spline> SplineSealed;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
    }

    private void OnEnable()
    {
        graph.EdgeAdded += HandleEdgeAdded;
        graph.EdgeRemoved += HandleEdgeRemoved;

        taskManager.TaskSealed += OnTaskSealed;
        tutorialManager.TutorialSectionSealed += OnTutorialSealed;
    }
    private void OnDisable()
    {
        graph.EdgeAdded -= HandleEdgeAdded;
        graph.EdgeRemoved -= HandleEdgeRemoved;

        taskManager.TaskSealed += OnTaskSealed;
        tutorialManager.TutorialSectionSealed += OnTutorialSealed;
    }

    //private void Update()
    //{
    //    intersectionsList.Clear();
    //    foreach (var kvp in intersections)
    //    {
    //        intersectionsList.Add(new IntersectionEntry(kvp.Key, kvp.Value));
    //    }
    //}

    // :::::::::: EVENT METHODS ::::::::::
    // ::::: Building Lane
    private void HandleEdgeAdded(Vector2Int firstNodePosition, Vector2Int secondNodePosition)
    {
        Node firstNode = graph.GetNode(firstNodePosition);

        Vector3 firstWorldPosition = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
        Vector3 secondWorldPosition = grid.GetWorldPositionFromCellCentered(secondNodePosition.x, secondNodePosition.y);

        List<Vector2Int> firstNeighbors = graph.GetNeighborsPos(firstNodePosition);
        List<Vector2Int> secondNeighbors = graph.GetNeighborsPos(secondNodePosition);

        // Check Collinearity
        bool isFirstIntersection = false;
        foreach (Vector2Int neighbor in firstNeighbors)
            if (!graph.IsCollinear(neighbor, firstNodePosition, secondNodePosition))
            {
                isFirstIntersection = true;
                break;
            }

        bool isSecondIntersection = false;       
        foreach (Vector2Int neighbor in secondNeighbors.Where(n => n != firstNodePosition))
            if (!graph.IsCollinear(firstNodePosition, secondNodePosition, neighbor))
            {
                isSecondIntersection = true;
                break;
            }

        // Find Knot and Spline
        (Spline spline, int index) = FindKnotAndSpline(firstWorldPosition);

        if (spline == null) // Spline Not Found...
        {
            if (intersections.ContainsKey(firstWorldPosition)) // ...From an Intersection
            {
                if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection
                {
                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > Double Expansion");

                    Intersection intersection1 = intersections[firstWorldPosition];
                    ExpandIntersection(secondWorldPosition, firstWorldPosition, intersection1);

                    Intersection intersection2 = intersections[secondWorldPosition];
                    ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection2);
                }
                else
                {
                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > Expansion (From)");
                    Intersection intersection = intersections[firstWorldPosition];
                    ExpandIntersection(secondWorldPosition, firstWorldPosition, intersection);
                }
            }
            else if (intersections.ContainsKey(secondWorldPosition))
            {
                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > Expansion (Into)");
                Intersection intersection = intersections[secondWorldPosition];
                ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
            }
            else
            {
                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > New Spline");
                Spline newSpline = StartNewSpline(firstWorldPosition, secondWorldPosition);

                if (isSecondIntersection) // ...and Adapt 2 an Intersection
                {
                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > Intersection (Fusion)");
                    IntersectionFusing2StraighSplines(newSpline, 1, firstWorldPosition, secondWorldPosition);
                }
            }
        }
        else // Spline Found, Building...
        {
            if (IsIntersectionSpline(spline)) // ...from an Intersection Spline to...
            {
                if (intersections.ContainsKey(secondWorldPosition)) // ...an Intersection
                {
                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From an Intersection > Expansion (Into)");
                    Intersection intersection = intersections[secondWorldPosition];
                    ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
                }
                else // ...a New Spline
                {
                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From an Intersection > New Spline");
                    StartNewSpline(firstWorldPosition, secondWorldPosition);
                }
            }
            else // ...from a Straight Spline to...
            {
                if (intersections.ContainsKey(secondWorldPosition)) // ...an Intersection
                {
                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From a Straight > Expansion (Into)");
                    Intersection intersection = intersections[secondWorldPosition];
                    ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
                }
                else // ...an Empty Space...
                {
                    if (isFirstIntersection || isSecondIntersection)
                    {
                        if (isFirstIntersection) // ...and Form an Intersection
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Intersection");
                            IntersectionFromStraightSpline(spline, index, firstWorldPosition, secondWorldPosition);
                        }
                        if (isSecondIntersection)
                        {
                            if (secondNeighbors.Count < 3) // ...and Adapt 2 an Intersection
                            {
                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Intersection (Fusion)");
                                IntersectionFusing2StraighSplines(spline, index, firstWorldPosition, secondWorldPosition);
                            }
                            else
                            {
                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Continue");
                                TripleIntersectionCrossing2StraightSplines(spline, index, firstWorldPosition, secondWorldPosition);
                            }
                        }
                    }
                    else // ...and Add a Knot
                    {
                        if (firstNode.indestructible)
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Sealed");
                            StartNewSpline(firstWorldPosition, secondWorldPosition);
                        }
                        else
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > Straight Spline > Continue");
                            AddKnotToSpline(spline, index, secondWorldPosition);
                        }
                    }   
                }
            }
        }
    }

    // ::::: Destroying Lane
    private void HandleEdgeRemoved(Vector2Int firstNodePosition, Vector2Int secondNodePosition)
    {
        Vector3 firstWorldPosition = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
        Vector3 secondWorldPosition = grid.GetWorldPositionFromCellCentered(secondNodePosition.x, secondNodePosition.y);

        if (intersections.ContainsKey(firstWorldPosition)) // Remove an Intersection...
        {
            Node intersectionNode = graph.GetNode(firstNodePosition);
            Intersection intersection = intersections[firstWorldPosition];

            if (intersection.edges.Count > 2) // ...Triple
            {
                //Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Splited in Triple Intersection");
                SplitTripleIntersection(firstWorldPosition, intersection);
            }
            else // ...Simple
            {
                //Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Splited in Intersection");
                SplitIntersection(firstWorldPosition, intersection);
            }
        }
        else
        {
            // Find Knot and Spline
            (Spline spline, int index) = FindKnotAndSpline(firstWorldPosition);

            if (spline != null) // Destroying a Spline...
            {
                if (index == -1) // ...by the Middle
                {
                    //Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Splited");
                    SplitStraightSpline(spline, firstWorldPosition);
                }
                else // ...by the Edges
                {
                    BezierKnot previousKnot = GeneratePreviousKnot(spline, index);

                    if (intersections.ContainsKey(secondWorldPosition)) // ...in an Intersection
                    {
                        Intersection intersection = intersections[secondWorldPosition];
                        if (IsIntersectionSpline(spline))
                        {
                            //Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Intersection (Simple) Removed");
                            RemoveIntersection(index, secondWorldPosition, intersection);
                        }
                        else
                        {
                            //Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Intersection (Triple) Removed");
                            ReduceTripleIntersection(spline, firstWorldPosition, secondWorldPosition, intersection);
                        }
                    }
                    else
                    {
                        //Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Reduced or Removed");
                        RemoveKnotToSpline(spline, index, previousKnot.Position);
                    }
                }
            }
        }
    }

    // :::::::::: SPLINE (FUNDAMENTAL) METHODS ::::::::::
    // ::::: Spawn a New Spline
    private Spline StartNewSpline(Vector3 from, Vector3 to)
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

        return newSpline;
    }

    // ::::: Continue a Straight Spline
    private void AddKnotToSpline(Spline spline, int index, Vector3 position, bool isRemoving = false)
    {
        if (!isRemoving) spline.RemoveAt(index);
        BezierKnot newKnot = new BezierKnot(position);
        spline.Insert(index, newKnot, TangentMode.Broken, 0.5f);
        SplineUpdated?.Invoke(spline);
    }

    // ::::: Reduce or Remove a Spline
    private void RemoveKnotToSpline(Spline spline, int index, Vector3 position)
    {
        spline.RemoveAt(index);
        BezierKnot knot = new BezierKnot(position);
        if (!knot.Equals(spline.ElementAt(0)))
            AddKnotToSpline(spline, index, position, true);
        if (spline.Count < 2)
        {
            spline.Clear();
            splineContainer.RemoveSpline(spline);
        }
        SplineUpdated?.Invoke(spline);
    }

    // ::::: Spawn the Intesection Spline
    private Spline CreateIntersection(BezierKnot fromKnot, BezierKnot toKnot, Vector3 intersectionPosition)
    {
        Vector3 to = toKnot.Position;

        Spline intersectionSpline = new Spline();
        intersectionSpline.Insert(0, fromKnot, TangentMode.Broken, 0.5f);

        float tangentWeight = 1f; // Tangents
        Vector3 dir = (to - intersectionPosition).normalized;
        toKnot.TangentIn = -dir * tangentWeight;

        intersectionSpline.Insert(1, toKnot, TangentMode.Broken, 0.5f);
        splineContainer.AddSpline(intersectionSpline);
        SplineUpdated?.Invoke(intersectionSpline);

        // Intersection Dictionary
        if (!intersections.ContainsKey(intersectionPosition))
            intersections.Add(
                intersectionPosition,
                new Intersection
                {
                    spline = intersectionSpline,
                    edges = new List<Vector3>
                    {
                        fromKnot.Position,
                        to
                    },
                });

        return intersectionSpline;
    }

    // ::::: Expand the Intesection and Its Edges
    private void ExpandIntersection(Vector3 newEdgePosition, Vector3 intersectionPosition, Intersection intersection)
    {
        // Erase Intersection Spline
        Spline intersectionSpline = intersection.spline;
        intersectionSpline.Clear();
        SplineUpdated?.Invoke(intersectionSpline);
        splineContainer.RemoveSpline(intersectionSpline);

        // Intesections Dictionary
        intersection.spline = null;
        intersection.edges.Add(newEdgePosition);

        // Modify Edge Splines
        foreach (Vector3 edgePos in intersection.edges)
        {
            (Spline edgeSpline, int edgeIndex) = FindKnotAndSpline(edgePos);

            if (edgeSpline != null) // Spline Still Exists
                AddKnotToSpline(edgeSpline, edgeIndex, intersectionPosition);
            else StartNewSpline(edgePos, intersectionPosition);
        }
    }

    // :::::::::: SPLINE (BUILDING) METHODS ::::::::::
    // ::::: Create an Intersection From a Straight Spline
    private void IntersectionFromStraightSpline(Spline spline, int index, Vector3 intersectionPosition, Vector3 to)
    {
        BezierKnot fromKnot = GeneratePreviousKnot(spline, index);
        BezierKnot toKnot = new BezierKnot(to);

        // Original Straight Spline
        spline.RemoveAt(index);
        if (!fromKnot.Equals(spline.ElementAt(0)))
            spline.Insert(index, fromKnot, TangentMode.Broken, 0.5f);
        if (spline.Count < 2)
        {
            spline.Clear();
            splineContainer.RemoveSpline(spline);
        }
        SplineUpdated?.Invoke(spline);

        // New Intersection Spline
        CreateIntersection(fromKnot, toKnot, intersectionPosition);
    }

    // ::::: Create an Intersection by Fusing 2 Straight Splines
    private void IntersectionFusing2StraighSplines(Spline spline, int index, Vector3 from, Vector3 intersectionPosition)
    {
        // Original From Spline
        BezierKnot fromKnot = new BezierKnot(from);
        RemoveKnotToSpline(spline, index, from);

        // Original To Spline
        (Spline toSpline, int toIndex) = FindKnotAndSpline(intersectionPosition);
        BezierKnot toKnot = GeneratePreviousKnot(toSpline, toIndex);
        RemoveKnotToSpline(toSpline, toIndex, toKnot.Position);

        // New Intersection Spline
        CreateIntersection(fromKnot, toKnot, intersectionPosition);
    }

    // ::::: 
    private void TripleIntersectionCrossing2StraightSplines(Spline spline, int index, Vector3 newEdgePosition, Vector3 intersectionPosition)
    {
        (Spline crossedSpline, int crossedIndex) = FindKnotAndSpline(intersectionPosition);
        (Spline firstSpline, Spline secondSpline) = SplitStraightSpline(crossedSpline, intersectionPosition);

        Vector3 firstEdge = firstSpline.ElementAt(1).Position;
        Vector3 secondEdge = secondSpline.ElementAt(0).Position;

        // Expand Edge Splines
        AddKnotToSpline(spline, index, intersectionPosition);
        AddKnotToSpline(firstSpline, 1, intersectionPosition);
        AddKnotToSpline(secondSpline, 0, intersectionPosition);

        // Intersection Dictionary
        if (!intersections.ContainsKey(intersectionPosition))
            intersections.Add(
                intersectionPosition,
                new Intersection
                {
                    spline = null,
                    edges = new List<Vector3>
                    {
                        newEdgePosition,
                        firstEdge,
                        secondEdge
                    },
                });
    }

    // :::::::::: SPLINE (DESTRUCTION) METHODS ::::::::::
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

        // Intersection Dictionary
        intersections.Remove(intersectionPosition);
    }

    // ::::: Triple Intersection -> Simple Intersection
    private void ReduceTripleIntersection(Spline spline, Vector3 removedPosition, Vector3 intersectionPosition, Intersection intersection)
    {
        // Erase Intersection Spline
        spline.Clear();
        SplineUpdated?.Invoke(spline);
        splineContainer.RemoveSpline(spline);

        intersections[intersectionPosition].edges.Remove(removedPosition);

        // Modify Edge Splines
        foreach (Vector3 edgePos in intersection.edges)
        {
            (Spline edgeSpline, int edgeIndex) = FindKnotAndSpline(intersectionPosition);

            if (edgeSpline != null) // Spline Still Exists
            {
                BezierKnot previousKnot = GeneratePreviousKnot(edgeSpline, edgeIndex);
                RemoveKnotToSpline(edgeSpline, edgeIndex, previousKnot.Position);
            }
        }

        // New Intersection Spline
        Vector3 from = intersections[intersectionPosition].edges[0];
        BezierKnot fromKnot = new BezierKnot(from);

        Vector3 to = intersections[intersectionPosition].edges[1];
        BezierKnot toKnot = new BezierKnot(to);

        // Intersections Dictionary
        intersection.spline = CreateIntersection(fromKnot, toKnot, intersectionPosition);
    }

    // ::::: Spliting a Spline in Two
    private (Spline firstSpline, Spline secondSpline) SplitStraightSpline(Spline splitedSpline, Vector3 splitPosition)
    {
        // Original Spline
        Vector3 originalStart = splitedSpline.ElementAt(0).Position;
        Vector3 originalEnd = splitedSpline.ElementAt(1).Position;
        splitedSpline.Clear();
        SplineUpdated?.Invoke(splitedSpline);
        splineContainer.RemoveSpline(splitedSpline);

        // First New Spline (Edge = 1)
        Spline firstSpline = StartNewSpline(originalStart, splitPosition);
        BezierKnot firstKnot = GeneratePreviousKnot(firstSpline, 1);
        firstSpline.RemoveAt(1);
        firstSpline.Insert(1, firstKnot, TangentMode.Broken, 0.5f);
        SplineUpdated?.Invoke(firstSpline);
        splineContainer.AddSpline(firstSpline);

        // Second New Spline (Edge = 0)
        Spline secondSpline = StartNewSpline(splitPosition, originalEnd);
        BezierKnot secondKnot = GeneratePreviousKnot(secondSpline, 0);
        secondSpline.RemoveAt(0);
        secondSpline.Insert(0, secondKnot, TangentMode.Broken, 0.5f);
        SplineUpdated?.Invoke(secondSpline);
        splineContainer.AddSpline(secondSpline);

        return (firstSpline, secondSpline);
    }

    // ::::: Splitting in an Intersection
    private void SplitIntersection(Vector3 splitPosition, Intersection intersection)
    {
        // Intersection Spline
        Spline intersectionSpline = intersection.spline;
        intersectionSpline.Clear();
        SplineUpdated(intersectionSpline);
        splineContainer.RemoveSpline(intersectionSpline);


        // Intersections Dictionary
        intersections.Remove(splitPosition);
    }

    // ::::: Splitting in a Triple Intersection
    private void SplitTripleIntersection(Vector3 splitPosition, Intersection intersection)
    {
        // Modify Edge Splines
        foreach (Vector3 edgePos in intersection.edges)
        {
            (Spline edgeSpline, int edgeIndex) = FindKnotAndSpline(splitPosition);

            if (edgeSpline != null) // Spline Still Exists
            {
                BezierKnot previousKnot = GeneratePreviousKnot(edgeSpline, edgeIndex);
                RemoveKnotToSpline(edgeSpline, edgeIndex, previousKnot.Position);
            }
        }

        // Intersection Dictionary
        intersections.Remove(splitPosition);
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
    private (Spline, int) FindKnotAndSpline(Vector3 worldPosition, float threshold = 0.1f)
    {
        foreach (Spline spline in splineContainer.Splines)
        {
            if (spline.Count > 1)
            {
                Vector3 p0 = spline.ElementAt(0).Position;
                Vector3 p1 = spline.ElementAt(1).Position;

                // At the Edges
                if (p0.Equals(worldPosition)) return (spline, 0);
                if (p1.Equals(worldPosition)) return (spline, 1);

                // In the Midle
                float distance = DistanceToSegment(worldPosition, p0, p1);
                if (distance < threshold) return (spline, -1);
            }
        }
        return (null, -1);
    }

    // ::::: Distance Point-Segment
    private float DistanceToSegment(Vector3 point, Vector3 p0, Vector3 p1)
    {
        Vector3 v0 = p1 - p0;
        Vector3 v1 = point - p0;
        float t = Mathf.Clamp(Vector3.Dot(v1, v0) / v0.sqrMagnitude, 0f, 1f);
        Vector3 projection = p0 + t * v0;
        return Vector3.Distance(point, projection);
    }

    // ::::: Print All Edges From an Intersection
    public void DebugIntersections()
    {
        foreach (var kvp in intersections)
        {
            Vector3 position = kvp.Key;
            Intersection intersection = kvp.Value;

            Debug.Log($"Intersection {position}");

            if (intersection.edges != null && intersection.edges.Count > 0)
            {
                for (int i = 0; i < intersection.edges.Count; i++)
                    Debug.Log($"  Edge {i}: {intersection.edges[i]}");
            }
            else Debug.Log("  N/E");
        }
    }

    // ::::: Print All Sealed Splines
    private void DebugSealedSplines()
    {
        if (sealedSplines == null || sealedSplines.Count == 0)
        {
            Debug.Log("No splines in sealedSplines.");
            return;
        }

        int splineIndex = 0;
        foreach (Spline spline in sealedSplines)
        {
            if (spline == null)
            {
                Debug.Log($"Spline {splineIndex} is null");
                splineIndex++;
                continue;
            }

            Debug.Log($"Spline {splineIndex}: {(Vector3)spline.ElementAt(0).Position} - {(Vector3)spline.ElementAt(1).Position}");

            splineIndex++;
        }
    }

    // :::::::::: SEALING METHODS ::::::::::
    // ::::: Task Sealed
    private void OnTaskSealed(Task task)
    {
        List<Vector2Int> path = task.path;
        foreach (Vector2Int pos in path)
        {
            Vector3 worldPos = grid.GetWorldPositionFromCellCentered(pos.x, pos.y);
            (Spline spline, int index) = FindKnotAndSpline(worldPos);
            if (spline != null) SealSpline(spline);
        }
    }

    // ::::: Tutorial Section Completed
    private void OnTutorialSealed(List<Vector2Int> path)
    {
        foreach (Vector2Int pos in path)
        {
            Vector3 worldPos = grid.GetWorldPositionFromCellCentered(pos.x, pos.y);
            (Spline spline, int index) = FindKnotAndSpline(worldPos);
            if (spline != null) SealSpline(spline);
        }
    }

    public void SealSpline(Spline spline)
    {
        if (sealedSplines.Add(spline))
            SplineSealed?.Invoke(spline);
    }

    public bool IsSplineSealed(Spline spline) { return sealedSplines.Contains(spline); }

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Spline -> SplineData
    public SplineData SaveSplines()
    {
        SplineData data = new SplineData();

        // Splines
        foreach (var spline in splineContainer.Splines)
        {
            SplineData.SerializableSpline splineData = new SplineData.SerializableSpline
            {
                isSealed = IsSplineSealed(spline)
            };

            foreach (var knot in spline)
            {
                SplineData.SerializableKnot knotData = new SplineData.SerializableKnot
                {
                    position = knot.Position,
                    tangentIn = knot.TangentIn,
                    tangentOut = knot.TangentOut
                };
                splineData.knots.Add(knotData);
            }

            data.splines.Add(splineData);
        }

        // Intersections
        foreach (var kvp in intersections)
        {
            var position = kvp.Key;
            var intersection = kvp.Value;

            var intersectionData = new SplineData.SerializableIntersection
            {
                spline = new SplineData.SerializableSpline(),
                edges = intersection.spline != null ? new List<Vector3>(intersection.edges) : new List<Vector3>()
            };

            if (intersection.spline != null)
                foreach (var knot in intersection.spline)
                {
                    SplineData.SerializableKnot knotData = new SplineData.SerializableKnot
                    {
                        position = knot.Position,
                        tangentIn = knot.TangentIn,
                        tangentOut = knot.TangentOut
                    };
                    intersectionData.spline.knots.Add(knotData);
                }

            data.intersections.Add(new SplineData.IntersectionData
            {
                position = position,
                intersection = intersectionData
            });
        }

        return data;
    }

    // ::::: SplineData -> Spline
    public void LoadSplines(SplineData data)
    {
        // Splines
        foreach (var splineData in data.splines)
        {
            Spline newSpline = new Spline();

            foreach (var knotData in splineData.knots)
            {
                BezierKnot knot = new BezierKnot(knotData.position)
                {
                    TangentIn = knotData.tangentIn,
                    TangentOut = knotData.tangentOut
                };
                newSpline.Insert(newSpline.Count, knot, TangentMode.Broken, 0.5f);
            }

            splineContainer.AddSpline(newSpline);

            if (splineData.isSealed)
                SealSpline(newSpline);
            else SplineUpdated?.Invoke(newSpline);
        }

        // Intersections
        intersections.Clear();

        foreach (var intersectionData in data.intersections)
        {
            Intersection intersection = new Intersection
            {
                spline = new Spline(),
                edges = new List<Vector3>(intersectionData.intersection.edges)
            };

            foreach (var knotData in intersectionData.intersection.spline.knots)
            {
                BezierKnot knot = new BezierKnot(knotData.position)
                {
                    TangentIn = knotData.tangentIn,
                    TangentOut = knotData.tangentOut
                };
                intersection.spline.Insert(intersection.spline.Count, knot, TangentMode.Broken, 0.5f);
            }

            intersections[intersectionData.position] = intersection;
        }
    }
}

[System.Serializable]
public class Intersection
{
    public Spline spline;
    public List<Vector3> edges;
}

//[System.Serializable]
//public class IntersectionEntry
//{
//    public Vector3 position;
//    public Intersection intersection;

//    public IntersectionEntry(Vector3 pos, Intersection inter)
//    {
//        position = pos;
//        intersection = inter;
//    }
//}
