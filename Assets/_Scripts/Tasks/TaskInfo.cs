﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "Task", order = 1)]
public class TaskInfo : ScriptableObject
{
    [Header("ID")]
    public int map;
    public int number;

    [Header("Basic")]
    public string title;
    public Character character;
    public string[] dialogs;
    [TextArea(3, 6)]
    public string context;

    [Header("Map")]
    public CompoundInfo from;
    public CompoundInfo to;

    [Header("Requirements")]
    public bool safetyRequirement;
    public int minSafetyCount;
    public bool charmRequirement;
    public int minCharmCount;
    public bool flowRequirement;
    [Range(0f, 1f)] public float minFlowPercentage;
    public bool minMaterialRequirement;
    public int minMaterial;
    public bool maxMaterialRequirement;
    public int maxMaterial;

    [Header("Rewards")]
    public int materialReward;
    public Vector2Int[] unlockedTasks;
    public string[] rewardDialogs;
    // Character's Dialog
    // Social Network Comments
}