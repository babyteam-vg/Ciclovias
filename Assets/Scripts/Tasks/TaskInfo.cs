using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "Task", order = 1)]
public class TaskInfo : ScriptableObject
{
    public string title;
    //public Character character;

    [TextArea(3, 6)]
    public string description;

    public List<Vector2Int> startCells;
    public List<Vector2Int> destinationCells;

    public int minSafety;
    public int minCharm;
    [Range(0f, 1f)] public float flowPercentage;

    public int materialLimit;
}