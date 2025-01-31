using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    [Header("Bools")]
    [Range(0, 3)]
    public int state;
    public bool pinned; // UI Only

    [Header("Info")]
    public TaskInfo info;

    private float currentSafety;
    private float currentCharm;
    private float currentFlow;
    public List<Vector2Int> completedLane;

    // === Methods ===
    // State
    public void SetState(int value) { state = value; }
    public int GetState() { return state; }

    // Safety
    public void SetSafety(float value) { currentSafety = value; }
    public float GetSafety() { return currentSafety; }

    // Charm
    public void SetCharm(float value) { currentCharm = value; }
    public float GetCharm() { return currentCharm; }

    // Flow
    public void SetFlow(float value) { currentFlow = value; }
    public float GetFlow() { return currentFlow; }

    // Lane
    public void SetCompletedLane(List<Vector2Int> value) { completedLane = value; }
    public List<Vector2Int> GetCompletedLane() { return completedLane; }

    // Requirements
    public bool MeetsRequirements()
    {
        return currentSafety >= info.minSafety &&
            currentCharm >= info.minCharm &&
            currentFlow >= info.flowPercentage;
    }
}