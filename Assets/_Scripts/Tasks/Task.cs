using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    [Header("Basic")]
    [Range(0, 4)] public int state; // 0:Locked 1:Unlocked 2:Accepted 3:Active 4:Completed 5:Sealed
    public TaskInfo info;
    public Compound from;
    public Compound to;

    [Header("Requirements")]
    public int currentSafetyCount;
    public int currentCharmCount;
    public float currentFlowPercentage;
    public int usedMaterial;

    // :::::::::: METHODS ::::::::::
    // ::::: Requirements
    public bool MeetsRequirements()
    {
        return MeetsSafetyRequirement() &&
               MeetsCharmRequirement() &&
               MeetsFlowRequirement() &&
               MeetsMinMaterialRequirement() &&
               MeetsMaxMaterialRequirement();
    }

    public bool MeetsSafetyRequirement() { return !info.safetyRequirement || currentSafetyCount >= info.minSafetyCount; }
    public bool MeetsCharmRequirement() { return !info.charmRequirement || currentCharmCount >= info.minCharmCount; }
    public bool MeetsFlowRequirement() { return !info.flowRequirement || currentFlowPercentage >= info.minFlowPercentage; }
    public bool MeetsMinMaterialRequirement() { return !info.minMaterialRequirement || usedMaterial >= info.minMaterial; }
    public bool MeetsMaxMaterialRequirement() { return !info.maxMaterialRequirement || usedMaterial <= info.maxMaterial; }
}