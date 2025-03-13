using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class IntersectionData
{
    public Vector3 position;
    public int index;
}

[System.Serializable]
public class SplinesData
{
    [System.Serializable]
    public class SerializableKnot
    {
        public Vector3 position;
        public Vector3 tangentIn;
        public Vector3 tangentOut;
    }

    public List<SerializableKnot> knots = new List<SerializableKnot>();
    public List<IntersectionData> intersections = new List<IntersectionData>();
}

[System.Serializable]
public class SplineContainerData
{
    public List<SplinesData> splines = new List<SplinesData>();
}
