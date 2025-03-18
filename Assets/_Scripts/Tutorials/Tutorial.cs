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
    public float currentSafety;
    public float currentFlow;
    public float currentCharm;
    public int usedMaterial;

    // :::::::::: METHODS ::::::::::
    // ::::: Requirements
    public bool MeetsRequirements(float currentSafety, float currentCharm, float currentFlow, int usedMaterial)
    {
        return MeetsSafetyRequirement(currentSafety) &&
               MeetsCharmRequirement(currentCharm) &&
               MeetsFlowRequirement(currentFlow) &&
               MeetsMinMaterialRequirement(usedMaterial) &&
               MeetsMaxMaterialRequirement(usedMaterial);
    }

    public bool MeetsSafetyRequirement(float currentSafety) { return !info.safetyRequirement || currentSafety >= info.minSafety; }
    public bool MeetsCharmRequirement(float currentCharm) { return !info.charmRequirement || currentCharm >= info.minCharm; }
    public bool MeetsFlowRequirement(float currentFlow) { return !info.flowRequirement || currentFlow >= info.minFlow; }
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
