using System.Collections.Generic;
using Unity.VisualScripting;
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

    [Header("Flavor")]
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

    public bool MeetsSafetyRequirement() { return !info.safetyRequirement || currentSafetyCount >= info.minSafetyCount; }
    public bool MeetsCharmRequirement() { return !info.charmRequirement || currentCharmCount >= info.minCharmCount; }
    public bool MeetsFlowRequirement() { return !info.flowRequirement || currentFlowPercentage >= info.minFlowPercentage; }
    public bool MeetsMinMaterialRequirement() { return !info.minMaterialRequirement || usedMaterial >= info.minMaterial; }
    public bool MeetsMaxMaterialRequirement() { return !info.maxMaterialRequirement || usedMaterial <= info.maxMaterial; }

    // :::::::::: STORAGE ::::::::::
    // ::::: Task -> TaskData
    public TaskData SaveTask()
    {
        return new TaskData
        {
            state = this.state,

            start = this.start,
            end = this.end,

            currentSafetyCount = this.currentSafetyCount,
            currentCharmCount = this.currentCharmCount,
            currentFlowPercentage = this.currentFlowPercentage,
            usedMaterial = this.usedMaterial,

            currentToCross = this.currentToCross
        };
    }

    // ::::: TaskData -> Task
    public void LoadTask(TaskData serializableTask, TaskInfo info, Compound from, Compound to)
    {
        this.state = serializableTask.state;

        this.start = serializableTask.start;
        this.end = serializableTask.end;

        this.currentSafetyCount = serializableTask.currentSafetyCount;
        this.currentCharmCount = serializableTask.currentCharmCount;
        this.currentFlowPercentage = serializableTask.currentFlowPercentage;
        this.usedMaterial = serializableTask.usedMaterial;

        this.currentToCross = serializableTask.currentToCross;

        this.info = info;
        this.from = from;
        this.to = to;
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