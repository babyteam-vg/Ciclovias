using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tip", menuName = "Scriptable Object/Tip")]
public class TipInfo : ScriptableObject
{
    public int id;
    [TextArea(3, 6)] public string tipText;
}
