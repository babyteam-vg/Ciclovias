//using System.Collections.Generic;
//using UnityEngine;
//using static UnityEngine.Rendering.HableCurve;

//public class GraphSegmentator : MonoBehaviour
//{
//    [SerializeField] private Graph graph;

//    private List<SegmentData> segments = new List<SegmentData>();
//    private Dictionary<Node, List<SegmentData>> nodeToSegmentsMap = new Dictionary<Node, List<SegmentData>>();

//    // === Methods ===
//    // New Node Added, Create or Extend Segment?
//    public SegmentData CreateOrUpdateSegmentForNode(Vector2Int nodePosition)
//    {
//        Node newNode = graph.GetNode(nodePosition);
//        if (newNode == null)
//            return null;

//        int count = newNode.neighbors.Count;
//        if (count == 0) // Lonely Node
//            return null;
//        else if (count == 1)
//        {
//            Node neighbor = newNode.neighbors[0];

//            SegmentData extendedSegment = TryExtendSegment(neighbor, newNode); // Extend Existing Segment
//            if (extendedSegment != null)
//                return extendedSegment;

//            return CreateNewSegment(neighbor, newNode); // Create New Segment
//        }

//        return null;
//    }

//    private SegmentData TryExtendSegment(Node existingNode, Node newNode)
//    {
//        // 1. Is Existing Node Part of a Segment?
//        if (!nodeToSegmentsMap.TryGetValue(existingNode, out var segmentList)) // No
//            return null;
        
//        // 2. Yes, It Is
//        SegmentData candidate = null;
//        foreach (var seg in segmentList)
//        {
//            var segNodes = seg.nodesInSegment;
//            if (segNodes.Count > 0 && segNodes[segNodes.Count - 1] == existingNode)
//            {
//                candidate = seg;
//                break;
//            }
//        }

//        if (candidate == null)
//            return null;

//        // 3. Collinearity
//        if (!IsCollinearExtension(candidate, newNode))
//            return null;

//        // 4.Extend
//        RemoveSegment(candidate);

//        SegmentData extended = new SegmentData(candidate.start, newNode.worldPosition);
//        extended.nodesInSegment.AddRange(candidate.nodesInSegment);
//        extended.nodesInSegment.Add(newNode);

//        segments.Add(extended);
//        MapSegmentToNodes(extended);

//        return extended;
//    }
//}

//[System.Serializable]
//public class SegmentData
//{
//    public Vector2 start;
//    public Vector2 end;
//    public List<Node> nodesInSegment;

//    public SegmentData(Vector2 start, Vector2 end)
//    {
//        this.start = start;
//        this.end = end;
//        nodesInSegment = new List<Node>();
//    }
//}
