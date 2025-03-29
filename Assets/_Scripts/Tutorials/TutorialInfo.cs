using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Tutorial", menuName = "Scriptable Object/Tutorial")]
public class TutorialInfo : ScriptableObject
{
    [Header("Basic")]
    public Vector2Int id;
    public string title;
    public Character character;
    public TutorialSection[] sections;

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
}

[System.Serializable]
public struct TutorialSection
{
    public SectionType type;
    [TextArea(2, 4)] public List<string> dialogs;
    public bool checkRequirements;
    public Vector2Int start;
    public Vector2Int end;
    public bool dontAddToPath; // If True, the Path Won't Automatically Seal When the Section is Completed
    public MapData tutorialMap;
    public Keyframe keyframe;
}

public enum SectionType
{
    Build,
    Destroy,
    Close,
}

[System.Serializable]
public struct Keyframe
{
    public Vector3 position;
    public Quaternion rotation;
    public float zoom;
    public float duration;
}
