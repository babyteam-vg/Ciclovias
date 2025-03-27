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

            case FlavorType.Cross:
                switch (info.flavorDetails.toCross)
                {
                    case CellContent.Attraction:
                        return $"Go through {info.flavorDetails.numberToCross} attractions";
                    case CellContent.Crossing:
                        return $"Go through {info.flavorDetails.numberToCross} intersections with traffic lights";
                    case CellContent.Dangerous:
                        return $"Go through {info.flavorDetails.numberToCross} dangerous areas";
                    case CellContent.Green:
                        return $"Go through {info.flavorDetails.numberToCross} green areas";
                    case CellContent.Repulsive:
                        return $"Go through {info.flavorDetails.numberToCross} repulsive areas";
                    case CellContent.Road:
                        return $"Cross the street {info.flavorDetails.numberToCross} time(s)";
                    case CellContent.Sidewalk:
                        return $"Go through {info.flavorDetails.numberToCross} sidewalks";
                    case CellContent.Traffic:
                        return $"Go through {info.flavorDetails.numberToCross} entrances";
                    case CellContent.Zebra:
                        return $"Go through {info.flavorDetails.numberToCross} zebra crosses";
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