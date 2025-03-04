using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Post", menuName = "Scriptable Object/Post", order = 4)]
public class PostInfo : ScriptableObject
{
    public bool isRepeatable;
    [TextArea(3, 6)] public string text;
    public Sprite illustration;
}
