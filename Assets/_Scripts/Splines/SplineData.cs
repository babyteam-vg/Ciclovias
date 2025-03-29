using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SplineData
{
    [Serializable]
    public class SerializableKnot
    {
        public Vector3 position;
        public Vector3 tangentIn;
        public Vector3 tangentOut;
    }

    [Serializable]
    public class SerializableSpline
    {
        public bool isSealed;
        public List<SerializableKnot> knots = new List<SerializableKnot>();
    }

    [Serializable]
    public class SerializableIntersection
    {
        public SerializableSpline spline;
        public List<Vector3> edges = new List<Vector3>();
    }

    [Serializable]
    public class IntersectionData
    {
        public Vector3 position;
        public SerializableIntersection intersection;
    }

    public List<SerializableSpline> splines = new List<SerializableSpline>();
    public List<IntersectionData> intersections = new List<IntersectionData>();
}
