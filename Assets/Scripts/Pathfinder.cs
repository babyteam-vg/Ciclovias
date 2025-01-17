using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pathfinder
{
    private Graph graph;

    // === Methods ===
    // Constructor
    public Pathfinder(Graph graph) { this.graph = graph; }

    // A* Algorythm
    public (bool pathFound, List<Vector2Int> path) FindPath(Vector2Int start, Vector2Int end)
    {
        // Start and End Nodes
        Node startNode = graph.GetNode(start);
        Node endNode = graph.GetNode(end);

        // Open and Closed Lists
        List<PathNode> openList = new List<PathNode>();
        HashSet<PathNode> closedList = new HashSet<PathNode>();

        if (startNode == null || endNode == null)
            return (false, null);
        //          Add Start to the Open List <¬
        PathNode startPathNode = new PathNode(startNode);
        openList.Add(startPathNode);

        PathNode closestNodeToDestination = startPathNode;

        while (openList.Count > 0)
        {
            // Lowest fCost in the Open List
            PathNode currentNode = openList[0];

            foreach (PathNode node in openList)
                if (node.fCost < currentNode.fCost || (node.fCost == currentNode.fCost && node.hCost < currentNode.hCost))
                    currentNode = node;

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // End Found
            if (currentNode.node == endNode)
                return (true, RetracePath(startPathNode, currentNode));

            if (currentNode.hCost < closestNodeToDestination.hCost)
                closestNodeToDestination = currentNode;

            // Process Neighbors
            foreach (Node neighbor in currentNode.node.neighbors)
            {
                PathNode neighborPathNode = new PathNode(neighbor);

                if (closedList.Contains(neighborPathNode))
                    continue;
                //                   Start to Node Cost <¬
                float tentativeGCost = currentNode.gCost + Vector2.Distance(currentNode.node.position, neighbor.position);

                if (tentativeGCost < neighborPathNode.gCost || !openList.Contains(neighborPathNode))
                { //           Assign Costs <¬
                    neighborPathNode.gCost = tentativeGCost;
                    neighborPathNode.hCost = Vector2.Distance(neighbor.position, endNode.position);
                    neighborPathNode.fCost = neighborPathNode.gCost + neighborPathNode.hCost;
                    neighborPathNode.parentNode = currentNode;

                    if (!openList.Contains(neighborPathNode))
                        openList.Add(neighborPathNode);
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

public class PathNode
{
    public Node node;
    public float gCost; // Cost from the Start
    public float hCost; // Heuristic Cost
    public float fCost; // Total Coost
    public PathNode parentNode;

    public PathNode(Node node)
    {
        this.node = node;
        this.gCost = float.MaxValue;
        this.hCost = 0;
        this.fCost = 0;
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