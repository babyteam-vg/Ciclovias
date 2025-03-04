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

    // ::::: Add a Node
    public void AddNode(Vector2Int position, Vector2 worldPosition)
    {
        if (!nodes.ContainsKey(position))
        {
            nodes[position] = new Node(position, worldPosition);
            nodePositions.Add(position);
            OnNodeAdded?.Invoke(position);
        }
    }

    // ::::: Remove a Node
    public void RemoveNode(Vector2Int position)
    {
        if (nodes.TryGetValue(position, out Node node))
        {
            foreach (Node neighbor in node.neighbors)
            {
                neighbor.neighbors.Remove(node);
                OnEdgeRemoved?.Invoke(position, neighbor.position);
            }

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
            OnEdgeRemoved?.Invoke(positionA, positionB);
        }
    }

    // ::::: Check Direct Connection Between 2 Nodes
    public bool AreConnected(Vector2Int positionA, Vector2Int positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
            return nodes[positionA].neighbors.Contains(nodes[positionB]);

        return false;
    }

    // ::::: Get All Nodes
    public List<Node> GetAllNodes() { return new List<Node>(nodes.Values); }

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

    // ::::: Check if 2 Nodes are Connected
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

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Graph -> GraphData
    public GraphData SaveGraph()
    {
        GraphData graphData = new GraphData();

        // Nodes
        foreach (var node in nodes.Values)
        {
            graphData.nodes.Add(new GraphData.SerializableNode
            {
                position = node.position,
                worldPosition = node.worldPosition
            });
        }

        // Edges
        foreach (var node in nodes.Values)
        {
            foreach (var neighbor in node.neighbors)
            {
                if (node.position.x < neighbor.position.x || // Prrevent Duplicates
                    (node.position.x == neighbor.position.x && node.position.y < neighbor.position.y))
                {
                    graphData.edges.Add(new GraphData.SerializableEdge
                    {
                        nodeA = node.position,
                        nodeB = neighbor.position
                    });
                }
            }
        }

        return graphData;
    }

    // ::::: GraphData -> Graph
    public void LoadGraph(GraphData graphData)
    {
        nodes.Clear();
        nodePositions.Clear();

        foreach (var serializableNode in graphData.nodes) // Nodes
            AddNode(serializableNode.position, serializableNode.worldPosition);

        foreach (var serializableEdge in graphData.edges) // Edges
            AddEdge(serializableEdge.nodeA, serializableEdge.nodeB);
    }
}