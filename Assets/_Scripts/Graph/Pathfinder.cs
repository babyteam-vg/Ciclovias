using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    private Graph graph;

    // === Methods ===
    // Constructor
    public Pathfinder(Graph graph) { this.graph = graph; }

    // A* Algorythm
    public (bool pathFound, List<Vector2Int> path) FindPath(Vector2Int start, Vector2Int midPoint, Vector2Int end)
    {
        // First Stretch
        var (firstPathFound, firstPath) = FindPathSegment(start, midPoint);
        if (!firstPathFound)
            return (false, firstPath); // No se encontró un camino al punto intermedio

        // Second Stretch
        var (secondPathFound, secondPath) = FindPathSegment(midPoint, end);
        if (!secondPathFound)
            return (false, firstPath); // No se encontró un camino al destino

        // Unify Stretchs
        firstPath.Add(midPoint);
        firstPath.AddRange(secondPath);

        return (true, firstPath);
    }

    private (bool pathFound, List<Vector2Int> path) FindPathSegment(Vector2Int start, Vector2Int end)
    {
        // Start and End Nodes
        Node startNode = graph.GetNode(start);
        if (startNode == null)
            return (false, null);

        Node endNode = graph.GetNode(end);
        bool endNodeExists = endNode != null;

        // Open and Closed Sets
        var openSet = new SortedSet<PathNode>(new PathNodeComparer());
        var closedSet = new HashSet<PathNode>();
        //          Add Start to the Open Set <¬
        PathNode startPathNode = new PathNode(startNode, (start - end).sqrMagnitude);
        openSet.Add(startPathNode);

        PathNode closestNodeToDestination = startPathNode; // Negative Case

        while (openSet.Count > 0)
        {
            // Get Node w/Lowest fCost
            PathNode currentNode = openSet.Min;
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // End Found
            if (endNodeExists && currentNode.node == endNode)
                return (true, RetracePath(startPathNode, currentNode));

            if (currentNode.hCost < closestNodeToDestination.hCost) // Negative Case
                closestNodeToDestination = currentNode;

            // Process Neighbors
            foreach (Node neighbor in currentNode.node.neighbors)
            {
                PathNode neighborPathNode = new PathNode(neighbor);

                if (closedSet.Contains(neighborPathNode))
                    continue;
                //            Start to Node Cost <¬
                float tentativeGCost = currentNode.gCost + (currentNode.node.position - neighbor.position).sqrMagnitude;

                if (!openSet.Contains(neighborPathNode) || tentativeGCost < neighborPathNode.gCost)
                { //           Assign Costs <¬
                    neighborPathNode.gCost = tentativeGCost;
                    neighborPathNode.hCost = endNodeExists
                        ? (neighbor.position - endNode.position).sqrMagnitude
                        : (neighbor.position - end).sqrMagnitude;
                    neighborPathNode.fCost = neighborPathNode.gCost + neighborPathNode.hCost;
                    neighborPathNode.parentNode = currentNode;

                    if (!openSet.Contains(neighborPathNode))
                        openSet.Add(neighborPathNode);
                }
            }
        }

        return (false, RetracePath(startPathNode, closestNodeToDestination));
    }

    // End to Start (Fittest Path)
    private List<Vector2Int> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        PathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.node.position);
            currentNode = currentNode.parentNode;
        }

        path.Reverse();
        return path;
    }
}

// Comparer for PathNode
class PathNodeComparer : IComparer<PathNode>
{
    public int Compare(PathNode x, PathNode y)
    {
        int compare = x.fCost.CompareTo(y.fCost);
        if (compare == 0)
            compare = x.hCost.CompareTo(y.hCost);
        return compare;
    }
}

public class PathNode
{
    public Node node;
    public float gCost; // Cost from the Start
    public float hCost; // Heuristic Cost
    public float fCost; // Total Coost
    public PathNode parentNode;

    public PathNode(Node node, float hCost = 0f)
    {
        this.node = node;
        this.gCost = float.MaxValue;
        this.hCost = hCost;
        this.fCost = gCost + hCost;
        this.parentNode = null;
    }

    public override bool Equals(object obj)
    {
        if (obj is PathNode other)
            return node.position == other.node.position;

        return false;
    }

    public override int GetHashCode() { return node.position.GetHashCode(); }
}