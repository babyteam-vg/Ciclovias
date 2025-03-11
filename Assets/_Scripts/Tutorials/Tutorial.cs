using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tutorial
{
    [Header("Basic")]
    public bool completed;
    public TutorialInfo info;

    [Header("Requirements")]
    public int currentSafety;
    public float currentFlow;
    public int currentCharm;
    public int usedMaterial;

    // :::::::::: METHODS ::::::::::
    // ::::: Requirements
    public bool MeetsRequirements(int currentSafety, int currentCharm, float currentFlow, int usedMaterial)
    {
        return MeetsSafetyRequirement(currentSafety) &&
               MeetsCharmRequirement(currentCharm) &&
               MeetsFlowRequirement(currentFlow) &&
               MeetsMinMaterialRequirement(usedMaterial) &&
               MeetsMaxMaterialRequirement(usedMaterial);
    }

    public bool MeetsSafetyRequirement(int currentSafety) { return !info.safetyRequirement || currentSafety >= info.minSafetyCount; }
    public bool MeetsCharmRequirement(int currentCharm) { return !info.charmRequirement || currentCharm >= info.minCharmCount; }
    public bool MeetsFlowRequirement(float currentFlow) { return !info.flowRequirement || currentFlow >= info.minFlowPercentage; }
    public bool MeetsMinMaterialRequirement(int usedMaterial) { return !info.minMaterialRequirement || usedMaterial >= info.minMaterial; }
    public bool MeetsMaxMaterialRequirement(int usedMaterial) { return !info.maxMaterialRequirement || usedMaterial <= info.maxMaterial; }

    // :::::::::: STORAGE ::::::::::
    // ::::: Tutorial -> TutorialData
    public TutorialData SaveTutorial()
    {
        return new TutorialData
        {
            completed = this.completed,
        };
    }
}
