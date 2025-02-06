using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    [Header("Basic")]
    [Range(0, 4)] public int state; // 0:Locked 1:Unlocked 2:Accepted 3:Active 4:Completed
    public TaskInfo info;
    public List<Vector2Int> completedLane;

    [Header("Requirements")]
    public int currentSafetyDiscount;
    public int currentCharmCount;
    public float currentFlowPercentage;
    public int usedMaterial;

    // :::::::::: METHODS ::::::::::
    // ::::: Lane
    public void SetCompletedLane(List<Vector2Int> value) { completedLane = value; }
    public List<Vector2Int> GetCompletedLane() { return completedLane; }

    // ::::: Requirements
    public bool MeetsRequirements()
    {
        return currentSafetyDiscount >= info.maxSafetyDiscount &&
            currentCharmCount >= info.minCharmCount &&
            currentFlowPercentage >= info.minFlowPercentage &&
            usedMaterial >= info.minMaterial && usedMaterial <= info.maxMaterial;
    }
}