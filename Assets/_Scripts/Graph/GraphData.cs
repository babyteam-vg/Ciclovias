using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GraphData
{
    [System.Serializable]
    public class SerializableNode
    {
        public Vector2Int position;
        public Vector2 worldPosition;
        public bool indestructible;
    }

    [System.Serializable]
    public class SerializableEdge
    {
        public Vector2Int nodeA;
        public Vector2Int nodeB;
    }

    public List<SerializableNode> nodes = new List<SerializableNode>();
    public List<SerializableEdge> edges = new List<SerializableEdge>();
}