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
        public List<Vector2Int> neighbors;
        public bool indestructible;
        public List<Vector2Int> intersectionEdges;
        public List<Vector2Int> blockedPositions;
    }

    public List<SerializableNode> nodes = new List<SerializableNode>();
}