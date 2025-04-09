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

    [Header("Scores")]
    public float currentSafety;
    public float currentCharm;
    public float currentFlow;
    public int usedMaterial;

    [Header("Flavor")]
    public bool flavorMet;

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

    public string GenerateFlavorMessage()
    {
        switch (info.flavorDetails.flavorType)
        {
            case FlavorType.None:
                break;

            case FlavorType.Avoid:
                return $"Avoid the {info.flavorDetails.compound.compoundName}";

            case FlavorType.Visit:
                return $"Visit the {info.flavorDetails.compound.compoundName}";

            case FlavorType.DontCross:
                switch (info.flavorDetails.dontCross)
                {
                    case CellContent.Attraction:
                        return $"Avoid attractions";
                    case CellContent.Crossing:
                        return $"Avoid traffic lights";
                    case CellContent.Dangerous:
                        return $"Avoid dangerous areas";
                    case CellContent.Green:
                        return $"Avoid green areas";
                    case CellContent.Repulsive:
                        return $"Avoid repulsive areas";
                    case CellContent.Road:
                        return $"Avoid the streets";
                    case CellContent.Sidewalk:
                        return $"Avoid sidewalks";
                    case CellContent.Traffic:
                        return $"Avoid compound entrances and unmarked crossings";
                    case CellContent.Zebra:
                        return $"Avoid zebra crosses";
                }
                break;

            case FlavorType.UseLane:
                return $"Use a sealed lane";

            case FlavorType.AvoidLane:
                return $"Avoid sealed lanes";

            case FlavorType.Perfect:
                return $"Kepp a perfect score";
        }

        return "";
    }

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