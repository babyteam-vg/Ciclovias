using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class SplineManager : MonoBehaviour
{
    [SerializeField] private Graph graph;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;
    [SerializeField] private SplineContainer splineContainer;

    [Range(0f, 1f)]
    public float cornerRadius = 0.5f;
    public bool closedLoop = false;

    // === Methods ===
    private void OnEnable()
    {
        laneConstructor.OnLaneBuilt += BuildSplineSegment;
        laneDestructor.OnLaneDestroyed += DestroySplineSegment;
    }
    private void Start() { BuildSpline(); }
    private void OnDisable()
    {
        laneConstructor.OnLaneBuilt -= BuildSplineSegment;
        laneDestructor.OnLaneDestroyed -= DestroySplineSegment;
    }

    // Spline Update Via OnLaneBuilt
    private void BuildSplineSegment(Vector2Int affectedNodePosition)
    {
        var spline = splineContainer.Spline;

        Node affectedNode = graph.GetNode(affectedNodePosition);
        if (affectedNode == null)
            return;

        List<Node> segmentNodes = new List<Node>();
        segmentNodes.AddRange(affectedNode.neighbors); // Neighbours
        segmentNodes.Insert(0, affectedNode);

        foreach (var node in segmentNodes)
        {
            var index = graph.GetAllNodes().IndexOf(node);
            if (index < 0)
                continue;
            
            Node prevNode = (index > 0) ? graph.GetAllNodes()[index - 1] : null;
            Node currNode = graph.GetAllNodes()[index];
            Node nextNode = (index < graph.GetAllNodes().Count - 1) ? graph.GetAllNodes()[index + 1] : null;

            var dirIn = (prevNode != null) ? GetDirection(prevNode.position, currNode.position) : GridDirection.None; // ( )-( ) ( )
            var dirOut = (nextNode != null) ? GetDirection(currNode.position, nextNode.position) : GridDirection.None; // ( ) ( )-( )
            PathCase pathCase = GetPathCase(dirIn, dirOut);

            (Vector3 knotPosition, Vector3 tangentIn, Vector3 tangentOut) =
                CalculateKnot(
                    prevNode?.worldPosition ?? currNode.worldPosition,
                    currNode.worldPosition,
                    nextNode?.worldPosition ?? currNode.worldPosition,
                    pathCase, dirIn, dirOut);

            var knot = new BezierKnot(
                knotPosition,
                tangentIn,
                tangentOut,
                Quaternion.identity
            );

            if (index < spline.Count)
                spline[index] = knot;
            else
                spline.Add(knot, TangentMode.Continuous);
        }
    }

    // Spline Update Via OnLaneDestroyed
    private void DestroySplineSegment(Vector2Int affectedNodePosition)
    {
        //var spline = splineContainer.Spline;

        //Node affectedNode = graph.GetNode(affectedNodePosition);
        //if (affectedNode == null)
        //    return;

        //spline.Remove();
    }

    // Spline's First Generation
    private void BuildSpline()
    {
        var spline = splineContainer.Spline;
        spline.Clear();

        var allNodes = graph.GetAllNodes();

        if (allNodes.Count < 2)
            return;
        // ( )-(X)-( ) <¬
        for (int i = 0; i < allNodes.Count; i++)
        {
            Node prevNode = (i > 0) ? allNodes[i - 1] : null;
            Node currNode = allNodes[i];
            Node nextNode = (i < allNodes.Count - 1) ? allNodes[i + 1] : null;

            var dirIn = (prevNode != null) ? GetDirection(prevNode.position, currNode.position) : GridDirection.None; // ( )-( ) ( )
            var dirOut = (nextNode != null) ? GetDirection(currNode.position, nextNode.position) : GridDirection.None; // ( ) ( )-( )
            PathCase pathCase = GetPathCase(dirIn, dirOut);

            (Vector3 knotPosition, Vector3 tangentIn, Vector3 tangentOut) =
                CalculateKnot(
                    prevNode?.worldPosition ?? currNode.worldPosition,
                    currNode.worldPosition,
                    nextNode?.worldPosition ?? currNode.worldPosition,
                    pathCase, dirIn, dirOut);

            var knot = new BezierKnot(
                knotPosition,
                tangentIn,
                tangentOut,
                Quaternion.identity);

            spline.Add(knot, TangentMode.Continuous);
        }

        spline.Closed = closedLoop;
    }

    // Knot Management
    private (Vector3 knotPosition, Vector3 tangentIn, Vector3 tangentOut) CalculateKnot(
        Vector2 prevNodePos, Vector2 currNodePos, Vector2 nextNodePos,
        PathCase pathCase, GridDirection dirIn, GridDirection dirOut)
    {
        Vector3 prev3DNodePos = new Vector3(prevNodePos.x, 0f, prevNodePos.y);
        Vector3 curr3DNodePos = new Vector3(currNodePos.x, 0f, currNodePos.y);
        Vector3 next3DNodePos = new Vector3(nextNodePos.x, 0f, nextNodePos.y);

        Vector3 knotPosition = prev3DNodePos;
        Vector3 tangentIn = Vector3.zero;
        Vector3 tangentOut = Vector3.zero;

        switch (pathCase)
        {
            case PathCase.Straight:
                break;

            case PathCase.Corner:
                if (dirIn == GridDirection.Up)
                    if (dirOut == GridDirection.Right)
                    {
                        knotPosition = prev3DNodePos;
                        tangentOut = Vector3.forward;
                    }
                    else if (dirOut == GridDirection.Left)
                    {
                        knotPosition = prev3DNodePos;
                        tangentOut = Vector3.forward;
                    }
                //if (dirIn == GridDirection.Down)
                //    if (dirOut == GridDirection.Right)
                //        tangentIn = Vector3.back;
                //    else if (dirOut == GridDirection.Left)
                //        tangentIn = Vector3.forward;
                //if (dirIn == GridDirection.Right)
                //    if (dirOut == GridDirection.Up)
                //        tangentIn = Vector3.forward;
                //    else if (dirOut == GridDirection.Down)
                //        tangentIn = Vector3.back;
                //else if (dirIn == GridDirection.Left)
                //    if (dirOut == GridDirection.Up)
                //        tangentIn = Vector3.back;
                //    else if (dirOut == GridDirection.Down)
                //        tangentIn = Vector3.forward;
                break;

            case PathCase.Diagonal:
                // ...
                break;

            default:
                knotPosition = curr3DNodePos;
                break;
        }

        return (knotPosition, tangentIn, tangentOut);
    }

    // Identify Direction Between Nodes
    public GridDirection GetDirection(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;
        int dx = delta.x;
        int dy = delta.y;

        if (dx == 0 && dy > 0) return GridDirection.Up;
        if (dx == 0 && dy < 0) return GridDirection.Down;
        if (dy == 0 && dx > 0) return GridDirection.Right;
        if (dy == 0 && dx < 0) return GridDirection.Left;

        if (dx > 0 && dy > 0) return GridDirection.DiagUpRight;
        if (dx < 0 && dy > 0) return GridDirection.DiagUpLeft;
        if (dx > 0 && dy < 0) return GridDirection.DiagDownRight;
        if (dx < 0 && dy < 0) return GridDirection.DiagDownLeft;

        return GridDirection.None;
    }

    public PathCase GetPathCase(GridDirection dirIn, GridDirection dirOut)
    {
        if (dirIn == dirOut)
            return PathCase.Straight;

        if (IsVertical(dirIn) && IsHorizontal(dirOut) ||
            IsHorizontal(dirIn) && IsVertical(dirOut))
            return PathCase.Corner;

        if (IsDiagonal(dirIn) && (IsVertical(dirOut) || IsHorizontal(dirOut)) ||
            IsDiagonal(dirOut) && (IsVertical(dirIn) || IsHorizontal(dirIn)))
            return PathCase.Diagonal;

        return PathCase.Edge;
    }

    private bool IsVertical(GridDirection dir) { return dir == GridDirection.Up || dir == GridDirection.Down; }
    private bool IsHorizontal(GridDirection dir) { return dir == GridDirection.Left || dir == GridDirection.Right;}
    private bool IsDiagonal(GridDirection dir) { return dir == GridDirection.DiagUpRight || dir == GridDirection.DiagUpLeft ||
            dir == GridDirection.DiagDownRight || dir == GridDirection.DiagDownLeft; }
}

public enum GridDirection
{
    Up,             // (0, +1)
    Down,           // (0, -1)
    Right,          // (+1, 0)
    Left,           // (-1, 0)
    DiagUpRight,    // (+1, +1)
    DiagUpLeft,     // (-1, +1)
    DiagDownRight,  // (+1, -1)
    DiagDownLeft,   // (-1, -1)
    None
}

public enum PathCase
{
    Straight,
    Corner,
    Diagonal,
    Edge
}
