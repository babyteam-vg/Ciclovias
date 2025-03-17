using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class SplinesManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private Graph graph;
    [SerializeField] private TaskManager taskManager;

    public SplineContainer splineContainer;
    public HashSet<Vector3> sealedPositions = new HashSet<Vector3>();

    private Spline spline;
    private Dictionary<Vector3, (Spline, int)> intersections = new Dictionary<Vector3, (Spline, int)>();
    private bool recentIntersection = false;

    public event Action SplineChanged;
    public event Action SplineSealed;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        spline = splineContainer.Spline;
    }

    private void OnEnable()
    {
        taskManager.TaskSealed += UpdateSealedPositions;

        graph.OnEdgeAdded += HandleEdgeAdded;
        graph.OnEdgeRemoved += HandleEdgeRemoved;
    }
    private void OnDisable()
    {
        taskManager.TaskSealed -= UpdateSealedPositions;

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
        var (firstSpline, firstIndex) = FindKnotAndSpline(firstWorldPosition);

        // 'T' and 'Y' Intersections
        if (firstNeighbors.Count > 2) HandleTandY(firstWorldPosition);
        if (secondNeighbors.Count > 2) HandleTandY(secondWorldPosition);

        // New Spline
        if (firstSpline == null
            || (firstSpline != null && firstIndex != firstSpline.Count - 1)) // Mid Spline
        {
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

        if (isIntersection
            && firstNeighbors.Count < 3
            && !recentIntersection)
            HandleIntersection(secondWorldPosition, firstWorldPosition);
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

        // 'T' and 'Y' Intersections
        //if (secondNeighbors.Count < 3) HandleTandY(secondWorldPosition);

        if (firstSpline == null) return;
        spline = firstSpline;

        spline.RemoveAt(firstIndex);

        // Check Collinearity
        foreach (var neighbor in secondNeighbors)
            if (!graph.IsCollinear(secondNodePosition, firstNodePosition, neighbor)
                && secondIndex == -1)
            {
                recentIntersection = false;
                intersections.Remove(secondWorldPosition);
                Debug.Log($"Intersection Removed at {secondWorldPosition}");
                BezierKnot newKnot = new BezierKnot(secondWorldPosition);
                spline.Insert(firstIndex, newKnot, TangentMode.Broken, 0.5f);
                break;
            }

        if (spline.Count == 1)
            splineContainer.RemoveSpline(spline);

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
    }

    // ::::: Continue Spline (Intersection)
    private void HandleIntersection(Vector3 position, Vector3 beforePosition)
    {
        recentIntersection = true;

        Vector3 lastPosition = spline.ElementAt(spline.Count - 1).Position;
        if (lastPosition == beforePosition)
            spline.RemoveAt(spline.Count - 1);

        BezierKnot beforeIntersection = new BezierKnot(beforePosition);
        BezierKnot afterIntersection = new BezierKnot(position);

        // Direction
        Vector3 beforeDir = (afterIntersection.Position - beforeIntersection.Position);
        beforeDir = beforeDir.normalized;

        // Tangents
        float tangentWeight = 1f;
        beforeIntersection.TangentOut = beforeDir * tangentWeight;
        afterIntersection.TangentIn = -beforeDir * tangentWeight;

        spline.Insert(spline.Count, afterIntersection, TangentMode.Broken, 0.5f);
    }

    // ::::: 'T' and 'Y' Intersections
    private void HandleTandY(Vector3 position)
    {
        if (intersections.ContainsKey(position))
        {
            var (spline, index) = intersections[position];
            BezierKnot intersectionKnot = new BezierKnot(position);

            if (!spline.Contains(intersectionKnot)) // Building
                spline.Insert(index, intersectionKnot);
            else // Destroying
            {
                //List<(Spline, int)> splinesAndKnots = FindAllKnotsAndSplines(position);
                //foreach ((Spline s, int i) in splinesAndKnots)
                //{
                //    Spline iSpline = s;
                //    int iIndex = i;

                //    iSpline.Remove(intersectionKnot);
                //}
            }
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

    // ::::: Find Every Spline That Has a Knot
    private List<(Spline spline, int knotIndex)> FindAllKnotsAndSplines(Vector3 worldPosition)
    {
        var result = new List<(Spline, int)>();

        foreach (var currentSpline in splineContainer.Splines)
            for (int i = 0; i < currentSpline.Count; i++)
                if (currentSpline[i].Position.Equals((float3)worldPosition))
                    result.Add((currentSpline, i));

        return result;
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

    // ::::: 
    private void UpdateSealedPositions(Task task)
    {
        foreach (Vector2Int pos in task.path)
        {
            Vector3 centeredPos = grid.GetWorldPositionFromCellCentered(pos.x, pos.y);
            if (!sealedPositions.Contains(centeredPos)) sealedPositions.Add(centeredPos);
        }

        SplineSealed?.Invoke();
    }

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Splines -> SplinesData
    public SplineContainerData SaveSplines()
    {
        SplineContainerData containerData = new SplineContainerData();

        foreach (var spline in splineContainer.Splines)
        {
            SplinesData splineData = new SplinesData();

            // Knots
            foreach (BezierKnot knot in spline)
                splineData.knots.Add(new SplinesData.SerializableKnot
                {
                    position = knot.Position,
                    tangentIn = knot.TangentIn,
                    tangentOut = knot.TangentOut
                });

            containerData.splines.Add(splineData);

            // Intersections
            foreach (var kvp in intersections)
                if (kvp.Value.Item1 == spline)
                {
                    containerData.intersections.Add(new IntersectionData
                    {
                        position = kvp.Key,
                        index = kvp.Value.Item2
                    });
                }
        }

        // Sealed
        foreach (Vector3 seal in sealedPositions)
            containerData.sealedPositions.Add(seal);

        return containerData;
    }

    // ::::: SplinesData -> Splines
    public void LoadSplines(SplineContainerData containerData)
    {
        intersections.Clear();

        foreach (var splineData in containerData.splines)
        {
            Spline newSpline = new Spline();

            // Knots
            foreach (var knotData in splineData.knots)
            {
                BezierKnot knot = new BezierKnot(knotData.position)
                {
                    TangentIn = knotData.tangentIn,
                    TangentOut = knotData.tangentOut
                };

                newSpline.Insert(newSpline.Count, knot, TangentMode.Broken, 0.5f);
            }

            // Intersections
            foreach (var intersectionData in containerData.intersections)
                intersections[intersectionData.position] = (newSpline, intersectionData.index);

            splineContainer.AddSpline(newSpline);
        }

        foreach (Vector3 seal in containerData.sealedPositions)
            sealedPositions.Add(seal);
    }
}