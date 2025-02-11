using System.Collections;
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
    [TextArea(3, 6)]
    public string dialog;
    [TextArea(3, 6)]
    public string endDialog;

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
    public List<Vector2Int> unlockedTasks;
    // Character's Dialog
    // Social Network Comments
}