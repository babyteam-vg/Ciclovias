using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskData
{
    public TaskState state;

    public List<Vector2Int> path;
    //public Vector2Int start;
    //public Vector2Int end;

    public int currentSafetyCount;
    public int currentCharmCount;
    public float currentFlowPercentage;
    public int usedMaterial;

    public bool flavorMet;
    public int currentToCross;
}