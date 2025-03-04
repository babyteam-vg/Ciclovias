using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "Scriptable Object/Task", order = 1)]
public class TaskInfo : ScriptableObject
{
    [Header("Basic")]
    public Vector2Int id;
    public string title;
    public Character character;
    public string[] dialogs;
    [TextArea(3, 6)] public string context;

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
    public PostInfo[] posts;
}