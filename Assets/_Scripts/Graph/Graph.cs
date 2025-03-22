using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Graph : MonoBehaviour
{
    public Dictionary<Vector2Int, Node> nodes;
    private HashSet<Vector2Int> nodePositions;

    public event Action<Vector2Int> OnNodeAdded;
    public event Action<Vector2Int, Vector2Int> OnEdgeAdded;
    public event Action<Vector2Int> OnNodeRemoved;
    public event Action<Vector2Int, Vector2Int> OnEdgeRemoved;
    public event Action<Vector2Int> OnLonelyNodeRemoved;

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Constructor
    public Graph()
    {
        nodes = new Dictionary<Vector2Int, Node>();
        nodePositions = new HashSet<Vector2Int>();
    }

    // ::::: Get a Node
    public Node GetNode(Vector2Int position)
    {
        nodes.TryGetValue(position, out Node node);
        return node;
    }

    // ::::: Get All Edges
    public List<(Vector2Int, Vector2Int)> GetAllEdges()
    {
        List<(Vector2Int, Vector2Int)> edges = new List<(Vector2Int, Vector2Int)>();

        foreach (var node in nodes.Values)
            foreach (var neighbor in node.neighbors)
                if (node.position.x < neighbor.position.x ||
                    (node.position.x == neighbor.position.x && node.position.y < neighbor.position.y))
                    edges.Add((node.position, neighbor.position));

        return edges;
    }


    // ::::: Get Node's Neighbors
    public int GetNeighborsCount(Vector2Int position)
    {
        Node node = GetNode(position);
        if (node != null)
            return GetNode(position).neighbors.Count;
        return 0;
    }
    public List<Vector2Int> GetNeighborsPos(Vector2Int position)
    {
        Node node = GetNode(position);
        if (node != null)
        {
            List<Vector2Int> neighborPositions = new List<Vector2Int>();
            foreach (Node neighbor in node.neighbors)
                neighborPositions.Add(neighbor.position);
            return neighborPositions;
        }
        return new List<Vector2Int>();
    }

    // ::::: Add a Node
    public void AddNode(Vector2Int position, Vector2 worldPosition, bool indestructible = false)
    {
        if (!nodes.ContainsKey(position))
        {
            nodes[position] = new Node(position, worldPosition, indestructible);
            nodePositions.Add(position);
            OnNodeAdded?.Invoke(position);
        }
    }

    // ::::: Remove a Node
    public void RemoveNode(Vector2Int position)
    {
        if (nodes.TryGetValue(position, out Node node))
        {
            foreach (Node neighbor in new List<Node>(node.neighbors))
                RemoveEdge(position, neighbor.position);

            nodes.Remove(position);
            nodePositions.Remove(position);
            OnNodeRemoved?.Invoke(position);
        }
    }

    // ::::: Connect 2 Nodes
    public void AddEdge(Vector2Int positionA, Vector2Int positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
        {
            Node nodeA = nodes[positionA];
            Node nodeB = nodes[positionB];
            nodeA.AddNeighbor(nodeB);
            OnEdgeAdded?.Invoke(positionA, positionB);
        }
    }

    // ::::: Disconnect 2 Nodes
    public void RemoveEdge(Vector2Int positionA, Vector2Int positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
        {
            Node nodeA = nodes[positionA];
            Node nodeB = nodes[positionB];

            nodeA.RemoveNeighbor(nodeB);
            nodeB.RemoveNeighbor(nodeA);

            OnEdgeRemoved?.Invoke(positionA, positionB);

            if (nodeA.neighbors.Count == 0) // Lonely Node
            {
                nodes.Remove(positionA);
                nodePositions.Remove(positionA);
                OnLonelyNodeRemoved?.Invoke(positionA);
            }

            if (nodeB.neighbors.Count == 0)
            {
                nodes.Remove(positionB);
                nodePositions.Remove(positionB);
                OnLonelyNodeRemoved?.Invoke(positionB);
            }
        }
    }

    // ::::: Check If 2 Nodes Are Neighbors
    public bool AreNeighbors(Vector2Int positionA, Vector2Int positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
            return nodes[positionA].neighbors.Contains(nodes[positionB]);

        return false;
    }

    // ::::: Get All Nodes
    public List<Node> GetAllNodes() { return new List<Node>(nodes.Values); }
    public HashSet<Vector2Int> GetAllNodesPosition() { return nodePositions; }

    // ::::: Is Any of These (x, y) in the Graph?
    public bool ContainsAny(IEnumerable<Vector2Int> positions)
    {
        foreach (var position in positions)
            if (nodePositions.Contains(position))
                return true;

        return false;
    }

    // ::::: First Node (Coord) in Group of Cells
    public Vector2Int? FindNodePosInCells(List<Vector2Int> cells)
    {
        foreach (var cell in cells)
            if (nodePositions.Contains(cell))
                return cell;

        return null;
    }

    // ::::: 
    public Vector2Int? GetFirstAdjacentNodePosition(Vector2Int position)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0),   // Right
            new Vector2Int(1, 1),   // Up-Right
            new Vector2Int(1, -1),  // Down-Right
            new Vector2Int(-1, 1),  // Up-Left
            new Vector2Int(-1, -1)  // Down-Left
        };

        foreach (var dir in directions)
        {
            Vector2Int adjacentPosition = position + dir;
            if (nodePositions.Contains(adjacentPosition))
                return adjacentPosition;
        }

        return null;
    }

    // ::::: Remove a Lonely Node
    public void CheckAndRemoveNode(Vector2Int position)
    {
        Node node = GetNode(position);

        if (node != null && node.neighbors.Count == 0)
        {
            RemoveNode(position);
            OnLonelyNodeRemoved?.Invoke(position);
        }
    }

    // ::::: Check If 2 Separated Nodes are Connected
    public bool AreConnectedByPath(Vector2Int positionA, Vector2Int positionB)
    {
        if (!nodes.ContainsKey(positionA) || !nodes.ContainsKey(positionB))
            return false;

        Node nodeA = nodes[positionA];
        Node nodeB = nodes[positionB];

        if (nodeA == null || nodeB == null)
            return false;

        var visited = new HashSet<Node>();
        var stack = new Stack<Node>();
        stack.Push(nodeA);

        while (stack.Count > 0)
        {
            Node current = stack.Pop();
            if (current == nodeB) return true;

            if (!visited.Contains(current))
            {
                visited.Add(current);
                foreach (Node neighbor in current.neighbors)
                    stack.Push(neighbor);
            }
        }

        return false;
    }

    // ::::: Seal Nodes
    public void SealNodes(List<Vector2Int> listOfNodes)
    {
        foreach (Vector2Int nodePos in listOfNodes)
            GetNode(nodePos).indestructible = true;
    }

    // ::::: Detect Collinearity
    public bool IsCollinear(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        Vector2Int v1 = b - a;
        Vector2Int v2 = c - b;

        int crossProduct = v1.x * v2.y - v1.y * v2.x;

        return crossProduct == 0;
    }

    // ::::: Any Indestructible Node in the Path?
    public bool ContainsIndestructibleNode(List<Vector2Int> positions)
    {
        foreach (var position in positions)
            if (nodes.TryGetValue(position, out Node node) && node.indestructible)
                return true;

        return false;
    }

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Graph -> GraphData
    public GraphData SaveGraph()
    {
        GraphData graphData = new GraphData();

        foreach (var node in nodes.Values)
            graphData.nodes.Add(new GraphData.SerializableNode
            {
                position = node.position,
                worldPosition = node.worldPosition,
                indestructible = node.indestructible,
                neighbors = node.neighbors.ConvertAll(neighbor => neighbor.position)
            });

        return graphData;
    }

    // ::::: GraphData -> Graph
    public void LoadGraph(GraphData graphData)
    {
        nodes.Clear();
        nodePositions.Clear();

        // Nodes
        foreach (var nodeData in graphData.nodes)
            AddNode(nodeData.position, nodeData.worldPosition, nodeData.indestructible);

        // Edges
        foreach (var nodeData in graphData.nodes)
            if (nodes.TryGetValue(nodeData.position, out Node node))
                foreach (var neighborPos in nodeData.neighbors)
                    if (nodes.ContainsKey(neighborPos))
                    {
                        Node neighbor = nodes[neighborPos];
                        node.AddNeighbor(neighbor);
                    }
    }
}