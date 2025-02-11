using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    [Header("Basic")]
    [Range(0, 4)] public int state; // 0:Locked 1:Unlocked 2:Accepted 3:Active 4:Completed
    public TaskInfo info;
    public Compound fromCompound;
    public Compound toCompound;
    public List<Vector2Int> completedLane;

    [Header("Requirements")]
    public int currentSafetyCount;
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
        bool meetingRequirements = true;

        if (info.safetyRequirement && currentSafetyCount < info.minSafetyCount) return false;
        if (info.charmRequirement && currentCharmCount < info.minCharmCount) return false;
        if (info.flowRequirement && currentFlowPercentage < info.minFlowPercentage) return false;
        if (info.minMaterialRequirement && usedMaterial < info.minMaterial) return false;
        if (info.maxMaterialRequirement && usedMaterial > info.maxMaterial) return false;

        return meetingRequirements;
    }
}