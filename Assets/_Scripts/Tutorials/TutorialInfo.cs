using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Tutorial", menuName = "Scriptable Object/Tutorial")]
public class TutorialInfo : ScriptableObject
{
    [Header("Basic")]
    public Vector2Int id;
    public string title;
    public TutorialSection[] sections;

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
}

[System.Serializable]
public struct TutorialSection
{
    [TextArea(3, 6)] public string text;
    public bool destroyRequirement;
    public Vector2Int start;
    public Vector2Int end;
    public bool checkRequirements;
    public MapData tutorialMap;
    public Keyframe[] keyframes;
}

[System.Serializable]
public struct Keyframe
{
    public Vector3 position;
    public Quaternion rotation;
    public float zoom;
    public float duration;
}
