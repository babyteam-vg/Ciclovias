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
        bool meetingRequirements = true;

        if (info.safetyRequirement && currentSafetyCount < info.minSafetyCount) return false;
        if (info.charmRequirement && currentCharmCount < info.minCharmCount) return false;
        if (info.flowRequirement && currentFlowPercentage < info.minFlowPercentage) return false;
        if (info.minMaterialRequirement && usedMaterial < info.minMaterial) return false;
        if (info.maxMaterialRequirement && usedMaterial > info.maxMaterial) return false;

        return meetingRequirements;
    }
}