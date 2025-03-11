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
    public List<Vector2Int> path;
    //public Vector2Int start;
    //public Vector2Int end;

    [Header("Requirements")]
    public int currentSafetyCount;
    public int currentCharmCount;
    public float currentFlowPercentage;
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

            path = this.path,
            //start = this.start,
            //end = this.end,

            currentSafetyCount = this.currentSafetyCount,
            currentCharmCount = this.currentCharmCount,
            currentFlowPercentage = this.currentFlowPercentage,
            usedMaterial = this.usedMaterial,

            flavorMet = this.flavorMet,
            currentToCross = this.currentToCross
        };
    }

    // ::::: TaskData -> Task
    public void LoadTask(TaskData taskData, TaskInfo info, Compound from, Compound to)
    {
        this.state = taskData.state;

        this.path = taskData.path;
        //this.start = taskData.start;
        //this.end = taskData.end;

        this.currentSafetyCount = taskData.currentSafetyCount;
        this.currentCharmCount = taskData.currentCharmCount;
        this.currentFlowPercentage = taskData.currentFlowPercentage;
        this.usedMaterial = taskData.usedMaterial;

        this.flavorMet = taskData.flavorMet;
        this.currentToCross = taskData.currentToCross;

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