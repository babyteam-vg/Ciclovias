using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererUtility
{
    // ::::: Find the Highest Surface
    public float GetMaxElevationAtPoint(Vector3 point, GameObject plane)
    {
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(point.x, point.y + 100f, point.z), Vector3.down);
        Physics.Raycast(ray, out hit);
        return hit.point.y + 0.0002f;
    }
}
