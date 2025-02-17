using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int position;
    public Vector2 worldPosition;
    public List<Node> neighbors;

    // === Methods ===
    // Constructor
    public Node(Vector2Int position, Vector2 worldPosition)
    {
        this.position = position;
        this.worldPosition = worldPosition;
        this.neighbors = new List<Node>();
    }

    // O- + -O
    public void AddNeighbor(Node neighbor)
    {
        if (!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
            neighbor.neighbors.Add(this);
        }
    }

    // O- X -O
    public void RemoveNeighbor(Node neighbor)
    {
        if (neighbors.Contains(neighbor))
        {
            neighbors.Remove(neighbor);
            neighbor.neighbors.Remove(this);
        }
    }
}
