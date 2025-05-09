﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "Scriptable Object/Task", order = 1)]
public class TaskInfo : ScriptableObject
{
    [Header("Basic")]
    public Vector2Int id;
    public string title;
    public Character character;
    [TextArea(2, 4)] public List<string> dialogs;
    [TextArea(3, 6)] public string context;

    [Header("Requirements")]
    public bool safetyRequirement;
    [Range(0f, 1f)] public float minSafety;
    public bool charmRequirement;
    [Range(0f, 1f)] public float minCharm;
    public bool flowRequirement;
    [Range(0f, 1f)] public float minFlow;
    public bool minMaterialRequirement;
    public int minMaterial;
    public bool maxMaterialRequirement;
    public int maxMaterial;

    [Header("Flavor")]
    public Flavor flavorDetails;
    [TextArea(2, 4)] public string extraDialog;
    public int extraDialogIndex;

    [Header("Rewards")]
    public int materialReward;
    public Vector2Int[] unlockedTasks;
    [TextArea(2, 4)] public List<string> rewardDialogs;
    public PostInfo[] posts;
}

[System.Serializable]
public class Flavor
{
    public FlavorType flavorType;
    public CompoundInfo compound;
    public CellContent dontCross;
}

public enum FlavorType
{
    None,
    Visit,      // Visit a Compound/Park
    Avoid,      // Avoid a Compound/Park
    DontCross,  // Don't Cross a Type of Cell
    UseLane,    // Use a Sealed Lane
    AvoidLane,  // Don't Use a Sealed Lane
    Perfect,    // Don't Lower the Safety/Charm/Flow
}