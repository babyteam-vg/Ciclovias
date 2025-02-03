using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    [Header("Bools")]
    [Range(0, 3)]
    public int state;

    [Header("Info")]
    public TaskInfo info;

    public int currentSafety;
    public int currentCharm;
    public float currentFlow;
    public List<Vector2Int> completedLane;

    // === Methods ===
    // State
    public void SetState(int value) { state = value; }
    public int GetState() { return state; }

    // Safety
    public void SetSafety(int value) { currentSafety = value; }
    public int GetSafety() { return currentSafety; }

    // Charm
    public void SetCharm(int value) { currentCharm = value; }
    public int GetCharm() { return currentCharm; }

    // Flow
    public void SetFlow(float value) { currentFlow = value; }
    public float GetFlow() { return currentFlow; }

    // Lane
    public void SetCompletedLane(List<Vector2Int> value) { completedLane = value; }
    public List<Vector2Int> GetCompletedLane() { return completedLane; }

    // Requirements
    public bool MeetsRequirements()
    {
        return currentSafety >= info.maxSafetyDiscount &&
            currentCharm >= info.minCharm &&
            currentFlow >= info.minFlowPercentage;
    }
}