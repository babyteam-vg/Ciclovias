using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Object/Character", order = 1)]
public class Character : ScriptableObject
{
    public string characterName;
    public Sprite portrait;
}
