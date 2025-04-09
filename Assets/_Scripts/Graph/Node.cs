using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int position;
    public Vector2 worldPosition;
    public List<Node> neighbors;
    public bool indestructible;
    public List<Vector2Int> intersectionEdges;
    public List<Vector2Int> blockedPositions;

    // :::::::::: METHODS ::::::::::
    // ::::: Constructor
    public Node(Vector2Int position, Vector2 worldPosition, bool indestructible, List<Vector2Int> intersectionEdges, List<Vector2Int> blockedPositions)
    {
        this.position = position;
        this.worldPosition = worldPosition;
        this.neighbors = new List<Node>();
        this.indestructible = indestructible;
        this.intersectionEdges = intersectionEdges;
        this.blockedPositions = blockedPositions;
    }

    // ::::: O- + -O
    public void AddNeighbor(Node neighbor)
    {
        if (!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
            neighbor.neighbors.Add(this);
        }
    }

    // ::::: O- X -O
    public void RemoveNeighbor(Node neighbor)
    {
        if (neighbors.Contains(neighbor))
        {
            neighbors.Remove(neighbor);
            neighbor.neighbors.Remove(this);
        }
    }
}
