using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder
{
    private Graph graph;

    // === Methods ===
    // Constructor
    public Pathfinder(Graph graph) { this.graph = graph; }

    // A* Algorythm                                                         Player Input <¬
    public (bool pathFound, List<Vector2Int> path) FindPath(Vector2Int start, Vector2Int midPoint, Vector2Int end)
    {
        // Start and End Nodes
        Node startNode = graph.GetNode(start);
        Node midNode = graph.GetNode(midPoint);
        Node endNode = graph.GetNode(end);

        if (startNode == null || midNode == null)
            return (false, null);

        // Open and Closed Sets
        var openSet = new SortedSet<PathNode>(new PathNodeComparer());
        var closedSet = new HashSet<PathNode>();

        // Add Start Node to Open Set
        PathNode startPathNode = new PathNode(startNode, (start - midPoint).sqrMagnitude);
        openSet.Add(startPathNode);

        PathNode closestNodeToMid = startPathNode; // 1st Segment
        PathNode closestNodeToEnd = null; // 2nd Segment

        bool midReached = false;

        while (openSet.Count > 0)
        {
            // Get Node w/Lowest fCost
            PathNode currentNode = openSet.Min;
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Check if MidPoint is Reached
            if (!midReached && currentNode.node == midNode)
            {
                midReached = true;
                closestNodeToEnd = currentNode; // Start the 2nd Segment
            }

            // End Found (Passing Through Mid)
            if (midReached && currentNode.node == endNode)
                return (true, RetracePath(startPathNode, currentNode));

            // Update Closest Nodes for Negative Cases
            if (!midReached && currentNode.hCost < closestNodeToMid.hCost)
                closestNodeToMid = currentNode;

            if (midReached && (closestNodeToEnd == null || currentNode.hCost < closestNodeToEnd.hCost))
                closestNodeToEnd = currentNode;

            // Process Neighbors
            foreach (Node neighbor in currentNode.node.neighbors)
            {
                PathNode neighborPathNode = new PathNode(neighbor);

                if (closedSet.Contains(neighborPathNode))
                    continue;

                // Calculate Tentative gCost
                float tentativeGCost = currentNode.gCost + (currentNode.node.position - neighbor.position).sqrMagnitude;

                if (!openSet.Contains(neighborPathNode) || tentativeGCost < neighborPathNode.gCost)
                {
                    neighborPathNode.gCost = tentativeGCost;

                    // Set hCost Based on Current Segment
                    if (!midReached)
                        // 1st Segment: Heuristic is Distance to Mid
                        neighborPathNode.hCost = (neighbor.position - midPoint).sqrMagnitude;
                    else
                        // 2nd Segment: Heuristic is Distance to End
                        neighborPathNode.hCost = (neighbor.position - end).sqrMagnitude;

                    neighborPathNode.fCost = neighborPathNode.gCost + neighborPathNode.hCost;
                    neighborPathNode.parentNode = currentNode;

                    if (!openSet.Contains(neighborPathNode))
                        openSet.Add(neighborPathNode);
                }
            }
        }

        if (!midReached)
            // Could Not Rach Mid
            return (false, RetracePath(startPathNode, closestNodeToMid));
        else
            // Reached Mid, But Not End
            return (false, RetracePath(startPathNode, closestNodeToEnd));
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