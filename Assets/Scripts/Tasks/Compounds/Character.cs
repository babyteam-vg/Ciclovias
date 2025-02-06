using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Character", order = 1)]
public class Character : ScriptableObject
{
    public string name;
    public Sprite portrait;
}
