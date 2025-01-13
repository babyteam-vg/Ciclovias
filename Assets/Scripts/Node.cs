using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2 position;
    public List<Node> neighbors;

    // === Methods ===
    // Construcutor
    public Node(Vector2 position)
    {
        this.position = position;
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
