using UnityEngine;

[System.Serializable]
public class TaskData
{
    public TaskState state;

    public Vector2Int start;
    public Vector2Int end;

    public int currentSafetyCount;
    public int currentCharmCount;
    public float currentFlowPercentage;
    public int usedMaterial;

    public int currentToCross;
}