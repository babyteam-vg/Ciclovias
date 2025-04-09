using System;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();
    private HashSet<Vector2Int> nodePositions = new HashSet<Vector2Int>();

    public event Action<Vector2Int> NodeAdded;
    public event Action<Vector2Int, Vector2Int> EdgeAdded;
    public event Action<Vector2Int, Vector2Int> IntersectionAdded;

    public event Action<Vector2Int> NodeRemoved;
    public event Action<Vector2Int, Vector2Int> EdgeRemoved;
    public event Action<Vector2Int, Vector2Int> IntersectionRemoved;

    public event Action<Vector2Int> LonelyNodeRemoved;

    // :::::::::: GETTERS METHODS ::::::::::
    // ::::: Get All Nodes
    public HashSet<Node> GetAllNodes() { return new HashSet<Node>(nodes.Values); }
    public HashSet<Vector2Int> GetAllNodesPosition() { return nodePositions; }

    // ::::: Get a Node
    public Node GetNode(Vector2Int position)
    {
        nodes.TryGetValue(position, out Node node);
        return node;
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

    // :::::::::: EXPANSION METHODS ::::::::::
    // ::::: Add a Node
    public void AddNode(Vector2Int position, Vector2 worldPosition, bool indestructible = false, List<Vector2Int> intersectionEdges = null, List<Vector2Int> blockedPositions = null)
    {
        if (!nodes.ContainsKey(position))
        {
            intersectionEdges = intersectionEdges == null ? new List<Vector2Int>() : intersectionEdges;
            blockedPositions = blockedPositions == null ? new List<Vector2Int>() : blockedPositions;
            nodes[position] = new Node(position, worldPosition, indestructible, intersectionEdges, blockedPositions);
            nodePositions.Add(position);
        }
    }

    // ::::: Remove a Node (Never)
    public void RemoveNode(Vector2Int position)
    {
        if (nodes.TryGetValue(position, out Node node))
        {
            foreach (Node neighbor in new List<Node>(node.neighbors))
                RemoveEdge(position, neighbor.position);

            nodes.Remove(position);
            nodePositions.Remove(position);
        }
    }

    // ::::: Connect 2 Nodes
    public void AddEdge(Vector2Int positionA, Vector2Int positionB)
    {
        Node nodeA = GetNode(positionA);
        Node nodeB = GetNode(positionB);
        nodeA.AddNeighbor(nodeB);

        EdgeAdded?.Invoke(positionA, positionB); // !
    }

    // ::::: Disconnect 2 Nodes
    public void RemoveEdge(Vector2Int positionA, Vector2Int positionB)
    {
        Node nodeA = GetNode(positionA);
        Node nodeB = GetNode(positionB);
        nodeA.RemoveNeighbor(nodeB);
        nodeB.RemoveNeighbor(nodeA);

        EdgeRemoved?.Invoke(positionA, positionB);
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Check If a Position is Part of a Bike Lane
    public bool AreBuilt(List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            int neighbors = GetNeighborsCount(position);
            if (neighbors > 0)
                return true;
        }

        return false;
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

    // ::::: Blocked Positions Management (Intersections)
    public void NewIntersection(Vector2Int edgePos, Vector2Int intersectionPos, Node inputNode, Vector2Int inputPos)
    {
        Node edgeNode = GetNode(edgePos);

        // Input Node (First Edge of the Intersection)
        AddIntersectionsAndBlockPositions(edgePos, intersectionPos, inputNode, inputPos);

        // Edge Node (Second Edge of the Intersection)
        AddIntersectionsAndBlockPositions(inputPos, intersectionPos, edgeNode, edgePos);

        IntersectionAdded?.Invoke(inputPos, edgePos); // !
    }
    private void AddIntersectionsAndBlockPositions(Vector2Int edgePos, Vector2Int intersectionPos, Node inputNode, Vector2Int inputPos)
    {
        // Intersection Edges
        if (!inputNode.intersectionEdges.Contains(edgePos))
            inputNode.intersectionEdges.Add(edgePos);

        // Blocked Positions
        List<Vector2Int> blocked = GetNonCollinearBlockedPositions(edgePos, intersectionPos, inputPos);
        inputNode.blockedPositions = blocked;

        foreach (Vector2Int blockedPosition in inputNode.blockedPositions)
        {
            Node blockedNode = GetNode(blockedPosition);
            if (blockedNode != null)
                blockedNode.blockedPositions.Add(inputNode.position); // Reciprocity (Input)
        }
    }

    public void RemoveIntersection(Node inputNode, Vector2Int inputPos, Node edgeNode, Vector2Int edgePos)
    {
        RemoveIntersectionsAndBlockedPositions(inputNode, inputPos, edgePos);
        RemoveIntersectionsAndBlockedPositions(edgeNode, edgePos, inputPos);

        IntersectionRemoved?.Invoke(inputPos, edgePos); // !
    }
    private void RemoveIntersectionsAndBlockedPositions(Node node, Vector2Int inputPos, Vector2Int edgePos)
    {
        node.intersectionEdges.Remove(edgePos);
        if (node.intersectionEdges.Count == 0)
        {
            foreach (Vector2Int blockedPosition in node.blockedPositions)
            {
                Node blockedNode = GetNode(blockedPosition);
                if (blockedNode != null)
                    blockedNode.blockedPositions.Remove(node.position);
            }

            node.blockedPositions.Clear();
        }
    }

    // ::::: Check If 2 Nodes Are Neighbors
    public bool AreNeighbors(Vector2Int positionA, Vector2Int positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
            return nodes[positionA].neighbors.Contains(nodes[positionB]);

        return false;
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

    // ::::: Intersection Management
    public List<Vector2Int> GetNonCollinearBlockedPositions(Vector2Int edgePos, Vector2Int intersectionPos, Vector2Int inputPos)
    {
        List<Vector2Int> blocked = new List<Vector2Int>();

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

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = inputPos + dir;
            if (!IsCollinear(intersectionPos, inputPos, neighbor))
                blocked.Add(neighbor);
        }

        return blocked;
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
                neighbors = node.neighbors.ConvertAll(neighbor => neighbor.position),
                indestructible = node.indestructible,
                intersectionEdges = node.intersectionEdges,
                blockedPositions = node.blockedPositions
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
            AddNode(nodeData.position, nodeData.worldPosition, nodeData.indestructible, nodeData.intersectionEdges, nodeData.blockedPositions);

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