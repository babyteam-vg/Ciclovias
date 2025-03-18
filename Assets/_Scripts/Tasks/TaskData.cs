using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskData
{
    public TaskState state;

    public List<Vector2Int> path;

    public float currentSafety;
    public float currentCharm;
    public float currentFlow;
    public int usedMaterial;

    public bool flavorMet;
    public int currentToCross;
}