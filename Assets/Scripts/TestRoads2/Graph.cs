using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    private Dictionary<Vector2, Node> nodes;

    // === Methods ===
    // Constructor
    public Graph() { nodes = new Dictionary<Vector2, Node>(); }

    // Get a Node
    public Node GetNode(Vector2 position)
    {
        nodes.TryGetValue(position, out Node node);
        return node;
    }

    // Add a Node
    public void AddNode(Vector2 position)
    {
        if (!nodes.ContainsKey(position))
            nodes[position] = new Node(position);
    }

    // Remove a Node
    public void RemoveNode(Vector2 position)
    {
        if (nodes.TryGetValue(position, out Node node))
        {
            foreach (Node neighbor in node.neighbors)
                neighbor.neighbors.Remove(node);
            nodes.Remove(position);
        }
    }

    // Connect 2 Nodes
    public void AddEdge(Vector2 positionA, Vector2 positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
        {
            Node nodeA = nodes[positionA];
            Node nodeB = nodes[positionB];
            nodeA.AddNeighbor(nodeB);
        }
    }

    // Disconnect 2 Nodes
    public void RemoveEdge(Vector2 positionA, Vector2 positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
        {
            Node nodeA = nodes[positionA];
            Node nodeB = nodes[positionB];
            nodeA.RemoveNeighbor(nodeB);
        }
    }

    // Check Connection
    public bool AreConnected(Vector2 positionA, Vector2 positionB)
    {
        if (nodes.ContainsKey(positionA) && nodes.ContainsKey(positionB))
            return nodes[positionA].neighbors.Contains(nodes[positionB]);

        return false;
    }

    // Get All Nodes
    public List<Node> GetAllNodes() { return new List<Node>(nodes.Values); }

    // Debug Drawing
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (var node in nodes.Values)
        {
            Vector3 worldPosition = new Vector3(node.position.x, 0, node.position.y);
            Gizmos.DrawSphere(worldPosition, 0.2f);

            foreach (var neighbor in node.neighbors)
            {
                Vector3 neighborPosition = new Vector3(neighbor.position.x, 0, neighbor.position.y);
                Gizmos.DrawLine(worldPosition, neighborPosition);
            }
        }
    }
}
