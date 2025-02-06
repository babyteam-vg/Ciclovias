//using UnityEngine;
//using UnityEngine.Splines;
//using System.Collections.Generic;

//public class SplineManager : MonoBehaviour
//{
//    [SerializeField] private GraphSegmentator segmentator;
//    [SerializeField] private LaneConstructor laneConstructor;
//    [SerializeField] private LaneDestructor laneDestructor;
//    [SerializeField] private SplineContainer splineContainer;

//    private Dictionary<SegmentData, Spline> segmentToSpline = new Dictionary<SegmentData, Spline>();

//    // === Event Subsription ===
//    private void OnEnable()
//    {
//        laneConstructor.OnLaneBuilt += HandleLaneBuilt;
//        //laneDestructor.OnLaneDestroyed += HandleLaneDestroyed;
//    }
//    private void OnDisable()
//    {
//        laneConstructor.OnLaneBuilt -= HandleLaneBuilt;
//        //laneDestructor.OnLaneDestroyed -= HandleLaneDestroyed;
//    }

//    // === Event Handlers ===
//    // When a Lane is Built
//    private void HandleLaneBuilt(Vector2Int nodePos)
//    {
//        List<SegmentData> affectedSegments = segmentator.GetSegmentsByNodePosition(nodePos); // Affected Segments Only
//        foreach (SegmentData segment in affectedSegments)
//            CreateOrUpdateSpline(segment);
//    }

//    /// <summary>
//    /// Llamado cuando se destruye un nodo. 
//    /// Pedimos a GraphSegmentator el/los segmentos asociados a ese nodo
//    /// (si todavía existen, o si sabemos cuáles se han roto).
//    /// Luego eliminamos la spline de ese segmento.
//    /// </summary>
//    //private void HandleLaneDestroyed(Vector2Int nodePos)
//    //{
//    //    // 1. Obtener segmentos afectados
//    //    List<SegmentData> affectedSegments = segmentator.GetSegmentsByNodePosition(nodePos);

//    //    // 2. Eliminar spline para cada uno
//    //    foreach (SegmentData segment in affectedSegments)
//    //    {
//    //        RemoveSpline(segment);
//    //    }
//    //}

//    // === Update Splines ===
//    // Create or Update a Spline
//    private void CreateOrUpdateSpline(SegmentData segment)
//    {
//        // Does the Segment Have a Spline?
//        if (segmentToSpline.TryGetValue(segment, out Spline existingSpline)) // Yes
//            UpdateSplineKnots(existingSpline, segment);
//        else // No
//        {
//            Spline newSpline = new Spline();
//            UpdateSplineKnots(newSpline, segment);
//            splineContainer.AddSpline(newSpline);
//            segmentToSpline[segment] = newSpline;
//        }
//    }

//    /// <summary>
//    /// Quita la spline asociada a un segmento, si existe,
//    /// del SplineContainer y del diccionario.
//    /// </summary>
//    //private void RemoveSpline(SegmentData segment)
//    //{
//    //    if (segmentToSpline.TryGetValue(segment, out Spline spline))
//    //    {
//    //        // Retiramos la spline del container
//    //        splineContainer.RemoveSpline(spline);

//    //        // Quitamos la referencia del diccionario
//    //        segmentToSpline.Remove(segment);
//    //    }
//    //}

//    // === Knots Management ===
//    // Update Knots for a Spline
//    private void UpdateSplineKnots(Spline spline, SegmentData segment)
//    {
//        while (spline.Count < 2)
//            spline.Add(new BezierKnot());
//        while (spline.Count > 2)
//            spline.RemoveAt(spline.Count - 1);

//        Vector3 localStart = WorldToLocalPoint(segment.start);
//        Vector3 localEnd = WorldToLocalPoint(segment.end);

//        // Sart Knot
//        BezierKnot knotStart = spline[0];
//        knotStart.Position = localStart;
//        knotStart.TangentIn = Vector3.zero;
//        knotStart.TangentOut = Vector3.zero;
//        knotStart.Rotation = Quaternion.identity;
//        spline[0] = knotStart;

//        // End Knot
//        BezierKnot knotEnd = spline[1];
//        knotEnd.Position = localEnd;
//        knotEnd.TangentIn = Vector3.zero;
//        knotEnd.TangentOut = Vector3.zero;
//        knotEnd.Rotation = Quaternion.identity;
//        spline[1] = knotEnd;

//        spline.Closed = false;
//    }

//    // (x, y) -> (x, 0, y)
//    private Vector3 WorldToLocalPoint(Vector2 pos2D)
//    {
//        Vector3 worldPos3D = new Vector3(pos2D.x, 0f, pos2D.y);
//        return splineContainer.transform.InverseTransformPoint(worldPos3D);
//    }
//}
