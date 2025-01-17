using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    private Dictionary<Vector2Int, Node> nodes;

    // === Methods ===
    // Constructor
    public Graph() { nodes = new Dictionary<Vector2Int, Node>(); }

    // Get a Node
    public Node GetNode(Vector2Int position)
    {
        nodes.TryGetValue(position, out Node node);
        return node;
    }

    // Add a Node
    public void AddNode(Vector2Int position, Vector2 worldPosition)
    {
        if (!nodes.ContainsKey(position))
            nodes[position] = new Node(position, worldPosition);
    }

    // Remove a Node
    public void RemoveNode(Vector2Int position)
    {
        if (nodes.TryGetValue(position, out Node node))
        {
            foreach (Node neighbor in node.neighbors)
                neighbor.neighbors.Remove(node);
            nodes.Remove(position);
        }
    }

    // Connect 2 Nodes
    public void AddEdge(Vector2Int positionA, Vector2Int positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
        {
            Node nodeA = nodes[positionA];
            Node nodeB = nodes[positionB];
            nodeA.AddNeighbor(nodeB);
        }
    }

    // Disconnect 2 Nodes
    public void RemoveEdge(Vector2Int positionA, Vector2Int positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
        {
            Node nodeA = nodes[positionA];
            Node nodeB = nodes[positionB];
            nodeA.RemoveNeighbor(nodeB);
        }
    }

    // Check Connection
    public bool AreConnected(Vector2Int positionA, Vector2Int positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
            return nodes[positionA].neighbors.Contains(nodes[positionB]);

        return false;
    }

    // Get All Nodes
    public List<Node> GetAllNodes() { return new List<Node>(nodes.Values); }

    // Drawing the Nodes (Debug)
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (var node in nodes.Values)
        {
            Vector3 worldPosition = new Vector3(node.worldPosition.x, 0, node.worldPosition.y);
            Gizmos.DrawSphere(worldPosition, 0.2f);

            foreach (var neighbor in node.neighbors)
            {
                Vector3 neighborPosition = new Vector3(neighbor.worldPosition.x, 0, neighbor.worldPosition.y);
                Gizmos.DrawLine(worldPosition, neighborPosition);
            }
        }
    }
}
