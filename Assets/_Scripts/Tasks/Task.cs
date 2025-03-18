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
    public List<Vector2Int> path;

    [Header("Requirements")]
    public float currentSafety;
    public float currentCharm;
    public float currentFlow;
    public int usedMaterial;

    [Header("Flavor")]
    public bool flavorMet;
    public int currentToCross;

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

    public bool MeetsSafetyRequirement() { return !info.safetyRequirement || currentSafety >= info.minSafety; }
    public bool MeetsCharmRequirement() { return !info.charmRequirement || currentCharm >= info.minCharm; }
    public bool MeetsFlowRequirement() { return !info.flowRequirement || currentFlow >= info.minFlow; }
    public bool MeetsMinMaterialRequirement() { return !info.minMaterialRequirement || usedMaterial >= info.minMaterial; }
    public bool MeetsMaxMaterialRequirement() { return !info.maxMaterialRequirement || usedMaterial <= info.maxMaterial; }

    // :::::::::: STORAGE ::::::::::
    // ::::: Task -> TaskData
    public TaskData SaveTask()
    {
        return new TaskData
        {
            state = this.state,

            path = this.path,

            currentSafety = this.currentSafety,
            currentCharm = this.currentCharm,
            currentFlow = this.currentFlow,
            usedMaterial = this.usedMaterial,

            flavorMet = this.flavorMet,
            currentToCross = this.currentToCross
        };
    }
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