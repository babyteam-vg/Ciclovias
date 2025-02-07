using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Compound", menuName = "Compound", order = 1)]
public class CompoundInfo : ScriptableObject
{
    [Header("Basic")]
    public string compoundName;
    public List<Vector2Int> surroundings;
}
