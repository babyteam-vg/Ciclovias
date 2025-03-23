using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "Scriptable Object/Task", order = 1)]
public class TaskInfo : ScriptableObject
{
    [Header("Basic")]
    public Vector2Int id;
    public string title;
    public Character character;
    [TextArea(2, 4)] public string[] dialogs;
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

    [Header("Rewards")]
    public int materialReward;
    public Vector2Int[] unlockedTasks;
    [TextArea(2, 4)] public string[] rewardDialogs;
    public PostInfo[] posts;
}

[System.Serializable]
public class Flavor
{
    public FlavorType flavorType;
    public CompoundInfo compound;
    public CellContent toCross;
    public int numberToCross;
    [TextArea(3, 6)] public string flavorMessage;
}

public enum FlavorType
{
    None,
    Visit,      // Visit a Compound/Park
    Avoid,      // Avoid a Compound/Park
    Cross,      // Cross an Amount of (Type of) Cells
    UseLane,    // Use a Sealed Lane
    AvoidLane,  // Don't Use a Sealed Lane
    Perfect,    // Don't Lower the Safety/Charm/Flow
}