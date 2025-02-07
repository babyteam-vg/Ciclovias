using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class GraphSegmentator : MonoBehaviour
{
    [SerializeField] private Graph graph;

    private List<SegmentData> segments = new List<SegmentData>();
    private Dictionary<Node, List<SegmentData>> nodeToSegmentsMap = new Dictionary<Node, List<SegmentData>>();

    // :::::::::: Methods ::::::::::
    // New Node Added, Create or Extend Segment?
    public SegmentData CreateOrUpdateSegmentForNode(Vector2Int nodePosition)
    {
        Debug.Log("CreateOrUpdateSegmentForNode");
        Node newNode = graph.GetNode(nodePosition);
        if (newNode == null)
            return null;

        int count = newNode.neighbors.Count;
        if (count == 0) // Lonely Node
            return null;
        else if (count == 1)
        {
            Node neighbor = newNode.neighbors[0];

            SegmentData extendedSegment = TryExtendSegment(neighbor, newNode); // Extend Existing Segment
            if (extendedSegment != null)
                return extendedSegment;

            return CreateNewSegment(neighbor, newNode); // Create New Segment
        }

        return null;
    }

    private SegmentData TryExtendSegment(Node existingNode, Node newNode)
    {
        Debug.Log("TryExtendSegment");
        // 1. Is Existing Node Part of a Segment?
        if (!nodeToSegmentsMap.TryGetValue(existingNode, out var segmentList)) // No
            return null;

        // 2. Yes, It Is
        SegmentData candidate = null;
        foreach (var seg in segmentList)
        {
            var segNodes = seg.nodesInSegment;
            if (segNodes.Count > 0 && segNodes[segNodes.Count - 1] == existingNode)
            {
                candidate = seg;
                break;
            }
        }

        if (candidate == null)
            return null;

        // 3. Collinearity
        if (!IsCollinearExtension(candidate, newNode))
            return null;

        // 4.Extend
        RemoveSegment(candidate);

        SegmentData extended = new SegmentData(candidate.start, newNode.worldPosition);
        extended.nodesInSegment.AddRange(candidate.nodesInSegment);
        extended.nodesInSegment.Add(newNode);

        segments.Add(extended);
        MapSegmentToNodes(extended);

        return extended;
    }

    private bool IsCollinearExtension(SegmentData segment, Node newNode)
    {
        Debug.Log("IsCollinearExtension");
        var segNodes = segment.nodesInSegment;
        if (segNodes.Count < 2)
            return false;

        Node secondLast = segNodes[segNodes.Count - 2];
        Node last = segNodes[segNodes.Count - 1];

        Vector2 dirOld = (last.worldPosition - secondLast.worldPosition).normalized;
        Vector2 dirNew = (newNode.worldPosition - last.worldPosition).normalized;

        float dot = Vector2.Dot(dirOld, dirNew);
        return (dot > 0.99f);
    }

    private SegmentData CreateNewSegment(Node a, Node b)
    {
        Debug.Log("CreateNewSegment");
        SegmentData segment = new SegmentData(a.worldPosition, b.worldPosition);
        segment.nodesInSegment.Add(a);
        segment.nodesInSegment.Add(b);

        segments.Add(segment);
        MapSegmentToNodes(segment);

        return segment;
    }

    private void RemoveSegment(SegmentData segment)
    {
        Debug.Log("RemoveSegment");
        segments.Remove(segment);
        foreach (var node in segment.nodesInSegment)
        {
            if (nodeToSegmentsMap.TryGetValue(node, out var segList))
            {
                segList.Remove(segment);
            }
        }
    }

    private void MapSegmentToNodes(SegmentData segment)
    {
        Debug.Log("MapSegmentToNodes");
        foreach (var node in segment.nodesInSegment)
        {
            if (!nodeToSegmentsMap.ContainsKey(node))
                nodeToSegmentsMap[node] = new List<SegmentData>();

            nodeToSegmentsMap[node].Add(segment);
        }
    }

    public List<SegmentData> GetSegmentsByNodePosition(Vector2Int nodePosition)
    {
        Debug.Log("GetSegmentsByNodePosition");
        Node node = graph.GetNode(nodePosition);
        if (node == null)
            return new List<SegmentData>();

        if (nodeToSegmentsMap.TryGetValue(node, out var segList))
            return segList;
        else
            return new List<SegmentData>();
    }
}

[System.Serializable]
public class SegmentData
{
    public Vector2 start;
    public Vector2 end;
    public List<Node> nodesInSegment;

    public SegmentData(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
        nodesInSegment = new List<Node>();
    }
}
