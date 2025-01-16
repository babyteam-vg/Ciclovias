using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] private Graph graph;

    // === Methods ===
    // Constructor
    public Pathfinder(Graph graph)
    {
        this.graph = graph;
    }

    // A* Algorythm
    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        // Start and End Nodes
        Node startNode = graph.GetNode(start);
        Node endNode = graph.GetNode(end);

        // Open and Closed Lists
        List<PathNode> openList = new List<PathNode>();
        HashSet<PathNode> closedList = new HashSet<PathNode>();

        if (startNode == null || endNode == null)
            return null;
        //          Add Start to the Open List <�
        PathNode startPathNode = new PathNode(startNode);
        openList.Add(startPathNode);

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
                return RetracePath(startPathNode, currentNode);

            // Process Neighbors
            foreach (Node neighbor in currentNode.node.neighbors)
            {
                PathNode neighborPathNode = new PathNode(neighbor);

                if (closedList.Contains(neighborPathNode))
                    continue;
                //                   Start to Node Cost <�
                float tentativeGCost = currentNode.gCost + Vector2.Distance(currentNode.node.position, neighbor.position);

                if (tentativeGCost < neighborPathNode.gCost || !openList.Contains(neighborPathNode))
                { //           Assign Costs <�
                    neighborPathNode.gCost = tentativeGCost;
                    neighborPathNode.hCost = Vector2.Distance(neighbor.position, endNode.position);
                    neighborPathNode.fCost = neighborPathNode.gCost + neighborPathNode.hCost;
                    neighborPathNode.parentNode = currentNode;

                    if (!openList.Contains(neighborPathNode))
                        openList.Add(neighborPathNode);
                }
            }
        }

        Debug.LogWarning("A* Couldn't Find a Path");
        return null;
    }

    // End to Start (Fittest Path)
    private List<Vector2> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<Vector2> path = new List<Vector2>();
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