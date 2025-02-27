using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    [Header("Basic")]
    public TaskState state;
    public TaskInfo info;
    public Compound from;
    public Compound to;

    [Header("Path")]
    public Vector2Int start;
    public Vector2Int end;

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

public enum TaskState
{
    Locked,
    Unlocked,
    Accepted,
    Active,
    Completed,
    Sealed
}