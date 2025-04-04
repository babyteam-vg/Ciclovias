using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using static UnityEngine.Rendering.VolumeComponent;

public class SplineManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TutorialManager tutorialManager;

    public SplineContainer splineContainer;

    [SerializeField] private List<IntersectionEntry> intersectionsList = new List<IntersectionEntry>();
    private Dictionary<Vector3, Intersection> intersections = new Dictionary<Vector3, Intersection>();

    public event Action<Spline> SplineUpdated;

    // :::::::::: MONO METHODS ::::::::::
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

    private void Update()
    {
        intersectionsList.Clear();
        foreach (var kvp in intersections)
        {
            intersectionsList.Add(new IntersectionEntry(kvp.Key, kvp.Value));
        }
    }

    // :::::::::: EVENT METHODS ::::::::::
    // ::::: Building Tree
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
        (Spline firstSpline, int firstIndex) = FindKnotAndSpline(firstWorldPosition);
        (Spline secondSpline, int secondIndex) = FindKnotAndSpline(secondWorldPosition);

        if (firstSpline == null) // Spline Not Found, Starting...
        {
            if (intersections.ContainsKey(firstWorldPosition)) // ...From an Intersection...
            {
                if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
                {
                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To an Intersection");

                    Intersection intersection1 = intersections[firstWorldPosition];
                    ExpandIntersection(secondWorldPosition, firstWorldPosition, intersection1);

                    Intersection intersection2 = intersections[secondWorldPosition];
                    ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection2);
                }
                else
                {
                    if (secondSpline == null) // ...To Nothing *
                    {
                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To Nothing");
                        Intersection intersection = intersections[firstWorldPosition];
                        ExpandIntersection(secondWorldPosition, firstWorldPosition, intersection);
                    }
                    else // ...To a Spline...
                    {
                        Intersection intersection = intersections[firstWorldPosition];
                        Spline newSpline = ExpandIntersection(secondWorldPosition, firstWorldPosition, intersection);

                        if (secondIndex == -1) // ...at the Middle *
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To a Spline > at the Middle");
                            CreateTripleIntersectionByCrossingStraightSplines(newSpline, 1, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
                        }
                        else // ...at the Edge...
                        {
                            if (isSecondIntersection) // ...and They're Not Collinear *
                            {
                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To a Spline > at the Edge > and They're Not Collinear");
                                CreateIntersectionByFusingStraighSplines(newSpline, 1, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
                            }
                            else // ...and They're Collinear...
                            {
                                if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To a Spline > at the Edge > and They're Collinear > but Is an Intersection Spline");
                                }
                                else // ...and Is a Straight Spline *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From an Intersection > To a Spline > at the Edge > and They're Collinear > and Is a Straight Spline");
                                    FuseStraightSplines(newSpline, 1, secondSpline, secondIndex);
                                }
                            }
                        }
                    }
                }
            }
            else // ...From Nothing... ***
            {
                if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
                {
                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To an Intersection");
                    Intersection intersection = intersections[secondWorldPosition];
                    ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
                }
                else
                {
                    if (secondSpline == null) // ...To Nothing *
                    {
                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To Nothing");
                        StartNewSpline(firstWorldPosition, secondWorldPosition);
                    }
                    else // ...To a Spline...
                    {
                        Spline newSpline = StartNewSpline(firstWorldPosition, secondWorldPosition);

                        if (secondIndex == -1) // ...at the Middle *
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To a Spline > at the Middle");
                            CreateTripleIntersectionByCrossingStraightSplines(newSpline, 1, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
                        }
                        else // ...at the Edge...
                        {
                            if (isSecondIntersection) // ...and They're Not Collinear *
                            {
                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To a Spline > at the Edge > and They're Not Collinear");
                                CreateIntersectionByFusingStraighSplines(newSpline, 1, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
                            }
                            else // ...and They're Collinear...
                            {
                                if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To a Spline > at the Edge > and They're Collinear > but Is an Intersection Spline");
                                    // Do Nothing?
                                }
                                else // ...and Is a Straight Spline *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Not Found > From Nothing > To a Spline > at the Edge > and They're Collinear > and Is a Straight Spline");
                                    FuseStraightSplines(newSpline, 1, secondSpline, secondIndex);
                                }
                            }
                        }
                    }
                }
            }
        }
        else // Spline Found, Starting...
        {
            if (firstIndex == -1) // ...From the Middle...
            {
                if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
                {
                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To an Intersection");

                    Spline newSpline = CreateTripleIntersectionFromAStraightSpline(firstSpline, firstWorldPosition, secondWorldPosition);
                    ClearSpline(newSpline);

                    Intersection intersection = intersections[secondWorldPosition];
                    ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
                }
                else
                {
                    if (secondSpline == null) // ...To Nothing *
                    {
                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To Nothing");
                        CreateTripleIntersectionFromAStraightSpline(firstSpline, firstWorldPosition, secondWorldPosition);
                    }
                    else // ...To a Spline...
                    {
                        Spline newSpline = CreateTripleIntersectionFromAStraightSpline(firstSpline, firstWorldPosition, secondWorldPosition);

                        if (secondIndex == -1) // ...at the Middle *
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To a Spline > at the Middle");
                            ClearSpline(newSpline);
                            CreateTripleIntersectionFromAStraightSpline(secondSpline, secondWorldPosition, firstWorldPosition);
                        }
                        else // ...at the Edge...
                        {
                            if (isSecondIntersection) // ...and They're Not Collinear *
                            {
                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To a Spline > at the Edge > and They're Not Collinear");
                                CreateIntersectionByFusingStraighSplines(newSpline, 1, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
                            }
                            else // ...and They're Collinear...
                            {
                                if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To a Spline > and They're Collinear > but Is an Intersection Spline");
                                    AddKnotToSpline(newSpline, 1, secondWorldPosition);
                                }
                                else // ...and Is a Straight Spline *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Middle > To a Spline > and They're Collinear > and Is a Straight Spline");
                                    FuseStraightSplines(newSpline, 1, secondSpline, secondIndex);
                                }
                            }
                        }
                    }
                }
            }
            else // ...From the Edge...
            {
                if (isFirstIntersection) // ...with a Change of Direction...
                {
                    if (IsIntersectionSpline(firstSpline)) // ...and From an Intersection Spline...
                    {
                        if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To an Intersection");
                            Intersection intersection = intersections[secondWorldPosition];
                            ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
                        }
                        else
                        {
                            if (secondSpline == null) // ...To Nothing *
                            {
                                if (firstNeighbors.Count > 2) // ...but From a Conflictive Point *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To Nothing > but From a Conflictive Point");
                                    CreateTripleIntersectionFromAnIntersectionSpline(firstWorldPosition, secondWorldPosition);
                                }
                                else  // ...and From a Single Spline *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To Nothing > and From a Single Spline");
                                    StartNewSpline(firstWorldPosition, secondWorldPosition);
                                }
                            }
                            else // ...To a Spline...
                            {
                                if (secondIndex == -1) // ...at the Middle *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To a Spline > at the Middle");
                                    Spline newSpline = CreateTripleIntersectionFromAStraightSpline(secondSpline, secondWorldPosition, firstWorldPosition);
                                }
                                else // ...at the Edge...
                                {
                                    if (isSecondIntersection) // ...and They're Not Collinear...
                                    {
                                        if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
                                        {
                                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To a Spline > at the Edge > and They're Not Collinear > but Is an Intersection Spline");
                                            StartNewSpline(firstWorldPosition, secondWorldPosition);
                                        }
                                        else // ...but is a Straight Spline *
                                        {
                                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To a Spline > at the Edge > and They're Not Collinear > but is a Straight Spline");
                                            CreateIntersectionFromAnIntersectionSpline(firstWorldPosition, secondWorldPosition, secondSpline, secondIndex);
                                        }
                                    }
                                    else // ...and They're Collinear *
                                    {
                                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From an Intersection Spline > To a Spline > at the Edge > and They're Collinear");
                                        Spline newnSpline = StartNewSpline(firstWorldPosition, secondWorldPosition);
                                        FuseStraightSplines(newnSpline, 1, secondSpline, secondIndex);
                                    }
                                }
                            }
                        }
                    }
                    else // ...and From a Straight Spline...
                    {
                        if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To an Intersection");
                            CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
                            Intersection intersection = intersections[secondWorldPosition];
                            Spline newSpline = ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
                            ClearSpline(newSpline);
                        }
                        else
                        {
                            if (secondSpline == null) // ...To Nothing...
                            {
                                if (firstNeighbors.Count > 2) // ...but From a Conflictive Point *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To Nothing > but From a Conflictive Point");
                                    CreateTripleIntersectionFromAnIntersectionSpline(firstWorldPosition, secondWorldPosition);
                                }
                                else  // ...and From a Single Spline *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To Nothing > and From a Single Spline");
                                    CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
                                }
                            }
                            else // ...To a Spline...
                            {
                                if (secondIndex == -1) // ...at the Middle *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To a Spline > at the Middle");
                                    CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
                                    Spline newSpline = CreateTripleIntersectionFromAStraightSpline(secondSpline, secondWorldPosition, firstWorldPosition);
                                    ClearSpline(newSpline);
                                }
                                else // ...at the Edge...
                                {
                                    if (isSecondIntersection) // ...and They're Not Collinear *
                                    {
                                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > and They're Not Collinear");
                                        CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
                                    }
                                    else // ...and They're Collinear...
                                    {
                                        if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
                                        {
                                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > and They're Collinear > but Is an Intersection Spline");
                                            CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
                                        }
                                        else // ...and Is a Straight Spline *
                                        {
                                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > with a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > and They're Collinear > and Is a Straight Spline");
                                            CreateIntersectionFromStraightSpline(firstSpline, firstIndex, firstWorldPosition, secondWorldPosition);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else // ...w/o a Change of Direction...
                {
                    if (IsIntersectionSpline(firstSpline)) // ...and From an Intersection Spline...
                    {
                        if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To an Intersection");

                            Intersection intersection = intersections[secondWorldPosition];
                            ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);
                        }
                        else
                        {
                            if (secondSpline == null) // ...To Nothing *
                            {
                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To Nothing");
                                StartNewSpline(firstWorldPosition, secondWorldPosition);
                            }
                            else // ...To a Spline...
                            {
                                if (secondIndex == -1) // ...at the Middle *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To a Splinee > at the Middle");
                                    CreateTripleIntersectionFromAStraightSpline(secondSpline, secondWorldPosition, firstWorldPosition);
                                }
                                else // ...at the Edge...
                                {
                                    if (isSecondIntersection) // ...and They're Not Collinear *
                                    {
                                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To a Splinee > at the Edge > and They're Not Collinear");
                                        CreateIntersectionFromAnIntersectionSpline(firstWorldPosition, secondWorldPosition, secondSpline, secondIndex);
                                    }
                                    else // ...and They're Collinear...
                                    {
                                        if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
                                        {
                                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To a Splinee > at the Edge > but Is an Intersection Spline");
                                            StartNewSpline(firstWorldPosition, secondWorldPosition);
                                        }
                                        else // ...and Is a Straight Spline *
                                        {
                                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From an Intersection Spline > To a Splinee > at the Edge > and Is a Straight Spline");
                                            Spline newSpline = StartNewSpline(firstWorldPosition, secondWorldPosition);
                                            FuseStraightSplines(newSpline, 1, secondSpline, secondIndex);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else // ...and From a Straight Spline...
                    {
                        if (intersections.ContainsKey(secondWorldPosition)) // ...To an Intersection *
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To an Intersection");

                            Intersection intersection = intersections[secondWorldPosition];
                            Spline newSpline = ExpandIntersection(firstWorldPosition, secondWorldPosition, intersection);

                            FuseStraightSplines(firstSpline, firstIndex, newSpline, 1);
                        }
                        else
                        {
                            if (secondSpline == null) // ...To Nothing *
                            {
                                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To Nothing");
                                AddKnotToSpline(firstSpline, firstIndex, secondWorldPosition);
                            }
                            else // ...To a Spline...
                            {
                                if (secondIndex == -1) // ...at the Middle *
                                {
                                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To a Spline > at the Middle");
                                    CreateTripleIntersectionByCrossingStraightSplines(firstSpline, firstIndex, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
                                }
                                else // ...at the Edge...
                                {
                                    if (isSecondIntersection) // ...and They're Not Collinear *
                                    {
                                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > and They're Not Collinear");
                                        AddKnotToSpline(firstSpline, firstIndex, secondWorldPosition);
                                        CreateIntersectionByFusingStraighSplines(firstSpline, firstIndex, firstWorldPosition, secondSpline, secondIndex, secondWorldPosition);
                                    }
                                    else // ...and They're Collinear...
                                    {
                                        if (IsIntersectionSpline(secondSpline)) // ...but Is an Intersection Spline *
                                        {
                                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > but Is an Intersection Spline");
                                            AddKnotToSpline(firstSpline, firstIndex, secondWorldPosition);
                                        }
                                        else // ...and Is a Straight Spline *
                                        {
                                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Found > From the Edge > w/o a Change of Direction > and From a Straight Spline > To a Spline > at the Edge > and Is a Straight Spline");
                                            FuseStraightSplines(firstSpline, firstIndex, secondSpline, secondIndex);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // ::::: Destroying Tree
    private void HandleEdgeRemoved(Vector2Int firstNodePosition, Vector2Int secondNodePosition)
    {
        Vector3 firstWorldPosition = grid.GetWorldPositionFromCellCentered(firstNodePosition.x, firstNodePosition.y);
        Vector3 secondWorldPosition = grid.GetWorldPositionFromCellCentered(secondNodePosition.x, secondNodePosition.y);

        List<Vector2Int> secondNeighbors = graph.GetNeighborsPos(secondNodePosition);

        if (intersections.ContainsKey(firstWorldPosition)) // Remove an Intersection...
        {
            Intersection intersection = intersections[firstWorldPosition];

            if (intersection.edges.Count > 2) // ...Triple *
            {
                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Remove an Intersection > Triple");
                SplitTripleIntersection(firstWorldPosition, intersection);
            }
            else // ...Simple *
            {
                Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Remove an Intersection > Simple");
                SplitIntersection(firstWorldPosition, intersection);
            }
        }
        else // Remove a Knot...
        {
            // Find Knot and Spline
            (Spline spline, int index) = FindKnotAndSpline(firstWorldPosition);

            if (spline != null) // Destroying a Spline...
            {
                if (index == -1) // ...by the Middle *
                {
                    Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Remove a Knot > Destroying a Spline > by the Middle");
                    SplitStraightSpline(spline, firstWorldPosition);
                }
                else // ...by the Edges...
                {
                    if (intersections.ContainsKey(secondWorldPosition)) // ...in an Intersection...
                    {
                        Intersection intersection = intersections[secondWorldPosition];

                        if (secondNeighbors.Count < 2) // ...Simple *
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Remove a Knot > Destroying a Spline > by the Edges > in an Intersection > Simple");
                            RemoveIntersection(firstWorldPosition, secondWorldPosition, intersection);
                        }
                        else // ...Triple *
                        {
                            Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Remove a Knot > Destroying a Spline > by the Edges > in an Intersection > Triple");
                            ReduceTripleIntersection(spline, firstWorldPosition, secondWorldPosition, intersection);
                        }
                    }
                    else // ...in a Straight Spline *
                    {
                        Debug.Log($"{firstWorldPosition} - {secondWorldPosition} Spline Reduced or Remove a Knot > Destroying a Spline > by the Edges > in a Straight Spline");
                        RemoveKnotToSpline(spline, index);
                    }
                }
            }
        }
    }

    // :::::::::: SPLINE (FUNDAMENTAL) METHODS ::::::::::
    // ::::: Spawn a New Spline [V]
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
        SplineUpdated?.Invoke(newSpline); // !

        return newSpline;
    }

    // ::::: Continue a Straight Spline [V]
    private void AddKnotToSpline(Spline spline, int index, Vector3 position)
    {
        spline.RemoveAt(index);
        BezierKnot newKnot = new BezierKnot(position);
        spline.Insert(index, newKnot, TangentMode.Broken, 0.5f);
        SplineUpdated?.Invoke(spline); // !
    }

    // ::::: Reduce or Remove a Spline [V]
    private void RemoveKnotToSpline(Spline spline, int index)
    {
        if (IsOneUnitLong(spline, index)) ClearSpline(spline);
        else
        {
            BezierKnot previousKnot = GeneratePreviousKnot(spline, index);
            AddKnotToSpline(spline, index, previousKnot.Position);
        }
    }

    // ::::: Spawn an Intesection Spline [V]
    private Spline CreateIntersection(BezierKnot fromKnot, BezierKnot toKnot, Vector3 intersectionPosition)
    {
        Vector3 to = toKnot.Position;

        // Tangents
        float tangentWeight = 1f;
        Vector3 dir = (to - intersectionPosition).normalized;
        toKnot.TangentIn = -dir * tangentWeight;

        // New Intersection Spline
        Spline intersectionSpline = new Spline();
        intersectionSpline.Insert(0, fromKnot, TangentMode.Broken, 0.5f);
        intersectionSpline.Insert(1, toKnot, TangentMode.Broken, 0.5f);
        splineContainer.AddSpline(intersectionSpline);
        SplineUpdated?.Invoke(intersectionSpline);

        // Intersections Dictionary
        Intersection newIntersection = new Intersection
        {
            spline = intersectionSpline,
            edges = new List<Vector3>
            {
                fromKnot.Position,
                to
            },
        };

        if (!intersections.ContainsKey(intersectionPosition))
            intersections.Add(intersectionPosition, newIntersection);
        else intersections[intersectionPosition] = newIntersection;

        return intersectionSpline;
    }
    
    // ::::: Expand the Intesection and Its Edges [V]
    private Spline ExpandIntersection(Vector3 newEdgePosition, Vector3 intersectionPosition, Intersection intersection)
    {
        // Erase Intersection Spline
        ClearSpline(intersection.spline);

        // Modify Edge Splines
        foreach (Vector3 edgePos in intersection.edges)
        {
            (Spline edgeSpline, int edgeIndex) = FindKnotAndSpline(edgePos);

            if (edgeSpline != null) AddKnotToSpline(edgeSpline, edgeIndex, intersectionPosition);
            else StartNewSpline(intersectionPosition, edgePos);
        }

        // Intesections Dictionary
        intersection.spline = null;
        intersection.edges.Add(newEdgePosition);

        // New Straig Spline
        return StartNewSpline(intersectionPosition, newEdgePosition);
    }

    // :::::::::: SPLINE (BUILDING) METHODS ::::::::::
    // ::::: Create an Intersection From a Straight Spline [V]
    private Spline CreateIntersectionFromStraightSpline(Spline spline, int index, Vector3 intersectionPosition, Vector3 toPosition)
    {
        BezierKnot fromKnot = GeneratePreviousKnot(spline, index);
        BezierKnot toKnot = new BezierKnot(toPosition);

        // Original Straight Spline
        RemoveKnotToSpline(spline, index);

        // New Intersection Spline
        return CreateIntersection(fromKnot, toKnot, intersectionPosition);
    }

    // ::::: Create an Intersection by Fusing 2 Straight Splines on Their Edges [V]
    private void CreateIntersectionByFusingStraighSplines(
        Spline spline1, int index1, Vector3 fromPosition,           // Expanded Spline
        Spline spline2, int index2, Vector3 intersectionPosition)   // Static Spline
    {
        BezierKnot fromKnot = new BezierKnot(fromPosition);
        BezierKnot toKnot = GeneratePreviousKnot(spline2, index2);

        // Expanded Spline
        RemoveKnotToSpline(spline1, index1);

        // Static Spline
        RemoveKnotToSpline(spline2, index2);

        // New Intersection Spline
        CreateIntersection(fromKnot, toKnot, intersectionPosition);
    }

    // ::::: From an Intersection Spline to a No Collinear Spline's Edge
    private void CreateIntersectionFromAnIntersectionSpline(Vector3 fromPosition, Vector3 intersectionPosition, Spline toSpline, int toIndex)
    {
        BezierKnot fromKnot = new BezierKnot(fromPosition);
        BezierKnot toKnot = GeneratePreviousKnot(toSpline, toIndex);

        // Original To Spline
        RemoveKnotToSpline(toSpline, toIndex);

        // New Intersection Spline
        CreateIntersection(fromKnot, toKnot, intersectionPosition);
    }

    // ::::: Create a Triple Intersection By Crossing 2 Straight Splines (Edge - Middle) [V]
    private void CreateTripleIntersectionByCrossingStraightSplines(
        Spline spline1, int index1, Vector3 newEdgePosition,        // Crossing Spline
        Spline spline2, int index2, Vector3 intersectionPosition)   // Crossed Spline
    {
        (Spline firstSpline, Spline secondSpline) = SplitStraightSpline(spline2, intersectionPosition);

        Vector3 firstEdge = firstSpline.ElementAt(1).Position;
        Vector3 secondEdge = secondSpline.ElementAt(0).Position;

        // Expand Edge Splines
        AddKnotToSpline(spline1, index1, intersectionPosition);
        AddKnotToSpline(firstSpline, 1, intersectionPosition);
        AddKnotToSpline(secondSpline, 0, intersectionPosition);

        // Intersections Dictionary
        Intersection newIntersection = new Intersection
        {
            spline = null,
            edges = new List<Vector3>
            {
                newEdgePosition,
                firstEdge,
                secondEdge
            },
        };

        if (!intersections.ContainsKey(intersectionPosition))
            intersections.Add(intersectionPosition, newIntersection);
        else intersections[intersectionPosition] = newIntersection;
    }

    // ::::: 
    private void CreateTripleIntersectionFromAnIntersectionSpline(Vector3 intersectionPosition, Vector3 newEdgePosition)
    {
        StartNewSpline(intersectionPosition, newEdgePosition);

        // Finding the Edges
        Vector2Int? cellPosition = grid.GetCellFromWorldPosition(intersectionPosition);
        List<Vector3> intersectionEdges = new List<Vector3>();
        foreach (Vector2Int neighbor in graph.GetNeighborsPos(cellPosition.Value))
        {
            Vector3 edge = grid.GetWorldPositionFromCellCentered(neighbor.x, neighbor.y);
            intersectionEdges.Add(edge);
        }

        // Intersections Dictioniary
        Intersection newIntersection = new Intersection
        {
            spline = null,
            edges = intersectionEdges
        };

        if (!intersections.ContainsKey(intersectionPosition))
            intersections.Add(intersectionPosition, newIntersection);
        else intersections[intersectionPosition] = newIntersection;
    }

    // ::::: Create Triple Intersection By Spawning a New Spline In the Middle of Another
    private Spline CreateTripleIntersectionFromAStraightSpline(Spline spline, Vector3 intersectionPosition, Vector3 newEdgePosition)
    {
        (Spline firstSpline, Spline secondSpline) = SplitStraightSpline(spline, intersectionPosition);

        Vector3 firstEdge = firstSpline.ElementAt(1).Position;
        Vector3 secondEdge = secondSpline.ElementAt(0).Position;

        // Expand Edge Splines
        AddKnotToSpline(firstSpline, 1, intersectionPosition);
        AddKnotToSpline(secondSpline, 0, intersectionPosition);

        // Intersections Dictionary
        Intersection newIntersection = new Intersection
        {
            spline = null,
            edges = new List<Vector3>
            {
                newEdgePosition,
                firstEdge,
                secondEdge
            },
        };

        if (!intersections.ContainsKey(intersectionPosition))
            intersections.Add(intersectionPosition, newIntersection);
        else intersections[intersectionPosition] = newIntersection;

        return StartNewSpline(intersectionPosition, newEdgePosition);
    }

    // ::::: Fusing 2 Straight Splines [V]
    private void FuseStraightSplines(Spline spline1, int index1, Spline spline2, int index2)
    {
        Vector3 originalStart = spline1.ElementAt(1 - index1).Position;
        Vector3 originalEnd = spline2.ElementAt(1 - index2).Position;

        ClearSpline(spline1);
        ClearSpline(spline2);

        // New Fused Spline
        Spline fusedSpline = StartNewSpline(originalStart, originalEnd);
    }

    // :::::::::: SPLINE (DESTRUCTION) METHODS ::::::::::
    // ::::: Handle the Removal of an Intersection and Its Splines by the Edges [V]
    private void RemoveIntersection(Vector3 removedPosition, Vector3 intersectionPosition, Intersection intersection)
    {
        intersection.edges.Remove(removedPosition);
        ClearSpline(intersection.spline);

        // Modify Edge Spline
        Vector3 remainingEdge = intersection.edges.First();
        Vector2Int? remainingPosition = grid.GetCellFromWorldPosition(remainingEdge);
        bool conflictive = graph.GetNeighborsCount(remainingPosition.Value) > 1;
        (Spline edgeSpline, int edgeIndex) = FindKnotAndSpline(remainingEdge);
        if (edgeSpline == null || conflictive) StartNewSpline(remainingEdge, intersectionPosition);
        else AddKnotToSpline(edgeSpline, edgeIndex, intersectionPosition);

        // Intersections Dictionary
        intersections.Remove(intersectionPosition);
    }

    // ::::: Triple Intersection -> Simple Intersection [V]
    private void ReduceTripleIntersection(Spline spline, Vector3 removedPosition, Vector3 intersectionPosition, Intersection intersection)
    {
        // Erase Original From Spline
        ClearSpline(spline);

        // Intersections Dictionary
        intersections[intersectionPosition].edges.Remove(removedPosition);

        // Check Edge Splines
        bool conflictive = false;
        foreach (Vector3 edgePos in intersection.edges)
        {
            if (intersections.ContainsKey(edgePos))
            {
                conflictive = true;
                break;
            }
            else
            {
                (Spline edgeSpline, int edgeIndex) = FindKnotAndSpline(intersectionPosition);
                if (edgeSpline != null) RemoveKnotToSpline(edgeSpline, edgeIndex);
            }
        }

        // Intersection Point
        if (conflictive)
        {
            intersections.Remove(intersectionPosition);

        }
        else
        {
            // New Intersection Spline
            Vector3 from = intersections[intersectionPosition].edges.First();
            BezierKnot fromKnot = new BezierKnot(from);

            Vector3 to = intersections[intersectionPosition].edges.Last();
            BezierKnot toKnot = new BezierKnot(to);

            // Intersections Dictionary
            intersection.spline = CreateIntersection(fromKnot, toKnot, intersectionPosition);
        }
    }

    // :::::::::: SPLINE (SPLITING) METHODS ::::::::::
    // ::::: Spliting a Spline in Two [V]
    private (Spline firstSpline, Spline secondSpline) SplitStraightSpline(Spline splitedSpline, Vector3 splitPosition)
    {
        // Original Spline
        Vector3 originalStart = splitedSpline.ElementAt(0).Position;
        Vector3 originalEnd = splitedSpline.ElementAt(1).Position;
        ClearSpline(splitedSpline);

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

    // ::::: Splitting in an Intersection [V]
    private void SplitIntersection(Vector3 splitPosition, Intersection intersection)
    {
        // Intersection Spline
        ClearSpline(intersection.spline);

        // Intersections Dictionary
        intersections.Remove(splitPosition);
    }

    // ::::: Splitting in a Triple Intersection [V]
    private void SplitTripleIntersection(Vector3 splitPosition, Intersection intersection)
    {
        // Modify Edge Splines
        foreach (Vector3 edgePos in intersection.edges)
        {
            (Spline edgeSpline, int edgeIndex) = FindKnotAndSpline(splitPosition);
            if (edgeSpline != null) RemoveKnotToSpline(edgeSpline, edgeIndex);
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
    private BezierKnot GeneratePreviousKnot(Spline spline, int index)
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

    // ::::: Clear and Remove a Spline
    private void ClearSpline(Spline spline)
    {
        spline.Clear();
        SplineUpdated?.Invoke(spline);
        splineContainer.RemoveSpline(spline);
    }

    // ::::: Check How Far Apart the Knots of a Straight Spline Are
    private bool IsOneUnitLong(Spline spline, int index)
    {
        BezierKnot previousKnot = GeneratePreviousKnot(spline, index);
        return previousKnot.Equals(spline.ElementAt(1 - index));
    }

    // :::::::::: DEBUG METHODS ::::::::::
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

    // :::::::::: SEALING METHODS ::::::::::
    // ::::: Task Sealed
    private void OnTaskSealed(Task task)
    {
        
    }

    // ::::: Tutorial Section Completed
    private void OnTutorialSealed(List<Vector2Int> path)
    {
        
    }

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Spline -> SplineData
    public SplineData SaveSplines()
    {
        SplineData data = new SplineData();

        // Splines
        foreach (var spline in splineContainer.Splines)
        {
            SplineData.SerializableSpline splineData = new SplineData.SerializableSpline();

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
            SplineUpdated?.Invoke(newSpline);
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

[System.Serializable]
public class IntersectionEntry
{
    public Vector3 position;
    public Intersection intersection;

    public IntersectionEntry(Vector3 pos, Intersection inter)
    {
        position = pos;
        intersection = inter;
    }
}
